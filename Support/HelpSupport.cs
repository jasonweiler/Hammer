using System.Collections.Generic;
using System.Linq;
using Hammer.Attributes;
using Hammer.Extensions;

namespace Hammer.Support
{
    public static class HelpSupport
    {
        static readonly string OptionalTag = "[opt]";
        static readonly string NonOptionalTag = new string(' ', OptionalTag.Length);

        public static void OutputCommandGroupHelp(CommandGroupInfo groupInfo)
        {
            // Output all entry points in this group
            var groupDescriptionText = groupInfo.Attribute.Description ?? "";

            if (groupDescriptionText.Any())
            {
                groupDescriptionText = $" - {groupDescriptionText}";
            }

            Log.Out($"{groupInfo.GetEffectiveName()}{groupDescriptionText}");
            var groupCommandInfos = groupInfo.EnumerateGroupCommands();

            foreach (var commandInfo in groupCommandInfos)
            {
                var descriptionText = commandInfo.CmdAttribute.Description ?? "";

                if (descriptionText.Any())
                {
                    descriptionText = $" - {descriptionText}";
                }

                Log.Out($"\t{groupInfo.GetEffectiveName()}.{commandInfo.GetEffectiveName()}{descriptionText}");
            }

            Log.Out();
        }

        public static void OutputAllCommandGroupHelp(string cmdGroupFilterName)
        {
            var targetGroups = new List<CommandGroupInfo>();

            if (cmdGroupFilterName != null)
            {
                var commandGroup = CommandSupport.FindCommandGroup(cmdGroupFilterName);
                if (commandGroup != null)
                {
                    targetGroups.Add(commandGroup);
                }
                else
                {
                    Log.Error($"Command group '{cmdGroupFilterName}' not found!");
                }
            }
            else
            {
                targetGroups.AddRange(CommandSupport.FindAllCommandGroups());
            }

            foreach (var groupInfo in targetGroups.OrderBy(info => info.GetEffectiveName()))
            {
                OutputCommandGroupHelp(groupInfo);
            }
        }

        public static void OutputCommandHelp(string groupName, string cmdName)
        {
            var groupInfo = CommandSupport.FindCommandGroup(groupName);
            if (groupInfo == null)
            {
                Log.Warning($"CommandGroup '{groupName}' was not found.");
                return;
            }

            var cmdInfo = groupInfo.FindCommand(cmdName);
            if (cmdInfo == null)
            {
                Log.Warning($"Command '{groupName}.{cmdName}' was not found.");
                return;
            }

            OutputCommandHelp(groupInfo, cmdInfo);
        }

        public static void OutputCommandHelp(CommandGroupInfo groupInfo, CommandInfo cmdInfo)
        {
            var descText = cmdInfo.CmdAttribute.Description;

            if (descText != null)
            {
                descText = $"- {descText}";
            }

            Log.Out($"{groupInfo.GetEffectiveName()}.{cmdInfo.GetEffectiveName()} {descText ?? ""}");

            foreach(var paramInfo in cmdInfo.Parameters)
            {
                var optTagText = NonOptionalTag;
                string defaultValueText = null;

                if (paramInfo.IsOptional())
                {
                    optTagText = OptionalTag;
                    var defaultValue = paramInfo.GetParameterDefaultValue();
                    if (paramInfo.Metadata.ParameterType == typeof(string))
                    {
                        defaultValueText = $"(default: \"{defaultValue}\")";
                    }
                    else if (defaultValue != null)
                    {
                        defaultValueText = $"(default: {defaultValue})";
                    }
                }

                var valueOptions = CreateArgumentOptions(paramInfo);
                string valueOptionsText = null;
                if (valueOptions?.Any() == true)
                {
                    valueOptionsText = $"(one of: {", ".JoinString(valueOptions)})";
                }

                var paramDescText = paramInfo.ParamAttribute?.Description;

                if (paramDescText != null)
                {
                    paramDescText = $"- {paramDescText}";
                }

                if (paramInfo.IsTargetParameter())
                {
                    var rangeText = "";
                    if (paramInfo.ParamAttribute is TargetParameterAttribute targetAttribute)
                    {
                        if (targetAttribute.MinCount <= 0 && targetAttribute.MaxCount < 0)
                        {
                            rangeText = " (any number)";
                        }
                        else if (targetAttribute.MinCount > 0 && targetAttribute.MaxCount < 0)
                        {
                            rangeText = $" (at least {targetAttribute.MinCount})";
                        }
                        else if (targetAttribute.MinCount <= 0 && targetAttribute.MaxCount > 0)
                        {
                            rangeText = $" (up to {targetAttribute.MaxCount})";
                        }
                        else
                        {
                            rangeText = $" (between {targetAttribute.MinCount}..{targetAttribute.MaxCount})";
                        }
                    }
                    Log.Out($"\t{optTagText} [Targets{rangeText}] {paramDescText}");
                }
                else
                {
                    Log.Out($"\t{optTagText} /{paramInfo.GetEffectiveName()} {paramDescText ?? ""}\t{defaultValueText ?? ""}");
                    if (valueOptionsText != null)
                    {
                        Log.Out($"\t\t{valueOptionsText}");
                    }

                }
            }
        }

        static string CreateArgumentTypeText(CommandParameterInfo paramInfo)
        {
            var paramType = paramInfo?.Metadata?.ParameterType;
            return paramType?.Name ?? "";
        }

        static IEnumerable<string> CreateArgumentOptions(CommandParameterInfo paramInfo)
        {
            var paramType = paramInfo?.Metadata?.ParameterType;
            if (paramType != null && paramType.IsEnum)
            {
                return paramType.GetEnumNames();
            }

            return null;
        }
    }

}
