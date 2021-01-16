using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TargetsAttribute : ParameterAttributeBase
    {
        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = Int32.MaxValue;
    }
}
