using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;

namespace ActionFlow
{
    /// <summary>
    /// node的数据展示UI
    /// </summary>
    public class EditorNodeElement : VisualElement
    {

        
        public EditorNodeElement(ScriptableObject so, EditorActionNode node)
        {
            _so = so;
            _node = node;
            CreateFieldElement();
        }

        private ScriptableObject _so;
        private EditorActionNode _node;


        private void CreateFieldElement()
        {
            var type = _so.GetType();

            var value =  type.GetField("Value");
            var fields = value.FieldType.GetFields();
            var mSO = new SerializedObject(_so);
            for (int i = 0; i < fields.Length; i++)
            {
                var ve = new VisualElement();
                ve.AddToClassList("node-field");

                var labelField = new Label(fields[i].Name);
                labelField.AddToClassList("node-field-label");
                ve.Add(labelField);

                var be = DrawField(fields[i].FieldType);
                be.bindingPath = $"Value.{fields[i].Name}";
                be.Bind(mSO);
                be.AddToClassList("node-field-input");
                ve.Add(be);

                Add(ve);

            }
        }


        private BindableElement DrawField(Type type)
        {
            if (type == typeof(float)) return new FloatField();
            else if (type == typeof(int)) return new IntegerField();
            else if (type == typeof(Vector2)) return new Vector2Field();
            else if (type == typeof(string)) return new TextField();
            else
            {
                var of = new ObjectField();
                of.objectType = type;
                return of;
            }
        }

    }
}
