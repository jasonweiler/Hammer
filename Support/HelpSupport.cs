using System.Collections.Generic;
using System.Linq;
using Hammer.Extensions;

namespace Hammer.Support
{
    public static class HelpSupport
    {
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

            if (cmdGroupFilterName == null)
            {
                targetGroups.AddRange(CommandSupport.FindAllCommandGroups());
            }
            else
            {
                targetGroups.Add(CommandSupport.FindCommandGroup(cmdGroupFilterName));
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
                Log.Warning($"CommandGroup \"{groupName}\" was not found.");
                return;
            }

            var cmdInfo = groupInfo.FindCommand(cmdName);
            if (cmdInfo == null)
            {
                Log.Warning($"Command \"{groupName}.{cmdName}\" was not found.");
                return;
            }

            OutputCommandHelp(groupInfo, cmdInfo);
        }

        public static void OutputCommandHelp(CommandGroupInfo groupInfo, CommandInfo cmdInfo)
        {
            var descText = cmdInfo.CmdAttribute.Description ?? "";

            if (descText.Any())
            {
                descText = $" - {descText}";
            }

            Log.Out($"{groupInfo.GetEffectiveName()}.{cmdInfo.GetEffectiveName()}{descText}");

            foreach(var paramInfo in cmdInfo.Parameters)
            {
                var paramDescText = paramInfo.ParamAttribute?.Description ?? "";
                var optText = paramInfo.GetIsOptional() ? "[opt]" : "     " ;
                
                if (paramDescText.Any())
                {
                    paramDescText = $" - {paramDescText}";
                }

                Log.Out($"\t{optText} /{paramInfo.GetEffectiveName()}{paramDescText}");
            }
        }
    }

}
