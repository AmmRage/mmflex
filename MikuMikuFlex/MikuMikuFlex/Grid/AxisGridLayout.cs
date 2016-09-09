using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace MMF.Grid
{
    /// <summary>
    ///     Use of axis input layout
    /// </summary>
    public struct AxisGridLayout
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
            },
            new InputElement
            {
                SemanticName = "COLOR",
                Format = Format.R32G32B32A32_Float,
                AlignedByteOffset = InputElement.AppendAligned
            }
        };

        /// <summary>
        ///     Color
        /// </summary>
        public Vector4 Color;

        /// <summary>
        ///     Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The size of one element per
        /// </summary>
        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof (AxisGridLayout)); }
        }
    }
}