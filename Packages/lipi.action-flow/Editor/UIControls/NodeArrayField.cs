using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace ActionFlow
{

    public class NodeArrayField : BindableElement
    {
        public NodeArrayField(SerializedObject so, string bindingPath, NodeTypeInfo.FieldInfo fieldInfo, EditorActionNode node)
        {
            var elementType = fieldInfo.FieldType.GetElementType();

            _so = so;
            this.bindingPath = bindingPath;
            _sp = _so.FindProperty(bindingPath);
            _elementType = elementType;
            _fieldInfo = fieldInfo;
            _node = node;
            node.EdgeChangeHandler += EdgeChangeHandler;

            _content = new VisualElement();
            _content.AddToClassList("node-array-content");
            DrawList(_content);
            Add(_content);

            //Add(new Button(buttonClick));
        }

        private void EdgeChangeHandler()
        {
            Refresh();
        }

        private SerializedObject _so;
        private SerializedProperty _sp;
        private Type _elementType;
        private VisualElement _content;
        private NodeTypeInfo.FieldInfo _fieldInfo;
        private EditorActionNode _node;

        private List<VisualElement> visualElements;
        //private void buttonClick()
        //{
        //    _sp.InsertArrayElementAtIndex(_sp.arraySize);
        //    _so.ApplyModifiedProperties();
        //    Refresh();
        //}


        private void Refresh()
        {
            var maxIndex = MaxLinkIndex();
            if (visualElements != null )
            {
                while(visualElements.Count - 2 < maxIndex)
                {
                    if (_fieldInfo.MaxLink != -1 && visualElements.Count >= _fieldInfo.MaxLink) break;
                    _sp.InsertArrayElementAtIndex(visualElements.Count);
                    _so.ApplyModifiedProperties();
                    var ve = AddElement(visualElements.Count);
                    visualElements.Add(ve);
                    _content.Add(ve);
                }
            }
            
        }

        private NodeLink[] BTLinks()
        {
            var links = _node.NodeInfo.Childs;
            var maxIndex = -1;
            Dictionary<int, NodeLink> btLinkMap = new Dictionary<int, NodeLink>();
            for (int i = 0; i < links.Count; i++)
            {
                var fromID = links[i].FromID;
                if (fromID >= NodeLink.BTIDPre)
                {
                    int index = (fromID - NodeLink.BTIDPre)/100;
                    btLinkMap.Add(index, links[i]);
                    if (index > maxIndex) maxIndex = index;
                }
            }
            NodeLink[] nodeLinks = new NodeLink[maxIndex+1];
            return nodeLinks;
        }

        private int MaxLinkIndex()
        {
            var links = _node.NodeInfo.Childs;
            var maxIndex = -1;
            for (int i = 0; i < links.Count; i++)
            {
                var fromID = links[i].FromID;
                if (fromID >= NodeLink.BTIDPre)
                {
                    int index = (fromID - NodeLink.BTIDPre) / 100;
                    if (index > maxIndex) maxIndex = index;
                }
            }
            return maxIndex;
        }

        


        private void DrawList(VisualElement parent)
        {
            var arrayLength = _sp.arraySize;
            var links = BTLinks();
            int maxIndex;
            if (_fieldInfo.MaxLink != -1)
            {
                maxIndex = links.Length;
                if (maxIndex >= _fieldInfo.MaxLink) maxIndex = _fieldInfo.MaxLink - 1;

                while (arrayLength-1 > maxIndex)
                {
                    _sp.DeleteArrayElementAtIndex(arrayLength - 1);
                    arrayLength -= 1;
                }
                while (arrayLength-1 < maxIndex)
                {
                    _sp.InsertArrayElementAtIndex(arrayLength);
                    arrayLength += 1;
                }
                _so.ApplyModifiedProperties();
            } else
            {
                maxIndex = arrayLength;
            }
            visualElements = new List<VisualElement>();
            for (int i = 0; i <= maxIndex; i++)
            {
                var arrItem = AddElement(i);
                visualElements.Add(arrItem);
                parent.Add(arrItem);
            }
        }


        private VisualElement AddElement(int i)
        {
            var arrItem = new VisualElement();
            arrItem.AddToClassList("node-field");

            var label = new Label(_fieldInfo.Name);
            label.AddToClassList("node-field-label");
            arrItem.Add(label);

            var arrayItem = _sp.GetArrayElementAtIndex(i);
            var be = ElementGenerate.Generate(_elementType, _so, arrayItem.propertyPath);// $"{_fieldInfo.Path}.Array.Data[{i}]");
            arrItem.Add(be);


            var port = _node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
            var ioInfo = Clone(_fieldInfo.BT_IOInfo);
            ioInfo.ID += i * 100;
            port.source = ioInfo;
            port.portColor = NodeTypeInfo.IOModeColor(_fieldInfo.BT_IOInfo.Mode);
            port.portName = "";// item.IOInfo.Name;
            port.AddToClassList("outputparm-field");
            arrItem.Add(port);
            return arrItem;
        }

        private NodeTypeInfo.IOInfo Clone(NodeTypeInfo.IOInfo info)
        {
            return new NodeTypeInfo.IOInfo
            {
                ID = info.ID,
                Name = info.Name,
                Mode = info.Mode,
                Type = info.Type
            };
        }





    }

}
//while(visualElements.Count-2 > maxIndex)
//{
//    _sp.DeleteArrayElementAtIndex(visualElements.Count - 1);
//    var ve = visualElements[visualElements.Count - 1];
//    visualElements.Remove(ve);
//    _content.Remove(ve);
//}