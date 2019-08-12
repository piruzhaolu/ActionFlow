using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ActionFlow
{

    public class ActionGraphView:GraphView
    {

        public ActionGraphView(GraphEditor window)
        {
            toolbar = new Toolbar();
            runningEntityMenu = new ToolbarMenu();
            runningEntityMenu.text = "None";
            //menu.menu.AppendAction("abc1", action, DropdownMenuAction.Status.Checked );
            //menu.menu.AppendAction("abc2", action, DropdownMenuAction.Status.Normal);

            toolbar.Add(runningEntityMenu);
            Add(toolbar);
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

        private void runningEntityMenuAction(DropdownMenuAction obj)
        {
            var v = obj.name;
            var arr = v.Split('-');
            var intV = Convert.ToInt32(arr[arr.Length - 1]);
            runningEntityMenu.text = obj.name;
            SelectedIndex = intV;
        }
        private DropdownMenuAction.Status runningEntityMenuStatusAction(DropdownMenuAction arg)
        {
            var v = arg.name;
            var arr = v.Split('-');
            var intV = Convert.ToInt32(arr[arr.Length - 1]);
            if (intV == SelectedIndex)
            {
                return DropdownMenuAction.Status.Checked;
            }else
            {
                return DropdownMenuAction.Status.Normal;
            }
        }




        private GraphEditor CurrentWindow;
        private Toolbar toolbar;
        private ToolbarMenu runningEntityMenu;//运行中的Entity
        private int SelectedIndex = 0;//当前选中的Entity索引

        public SerializedObject SerializedGraphAsset { private set; get; }
        private GraphAsset _GraphAsset;
        public GraphAsset GraphAsset { private set
            {
                SerializedGraphAsset = (value == null)?null:new SerializedObject(value);
                _GraphAsset = value;
            }
            get { return _GraphAsset; }
        }

        public void Show(GraphAsset graphAsset)
        {
            if (graphAsset != GraphAsset)
            {
                GraphAsset = graphAsset;
                graphElements.ForEach((element) => RemoveElement(element));

                for (int i = 0; i < GraphAsset.m_Nodes.Count; i++)// GraphAsset.Nodes.Count
                {
                    DrawNode(i);
                }
                for (int i = 0; i < GraphAsset.m_Nodes.Count; i++)
                {
                    DrawEdge(i);
                }
                this.UpdateViewTransform(graphAsset.ViewPosition, graphAsset.ViewScale);
            }
        }
        private int _version = -1;
        public void SetRunningEntity(List<RunningGraphAsset.Info> infos, int version)
        {
            if (_version == version) return;
            _version = version;
            var sIndex = -1 ;
            var count = (infos.Count >= 10) ? 10 : infos.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                //DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;
                if (SelectedIndex == infos[i].Index)
                {
                    //status = DropdownMenuAction.Status.Checked;
                    sIndex = SelectedIndex;
                }
                if (i == 0 && sIndex == -1)
                {
                    //status = DropdownMenuAction.Status.Checked;
                    sIndex = 0;
                    SelectedIndex = 0;
                }
                runningEntityMenu.menu.AppendAction($"{infos[i].Name}-{infos[i].Index}", runningEntityMenuAction, runningEntityMenuStatusAction);
            }
        }


        public List<(EditorActionNode, float)> ps = new List<(EditorActionNode, float)>();


        //在编辑器Playing中每帧调用，显示节点状态
        public void PlayingUpdata()
        {
            var list = this.Query<EditorActionNode>().ToList();
            var info = RunningGraphAsset.Instance.GetInfo(GraphAsset, SelectedIndex);
            if (info == null) return;
            var t = Time.realtimeSinceStartup;

            ps.Clear();
            foreach (var item in list)
            {
                var v = info.GetNodeCycle(item.Index);
                var inputTime = RunningGraphAsset.Instance.GetInputTime(GraphAsset,
                    new ActionStateIndex() { ChunkIndex = SelectedIndex, NodeIndex = item.Index });
                if (t - inputTime < 3f)
                {
                    ps.Add((item, inputTime));
                    //item.InputTween((t - inputTime));
                }

                if (v != NodeCycle.Inactive)
                {
                    item.Running = true;
                } else
                {
                    item.Running = false;
                }
            }
            ps.Sort(psSortFn);
            for (int i = 0; i < ps.Count; i++)
            {
                var tValue = t - (ps[i].Item2 + i * 0.1f);
                if (tValue >= 0)
                {
                    ps[i].Item1.InputTween(tValue);
                }
            }

        }

        private int psSortFn((EditorActionNode, float) x, (EditorActionNode, float) y)
        {
            if (x.Item2 < y.Item2) return -1;
            else return 1;
        }

        public void PlayingExit()
        {
            var list = this.Query<EditorActionNode>().ToList();
            foreach (var item in list)
            {
                item.Running = false;
                item.InputTween(1f);
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
            var asset = GraphAsset.m_Nodes[index];// GraphAsset.Nodes[index];
            if (asset == null) return null;
            var property = SerializedGraphAsset.FindProperty("m_Nodes").GetArrayElementAtIndex(index);
            var editorInfo = GraphAsset.NodeEditorInfo[index];
            var node = new EditorActionNode(property, GraphAsset.NodeInfo[index], editorInfo,index);
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
                var e3 = CreateEdge(cNode, link, NodeTypeInfo.IOMode.BTOutput, NodeTypeInfo.IOMode.BTInput);
                if (e3 != null) AddElement(e3);
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


            if (type.IsSubclassOf(typeof(ScriptableObject)))
            { // TODO:旧功能
                var index = GraphAsset.Add(ScriptableObject.CreateInstance(type), scaleNodePos);
                DrawNode(index);
            }
            else
            {
                var node = (INode) Activator.CreateInstance(type);
                Debug.Assert(node != null, "NodeInfo Attribute Can only be added to INode !");
                var index = GraphAsset.Add(node, scaleNodePos);
                SerializedGraphAsset.Update();
                DrawNode(index);
            }
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
            //var mainNode = reverse ? inputNode : outputNode;
            //mainNode.EdgeAddOrRemove();
        }


        private void nodeRemove(EditorActionNode node)
        {
            var index = node.Index;
            GraphAsset.m_Nodes[index] = null;
            //var so = GraphAsset.Nodes[index];
            //GraphAsset.Remove(so);
            //GraphAsset.Nodes[index] = null;

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
                EditorActionNode childNode = inputNode;
                bool reverse = false;
                if (outputInfo.Mode == NodeTypeInfo.IOMode.OutputParm)
                {
                    mainNode = inputNode;
                    childNode = outputNode;
                    reverse = true;
                }
                
                if (mainNode.NodeInfo.Childs == null) mainNode.NodeInfo.Childs = new List<NodeLink>();
                mainNode.NodeInfo.Childs.Add(new NodeLink()
                {
                    FromID = reverse?inputInfo.ID: outputInfo.ID,
                    Index = reverse?outputNode.Index:inputNode.Index,
                    ToID = reverse?outputInfo.ID: inputInfo.ID
                });
                childNode.NodeInfo.ParentIndex = mainNode.Index;

                mainNode.EdgeAddOrRemove();
            }
            
        }



    }
}
