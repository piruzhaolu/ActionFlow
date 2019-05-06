using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionFlow
{

    [Serializable]
    public class DelayNode : StatusNodeBase<NullStatus>, IBehaviorNode
    {
        public float Time;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            context.SetWakeTimerAndSleep(Time);
            return BehaviorStatus.Running;
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            throw new NotImplementedException();
        }

        public override void OnTick(ref Context context)
        {
            context.BehaviorRunningCompleted(BehaviorStatus.Success);
            context.Inactive();
        }
    }


    [NodeInfo("Delay")]
    public class DelayNodeAsset:NodeAsset<DelayNode>
    {

    }
}
