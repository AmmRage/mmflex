using System;
using MMF.Model;
using SlimDX;

namespace MMF.Matricies.World
{
    /// <summary>
    ///     World matrix interface
    /// </summary>
    public interface IWorldMatrixProvider
    {
        /// <summary>
        ///     Magnification level
        /// </summary>
        Vector3 Scaling { get; set; }

        /// <summary>
        ///     Rotation
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        ///     Translate
        /// </summary>
        Vector3 Translation { get; set; }

        /// <summary>
        ///     Local values and together create a world transformation matrix
        /// </summary>
        /// <param name="scalingLocal">Local expansion</param>
        /// <param name="rotationLocal">Local rotation</param>
        /// <param name="translationLocal">Local translation</param>
        /// <returns></returns>
        Matrix getWorldMatrix(Vector3 scalingLocal, Quaternion rotationLocal, Vector3 translationLocal);

        /// <summary>
        ///     Create a world transformation matrix of the specified model。
        /// </summary>
        /// <param name="drawable">Models, including local coordinates to get</param>
        /// <returns>Value</returns>
        Matrix getWorldMatrix(IDrawable drawable);

         Matrix getWorldMatrix(Matrix localMatrix);

        /// <summary>
        ///     Informs that changed the world
        /// </summary>
        event EventHandler<WorldMatrixChangedEventArgs> WorldMatrixChanged;
    }
}