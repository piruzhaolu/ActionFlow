using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace ActionFlow
{
    public class DefaltEditorNodeDraw : IEditorNodeDraw
    {
        private static DefaltEditorNodeDraw Defalt = new DefaltEditorNodeDraw();

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

            for (int i = 0; i < nodeTypeInfo.Inputs.Count; i++)
            {
                var inputInfo = nodeTypeInfo.Inputs[i];
                var portIn = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, null);
               // portIn.portColor = NodePortColor.Uint2color(attrItem.PortColor);
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
