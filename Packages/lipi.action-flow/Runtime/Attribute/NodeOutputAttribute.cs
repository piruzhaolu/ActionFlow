using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionFlow
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NodeOutputAttribute:Attribute
    {
        public string Name = string.Empty;
        public int ID;
        public Type Type;
        
        public NodeOutputAttribute()
        {

        }

        public NodeOutputAttribute(Type type, int id = 0)
        {
            Type = type;
            ID = id;
        }
    }
}
