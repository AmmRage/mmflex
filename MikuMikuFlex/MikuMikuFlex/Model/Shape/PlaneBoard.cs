using System.Collections.Generic;
using MMF.Sprite;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct3D11;

namespace MMF.Model.Shape
{
    public class PlaneBoard:IDrawable
    {

        public Buffer VertexBuffer;
        private EffectPass renderPass;
        private RenderContext context;
        private readonly ShaderResourceView _resView;
        private InputLayout VertexInputLayout;

        /// <summary>
        /// Used to draw the sprite effects
        /// </summary>
        private Effect SpriteEffect { get; set; }

        public PlaneBoard(RenderContext context, ShaderResourceView resView):this(context,resView,new Vector2(200,200))
        {
            
        }
        public PlaneBoard(RenderContext context,ShaderResourceView resView,Vector2 size)
        {
            this.context = context;
            this._resView = resView;
            this.Visibility = true;
            this.SpriteEffect = CGHelper.CreateEffectFx5FromResource("MMF.Resource.Shader.SpriteShader.fx", context.DeviceManager.Device);
            this.VertexInputLayout = new InputLayout(context.DeviceManager.Device, this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, SpriteVertexLayout.InputElements);
            this.renderPass = this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            float width = size.X/2, height = size.Y/2;
            List<byte> vertexBytes=new List<byte>();
            CGHelper.AddListBuffer(new Vector3(-width, height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(width, height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(-width, -height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 1), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(width, height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(width, -height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 1), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(-width, -height, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 1), vertexBytes);
            using (DataStream ds = new DataStream(vertexBytes.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    SizeInBytes = (int)ds.Length
                };
                this.VertexBuffer = new Buffer(context.DeviceManager.Device, ds, bufDesc);
            }
            this.Transformer=new BasicTransformer();
            this.Transformer.Scale=new Vector3(0.2f);
        }
        public void Dispose()
        {
            if(this.VertexBuffer!=null&&!this.VertexBuffer.Disposed) this.VertexBuffer.Dispose();
            if(this.VertexInputLayout!=null&&!this.VertexInputLayout.Disposed) this.VertexInputLayout.Dispose();
        }

        public bool Visibility { get; set; }
        public string FileName { get; private set; }
        public int SubsetCount { get; private set; }
        public int VertexCount { get; private set; }
        public ITransformer Transformer { get; private set; }
        public void Draw()
        {
            this.SpriteEffect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.context.MatrixManager.makeWorldViewProjectionMatrix(this));
            this.SpriteEffect.GetVariableBySemantic("SPRITETEXTURE").AsResource().SetResource(this._resView);
            this.context.DeviceManager.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.VertexBuffer, SpriteVertexLayout.SizeInBytes, 0));
            this.context.DeviceManager.Context.InputAssembler.InputLayout = this.VertexInputLayout;
            this.context.DeviceManager.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.renderPass.Apply(this.context.DeviceManager.Context);
            this.context.DeviceManager.Context.Draw(12, 0);
        }

        public void Update()
        {
            
        }

        public Vector4 SelfShadowColor { get; set; }
        public Vector4 GroundShadowColor { get; set; }
    }
}
