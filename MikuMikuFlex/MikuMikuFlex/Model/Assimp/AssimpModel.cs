using System.Collections.Generic;
using System.IO;
using Assimp;
using Assimp.Configs;
using MMF.MME;
using SlimDX;
using SlimDX.Direct3D11;

namespace MMF.Model.Assimp
{
    /// <summary>
    /// Class AssimpModel.
    /// </summary>
    /// <remarks>Assimp is a portable Open Source library to import various well-known 3D model formats in a uniform manner.</remarks>
    public class AssimpModel:IDrawable,ISubsetDivided
    {
        private Scene modelScene;

        private ISubresourceLoader loader;

        private RenderContext context;

        private List<ISubset> subsets=new List<ISubset>();

        private MMEEffectManager effectManager;

        private InputLayout layout;

        public AssimpModel(RenderContext context,string fileName)
        {
            this.FileName = Path.GetFileName(fileName);
            this.context = context;
            this.loader=new BasicSubresourceLoader(Path.GetDirectoryName(fileName));

            

            AssimpImporter importer=new AssimpImporter();
            this.modelScene=importer.ImportFile(fileName,PostProcessSteps.Triangulate|PostProcessSteps.GenerateSmoothNormals);
            this.Visibility = true;
            Initialize();
        }

        /// <summary>
        /// Initialize the AssimpModel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileName"></param>
        /// <param name="modelScene">PostProcessSteps.Triangulate|PostProcessSteps.GenerateNormalsを指定して読み込んだSceneである必要があります。</param>
        /// <param name="loader"></param>
        public AssimpModel(RenderContext context,string fileName, Scene modelScene,ISubresourceLoader loader)
        {
            this.modelScene = modelScene;
            this.context = context;
            this.FileName = fileName;
            this.loader = loader;
            this.Visibility = true;
            Initialize();
        }

        private void Initialize()
        {
            this.Transformer=new BasicTransformer();
            for (int i = 0; i < this.modelScene.Meshes.Length; i++)
            {
                this.subsets.Add(new AssimpSubset(this.context, this.loader,this, this.modelScene,i));
            }
            this.effectManager = MMEEffectManager.LoadFromResource(@"MMF.Resource.Shader.DefaultShader.fx", this, this.context, this.loader);
            this.layout = new InputLayout(this.context.DeviceManager.Device, this.effectManager.EffectFile.GetTechniqueByIndex(1).GetPassByIndex(0).Description.Signature, BasicInputLayout.VertexElements);
        }

        /// <summary>
        /// Unmanaged リソースの解放およびリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
            if(this.layout!=null&&!this.layout.Disposed) this.layout.Dispose();
            this.effectManager.Dispose();
            foreach (var subset in this.subsets)
            {
                subset.Dispose();
            }
        }

        /// <summary>
        /// To determine whether the listed variables
        /// </summary>
        /// <value><c>true</c> if visibility; otherwise, <c>false</c>.</value>
        public bool Visibility { get;set; }

        /// <summary>
        /// Variable to hold the file name
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; private set; }

        /// <summary>
        /// The number of subsets
        /// </summary>
        /// <value>The subset count.</value>
        public int SubsetCount
        {
            get { return this.subsets.Count; }
        }

        /// <summary>
        /// Number of vertices
        /// </summary>
        /// <value>The vertex count.</value>
        public int VertexCount { get; private set; }
        /// <summary>
        /// When you move the model classes
        /// </summary>
        /// <value>The transformer.</value>
        public ITransformer Transformer { get; private set; }
        /// <summary>
        /// When you draw the model call。
        /// </summary>
        public void Draw()
        {
            this.effectManager.ApplyAllMatrixVariables();
            this.context.DeviceManager.Device.ImmediateContext.InputAssembler.InputLayout = this.layout;
            this.context.DeviceManager.Device.ImmediateContext.InputAssembler.PrimitiveTopology =
                PrimitiveTopology.TriangleList;
            this.effectManager.EffectFile.GetVariableBySemantic("BONETRANS")
                .AsMatrix()
                .SetMatrixArray(new Matrix[] {Matrix.Identity});
            foreach (var subset in this.subsets)
            {
                this.effectManager.ApplyAllMaterialVariables(subset.MaterialInfo);
                this.effectManager.ApplyEffectPass(subset, MMEEffectPassType.Object, (isubset) => isubset.Draw(this.context.DeviceManager.Device));
            }
        }

        /// <summary>
        /// Called when the model is updated
        /// </summary>
        public void Update()
        {
          
        }

        /// <summary>
        /// Self shadow color
        /// </summary>
        /// <value>The color of the self shadow.</value>
        public Vector4 SelfShadowColor { get; set; }

        /// <summary>
        /// Ground shadow color
        /// </summary>
        /// <value>The color of the ground shadow.</value>
        public Vector4 GroundShadowColor { get; set; }
    }
}
