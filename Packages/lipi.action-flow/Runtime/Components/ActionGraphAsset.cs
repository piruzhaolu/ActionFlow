using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ActionFlow
{

    public struct ActionGraphAsset:ISharedComponentData, IEquatable<ActionGraphAsset>
    {
        public GraphAsset Asset;

        public bool Equals(ActionGraphAsset other)
        {
            return Asset == other.Asset;
        }

        public override int GetHashCode()
        {
            return Asset.GetHashCode();
        }
    }





}
