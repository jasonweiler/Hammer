using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArgumentsAttribute : Attribute
    {
        public string Description{ get; set; } = "";
        public string AltName{ get; set; } = null;
        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = Int32.MaxValue;
    }
}
