using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : ParameterAttributeBase
    {
        public string AltName{ get; set; } = null;
    }
}
