using System;
using System.Collections.Generic;
using System.ComponentModel;
using Hammer.Extensions;
using Hammer.Support;


namespace Hammer
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandArgs = CommandLineParser.Parse(args);

            var logParam = commandArgs.FindHammerParameter("log");
            if (logParam != null)
            {
                AdjustLogLevel(logParam);
            }

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

        static void AdjustLogLevel(Argument logParam)
        {
            var adjustLogLevelSuccess = false;
            if (logParam.HasValue)
            {
                try
                {
                    if (ParseEnumeratedValue(typeof(LogLevel), logParam.Value) is LogLevel newLogLevel)
                    {
                        Log.LogLevel = newLogLevel;
                        adjustLogLevelSuccess = true;
                    }
                }
                catch
                {
                    // just eat the exception
                }
            }

            if (!adjustLogLevelSuccess)
            {
                Log.Out($"New log level '{logParam.Value}' is not valid. Can be one of: {", ".JoinString(typeof(LogLevel).GetEnumNames())}");
            }
        }

        static void ExecuteHammerCommand(CommandCall callArgs)
        {
            try
            {
                var cmdGroupInfo = CommandSupport.FindCommandGroup(callArgs.GroupName);
                if (cmdGroupInfo == null)
                {
                    Log.Error($"Couldn't find CommandGroup {callArgs.GroupName}");
                    return;
                }

                var cmdInfo = cmdGroupInfo.FindCommand(callArgs.Name);
                if (cmdInfo == null)
                {
                    Log.Error($"Couldn't find Command \"{callArgs.GetFullCommandName()}\"");
                    return;
                }
                
                var parameterMapping = CreateParameterMapping(cmdInfo, callArgs);
                if (parameterMapping == null)
                {
                    HelpSupport.OutputCommandHelp(cmdGroupInfo.GetEffectiveName(), cmdInfo.GetEffectiveName());
                    return;
                }

                // Call our function
                var functionType = cmdInfo.Metadata.ReflectedType;
                if (functionType != null)
                {
                    var invokeObj = Activator.CreateInstance(functionType);
                    cmdInfo.Metadata.Invoke(invokeObj, parameterMapping.Arguments.ToArray());
                }
                else
                {
                    Log.Critical($"Couldn't find command invocation type for {cmdInfo.GetEffectiveName()}");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public class ParameterMapping
        {
            public List<object> Arguments { get; }= new List<object>();
        }

        static ParameterMapping CreateParameterMapping(CommandInfo cmdInfo, CommandCall callArgs)
        {
            var paramMapping = new ParameterMapping();
           
            foreach(var param in cmdInfo.Parameters)
            {
                // find an arg in the call or use the default
                var paramName = param.GetEffectiveName();

                object parameterValue;
                if (param.IsArgumentList())
                {
                    var newContainer = param.CreateContainer();
                    var containerType = newContainer.GetType();
                    
                    var addMethod = newContainer.GetType().GetMethod("Add");
                    if (addMethod != null)
                    {
                        var strConv = TypeDescriptor.GetConverter(typeof(string));
                        var containerInnerType = containerType.GetGenericArguments()[0];

                        foreach (var tgtArg in callArgs.TargetParameters)
                        {
                            var tgtObj = strConv.ConvertTo(tgtArg, containerInnerType);
                            addMethod.Invoke(newContainer, new[] { tgtObj });
                        }
                    }

                    parameterValue = newContainer;
                }
                else
                {
                    var callArg = callArgs.FindCommandArgument(paramName);

                    if (callArg != null && callArg.HasValue)
                    {
                        if (param.Metadata.ParameterType.IsEnum)
                        {
                            try
                            {
                                parameterValue = ParseEnumeratedValue(param.Metadata.ParameterType, callArg.Value);
                            }
                            catch(Exception ex)
                            {
                                Log.Exception(ex, $"Couldn't parse enumerated value argument: \"{param.Metadata.ParameterType.Name}.{callArg.Value}\"");
                                return null;
                            }
                            
                            if (parameterValue == null)
                            {
                                Log.Error($"Couldn't parse enumerated value argument: \"{param.Metadata.ParameterType.Name}.{callArg.Value}\"");
                                return null;
                            }
                        }
                        else
                        {
                            // regular argument w/ a value
                            parameterValue = Convert.ChangeType(callArg.Value, param.Metadata.ParameterType);
                        }
                    }
                    else if (callArg != null && param.Metadata.ParameterType == typeof(bool))
                    {
                        // bool arg w/o a value, so existence vs. not
                        parameterValue = true;
                    }
                    else if (param.GetIsOptional())
                    {
                        // add in the default value
                        parameterValue = param.GetParameterDefaultValue();
                    }
                    else
                    {
                        // Can't map this arg
                        Log.Error($"Failed to map parameter: '{paramName}' to Command: \"{callArgs.GetFullCommandName()}\"");
                        return null;
                    }
                }

                paramMapping.Arguments.Add(parameterValue);
            }

            return paramMapping;
        }

        static object ParseEnumeratedValue(Type enumType, string enumValueString)
        {
            object result = null;
            if (enumType.IsEnum)
            {
                result = Enum.Parse(enumType, enumValueString, true);
            }
            
            return result;
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
                case "log":
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
