using System;

namespace MMF.Matricies.Projection
{
    public class ProjectionMatrixChangedEventArgs:EventArgs
    {
        /// <summary>
        /// Changed variable
        /// </summary>
        public ProjectionMatrixChangedVariableType ChangedType { get; private set; }

        public ProjectionMatrixChangedEventArgs(ProjectionMatrixChangedVariableType type)
        {
            this.ChangedType = type;
        }
    }
}
