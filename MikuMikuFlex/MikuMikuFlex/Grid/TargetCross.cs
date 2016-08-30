using System.Collections.Generic;
using MMF.Model;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace MMF.Grid
{
    /// <summary>
    ///     Used to indicate the position of the bone, such as for debugging purposes, such as
    /// </summary>
    public class TargetCross : IDrawable
    {
        /// <summary>
        ///     The length of the shaft
        /// </summary>
        private const int AxisLength = 3;

        /// <summary>
        ///     Axis input layout
        /// </summary>
        private InputLayout axisLayout;

        /// <summary>
        ///     Axes count
        /// </summary>
        private int axisVectorCount;

        /// <summary>
        ///     Axis the vertex buffer
        /// </summary>
        private Buffer axisVertexBuffer;

        /// <summary>
        ///     Effects
        /// </summary>
        private Effect effect;

        /// <summary>
        ///     Constructor
        /// </summary>
        public TargetCross()
        {
            this.Transformer = new BasicTransformer();
        }

        /// <summary>
        ///     Visibility
        /// </summary>
        public bool IsVisibleAxisGrid { get; set; }

        /// <summary>
        ///     Context that is used to render the
        /// </summary>
        private RenderContext RenderContext { get; set; }

        /// <summary>
        ///     Drawing
        /// </summary>
        public void Draw()
        {
            DeviceContext context = this.RenderContext.DeviceManager.Device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            this.effect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.RenderContext.MatrixManager.makeWorldViewProjectionMatrix(this.Transformer.Scale, this.Transformer.Rotation, this.Transformer.Position));
            if (this.IsVisibleAxisGrid)
            {
                context.InputAssembler.InputLayout = this.axisLayout;
                context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.axisVertexBuffer, AxisGridLayout.SizeInBytes, 0));
                this.effect.GetTechniqueByIndex(0).GetPassByIndex(1).Apply(context);
                context.Draw(7*this.axisVectorCount, 0);
            }
        }

        public int VertexCount { get; private set; }

        /// <summary>
        ///     ITransformer use of deformation
        /// </summary>
        public ITransformer Transformer { get; private set; }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            this.axisVertexBuffer.Dispose();
            this.axisLayout.Dispose();
            this.effect.Dispose();
        }

        public void Update()
        {
        }

        /// <summary>
        ///     Initialization
        /// </summary>
        /// <param name="renderContext">Context that is used to render the</param>
        public void Load(RenderContext renderContext)
        {
            this.IsVisibleAxisGrid = true;
            this.RenderContext = renderContext;
            MakeGridVectors();
            this.SubsetCount = 1;
        }

        public void GetFileName()
        {

        }

        /// <summary>
        ///     To create a buffer
        /// </summary>
        private void MakeGridVectors()
        {
            //Load effect
            using (ShaderBytecode byteCode = ShaderBytecode.CompileFromFile(@"Shader\grid.fx", "fx_5_0"))
            {
                this.effect = new Effect(this.RenderContext.DeviceManager.Device, byteCode);
            }
            //First on the list containing the vertex
            List<float> axisVector = new List<float>();
            AddAxisVector(axisVector, AxisLength, 0, 0, new Vector4(1, 0, 0, 1));
            AddAxisVector(axisVector, 0, AxisLength, 0, new Vector4(0, 1, 0, 1));
            AddAxisVector(axisVector, 0, 0, AxisLength, new Vector4(0, 0, 1, 1));
            //Create a buffer
            using (DataStream vs = new DataStream(axisVector.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription
                {
                    BindFlags = BindFlags.VertexBuffer,
                    SizeInBytes = (int) vs.Length
                };
                this.axisVertexBuffer = new Buffer(this.RenderContext.DeviceManager.Device, vs, bufDesc);
            }
            this.axisVectorCount = axisVector.Count;
            //Create the input layout
            ShaderSignature v = this.effect.GetTechniqueByIndex(0).GetPassByIndex(1).Description.Signature;
            this.axisLayout = new InputLayout(this.RenderContext.DeviceManager.Device, v, AxisGridLayout.VertexElements);
            this.VertexCount = this.axisVectorCount;
        }

        /// <summary>
        ///     As the axis containing the vertex
        ///     x,y,zはどれか１つがnot 0,それ以外0を想定
        /// </summary>
        /// <param name="vertexList">List for creating a vertex buffer</param>
        /// <param name="x">xOf the length</param>
        /// <param name="y">yOf the length</param>
        /// <param name="z">zOf the length</param>
        /// <param name="color">Color</param>
        private static void AddAxisVector(List<float> vertexList, float x, float y, float z, Vector4 color)
        {
            vertexList.Add(x);
            vertexList.Add(y);
            vertexList.Add(z);
            vertexList.Add(color.X);
            vertexList.Add(color.Y);
            vertexList.Add(color.Z);
            vertexList.Add(color.W);
            vertexList.Add(-x);
            vertexList.Add(-y);
            vertexList.Add(-z);
            vertexList.Add(color.X);
            vertexList.Add(color.Y);
            vertexList.Add(color.Z);
            vertexList.Add(color.W);
        }

        #region IDrawable メンバー

        public Vector4 SelfShadowColor { get; set; }

        public Vector4 GroundShadowColor { get; set; }

        public bool Visibility { get; set; }

        public string FileName { get; set; }
        public int SubsetCount { get; private set; }

        #endregion
    }
}