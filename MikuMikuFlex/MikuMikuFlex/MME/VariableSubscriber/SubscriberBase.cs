using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

namespace MMF.MME.VariableSubscriber
{
    /// <summary>
    ///     Base class for class registration
    /// </summary>
    public abstract class SubscriberBase
    {
        /// <summary>
        ///     Semantics
        /// </summary>
        public abstract string Semantics { get; }

        /// <summary>
        ///     Type can be used
        /// </summary>
        public abstract VariableType[] Types { get; }

        public virtual UpdateBy UpdateTiming
        {
            get { return UpdateBy.Model; }
        }

        /// <summary>
        ///     Check the index of the specified effect
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="context"></param>
        /// <param name="effectManager"></param>
        /// <param name="semanticIndex"></param>
        /// <param name="effect">To examine effects</param>
        /// <param name="index">Index of variables to test</param>
        /// <return>登録時にアノテーションとして送るobject</return>
        public abstract SubscriberBase GetSubscriberInstance(EffectVariable variable, RenderContext context,MMEEffectManager effectManager,int semanticIndex);

        public void CheckType(EffectVariable variable)
        {
            EffectType type = variable.GetVariableType();
            string typeName = type.Description.TypeName.ToLower();
            VariableType valType;
            switch (typeName)
            {
                case "float4x4":
                    valType = VariableType.Float4x4;
                    break;
                case "float4":
                    valType = VariableType.Float4;
                    break;
                case "float3":
                    valType = VariableType.Float3;
                    break;
                case "float2":
                    valType = VariableType.Float2;
                    break;
                case "float":
                    valType = VariableType.Float;
                    break;
                case "uint":
                    valType = VariableType.Uint;
                    break;
                case "texture2d":
                    valType = VariableType.Texture2D;
                    break;
                case "texture":
                    valType=VariableType.Texture;
                    break;
                case "texture3d":
                    valType=VariableType.Texture3D;
                    break;
                case "texturecube":
                    valType=VariableType.TextureCUBE;
                    break;
                case "int":
                    valType = VariableType.Int;
                    break;
                case "bool":
                    valType = VariableType.Bool;
                    break;
                case "cbuffer":
                    valType = VariableType.Cbuffer;
                    break;
                default:
                    throw new InvalidMMEEffectShaderException(
                        string.Format("定義済みセマンティクス「{0}」に対して不適切な型「{1}」が使用されました。これは「{2}」であるべきセマンティクスです。", this.Semantics,
                            typeName, getSupportedTypes()));
            }
            if (!this.Types.Contains(valType))
            {
                throw new InvalidMMEEffectShaderException(
                    string.Format("定義済みセマンティクス「{0}」に対して不適切な型「{1}」が使用されました。これは「{2}」であるべきセマンティクスです。", this.Semantics, typeName,
                        getSupportedTypes()));
            }
        }

        public abstract void Subscribe(EffectVariable subscribeTo, SubscribeArgument variable);

        private string getSupportedTypes()
        {
            StringBuilder builder = new StringBuilder();
            foreach (VariableType variableType in this.Types)
            {
                builder.Append(variableType.ToString().ToLower());
                builder.Append("/");
            }
            return builder.ToString();
        }
    }
}