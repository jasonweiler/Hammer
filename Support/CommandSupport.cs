using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hammer.Attributes;
using Hammer.Extensions;

namespace Hammer.Support
{
    public class CommandGroupInfo
    {
        public Type Metadata { get; internal set; }
        public CommandGroupAttribute Attribute { get; internal set; }
    }

    public class CommandInfo
    {
        public MethodInfo Metadata { get; internal set; }
        public CommandAttribute CmdAttribute { get; internal set; }

        public IEnumerable<CommandParameterInfo> Parameters { get; internal set; }
    }

    public class CommandParameterInfo
    {
        public ParameterInfo Metadata { get; internal set; }
        public ParameterAttribute ParamAttribute { get; internal set; }
    }

    public class CommandSupport
    {
        public static CommandGroupInfo FindCommandGroup(string cmdGroupName)
        {
            return FindAllCommandGroups()
                .FirstOrDefault(cg => cg.GetEffectiveName().IEquals(cmdGroupName));
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
}
