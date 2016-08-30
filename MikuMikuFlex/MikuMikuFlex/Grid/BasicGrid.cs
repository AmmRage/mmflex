using System.Collections.Generic;
using MMF.Model;
using MMF.Utility;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace MMF.Grid
{
    /// <summary>
    ///     Standard grid
    /// </summary>
    public class BasicGrid : IDrawable
    {
        /// <summary>
        ///     The length of the grid
        /// </summary>
        private const int GridLength = 100;

        /// <summary>
        ///     The width of the grid
        /// </summary>
        private const int GridWidth = 10;

        /// <summary>
        ///     The width of the grid
        /// </summary>
        private const int GridCount = 20;

        /// <summary>
        ///     The length of the shaft
        /// </summary>
        private const int AxisLength = 300;

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
        ///     To use rendering effects
        /// </summary>
        private Effect effect;

        /// <summary>
        ///     Input layout grid
        /// </summary>
        private InputLayout layout;

        /// <summary>
        ///     Count of grid
        /// </summary>
        private int vectorCount;

        /// <summary>
        ///     Grid vertex buffer
        /// </summary>
        private Buffer vertexBuffer;

        private bool _isVisibleMeasureGrid;
        private bool _isVisibleAxisGrid;

        /// <summary>
        ///     Constructor
        /// </summary>
        public BasicGrid()
        {
            this.Transformer = new BasicTransformer();
            this.Visibility = true;
            this.FileName = "@MMF.CG.Model.Grid.BasicGrid@";
        }

        /// <summary>
        ///     Whether or not the grid is visible
        /// </summary>
        public bool IsVisibleMeasureGrid
        {
            get { return this._isVisibleMeasureGrid; }
            set { this._isVisibleMeasureGrid = value; }
        }

        /// <summary>
        ///     Whether or not the seen is axis
        /// </summary>
        public bool IsVisibleAxisGrid
        {
            get { return this._isVisibleAxisGrid; }
            set { this._isVisibleAxisGrid = value; }
        }

        /// <summary>
        ///     Context to use to render the
        /// </summary>
        private RenderContext RenderContext { get; set; }

        public void Update()
        {
        }

        /// <summary>
        ///     Drawing
        /// </summary>
        public void Draw()
        {
            DeviceContext context = this.RenderContext.DeviceManager.Device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            this.effect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.RenderContext.MatrixManager.makeWorldViewProjectionMatrix(this));
            if (this.IsVisibleMeasureGrid)
            {
                context.InputAssembler.InputLayout = this.layout;
                context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.vertexBuffer, MeasureGridLayout.SizeInBytes, 0));
                this.effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(context);
                context.Draw(4*this.vectorCount, 0);
            }
            if (this.IsVisibleAxisGrid)
            {
                context.InputAssembler.InputLayout = this.axisLayout;
                context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.axisVertexBuffer, AxisGridLayout.SizeInBytes, 0));
                this.effect.GetTechniqueByIndex(0).GetPassByIndex(1).Apply(context);
                context.Draw(7*this.axisVectorCount, 0);
            }
        }

        public void GetFileName()
        {

        }

        public int VertexCount { get; private set; }

        /// <summary>
        ///     ITransformer use of mobile
        /// </summary>
        public ITransformer Transformer { get; private set; }

        /// <summary>
        ///     IDisposableImplementation
        /// </summary>
        public void Dispose()
        {
            this.axisVertexBuffer.Dispose();
            this.vertexBuffer.Dispose();
            this.layout.Dispose();
            this.axisLayout.Dispose();
            this.effect.Dispose();
        }

        /// <summary>
        ///     Initialization
        /// </summary>
        /// <param name="renderContext">And wars are in the rendering context to use</param>
        public void Load(RenderContext renderContext)
        {
            this.IsVisibleAxisGrid = true;
            this.IsVisibleMeasureGrid = true;
            this.RenderContext = renderContext;
            MakeGridVectors();
            this.SubsetCount = 1;
        }

        /// <summary>
        ///     Create a vertex buffer
        /// </summary>
        private void MakeGridVectors()
        {
            List<Vector3> gridVector = new List<Vector3>();
            this.effect = CGHelper.CreateEffectFx5FromResource(@"MMF.Resource.Shader.GridShader.fx", this.RenderContext.DeviceManager.Device);
            for (int i = 0; i <= GridCount; i++)
            {
                if (i != GridCount/2)
                {
                    gridVector.Add(new Vector3(-GridLength, 0, -GridLength + i*GridWidth));
                    gridVector.Add(new Vector3(GridLength, 0, -GridLength + i*GridWidth));
                }
            }
            for (int i = 0; i <= GridCount; i++)
            {
                if (i != GridCount/2)
                {
                    gridVector.Add(new Vector3(-GridLength + i*GridWidth, 0, -GridLength));
                    gridVector.Add(new Vector3(-GridLength + i*GridWidth, 0, GridLength));
                }
            }
            using (DataStream vs = new DataStream(gridVector.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription
                {
                    BindFlags = BindFlags.VertexBuffer,
                    SizeInBytes = (int) vs.Length
                };
                this.vertexBuffer = new Buffer(this.RenderContext.DeviceManager.Device, vs, bufDesc);
            }
            this.vectorCount = gridVector.Count;
            List<float> axisVector = new List<float>();
            addAxisVector(axisVector, AxisLength, 0, 0, new Vector4(1, 0, 0, 1));
            addAxisVector(axisVector, 0, AxisLength, 0, new Vector4(0, 1, 0, 1));
            addAxisVector(axisVector, 0, 0, AxisLength, new Vector4(0, 0, 1, 1));
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
            ShaderSignature v = this.effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature;
            this.layout = new InputLayout(this.RenderContext.DeviceManager.Device, v, MeasureGridLayout.VertexElements);
            v = this.effect.GetTechniqueByIndex(0).GetPassByIndex(1).Description.Signature;
            this.axisLayout = new InputLayout(this.RenderContext.DeviceManager.Device, v, AxisGridLayout.VertexElements);
            this.VertexCount = this.axisVectorCount + this.vectorCount;
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
        private static void addAxisVector(List<float> vertexList, float x, float y, float z, Vector4 color)
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