using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace ActionFlow
{
    public static class IOModeExt
    {
        public static bool Match(this NodeTypeInfo.IOMode a, NodeTypeInfo.IOMode b)
        {
            var av = (int)a >> 4;
            var bm = (int)b & (int)NodeTypeInfo.IOMode.Mask;
            if ((av & bm) == av) return true;
            else return false;

        }
    }
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
        public enum IOMode //前一半为标志位，后一半为匹配位
        {
            Input       = 0b_0001_0010,
            Output      = 0b_0010_0001,
            InputParm   = 0b_0100_1000,
            OutputParm  = 0b_1000_0100,
            Mask        = 0b_0000_1111,
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

        public static ulong TypeInfoHash(Type type, IOMode mode = IOMode.Input, int childID = 0)
        {
            var text = type.AssemblyQualifiedName;
            text = $"{text}_{IOModeToString(mode)}_{childID}";

            // Using http://www.isthe.com/chongo/tech/comp/fnv/index.html#FNV-1a
            // with basis and prime:
            const ulong offsetBasis = 14695981039346656037;
            const ulong prime = 1099511628211;

            ulong result = offsetBasis;
            foreach (var c in text)
            {
                result = prime * (result ^ (byte)(c & 255));
                result = prime * (result ^ (byte)(c >> 8));
            }
            return result;
        }


        #endregion
        //======================================================

        public NodeTypeInfo(Type type)
        {
            _valueType = type.GetField("Value").FieldType;
            if (_valueType.IsSerializable == false)
            {
                throw new Exception($"{_valueType.Name} no add Serializable Attribute");
            }

            buildInputInfo();
            buildOutputInfo();
            buildFieldInfo();
            buildOutputParmInfo();
            //buildInputParameterInfo(type);
        }

        public List<IOInfo> Inputs;
        public List<IOInfo> Outputs;
        public List<FieldInfo> FieldInfos;
        public List<IOInfo> OutputParm;

        private Type _valueType;


        private void buildOutputParmInfo()
        {
            OutputParm = new List<IOInfo>();
            var methods = _valueType.GetMethods();
            foreach (var item in methods)
            {
                var attrs = item.GetCustomAttributes(typeof(NodeOutputParmAttribute), false);
                var attr = (attrs == null || attrs.Length == 0) ? null : attrs[0];
                if (attr is NodeOutputParmAttribute a)
                {
                    OutputParm.Add(new IOInfo()
                    {
                        Name = a.Name,
                        Type = item.ReturnType,
                        ID = NodeLink.ParmIDPre + a.ID,
                        Mode = IOMode.OutputParm
                    });
                }
            }
        }


        private void buildFieldInfo()
        {
            FieldInfos = new List<FieldInfo>();
            var fields = _valueType.GetFields();
            foreach (var item in fields)
            {
                var attr = Get(item);
                IOInfo ioInfo = null;
                if (attr is NodeInputParmAttribute a)
                {
                    ioInfo = new IOInfo()
                    {
                        Type = item.FieldType,
                        ID = NodeLink.ParmIDPre + a.ID,
                        Mode = IOMode.InputParm,
                        Name = item.Name
                    };
                } else if(attr is NodeOutputParmAttribute b)
                {
                    ioInfo = new IOInfo()
                    {
                        Type = item.FieldType,
                        ID = NodeLink.ParmIDPre + b.ID,
                        Mode = IOMode.OutputParm,
                        Name = item.Name
                    };
                }

                FieldInfos.Add(new FieldInfo()
                {
                    Path = $"Value.{item.Name}",
                    FieldType = item.FieldType,
                    Name = item.Name,
                    IOInfo = ioInfo
                });
            }

            object Get(System.Reflection.FieldInfo item)
            {
                var attrs = item.GetCustomAttributes(typeof(NodeInputParmAttribute), false);
                var attr = (attrs != null && attrs.Length > 0) ? attrs[0] : null;
                if (attr != null) return attr;

                attrs = item.GetCustomAttributes(typeof(NodeOutputParmAttribute), false);
                attr = (attrs != null && attrs.Length > 0) ? attrs[0] : null;

                return attr;
            }

        }


        private void buildOutputInfo()
        {
            Outputs = new List<IOInfo>();
            
            var valueType = _valueType;

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
                        ID = outputAttri.ID,// TypeInfoHash(outputAttri.Type, IOMode.Output, outputAttri.ID),// outputAttri.ID,
                        Mode = IOMode.Output
                    });
                }
            }

        }


        private void buildInputInfo()
        {
            Inputs = new List<IOInfo>();
            var valueType = _valueType;
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
                        aInputInfo.ID = nodeAttri.ID;// TypeInfoHash(aInputInfo.Type, IOMode.Input, nodeAttri.ID);
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
                return Mode.Match(port.Mode);
            }
        }

       
        public class FieldInfo
        {
            public string Path;
            public Type FieldType;
            public string Name;
            public IOInfo IOInfo;
        }



       

    }

}
