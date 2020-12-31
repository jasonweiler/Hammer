using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hammer.Attributes;

namespace Hammer
{
    public class CommandGroupInfo
    {
        public Type Metadata { get; internal set; }
        public CommandGroupAttribute Attribute { get; internal set; }

        //public string EffectiveName { get; internal set; }
    }

    public class CommandInfo
    {
        public MethodInfo Metadata { get; internal set; }
        public CommandAttribute CmdAttribute { get; internal set; }
        // public string EffectiveName { get; internal set; }

        public IEnumerable<CommandParameterInfo> Parameters { get; internal set; }
    }

    public class CommandParameterInfo
    {
        public ParameterInfo Metadata { get; internal set; }
        public ParameterAttribute ParamAttribute { get; internal set; }
        // public string EffectiveName { get; internal set; }
    }

    public class CommandSupport
    {
        public static CommandGroupInfo FindCommandGroup(string cmdGroupName)
        {
            return FindAllCommandGroups()
                .FirstOrDefault(cg => cg.GetEffectiveName() == cmdGroupName);
        }

        public static IEnumerable<CommandGroupInfo> FindAllCommandGroups()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                var allAssemblyTypes = assembly.GetTypes();

                foreach (var groupType in allAssemblyTypes)
                {
                    var groupAttribute = groupType.GetCustomAttribute<CommandGroupAttribute>();
                    if (groupAttribute != null)
                    {
                        yield return new CommandGroupInfo
                        {
                            Metadata = groupType,
                            Attribute = groupAttribute,
                        };
                    }
                }
            }
        }
    }

    public static class CommandGroupExtensions
    {
        static int _commandsLength = "Commands".Length;

        public static string GetEffectiveName(this CommandGroupInfo @this)
        {

            var result = @this.Attribute?.AltName;
            if (result == null)
            {
                result = @this.Metadata.Name;
                if (result.EndsWith("Commands"))
                {
                    result = result.Substring(0, result.Length - _commandsLength);
                }
            }

            return result;
        }

        public static CommandInfo FindCommand(this CommandGroupInfo @this, string cmdName)
        {
            return @this.Metadata.FindCommand(cmdName);
        }

        public static CommandInfo FindCommand(this Type @this, string cmdName)
        {
            // find command by name within the groupType. Don't worry about
            //  matching arguments just yet
            var allGroupCommands = @this.EnumerateGroupCommands();

            return allGroupCommands.FirstOrDefault(cmd => cmd.GetEffectiveName() == cmdName);
        }

        public static IEnumerable<CommandInfo> EnumerateGroupCommands(this CommandGroupInfo @this)
        {
            return @this.Metadata.EnumerateGroupCommands();
        }
        public static IEnumerable<CommandInfo> EnumerateGroupCommands(this Type @this)
        {
            var allStaticMethods = @this.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var methodInfo in allStaticMethods)
            {
                var cmdAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();
                if (cmdAttribute != null)
                {
                    yield return new CommandInfo
                    {
                        Metadata = methodInfo,
                        CmdAttribute = cmdAttribute,
                        Parameters = methodInfo.EnumerateCommandParameters().ToList(),
                    };
                }
            }
        }
    }

    public static class CommandExtensions
    {
        public static string GetEffectiveName(this CommandInfo @this)
        {
            return @this.CmdAttribute?.AltName ?? @this.Metadata.Name;
        }

        public static IEnumerable<CommandParameterInfo> EnumerateCommandParameters(this CommandInfo @this)
        {
            return @this.Metadata.EnumerateCommandParameters();
        }

        public static IEnumerable<CommandParameterInfo> EnumerateCommandParameters(this MethodInfo @this)
        {
            var allParameters = @this.GetParameters();

            foreach (var paramInfo in allParameters)
            {
                var paramAttribute = paramInfo.GetCustomAttribute<ParameterAttribute>();

                yield return new CommandParameterInfo
                {
                    Metadata = paramInfo,
                    ParamAttribute = paramAttribute,
                };
            }
        }

        public static bool GetIsOptional(this CommandParameterInfo @this)
        {
            return @this.Metadata.IsOptional || @this.ParamAttribute.Optional;
        }
    }

    public static class CommandParameterExtensions
    {
        public static string GetEffectiveName(this CommandParameterInfo @this)
        {
            return @this.ParamAttribute?.AltName ?? @this.Metadata.Name;
        }

        public static object GetDefaultValueForType(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static object GetParameterDefaultValue(this CommandParameterInfo @this)
        {
            if (@this.ParamAttribute?.Optional ?? false)
            {
                return @this.ParamAttribute.Default;
            }

            if (@this.Metadata.IsOptional)
            {
                return @this.Metadata.DefaultValue;
            }

            return GetDefaultValueForType(@this.Metadata.ParameterType);
        }
    }

    public static class CommandCallExtensions
    {
        public static Argument FindCommandArgument(this CommandCall @this, string argName)
        {
            return @this.CommandArguments.FirstOrDefault(arg => arg.Name == argName);
        }
    }
}
