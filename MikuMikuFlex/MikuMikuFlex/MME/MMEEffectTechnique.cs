using System;
using System.Collections.Generic;
using MMF.MME.Script;
using MMF.Model;
using SlimDX.Direct3D11;

namespace MMF.MME
{
    /// <summary>
    ///     MMEClass to manage the format of shader techniques
    /// </summary>
    public class MMEEffectTechnique
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="technique"></param>
        /// <param name="subsetCount"></param>
        /// <param name="context"></param>
        public MMEEffectTechnique(MMEEffectManager manager, EffectTechnique technique, int subsetCount, 
            RenderContext context)
        {
            this.Subset = new HashSet<int>();
            this.Passes = new Dictionary<string, MMEEffectPass>();
            if (!technique.IsValid)
                throw new InvalidMMEEffectShaderException(string.Format("テクニック「{0}」の検証に失敗しました。",
                    technique.Description.Name));
            //Loading MMDPass
            string mmdpass = EffectParseHelper.getAnnotationString(technique, "MMDPass");
            if (String.IsNullOrWhiteSpace(mmdpass))
            {
                this.MMDPassAnnotation = MMEEffectPassType.Object;
            }
            else
            {
                mmdpass = mmdpass.ToLower();
                switch (mmdpass)
                {
                    case "object":
                        this.MMDPassAnnotation = MMEEffectPassType.Object;
                        break;
                    case "object_ss":
                        this.MMDPassAnnotation = MMEEffectPassType.Object_SelfShadow;
                        break;
                    case "zplot":
                        this.MMDPassAnnotation = MMEEffectPassType.ZPlot;
                        break;
                    case "shadow":
                        this.MMDPassAnnotation = MMEEffectPassType.Shadow;
                        break;
                    case "edge":
                        this.MMDPassAnnotation = MMEEffectPassType.Edge;
                        break;
                    default:
                        throw new InvalidOperationException("予期しない識別子");
                }
            }
            //Loading UseTexture
            this.UseTexture = EffectParseHelper.getAnnotationBoolean(technique, "UseTexture");
            this.UseSphereMap = EffectParseHelper.getAnnotationBoolean(technique, "UseSphereMap");
            this.UseToon = EffectParseHelper.getAnnotationBoolean(technique, "UseToon");
            this.UseSelfShadow = EffectParseHelper.getAnnotationBoolean(technique, "UseSelfShadow");
            this.MulSphere = EffectParseHelper.getAnnotationBoolean(technique, "MulSphere");
            GetSubsets(technique, subsetCount);
            EffectVariable rawScript = EffectParseHelper.getAnnotation(technique, "Script", "string");
            for (int i = 0; i < technique.Description.PassCount; i++)
            {
                EffectPass pass = technique.GetPassByIndex(i);
                this.Passes.Add(pass.Description.Name,new MMEEffectPass(context, manager, pass));
            }
            if (rawScript != null)
            {
                this.ScriptRuntime = new ScriptRuntime(rawScript.AsString().GetString(), context, manager, this);
            }
            else
            {
                this.ScriptRuntime = new ScriptRuntime("", context, manager, this);
            }
        }

        /// <summary>
        ///     This class is drawn subset numbers
        /// </summary>
        public HashSet<int> Subset { get; private set; }

        /// <summary>
        ///     The path used for this technique
        /// </summary>
        public Dictionary<string,MMEEffectPass> Passes { get; private set; }

        public ScriptRuntime ScriptRuntime { get; private set; }

        /// <summary>
        ///     MMDPassOf the type
        /// </summary>
        public MMEEffectPassType MMDPassAnnotation { get; private set; }

        /// <summary>
        ///     Whether or not using a sphere map
        /// </summary>
        public ExtendedBoolean UseSphereMap { get; private set; }

        /// <summary>
        ///     Whether or not using a texture
        /// </summary>
        public ExtendedBoolean UseTexture { get; private set; }

        /// <summary>
        ///     Whether using the Toon
        /// </summary>
        public ExtendedBoolean UseToon { get; private set; }

        /// <summary>
        ///     Using self shadow or whether or not (MMM specifications)
        /// </summary>
        public ExtendedBoolean UseSelfShadow { get; private set; }

        public ExtendedBoolean MulSphere { get; private set; }

        private void GetSubsets(EffectTechnique technique, int subsetCount)
        {
            string subset = EffectParseHelper.getAnnotationString(technique, "Subset");
            //Subset analysis
            if (string.IsNullOrWhiteSpace(subset))
            {
                for (int i = 0; i <= subsetCount; i++) //If you do not specify subset rendering which will all
                {
                    this.Subset.Add(i);
                }
            }
            else
            {
                string[] chunks = subset.Split(','); //,でサブセットアノテーションを分割
                foreach (string chunk in chunks)
                {
                    if (chunk.IndexOf('-') == -1) //-Are recognized and that unit is not
                    {
                        int value = 0;
                        if (int.TryParse(chunk, out value))
                        {
                            this.Subset.Add(value);
                        }
                        else
                        {
                            throw new InvalidMMEEffectShaderException(
                                string.Format("テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」は認識されません。",
                                    technique.Description.Name, subset, chunk));
                        }
                    }
                    else
                    {
                        string[] regions = chunk.Split('-'); //-Scoping and to recognize if you have。
                        if (regions.Length > 2)
                            throw new InvalidMMEEffectShaderException(
                                string.Format("テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」には\"-\"が2つ以上存在します。",
                                    technique.Description.Name, subset, chunk));
                        if (string.IsNullOrWhiteSpace(regions[1])) //In this case, x-shaped and recognized。
                        {
                            int value = 0;
                            if (int.TryParse(regions[0], out value))
                            {
                                for (int i = value; i <= subsetCount; i++)
                                {
                                    this.Subset.Add(i);
                                }
                            }
                            else
                            {
                                throw new InvalidMMEEffectShaderException(
                                    string.Format("テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」の「{3}」は認識されません。",
                                        technique.Description.Name, subset, chunk, regions[0]));
                            }
                        }
                        else //In this case believes that x-y format
                        {
                            int value1 = 0;
                            int value2 = 0;
                            if (int.TryParse(regions[0], out value1) && int.TryParse(regions[1], out value2))
                            {
                                for (int i = value1; i <= value2; i++)
                                {
                                    this.Subset.Add(i);
                                }
                            }
                            else
                            {
                                throw new InvalidMMEEffectShaderException(
                                    string.Format(
                                        "テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」の「{3}」もしくは「{4}」は認識されません。",
                                        technique.Description.Name, subset, chunk, regions[0], regions[1]));
                            }
                        }
                    }
                }
            }
        }

        public void ExecuteTechnique(DeviceContext context,Action<ISubset> drawAction,ISubset ipmxSubset)
        {
            if (string.IsNullOrWhiteSpace(this.ScriptRuntime.ScriptCode))
            {
                foreach (MMEEffectPass pass in this.Passes.Values)
                {
                    pass.Pass.Apply(context);
                    drawAction(ipmxSubset);
                }
            }
            else//If you have a script at Script Runtime delegate to
            {
                this.ScriptRuntime.Execute(drawAction, ipmxSubset);
            }
        }

        public static bool CheckExtebdedBoolean(ExtendedBoolean teqValue, bool subsetValue)
        {
            if (subsetValue)
            {
                return teqValue != ExtendedBoolean.Disable;
            }
            return teqValue != ExtendedBoolean.Enable;
        }
    }
}