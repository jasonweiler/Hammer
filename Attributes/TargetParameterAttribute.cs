using System;

namespace Maul.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TargetParameterAttribute : ParameterAttributeBase
    {
        public int MinCount { get; set; } = 0;
        public int MaxCount { get; set; } = -1;
    }
}
