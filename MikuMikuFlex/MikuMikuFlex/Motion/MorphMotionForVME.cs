using System;
using System.Collections.Generic;
using System.Linq;
using MMF.Utility;
using OpenMMDFormat;

namespace MMF.Motion
{
    /// <summary>
    /// Motion morph one set (the VME version)
    /// Lined up in ascending order of the frame
    /// </summary>
    class MorphMotionForVME
    {
        /// <summary>
        /// Frame Manager
        /// </summary>
        private MMDFileParser.FrameManager frameManager = new MMDFileParser.FrameManager();

        /// <summary>
        /// The name of the morph
        /// </summary>
        public string MorphName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="morphName">The name of the morph</param>
        /// <param name="morphFrames">Morph motion data</param>
        public MorphMotionForVME(string morphName, List<MorphFrame> morphFrames)
        {
            this.MorphName = this.MorphName;
            foreach (var morphFrame in morphFrames) this.frameManager.AddFrameData(morphFrame);
            if (!this.frameManager.IsSorted()) throw new Exception("VMEデータがソートされていません");
        }

        /// <summary>
        /// Returns a morph of the specified frame number
        /// </summary>
        /// <param name="frameNumber">Frame number</param>
        public float GetMorphValue(ulong frameNumber)
        {
            // 現在のフレームの前後のキーフレームを探す
            MMDFileParser.IFrameData pastFrame, futureFrame;
            this.frameManager.SearchKeyFrame(frameNumber, out pastFrame, out futureFrame);
            var pastMorphFrame = (MorphFrame)pastFrame;
            var futureMorphFrame = (MorphFrame)futureFrame;

            // 現在のフレームの前後キーフレーム間での進行度を求める
            float s = (futureMorphFrame.frameNumber == pastMorphFrame.frameNumber)? 0 : 
                (float)(frameNumber - pastMorphFrame.frameNumber) / (float)(futureMorphFrame.frameNumber - pastMorphFrame.frameNumber); // 進行度

            // 線形補完で値を求める
            return CGHelper.Lerp(pastMorphFrame.value, futureMorphFrame.value, s);
        }

    }
}
