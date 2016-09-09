using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMF.Bone;
using MMF.Model;
using MMF.Morph;
using OpenMMDFormat;

namespace MMF.Motion
{
    /// <summary>
    /// VMEFor promotion provider
    /// </summary>
    public class MMDMotionForVME : IMotionProvider
    {
        /// <summary>
        /// VMEFor motion structure
        /// </summary>
        private VocaloidMotionEvolved vocaloidMotionEvolved;

        /// <summary>
        /// Bone
        /// </summary>
        private PMXBone[] bones;

        /// <summary>
        /// "全ての親"を無視するか否か
        /// </summary>
        private bool ignoreParent;

        /// <summary>
        /// Collective motion of the bones
        /// </summary>
        private readonly List<BoneMotionForVME> boneMotions = new List<BoneMotionForVME>();

        /// <summary>
        /// Motion morph set
        /// </summary>
        private readonly List<MorphMotionForVME> morphMotions = new List<MorphMotionForVME>();


        /// <summary>
        /// Whether or not loaded
        /// </summary>
        private bool isAttached = false;

        /// <summary>
        /// Whether or not playing
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// Motion play finish after action
        /// </summary>
        private ActionAfterMotion actionAfterMotion = ActionAfterMotion.Nothing;

        /// <summary>
        /// The contents of the constructor
        /// </summary>
        private void _MMDMotionFromVME(Stream fs, bool ignoreParent)
        {
            this.ignoreParent = ignoreParent;

            // VME用モーション構造体の取得
            this.vocaloidMotionEvolved = ProtoBuf.Serializer.Deserialize<VocaloidMotionEvolved>(fs);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">VMEFile name</param>
        /// <param name="ignoreParent">"全ての親"を無視するか否か</param>
        public MMDMotionForVME(string filePath, bool ignoreParent)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                _MMDMotionFromVME(fs, ignoreParent);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fs">VMEFile streams</param>
        /// <param name="ignoreParent">"全ての親"を無視するか否か</param>
        public MMDMotionForVME(Stream fs, bool ignoreParent)
        {
            _MMDMotionFromVME(fs, ignoreParent);
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public float CurrentFrame { get; set; }
        public int FinalFrame { get; private set; }
        public event EventHandler<EventArgs> FrameTicked;
        public event EventHandler<ActionAfterMotion> MotionFinished;

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void AttachMotion(PMXBone[] bones)
        {
            this.bones = bones;

            // ボーンのモーションのセット
            var boneIDDictionary = new Dictionary<ulong, string>();
            foreach (var idTag in this.vocaloidMotionEvolved.boneIDTable) boneIDDictionary[idTag.id] = idTag.name;
            foreach (var boneFrameTable in this.vocaloidMotionEvolved.boneFrameTables)
            {
                var boneName = boneIDDictionary[boneFrameTable.id];
                if ((this.ignoreParent && boneName.Equals("全ての親")) || !bones.Any(b => b.BoneName.Equals(boneName))) continue;
                this.boneMotions.Add(new BoneMotionForVME(bones.Single(b => b.BoneName.Equals(boneName)), boneFrameTable.frames));
            }

            // モーフのモーションのセット
            var morphIDDictionary = new Dictionary<ulong, string>();
            foreach (var idTag in this.vocaloidMotionEvolved.morphIDTable) morphIDDictionary[idTag.id] = idTag.name;
            foreach (var morphFrameTable in this.vocaloidMotionEvolved.morphFrameTables)
            {
                var morphName = morphIDDictionary[morphFrameTable.id];
                this.morphMotions.Add(new MorphMotionForVME(morphName, morphFrameTable.frames));
            }
            
            // FinalFrameの検出
            foreach (var boneMotion in this.boneMotions)
            {
                this.FinalFrame = Math.Max((int)boneMotion.GetFinalFrame(), this.FinalFrame);
            }

            // ロード完了
            this.isAttached = true;
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void Tick(int fps, float elapsedTime, IMorphManager morphManager)
        {
            // 行列の更新
            foreach (var boneMotion in this.boneMotions) boneMotion.ReviseBone((ulong) this.CurrentFrame);
            foreach (var morphMotion in this.morphMotions) morphManager.ApplyMorphProgress(morphMotion.GetMorphValue((ulong) this.CurrentFrame), morphMotion.MorphName);

            // 停止中はフレームを進めない
            if (!this.isPlaying) return;

            // フレームを進める
            this.CurrentFrame += elapsedTime * fps;
            if (FrameTicked != null) FrameTicked(this, new EventArgs());

            // 最終フレームに達した時の処理
            if (this.CurrentFrame >= this.FinalFrame)
            {
                this.CurrentFrame = (this.actionAfterMotion == ActionAfterMotion.Replay) ? 1.0e-3f : this.FinalFrame;
                if (MotionFinished != null) MotionFinished(this, this.actionAfterMotion);
            }
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void Start(float frame, ActionAfterMotion actionAfterMotion)
        {
            if (frame > this.FinalFrame)
            {
                throw new InvalidOperationException("最終フレームを超えた場所から再生を求められました。");
            }
            this.CurrentFrame = frame;
            this.actionAfterMotion = actionAfterMotion;
            this.isPlaying = true;
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public void Stop()
        {
            this.isPlaying = false;
        }

        /// <summary>
        /// IMotionProviderImplementation of a Member
        /// </summary>
        public bool IsAttached
        {
            get { return this.isAttached; }
        }
    }
}
