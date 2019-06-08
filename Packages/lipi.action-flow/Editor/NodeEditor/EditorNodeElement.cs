using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

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
            _typeInfo = NodeTypeInfo.GetNodeTypeInfo(so.GetType());
            CreateFieldElement();
        }

        private ScriptableObject _so;
        private EditorActionNode _node;
        private NodeTypeInfo _typeInfo;


        private void CreateFieldElement()
        {
            var mSO = new SerializedObject(_so);
            var fieldInfos = _typeInfo.FieldInfos;
            foreach (var item in fieldInfos)
            {
                var ve = new VisualElement();
                ve.AddToClassList("node-field");

                if (item.FieldType.IsArray && (item.MaxLink != -1 )) //|| item.IOInfo != null
                {
                    var arrayField = new NodeArrayField(mSO, item.Path, item, _node);
                    ve.Add(arrayField);
                }
                else
                {
                    Port outPort = null;
                    if (item.IOInfo != null)
                    {
                        if (item.IOInfo.Mode == NodeTypeInfo.IOMode.InputParm)
                        {
                            var portIn = _node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, null);
                            portIn.source = item.IOInfo;
                            portIn.portName = item.IOInfo.Name;
                            portIn.portColor = NodeTypeInfo.IOModeColor(NodeTypeInfo.IOMode.InputParm);
                            portIn.AddToClassList("inputparm-field");
                            ve.Add(portIn);
                        }
                        else if (item.IOInfo.Mode == NodeTypeInfo.IOMode.OutputParm)
                        {
                            var port = _node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, null);
                            port.source = item.IOInfo;
                            port.portName = "";// item.IOInfo.Name;
                            port.portColor = NodeTypeInfo.IOModeColor(NodeTypeInfo.IOMode.OutputParm);
                            port.AddToClassList("outputparm-field");
                            outPort = port;

                        }
                    } else if(item.BT_IOInfo != null)
                    {
                        var port = _node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
                        port.source = item.BT_IOInfo;
                        port.portName = "";// item.IOInfo.Name;
                        port.portColor = NodeTypeInfo.IOModeColor(NodeTypeInfo.IOMode.BTOutput);
                        port.AddToClassList("outputparm-field");
                        outPort = port;
                    }

                    if (item.Name != string.Empty && (item.IOInfo == null || outPort != null))
                    {
                        var labelField = new Label(item.Name);
                        labelField.AddToClassList("node-field-label");
                        ve.Add(labelField);
                    }

                    AddField(ve, item.FieldType, mSO, item.Path);
                    if (outPort != null)
                    {
                        ve.Add(outPort);
                    }
                }
               
                Add(ve);
            }

            var outputParms = _typeInfo.OutputParm;
            foreach (var item in outputParms)
            {
                var ve = new VisualElement();
                ve.AddToClassList("node-field");
                ve.AddToClassList("outputparm-method");

                var port = _node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, null);
                port.source = item;
                port.portName = item.Name;
                
                ve.Add(port);
                Add(ve);
            }
        }




        private void AddField(VisualElement parent, Type fieldType, SerializedObject so, string path)
        {
            var be = DrawField(fieldType);
            be.bindingPath = path;// $"Value.{fields[i].Name}";
            be.Bind(so);
            be.AddToClassList("node-field-input");
            parent.Add(be);
        }



        private BindableElement DrawField(Type type)
        {
            if (type == typeof(float)) return new FloatField();
            else if (type == typeof(int)) return new IntegerField();
            else if (type == typeof(Vector2)) return new Vector2Field();
            else if (type == typeof(Vector3)) return new Vector3Field();
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


//var be = DrawField(item.FieldType);
//be.bindingPath = item.Path;// $"Value.{fields[i].Name}";
//be.Bind(mSO);
//be.AddToClassList("node-field-input");
//ve.Add(be);