using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 自動実装コード VmeProto の追加要素
namespace OpenMMDFormat
{
    /// <summary>
    /// BoneFrameImplement the IFrameData interface for additional class
    /// </summary>
    public partial class BoneFrame : MMDFileParser.IFrameData
    {
        public uint FrameNumber
        {
            get { return (uint) this._frameNumber; }
        }

        public int CompareTo(Object x)
        {
            return (int) this.FrameNumber - (int)((MMDFileParser.IFrameData)x).FrameNumber;
        }
    }

    /// <summary>
    /// MorphFrameImplement the IFrameData interface for additional class
    /// </summary>
    public partial class MorphFrame : MMDFileParser.IFrameData
    {
        public uint FrameNumber
        {
            get { return (uint) this._frameNumber; }
        }

        public int CompareTo(Object x)
        {
            return (int) this.FrameNumber - (int)((MMDFileParser.IFrameData)x).FrameNumber;
        }
    }
}
