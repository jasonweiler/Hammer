using System.Collections.Generic;

namespace Hammer
{
    public class NamedArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HasValue { get; set; }
        public bool WasMapped { get; set; } = false;
    }

    public class TargetArgument
    {
        public TargetArgument(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
        public bool WasMapped { get; set; } = false;
    }

    public class CommandCall
    {
        public string GroupName { get; set; }
        public string Name {get; set; }

        // Arguments that are for the framework go here (eg. -help)
        public IList<NamedArgument> HammerArguments { get; } = new List<NamedArgument>();

        // all command-specific args go here
        public IList<NamedArgument> CommandArguments { get; } = new List<NamedArgument>();

        // non-switch arguments go here
        public IList<TargetArgument> TargetParameters { get; } = new List<TargetArgument>();

    }
}
