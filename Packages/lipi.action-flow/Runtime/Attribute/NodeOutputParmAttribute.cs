using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple =false)]
    public class NodeOutputParmAttribute : Attribute
    {

        public int ID;
        public string Name;

        public NodeOutputParmAttribute()
        {

        }

        public NodeOutputParmAttribute(string name, int id = 0)
        {
            ID = id;
            Name = name;

        }

    }

}
