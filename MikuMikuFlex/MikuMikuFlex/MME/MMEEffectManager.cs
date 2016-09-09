using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MMF.DeviceManager;
using MMF.MME.Includer;
using MMF.MME.VariableSubscriber;
using MMF.MME.VariableSubscriber.ConstantSubscriber;
using MMF.MME.VariableSubscriber.ControlInfoSubscriber;
using MMF.MME.VariableSubscriber.MaterialSubscriber;
using MMF.MME.VariableSubscriber.MatrixSubscriber;
using MMF.MME.VariableSubscriber.MouseSubscriber;
using MMF.MME.VariableSubscriber.PeculiarValueSubscriber;
using MMF.MME.VariableSubscriber.ScreenInfoSubscriber;
using MMF.MME.VariableSubscriber.TextureSubscriber;
using MMF.MME.VariableSubscriber.TimeSubscriber;
using MMF.MME.VariableSubscriber.WorldInfoSubscriber;
using MMF.Model;
using MMF.Model.PMX;
using MMF.Utility;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace MMF.MME
{
    /// <summary>
    ///     MMEOf the class to manage effects
    /// </summary>
    public class MMEEffectManager : IDisposable
    {
        private static string applicationDefinition = "MMFApp";

        /// <summary>
        ///     Target model
        /// </summary>
        private readonly IDrawable model;

        private string fileName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="context"></param>
        /// <param name="effect">Effects of being interpreted</param>
        /// <param name="model">Want to use models</param>
        /// <param name="loader"></param>
        private MMEEffectManager(string fileName, RenderContext context, Effect effect, IDrawable model,ISubresourceLoader loader)
        {
            this.fileName = fileName;
            this.SubresourceLoader = loader;
            if (!fileName.Equals(MMFDefaultShaderResourcePath))
            {
                //The default shaders loaded and the default file path is not equal:。
                this.DefaultShader = LoadFromResource(MMFDefaultShaderResourcePath, model, context,new BasicSubresourceLoader("Shader"));
            }
            else
            {
                this.DefaultShader = this; //Equal to itself is equal to the default shader。
            }
            this.Context = context;
            this.EffectFile = effect;
            this.EffectInfo = new MMEEffectInfo(effect);
            this.ActiveSubscriberByMaterial = new Dictionary<EffectVariable, SubscriberBase>();
            this.ActiveSubscriberByModel = new Dictionary<EffectVariable, SubscriberBase>();
            this.ActivePeculiarSubscriber = new Dictionary<EffectVariable, PeculiarValueSubscriberBase>();
            this.Techniques = new List<MMEEffectTechnique>();
            this.RenderColorTargetViewes=new Dictionary<string, RenderTargetView>();
            this.RenderDepthStencilTargets=new Dictionary<string, DepthStencilView>();
            this.model = model;
            //Creating global variables
            int valCount = effect.Description.GlobalVariableCount;
            for (int i = 0; i < valCount; i++)
            {
                string semantic = Regex.Replace(effect.GetVariableByIndex(i).Description.Semantic,"[0-9]","");
                string semanticIndexStr = Regex.Replace(effect.GetVariableByIndex(i).Description.Semantic, "[^0-9]", "");
                int semanticIndex =string.IsNullOrEmpty(semanticIndexStr)?0:int.Parse(semanticIndexStr);
                string typeName = effect.GetVariableByIndex(i).GetVariableType().Description.TypeName.ToLower();
                semantic = semantic.ToUpper(); //A good case for all converted to uppercase
                if (EffectSubscriber.ContainsKey(semantic))
                {//Usually the register according to the semantics
                    SubscriberBase subs = EffectSubscriber[semantic];
                    EffectVariable variable = effect.GetVariableByIndex(i);
                    subs.CheckType(variable);
                    if (subs.UpdateTiming == UpdateBy.Material)
                    {
                        this.ActiveSubscriberByMaterial.Add(variable, subs.GetSubscriberInstance(variable, context, this, semanticIndex));
                    }
                    else
                    {
                        this.ActiveSubscriberByModel.Add(variable, subs.GetSubscriberInstance(variable, context, this, semanticIndex));
                    }
                }else if (typeName.Equals("texture") || typeName.Equals("texture2d") || typeName.Equals("texture3d") ||
                          typeName.Equals("texturecube"))//Textures only in exceptions, depending on the variable type to register
                {
                    SubscriberBase subs=new TextureSubscriber();
                    EffectVariable variable = effect.GetVariableByIndex(i);
                    subs.CheckType(variable);
                    this.ActiveSubscriberByModel.Add(variable,subs.GetSubscriberInstance(variable,context,this, semanticIndex));
                }
                else//Special variable to register according to the variable name
                {
                    string name = effect.GetVariableByIndex(i).Description.Name;
                    name = name.ToLower();
                    if (PeculiarEffectSubscriber.ContainsKey(name))
                    {
                        this.ActivePeculiarSubscriber.Add(effect.GetVariableByIndex(i), PeculiarEffectSubscriber[name]);
                    }
                }
            }
            //Constant buffer register
            valCount = effect.Description.ConstantBufferCount;
            for (int i = 0; i < valCount; i++)
            {
                string name = effect.GetConstantBufferByIndex(i).Description.Name;
                name = name.ToUpper();
                if (EffectSubscriber.ContainsKey(name))
                {
                    SubscriberBase subs = EffectSubscriber[name]; //A constant buffer semantics, so instead the variable name
                    EffectConstantBuffer variable = effect.GetConstantBufferByIndex(i);
                    subs.CheckType(variable);
                    if (subs.UpdateTiming == UpdateBy.Material)
                    {
                        this.ActiveSubscriberByMaterial.Add(variable, subs.GetSubscriberInstance(variable, context, this, 0));
                    }
                    else
                    {
                        this.ActiveSubscriberByModel.Add(variable, subs.GetSubscriberInstance(variable, context, this, 0));
                    }
                }
            }

            int subsetCount = model is ISubsetDivided ? ((ISubsetDivided)model).SubsetCount : 1;
            foreach (EffectTechnique t in this.EffectInfo.SortedTechnique)//MMEEffectTechnique are sorted by
            {
                //All techniques will be read
                this.Techniques.Add(new MMEEffectTechnique(this, t, subsetCount, context));
            }
        }

        #region Static parts

        /// <summary>
        ///     MMF to define effect constants
        /// </summary>
        public const string MMFDefinition = "MMF";

        /// <summary>
        ///     Path of the default shader
        /// </summary>
        public const string MMFDefaultShaderResourcePath = "MMF.Resource.Shader.DefaultShader.fx";

        /// <summary>
        ///     Effects of variable registration class list
        /// </summary>
        public static EffectSubscriberDictionary EffectSubscriber { get; private set; }

        public static PeculiarEffectSubscriberDictionary PeculiarEffectSubscriber { get; private set; }

        /// <summary>
        ///     List of macros defined in the effect
        /// </summary>
        public static List<ShaderMacro> EffectMacros { get; private set; }

        /// <summary>
        ///     Effects include Include list
        /// </summary>
        public static Include EffectInclude { get; private set; }

        /// <summary>
        ///     Application effect in defining constants
        /// </summary>
        public static string ApplicationDefinition
        {
            get { return applicationDefinition; }
            set
            {
                if (applicationDefinition != value)
                {
//If values different from the previous set, fool the list of macros
                    EffectMacros.Remove(new ShaderMacro(applicationDefinition));
                    applicationDefinition = value;
                    EffectMacros.Add(new ShaderMacro(value));
                }
            }
        }

        #endregion

        private RenderContext Context { get; set; }

        public MMEEffectInfo EffectInfo { get; private set; }
        
        /// <summary>
        /// Register instance variables should be listed separately for each model
        /// </summary>
        public Dictionary<EffectVariable, SubscriberBase> ActiveSubscriberByModel { get; private set; }

        /// <summary>
        /// Register instance variables should be listed separately for each material
        /// </summary>
        public Dictionary<EffectVariable, SubscriberBase> ActiveSubscriberByMaterial { get; private set; }

        /// <summary>
        /// Special variable type in the variable name to determine the registered instance
        /// </summary>
        public Dictionary<EffectVariable, PeculiarValueSubscriberBase> ActivePeculiarSubscriber { get; private set; }

        public Dictionary<string,RenderTargetView> RenderColorTargetViewes { get; private set; } 

        public Dictionary<string,DepthStencilView> RenderDepthStencilTargets { get; private set; } 
        /// <summary>
        ///     List of techniques described in the effect
        /// </summary>
        public List<MMEEffectTechnique> Techniques { get; private set; }

        /// <summary>
        /// Loader to use to resolve paths, such as texture
        /// </summary>
        public ISubresourceLoader SubresourceLoader { get; private set; }

        /// <summary>
        ///     To use the effect file body
        /// </summary>
        private Effect effect { get; set; }

        /// <summary>
        ///     The default shader
        /// </summary>
        public MMEEffectManager DefaultShader { get; private set; }


        /// <summary>
        ///     Effect file
        /// </summary>
        public Effect EffectFile
        {
            get { return this.effect; }
            private set { this.effect = value; }
        }

        /// <summary>
        ///     Discard
        /// </summary>
        public void Dispose()
        {
            this.EffectFile.Dispose();
            foreach (KeyValuePair<EffectVariable, SubscriberBase> subscriberAnnotation in this.ActiveSubscriberByMaterial)
            {
                if (subscriberAnnotation.Value is IDisposable)
                {
                    IDisposable disposeTarget = (IDisposable) subscriberAnnotation.Value;
                    if (disposeTarget != null) disposeTarget.Dispose();
                }
            }
            foreach (KeyValuePair<EffectVariable, SubscriberBase> subscriberBase in this.ActiveSubscriberByModel)
            {
                if (subscriberBase.Value is IDisposable)
                {
                    IDisposable disposeTarget = (IDisposable)subscriberBase.Value;
                    if (disposeTarget != null) disposeTarget.Dispose();
                }
            }
        }

        internal static void IniatializeMMEEffectManager(IDeviceManager deviceManager)
        {
            //Effects of variable registration class registration
            EffectSubscriber = new EffectSubscriberDictionary
            {
                new WorldMatrixSubscriber(),
                new ProjectionMatrixSubscriber(),
                new ViewMatrixSubscriber(),
                new WorldInverseMatrixSubscriber(),
                new WorldTransposeMatrixSubscriber(),
                new WorldInverseTransposeMatrixSubscriber(),
                new ViewInverseMatrixSubscriber(),
                new ViewTransposeMatrixSubscriber(),
                new ViewInverseTransposeMatrixSubsriber(),
                new ProjectionInverseMatrixSubscriber(),
                new ProjectionTransposeMatrixSubscriber(),
                new ProjectionInverseTransposeMatrixSubscriber(),
                new WorldViewMatrixSubscriber(),
                new WorldViewInverseMatrixSubscriber(),
                new WorldViewTransposeMatrixSubscriber(),
                new ViewProjectionMatrixSubscriber(),
                new ViewProjectionInverseMatrixSubscriber(),
                new ViewProjectionTransposeMatrixSubscriber(),
                new ViewProjectionInverseTransposeMatrixSubscriber(),
                new WorldViewProjectionMatrixSubscriber(),
                new WorldViewProjectionInverseMatrixSubscriber(),
                new WorldViewProjectionTransposeMatrixSubscriber(),
                new WorldViewProjectionInverseTransposeMatrixSubscriber(),
                //Material
                new DiffuseVectorSubscriber(),
                new AmbientVectorSubscriber(),
                new SpecularVectorSubscriber(),
                new SpecularPowerSubscriber(),
                new ToonVectorSubscriber(),
                new EdgeVectorSubscriber(),
                new GroundShadowColorVectorSubscriber(),
                new MaterialTextureSubscriber(),
                new MaterialSphereMapSubscriber(),
                new MaterialToonTextureSubscriber(),
                new AddingTextureSubscriber(),
                new MultiplyingTextureSubscriber(),
                new AddingSphereTextureSubscriber(),
                new MultiplyingSphereTextureSubscriber(),
                new EdgeThicknessSubscriber(),
                //Position/方向
                new PositionSubscriber(),
                new DirectionSubscriber(),
                //Time
                new TimeSubScriber(),
                new ElapsedTimeSubScriber(),
                //Mouse
                new MousePositionSubscriber(),
                new LeftMouseDownSubscriber(),
                new MiddleMouseDownSubscriber(),
                new RightMouseDownSubscriber(),
                //Screen information
                new ViewPortPixelSizeScriber(),
                //Constant buffer
                new BasicMaterialConstantSubscriber(),
                new FullMaterialConstantSubscriber(),
                //Control object
                new ControlObjectSubscriber(),
                new RenderColorTargetSubscriber(),
                new RenderDepthStencilTargetSubscriber(),
            };
            PeculiarEffectSubscriber = new PeculiarEffectSubscriberDictionary
            {
                new OpAddSubscriber(),
                new ParthfSubscriber(),
                new SpAddSubscriber(),
                new SubsetCountSubscriber(),
                new TranspSubscriber(),
                new Use_SpheremapSubscriber(),
                new Use_TextureSubscriber(),
                new Use_ToonSubscriber(),
                new VertexCountSubscriber()
            };
            //Initialize the effect compile-time macros
            EffectMacros = new List<ShaderMacro>();
            EffectMacros.Add(new ShaderMacro(MMFDefinition)); //Define constant MMF
            EffectMacros.Add(new ShaderMacro(ApplicationDefinition)); //Application of constant definition
            EffectMacros.Add(new ShaderMacro("MMM_LightCount", "3"));
            if (deviceManager.DeviceFeatureLevel == FeatureLevel.Level_11_0)
            {
                EffectMacros.Add(new ShaderMacro("DX_LEVEL_11_0"));
            }
            else
            {
                EffectMacros.Add(new ShaderMacro("DX_LEVEL_10_1"));
            }
            EffectInclude = new BasicEffectIncluder();
        }

        /// <summary>
        ///     Assign all the necessary variables for the effect
        /// </summary>
        public void ApplyAllMatrixVariables()
        {
            SubscribeArgument argument = new SubscribeArgument(this.model, this.Context);
            foreach (KeyValuePair<EffectVariable, SubscriberBase> subscriberBase in this.ActiveSubscriberByModel)
            {
                subscriberBase.Value.Subscribe(subscriberBase.Key, argument);
            }
        }

        /// <summary>
        ///     To assign a variable of all necessary material
        /// </summary>
        /// <param name="info">Use of material information</param>
        public void ApplyAllMaterialVariables(MaterialInfo info)
        {
            
            SubscribeArgument argument = new SubscribeArgument(info, this.model, this.Context);
            foreach (KeyValuePair<EffectVariable, SubscriberBase> item in this.ActiveSubscriberByMaterial)
            {
                item.Value.Subscribe(item.Key, argument);
            }
            foreach (
                KeyValuePair<EffectVariable, PeculiarValueSubscriberBase> peculiarValueSubscriberBase in this.ActivePeculiarSubscriber)
            {
                peculiarValueSubscriberBase.Value.Subscribe(peculiarValueSubscriberBase.Key,
                    argument);
            }
        }

        /// <summary>
        ///     Assign the effect pass
        /// </summary>
        /// <param name="ipmxSubset"></param>
        public void ApplyEffectPass(ISubset ipmxSubset, MMEEffectPassType passType, Action<ISubset> drawAction)
        {
            if(ipmxSubset.MaterialInfo.DiffuseColor.W==0)return;
            //Switch a culling value depending on whether double-sided drawing
            if (ipmxSubset.DoCulling)
            {
                this.Context.DeviceManager.Context.Rasterizer.State = this.Context.CullingRasterizerState;
            }
            else
            {
                this.Context.DeviceManager.Context.Rasterizer.State = this.Context.NonCullingRasterizerState;
            }
            //To find a technique to use
            MMEEffectTechnique[] techniques = (from teq in this.Techniques
                where
                    teq.Subset.Contains(ipmxSubset.SubsetId) && teq.MMDPassAnnotation == passType &&
                    MMEEffectTechnique.CheckExtebdedBoolean(teq.UseToon, ipmxSubset.MaterialInfo.IsToonUsed) &&
                    MMEEffectTechnique.CheckExtebdedBoolean(teq.UseTexture, ipmxSubset.MaterialInfo.MaterialTexture != null) &&
                    MMEEffectTechnique.CheckExtebdedBoolean(teq.UseSphereMap,
                        ipmxSubset.MaterialInfo.MaterialSphereMap != null) && MMEEffectTechnique.CheckExtebdedBoolean(teq.MulSphere, ipmxSubset.MaterialInfo.SphereMode == MMDFileParser.PMXModelParser.SphereMode.Multiply)
                select teq).ToArray();
            foreach (MMEEffectTechnique technique in techniques)
            {
                technique.ExecuteTechnique(this.Context.DeviceManager.Context,drawAction,ipmxSubset);
                return;
            }
        }

        /// <summary>
        ///     MMEThe load effect as
        /// </summary>
        /// <param name="str">Effect pass</param>
        /// <param name="model">Want to use models</param>
        /// <param name="context"></param>
        /// <param name="loader"></param>
        /// <param name="device">Device</param>
        /// <returns>MMEEffect of specifications</returns>
        public static MMEEffectManager Load(string str, IDrawable model, RenderContext context,ISubresourceLoader loader)
        {
            return new MMEEffectManager(str, context, CGHelper.CreateEffectFx5(str, context.DeviceManager.Device), model, loader);
        }

        /// <summary>
        ///     MMEThe load effect as
        /// </summary>
        /// <param name="str">Effect pass</param>
        /// <param name="model">Want to use models</param>
        /// <param name="context"></param>
        /// <param name="loader"></param>
        /// <returns>MMEEffect of specifications</returns>
        internal static MMEEffectManager LoadFromResource(string str, IDrawable model, RenderContext context,ISubresourceLoader loader)
        {
            Effect effect = CGHelper.CreateEffectFx5FromResource(str, context.DeviceManager.Device);
            return new MMEEffectManager(str, context, effect, model,loader);
        }
    }
}