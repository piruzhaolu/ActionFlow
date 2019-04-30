using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionFlow
{

    public class ActionGraphView:GraphView
    {

        public ActionGraphView(GraphEditor window)
        {
            this.AddManipulator(new ContentZoomer()
            {
                minScale = 0.5f,
                maxScale = 1.5f
            });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.StretchToParentSize();
            CurrentWindow = window;


            styleSheets.Add(Resources.Load<StyleSheet>("ActionFlowStyle"));

           // this.AddElement(new EditorActionNode());

            nodeCreationRequest += creationRequestHandler;
            graphViewChanged += graphViewChangedHandler;
            viewTransformChanged += viewTransformChangedHandler;
        }

        

        private GraphEditor CurrentWindow;

        public GraphAsset GraphAsset { private set; get; }
       

        public void Show(GraphAsset graphAsset)
        {
            if (graphAsset != GraphAsset)
            {
                GraphAsset = graphAsset;
                graphElements.ForEach((element) => RemoveElement(element));

                for (int i = 0; i < GraphAsset.Nodes.Count; i++)
                {
                    DrawNode(i);
                }
                for (int i = 0; i < GraphAsset.Nodes.Count; i++)
                {
                    DrawEdge(i);
                }
                this.UpdateViewTransform(graphAsset.ViewPosition, graphAsset.ViewScale);
            }
        }

       

        public EditorActionNode GetNode(int index)
        {
            var list = this.Query<EditorActionNode>().ToList();
            foreach (var item in list)
            {
                if (item.Index == index) return item;
            }
            return null;
        }

       

        private EditorActionNode DrawNode(int index)
        {
            var asset = GraphAsset.Nodes[index];
            if (asset == null) return null;
            var editorInfo = GraphAsset.NodeEditorInfo[index];
            var node = new EditorActionNode(asset, GraphAsset.NodeInfo[index], editorInfo,index);
            node.SetPosition(new Rect(editorInfo.Pos, editorInfo.Pos));
            AddElement(node);
            return node;
        }

        private void DrawEdge(int index)
        {
            var nodeInfo = GraphAsset.NodeInfo[index];
            var cNode = GetNode(index);
            if (nodeInfo.Childs == null) return;
            foreach (var link in nodeInfo.Childs)
            {
                var e1 = CreateEdge(cNode, link, NodeTypeInfo.IOMode.Output, NodeTypeInfo.IOMode.Input);
                if (e1 != null) AddElement(e1);
                var e2 = CreateEdge(cNode, link, NodeTypeInfo.IOMode.InputParm, NodeTypeInfo.IOMode.OutputParm);
                if (e2 != null) AddElement(e2);
            }
        }

        private Edge CreateEdge(EditorActionNode cNode, NodeLink link, NodeTypeInfo.IOMode a, NodeTypeInfo.IOMode b)
        {
            Port port1 = cNode?.GetPort(link.FromID, a);
            var tNode = GetNode(link.Index);
            Port port2 = tNode?.GetPort(link.ToID, b);
            if (port1 != null && port2 != null)
            {
                var edge = port1.ConnectTo(port2);
                return edge;
            }
            return null;
        }




        public void CreatedHandler(Type type, Vector2 pos)
        {
            var nodePos = pos - CurrentWindow.position.min - contentViewContainer.worldBound.position;
            var scaleNodePos = new Vector2(nodePos.x / viewTransform.scale.x, nodePos.y / viewTransform.scale.y);
            var index = GraphAsset.Add(ScriptableObject.CreateInstance(type), scaleNodePos);

            DrawNode(index);
        }



        private void creationRequestHandler(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), EditorNodeCreator.GetCreateor(this));
        }

        private void viewTransformChangedHandler(GraphView graphView)
        {
            if (GraphAsset != null) {
                GraphAsset.ViewPosition = graphView.viewTransform.position;
                GraphAsset.ViewScale = graphView.viewTransform.scale;
            }

        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var li = new List<Port>();
            ports.ForEach(delegate (Port port)
            {
                if (port.direction != startPort.direction && port.node != startPort.node)
                {
                    var _port = port.source as NodeTypeInfo.IOInfo;
                    var _startPort = startPort.source as NodeTypeInfo.IOInfo;
                    if (_startPort.Match(_port))
                    {
                        li.Add(port);
                    }

                }
            });
            return li;
            //return new List<Port>() { pp };
        }


        private GraphViewChange graphViewChangedHandler(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                edgesToCreate(graphViewChange.edgesToCreate);
            }
            if (graphViewChange.elementsToRemove != null)
            {
                var list = graphViewChange.elementsToRemove;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Edge) edgeRemove((Edge)list[i]);
                    else if (list[i] is EditorActionNode) nodeRemove((EditorActionNode)list[i]);
                }
                
            }
            if (graphViewChange.movedElements != null)
            {
                var list = graphViewChange.movedElements;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is EditorActionNode) nodeMoved((EditorActionNode)list[i], graphViewChange.moveDelta);
                }
            }

            EditorUtility.SetDirty(GraphAsset);
            return graphViewChange;
        }


        private void nodeMoved(EditorActionNode node, Vector2 moveDelta)
        {
            var index = node.Index;
            var item = GraphAsset.NodeEditorInfo[index];
            item.Pos = node.layout.position;
            GraphAsset.NodeEditorInfo[index] = item;
           // Debug.Log($"OOO==={node.worldBound.position}; ddd -- {item.Pos }");
        }


        private void edgeRemove(Edge edge)
        {
            var inputInfo = edge.input.source as NodeTypeInfo.IOInfo;
            var outputInfo = edge.output.source as NodeTypeInfo.IOInfo;

            var inputNode = edge.input.node as EditorActionNode;
            var outputNode = edge.output.node as EditorActionNode;

            bool reverse = false; //对于参数类连接port，input outpu和实际逻辑相反

            if (outputInfo.Mode == NodeTypeInfo.IOMode.OutputParm)
            {
                reverse = true;
            }
            var nodeInfo = reverse? inputNode.NodeInfo: outputNode.NodeInfo;
            for (int i = 0; i < nodeInfo.Childs.Count; i++)
            {
                var m_id = reverse ? inputInfo.ID : outputInfo.ID;
                if (nodeInfo.Childs[i].FromID == m_id)
                {
                    nodeInfo.Childs.RemoveAt(i);
                    break;
                }
            }
        }


        private void nodeRemove(EditorActionNode node)
        {
            var index = node.Index;
            var so = GraphAsset.Nodes[index];
            GraphAsset.Remove(so);
            GraphAsset.Nodes[index] = null;

        }


        private void edgesToCreate(List<Edge> lists)
        {
            for (int i = 0; i < lists.Count; i++)
            {
                var inputNode = lists[i].input.node as EditorActionNode;
                var outputNode = lists[i].output.node as EditorActionNode;

                var inputInfo = lists[i].input.source as NodeTypeInfo.IOInfo;
                var outputInfo = lists[i].output.source as NodeTypeInfo.IOInfo;

                EditorActionNode mainNode = outputNode;
                bool reverse = false;
                if (outputInfo.Mode == NodeTypeInfo.IOMode.OutputParm)
                {
                    mainNode = inputNode;
                    reverse = true;
                }
                
                if (mainNode.NodeInfo.Childs == null) mainNode.NodeInfo.Childs = new List<NodeLink>();
                mainNode.NodeInfo.Childs.Add(new NodeLink()
                {
                    FromID = reverse?inputInfo.ID: outputInfo.ID,
                    Index = reverse?outputNode.Index:inputNode.Index,
                    ToID = reverse?outputInfo.ID: inputInfo.ID
                });
            }
            
        }



    }
}
