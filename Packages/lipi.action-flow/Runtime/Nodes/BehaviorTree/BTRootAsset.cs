using System;

namespace ActionFlow
{
    [Serializable]
    public class BTRoot : INode, IBehaviorNode, INodeInput
    {

        public void OnInput(ref Context context)
        {

        }



        public BehaviorStatus BehaviorInput(ref Context context)
        {

            return BehaviorStatus.Success;
        }


        public void Completed(ref Context context, int index)
        {
            
        }

        

    }


    [NodeInfo("BT/Root")]
    public class BTRootAsset : NodeAsset<BTRoot>
    {

    }
}
