using System;
using System.Collections.Generic;
using MMF.Model;
using MMF.Morph;
using SlimDX.Direct3D11;

namespace MMF.Bone
{
    public interface ISkinningProvider : IDisposable
    {
        PMXBone[] Bone { get; }

        Dictionary<string,PMXBone> BoneDictionary { get; }
        
        List<PMXBone> IkBone { get; }

        List<ITransformUpdater> KinematicsProviders { get; }

        /// <summary>
        ///     Call to apply effects to motion data。
        /// </summary>
        /// <param name="effect"></param>
        void ApplyEffect(Effect effect);

        /// <summary>
        ///     Call to update the skinning data per frame。
        /// </summary>
        /// <param name="morphManager"></param>
        void UpdateSkinning(IMorphManager morphManager);

        /// <summary>
        ///     Resets all the rotating joint。
        /// </summary>
        void ResetAllBoneTransform();

        event EventHandler SkeletonUpdated;
    }
}