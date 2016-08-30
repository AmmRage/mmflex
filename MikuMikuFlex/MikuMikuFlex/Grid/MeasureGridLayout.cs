using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace MMF.Grid
{
    /// <summary>
    ///     Input layout for drawing grid
    /// </summary>
    public struct MeasureGridLayout
    {
        /// <summary>
        ///     Input layout
        /// </summary>
        public static readonly InputElement[] VertexElements =
        {
            new InputElement
            {
                SemanticName = "POSITION",
                Format = Format.R32G32B32_Float
            }
        };

        /// <summary>
        ///     Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The size of one element per
        /// </summary>
        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof (MeasureGridLayout)); }
        }
    }
}