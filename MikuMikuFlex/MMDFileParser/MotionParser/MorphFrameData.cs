using MMDFileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class MorphFrameData: IFrameData
    {
        internal static MorphFrameData getMorphFrame(Stream fs)
        {
            var morphFrameData = new MorphFrameData
            {
                Name = ParserHelper.getShift_JISString(fs, 15),
                FrameNumber = ParserHelper.getDWORD(fs),
                MorphValue = ParserHelper.getFloat(fs)
            };
            return morphFrameData;
        }

        public String Name;

        public float MorphValue;

        public uint FrameNumber { get; private set; }

        public int CompareTo(Object x)
        {
            return (int) this.FrameNumber - (int)((IFrameData)x).FrameNumber;
        }

    }
}
