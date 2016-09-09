using System.Collections.Generic;

namespace MMF.Bone
{
    /// <summary>
    ///     Class for sorting the bones
    /// </summary>
    internal class BoneComparer : IComparer<PMXBone>
    {
        public BoneComparer(int boneCount)
        {
            this.BoneCount = boneCount;
        }

        public int BoneCount { get; private set; }

        #region IComparer<Bone> メンバー

        /// <summary>
        ///     Bone calculating sort order
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(PMXBone x, PMXBone y)
        {
            //Once the score increases to calculate
            int xScore = 0;
            int yScore = 0;
            if (x.PhysicsOrder == PhysicsOrder.After)
            {
                xScore += this.BoneCount*this.BoneCount;
            }
            if (y.PhysicsOrder == PhysicsOrder.After)
            {
                yScore += this.BoneCount*this.BoneCount;
            }
            xScore += this.BoneCount*x.Layer;
            yScore += this.BoneCount*y.Layer;
            xScore += x.BoneIndex;
            yScore += y.BoneIndex;
            return xScore - yScore;
        }

        #endregion
    }
}