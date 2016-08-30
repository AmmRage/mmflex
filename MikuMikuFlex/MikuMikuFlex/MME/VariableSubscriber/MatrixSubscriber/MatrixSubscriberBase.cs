using SlimDX;
using SlimDX.Direct3D11;

namespace MMF.MME.VariableSubscriber.MatrixSubscriber
{
    /// <summary>
    ///     The registration class of a matrix-based
    /// </summary>
    public abstract class MatrixSubscriberBase : SubscriberBase
    {
        protected ObjectAnnotationType TargetObject;

        protected MatrixSubscriberBase(ObjectAnnotationType Object)
        {
            this.TargetObject = Object;
        }

        protected MatrixSubscriberBase()
        {
        }

        public override VariableType[] Types
        {
            get { return new[] {VariableType.Float4x4}; }
        }

        /// <summary>
        ///     To register as the matrix effect
        /// </summary>
        /// <param name="matrix">To register the matrix</param>
        /// <param name="effect">Effects</param>
        /// <param name="index">Index of the variable</param>
        protected void SetAsMatrix(Matrix matrix, EffectVariable variable)
        {
            variable.AsMatrix().SetMatrix(matrix);
        }

        /// <summary>
        ///     If the matrix each examine the Camera or Light?
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="context"></param>
        /// <param name="effectManager"></param>
        /// <param name="semanticIndex"></param>
        /// <param name="effect">Effect of test</param>
        /// <param name="index">Inspect variable index</param>
        /// <returns></returns>
        public override SubscriberBase GetSubscriberInstance(EffectVariable variable, RenderContext context, MMEEffectManager effectManager, int semanticIndex)
        {
            string obj;
            EffectVariable annotation = EffectParseHelper.getAnnotation(variable, "Object", "string");
            obj = annotation == null ? "" : annotation.AsString().GetString(); //The annotation is not present""とする
            if (string.IsNullOrWhiteSpace(obj)) return GetSubscriberInstance(ObjectAnnotationType.Camera);
            switch (obj.ToLower())
            {
                case "camera":
                    return GetSubscriberInstance(ObjectAnnotationType.Camera);
                case "light":
                    return GetSubscriberInstance(ObjectAnnotationType.Light);
                case "":
                    throw new InvalidMMEEffectShaderException(
                        string.Format(
                            "変数「{0} {1}:{2}」には、アノテーション「string Object=\"Camera\"」または、「string Object=\"Light\"」が必須ですが指定されませんでした。",
                            variable.GetVariableType().Description.TypeName.ToLower(), variable.Description.Name,
                            variable.Description.Semantic));
                default:
                    throw new InvalidMMEEffectShaderException(
                        string.Format(
                            "変数「{0} {1}:{2}」には、アノテーション「string Object=\"Camera\"」または、「string Object=\"Light\"」が必須ですが指定されたのは「string Object=\"{3}\"」でした。(スペルミス?)",
                            variable.GetVariableType().Description.TypeName.ToLower(), variable.Description.Name,
                            variable.Description.Semantic, obj));
            }
        }

        protected abstract SubscriberBase GetSubscriberInstance(ObjectAnnotationType Object);
    }
}