using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : Attribute
    {
        public string Description{ get; set; } = "";
        public string AltName{ get; set; } = null;
        public object Default{ get; set; } = null;
        public bool Optional { get; set; } = false;
    }
}
