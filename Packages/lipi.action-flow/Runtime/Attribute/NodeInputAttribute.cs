using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeInputAttribute : Attribute
    {
        public string Name;
        public int ID;

        public NodeInputAttribute()
        {
            
        }
        public NodeInputAttribute(int id)
        {
            ID = id;
        }


    }
}
