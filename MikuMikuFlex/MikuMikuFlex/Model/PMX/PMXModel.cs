// ***********************************************************************
// Assembly         : MikuMikuFlex
// Author           : Lime
// Created          : 01-17-2014
//
// Last Modified By : Lime
// Last Modified On : 02-02-2014
// ***********************************************************************
// <copyright file="MMDModel.cs" company="MMF Development Team">
//     Copyright (c) MMF Development Team. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics;
using System.IO;
using MMDFileParser.PMXModelParser;
using MMF.Bone;
using MMF.Controls.Forms;
using MMF.MME;
using MMF.Morph;
using MMF.Motion;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

namespace MMF.Model.PMX
{
    /// <summary>
    /// Class MMDModel.
    /// </summary>
    public class PMXModel :ISubsetDivided,IMovable,IEdgeDrawable,IGroundShadowDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PMXModel"/> class.
        /// </summary>
        /// <param name="modeldata">The modeldata.</param>
        /// <param name="subResourceLoader">The sub resource loader.</param>
        /// <param name="filename">The filename.</param>
        public PMXModel(ModelData modeldata, ISubresourceLoader subResourceLoader, string filename)
        {
            this.Model = modeldata;
            this.SubresourceLoader = subResourceLoader;
            this.Transformer = new BasicTransformer();
            this.SubsetManager = new PMXSubsetManager(this,modeldata);
            //PhongConstantBuffer = new PhongShadingConstantBufferManager();
            this.ToonManager = new PMXToonTextureManager();
            this.SelfShadowColor = new Vector4(0, 0, 0, 1);
            this.GroundShadowColor = new Vector4(0, 0, 0, 1);
            this.FileName = filename;
            this.Visibility = true;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="PMXModel"/> class from being created.
        /// </summary>
        private PMXModel()
        {
            
        }

        /// <summary>
        /// Interface to resolve the path of the resource, such as a texture
        /// </summary>
        /// <value>The subresource loader.</value>
        public ISubresourceLoader SubresourceLoader { get; private set; }

        /// <summary>
        /// Load model
        /// </summary>
        /// <value>The model.</value>
        public ModelData Model { get; private set; }


        /// <summary>
        /// Buffer management interface
        /// </summary>
        /// <value>The buffer manager.</value>
        public IBufferManager BufferManager { get; private set; }

        /// <summary>
        /// Subset management interface
        /// </summary>
        /// <value>The subset manager.</value>
        public ISubsetManager SubsetManager { get; private set; }

        /// <summary>
        /// Gets the morphmanager.
        /// </summary>
        /// <value>The morphmanager.</value>
        public IMorphManager Morphmanager { get; private set; }


        /// <summary>
        /// MME effects to use to draw the
        /// </summary>
        /// <value>The effect.</value>
        public MMEEffectManager Effect { get; private set; }



       // public PhongShadingConstantBufferManager PhongConstantBuffer { get; private set; }

        /// <summary>
        /// Interface will manage the Toon
        /// </summary>
        /// <value>The toon manager.</value>
        public IToonTextureManager ToonManager { get; private set; }

        /// <summary>
        /// Render context
        /// </summary>
        /// <value>The render context.</value>
        public RenderContext RenderContext { get; private set; }

        /// <summary>
        /// Gets or sets the z plot pass.
        /// </summary>
        /// <value>The z plot pass.</value>
        private EffectPass ZPlotPass { get; set; }

        /// <summary>
        /// Initialize test
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        private bool IsInitialized { get; set; }

        #region IDrawableImplementation of the interface

        /// <summary>
        /// The ground shadow color
        /// </summary>
        private Vector4 groundShadowColor;
        //private PhongShadingConstantBufferInputLayout phongBuffer;
        /// <summary>
        /// The self shadow color
        /// </summary>
        private Vector4 selfShadowColor;
        /// <summary>
        /// To determine whether the listed variables
        /// </summary>
        /// <value><c>true</c> if visibility; otherwise, <c>false</c>.</value>
        public bool Visibility { get; set; }
        /// <summary>
        /// Variable to hold the file name
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Loads from effect file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void LoadFromEffectFile(string filePath)
        {
            this.Effect = MMEEffectManager.Load(filePath, this, this.RenderContext, this.SubresourceLoader);
        }

        /// <summary>
        /// The number of subsets
        /// </summary>
        /// <value>The subset count.</value>
        public int SubsetCount
        {
            get
            {
                return this.SubsetManager.SubsetCount;
            }
        }

        /// <summary>
        /// Number of vertices
        /// </summary>
        /// <value>The vertex count.</value>
        public int VertexCount
        {
            get
            {
                return this.BufferManager.VerticiesCount;
            }
        }

        /// <summary>
        /// When you draw the model call。
        /// </summary>
        public virtual void Draw()
        {
            this.Effect.ApplyAllMatrixVariables();
            this.Skinning.ApplyEffect(this.Effect.EffectFile);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(this.BufferManager.VertexBuffer, BasicInputLayout.SizeInBytes, 0));
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.BufferManager.IndexBuffer, Format.R32_UInt, 0);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.InputLayout = this.BufferManager.VertexLayout;
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.PrimitiveTopology =
                PrimitiveTopology.TriangleList;

