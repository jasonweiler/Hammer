using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Schema;
using Hammer.Attributes;
using Hammer.Extensions;
using Hammer.Support;


namespace Hammer
{
    class Program
    {
        public class ParameterMapping
        {
            public List<object> Arguments { get; }= new List<object>();
        }

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

        static void AdjustLogLevel(NamedArgument logParam)
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

                // We have a valid mapping, so output some warnings for unmapped parameters
                LogUnusedArguments(callArgs);

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

        static void LogUnusedArguments(CommandCall callArgs)
        {
            foreach(var arg in callArgs.CommandArguments.Where(arg => !arg.WasMapped))
            {
                if (arg.HasValue)
                {
                    Log.Warning($"Unused argument: /{arg.Name}={arg.Value}");
                }
                else
                {
                    Log.Warning($"Unused switch: /{arg.Name}");
                }
            }

            foreach(var target in callArgs.TargetParameters.Where(target => !target.WasMapped))
            {
                Log.Warning($"Unused target: {target.Value}");
            }
        }

        static bool CreateListTargetParameterValue(CommandParameterInfo param, CommandCall callArgs, out object parameterValue)
        {
            parameterValue = null;
            if (!(param.ParamAttribute is TargetParameterAttribute targetAttribute)) 
            {
                // Should never happen - can't have a target parameter without an attribute
                return false;
            }

            if (callArgs.TargetParameters.Count < targetAttribute.MinCount)
            {
                Log.Error($"Found {callArgs.TargetParameters.Count} target arguments. Expected: At least {targetAttribute.MinCount}.");
                return false;
            }

            if (callArgs.TargetParameters.Count > targetAttribute.MaxCount)
            {
                Log.Error($"Found {callArgs.TargetParameters.Count} target arguments. Expected: No more than {targetAttribute.MaxCount}.");
                return false;
            }

            // the target is a generic container, so pile all the target arguments into it
            var newContainer = param.CreateContainer();
            var containerType = newContainer.GetType();
                        
            var addMethod = newContainer.GetType().GetMethod("Add");
            if (addMethod != null)
            {
                var strConv = TypeDescriptor.GetConverter(typeof(string));
                var containerInnerType = containerType.GetGenericArguments()[0];

                foreach (var targetParam in callArgs.TargetParameters)
                {
                    var tgtObj = strConv.ConvertTo(targetParam.Value, containerInnerType);
                    addMethod.Invoke(newContainer, new[] { tgtObj });

                    targetParam.WasMapped = true;
                }
            }

            parameterValue = newContainer;

            return parameterValue != null;
        }


        static bool CreateSingleTargetParameterValue(CommandParameterInfo param, CommandCall callArgs, out object parameterValue)
        {
            parameterValue = null;
            // the target parameter is just one value
            if (callArgs.TargetParameters.Count > 1 && !param.IsOptional())
            {
                Log.Warning($"Found {callArgs.TargetParameters.Count} target arguments. Expected: 1 (extra will be ignored)");
            }

            if (callArgs.TargetParameters.Any())
            {
                var targetParam = callArgs.TargetParameters.First();
                parameterValue = Convert.ChangeType(targetParam, param.Metadata.ParameterType);
                targetParam.WasMapped = true;
            }
            else if (param.IsOptional())
            {
                parameterValue = param.GetParameterDefaultValue();
            }
            else
            {
                Log.Error($"No target arguments found. Expected: 1");
            }

            return parameterValue != null;
        }

        static bool CreateTargetParameterValue(CommandParameterInfo param, CommandCall callArgs, out object parameterValue)
        {
            parameterValue = null;

            if (param.IsTargetList())
            {
                return CreateListTargetParameterValue(param, callArgs, out parameterValue);
            }
            
            if (param.IsTargetSingle())
            {
                return CreateSingleTargetParameterValue(param, callArgs, out parameterValue);
            }

            return false;
        }

        static bool CreateNamedParameterValue(CommandParameterInfo param, CommandCall callArgs, out object parameterValue)
        {
            parameterValue = null;

            // find an arg in the call or use the default
            var paramName = param.GetEffectiveName();

            var callArg = callArgs.FindCommandArgument(paramName);

            if (callArg != null && callArg.HasValue)
            {
                if (param.Metadata.ParameterType.IsEnum)
                {
                    try
                    {
                        parameterValue = ParseEnumeratedValue(param.Metadata.ParameterType, callArg.Value);
                        callArg.WasMapped = true;
                    }
                    catch(Exception ex)
                    {
                        Log.Exception(ex, $"Couldn't parse enumerated value argument: \"{param.Metadata.ParameterType.Name}.{callArg.Value}\"");
                        
                        return false;
                    }
                    
                    if (parameterValue == null)
                    {
                        Log.Error($"Couldn't parse enumerated value argument: \"{param.Metadata.ParameterType.Name}.{callArg.Value}\"");
                        return false;
                    }
                }
                else
                {
                    // regular argument with a value
                    parameterValue = Convert.ChangeType(callArg.Value, param.Metadata.ParameterType);
                    callArg.WasMapped = true;
                }
            }
            else if (callArg != null && param.Metadata.ParameterType == typeof(bool))
            {
                // bool arg w/o a value, so simple existence means true
                parameterValue = true;
                callArg.WasMapped = true;
            }
            else if (param.IsOptional())
            {
                // add in the default value
                parameterValue = param.GetParameterDefaultValue();
            }
            else
            {
                // Can't map this arg
                Log.Error($"Failed to map parameter: '{paramName}' to Command: \"{callArgs.GetFullCommandName()}\"");
                return false;
            }

            return true;
        }

        static ParameterMapping CreateParameterMapping(CommandInfo cmdInfo, CommandCall callArgs)
        {
            var paramMapping = new ParameterMapping();
           
            foreach(var param in cmdInfo.Parameters)
            {
                object parameterValue;
                if (param.IsTargetParameter())
                {
                    if (!CreateTargetParameterValue(param, callArgs, out parameterValue))
                    {
                        return null;
                    }
                }
                else if (param.IsNamedParameter())
                {
                    if (!CreateNamedParameterValue(param, callArgs, out parameterValue))
                    {
                        return null;
                    }
                }
                else
                {
                    // really shouldn't happen since the absence of a parameter attribute is considered named
                    Log.Error($"Unresolved parameter {param.GetEffectiveName()} could not be mapped");
                    return null;
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
        public static bool IsHammerSwitch(this NamedArgument @this)
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
