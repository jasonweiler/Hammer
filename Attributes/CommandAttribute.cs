using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Description { get; set; } = null;
        public string AltName { get; set; } = null;
    }
}
