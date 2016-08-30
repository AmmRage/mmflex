using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    /// <summary>
    /// PMX521 line refer to specifications, frame element class
    /// </summary>
    public class FrameElementData
    {
        internal static FrameElementData GetFrameElementData(Stream fs,Header header)
        {
            FrameElementData data=new FrameElementData();
            data.IsMorph = ParserHelper.getByte(fs) == 1;
            if (data.IsMorph)
            {
                data.Index = ParserHelper.getIndex(fs, header.MorphIndexSize);
            }
            else
            {
                data.Index = ParserHelper.getIndex(fs, header.BoneIndexSize);
            }
            return data;
        }

        /// <summary>
        /// This morph?
        /// falseIf the target element's index of bone
        /// </summary>
        public bool IsMorph { get; private set; }

        /// <summary>
        /// The index for the element
        /// </summary>
        public int Index { get; private set; }
    }
}
