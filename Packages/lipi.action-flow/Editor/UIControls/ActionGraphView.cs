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
            
            
            var inst = new VisualElement();
            inst.style.width = 100;
            inst.style.left = 0;
            Add(inst);


            this.AddManipulator(new ContentZoomer()
            {
                minScale = 0.5f,
                maxScale = 1.5f
            });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.StretchToParentSize();
            _currentWindow = window;


            styleSheets.Add(Resources.Load<StyleSheet>("ActionFlowStyle"));


            nodeCreationRequest += creationRequestHandler;
            graphViewChanged += graphViewChangedHandler;
            viewTransformChanged += viewTransformChangedHandler;
        }

        private void RunningEntityMenuAction(DropdownMenuAction obj)
        {
            var v = obj.name;
            var arr = v.Split('-');
            var intV = Convert.ToInt32(arr[arr.Length - 1]);
            //_runningEntityMenu.text = obj.name;
            _selectedIndex = intV;
        }
        private DropdownMenuAction.Status RunningEntityMenuStatusAction(DropdownMenuAction arg)
        {
            var v = arg.name;
            var arr = v.Split('-');
            var intV = Convert.ToInt32(arr[arr.Length - 1]);
            return intV == _selectedIndex ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }




        private readonly GraphEditor _currentWindow;
        
        
        private int _selectedIndex = 0;//当前选中的Entity索引

        private SerializedObject SerializedGraphAsset { set; get; }
        private GraphAsset _graphAsset;

        private GraphAsset GraphAsset {
            set
            {
                SerializedGraphAsset = (value == null)?null:new SerializedObject(value);
                _graphAsset = value;
            }
            get => _graphAsset;
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
                if (_selectedIndex == infos[i].Index)
                {
                    //status = DropdownMenuAction.Status.Checked;
                    sIndex = _selectedIndex;
                }
                if (i == 0 && sIndex == -1)
                {
                    //status = DropdownMenuAction.Status.Checked;
                    sIndex = 0;
                    _selectedIndex = 0;
                }
               // _runningEntityMenu.menu.AppendAction($"{infos[i].Name}-{infos[i].Index}", RunningEntityMenuAction, RunningEntityMenuStatusAction);
            }
        }


        public readonly List<(ActionNode, float)> Ps = new List<(ActionNode, float)>();


        //在编辑器Playing中每帧调用，显示节点状态
        public void PlayingUpdata()
        {
            var list = this.Query<ActionNode>().ToList();
            var info = RunningGraphAsset.Instance.GetInfo(GraphAsset, _selectedIndex);
            if (info == null) return;
            var t = Time.realtimeSinceStartup;

            Ps.Clear();
            foreach (var item in list)
            {
                var v = info.GetNodeCycle(item.Index);
                var inputTime = RunningGraphAsset.Instance.GetInputTime(GraphAsset,
                    new ActionStateIndex() { ChunkIndex = _selectedIndex, NodeIndex = item.Index });
                if (t - inputTime < 3f)
                {
                    Ps.Add((item, inputTime));
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
            Ps.Sort(psSortFn);
            for (int i = 0; i < Ps.Count; i++)
            {
                var tValue = t - (Ps[i].Item2 + i * 0.1f);
                if (tValue >= 0)
                {
                    Ps[i].Item1.InputTween(tValue);
                }
            }

        }

        private int psSortFn((ActionNode, float) x, (ActionNode, float) y)
        {
            if (x.Item2 < y.Item2) return -1;
            else return 1;
        }

        public void PlayingExit()
        {
            var list = this.Query<ActionNode>().ToList();
            foreach (var item in list)
            {
                item.Running = false;
                item.InputTween(1f);
            }
        }


        private ActionNode GetNode(int index)
        {
            var list = this.Query<ActionNode>().ToList();
            foreach (var item in list)
            {
                if (item.Index == index) return item;
            }
            return null;
        }

       

        private ActionNode DrawNode(int index)
        {
            var asset = GraphAsset.m_Nodes[index];// GraphAsset.Nodes[index];
            if (asset == null) return null;
            //var property = SerializedGraphAsset.FindProperty("m_Nodes").GetArrayElementAtIndex(index);
            var editorInfo = GraphAsset.NodeEditorInfo[index];
            var node = new ActionNode(asset, GraphAsset.NodeInfo[index], editorInfo,index);
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

        private Edge CreateEdge(ActionNode cNode, NodeLink link, NodeTypeInfo.IOMode a, NodeTypeInfo.IOMode b)
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
            var nodePos = pos - _currentWindow.position.min - contentViewContainer.worldBound.position;
            var scaleNodePos = new Vector2(nodePos.x / viewTransform.scale.x, nodePos.y / viewTransform.scale.y);


            if (type.IsSubclassOf(typeof(ScriptableObject)))
            { // TODO:旧功能
                //var index = GraphAsset.Add(ScriptableObject.CreateInstance(type), scaleNodePos);
                //DrawNode(index);
                throw new Exception("已不能添加此类型");
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
                    var mport = port.source as NodeTypeInfo.IOInfo;
                    if (startPort.source is NodeTypeInfo.IOInfo mstartPort && mstartPort.Match(mport))
                    {
                        li.Add(port);
                    }

                }
            });
            return li;
        }


        private GraphViewChange graphViewChangedHandler(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                EdgesToCreate(graphViewChange.edgesToCreate);
            }
            if (graphViewChange.elementsToRemove != null)
            {
                var list = graphViewChange.elementsToRemove;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is Edge) EdgeRemove((Edge)list[i]);
                    else if (list[i] is ActionNode) NodeRemove((ActionNode)list[i]);
                }
                
            }
            if (graphViewChange.movedElements != null)
            {
                var list = graphViewChange.movedElements;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is ActionNode) NodeMoved((ActionNode)list[i], graphViewChange.moveDelta);
                }
            }

            EditorUtility.SetDirty(GraphAsset);
            return graphViewChange;
        }


        private void NodeMoved(ActionNode node, Vector2 moveDelta)
        {
            var index = node.Index;
            var item = GraphAsset.NodeEditorInfo[index];
            item.Pos = node.layout.position;
            GraphAsset.NodeEditorInfo[index] = item;
        }


        private void EdgeRemove(Edge edge)
        {
            var inputInfo = edge.input.source as NodeTypeInfo.IOInfo;
            var outputInfo = edge.output.source as NodeTypeInfo.IOInfo;

            var inputNode = edge.input.node as ActionNode;
            var outputNode = edge.output.node as ActionNode;
            
            if (inputNode == null || outputNode==null) return;
            if (inputInfo == null || outputInfo == null) return;

            var reverse = outputInfo.Mode == NodeTypeInfo.IOMode.OutputParm; //对于参数类连接port，input outpu和实际逻辑相反

            var nodeInfo = reverse? inputNode.NodeInfo: outputNode.NodeInfo;
            for (int i = 0; i < nodeInfo.Childs.Count; i++)
            {
                var mId = reverse ? inputInfo.ID : outputInfo.ID;
                if (nodeInfo.Childs[i].FromID == mId)
                {
                    nodeInfo.Childs.RemoveAt(i);
                    
                    break;
                }
            }
        }


        private void NodeRemove(ActionNode node)
        {
            var index = node.Index;
            GraphAsset.m_Nodes[index] = null;
            //var so = GraphAsset.Nodes[index];
            //GraphAsset.Remove(so);
            //GraphAsset.Nodes[index] = null;

        }


        private void EdgesToCreate(List<Edge> lists)
        {
            for (var i = 0; i < lists.Count; i++)
            {
                var inputNode = lists[i].input.node as ActionNode;
                var outputNode = lists[i].output.node as ActionNode;

                var inputInfo = lists[i].input.source as NodeTypeInfo.IOInfo;
                var outputInfo = lists[i].output.source as NodeTypeInfo.IOInfo;

                var mainNode = outputNode;
                var childNode = inputNode;
                
                if (mainNode == null || childNode==null) continue;
                if (inputInfo == null || outputInfo == null) continue;
                
                var reverse = false;
                if ( outputInfo.Mode == NodeTypeInfo.IOMode.OutputParm)
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
