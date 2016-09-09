using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMDFileParser.PMXModelParser;
using MMF.MME;
using SlimDX.Direct3D11;

namespace MMF.Model.PMX
{
    /// <summary>
    /// 子资源？
    /// </summary>
    internal class PMXSubsetManager : ISubsetManager
    {
        private readonly ModelData model;
        // private readonly BasicMaterialConstantBufferManager basicMaterialConstantBuffer = new BasicMaterialConstantBufferManager();
        private Device device;
        //private BasicMaterialConstantBufferInputLayout materialBuffer;
        private ISubresourceLoader subresourceManager;
        private IToonTextureManager toonManager;
        public List<ISubset> Subsets { get; private set; }

        public PMXSubsetManager(PMXModel drawable,ModelData model)
        {
            this.model = model;
            this.Drawable = drawable;

        }

        public void Initialze(RenderContext context, MMEEffectManager effectManager, ISubresourceLoader subresourceManager,
            IToonTextureManager ToonManager)
        {
            this.MMEEffect = effectManager;
            this.toonManager = ToonManager;
            this.Subsets = new List<ISubset>();
            this.device = context.DeviceManager.Device;
            this.subresourceManager = subresourceManager;
            ModelData model = this.model;
            int vertexSum = 0;
            for (int i = 0; i < model.MaterialList.MaterialCount; i++)
            {
                MaterialData material = model.MaterialList.Materials[i];//To create a subset of data
                PMXSubset dr = new PMXSubset(this.Drawable,material,i);
                dr.DoCulling = !material.bitFlag.HasFlag(RenderFlag.CullNone);
                dr.VertexCount = material.VertexNumber/3;
                dr.StartIndex = vertexSum;
                if (material.textureIndex >= this.toonManager.ResourceViews.Length)
                {
                    if (this.toonManager.ResourceViews.Length == 0)
                    {
                        int index = ToonManager.LoadToon(model.TextureList.TexturePathes[material.textureIndex]);
                        dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[index];
                        dr.MaterialInfo.IsToonUsed = false;
                    }
                    else
                    {
                        dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[0];
                        dr.MaterialInfo.IsToonUsed = false;
                    }
                }
                else
                {
                    if (material.ShareToonFlag == 1)
                    {
                        dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[material.textureIndex + 1];
                        dr.MaterialInfo.IsToonUsed = true;
                    }
                    else if (material.textureIndex != -1)
                    {
                        if (model.TextureList.TexturePathes.Count < material.textureIndex + 1)
                        {
                            dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[0];
                            dr.MaterialInfo.IsToonUsed = true;
                        }
                        else
                        {
                            int index = ToonManager.LoadToon(model.TextureList.TexturePathes[material.textureIndex]);
                            dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[index];
                            dr.MaterialInfo.IsToonUsed = true;
                        }
                    }
                    else
                    {
                        dr.MaterialInfo.ToonTexture = ToonManager.ResourceViews[0];
                        dr.MaterialInfo.IsToonUsed = true;
                    }
                }
                vertexSum += material.VertexNumber;

                //Load texture
                if (material.TextureTableReferenceIndex != -1)
                {
                    dr.MaterialInfo.MaterialTexture = GetSubresourceById(model.TextureList.TexturePathes[material.TextureTableReferenceIndex]);
                }
                //Loading of the sphere map
                if (material.SphereTextureTableReferenceIndex != -1)
                {
                    dr.MaterialInfo.SphereMode = material.SphereMode;
                    dr.MaterialInfo.MaterialSphereMap =
                        GetSubresourceById(model.TextureList.TexturePathes[material.SphereTextureTableReferenceIndex]);
                }
                this.Subsets.Add(dr);
            }
        }

        public void ResetEffect(MMEEffectManager effect)
        {
            this.MMEEffect=effect;
        }


        public void DrawAll()
        {
            for (int i = 0; i < this.Subsets.Count; i++)
            {
                UpdateConstantByMaterial(this.Subsets[i]);
                this.MMEEffect.ApplyEffectPass(this.Subsets[i], MMEEffectPassType.Object, (subset) => subset.Draw(this.device));

            }
        }

        public void DrawEdges()
        {
            //TODO边缘绘图的实现
            foreach (PMXSubset variable in from subset in Subsets where subset.MaterialInfo.isEdgeEnable select subset)
            {
                UpdateConstantByMaterial(variable);
                MMEEffect.ApplyEffectPass(variable, MMEEffectPassType.Edge, (subset) => subset.Draw(device));
            }
        }

        public void DrawGroundShadow()
        {
            //TODO地面影子的实现
            foreach (PMXSubset variable in from subset in Subsets where subset.MaterialInfo.isGroundShadowEnable select subset)
            {
                UpdateConstantByMaterial(variable);
                MMEEffect.ApplyEffectPass(variable, MMEEffectPassType.Shadow, (subset) => subset.Draw(device));
            }
        }

        public int SubsetCount
        {
            get
            {
                return this.Subsets.Count;
            }
        }

        public void Dispose()
        {
            foreach (PMXSubset drawableResource in this.Subsets)
            {
                drawableResource.Dispose();
            }
        }

        private void UpdateConstantByMaterial(ISubset ipmxSubset)
        {
            this.MMEEffect.ApplyAllMaterialVariables(ipmxSubset.MaterialInfo);
        }

        private ShaderResourceView GetSubresourceById(string p)
        {
            using (Stream stream = this.subresourceManager.getSubresourceByName(p))
            {
                if (stream == null) return null;
                return ShaderResourceView.FromStream(this.device, stream, (int) stream.Length);
            }
        }

        public IDrawable Drawable { get; set; }

        public MMEEffectManager MMEEffect { get; set; }
    }
}