using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SlimDX.Direct3D11.Buffer;
using SlimDX;
using SlimDX.D3DCompiler;
using MMDFileParser.MotionParser;
namespace MMDFileParser
{
    public static class CGHelper
    {
        /// Values in the range of maximum and minimum values fit
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Contains the value</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (min > value)
                return min;
            if (max < value)
                return max;
            return value;
        }
    }
}
