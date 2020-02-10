
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace ActionFlow
{
    public class ActionNode:Node
    {
        public ActionNode(INode node, GraphNodeInfo nodeInfo, GraphNodeEditorInfo nodeEditorInfo, int index)
        {
            _nodeData = node;
            _nodeInfo = nodeInfo;
            _nodeEditorInfo = nodeEditorInfo;
            _index = index;

            SetTitle();
            InputDraw(node);
            OutputDraw(node);
        }

        private INode _nodeData;
        private GraphNodeInfo _nodeInfo;
        private GraphNodeEditorInfo _nodeEditorInfo;
        private int _index;


        private void SetTitle()
        {
            var info = _nodeData.GetType().GetCustomAttribute<NodeInfoAttribute>(false);
            title = info != null ? info.Name : string.Empty;
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
        }

    }
}