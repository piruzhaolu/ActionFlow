using Unity.Entities;

namespace ActionFlow
{
    public struct ActionRunState : IComponentData
    {
        public ActionStateData State;
    }
}
