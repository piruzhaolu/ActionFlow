using UnityEngine;
using System.Collections;
using System;


namespace ActionFlow
{
    public class NodeInfoAttribute : Attribute
    {
        public string Name;
        public string MenuName;

        public NodeInfoAttribute(string name)
        {
            var str = name.Split('/');
            Name = str[str.Length - 1];
            MenuName = name;

        }


    }

}