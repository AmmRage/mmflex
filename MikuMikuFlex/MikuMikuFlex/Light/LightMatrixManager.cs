using MMF.Matricies;
using MMF.Matricies.Camera;
using MMF.Matricies.Projection;
using SlimDX;

namespace MMF.Light
{
    public class LightMatrixManager
    {
        public LightMatrixManager(MatrixManager manager)
        {
            this.manager = manager;
            this.Camera = new BasicCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            this.Projection=new BasicProjectionMatrixProvider();
        }

        private MatrixManager manager;
        private Vector3 direction;

        public CameraProvider Camera { get; private set; }

        public BasicProjectionMatrixProvider Projection { get; private set; }

        public Vector3 Position
        {
            get
            {
                return this.Camera.CameraPosition;
            }
            set
            {
                this.Camera.CameraPosition = value;
                UpdateDirection();
            }
        }

        private void UpdateDirection()
        {
            this.direction = Vector3.Normalize(-this.Camera.CameraPosition);
        }

        public Vector3 Direction
        {
            get { return this.direction; }
            private set { this.direction = value; }
        }
    }
}
