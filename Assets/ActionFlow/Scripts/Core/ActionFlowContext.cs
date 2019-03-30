using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ActionFlow
{
    public struct ActionFlowContext
    {
        public Entity CurrEntity { get; set; }

        public Entity TargetEntity { get; set; }

        public EntityManager EM { get; set; }

        public ActionFlowStateData StateData { get; set; }

        public int Index { get; set; }

        public ActionFlowGraphAsset Asset { get; set; }

        public GameTime GameTime { get; set; }



        
    }
}
