using Unity.Entities;

namespace ActionFlow
{
    public struct ActionRunState : ISystemStateComponentData
    {
        public int InstanceID;
        public int Index;
        //public ActionStateData State;
    }
}
