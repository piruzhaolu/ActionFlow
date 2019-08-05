using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEditor;

namespace ActionFlow
{


    public class ElementGenerate
    {
        private static Dictionary<Type, Type> ValueElementMap = null;

        private static Dictionary<Type, Type> GetValueElementMap()
        {
            if (ValueElementMap == null)
            {
                ValueElementMap = new Dictionary<Type, Type>()
                {
                    [typeof(float)] = typeof(FloatField),
                    [typeof(int)] = typeof(IntegerField),
                    [typeof(Vector2)] = typeof(Vector2Field),
                    [typeof(string)] = typeof(TextField),
                };
            }
            return ValueElementMap;
        }

        private static BindableElement GetField(Type type)
        {
            var map = GetValueElementMap();

            if (map.TryGetValue(type, out var be))
            {
                var inst = Activator.CreateInstance(be) as BindableElement;
                return inst;
            }

            var of = new ObjectField();
            of.objectType = type;
            return of;
        }

        



        public static BindableElement Generate(Type type, SerializedProperty so)//, string bindingPath
        {
            if (type == typeof(NullStatus)) return null;
            var be = GetField(type);
            //be.bindingPath = bindingPath;
            be.BindProperty(so);
            be.AddToClassList("node-field-input");
            return be;
        }

    }

}
