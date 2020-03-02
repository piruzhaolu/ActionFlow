using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple =false)]
    public class NodeOutputBTAttribute : Attribute
    {
        public int MaxLink;
        public int ID;

        public NodeOutputBTAttribute(int maxLink)
        {
            MaxLink = maxLink;
        }

        public NodeOutputBTAttribute()
        {
            MaxLink = 1;
        }
    }

}

