using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using Object = System.Object;

namespace ActionFlow
{
    /// <summary>
    /// node的数据展示UI
    /// </summary>
    public class EditorNodeElement : VisualElement
    {


        public EditorNodeElement(INode so, EditorActionNode node)
        {
            _so = so;
            _node = node;
            _typeInfo = NodeTypeInfo.GetNodeTypeInfo(so.GetType());
            CreateFieldElement();
        }

        private readonly object _so;
        private EditorActionNode _node;
        private NodeTypeInfo _typeInfo;


        private void CreateFieldElement()
        {
            var element = new InspectorElement(_so);
            Add(element);
            return;
            var mSo = _so;
            var fieldInfos = _typeInfo.FieldInfos;
            foreach (var item in fieldInfos)
            {

                var ve = new VisualElement();
                ve.AddToClassList("node-field");

                if (item.FieldType.IsArray && (item.MaxLink != -1)) //|| item.IOInfo != null
                {
//                    var arrayField = new NodeArrayField(mSO, item.Path, item, _node);
//                    ve.Add(arrayField);
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
                    } else if (item.BT_IOInfo != null)
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
                        //ve.Add(GetField(mSO, item.Path, item.Name)); TODO:Re
                    }
                    else
                    {
                        //ve.Add(GetField(mSO, item.Path)); TODO:Re
                    }

                    //AddField(ve, item.FieldType, mSO, item.Path); 
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

            if (_typeInfo.BTOutput != null)
            {
                var ve = new VisualElement();
                ve.AddToClassList("node-field");
                ve.AddToClassList("outputparm-method");

                var port = _node.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, null);
                port.portName = string.Empty;
                port.source = _typeInfo.BTOutput.IOInfo;
                port.portColor = NodeTypeInfo.IOModeColor(_typeInfo.BTOutput.IOInfo.Mode);

                ve.Add(port);
                var _content = new VisualElement();
                _content.AddToClassList("node-array-content");
                _content.Add(ve);
                Add(_content);
            }
        }

//        private VisualElement GetField(SerializedProperty so, string path, string label = null)
//        {
//            var prop = so.FindPropertyRelative(path);
//            var valueType = prop.propertyType;// prop.GetValueType();
//            if (valueType == SerializedPropertyType.ObjectReference) // valueType.IsSubclassOf(typeof(UnityEngine.Object))
//            {
//                var objectField = new ObjectField(label);
//                if (label == null)
//                {
//                    objectField.AddToClassList("hideLabel");
//                }
//                objectField.objectType = prop.GetValueType();
//                objectField.allowSceneObjects = false;
//                objectField.bindingPath = prop.propertyPath;
//                objectField.Bind(prop.serializedObject);
//                objectField.AddToClassList("node-field-input");
//                objectField.binding = new BBB();
//                return objectField;
//
//            } else
//            {
//                PropertyField p = new PropertyField(prop, label);
//                if (label == null)
//                {
//                    p.AddToClassList("hideLabel");
//                }
//                p.Bind(so.serializedObject);
//                p.AddToClassList("node-field-input");
//                return p;
//            }
//        }

    }
}

