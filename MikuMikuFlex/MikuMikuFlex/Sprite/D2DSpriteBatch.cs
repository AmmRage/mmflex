using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MMF.DeviceManager;
using MMF.Sprite.D2D;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using FeatureLevel = SlimDX.Direct2D.FeatureLevel;
using FontStyle = SlimDX.DirectWrite.FontStyle;
using Resource = SlimDX.DXGI.Resource;

namespace MMF.Sprite
{
    /// <summary>
    /// DirectUsing the 2-D sprites show class
    /// When writing large amounts of more than GDISpriteBatch, if the FPS display refresh speed inferior click here for more。
    /// Also impair VisualStudio graphical debugger that you GDISpriteBatch be used should。
    /// </summary>
    public class D2DSpriteBatch:IDisposable
    {
        /// <summary>
        /// Device Manager
        /// </summary>
        IDeviceManager DeviceManager { get; set; }

        /// <summary>
        /// DX11 side textures to use to display the sprite
        /// </summary>
        private Texture2D TextureD3D11 { get; set; }

        /// <summary>
        /// DX10 side textures to use to display the sprite
        /// </summary>
        private SlimDX.Direct3D10.Texture2D TextureD3D10 { get;  set; }

        /// <summary>
        /// DXEaedmutex 10 and DX11 texture of shared use
        /// </summary>
        private KeyedMutex MutexD3D10 { get; set; }

        /// <summary>
        /// DXKeyedMutex 10 and DX11 texture of shared use
        /// </summary>
        private KeyedMutex MutexD3D11 { get; set; }



        /// <summary>
        /// Drawable
        /// </summary>
        public RenderTarget DWRenderTarget { get; private set; }


        /// <summary>
        /// Sprite blend mode
        /// </summary>
        private BlendState TransParentBlendState { get; set; }

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

        internal RenderContext context;

        public Rectangle FillRectangle
        {
            get
            {
                return new Rectangle(0,0,(int) this.TextureSize.X,(int) this.TextureSize.Y);
            }
        }

        /// <summary>
        /// The destination vertex buffer the sprite texture
        /// </summary>
        private SlimDX.Direct3D11.Buffer VertexBuffer { get; set; }

        /// <summary>
        /// Sprite input layout//Oculus Rift開発キット
        /// </summary>
        private InputLayout VertexInputLayout { get; set; }

        /// <summary>
        /// Used to draw the sprite effects
        /// </summary>
        private Effect SpriteEffect { get; set; }

        /// <summary>
        /// The path to use to draw the sprite
        /// </summary>
        private EffectPass renderPass { get; set; }


        /// <summary>
        /// For the texture Sampler for sprites
        /// </summary>
        private SamplerState sampler { get; set; }

        /// <summary>
        /// DirectWriteInforms that the render target
        /// </summary>
        public event EventHandler<EventArgs> RenderTargetRecreated;

        public event EventHandler<EventArgs> BatchDisposing;

        public D2DSpriteBatch(RenderContext context)
        {
            this.context = context;
            this.DeviceManager = context.DeviceManager;
            this.SpriteEffect = CGHelper.CreateEffectFx5FromResource("MMF.Resource.Shader.SpriteShader.fx", this.DeviceManager.Device);
            this.renderPass = this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(1);
            this.VertexInputLayout = new InputLayout(this.DeviceManager.Device, this.SpriteEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, SpriteVertexLayout.InputElements);
            SamplerDescription desc = new SamplerDescription();
            desc.Filter = Filter.MinMagMipLinear;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;
            desc.AddressW = TextureAddressMode.Wrap;
            this.sampler = SamplerState.FromDescription(this.DeviceManager.Device, desc);
            BlendStateDescription blendDesc = new BlendStateDescription();
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
            this.state = BlendState.FromDescription(this.DeviceManager.Device, blendDesc);
            
            BlendStateDescription bsd=new BlendStateDescription();
            bsd.RenderTargets[0].BlendEnable = true;
            bsd.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
            bsd.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            bsd.RenderTargets[0].BlendOperation = BlendOperation.Add;
            bsd.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            bsd.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            bsd.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            bsd.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            this.TransParentBlendState = BlendState.FromDescription(this.DeviceManager.Device, bsd);
            Resize();
        }

