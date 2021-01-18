using System;

namespace Maul.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        public string Description { get; set; } = null;
        public string AltName { get; set; } = null;
    }
}
