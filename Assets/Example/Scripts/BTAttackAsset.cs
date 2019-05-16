using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    [System.Serializable]
    public class BTAttack : INode, IBehaviorNode
    {
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            return BehaviorStatus.Failure;
        }
    }


    [NodeInfo("Example/Attack")]
    public class BTAttackAsset : NodeAsset<BTAttack>
    {

        
    }

}
