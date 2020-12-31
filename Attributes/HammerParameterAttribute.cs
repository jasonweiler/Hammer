using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hammer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HammerParameterAttribute : Attribute
    {
        public string Description{ get; set; }
        public string AltName{get; set;}
    }
}
