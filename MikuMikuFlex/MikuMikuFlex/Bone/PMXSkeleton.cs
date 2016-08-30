using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMF.Model;
using MMF.Morph;
using SlimDX;
using SlimDX.Direct3D11;
using Debug=System.Diagnostics.Debug;

namespace MMF.Bone
{
	public class PMXSkeleton : ISkinningProvider
    {
        /// <summary>
        ///     Global pose for each bone
        /// </summary>
        public Matrix[] GlobalBonePose;

        /// <summary>
        /// Indicates that mangled bones after。
        /// </summary>
	    public event EventHandler SkeletonUpdated = delegate { };

        /// <summary>
        ///     IkInterface to handle
        /// </summary>
        public ITransformUpdater IkProvider;


        public List<ITransformUpdater> KinematicsProviders { get; private set; }

        /// <summary>
        ///     Root of the bone (which may be more than one root)
        /// </summary>
        public List<PMXBone> RootBone = new List<PMXBone>();

        public PMXSkeleton(ModelData model)
        {
            
            //Initialize only the number of bones
            this.GlobalBonePose = new Matrix[model.BoneList.BoneCount];
            this.Bone = new PMXBone[model.BoneList.BoneCount];
            this.IkBone = new List<PMXBone>();
            //Born to read
            LoadBones(model);
            this.BoneDictionary = new Dictionary<string, PMXBone>();
            foreach (var bone in this.Bone)
            {
                if (this.BoneDictionary.ContainsKey(bone.BoneName))
                {

                    int i = 0;
                    do
                    {
                        i++;
                    } while (this.BoneDictionary.ContainsKey(bone.BoneName + i.ToString()));
                    this.BoneDictionary.Add(bone.BoneName+i.ToString(),bone);
                    Debug.WriteLine("ボーン名{0}は重複しています。自動的にボーン名{1}と読み替えられました。",bone.BoneName,bone.BoneName+i);
                }else
                    this.BoneDictionary.Add(bone.BoneName,bone);
            }
            this.KinematicsProviders = new List<ITransformUpdater>();
            this.IkProvider = new CCDIK(this.IkBone);
            this.KinematicsProviders.Add(this.IkProvider);
            this.KinematicsProviders.Add(new BestrowKinematicsProvider(this.Bone));
            if (this.Bone.Length > 512)
            {
                throw new InvalidOperationException("MMFでは現在512以上のボーンを持つモデルについてサポートしていません。\nただし、Resource\\Shader\\DefaultShader.fx内のボーン変形行列の配列float4x4 BoneTrans[512]:BONETRANS;の要素数を拡張しこの部分をコメントアウトすれば暫定的に利用することができるかもしれません。");
            }
        }


        /// <summary>
        ///     Bone (in index order)
        /// </summary>
        public PMXBone[] Bone { get; set; }

	    public Dictionary<string, PMXBone> BoneDictionary { get; private set; }

	    /// <summary>
        ///     IkList of bones
        /// </summary>
        public List<PMXBone> IkBone { get; set; }

        /// <summary>
        ///     Pass the bone for the current effect。
        /// </summary>
        /// <param name="effect"></param>
        public void ApplyEffect(Effect effect)
        {
            effect.GetVariableBySemantic("BONETRANS").AsMatrix().SetMatrixArray(this.GlobalBonePose);
        }

	    /// <summary>
	    ///     Update the whole skinning
	    /// </summary>
	    /// <param name="morphManager"></param>
	    public virtual void UpdateSkinning(IMorphManager morphManager)
        {
            ResetAllBoneTransform();
            UpdateGlobal();
            foreach (ITransformUpdater kinematicsProvider in this.KinematicsProviders)
            {
                if (kinematicsProvider.UpdateTransform())
                {
                    UpdateGlobal();
                    //ResetAllBoneTransform();// BUG これなんでいれたっけ？
                }
            }
	        foreach (var pmxBone in this.RootBone)
	        {
	            pmxBone.UpdateGrobalPose();
	        }
            SkeletonUpdated(this,new EventArgs());
            for (int i = 0; i < this.Bone.Length; i++)
            {
                this.GlobalBonePose[this.Bone[i].BoneIndex] = this.Bone[i].GlobalPose;
            }
        }

        /// <summary>
        ///     All rotation・移動を元に戻します。
        /// </summary>
        public void ResetAllBoneTransform()
        {
            foreach (PMXBone item in this.Bone)
            {
                item.Rotation = Quaternion.Identity;
                item.Translation = Vector3.Zero;
            }
        }

        private void LoadBones(ModelData model)
        {
            for (int i = 0; i < model.BoneList.BoneCount; i++)
            {
                if (model.BoneList.Bones[i].ParentBoneIndex == -1)
                {
                    this.RootBone.Add(new PMXBone(model.BoneList.Bones, i, 0, this));
                }
            }
            BoneComparer comparer = new BoneComparer(model.BoneList.Bones.Count);
            this.IkBone.Sort(comparer);
            this.RootBone.Sort(comparer);
        }

        /// <summary>
        ///     Based on the current rotation matrix and calculates the global pose from the root bone。
        /// </summary>
        protected void UpdateGlobal()
        {
            // Parallel.ForEach(RootBone, (item) => item.UpdateGrobalPose());
            foreach (var root in this.RootBone)
                root.UpdateGrobalPose();
        }

		public virtual void Dispose() {}
    }
}