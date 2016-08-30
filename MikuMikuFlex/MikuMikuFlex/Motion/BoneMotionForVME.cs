using System;
using System.Collections.Generic;
using System.Linq;
using MMF.Bone;
using MMF.Utility;
using OpenMMDFormat;

namespace MMF.Motion
{
    /// <summary>
    /// VMEClass to update the bone using motion data
    /// </summary>
    internal class BoneMotionForVME
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
        /// <param name="boneFrames">Motion data of the bone</param>
        public BoneMotionForVME(PMXBone bone, List<BoneFrame> boneFrames)
        {
            this.bone = bone;
            foreach (var boneFrame in boneFrames) this.frameManager.AddFrameData(boneFrame);
            if (!this.frameManager.IsSorted()) throw new Exception("VMEデータがソートされていません");
        }

        /// <summary>
        /// To get the frame number of the last
        /// </summary>
        /// <returns>The last frame number</returns>
        public uint GetFinalFrame()
        {
            return this.frameManager.GetFinalFrameNumber();
        }

        /// <summary>
        /// To update the position of the bones to the specified frame number
        /// </summary>
        /// <param name="frameNumber">Frame number</param>
        public void ReviseBone(ulong frameNumber)
        {
            // 現在のフレームの前後のキーフレームを探す
            MMDFileParser.IFrameData pastFrame, futureFrame;
            this.frameManager.SearchKeyFrame(frameNumber, out pastFrame, out futureFrame);
            var pastBoneFrame = (BoneFrame)pastFrame;
            var futureBoneFrame = (BoneFrame)futureFrame;

            // 現在のフレームの前後キーフレーム間での進行度を求めてペジェ関数で変換する
            float s = (pastBoneFrame.frameNumber == futureBoneFrame.frameNumber)? 0 :
                (float)(frameNumber - pastBoneFrame.frameNumber) / (float)(futureBoneFrame.frameNumber - pastBoneFrame.frameNumber); // 進行度
            BezInterpolParams p = pastBoneFrame.interpolParameters;
            float s_X, s_Y, s_Z,s_R;
            if (p != null)
            {
                s_X = BezEvaluate(p.X1, p.X2, s);
                s_Y = BezEvaluate(p.Y1, p.Y2, s);
                s_Z = BezEvaluate(p.Z1, p.Z2, s);
                s_R = BezEvaluate(p.R1, p.R2, s); // ペジェ変換後の進行度
            }
            else
            {//In the absence of a parameter of a Bézier curve uses a s as the amount of linear interpolation
                s_X = s_Y = s_Z = s_R = s;
            }
            // ボーンを更新する
            this.bone.Translation = new SlimDX.Vector3(
                CGHelper.Lerp(pastBoneFrame.position.x, futureBoneFrame.position.x, s_X),
                CGHelper.Lerp(pastBoneFrame.position.y, futureBoneFrame.position.y, s_Y),
                CGHelper.Lerp(pastBoneFrame.position.z, futureBoneFrame.position.z, s_Z));
            this.bone.Rotation = SlimDX.Quaternion.Slerp(pastBoneFrame.rotation.ToSlimDX(), futureBoneFrame.rotation.ToSlimDX(), s_R);
        }

        /// <summary>
        /// Bezier functions
        /// </summary>
        /// <param name="v1">Bezier shape parameter 1</param>
        /// <param name="v2">Bezier shape parameter 2</param>
        /// <param name="s">Variable</param>
        /// <returns>Bezier function value</returns>
        private float BezEvaluate(bvec2 v1, bvec2 v2, float s)
        {
            var curve = new MMDFileParser.MotionParser.BezierCurve();
            curve.v1 = v1.ToSlimDX() / 127;
            curve.v2 = v2.ToSlimDX() / 127;
            return curve.Evaluate(s);
        }

    }
}
