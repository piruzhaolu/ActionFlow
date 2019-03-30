using System;

namespace ActionFlow
{
    public enum PortMode
    {
        Process,
        Parameter,
        BT
    }

    public interface INodePort
    {
        int GetID();
        Type GetDataType();
        PortMode GetPortMode();
    }
}
