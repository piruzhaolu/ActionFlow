using System;

namespace ActionFlow
{
    

    public interface INodePort
    {
        bool Match(INodePort port);
    }
}
