using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace ActionFlow
{
    public class DefaultEditorNodeDraw : IEditorNodeDraw
    {
        private static DefaultEditorNodeDraw Defalt = new DefaultEditorNodeDraw();

        public static IEditorNodeDraw GetEditor(Type type)
        {
            return Defalt;
        }



        public virtual void Create(EditorActionNode node, SerializedProperty asset)
        {
          //  node.Q(name: "selection-border").Add(new Label("aaa"));
            InputDraw(node, asset);
            OutputDraw(node, asset);
            ExtensionDraw(node, asset);
            node.RefreshExpandedState();
        }

        public virtual void DoubleClick(EditorActionNode node, SerializedProperty asset)
        {
        }



        public virtual void ExtensionDraw(EditorActionNode node, SerializedProperty asset)
        {
            var elem = new EditorNodeElement(asset, node);
            node.mainContainer.Add(elem);
        }


        public virtual void InputDraw(EditorActionNode node, SerializedProperty asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetValueType());
            List<NodeTypeInfo.IOInfo> list = new List<NodeTypeInfo.IOInfo>();
            list.AddRange(nodeTypeInfo.Inputs);
            list.AddRange(nodeTypeInfo.BTInputs);

            foreach (var inputInfo in list)
            {
                if(inputInfo.Mode == NodeTypeInfo.IOMode.BTInput)
                {
                    Port.Capacity capacity = Port.Capacity.Single;
                    var portIn = node.InstantiatePort(Orientation.Vertical, Direction.Input, capacity, null);
                    portIn.portColor = NodeTypeInfo.IOModeColor(inputInfo.Mode);
                    portIn.portName = string.Empty;
                    portIn.source = inputInfo;
                    portIn.style.paddingTop = 2;
                    //node.titleContainer.Add(portIn);
                    //node.titleContainer.style.justifyContent = Justify.FlexStart;
                    //node.titleButtonContainer.style.display = DisplayStyle.None;
                    portIn.AddToClassList("btInputPort");
                    //node.inputContainer.Add(portIn);
                    //node.hierarchy.mo
                    node.hierarchy.Insert(0,portIn);
                }
                else
                {
                    Port.Capacity capacity = Port.Capacity.Multi;
                    var portIn = node.InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, null);
                    portIn.portColor = NodeTypeInfo.IOModeColor(inputInfo.Mode);
                    portIn.portName = inputInfo.GetName();
                    portIn.source = inputInfo;
                    node.inputContainer.Add(portIn);
                }
            }

        }

        public virtual void OutputDraw(EditorActionNode node, SerializedProperty asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetValueType());
            for (int i = 0; i < nodeTypeInfo.Outputs.Count; i++)
            {
                var outputInfo = nodeTypeInfo.Outputs[i];

                Port.Capacity capacity = (outputInfo.Mode == NodeTypeInfo.IOMode.BTOutput) ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = node.InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, null);
                port.portName = outputInfo.GetName();
                port.source = outputInfo;
                port.portColor = NodeTypeInfo.IOModeColor(outputInfo.Mode);
                node.outputContainer.Add(port);
            }
            


        }
    }
}
