using Unity.Entities;

namespace ActionFlow
{
    /// <summary>
    /// Node Sleeping 标记。
    /// </summary>
    public struct NodeSleeping: IBufferElementData
    {
        public Entity Entity;
        public ComponentType ComponentType;
        public int NodeIndex;
        


    }
}