        public void Resize()
        {
            Viewport vp = this.DeviceManager.Context.Rasterizer.GetViewports()[0];
            int width = (int) vp.Width;
            int height = (int) vp.Height;
            if(height==0||width==0)return;
            this.TextureSize = new Vector2(width, height);
            float w = width / 2f, h = height / 2f;
            List<byte> vertexBytes = new List<byte>();
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
                    SizeInBytes = (int)ds.Length
                };
                if (this.VertexBuffer != null && !this.VertexBuffer.Disposed) this.VertexBuffer.Dispose();
                this.VertexBuffer = new SlimDX.Direct3D11.Buffer(this.DeviceManager.Device, ds, bufDesc);
            }
            this.SpriteProjectionMatrix = Matrix.OrthoLH(width, height, 0, 100);
            this.spriteViewport = new Viewport()
            {
                Width = width,
                Height = height,
                MaxZ = 1
            };


            this.ViewMatrix = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            
            //DirectX11 for texture creation
            if (this.TextureD3D11 != null && !this.TextureD3D11.Disposed) this.TextureD3D11.Dispose();
            this.TextureD3D11 = new Texture2D(this.DeviceManager.Device, new Texture2DDescription()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.KeyedMutex
            });
            //Share DX10 and DX11 texture resources
            Resource sharedResource = new Resource(this.TextureD3D11);
            if (this.TextureD3D10 != null && !this.TextureD3D10.Disposed) this.TextureD3D10.Dispose();
            this.TextureD3D10 = this.DeviceManager.Device10.OpenSharedResource<SlimDX.Direct3D10.Texture2D>(sharedResource.SharedHandle);
            if (this.MutexD3D10 != null && !this.MutexD3D10.Disposed) this.MutexD3D10.Dispose();
            if (this.MutexD3D11 != null && !this.MutexD3D11.Disposed) this.MutexD3D11.Dispose();
            this.MutexD3D10 = new KeyedMutex(this.TextureD3D10);
            this.MutexD3D11 = new KeyedMutex(this.TextureD3D11);
            sharedResource.Dispose();
            Surface surface = this.TextureD3D10.AsSurface();
            RenderTargetProperties rtp = new RenderTargetProperties();
            rtp.MinimumFeatureLevel = FeatureLevel.Direct3D10;
            rtp.Type = RenderTargetType.Hardware;
            rtp.Usage = RenderTargetUsage.None;
            rtp.PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied);
            if (this.DWRenderTarget != null && !this.DWRenderTarget.Disposed) this.DWRenderTarget.Dispose();
            this.DWRenderTarget = RenderTarget.FromDXGI(this.context.D2DFactory, surface, rtp);
            surface.Dispose();
           
            if(RenderTargetRecreated!=null)RenderTargetRecreated(this,new EventArgs());
        }

        public void Begin()
        {
            if(this.TextureSize==Vector2.Zero)return;
            this.MutexD3D10.Acquire(0, 100);
            this.DWRenderTarget.BeginDraw();
            this.DWRenderTarget.Clear(new Color4(0,0,0,0));     
        }

        public void End()
        {
            if (this.TextureSize == Vector2.Zero) return;
            this.DWRenderTarget.EndDraw();
            this.MutexD3D10.Release(0);
            this.MutexD3D11.Acquire(0, 100);
            ShaderResourceView srv=new ShaderResourceView(this.DeviceManager.Device, this.TextureD3D11);
            BlendState lastBlendState = this.DeviceManager.Context.OutputMerger.BlendState;
            Viewport[] lastViewports = this.DeviceManager.Context.Rasterizer.GetViewports();
            //Pass the WVP matrix effect
            this.SpriteEffect.GetVariableBySemantic("WORLDVIEWPROJECTION")
                .AsMatrix()
                .SetMatrix(this.ViewMatrix *this.SpriteProjectionMatrix);
            this.SpriteEffect.GetVariableBySemantic("SPRITETEXTURE").AsResource().SetResource(srv);
            this.DeviceManager.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.VertexBuffer, SpriteVertexLayout.SizeInBytes, 0));
            this.DeviceManager.Context.InputAssembler.InputLayout = this.VertexInputLayout;
            this.DeviceManager.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceManager.Context.OutputMerger.BlendState = this.state;
            this.DeviceManager.Context.Rasterizer.SetViewports(this.spriteViewport);
            this.renderPass.Apply(this.DeviceManager.Context);
            this.DeviceManager.Context.Draw(6, 0);
            this.DeviceManager.Context.Rasterizer.SetViewports(lastViewports);
            this.DeviceManager.Context.OutputMerger.BlendState = lastBlendState;
            srv.Dispose();
            this.MutexD3D11.Release(0);
        }

        public void Dispose()
        {
            if (BatchDisposing != null) BatchDisposing(this, new EventArgs());
            if (this.TextureD3D10 != null && !this.TextureD3D10.Disposed) this.TextureD3D10.Dispose();
            if (this.TextureD3D11 != null && !this.TextureD3D11.Disposed) this.TextureD3D11.Dispose();
            if (this.MutexD3D10 != null && !this.MutexD3D10.Disposed) this.MutexD3D10.Dispose();
            if (this.MutexD3D11 != null && !this.MutexD3D11.Disposed) this.MutexD3D11.Dispose();
            if (this.DWRenderTarget != null && !this.DWRenderTarget.Disposed) this.DWRenderTarget.Dispose();
            if (this.VertexBuffer != null && !this.VertexBuffer.Disposed) this.VertexBuffer.Dispose();
            if (this.VertexInputLayout != null && !this.VertexInputLayout.Disposed) this.VertexInputLayout.Dispose();
            if (this.SpriteEffect != null && !this.SpriteEffect.Disposed) this.SpriteEffect.Dispose();
            if (this.sampler != null && !this.sampler.Disposed) this.sampler.Dispose();
            //Called when the following two will Dispose the RenderContext out bug。(画面真っ黒)
            //Can be predicted from this they Dispose the blend of the same type.。
            //Because of this, put on the list of plans to Dispose the RenderContext when processing
            this.context.Disposables.Add(this.TransParentBlendState);
            this.context.Disposables.Add(this.state);
        }


        public Vector2 TextureSize { get;private set; }

        /// <summary>
        /// Gets the specified solid color brush。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public D2DSpriteSolidColorBrush CreateSolidColorBrush(Color color)
        {
            return new D2DSpriteSolidColorBrush(this,color);
        }

        public D2DSpriteTextformat CreateTextformat(string fontFamiry, int size=15, FontWeight weight=FontWeight.Normal,
            FontStyle style=FontStyle.Normal, FontStretch stretch=FontStretch.Normal, string locale="ja-jp")
        {
           return new D2DSpriteTextformat(this,fontFamiry,size,weight,style,stretch,locale);
        }

        public D2DSpriteBitmap CreateBitmap(string fileName)
        {
            return new D2DSpriteBitmap(this,File.OpenRead(fileName));
        }

        public D2DSpriteBitmap CreateBitmap(Stream fs)
        {
            return new D2DSpriteBitmap(this,fs);
        }

        public D2DSpriteBitmapBrush CreateBitmapBrush(string fileName,BitmapBrushProperties bbp=new BitmapBrushProperties())
        {
            return new D2DSpriteBitmapBrush(this,CreateBitmap(fileName),bbp);
        }

        public D2DSpriteBitmapBrush CreateBitmapBrush(Stream fileStream, BitmapBrushProperties bbp = new BitmapBrushProperties())
        {
            return new D2DSpriteBitmapBrush(this, CreateBitmap(fileStream), bbp);
        }

        public D2DSpriteLinearGradientBrush CreateLinearGradientBrush(D2DSpriteGradientStopCollection collection,
            LinearGradientBrushProperties gradient)
        {
            return new D2DSpriteLinearGradientBrush(this,collection,gradient);
        }

        public D2DSpriteGradientStopCollection CreateGradientStopCollection(GradientStop[] stops)
        {
            return new D2DSpriteGradientStopCollection(this,stops,Gamma.Linear,ExtendMode.Mirror);
        }

        public D2DSpriteRadialGradientBrush CreateRadialGradientBrush(D2DSpriteGradientStopCollection collection,
            RadialGradientBrushProperties r)
        {
            return new D2DSpriteRadialGradientBrush(this,collection.GradientStopCollection,r);
        }
    }
}
