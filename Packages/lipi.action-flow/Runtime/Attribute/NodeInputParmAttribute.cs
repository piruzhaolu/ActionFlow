using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple =false)]
    public class NodeInputParmAttribute : Attribute
    {

        public int ID;
        public NodeInputParmAttribute()
        {
            
        }

        public NodeInputParmAttribute(int id)
        {
            ID = id;
        }



    }

}
