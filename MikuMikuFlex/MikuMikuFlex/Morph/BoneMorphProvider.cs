using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MMF.Bone;
using MMF.Model;
using MMF.Model.PMX;
using MMF.Motion;
using SlimDX;

namespace MMF.Morph
{
    internal class BoneMorphProvider : IMorphProvider
    {
        public Dictionary<string, BoneMorphData> MorphList = new Dictionary<string, BoneMorphData>();
        private ISkinningProvider skinningProvider;

        public BoneMorphProvider(PMXModel model)
        {
            this.skinningProvider = model.Skinning;
            foreach (MorphData morphData in model.Model.MorphList.Morphes)
            {
                if (morphData.type == MorphType.Bone)
                {
                    this.MorphList.Add(morphData.MorphName, new BoneMorphData(morphData));
                }
            }
        }

        public void ApplyMorphProgress(float frameNumber, IEnumerable<MorphMotion> morphMotions)
        {
            foreach (var morphMotion in morphMotions)
            {
                SetMorphProgress(morphMotion.GetMorphValue(frameNumber), morphMotion.MorphName);
            }
        }

        public bool ApplyMorphProgress(float progress, string morphName)
        {
            return SetMorphProgress(progress, morphName);
        }

        public void UpdateFrame()
        {
            
        }

        private bool SetMorphProgress(float progress, string morphName)
        {
            if (!this.MorphList.ContainsKey(morphName)) return false;
            BoneMorphData data = this.MorphList[morphName];
            foreach (BoneMorphOffset boneMorphOffset in data.BoneMorphs)
            {
                Quaternion rot=new Quaternion(boneMorphOffset.QuantityOfRotating.X,boneMorphOffset.QuantityOfRotating.Y,boneMorphOffset.QuantityOfRotating.Z,boneMorphOffset.QuantityOfRotating.W);
                this.skinningProvider.Bone[boneMorphOffset.BoneIndex].Rotation *= rot;
                this.skinningProvider.Bone[boneMorphOffset.BoneIndex].Translation += boneMorphOffset.QuantityOfMoving;
            }
            return true;
        }
    }
}