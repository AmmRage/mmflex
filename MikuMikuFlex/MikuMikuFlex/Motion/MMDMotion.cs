using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMDFileParser.MotionParser;
using MMF.Bone;
using MMF.Model;
using MMF.Morph;

namespace MMF.Motion
{
    /// <summary>
    /// Class to manage the promotion
    /// </summary>
    public class MMDMotion : IMotionProvider
    {
        /// <summary>
        /// Bone
        /// </summary>
        private PMXBone[] bones;

        /// <summary>
        /// List of bones motion
        /// </summary>
        private readonly List<BoneMotion> boneMotions = new List<BoneMotion>();

        /// <summary>
        /// Motion morph list
        /// </summary>
        private readonly List<MorphMotion> morphMotions = new List<MorphMotion>(); 

        /// <summary>
        ///     Motion data
        /// </summary>
        private MotionData motionData;

        /// <summary>
        /// Motion play finish after behavior
        /// </summary>
        private ActionAfterMotion actionAfterMotion = ActionAfterMotion.Nothing;

        /// <summary>
        /// This motion is being played whether or
        /// </summary>
        private bool isPlaying;

        /// <summary>
        /// Whether or not the attached
        /// </summary>
        private bool isAttached;

        /// <summary>
        /// Whether or not to ignore the parent
        /// </summary>
        private bool ignoreParent;

        /// <summary>
        /// The contents of the constructor
        /// </summary>
        private void _MMDMotion(Stream fs, bool ignoreParent)
        {
            this.ignoreParent = ignoreParent;
            this.motionData = MotionData.getMotion(fs);
        }

        /// Constructor
        /// </summary>
        /// <param name="filePath">VMEFile name</param>
        /// <param name="ignoreParent">"全ての親"を無視するか否か</param>        
        public MMDMotion(string filePath, bool ignoreParent)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                _MMDMotion(fs, ignoreParent);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fs">VMEFile streams</param>
        /// <param name="ignoreParent">"全ての親"を無視するか否か</param>
        public MMDMotion(Stream fs, bool ignoreParent)
        {
            _MMDMotion(fs, ignoreParent);
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public event EventHandler<EventArgs> FrameTicked;
        public event EventHandler<ActionAfterMotion> MotionFinished;
        public float CurrentFrame { get; set; }
        public int FinalFrame { get; private set; }
        public bool IsAttached { get { return this.isAttached; } }
        public void Stop() {
            this.isPlaying = false; }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void AttachMotion(PMXBone[] bones)
        {
            this.bones = bones;

            // データのアタッチ
            AttachBoneFrameDataToBoneMotion();
            AttachMorphFrameDataToMorphMotion();

            //Sort of frame and last frame detection
            foreach (var boneMotion in this.boneMotions)
            {
                boneMotion.SortBoneFrameDatas();
                this.FinalFrame = Math.Max((int)boneMotion.GetFinalFrameNumber(), this.FinalFrame);
            }
            foreach (var morphMotion in this.morphMotions)
            {
                morphMotion.SortMorphFrameDatas();
            }

            this.isAttached = true;
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void Tick(int fps, float elapsedTime, IMorphManager morphManager)
        {
            // 行列の更新
            foreach (var boneMotion in this.boneMotions) boneMotion.ReviseBone(this.CurrentFrame);
            foreach (var morphMotion in this.morphMotions) morphManager.ApplyMorphProgress(morphMotion.GetMorphValue((ulong) this.CurrentFrame), morphMotion.MorphName);

            if (!this.isPlaying) return;
            this.CurrentFrame += (float)elapsedTime * fps;
            if (this.CurrentFrame >= this.FinalFrame) this.CurrentFrame = this.FinalFrame;
            if (FrameTicked != null) FrameTicked(this, new EventArgs());
            if (this.CurrentFrame >= this.FinalFrame)
            {
                if (MotionFinished != null) MotionFinished(this, this.actionAfterMotion);
                if (this.actionAfterMotion == ActionAfterMotion.Replay) this.CurrentFrame = 1.0e-3f;
            }
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void Start(float frame, ActionAfterMotion action)
        {
            if (frame > this.FinalFrame) throw new InvalidOperationException("最終フレームを超えた場所から再生を求められました。");
            this.CurrentFrame = frame;
            this.isPlaying = true;
            this.actionAfterMotion = action;
        }

        /// <summary>
        /// Bone motion to attach to the bone frame data
        /// </summary>
        private void AttachBoneFrameDataToBoneMotion()
        {
            foreach (var boneFrameData in this.motionData.boneFrameList.boneFrameDatas)
            {
                if (this.ignoreParent && boneFrameData.BoneName.Equals("全ての親")) continue;
                if (!this.bones.Any(b => b.BoneName.Equals(boneFrameData.BoneName))) continue;
                var bone = this.bones.Single(b => b.BoneName.Equals(boneFrameData.BoneName));
                if (!this.boneMotions.Any(bm => bm.GetBoneName().Equals(boneFrameData.BoneName)))
                {
                    var boneMotion = new BoneMotion(bone);
                    boneMotion.AddBoneFrameData(boneFrameData);
                    this.boneMotions.Add(boneMotion);
                    continue;
                }
                this.boneMotions.Single(bm => bm.GetBoneName().Equals(boneFrameData.BoneName)).AddBoneFrameData(boneFrameData);
            }
        }

        /// <summary>
        /// Morph motion to attach to morph frame data
        /// </summary>
        private void AttachMorphFrameDataToMorphMotion()
        {
            foreach (var morphFrameData in this.motionData.morphFrameList.morphFrameDatas)
            {
                if (!this.morphMotions.Any(mm => mm.MorphName.Equals(morphFrameData.Name)))
                {
                    var morphMotion = new MorphMotion(morphFrameData.Name);
                    morphMotion.AddMorphFrameData(morphFrameData);
                    this.morphMotions.Add(morphMotion);
                }
                this.morphMotions.Single(mm => mm.MorphName.Equals(morphFrameData.Name)).AddMorphFrameData(morphFrameData);
            }
        }


    }
}