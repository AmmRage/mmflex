using System.Collections.Generic;
using SlimDX.Direct3D11;
using Debug = System.Diagnostics.Debug;

namespace MMF.MME
{
    public class MMEEffectInfo
    {
        public MMEEffectInfo(Effect effect)
        {
            this.ScriptOutput = "color";
            this.ScriptClass = ScriptClass.Object;
            this.ScriptOrder = ScriptOrder.Standard;
            this.StandardGlobalScript = "";
            this.SortedTechnique = new List<EffectTechnique>();
            for (int i = 0; i < effect.Description.GlobalVariableCount; i++)
            {
                EffectVariable variable = effect.GetVariableByIndex(i);
                if (variable.Description.Semantic.ToUpper().Equals("STANDARDGLOBAL"))
                {
//When this variable is STANDARDGLOBAL
                    ParseStandardGlobal(effect, variable);
                    break;
                }
            }
        }

        public string ScriptOutput { get; private set; }

        public ScriptClass ScriptClass { get; private set; }

        public ScriptOrder ScriptOrder { get; private set; }

        public string StandardGlobalScript { get; private set; }

        public List<EffectTechnique> SortedTechnique { get; private set; }

        private void ParseStandardGlobal(Effect effect, EffectVariable sg)
        {
            if (!sg.Description.Name.ToLower().Equals("script"))
            {
                throw new InvalidMMEEffectShaderException(
                    string.Format("STANDARDGLOBALセマンティクスの指定される変数名は\"Script\"である必要があります、指定された変数名は\"{0}\"でした。",
                        sg.Description.Name));
            }
            if (!sg.GetVariableType().Description.TypeName.ToLower().Equals("float"))
            {
                throw new InvalidMMEEffectShaderException(
                    string.Format("STANDARDGLOBALセマンティクスの指定される変数型は\"float\"である必要があります、指定された変数名は\"{0}\"でした。",
                        sg.GetVariableType().Description.TypeName.ToLower()));
            }
            if (sg.AsScalar().GetFloat() != 0.8f)
            {
                throw new InvalidMMEEffectShaderException(
                    string.Format("STANDARDGLOBALセマンティクスの指定される値は\"0.8\"である必要があります、指定された値は\"{0}\"でした。",
                        sg.AsScalar().GetFloat()));
            }
            //ScriptOutput analysis
            EffectVariable soVal = EffectParseHelper.getAnnotation(sg, "ScriptOutput", "string");
            if (soVal != null)
            {
                if (!soVal.AsString().GetString().Equals("color"))
                    throw new InvalidMMEEffectShaderException(
                        string.Format(
                            "STANDARDGLOBALセマンティクスの指定される変数のアノテーション「string ScriptOutput」は、\"color\"でなくてはなりません。指定された値は\"{0}\"でした。",
                            soVal.AsString().GetString().ToLower()));
            }
            EffectVariable scVal = EffectParseHelper.getAnnotation(sg, "ScriptClass", "string");
            if (scVal != null)
            {
                string sc = scVal.AsString().GetString();
                switch (sc.ToLower())
                {
                    case "object":
                        this.ScriptClass = ScriptClass.Object;
                        break;
                    case "scene":
                        this.ScriptClass = ScriptClass.Scene;
                        break;
                    case "sceneorobject":
                        this.ScriptClass = ScriptClass.SceneOrObject;
                        break;
                    default:
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "STANDARDGLOBALセマンティクスの指定される変数のアノテーション「string ScriptClass」は、\"object\",\"scene\",\"sceneorobject\"でなくてはなりません。指定された値は\"{0}\"でした。(スペルミス?)",
                                sc.ToLower()));
                }
            }
            EffectVariable sorVal = EffectParseHelper.getAnnotation(sg, "ScriptOrder", "string");
            if (sorVal != null)
            {
                string sor = sorVal.AsString().GetString();
                switch (sor.ToLower())
                {
                    case "standard":
                        this.ScriptOrder = ScriptOrder.Standard;
                        break;
                    case "preprocess":
                        this.ScriptOrder = ScriptOrder.Preprocess;
                        break;
                    case "postprocess":
                        this.ScriptOrder = ScriptOrder.Postprocess;
                        break;
                    default:
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "STANDARDGLOBALセマンティクスの指定される変数のアノテーション「string ScriptOrder」は、\"standard\",\"preprocess\",\"postprocess\"でなくてはなりません。指定された値は\"{0}\"でした。(スペルミス?)",
                                sor.ToLower()));
                }
            }
            EffectVariable scrVal = EffectParseHelper.getAnnotation(sg, "Script", "string");
            if (scrVal != null)
            {
                this.StandardGlobalScript = scrVal.AsString().GetString();
                if (string.IsNullOrEmpty(this.StandardGlobalScript))
                {
                    for (int i = 0; i < effect.Description.TechniqueCount; i++)
                    {
                        this.SortedTechnique.Add(effect.GetTechniqueByIndex(i));
                    }
                }
                else
                {
                    string[] scriptChunks = this.StandardGlobalScript.Split(';');
                    if (scriptChunks.Length == 1)
                    {
                        throw new InvalidMMEEffectShaderException(
                            string.Format("STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。\";\"が足りません。", this.StandardGlobalScript));
                    }
                    string targetScript = scriptChunks[scriptChunks.Length - 2]; //Non-script with a final semicolon is ignored
                    if (this.StandardGlobalScript.IndexOf("?") == -1) //When not found
                    {
                        string[] args = targetScript.Split('=');
                        if (args.Length > 2)
                        {
                            throw new InvalidMMEEffectShaderException(
                                string.Format("STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。\"=\"の数が多すぎます。",
                                    targetScript));
                        }
                        if (!args[0].ToLower().Equals("technique"))
                        {
                            throw new InvalidMMEEffectShaderException(
                                string.Format(
                                    "STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。\"{1}\"は\"Technique\"であるべきです。(スペルミス?)",
                                    targetScript, args[0]));
                        }
                        EffectTechnique technique = effect.GetTechniqueByName(args[1]);
                        if (technique != null)
                        {
                            this.SortedTechnique.Add(technique);
                        }
                        else
                        {
                            throw new InvalidMMEEffectShaderException(
                                string.Format(
                                    "STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。テクニック\"{1}\"は存在しません。(スペルミス?)",
                                    targetScript, args[1]));
                        }
                    }
                    else //?が見つかるとき
                    {
                        string[] args = targetScript.Split('?');
                        if (args.Length == 2)
                        {
                            string[] techniques = args[1].Split(':');
                            foreach (string technique in techniques)
                            {
                                EffectTechnique effectTechnique = effect.GetTechniqueByName(technique);
                                if (effectTechnique != null)
                                {
                                    this.SortedTechnique.Add(effectTechnique);
                                }
                                else
                                {
                                    throw new InvalidMMEEffectShaderException(
                                        string.Format(
                                            "STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。テクニック\"{1}\"は見つかりません。(スペルミス?)",
                                            targetScript, technique));
                                }
                            }
                        }
                        else if (args.Length > 2)
                        {
                            throw new InvalidMMEEffectShaderException(
                                string.Format("STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」は読み込めませんでした。\"?\"の数が多すぎます。",
                                    targetScript));
                        }
                    }
                    if (scriptChunks.Length > 2)
                    {
                        Debug.WriteLine(
                            string.Format(
                                "STANDARDGLOBALセマンティクスの指定される変数のスクリプト「{0}」では、複数回Techniqueの代入が行われていますが、最後の代入以外は無視されます。", this.StandardGlobalScript));
                    }
                }
            }
        }
    }

    public enum ScriptOrder
    {
        Standard,
        Preprocess,
        Postprocess
    }

    public enum ScriptClass
    {
        Object,
        Scene,
        SceneOrObject
    }
}