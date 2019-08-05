using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace ActionFlow
{
    public class EditorNodeCreator : ScriptableObject, ISearchWindowProvider
    {

        private static EditorNodeCreator _createor = null;
        public static EditorNodeCreator GetCreateor(ActionGraphView graphView)
        {
            if (_createor == null)
            {
                _createor = ScriptableObject.CreateInstance<EditorNodeCreator>();
                _createor.hideFlags = HideFlags.HideAndDontSave;
            }
            _createor._graphView = graphView;
            return _createor;
        }

        private ActionGraphView _graphView;

        private List<SearchTreeEntry> _searchTree = null;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (_searchTree == null)
            {
                _searchTree = new List<SearchTreeEntry>();
                _searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));
                GetAllNode(_searchTree);
            }
            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            // var so = ScriptableObject.CreateInstance(SearchTreeEntry)
            var type = (Type)SearchTreeEntry.userData;
            _graphView.CreatedHandler(type,  context.screenMousePosition);
            return true;
        }

        
        private class EntryItem
        {
            public string[] Menu;
            public Type Type;
        }


        private void GetAllNode(List<SearchTreeEntry> treeEntries)
        {
            List<EntryItem> entryItems = new List<EntryItem>() ;
            var attrs = UnityEditor.TypeCache.GetTypesWithAttribute<NodeInfoAttribute>();
            for (int i = 0; i < attrs.Count; i++)
            {
                var aType = attrs[i];
                var nodeInfo = (NodeInfoAttribute) Attribute.GetCustomAttribute(aType, typeof(NodeInfoAttribute));
                var menuName = nodeInfo.MenuName.Split('/');
                entryItems.Add(new EntryItem()
                {
                    Menu = menuName,
                    Type = aType
                });

            }

            entryItems.Sort(SortFn);

            List<string> currentPath = new List<string>();
            for (int i = 0; i < entryItems.Count; i++)
            {
                var item = entryItems[i];
                for (int j = 0; j < item.Menu.Length; j++)
                {
                    if (currentPath.Count <= j)
                    {
                        _addTreeEntry(j, item);

                    }
                    else
                    {
                        if (item.Menu[j] == currentPath[j]) continue;
                        else
                        {
                            currentPath.RemoveRange(j, currentPath.Count - j);
                            _addTreeEntry(j, item);
                        }
                    }
                }
            }

            //local function 
            void _addTreeEntry(int j, EntryItem item){
                if (j == item.Menu.Length - 1)
                {
                    _searchTree.Add(new SearchTreeEntry(new GUIContent(item.Menu[j], Texture2D.blackTexture))
                    {
                        level = j + 1,
                        userData = item.Type

                    });
                }
                else
                {
                    _searchTree.Add(new SearchTreeGroupEntry(new GUIContent(item.Menu[j]), j + 1));
                    currentPath.Add(item.Menu[j]);
                }
            }

        }



        private int SortFn(EntryItem x, EntryItem y)
        {
            var len = Math.Max(x.Menu.Length, y.Menu.Length);

            for (int i = 0; i < len; i++)
            {
                if (i == x.Menu.Length - 1 && i < y.Menu.Length - 1) return 1;
                if (i < x.Menu.Length - 1 && i == y.Menu.Length - 1) return -1;
                if (x.Menu.Length <= i) return 1;
                if (y.Menu.Length <= i) return -1;
                var xName = x.Menu[i];
                var yName = y.Menu[i];
                if (xName != yName)
                {
                    return xName.CompareTo(yName);
                }

            }
            return 0;
        }

    }
}



//if (j == item.Menu.Length - 1)
//{
//    _searchTree.Add(new SearchTreeEntry(new GUIContent(item.Menu[j], Texture2D.blackTexture)) {
//        level = j + 1,
//        userData = item.Type
//    });
//}
//else
//{
//    _searchTree.Add(new SearchTreeGroupEntry(new GUIContent(item.Menu[j]), j + 1));
//    currentPath.Add(item.Menu[j]);
//}

//if (j == item.Menu.Length - 1)
//{
//    _searchTree.Add(new SearchTreeEntry(new GUIContent(item.Menu[j], Texture2D.blackTexture)) {
//        level = j+1,
//        userData = item.Type

//    });
//} else
//{
//    _searchTree.Add(new SearchTreeGroupEntry(new GUIContent(item.Menu[j]), j+1));
//    currentPath.Add(item.Menu[j]);
//}


//private class TreeNode
//{
//    public TreeNode(string name)
//    {
//        Name = name;
//        Childs = new List<TreeNode>();
//    }
//    public string Name;
//    public List<TreeNode> Childs;
//    public Type Type;

//    public void Add(string[] path, Type type)
//    {
//        for (int i = 0; i < path.Length; i++)
//        {
//            var mName = path[i];
//            for (int j = 0; j < Childs.Count; j++)
//            {
//                if (Childs[j].Name == mName)
//                {
//                    var newPath = new string[path.Length - 1];
//                    Array.Copy(path, 1, newPath, 0, path.Length - 1);
//                    Add(path, type);
//                    break;
//                }
//            }
//        }
//    }
//}


//var allAssembly = AppDomain.CurrentDomain.GetAssemblies();
//for (int i = 0; i < allAssembly.Length; i++)
//{
//    Assembly assembly = allAssembly[i];
//    if (assembly != null)
//    {
//        Type[] types = assembly.GetTypes();
//        for (int j = 0; j < types.Length; j++)
//        {
//            if (!types[j].IsClass) continue;
//            NodeInfoAttribute nodeAttribute = (NodeInfoAttribute)Attribute.GetCustomAttribute(types[j], typeof(NodeInfoAttribute));
//            if (nodeAttribute != null)
//            {
//                var menuName = nodeAttribute.MenuName.Split('/');
//                entryItems.Add(new EntryItem()
//                {
//                    Menu = menuName,
//                    Type = types[j]
//                });
//            }

//        }

//    }
//}