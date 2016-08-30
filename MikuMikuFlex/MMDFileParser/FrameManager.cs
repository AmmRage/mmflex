using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser
{
    public class FrameManager
    {
        private List<IFrameData> frameDatas = new List<IFrameData>();

        /// <summary>
        /// Last frame of the last keyframe index
        /// </summary>
        private int beforePastFrameIndex = 0;

        /// <summary>
        /// Add the frame data
        /// </summary>
        /// <param name="frameData">Frame data</param>
        public void AddFrameData(IFrameData frameData)
        {
            this.frameDatas.Add(frameData);
        }

        /// <summary>
        /// Sort the data frame
        /// </summary>
        public void SortFrameDatas()
        {
            this.frameDatas.Sort();
        }

        /// <summary>
        /// To check that lined up in ascending order of the frame
        /// </summary>
        public bool IsSorted()
        {
            var prev = this.frameDatas[0].FrameNumber;
            foreach (var frameData in this.frameDatas)
            {
                if (prev > frameData.FrameNumber) return false;
                prev = frameData.FrameNumber;
            }
            return true;
        }

        /// <summary>
        /// Gets the ending frame number the frame data
        /// </summary>
        /// <returns>Last frame number the frame data</returns>
        public uint GetFinalFrameNumber()
        {
            return this.frameDatas.Last().FrameNumber;
        }

        /// <summary>
        /// Find the keyframes of the current frame
        /// </summary>
        /// <param name="frameNumber">The current frame number</param>
        /// <param name="pastFrame">The last keyframe</param>
        /// <param name="futureFrame">Keyframes for the future</param>
        public void SearchKeyFrame(float frameNumber, out IFrameData pastFrame, out IFrameData futureFrame)
        {
            // 現在のフレームが最初のキーフレームより前にある場合
            if (frameNumber < this.frameDatas.First().FrameNumber)
            {
                pastFrame = futureFrame = this.frameDatas.First();
                return;
            }

            // 現在のフレームが最後のキーフレームより後にある場合
            if (frameNumber >= this.frameDatas.Last().FrameNumber)
            {
                pastFrame = futureFrame = this.frameDatas.Last();
                return;
            }

            // 現在のフレームの後のキーフレームのインデックスを探す
            // 高速化のため、現在のフレームが前回の過去キーフレームより進んでいれば、前回の過去キーフレームから探す
            int futureFrameIndex;
            if (this.frameDatas[this.beforePastFrameIndex].FrameNumber < frameNumber)
                futureFrameIndex = this.frameDatas.FindIndex(this.beforePastFrameIndex, b => b.FrameNumber > frameNumber);
            else
                futureFrameIndex = this.frameDatas.FindIndex(b => b.FrameNumber > frameNumber);

            // 現在のフレームの前後のキーフレームを出力
            pastFrame = this.frameDatas[futureFrameIndex - 1];
            futureFrame = this.frameDatas[futureFrameIndex];

            // 現在の過去キーフレームを記憶しておく
            this.beforePastFrameIndex = futureFrameIndex - 1;
        }
    }
}
