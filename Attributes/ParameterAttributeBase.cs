using System;

namespace Hammer.Attributes
{
    public abstract class ParameterAttributeBase : Attribute
    {
        public string Description{ get; set; } = "";
        public object Default{ get; set; } = null;
        public bool Optional { get; set; } = false;
    }
}
