using System;

namespace Maul.Attributes
{
    public abstract class ParameterAttributeBase : Attribute
    {
        public string Description{ get; set; } = null;
    }
}
