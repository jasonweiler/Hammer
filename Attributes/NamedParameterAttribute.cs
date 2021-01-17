using System;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class NamedParameterAttribute : ParameterAttributeBase
    {
        public string AltName{ get; set; } = null;
        public bool Optional { get; set; } = false;
        public object Default{ get; set; } = null;
    }
}
