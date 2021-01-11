using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Hammer
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandArgs = CommandLineParser.Parse(args);

            if (commandArgs.IsEnumeratedHelpCommand())
            {
                // list all commands in one or all hives
                HelpSupport.OutputAllCommandGroupHelp(commandArgs.GroupName);
            }
            else 
            {
                var helpParam = commandArgs.FindHammerParameter("Help") ?? commandArgs.FindHammerParameter("?");
                if (helpParam != null)
                {
                    HelpSupport.OutputCommandHelp(commandArgs.GroupName, commandArgs.Name);
                }
                else
                {
                    ExecuteHammerCommand(commandArgs);
                }
            }
        }

        private static void ExecuteHammerCommand(CommandCall callArgs)
        {
            try
            {
                var cmdGroupInfo = CommandSupport.FindCommandGroup(callArgs.GroupName);
                if (cmdGroupInfo != null)
                {
                    var cmdInfo = cmdGroupInfo.FindCommand(callArgs.Name);
                    if (cmdInfo != null)
                    {
                        var parameterMapping = CreateParameterMapping(cmdInfo, callArgs);

                        if (parameterMapping == null || parameterMapping.ErrorDiagnostics.Any())
                        {
                            // error!
                            foreach(var diagnostic in parameterMapping.ErrorDiagnostics)
                            {
                                Console.Out.WriteLine("Error!");
                                Console.Out.WriteLine($"\t{diagnostic}");
                            }
                        }
                        else
                        {
                            // Call our function
                            var functionType = cmdInfo.Metadata.ReflectedType;
                            var invokeObj = Activator.CreateInstance(functionType);
                            cmdInfo.Metadata.Invoke(invokeObj, parameterMapping.Arguments.ToArray());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class ParameterMapping
        {
            public List<object> Arguments { get; }= new List<object>();

            public List<string> ErrorDiagnostics { get; } = new List<string>();
        }

        private static ParameterMapping CreateParameterMapping(CommandInfo cmdInfo, CommandCall callArgs)
        {
            object newContainer = null;
            var result = new ParameterMapping();
           
            foreach(var param in cmdInfo.Parameters)
            {
                // find an arg in the call or use the default
                var paramName = param.GetEffectiveName();

                object value = null;

                if (param.IsArgumentList())
                {
                    newContainer = param.CreateContainer();
                    Type containerType = newContainer.GetType();
                    
                    var addMethod = newContainer.GetType().GetMethod("Add");

                    var strConv = TypeDescriptor.GetConverter(typeof(string));
                    var containerInnerType = containerType.GetGenericArguments()[0];

                    foreach (var tgtArg in callArgs.TargetParameters)
                    {
                        var tgtObj = strConv.ConvertTo(tgtArg, containerInnerType);
                        addMethod.Invoke(newContainer, new object[] {tgtObj});
                    }

                    value = newContainer;
                }
                else
                {
                    var callArg = callArgs.FindCommandArgument(paramName);

                    if (callArg != null && callArg.HasValue)
                    {
                        if (param.Metadata.ParameterType.IsEnum)
                        {
                            value = ParseEnumeratedValue(param.Metadata.ParameterType, callArg.Value);
                        }
                        else
                        {
                            // regular argument w/ a value
                            value = Convert.ChangeType(callArg.Value, param.Metadata.ParameterType);
                        }
                    }
                    else if (callArg != null && param.Metadata.ParameterType == typeof(bool))
                    {
                        // bool arg w/o a value, so existence vs. not
                        value = true;
                    }
                    else if (param.GetIsOptional())
                    {
                        // add in the default value
                        value = param.GetParameterDefaultValue();
                    }
                    else
                    {
                        // Can't map this arg
                        result.ErrorDiagnostics.Add($"Failed to map parameter \"{paramName}\"");
                        value = null;
                    }
                }

                result.Arguments.Add(value);
            }

            return result;
        }

        private static object ParseEnumeratedValue(Type parameterType, string strValue)
        {
            object result = null;
            if (parameterType.IsEnum)
            {
                try
                {
                    result = Enum.Parse(parameterType, strValue, true);
                }
                finally
                { }
            }
            
            return result;
        }

        private static bool ValidateCommand(CommandInfo cmdInfo, CommandCall cmdArgs)
        {
            var result = true;
            // make sure we have all the required args
            foreach(var paramInfo in cmdInfo.Parameters)
            {
                var name = paramInfo.GetEffectiveName();

                // see if this parameter has a command line arg
                var cmdParam = cmdArgs.FindParameter(name);
                if (cmdParam == null && !paramInfo.GetIsOptional())
                {
                    // not specified, and not optional, so this is an error
                    // ADD ERROR!
                }
                else //if (ValidateParameter(paramInfo, cmdParam))
                {
                    //CreateArgumentMapping(param)
                    // make sure the type is correct

                }
            }
            
            return result;
        }
        
        private static void CallCommand(CommandInfo cmdInfo, CommandCall cmdArgs)
        {
            throw new NotImplementedException();
        }
    }

    public static class HammerArgExtensions
    {
        public static bool IsHammerSwitch(this Argument @this)
        {
            switch (@this.Name)
            {
                case "help":
                case "?":
                    return true;
            }

            return false;
        }

        public static bool IsEnumeratedHelpCommand(this CommandCall @this)
        {
            return @this.GroupName == null || @this.Name == null;
        }
    }
}
