using System.Collections.Generic;
using MMDFileParser.MotionParser;
using MMF.Utility;
using System.Linq;

namespace MMF.Motion
{
    public class MorphMotion
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
        public MorphMotion(string morphName)
        {
            this.MorphName = morphName;
        }

        /// <summary>
        /// Add morph frame data
        /// </summary>
        /// <param name="morphFrameData">Morph frame data</param>
        public void AddMorphFrameData(MorphFrameData morphFrameData)
        {
            this.frameManager.AddFrameData(morphFrameData);
        }

        /// <summary>
        /// To sort the list of moefframe data
        /// </summary>
        public void SortMorphFrameDatas()
        {
            this.frameManager.SortFrameDatas();
        }

        /// <summary>
        /// Returns a morph of the specified frame number
        /// </summary>
        /// <param name="frame">Frame number</param>
        public float GetMorphValue(float frameNumber)
        {
            // 現在のフレームの前後のキーフレームを探す
            MMDFileParser.IFrameData pastFrame, futureFrame;
            this.frameManager.SearchKeyFrame(frameNumber, out pastFrame, out futureFrame);
            var pastMorphFrame = (MorphFrameData)pastFrame;
            var futureMorphFrame = (MorphFrameData)futureFrame;

            // 現在のフレームの前後キーフレーム間での進行度を求めてペジェ関数で変換する
            float s = (futureMorphFrame.FrameNumber == pastMorphFrame.FrameNumber) ? 0 :
                (float)(frameNumber - pastMorphFrame.FrameNumber) / (float)(futureMorphFrame.FrameNumber - pastMorphFrame.FrameNumber); // 進行度
            return CGHelper.Lerp(pastMorphFrame.MorphValue, futureMorphFrame.MorphValue, s);
        }

    }
}
