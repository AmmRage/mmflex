using System;
using MMF.Utility;
using SlimDX;

namespace MMF.Bone
{
    /// <summary>
    ///     IKLinks to related data
    /// </summary>
    public class IkLink
    {
        private readonly int index;
        private readonly ISkinningProvider skinning;

        /// <summary>
        ///     Whether or not the rotation quantity limitations
        /// </summary>
        public bool isLimited = false;

        /// <summary>
        ///     The maximum amount of rotation (X,Y,Z)
        /// </summary>
        public Vector3 maxRot;

        /// <summary>
        ///     The minimum amount of rotation (X,Y,Z)
        /// </summary>
        public Vector3 minRot;

        public int loopCount;

        public IkLink(ISkinningProvider skinning,MMDFileParser.PMXModelParser.IkLinkData linkData)
        {
            this.skinning = skinning;
            this.index = linkData.LinkBoneIndex;
            Vector3 maxVec = linkData.MinimumRadian;
            Vector3 minVec = linkData.MaximumRadian;
            //min and max load correctly
            this.minRot = new Vector3(Math.Min(maxVec.X, minVec.X), Math.Min(maxVec.Y, minVec.Y),
                Math.Min(maxVec.Z, minVec.Z));
            this.maxRot = new Vector3(Math.Max(maxVec.X, minVec.X), Math.Max(maxVec.Y, minVec.Y),
                Math.Max(maxVec.Z, minVec.Z));
            this.maxRot = Vector3.Clamp(this.maxRot, CGHelper.EularMinimum, CGHelper.EularMaximum);
            this.minRot = Vector3.Clamp(this.minRot, CGHelper.EularMinimum, CGHelper.EularMaximum);
            this.isLimited = linkData.isRotateLimited;
        }

        /// <summary>
        ///     ikLink born
        /// </summary>
        public PMXBone ikLinkBone
        {
            get { return this.skinning.Bone[this.index]; }
        }
    }
}