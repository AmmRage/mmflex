using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace MMF.Sprite
{
    /// <summary>
    /// Classes for displaying 2-D
    /// Cannot guarantee the speed
    /// </summary>
    public class GDISpriteBatch:IDisposable
    {
        /// <summary>
        /// BlendState using blend
        /// </summary>
        private BlendState state;

        /// <summary>
        /// The view matrix to use to draw the sprite
        /// </summary>
        private Matrix ViewMatrix;

        /// <summary>
        /// Used to draw the sprite projection matrix
        /// </summary>
        private Matrix SpriteProjectionMatrix;

        /// <summary>
        /// Viewport to use to draw the sprite
        /// </summary>
        private Viewport spriteViewport;

        /// <summary>
        /// The destination vertex buffer the sprite texture
        /// </summary>
        private Buffer VertexBuffer { get;set; }

        /// <summary>
        /// Sprite input layout
        /// </summary>
        private InputLayout VertexInputLayout { get; set; }

        /// <summary>
        /// Used to draw the sprite effects
        /// </summary>
        public Effect SpriteEffect { get; private set; }

        /// <summary>
        /// The path to use to draw the sprite
        /// </summary>
        private EffectPass renderPass { get; set; }

        /// <summary>
        /// Texture drawn sprite
        /// </summary>
        public ShaderResourceView SpriteTexture { get; private set; }

        /// <summary>
        /// The size of the texture
        /// </summary>
        public Vector2 TextureSize { get; private set; }

        /// <summary>
        /// Transparent color
        /// </summary>
        public Vector3 TransparentColor { get; private set; }

        /// <summary>
        /// Bitmap is attached to a sprite
        /// </summary>
        private Bitmap mapedBitmap { get; set; }

        /// <summary>
        /// Graphic is attached to a sprite
        /// After you edit a true NeedRedraw。
        /// </summary>
        public  Graphics mapedGraphic { get;private set; }

        /// <summary>
        /// Whether or not need to redraw the sprite texture
        /// </summary>
        public bool NeedRedraw { get; set; }

        /// <summary>
        /// For the texture Sampler for sprites
        /// </summary>
        private SamplerState sampler { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Render context</param>
        /// <param name="width">The width used to draw the texture resolution</param>
        /// <param name="height">High resolution textures used to render</param>
        public GDISpriteBatch(RenderContext context,int width,int height)
        {
            this.Context = context;
            Resize(width,height);
            this.SpriteEffect = CGHelper.CreateEffectFx5("Shader\\sprite.fx", context.DeviceManager.Device);
            this.renderPass = this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            this.VertexInputLayout = new InputLayout(context.DeviceManager.Device, this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, SpriteVertexLayout.InputElements);
            SamplerDescription desc=new SamplerDescription();
            desc.Filter = Filter.MinMagMipLinear;
            desc.AddressU=TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;
            desc.AddressW = TextureAddressMode.Wrap;
            this.sampler = SamplerState.FromDescription(context.DeviceManager.Device, desc);
            this.mapedGraphic = Graphics.FromImage(this.mapedBitmap);
            BlendStateDescription blendDesc=new BlendStateDescription();
            blendDesc.AlphaToCoverageEnable = false;
            blendDesc.IndependentBlendEnable = false;
            for (int i = 0; i < blendDesc.RenderTargets.Length; i++)
            {
                blendDesc.RenderTargets[i].BlendEnable = true;
                blendDesc.RenderTargets[i].SourceBlend = BlendOption.SourceAlpha;
                blendDesc.RenderTargets[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDesc.RenderTargets[i].BlendOperation = BlendOperation.Add;
                blendDesc.RenderTargets[i].SourceBlendAlpha = BlendOption.One;
                blendDesc.RenderTargets[i].DestinationBlendAlpha = BlendOption.Zero;
                blendDesc.RenderTargets[i].BlendOperationAlpha = BlendOperation.Add;
                blendDesc.RenderTargets[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            this.ViewMatrix = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            this.state = BlendState.FromDescription(context.DeviceManager.Device, blendDesc);
            this.NeedRedraw = true;
            Update();
        }

        /// <summary>
        /// To change the size of the texture
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void Resize(int width, int height)
        {
            this.TextureSize=new Vector2(width,height);
            float w = width/2f, h = height/2f;
            List<byte> vertexBytes = new List<byte>();
            this.TransparentColor = new Vector3(0, 0, 0);
            CGHelper.AddListBuffer(new Vector3(-w, h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(w, h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(-w, -h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 1), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(w, h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(w, -h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(1, 1), vertexBytes);
            CGHelper.AddListBuffer(new Vector3(-w, -h, 0), vertexBytes);
            CGHelper.AddListBuffer(new Vector2(0, 1), vertexBytes);
            using (DataStream ds = new DataStream(vertexBytes.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    SizeInBytes = (int) ds.Length
                };
                this.VertexBuffer = new Buffer(this.Context.DeviceManager.Device, ds, bufDesc);
            }
            this.SpriteProjectionMatrix = Matrix.OrthoLH(width, height, 0, 100);
            this.spriteViewport = new Viewport()
            {
                Width = width,
                Height = height,
                MaxZ = 1
            };
            this.mapedBitmap=new Bitmap(width,height);
            if(this.mapedGraphic!=null) this.mapedGraphic.Dispose();
            this.mapedGraphic = Graphics.FromImage(this.mapedBitmap);
        }

        /// <summary>
        /// If you need to update the texture update
        /// Calling the render with the same timing。
        /// </summary>
        public void Update()
        {
            if (this.NeedRedraw)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                        lock (this.mapedBitmap)
                        {
                            this.mapedBitmap.Save(ms, ImageFormat.Tiff);
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        if(this.SpriteTexture!=null)
                        lock (this.SpriteTexture)
                        {
                            this.SpriteTexture.Dispose();
                            this.SpriteTexture = ShaderResourceView.FromStream(this.Context.DeviceManager.Device, ms, (int)ms.Length);
                        }
                        else
                        {
                            this.SpriteTexture = ShaderResourceView.FromStream(this.Context.DeviceManager.Device, ms, (int)ms.Length);
                        }
                    }
                    catch (Direct3D11Exception)
                    {
                        //Why is thrown at the end of。とりあえず無視
                    }
                }
                this.NeedRedraw = false;
            }
        }

        /// <summary>
        /// To draw a sprite
        /// </summary>
        public void Draw()
        {
            BlendState lastBlendState = this.Context.DeviceManager.Context.OutputMerger.BlendState;
            Viewport[] lastViewports = this.Context.DeviceManager.Context.Rasterizer.GetViewports();
            //Pass the WVP matrix effect
            this.SpriteEffect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.ViewMatrix*this.SpriteProjectionMatrix);
            this.SpriteEffect.GetVariableBySemantic("TRANSCOLOR").AsVector().Set(this.TransparentColor);
            this.SpriteEffect.GetVariableBySemantic("SPRITETEXTURE").AsResource().SetResource(this.SpriteTexture);
            this.Context.DeviceManager.Context.InputAssembler.SetVertexBuffers(0,new VertexBufferBinding(this.VertexBuffer,SpriteVertexLayout.SizeInBytes,0));
            this.Context.DeviceManager.Context.InputAssembler.InputLayout = this.VertexInputLayout;
            this.Context.DeviceManager.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.Context.DeviceManager.Context.OutputMerger.BlendState = this.state;
            this.Context.DeviceManager.Context.Rasterizer.SetViewports(this.spriteViewport);
            this.renderPass.Apply(this.Context.DeviceManager.Context);
            this.Context.DeviceManager.Context.Draw(6,0);
            this.Context.DeviceManager.Context.Rasterizer.SetViewports(lastViewports);
            this.Context.DeviceManager.Context.OutputMerger.BlendState = lastBlendState;
        }



        private RenderContext Context { get;set; }
        public void Dispose()
        {
            if (this.VertexBuffer != null && !this.VertexBuffer.Disposed) this.VertexBuffer.Dispose();
            if (this.VertexInputLayout != null && !this.VertexInputLayout.Disposed) this.VertexInputLayout.Dispose();
            if (this.SpriteEffect != null && !this.SpriteEffect.Disposed) this.SpriteEffect.Dispose();
            if (this.SpriteTexture != null && !this.SpriteTexture.Disposed) this.SpriteTexture.Dispose();
            if (this.mapedBitmap != null) this.mapedBitmap.Dispose();
            if (this.mapedGraphic != null) this.mapedGraphic.Dispose();
        }
    }
}
