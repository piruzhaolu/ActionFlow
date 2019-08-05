using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ActionFlow
{
    public static class SerializedPropertyExt
    {
        public static Type GetValueType(this SerializedProperty property)
        {
            return GetValue(property)?.GetType();
        }

        public static object GetValue(this SerializedProperty property)
        {
            var pPath = property.propertyPath.Replace(".Array.data[", "[");
            var obj = (object)property.serializedObject.targetObject;
            var elements = pPath.Split('.');
            foreach (var element in elements)//.Take(elements.Length-1)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }



        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }

}
