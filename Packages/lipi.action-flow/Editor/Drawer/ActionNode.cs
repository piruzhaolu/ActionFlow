
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionFlow
{
    public class ActionNode:Node
    {
        public ActionNode(INode node, GraphNodeInfo nodeInfo, GraphNodeEditorInfo nodeEditorInfo, int index)
        {
            _nodeData = node;
            NodeInfo = nodeInfo;
            _nodeEditorInfo = nodeEditorInfo;
            Index = index;

            SetTitle();
            InputDraw(node);
            OutputDraw(node);
        }

        private readonly INode _nodeData;
        private GraphNodeEditorInfo _nodeEditorInfo;

        public GraphNodeInfo NodeInfo { get; }

        public INode NodeData => _nodeData;

        public int Index { get; }
        
        public void EdgeAddOrRemove()
        {
        }

        private void SetTitle()
        {
            var info = _nodeData.GetType().GetCustomAttribute<NodeInfoAttribute>(false);
            title = info != null ? info.Name : string.Empty;
        }

        public bool Running;
        
        public void InputTween(float t)
        {
            
        }

        
        private void InputDraw(INode asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetType());
            var list = new List<NodeTypeInfo.IOInfo>();
            list.AddRange(nodeTypeInfo.Inputs);
            list.AddRange(nodeTypeInfo.BTInputs);

            foreach (var inputInfo in list)
            {
                var capacity = inputInfo.Mode == NodeTypeInfo.IOMode.BTInput? Port.Capacity.Single:Port.Capacity.Multi;
                var portIn = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, null);
                portIn.portColor = NodeTypeInfo.IOModeColor(inputInfo.Mode);
                portIn.portName = inputInfo.GetName();
                portIn.source = inputInfo;
                inputContainer.Add(portIn);
            }
        }
        
        private void OutputDraw(INode asset)
        {
            var nodeTypeInfo = NodeTypeInfo.GetNodeTypeInfo(asset.GetType());
            foreach (var outputInfo in nodeTypeInfo.Outputs)
            {
                var capacity = outputInfo.Mode == NodeTypeInfo.IOMode.BTOutput ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, null);
                port.portName = outputInfo.GetName();
                port.source = outputInfo;
                port.portColor = NodeTypeInfo.IOModeColor(outputInfo.Mode);
                outputContainer.Add(port);
            }

            if (nodeTypeInfo.BTOutput != null)
            {
                var count = Math.Min(nodeTypeInfo.BTOutput.MaxLink, 20);
                for (var i = 0; i < count; i++)
                {
                    var capacity = Port.Capacity.Single;
                    var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, null);
                    if (count > 1)
                    {
                        port.AddToClassList("small-port");
                    }
                    
                    port.portName = i.ToString();
                    var newIOInfo = nodeTypeInfo.BTOutput.IOInfo.Clone();
                    newIOInfo.ID += i;
                    port.source = newIOInfo;
                    port.portColor = NodeTypeInfo.IOModeColor(NodeTypeInfo.IOMode.BTOutput);
                    
                    outputContainer.Add(port);
                }
            }
        }
        
        
        public Port GetPort(int id, NodeTypeInfo.IOMode mode)
        {
            var list = this.Query<Port>().ToList();
            foreach (var item in list)
            {
                var info = (NodeTypeInfo.IOInfo)item.source;
                if (info != null && info.ID == id && info.Mode == mode)
                {
                    return item;
                }

            }
            return null;
        }

    }
}