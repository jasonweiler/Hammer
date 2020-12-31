using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Hammer.Attributes;

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
                    var parameterMapping = CreateParameterMapping(cmdInfo, callArgs);

                    if (parameterMapping == null || parameterMapping.ErrorDiagnostics.Any())
                    {
                        // error!
                    }
                    else
                    {
                        // Call our function
                        var functionType = cmdInfo.Metadata.ReflectedType;
                        var invokeObj = Activator.CreateInstance(functionType);
                        cmdInfo.Metadata.Invoke(invokeObj, parameterMapping.Arguments.ToArray());
                    }

                    /*
                    if (ValidateCommand(cmdInfo, callArgs))
                    {
                        CallCommand(cmdInfo, callArgs);
                    }
                    */
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
            var result = new ParameterMapping();
            
            foreach(var param in cmdInfo.Parameters)
            {
                // find an arg in the call or use the default
                var paramName = param.GetEffectiveName();

                var callArg = callArgs.FindCommandArgument(paramName);

                object value = null;
                if (callArg != null)
                {
                    value = Convert.ChangeType(callArg.Value, param.Metadata.ParameterType);
                }
                else if (param.GetIsOptional())
                {
                    // add in the default value
                    value = param.GetParameterDefaultValue();
                }
                else
                {
                    result.ErrorDiagnostics.Add($"Failed to map parameter \"{paramName}\"");
                    value = null;
                }

                result.Arguments.Add(value);
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
