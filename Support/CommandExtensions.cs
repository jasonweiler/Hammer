using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hammer.Attributes;
using Hammer.Support;

namespace Hammer.Extensions
{
    public static class CommandGroupExtensions
    {
        static readonly string CommandsSuffix = "Commands";
        static readonly int CommandsSuffixLength = CommandsSuffix.Length;

        public static string GetEffectiveName(this CommandGroupInfo @this)
        {

            var result = @this.Attribute?.AltName;
            if (result == null)
            {
                result = @this.Metadata.Name;
                if (result.EndsWith(CommandsSuffix))
                {
                    result = result.Substring(0, result.Length - CommandsSuffixLength);
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

            return allGroupCommands.FirstOrDefault(cmd => cmd.GetEffectiveName().IEquals(cmdName));
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
            if (@this.ParamAttribute?.Optional == true)
            {
                return @this.ParamAttribute.Default;
            }

            return @this.Metadata.IsOptional 
                ? @this.Metadata.DefaultValue 
                : GetDefaultValueForType(@this.Metadata.ParameterType);
        }

        public static object CreateContainer(this CommandParameterInfo @this)
        {
            object result = null;

            var paramType = @this.Metadata.ParameterType;
            if (paramType.IsGenericType)
            {
                var fieldType = @this.Metadata.ParameterType;
                if (fieldType.IsGenericType)
                {
                    var genericArgs = fieldType.GenericTypeArguments; // there better be only one of these!

                    var concreteEnumerableType = typeof(IEnumerable<>).MakeGenericType(genericArgs);

                    if (concreteEnumerableType.IsAssignableFrom(fieldType))
                    {
                        var concreteListType = typeof(List<>).MakeGenericType(genericArgs);

                        result =  Activator.CreateInstance(concreteListType);
                    }
                }
            }

            return result;
        }

        public static bool IsArgumentList(this CommandParameterInfo @this)
        {
            var paramType = @this.Metadata.ParameterType;
            return paramType.IsGenericType 
                   && typeof(IEnumerable<>).IsAssignableFrom(paramType.GetGenericTypeDefinition());
        }
    }

    public static class CommandCallExtensions
    {
        public static Argument FindCommandArgument(this CommandCall @this, string argName)
        {
            return @this.CommandArguments.FirstOrDefault(arg => arg.Name.IEquals(argName));
        }

        public static string GetFullCommandName(this CommandCall @this)
        {
            return $"{@this.GroupName}.{@this.Name}";
        }

        public static Argument FindParameter(this CommandCall @this, string parameterName)
        {
            return @this.CommandArguments.FirstOrDefault(param => param.Name.IEquals(parameterName));
        }

        public static Argument FindHammerParameter(this CommandCall @this, string parameterName)
        {
            return @this.HammerArguments.FirstOrDefault(param => param.Name.IEquals(parameterName));
        }
    }
}
