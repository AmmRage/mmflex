using System;
using System.Collections.Generic;
using System.Linq;
using MMDFileParser.MotionParser;
using MMF.Bone;
using MMF.Utility;
using SlimDX;

namespace MMF.Motion
{
    /// <summary>
    /// VMDClass to update the bone using motion data
    /// </summary>
    internal class BoneMotion
    {
        /// <summary>
        /// Bone
        /// </summary>
        private PMXBone bone;

        /// <summary>
        /// Frame Manager
        /// </summary>
        private MMDFileParser.FrameManager frameManager = new MMDFileParser.FrameManager();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bone">Bone</param>
        public BoneMotion(PMXBone bone)
        {
            this.bone = bone;
        }

        /// <summary>
        /// Add bone frame
        /// </summary>
        /// <param name="boneFrameData">Bone frame data</param>
        public void AddBoneFrameData(BoneFrameData boneFrameData)
        {
            this.frameManager.AddFrameData(boneFrameData);
        }

        /// <summary>
        /// To sort the list of bone frame data
        /// </summary>
        public void SortBoneFrameDatas()
        {
            this.frameManager.SortFrameDatas();
        }

        /// <summary>
        /// To get the last frame number of the bone frame data
        /// </summary>
        /// <returns>Last frame number of the bone frame data</returns>
        public uint GetFinalFrameNumber()
        {
            return this.frameManager.GetFinalFrameNumber();
        }

        /// <summary>
        /// Get the bone name
        /// </summary>
        /// <returns>Bone name</returns>
        public String GetBoneName() { return this.bone.BoneName; }

        /// <summary>
        /// To update the position of the bones to the specified frame number
        /// </summary>
        /// <param name="frameNumber">The current frame number</param>
        public void ReviseBone(float frameNumber)
        {
            // 現在のフレームの前後のキーフレームを探す
            MMDFileParser.IFrameData pastFrame, futureFrame;
            this.frameManager.SearchKeyFrame(frameNumber, out pastFrame, out futureFrame);
            var pastBoneFrame = (BoneFrameData)pastFrame;
            var futureBoneFrame = (BoneFrameData)futureFrame;

            // 現在のフレームの前後キーフレーム間での進行度を求めてペジェ関数で変換する
            float s = (futureBoneFrame.FrameNumber == pastBoneFrame.FrameNumber) ? 0 : 
                (float)(frameNumber - pastBoneFrame.FrameNumber) / (float)(futureBoneFrame.FrameNumber - pastBoneFrame.FrameNumber); // 進行度
            var ss = new float[4];
            for (int i = 0; i < ss.Length; ++i) ss[i] = pastBoneFrame.Curves[i].Evaluate(s);

            // ボーンを更新する
            this.bone.Translation = CGHelper.ComplementTranslate(pastBoneFrame, futureBoneFrame, new Vector3(ss[0], ss[1], ss[2]));
            this.bone.Rotation = CGHelper.ComplementRotateQuaternion(pastBoneFrame, futureBoneFrame, ss[3]);
        }

    }
}