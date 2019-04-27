using UnityEngine;
using System.Collections.Generic;
using System;

namespace ActionFlow
{

    public class NodeTypeInfo
    {

        #region static
        private static Dictionary<Type, NodeTypeInfo> _typeInfos;

        public static NodeTypeInfo GetNodeTypeInfo(Type type)
        {
            if(typeof(NodeAsset<>) != type.BaseType.GetGenericTypeDefinition())
            {
                throw new Exception("type需要是NodeAsset<>的子类");
            }
            Debug.Log(Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<ActionStateData>());
            if (_typeInfos == null) _typeInfos = new Dictionary<Type, NodeTypeInfo>();
            if (_typeInfos.TryGetValue(type, out var info))
            {
                return info;
            }
            info = new NodeTypeInfo(type);
            _typeInfos.Add(type,info);
            return info;
        }

        public static string TypeToString(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";

            return type.ToString();
        }
        public enum IOMode
        {
            Input,
            Output
        }

        private static string IOModeToString(IOMode mode, string typeName = "")
        {
            switch (mode)
            {
                case IOMode.Output: return "output" + typeName;
                case IOMode.Input: return "input" + typeName;
            }
            return string.Empty;
        }

        #endregion


        public NodeTypeInfo(Type type)
        {
            buildInputInfo(type);
            buildOutputInfo(type);
        }

        public List<IOInfo> Inputs;
        public List<IOInfo> Outputs;


        private void buildOutputInfo(Type type)
        {
            Outputs = new List<IOInfo>();
            
            var valueType = type.GetField("Value").FieldType;

            var methods = valueType.GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                var arris = methods[i].GetCustomAttributes(typeof(NodeOutputAttribute),false);
                for (int j = 0; j < arris.Length; j++)
                {
                    var outputAttri = (NodeOutputAttribute)arris[j];
                    Outputs.Add(new IOInfo()
                    {
                        Name = outputAttri.Name,
                        Type = outputAttri.Type,
                        ID = outputAttri.ID,
                        Mode = IOMode.Output
                    });
                }
            }

        }


        private void buildInputInfo(Type type)
        {
            Inputs = new List<IOInfo>();
            var valueType = type.GetField("Value").FieldType;
            var methods = valueType.GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == "OnInput")
                {
                    var parameters = methods[i].GetParameters();
                    var aInputInfo = new IOInfo();
                    aInputInfo.Mode = IOMode.Input;
                    if (parameters.Length > 1)
                    {
                        aInputInfo.Type = parameters[1].ParameterType;
                    }
                    var attri = methods[i].GetCustomAttributes(typeof(NodeInputAttribute), false);
                    if (attri != null && attri.Length > 0)
                    {
                        var nodeAttri = (NodeInputAttribute)attri[0];
                        aInputInfo.ID = nodeAttri.ID;
                        if (nodeAttri.Name != string.Empty) aInputInfo.Name = nodeAttri.Name;
                    }

                    Inputs.Add(aInputInfo);
                }
            }
        }


        public class IOInfo
        {
            public Type Type;
            public int ID;
            public string Name = string.Empty;

            public IOMode Mode;

            public string GetName()
            {
                if (Name != string.Empty) return Name;
                if (Type == null) return IOModeToString(Mode);
                else return IOModeToString(Mode, $" ({TypeToString(Type)})");
            }

            public bool Match(IOInfo port)
            {
                if (Mode == IOMode.Input && port.Mode == IOMode.Output || 
                    (Mode == IOMode.Output && port.Mode == IOMode.Input))
                {
                    if (port.Type == Type) { return true; }
                }
                return false;
            }
        }

       


        //public class InputInfo: INodePort
        //{
        //    public Type Type;
        //    public int ID = 0;
        //    public string Name = string.Empty;

        //    public string GetName()
        //    {
        //        if (Name != string.Empty) return Name;
        //        if (Type == null) return "Input";
        //        else return $"Input ({TypeToString(Type)})";
        //    }

        //    public bool Match(INodePort port)
        //    {
        //        if (port is OutputInfo)
        //        {
        //            var info = port as OutputInfo;
        //            if (info.Type == Type) { return true; }
        //        }
        //        return false;
        //    }
        //}


    }

}
