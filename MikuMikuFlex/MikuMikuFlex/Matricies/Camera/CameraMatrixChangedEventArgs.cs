using System;

namespace MMF.Matricies.Camera
{
    /// <summary>
    ///     Camera settings are changed when the event argument
    /// </summary>
    public class CameraMatrixChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">The contents of the camera changed</param>
        public CameraMatrixChangedEventArgs(CameraMatrixChangedVariableType type)
        {
            this.ChangedType = type;
        }

        /// <summary>
        ///     Content changed
        /// </summary>
        public CameraMatrixChangedVariableType ChangedType { get; private set; }
    }
}