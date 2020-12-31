using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hammer
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

            Console.Out.WriteLine($"{groupInfo.GetEffectiveName()}{groupDescriptionText}");
            var groupCommandInfos = groupInfo.EnumerateGroupCommands();

            foreach (var commandInfo in groupCommandInfos)
            {
                var descriptionText = commandInfo.CmdAttribute.Description ?? "";

                if (descriptionText.Any())
                {
                    descriptionText = $" - {descriptionText}";
                }

                Console.Out.WriteLine($"\t{groupInfo.GetEffectiveName()}.{commandInfo.GetEffectiveName()}{descriptionText}");
            }

            Console.Out.WriteLine();
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

            var cmdInfo = groupInfo.FindCommand(cmdName);

            var descText = cmdInfo.CmdAttribute.Description ?? "";

            if (descText.Any())
            {
                descText = $" - {descText}";
            }

            Console.Out.WriteLine($"{groupInfo.GetEffectiveName()}.{cmdInfo.GetEffectiveName()}{descText}");

            foreach(var paramInfo in cmdInfo.Parameters)
            {
                var paramDescText = paramInfo.ParamAttribute?.Description ?? "";
                var optText = paramInfo.GetIsOptional() ? "[opt]" : "     " ;
                
                if (paramDescText.Any())
                {
                    paramDescText = $" - {paramDescText}";
                }

                Console.Out.WriteLine($"\t{optText} /{paramInfo.GetEffectiveName()}{paramDescText}");
            }
        }
    }

}