            this.SubsetManager.DrawAll();
        }


        /// <summary>
        /// Draws the edge.
        /// </summary>
        public void DrawEdge()
        {
            this.Effect.ApplyAllMatrixVariables();
            this.Skinning.ApplyEffect(this.Effect.EffectFile);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(this.BufferManager.VertexBuffer, BasicInputLayout.SizeInBytes, 0));
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.BufferManager.IndexBuffer, Format.R32_UInt, 0);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.InputLayout = this.BufferManager.VertexLayout;
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.PrimitiveTopology =
                PrimitiveTopology.TriangleList;
            this.SubsetManager.DrawEdges();
        }

        /// <summary>
        /// Draws the ground shadow.
        /// </summary>
        public void DrawGroundShadow()
        {
            this.Effect.ApplyAllMatrixVariables();
            this.Skinning.ApplyEffect(this.Effect.EffectFile);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(this.BufferManager.VertexBuffer, BasicInputLayout.SizeInBytes, 0));
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.BufferManager.IndexBuffer, Format.R32_UInt, 0);
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.InputLayout = this.BufferManager.VertexLayout;
            this.RenderContext.DeviceManager.Device.ImmediateContext.InputAssembler.PrimitiveTopology =
                PrimitiveTopology.TriangleList;
            this.SubsetManager.DrawGroundShadow();
        }

        /// <summary>
        /// Called when the model is updated
        /// </summary>
        public void Update()
        {
            if(this.BufferManager!=null) this.BufferManager.RecreateVerticies();
            this.Morphmanager.UpdateFrame();
            this.Skinning.UpdateSkinning(this.Morphmanager);
            foreach (var pmxSubset in this.SubsetManager.Subsets)
            {
                pmxSubset.MaterialInfo.UpdateMaterials();
            }
        }

        /// <summary>
        /// It is called for each 1000 / 30ms, to update the frame.
        /// </summary>
        public virtual void ApplyMove()
        {
        }
        /// <summary>
        /// Load model. 
        /// Load the specified render context.
        /// </summary>
        /// <param name="renderContext">The render context.</param>
        public void Load(RenderContext renderContext)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            this.RenderContext = renderContext;
            Device device = this.RenderContext.DeviceManager.Device;
            if (!this.IsInitialized)
            {
                //Do not reset the load models, such as Shader loading
                //load toon..
                this.ToonManager.Initialize(this.RenderContext, this.SubresourceLoader);

                this.SubsetManager.Initialze(this.RenderContext, this.Effect, this.SubresourceLoader, this.ToonManager);
                this.Effect = InitializeEffect();
                this.SubsetManager.ResetEffect(this.Effect);

                //Constant buffer
                //PhongConstantBuffer.Initialize(device, Effect.EffectFile,
                //    PhongShadingConstantBufferInputLayout.SizeInBytes, new PhongShadingConstantBufferInputLayout());

                //z 绘图
                this.ZPlotPass = this.Effect.EffectFile.GetTechniqueByIndex(1).GetPassByIndex(0);

                //加载绘图信息到device
                InitializeBuffers(device);

                // 设置骨架，刚体信息和关节信息
                this.Skinning = InitializeSkinning();

                //变形
                this.Morphmanager=new PMXMorphManager(this);
                this.IsInitialized = true;
            }
            this.MotionManager = InitializeMotionManager();
            InitializeOther(device);
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
        }


        //public Task LoadAsync(RenderContext renderContext)
        //{
        //    Task task=new Task(() => Load(renderContext));
        //    task.Start();
        //    return task;
        //}


        #endregion

        #region Initialize system

        /// <summary>
        /// Loads the effect.
        /// </summary>
        /// <param name="effectFile">The effect file.</param>
        public void LoadEffect(string effectFile)
        {
            if (this.Effect != null) this.Effect.Dispose();
            if (string.IsNullOrEmpty(effectFile))
            {
                this.Effect = MMEEffectManager.Load(@"Shader\DefaultShader.fx", this, this.RenderContext, this.SubresourceLoader);
            }
            else
            {
                this.Effect = MMEEffectManager.Load(effectFile, this, this.RenderContext, this.SubresourceLoader);
            }
            this.SubsetManager.ResetEffect(this.Effect);
        }

        /// <summary>
        /// Initializes the skinning.
        /// </summary>
        /// <returns>ISkinningProvider.</returns>
        protected virtual ISkinningProvider InitializeSkinning()
        {
            PMXSkeleton skel=new PMXSkeleton(this.Model);
            return skel;
        }

        /// <summary>
        /// Initializes the motion manager.
        /// </summary>
        /// <returns>IMotionManager.</returns>
        protected virtual IMotionManager InitializeMotionManager()
        {
            BasicMotionManager mm = new BasicMotionManager(this.RenderContext);
            mm.Initialize(this.Model, this.Morphmanager, this.Skinning, this.BufferManager);
            ((PMXSkeleton) this.Skinning).KinematicsProviders.Insert(0,mm);
            return mm;
        }

        /// <summary>
        /// Initializes the effect.
        /// </summary>
        /// <returns>MMEEffectManager.</returns>
        protected virtual MMEEffectManager InitializeEffect()
        {
            return MMEEffectManager.LoadFromResource(@"MMF.Resource.Shader.DefaultShader.fx", this, this.RenderContext, this.SubresourceLoader);
        }

        /// <summary>
        /// Initializes the other.
        /// </summary>
        /// <param name="device">The device.</param>
        protected virtual void InitializeOther(Device device)
        {
        }

        /// <summary>
        /// Initializes the buffers.
        /// </summary>
        /// <param name="device">The device.</param>
        protected virtual void InitializeBuffers(Device device)
        {
            this.BufferManager = new PMXModelBufferManager();
            this.BufferManager.Initialize(this.Model, device, this.Effect.EffectFile);
        }

        #endregion

        /// <summary>
        /// Color of the shadow self
        /// </summary>
        /// <value>The color of the self shadow.</value>
        public Vector4 SelfShadowColor
        {
            get { return this.selfShadowColor; }
            set
            {
                this.selfShadowColor = value;
            }
        }

        /// <summary>
        /// Ground shadow color
        /// </summary>
        /// <value>The color of the ground shadow.</value>
        public Vector4 GroundShadowColor
        {
            get { return this.groundShadowColor; }
            set
            {
                this.groundShadowColor = value;
            }
        }

        /// <summary>
        /// Interface to manage the model position and rotation
        /// </summary>
        /// <value>The transformer.</value>
        public ITransformer Transformer { get; private set; }

        /// <summary>
        ///Performs application-defined tasks that are associated with the release and reset of Unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Effect.Dispose();
            this.BufferManager.Dispose();
            this.ToonManager.Dispose();
            this.SubsetManager.Dispose();
        }

        /// <summary>
        /// Motion control class
        /// </summary>
        /// <value>The motion manager.</value>
        public IMotionManager MotionManager { get; private set; }

        /// <summary>
        /// Skinning interface
        /// </summary>
        /// <value>The skinning.</value>
        public ISkinningProvider Skinning { get; private set; }

        #region Static initialization convenience methods

        
        #region FromFile
        /// <summary>
        /// Open the model file
        /// </summary>
        /// <param name="filePath">PMXThe file path, the texture is loaded from the same folder</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel FromFile(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);
            return FromFile(filePath, folder);
        }

        /// <summary>
        /// Open the model file
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <param name="textureFolder">Load the textures folder</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel FromFile(string filePath, string textureFolder)
        {
            return FromFile(filePath, new BasicSubresourceLoader(textureFolder));
        }

        /// <summary>
        /// Open the model file
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <param name="loader">Texture path resolution interface</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel FromFile(string filePath, ISubresourceLoader loader)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                return new PMXModel(ModelData.GetModel(fs),loader, Path.GetFileName(filePath));
            }
        }
        #endregion

        #region OpenLoad

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <param name="context">The rendering context</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel OpenLoad(string filePath,RenderContext context)
        {
            PMXModel model = FromFile(filePath);
            model.Load(context);
            return model;
        }

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="textureFolder">The textures folder</param>
        /// <param name="context">The rendering context</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel OpenLoad(string filePath, string textureFolder, RenderContext context)
        {
            PMXModel model = FromFile(filePath, textureFolder);
            model.Load(context);
            return model;
        }

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="loader">Solution path, such as texture interface</param>
        /// <param name="context">The rendering context</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel OpenLoad(string filePath, ISubresourceLoader loader, RenderContext context)
        {
            PMXModel model = FromFile(filePath, loader);
            model.Load(context);
            return model;
        }

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <param name="panel">Render Panel</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel OpenLoad(string filePath, RenderControl panel)
        {
            PMXModel model = FromFile(filePath);
            model.Load(panel.RenderContext);
            return model;
        }

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="textureFolder">The textures folder</param>
        /// <param name="panel">Render Panel</param>
        /// <returns>MMDModelFor instance</returns>
        public static PMXModel OpenLoad(string filePath, string textureFolder, RenderControl panel)
        {
            PMXModel model = FromFile(filePath, textureFolder);
            model.Load(panel.RenderContext);
            return model;
        }

        /// <summary>
        /// Open the initialize
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="loader">Solution path, such as texture interface</param>
        /// <param name="panel">The rendering context</param>
        /// <returns>MMDModel.</returns>
        public static PMXModel OpenLoad(string filePath, ISubresourceLoader loader, RenderControl panel)
        {
            PMXModel model = FromFile(filePath, loader);
            model.Load(panel.RenderContext);
            return model;
        }
        #endregion

        /*
        #region OpenLoadAsync
        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, RenderContext context)
        {
            Task<MMDModel> task=new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath);
                model.Load(context);
                return model;
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="textureFolder">The textures folder</param>
        /// <param name="context">The rendering context</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, string textureFolder, RenderContext context)
        {
            Task<MMDModel> task = new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath, textureFolder);
                model.Load(context);
                return model;
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="loader">Solution path, such as texture interface</param>
        /// <param name="context">The rendering context</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, ISubresourceLoader loader, RenderContext context)
        {
            Task<MMDModel> task = new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath, loader);
                model.Load(context);
                return model;
            });
            task.Start();
            return task;

        }

        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXThe file path</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, RenderControl panel)
        {
            Task<MMDModel> task = new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath);
                model.Load(panel.RenderContext);
                return model;
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="textureFolder">The textures folder</param>
        /// <param name="panel">Render Panel</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, string textureFolder, RenderControl panel)
        {
            Task<MMDModel> task = new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath, textureFolder);
                model.Load(panel.RenderContext);
                return model;
            });
            task.Start();
            return task;

        }

        /// <summary>
        /// Open the initialize (asynchronous)
        /// </summary>
        /// <param name="filePath">PMXPath of the file</param>
        /// <param name="loader">Solution path, such as texture interface</param>
        /// <param name="panel">Render Panel</param>
        /// <returns>MMDModelFor instance</returns>
        public static Task<MMDModel> OpenLoadAsync(string filePath, ISubresourceLoader loader, RenderControl panel)
        {
            Task<MMDModel> task = new Task<MMDModel>(() =>
            {
                MMDModel model = FromFile(filePath, loader);
                model.Load(panel.RenderContext);
                return model;
            });
            task.Start();
            return task;
        }
        #endregion
        */

        #endregion


    }
}