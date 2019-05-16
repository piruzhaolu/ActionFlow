using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionFlow
{

    [Serializable]
    public class BTWait : INode, IBehaviorNode, ISleepable //StatusNodeBase<NullStatus>
    {
        public float Time;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            context.SetWakeTimerAndSleep(this, Time);
            return BehaviorStatus.Running;
        }



        public void Wake(ref Context context)
        {
            context.BehaviorRunningCompleted(BehaviorStatus.Success);
           
        }
    }


    [NodeInfo("BT/Wait")]
    public class BTWaitAsset :NodeAsset<BTWait>
    {

    }
}
