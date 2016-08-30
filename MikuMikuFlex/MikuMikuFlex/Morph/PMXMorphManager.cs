using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMF.Model;
using MMF.Model.PMX;
using MMF.Motion;

namespace MMF.Morph
{
    /// <summary>
    /// PMXClass to manage the standard morph
    /// </summary>
    public class PMXMorphManager : IMorphManager
    {
        public PMXMorphManager(PMXModel model)
        {
            this.MMDMorphs.Add(new VertexMorphProvider(model.Model,model.BufferManager));
            this.MMDMorphs.Add(new BoneMorphProvider(model));
            this.MMDMorphs.Add(new MaterialMorphProvider(model));
            this.MMDMorphs.Add(new GroupMorphProvider(model,this));
            this.MMDMorphs.Add(new UVMorphProvider(model,MorphType.UV));
            this.MMDMorphs.Add(new UVMorphProvider(model,MorphType.UV_Additional1));
            this.MMDMorphs.Add(new UVMorphProvider(model,MorphType.UV_Additional2));
            this.MMDMorphs.Add(new UVMorphProvider(model, MorphType.UV_Additional3));
            this.MMDMorphs.Add(new UVMorphProvider(model, MorphType.UV_Additional4));
        }

        public List<IMorphProvider> MMDMorphs=new List<IMorphProvider>();

        private Dictionary<string,float> morphProgresses=new Dictionary<string, float>();

        public float getMorphProgress(string morphName)
        {
            return this.morphProgresses[morphName];
        }

        /// <summary>
        /// Motion morph from
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="morphs"></param>
        public void ApplyMorphProgress(float frameNumber, IEnumerable<MorphMotion> morphMotions)
        {
            foreach (var morphMotion in morphMotions)
            {
                if (this.morphProgresses.ContainsKey(morphMotion.MorphName))
                {
                    this.morphProgresses[morphMotion.MorphName] = morphMotion.GetMorphValue(frameNumber);
                }
                else
                {
                    this.morphProgresses.Add(morphMotion.MorphName,morphMotion.GetMorphValue(frameNumber));
                }
            }
            foreach (IMorphProvider mmdMorphManager in this.MMDMorphs)
            {
                mmdMorphManager.ApplyMorphProgress(frameNumber, morphMotions);
            }
        }

        /// <summary>
        /// Sets the frame of the morph
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="morphName"></param>
        public void ApplyMorphProgress(float frame, string morphName)
        {
            if (this.morphProgresses.ContainsKey(morphName))
            {
                this.morphProgresses[morphName] = frame;
            }
            else
            {
                this.morphProgresses.Add(morphName, frame);
            }
            foreach (IMorphProvider mmdMorphManager in this.MMDMorphs)
            {
                if(mmdMorphManager.ApplyMorphProgress(frame,morphName))return;
            }
        }

        public void UpdateFrame()
        {
            foreach (var mmdMorphProvider in this.MMDMorphs)
            {
                mmdMorphProvider.UpdateFrame();
            }
        }
    }
}
