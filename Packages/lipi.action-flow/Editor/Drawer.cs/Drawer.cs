
using System;
using System.Collections;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ActionFlow
{
    public class Drawer:VisualElement
    {
        private object _target;

        public Drawer(object target)
        {
            _target = target;
            Draw();
            

        }
        
        
        

        private void Draw()
        {
            if (_target == null) return;

            var fields = _target.GetType().GetFields();
            DrawFields(fields, "");
        }


        private void DrawFields(FieldInfo[] fieldInfos, string parentPath)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                BindableElement element = null;
                if (fieldInfo.FieldType == typeof(int))
                {
                    var field = new IntegerField(fieldInfo.Name);
                    RegisterChangeEvent(field);
                    element = field;
                } else if (fieldInfo.FieldType == typeof(float))
                {
                    var field = new FloatField(fieldInfo.Name);
                    RegisterChangeEvent(field);
                    element = field;
                } else if (fieldInfo.FieldType == typeof(string))
                {
                    var field = new TextField(fieldInfo.Name);
                    RegisterChangeEvent(field);
                    element = field;
                }else if(fieldInfo.FieldType.IsEnum)
                {
                    var field = new EnumField(fieldInfo.Name, (Enum) Activator.CreateInstance(fieldInfo.FieldType));
                    RegisterChangeEvent(field);
                    element = field;
                }else if(fieldInfo.FieldType == typeof(Object) || fieldInfo.FieldType.IsSubclassOf(typeof(Object))) {
                    var field = new ObjectField(fieldInfo.Name) {objectType = fieldInfo.FieldType};
                    RegisterChangeEvent(field);
                    element = field;
                } else if (fieldInfo.FieldType.IsClass)
                {
                    var label = new Label(fieldInfo.Name);
                    Add(label);
                    var subFieldInfos = fieldInfo.FieldType.GetFields();
                    var path = string.IsNullOrEmpty(parentPath)? fieldInfo.Name: $"{parentPath}/{fieldInfo.Name}" ;
                    DrawFields(subFieldInfos,path);
                } else if (fieldInfo.FieldType.IsValueType)
                {
                    var label = new Label(fieldInfo.Name);
                    Add(label);
                    var subFieldInfos = fieldInfo.FieldType.GetFields();
                    var path = string.IsNullOrEmpty(parentPath)? fieldInfo.Name: $"{parentPath}/{fieldInfo.Name}" ;
                    DrawFields(subFieldInfos,path);
                }
                
                
                if (element == null) continue;
                if (string.IsNullOrEmpty(parentPath))
                {
                    element.bindingPath = fieldInfo.Name;
                }
                else
                {
                    element.bindingPath = $"{parentPath}/{fieldInfo.Name}" ;
                }
                Add(element);
            }
        }

        private void RegisterChangeEvent<T>(BaseField<T> element)
        {
            element.RegisterCallback<ChangeEvent<T>>(delegate(ChangeEvent<T> evt) {  
                if (evt.currentTarget is BindableElement e)
                {
                    SetObjectValue(_target,evt.newValue, e.bindingPath);
                }
            });
        }
        


        //array.2.name
        public static void SetObjectValue(object target, object value, string path)
        {
            var index = path.IndexOf('/');
            var t = target.GetType();
           
            if (index == -1)
            {
                if (target is IList array)
                {
                    var arrayIndex = Convert.ToInt32(path);
                    array[arrayIndex] = value;
                }
                else
                {
                    var field = t.GetField(path);
                    field.SetValue(target, value);
                }
            }
            else
            {
                var fieldName = path.Substring(0, index);
                var subPath = path.Substring(index + 1);
                if (target is IList array)
                {
                    var arrayIndex = Convert.ToInt32(fieldName);
                    var curObject = array[arrayIndex];
                    var elementType = t.GetElementType();
                    if (curObject == null && elementType!=null) curObject = Activator.CreateInstance(elementType);
                    SetObjectValue(curObject, value, subPath);
                    array[arrayIndex] = curObject;
                }
                else
                {
                    var field = t.GetField(fieldName);
                    var curObject = field.GetValue(target);

                    if (curObject == null) curObject = Activator.CreateInstance(field.FieldType);
                    SetObjectValue(curObject, value, subPath);
                    field.SetValue(target, curObject);
                }
            }
        }
        
        


    }


}
