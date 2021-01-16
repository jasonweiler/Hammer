using System.Collections.Generic;

namespace Hammer
{
    public class Argument
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HasValue { get; set; }
    }

    public class CommandCall
    {
        public string GroupName { get; set; }
        public string Name {get; set; }

        // Arguments that are for the framework go here (eg. -help)
        public IList<Argument> HammerArguments { get; } = new List<Argument>();

        // all command-specific args go here
        public IList<Argument> CommandArguments { get; } = new List<Argument>();

        // non-switch arguments go here
        public IList<string> TargetParameters { get; } = new List<string>();

    }
}
