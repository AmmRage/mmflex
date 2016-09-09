using System;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

namespace MMF.MME
{
    /// <summary>
    ///     Helper class for the perspective effect
    /// </summary>
    public static class EffectParseHelper
    {
        /// <summary>
        ///     Ignore the case of name annotation, to get
        /// </summary>
        /// <param name="variable">To get the effect variables</param>
        /// <param name="target">Annotation name</param>
        /// <param name="typeName">Expected type</param>
        /// <returns>Annotation</returns>
        public static EffectVariable getAnnotation(EffectVariable variable, string target, string typeName)
        {
            string name = target.ToLower();
            string[] valid = name.Split('/');
            for (int i = 0; i < variable.Description.AnnotationCount; i++)
            {
                EffectVariable val = variable.GetAnnotationByIndex(i);
                string typeString = val.Description.Name.ToLower();
                if (typeString== name)
                {
                    if (
                        !valid.Contains(typeString)&&!String.IsNullOrWhiteSpace(typeString))
                    {
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "変数「{0} {1}:{2}」に適用されたアノテーション「{3} {4}」はアノテーションの型が正しくありません。期待した型は{5}でした。",
                                variable.GetVariableType().Description.TypeName, variable.Description.Name,
                                variable.Description.Semantic, val.GetVariableType().Description.TypeName,
                                val.Description.Name, getExpectedTypes(valid,val.Description.Name)));
                    }
                    return val;
                }
            }
            return null;
        }

        /// <summary>
        ///     Ignore the case of name annotation to retrieve
        /// </summary>
        /// <param name="pass">To get the path</param>
        /// <param name="target">Annotation name</param>
        /// <param name="typeName">Expected type</param>
        /// <returns>Annotation</returns>
        public static EffectVariable getAnnotation(EffectPass pass, string target, string typeName)
        {
            string name = target.ToLower();
            string[] valid = name.Split('/');
            for (int i = 0; i < pass.Description.AnnotationCount; i++)
            {
                EffectVariable val = pass.GetAnnotationByIndex(i);
                string typeString = val.Description.Name.ToLower();
                if (typeString == name)
                {
                    if (
                        !valid.Contains(typeString) && !String.IsNullOrWhiteSpace(typeString))
                    {
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "パス「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。",
                                pass.Description.Name, typeString, val.Description.Name,getExpectedTypes(valid,val.Description.Name)));
                    }
                    return val;
                }
            }
            return null;
        }

        /// <summary>
        ///     Ignore the case of name annotation to retrieve
        /// </summary>
        /// <param name="technique">Techniques to get</param>
        /// <param name="target">Annotation name</param>
        /// <param name="typeName">Expected type</param>
        /// <returns>Annotation</returns>
        public static EffectVariable getAnnotation(EffectTechnique technique, string target, string typeName)
        {
            string name = target.ToLower();
            string[] valid = name.Split('/');
            for (int i = 0; i < technique.Description.AnnotationCount; i++)
            {
                EffectVariable val = technique.GetAnnotationByIndex(i);
                string typeString = val.Description.Name.ToLower();
                if (typeString == name)
                {
                    if (
                        !valid.Contains(typeString) && !String.IsNullOrWhiteSpace(typeString))
                    {
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "テクニック「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。"
                                ,technique.Description.Name,typeString,val.Description.Name,getExpectedTypes(valid,val.Description.Name)));
                    }
                    return val;
                }
            }
            return null;
        }

        public static EffectVariable getAnnotation(EffectGroup group, string target, string typeName)
        {
            string name = target.ToLower();
            string[] valid = name.Split('/');
            for (int i = 0; i < group.Description.AnnotationCount; i++)
            {
                EffectVariable val = group.GetAnnotationByIndex(i);
                string typeString = val.Description.Name.ToLower();
                if (typeString == name)
                {
                    if (
                        !valid.Contains(typeString) && !String.IsNullOrWhiteSpace(typeString))
                    {
                        throw new InvalidMMEEffectShaderException(
                            string.Format(
                                "エフェクトグループ「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。"
                                , group.Description.Name, typeString, val.Description.Name, getExpectedTypes(valid, val.Description.Name)));
                    }
                    return val;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets the annotation techniques specified in string
        /// </summary>
        /// <param name="technique">Techniques</param>
        /// <param name="attrName">Annotation name</param>
        /// <returns>Value</returns>
        public static string getAnnotationString(EffectTechnique technique, string attrName)
        {
            EffectVariable annotationVariable = getAnnotation(technique, attrName, "string");
            if (annotationVariable == null) return "";
            return annotationVariable.AsString().GetString();
        }

        /// <summary>
        ///     Returns a Bool annotation techniques specified in
        /// </summary>
        /// <param name="technique">Techniques</param>
        /// <param name="attrName">Annotation name</param>
        /// <returns>Value</returns>
        public static ExtendedBoolean getAnnotationBoolean(EffectTechnique technique, string attrName)
        {
            EffectVariable annotationVariable = getAnnotation(technique, attrName, "bool");
            if (annotationVariable == null) return ExtendedBoolean.Ignore;
            int annotation = annotationVariable.AsScalar().GetInt();
            if (annotation == 1)
            {
                return ExtendedBoolean.Enable;
            }
            return ExtendedBoolean.Disable;
        }

        private static string getExpectedTypes(string[] types, string name)
        {
            StringBuilder builder=new StringBuilder();
            foreach (string type in types)
            {
                if(String.IsNullOrWhiteSpace(type))continue;
                if (builder.Length != 0) builder.Append(",");
                builder.Append(string.Format("「{0} {1}」", type, name));
            }
            return builder.ToString();
        }
    }
}