using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MMF.Model;
using MMF.Model.PMX;
using MMF.Motion;
using SlimDX;

namespace MMF.Morph
{
    public class UVMorphProvider:IMorphProvider
    {
        public Dictionary<string,UVMorphData> Morphs=new Dictionary<string, UVMorphData>();

        private ModelData model;

        private IBufferManager bufferManager;

        private MorphType targetMorph;

        public UVMorphProvider(PMXModel model,MorphType targetType)
        {
            this.bufferManager = model.BufferManager;
            this.targetMorph = targetType;
            this.model = model.Model;
            if(model.Model.Header.AdditionalUVCount+2<=(int)targetType)return;//Does not exist at this time for additional UV
            foreach (MorphData morphData in model.Model.MorphList.Morphes)
            {
                if (morphData.type == this.targetMorph)
                {
                    this.Morphs.Add(morphData.MorphName,new UVMorphData(morphData));
                }
            }
        }

        public void ApplyMorphProgress(float frameNumber, IEnumerable<MorphMotion> morphMotions)
        {
            foreach (MorphMotion morphMotion in morphMotions)
            {
                SetMorphProgress(morphMotion.GetMorphValue(frameNumber), morphMotion.MorphName);
            }
        }

        public bool ApplyMorphProgress(float progress, string morphName)
        {
            return SetMorphProgress(progress, morphName);
        }

        public void UpdateFrame()
        {
            
        }

        private bool SetMorphProgress(float progress, string morphName)
        {
            if (!this.Morphs.ContainsKey(morphName)) return false;
            UVMorphData data = this.Morphs[morphName];
            foreach (UVMorphOffset uvMorphOffset in data.MorphOffsets)
            {
                switch (this.targetMorph)
                {
                    case MorphType.UV:
                        this.bufferManager.InputVerticies[uvMorphOffset.VertexIndex].UV = this.model.VertexList.Vertexes[uvMorphOffset.VertexIndex].UV+new Vector2(uvMorphOffset.UVOffset.X, uvMorphOffset.UVOffset.Y) * progress;
                        break;
                    case MorphType.UV_Additional1:
                        this.bufferManager.InputVerticies[uvMorphOffset.VertexIndex].AddUV1= this.model.VertexList.Vertexes[uvMorphOffset.VertexIndex].AdditionalUV[0] + uvMorphOffset.UVOffset * progress;
                        break;
                    case MorphType.UV_Additional2:
                        this.bufferManager.InputVerticies[uvMorphOffset.VertexIndex].AddUV2 = this.model.VertexList.Vertexes[uvMorphOffset.VertexIndex].AdditionalUV[1] + uvMorphOffset.UVOffset * progress;
                        break;
                    case MorphType.UV_Additional3:
                        this.bufferManager.InputVerticies[uvMorphOffset.VertexIndex].AddUV3 = this.model.VertexList.Vertexes[uvMorphOffset.VertexIndex].AdditionalUV[2] + uvMorphOffset.UVOffset * progress;
                        break;
                    case MorphType.UV_Additional4:
                        this.bufferManager.InputVerticies[uvMorphOffset.VertexIndex].AddUV4 = this.model.VertexList.Vertexes[uvMorphOffset.VertexIndex].AdditionalUV[3] + uvMorphOffset.UVOffset * progress;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("不適切なモーフタイプが渡されました");
                }

            }
            return true;
        }
    }
}
