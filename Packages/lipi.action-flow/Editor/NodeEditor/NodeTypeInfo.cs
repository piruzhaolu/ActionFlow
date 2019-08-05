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
            var av = (int)a >> 6;
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
            //if(typeof(NodeAsset<>) != type.BaseType.GetGenericTypeDefinition())
            //{
            //    throw new Exception("type需要是NodeAsset<>的子类");
            //}
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
            Input       = 0b_000001_000010,
            Output      = 0b_000010_000001,
            InputParm   = 0b_000100_001000,
            OutputParm  = 0b_001000_000100,
            BTInput     = 0b_010000_100000,
            BTOutput    = 0b_100000_010000,
            Mask        = 0b_000000_111111,
        }

        
        private static string IOModeToString(IOMode mode, string typeName = "")
        {
            switch (mode)
            {
                case IOMode.Output: return "output" + typeName;
                case IOMode.Input: return "input" + typeName;
                case IOMode.BTInput:return "BTin";
                case IOMode.BTOutput:return "BTout";
            }
            return string.Empty;
        }

        public static Color IOModeColor(IOMode mode)
        {
            switch (mode)
            {
                case IOMode.BTInput:
                case IOMode.BTOutput:
                    return new Color(0.46f, 0.72f, 0.85f);
                case IOMode.InputParm:
                case IOMode.OutputParm:
                    return new Color(0.7f, 0.85f, 0.7f);
                default:return Color.white;
            }
        }


       
        #endregion
        //======================================================

        public NodeTypeInfo(Type type)
        {
            _valueType = type;// type.GetField("Value").FieldType; //TODO：Del SO
            if (_valueType.IsSerializable == false)
            {
                throw new Exception($"{_valueType.Name} no add Serializable Attribute");
            }

            buildInputInfo();
            buildOutputInfo();
            buildFieldInfo();
            buildOutputParmInfo();
            buildBTInputInfo();
            buildBTOutputInfo();
        }

        public List<IOInfo> Inputs; //输入项列表
        public List<IOInfo> Outputs; //输出项列表
        public List<FieldInfo> FieldInfos; //所有field的信息，包括field上的输入参数和输出参数
        public List<IOInfo> OutputParm; // 以method为输出参数
        public List<IOInfo> BTInputs; //行为树输入
        public BTOutputInfo BTOutput; //行为树输出

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
                if (item.IsDefined(typeof(HideInGraphViewAttribute), true))
                {
                    continue;
                }

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
                int maxLink = -1;
                IOInfo btIOInfo = null;

                var attrs = item.GetCustomAttributes(typeof(NodeOutputBTAttribute), false);
                if (attrs != null && attrs.Length > 0  && attrs[0] is NodeOutputBTAttribute BTAttribute)
                {
                    maxLink = BTAttribute.MaxLink;
                    btIOInfo = new IOInfo()
                    {
                        ID = NodeLink.BTIDPre,
                        Mode = IOMode.BTOutput
                    };
                }

                var fileInfo = new FieldInfo()
                {
                    Path = $"{item.Name}",
                    FieldType = item.FieldType,
                    Name = item.Name,
                    MaxLink = maxLink,
                    IOInfo = ioInfo,
                    BT_IOInfo = btIOInfo
                };
                if (item.IsDefined(typeof(HideLabelInGraphViewAttribute), true)) fileInfo.Name = string.Empty;

                FieldInfos.Add(fileInfo);
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
                //var btArris = methods[i].GetCustomAttributes(typeof(NodeOutputBTAttribute), false);
                //for (int j = 0; j < btArris.Length; j++)
                //{
                //    if (j > 0) continue;
                //    Outputs.Add(new IOInfo()
                //    {
                //        Name = "",
                //        ID = NodeLink.BTIDPre,// TypeInfoHash(outputAttri.Type, IOMode.Output, outputAttri.ID),// outputAttri.ID,
                //        Mode = IOMode.BTOutput
                //    });
                //}
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


        private void buildBTInputInfo()
        {
            BTInputs = new List<IOInfo>();
            var methods = _valueType.GetMethods();
            foreach (var method in methods)
            {
                if (method.Name == "BehaviorInput")
                {
                    var info = new IOInfo();
                    info.Mode = IOMode.BTInput;
                    info.ID = NodeLink.BTIDPre;
                    BTInputs.Add(info);
                }
            }

        }

        private void buildBTOutputInfo()
        {
            BTOutput = null;

            //--------------Field在buildFieldInfo里处理
            //var fields = _valueType.GetFields();
            //foreach (var field in fields)
            //{
            //    var attri = field.GetCustomAttributes(typeof(NodeOutputBTAttribute), false);
            //    if (attri == null || attri.Length == 0) continue;
            //    if (attri[0] is NodeOutputBTAttribute outputBT)
            //    {
            //        var isArray = field.FieldType.IsArray ? 100 : 0;
            //        BTOutput = new BTOutputInfo()
            //        {
            //            MaxLink = outputBT.MaxLink,
            //            IOInfo = new IOInfo()
            //            {
            //                Mode = IOMode.BTOutput,
            //                ID = NodeLink.BTIDPre + isArray
            //            }
            //        };
            //    }
            //}

            var methods = _valueType.GetMethods();
            foreach (var method in methods)
            {
                var attri = method.GetCustomAttributes(typeof(NodeOutputBTAttribute), false);
                if (attri == null) continue;
                foreach (var item in attri)
                {
                    if (item is NodeOutputBTAttribute btAttri)
                    {
                        BTOutput = new BTOutputInfo()
                        {
                            MaxLink = 1,
                            IOInfo = new IOInfo()
                            {
                                Mode = IOMode.BTOutput,
                                ID = NodeLink.BTIDPre
                            }
                        };
                    }
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

       
        public class BTOutputInfo
        {
            public int MaxLink = 1;
            public IOInfo IOInfo;
        }

        public class FieldInfo
        {
            public string Path;
            public Type FieldType;
            public string Name;
            public int MaxLink = -1;
            public IOInfo IOInfo;
            public IOInfo BT_IOInfo;
        }



       

    }

}
