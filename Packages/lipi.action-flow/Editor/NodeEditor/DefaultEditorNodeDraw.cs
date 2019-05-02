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



        public virtual void Create(EditorActionNode node, ScriptableObject asset)
        {
          //  node.Q(name: "selection-border").Add(new Label("aaa"));
            InputDraw(node, asset);
            OutputDraw(node, asset);
            ExtensionDraw(node, asset);
            node.RefreshExpandedState();
        }

        public virtual void DoubleClick(EditorActionNode node, ScriptableObject asset)
        {
        }



        public virtual void ExtensionDraw(EditorActionNode node, ScriptableObject asset)
        {
            var elem = new EditorNodeElement(asset, node);
            node.mainContainer.Add(elem);
        }


        public virtual void InputDraw(EditorActionNode node, ScriptableObject asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetType());
            List<NodeTypeInfo.IOInfo> list = new List<NodeTypeInfo.IOInfo>();
            list.AddRange(nodeTypeInfo.Inputs);
            list.AddRange(nodeTypeInfo.BTInputs);

            foreach (var inputInfo in list)
            {
                Port.Capacity capacity = (inputInfo.Mode == NodeTypeInfo.IOMode.BTInput) ? Port.Capacity.Single : Port.Capacity.Multi;
                var portIn = node.InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, null);
                portIn.portColor = NodeTypeInfo.IOModeColor(inputInfo.Mode);
                portIn.portName = inputInfo.GetName();
                portIn.source = inputInfo;
                node.inputContainer.Add(portIn);
            }

        }

        public virtual void OutputDraw(EditorActionNode node, ScriptableObject asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetType());
            for (int i = 0; i < nodeTypeInfo.Outputs.Count; i++)
            {
                var outputInfo = nodeTypeInfo.Outputs[i];

                var port = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, null);
                port.portName = outputInfo.GetName();
                port.source = outputInfo;
                node.outputContainer.Add(port);
            }
            


        }
    }
}
