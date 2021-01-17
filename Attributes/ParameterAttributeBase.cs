using System;

namespace Hammer.Attributes
{
    public abstract class ParameterAttributeBase : Attribute
    {
        public string Description{ get; set; } = null;
    }
}
