using System.Collections.Generic;
using System.Drawing;
using MMF.DeviceManager;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace MMF.Model.Shape
{
    public abstract class ShapeBase : IDrawable,IHitTestable
    {
        protected Vector4 _color;
        private InputLayout layout;
        private Effect effect;
        private Buffer indexBuffer;
        private Buffer vertexBuffer;

        public ShapeBase(RenderContext context,Vector4 color)
        {
            this._color = color;
            this.RenderContext = context;
            this.Visibility = true;
            this.SubsetCount = 1;
            this.Transformer=new BasicTransformer();
            this.RenderContext = context;
        }

        public RenderContext RenderContext { get; set; }
        public bool Visibility { get; set; }
        public abstract string FileName { get; }
        public int SubsetCount { get; private set; }
        public abstract int VertexCount { get; }
        public ITransformer Transformer { get; private set; }
        public Vector4 SelfShadowColor { get; set; }
        public Vector4 GroundShadowColor { get; set; }

        public void Dispose()
        {
            if(this.indexBuffer!=null&&!this.indexBuffer.Disposed) this.indexBuffer.Dispose();
            if (this.vertexBuffer != null && !this.vertexBuffer.Disposed) this.vertexBuffer.Dispose();
            if (this.effect != null && !this.effect.Disposed) this.effect.Dispose();
            if(this.layout!=null&&!this.layout.Disposed) this.layout.Dispose();
        }

        public void Initialize()
        {
            this.effect = CGHelper.CreateEffectFx5FromResource(@"MMF.Resource.Shader.ShapeShader.fx", this.RenderContext.DeviceManager.Device);
            List<Vector4> positions=new List<Vector4>();
            InitializePositions(positions);
            this.vertexBuffer = CGHelper.CreateBuffer(positions, this.RenderContext.DeviceManager.Device, BindFlags.VertexBuffer);
            IndexBufferBuilder builder=new IndexBufferBuilder(this.RenderContext);
            InitializeIndex(builder);
            this.indexBuffer = builder.build();
            this.layout = new InputLayout(this.RenderContext.DeviceManager.Device,
                this.effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature,
                ShapeInputLayout.VertexElements);
        }

        protected abstract void InitializeIndex(IndexBufferBuilder builder);

        protected abstract void InitializePositions(List<Vector4> positions);


        public void Draw()
        {
            DeviceContext context = this.RenderContext.DeviceManager.Device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology=PrimitiveTopology.TriangleList;
            this.effect.GetVariableBySemantic("COLOR").AsVector().Set(this._color);
            this.effect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.RenderContext.MatrixManager.makeWorldViewProjectionMatrix(this));
            context.InputAssembler.InputLayout = this.layout;
            context.InputAssembler.SetIndexBuffer(this.indexBuffer,Format.R32_UInt,0);
            context.InputAssembler.SetVertexBuffers(0,new VertexBufferBinding(this.vertexBuffer,ShapeInputLayout.SizeInBytes,0));
            this.effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(context);
            context.DrawIndexed(this.VertexCount,0,0);
        }

        public void Update()
        {
            
        }

        public void RenderHitTestBuffer(float col)
        {
            DeviceContext context = this.RenderContext.DeviceManager.Device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.effect.GetVariableBySemantic("COLOR").AsVector().Set(new Vector4(col,0,0,0));
            this.effect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.RenderContext.MatrixManager.makeWorldViewProjectionMatrix(this));
            context.InputAssembler.InputLayout = this.layout;
            context.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, ShapeInputLayout.SizeInBytes, 0));
            this.effect.GetTechniqueByIndex(0).GetPassByIndex(1).Apply(context);
            context.DrawIndexed(this.VertexCount, 0, 0);
        }

        public virtual void HitTestResult(bool result, bool mouseState, Point mousePosition)
        {
            
        }
    }
}