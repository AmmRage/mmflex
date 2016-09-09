using SlimDX;

namespace MMF.Model
{
    public class BasicTransformer : ITransformer
    {

        private Vector3 foward;
        private Vector3 initialFoward;
        private Vector3 initialTop;
        private Quaternion rotation;
        private Vector3 top;

        private Matrix localTransform;
        private Vector3 position;
        private Vector3 scale;

        public BasicTransformer(Vector3 top, Vector3 forward)
        {
            this.InitialTop = top;
            this.InitialFoward = forward;
            Reset();
        }

        public BasicTransformer() : this(new Vector3(0, 1, 0), new Vector3(0, 0, -1))
        {
        }

        public Vector3 Position

        {
            get { return this.position; }
            set
            {
                this.position = value; 
                CalcLocalTransform();
            }
        }

        public Quaternion Rotation
        {
            get { return this.rotation; }
            set
            {
                this.rotation = value;
                this.rotation.Normalize();
                this.Foward = Vector3.TransformCoordinate(this.InitialFoward, Matrix.RotationQuaternion(this.Rotation));
                this.Top = Vector3.TransformCoordinate(this.InitialTop, Matrix.RotationQuaternion(this.Rotation));
                this.Top.Normalize();
                this.Foward.Normalize();
                CalcLocalTransform();
            }
        }

        public Vector3 Scale
        {
            get { return this.scale; }
            set
            {
                this.scale = value; 
                CalcLocalTransform();
            }
        }

        public Vector3 Foward
        {
            get { return this.foward; }
            set
            {
                this.foward = value;
                this.foward.Normalize();
            }
        }

        public Vector3 Top
        {
            get { return this.top; }
            set
            {
                this.top = value;
                this.top.Normalize();
            }
        }


        public Vector3 InitialTop
        {
            get { return this.initialTop; }
            private set
            {
                this.initialTop = value;
                this.initialTop.Normalize();
            }
        }

        public Vector3 InitialFoward
        {
            get { return this.initialFoward; }
            private set
            {
                this.initialFoward = value;
                this.InitialFoward.Normalize();
                
            }
        }

        public void Reset()
        {
            this.Top = this.initialTop;
            this.Foward = this.initialFoward;
            this.Rotation = Quaternion.Identity;
            this.Position = Vector3.Zero;
            this.Scale = new Vector3(1f);
            CalcLocalTransform();
        }

        private void CalcLocalTransform()
        {
            this.localTransform = Matrix.Scaling(this.Scale)*Matrix.RotationQuaternion(this.Rotation)*Matrix.Translation(this.Position);
        }

        public Matrix LocalTransform
        {
            get { return this.localTransform; }
            private set { this.localTransform = value; }
        }
    }
}