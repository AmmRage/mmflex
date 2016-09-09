using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Morph;
using MMF.Utility;
using SlimDX;

namespace MMF.Bone
{
    class BestrowKinematicsProvider : ITransformUpdater
    {
        /// <summary>
        /// An array of bones
        /// </summary>
        private HierarchicalOrderCollection<PMXBone> bones;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bones">An array of bones</param>
        public BestrowKinematicsProvider(PMXBone[] bones)
        {
            this.bones =new HierarchicalOrderCollection<PMXBone>(bones,new BestrowKinematicsOrderSolver());
        }

        /// <summary>
        /// ITransformUpdaterThe implementation of the Member
        /// </summary>
        public bool UpdateTransform()
        {
            foreach (var pmxBone in this.bones)
            {
                if (pmxBone.isMoveProvided)
                {
                    var pp = this.bones[pmxBone.ProvideParentBone];
                    pmxBone.Translation += Vector3.Lerp(Vector3.Zero, pp.Translation, pmxBone.ProvidedRatio);
                }
                if (pmxBone.isRotateProvided)
                {
                    var pp = this.bones[pmxBone.ProvideParentBone];
                    pmxBone.Rotation *= Quaternion.Slerp(Quaternion.Identity, pp.Rotation, pmxBone.ProvidedRatio);
                }
            }
            return true;
        }

        private class BestrowKinematicsOrderSolver : HierarchicalOrderSolver<PMXBone>
        {
            public int getParentIndex(PMXBone child)
            {
                if (!child.isMoveProvided && child.isRotateProvided) return -1;
                return child.ProvideParentBone;
            }

            public int getIndex(PMXBone target)
            {
                return target.BoneIndex;
            }
        }
 
    }
}
