using System;
using System.Collections;
using System.Collections.Generic;

namespace ActionFlow
{
    public static class DynamicObjectUtility
    {


        public static object GetObjectValue(object target, string path)
        {
            
            return null;
        }
        
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