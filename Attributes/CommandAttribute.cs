using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Description { get; set; }
        public string AltName { get; set; }
    }
}
