using Unity.Entities;

namespace ActionFlow
{
    public struct GameTime : IComponentData
    {


        public float DeltaTime;

        public float Time;

        public int FrameNumber;
    }
}