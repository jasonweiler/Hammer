using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Hammer.Attributes;

namespace Hammer
{
    class CommandGroupInfo
    {
        public Type GroupType {get; internal set;}
        public HammerCommandGroupAttribute Attribute {get; internal set;}

        public string EffectiveName { get; internal set; }
    }

    class CommandInfo
    {
        public MethodInfo MethodType {get; internal set;}
        public HammerCommandAttribute Attribute {get; internal set;}
        public string EffectiveName { get; internal set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var commandArgs = HammerCommandLineParser.Parse(args);

            if (commandArgs.IsEnumeratedHelpCommand())
            {
                // list all commands in one or all hives
                OutputAllCommandGroupHelp(commandArgs.GroupName);
            }
            else
            {
                ExecuteHammerCommand(commandArgs);
            }
        }

        private static void ExecuteHammerCommand(HammerCommand cmdArgs)
        {
            try
            {
                var cmdGroupInfo = FindCommandGroup(cmdArgs.GroupName);
                if (cmdGroupInfo != null)
                {
                    var cmdInfo = FindCommand(cmdGroupInfo.GroupType, cmdArgs.Name);
                    if (ValidateCommand(cmdInfo, cmdArgs))
                    {
                        CallCommand(cmdInfo, cmdArgs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool ValidateCommand(CommandInfo cmdInfo, HammerCommand cmdArgs)
        {
            var result = true;
            // make sure we have all the required args
            var parameters = cmdInfo.MethodType.GetParameters();

            foreach(var parameter in parameters)
            {
                var paramAttribute = parameter.GetCustomAttribute<HammerParameterAttribute>();
                string name = GetParameterName(parameter, paramAttribute);

                // see if this parameter has a command line arg
                
                var param = cmdArgs.FindParameter(name);
                if (param == null && !parameter.IsOptional)
                {
                    // not specified, and not optional, so this is an error

                }

            }
            
            return result;
        }

        private static void CallCommand(CommandInfo cmdInfo, HammerCommand cmdArgs)
        {
            throw new NotImplementedException();
        }


        static void OutputAllCommandGroupHelp(string cmdGroupFilterName)
        {
            var targetGroups = new List<CommandGroupInfo>();

            if (cmdGroupFilterName == null)
            {
                targetGroups.AddRange(FindAllCommandGroups());
            }
            else
            {
                targetGroups.Add(FindCommandGroup(cmdGroupFilterName));
            }

            foreach (var groupInfo in targetGroups.OrderBy(info => GetCommandGroupName(info.GroupType, info.Attribute)))
            {
                OutputCommandGroupHelp(groupInfo);
            }
        }

        private static CommandGroupInfo FindCommandGroup(string cmdGroupName)
        {
            return FindAllCommandGroups()
                .FirstOrDefault(cg => GetCommandGroupName(cg.GroupType, cg.Attribute) == cmdGroupName);
        }

        private static IEnumerable<CommandGroupInfo> FindAllCommandGroups()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in allAssemblies)
            {
                var allAssemblyTypes = assembly.GetTypes();

                foreach(var groupType in allAssemblyTypes)
                {
                    var groupAttribute = groupType.GetCustomAttribute<HammerCommandGroupAttribute>();
                    if (groupAttribute != null)
                    {
                        yield return new CommandGroupInfo
                        {
                            GroupType = groupType,
                            Attribute = groupAttribute,
                            EffectiveName = GetCommandGroupName(groupType, groupAttribute),
                        };
                    }
                }
            }
        }

        private static void OutputCommandGroupHelp(CommandGroupInfo groupInfo)
        {
            // Output all entry points in this group
            var groupDescriptionText = groupInfo.Attribute.Description ?? "";

            if (groupDescriptionText.Any())
            {
                groupDescriptionText = $" - {groupDescriptionText}";
            }


            Console.Out.WriteLine($"{groupInfo.EffectiveName}{groupDescriptionText}");
            var groupCommandInfos = EnumerateGroupCommands(groupInfo.GroupType);

            foreach (var commandInfo in groupCommandInfos)
            {
                var descriptionText = commandInfo.Attribute.Description ?? "";

                if (descriptionText.Any())
                {
                    descriptionText = $" - {descriptionText}";
                }

                Console.Out.WriteLine($"\t{groupInfo.EffectiveName}.{commandInfo.EffectiveName}{descriptionText}");
            }

            Console.Out.WriteLine();
        }

        private static IEnumerable<CommandInfo> EnumerateGroupCommands(Type groupType)
        {
            var allStaticMethods = groupType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var methodInfo in allStaticMethods)
            {
                var cmdAttribute = methodInfo.GetCustomAttribute<HammerCommandAttribute>();
                if (cmdAttribute != null)
                {
                    
                    yield return new CommandInfo
                    {
                        MethodType = methodInfo,
                        Attribute = cmdAttribute,
                        EffectiveName = GetCommandName(methodInfo, cmdAttribute),
                    };
                }
            }
        }

        private static CommandInfo FindCommand(Type groupType, string cmdName)
        {
            // find command by name within the groupType. Don't worry about
            //  matching arguments just yet
            var allGroupCommands = EnumerateGroupCommands(groupType);

            return allGroupCommands.FirstOrDefault(cmd => cmd.EffectiveName == cmdName);
        }
        
        private static string GetCommandGroupName(Type commandGroupType, HammerCommandGroupAttribute groupAttribute)
        {
            var result = groupAttribute?.AltName;
            if (result == null)
            {
                result = commandGroupType.Name;
                if (result.EndsWith("Commands"))
                {
                    result = result.Substring(0, result.Length - "Commands".Length);
                }
            }
                
            return result;
        }

        private static string GetCommandName(MethodInfo methodInfo, HammerCommandAttribute attribute)
        {
            return attribute?.AltName ?? methodInfo.Name;
        }

        private static string GetParameterName(ParameterInfo parameter, HammerParameterAttribute attribute)
        {
            return attribute?.AltName ?? parameter.Name;
        }
    }

    public class HammerArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class HammerCommand
    {
        public string GroupName { get; set; }
        public string Name {get; set; }

        // Arguments that are for the framework go here (eg. -help)
        public IList<HammerArgument> HammerParameters { get; } = new List<HammerArgument>();

        // all command-specific args go here
        public IList<HammerArgument> CommandParameters { get; } = new List<HammerArgument>();

        // non-switch arguments go here
        public IList<string> TargetParameters { get; } = new List<string>();

        public HammerArgument FindParameter(string parameterName)
        {
            return CommandParameters.FirstOrDefault(param => param.Name == parameterName);
        }
    }

    public class HammerCommandLineParser
    {
        public static HammerCommand Parse(string[] args)
        {
            // the first arg is always the hive/command pair
            // subsequent args are always -flag, /flag, -switch=value, or /switch=value
            // args that don't start with - or / are assumed to be unadorned batch values - file names, for example

            var result = new HammerCommand();

            if (args.Length > 0)
            {
                var hiveCmd = args[0];

                var cmdParts = hiveCmd.Split(new [] {'.' }, 2);

                result.GroupName = cmdParts[0];
                if (cmdParts.Length == 2)
                {
                    result.Name = cmdParts[1];
                }

                for (int i = 1; i < args.Length; ++i)
                {
                    if (args[i].StartsWith("-") || args[i].StartsWith("/"))
                    {
                        // is switch
                        var switchArg = ParseSwitch(args[i]);
                        if (switchArg.IsHammerSwitch())
                        {
                            result.HammerParameters.Add(switchArg);
                        }
                        else
                        {
                            result.CommandParameters.Add(switchArg);
                        }
                    }
                    else
                    {
                        result.TargetParameters.Add(args[i]);
                    }
                }
            }

            return result;
        }

        public static HammerArgument ParseSwitch(string argText)
        {
            var result = new HammerArgument();

            // 1. chop off the switch char
            argText = argText.Substring(1);

            // 2. split on any equals
            var argPair = argText.Split(new []{'='}, 2, StringSplitOptions.None);

            result.Name = argPair[0].Trim();
            if (argPair.Length == 2)
            {
                result.Value = argPair[1].Trim();
            }

            return result;
        }
    }

    public static class HammerArgExtensions
    {
        public static bool IsHammerSwitch(this HammerArgument @this)
        {
            switch (@this.Name)
            {
                case "help":
                case "?":
                    return true;
            }

            return false;
        }

        public static bool IsEnumeratedHelpCommand(this HammerCommand @this)
        {
            return @this.GroupName == null || @this.Name == null;
        }
    }
}
