using MMF.Matricies.Camera;
using MMF.Matricies.Projection;
using MMF.Matricies.World;
using MMF.Model;
using SlimDX;

namespace MMF.Matricies
{
    public class MatrixManager
    {
        private readonly IProjectionMatrixProvider projectionMatrixManager;
        private readonly CameraProvider viewMatrixManager;
        private readonly IWorldMatrixProvider worldMatrixManager;

        public MatrixManager(IWorldMatrixProvider world, CameraProvider cam, IProjectionMatrixProvider projection)
        {
            this.worldMatrixManager = world;
            this.viewMatrixManager = cam;
            this.projectionMatrixManager = projection;
        }

        public IWorldMatrixProvider WorldMatrixManager
        {
            get { return this.worldMatrixManager; }
        }

        public CameraProvider ViewMatrixManager
        {
            get { return this.viewMatrixManager; }
        }

        public IProjectionMatrixProvider ProjectionMatrixManager
        {
            get { return this.projectionMatrixManager; }
        }


        public Matrix makeWorldViewProjectionMatrix(Vector3 localScaling, Quaternion localRotation,
            Vector3 localTranslation)
        {
            return this.worldMatrixManager.getWorldMatrix(localScaling, localRotation, localTranslation)*this.viewMatrixManager.ViewMatrix*this.projectionMatrixManager.ProjectionMatrix;
        }

        public Matrix makeWorldViewProjectionMatrix(IDrawable drawable)
        {
            return this.worldMatrixManager.getWorldMatrix(drawable)*this.viewMatrixManager.ViewMatrix*this.projectionMatrixManager.ProjectionMatrix;
        }
    }
}