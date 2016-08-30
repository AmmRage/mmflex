using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.DeviceManager;
using MMF.Model.Controller.ControllerComponent;
using MMF.Model.Shape;
using SlimDX;

namespace MMF.Model.Controller
{
    public class TransformController : IDrawable, ILockableController
    {
        public void ResetTransformedEventHandler()
        {
            Transformed = delegate { };
        }

        private readonly TexturedBufferHitChecker _hitChecker;

        private Action<TransformController, TransformChangedEventArgs> transformChanged;

        /// <summary>
        /// Call control has changed。
        /// </summary>
        public class TransformChangedEventArgs : EventArgs
        {
            private IDrawable targetModel;

            private TransformType type;

            public TransformChangedEventArgs(IDrawable targetModel, TransformType type)
            {
                this.targetModel = targetModel;
                this.type = type;
            }

            public IDrawable TargetModel
            {
                get { return this.targetModel; }
            }

            public TransformType Type
            {
                get { return this.type; }
            }
        }

        /// <summary>
        /// enable transform controller in player
        /// </summary>
        [Flags]
        public enum TransformType
        {
            /// <summary>
            /// enable movement on x,y or z
            /// </summary>
            Translation = 0x01,

            /// <summary>
            /// enable rotating
            /// </summary>
            Rotation = 0x02,

            /// <summary>
            /// enable scaling
            /// </summary>
            Scaling = 0x04,
            All = 0x07
        }

        public event EventHandler<TransformChangedEventArgs> Transformed = delegate { };

        public event EventHandler TargetModelChanged = delegate { };

        private List<IHitTestable> controllers = new List<IHitTestable>();

        private RotateRingController xRotater;

        private RotateRingController yRotater;

        private RotateRingController zRotater;

        private TranslaterConeController xTranslater;

        private TranslaterConeController yTranslater;

        private TranslaterConeController zTranslater;

        private ScalingCubeController scaler;

        private CenterCrossLine cross;

        public float TransformRotationSensibility { get; set; }

        public float TransformTranslationSensibility { get; set; }

        public float TransformScalingSensibility { get; set; }

        public TransformController(RenderContext context, TexturedBufferHitChecker hitChecker)
        {
            this._hitChecker = hitChecker;
            this._type = TransformType.All;
            this.TransformRotationSensibility = 1.0f;
            this.TransformScalingSensibility = 1.0f;
            this.TransformTranslationSensibility = 1.0f;
            this.Transformer = new BasicTransformer();
            //Rotation control
            this.xRotater = new RotateRingController(context, this, new Vector4(1, 0, 0, 0.7f),
                new Vector4(1, 1, 0, 0.7f),
                new SilinderShape.SilinderShapeDescription(0.02f, 30));
            this.yRotater = new RotateRingController(context, this, new Vector4(0, 1, 0, 0.7f),
                new Vector4(1, 1, 0, 0.7f),
                new SilinderShape.SilinderShapeDescription(0.02f, 30));
            this.zRotater = new RotateRingController(context, this, new Vector4(0, 0, 1, 0.7f),
                new Vector4(1, 1, 0, 0.7f),
                new SilinderShape.SilinderShapeDescription(0.02f, 30));
            this.cross = new CenterCrossLine(context);
            this.xRotater.Transformer.Rotation *= Quaternion.RotationAxis(Vector3.UnitZ, (float) (Math.PI/2));
            this.zRotater.Transformer.Rotation *= Quaternion.RotationAxis(-Vector3.UnitX, -(float) (Math.PI/2));
            this.xRotater.Transformer.Scale =
                this.yRotater.Transformer.Scale = this.zRotater.Transformer.Scale = new Vector3(1f, 0.1f, 1f)*20;
            this.xRotater.Transformer.Scale *= 0.998f;
            this.zRotater.Transformer.Scale *= 0.990f;
            this.xRotater.Initialize();
            this.yRotater.Initialize();
            this.zRotater.Initialize();
            this.xRotater.OnRotated += RotationChanged;
            this.yRotater.OnRotated += RotationChanged;
            this.zRotater.OnRotated += RotationChanged;
            this.controllers.Add(this.xRotater);

            this.controllers.Add(this.yRotater);
            this.controllers.Add(this.zRotater);
            //Balance transfer control
            this.xTranslater = new TranslaterConeController(context, this, new Vector4(1, 0, 0, 0.3f),
                new Vector4(1, 1, 0, 0.7f));
            this.yTranslater = new TranslaterConeController(context, this, new Vector4(0, 1, 0, 0.3f),
                new Vector4(1, 1, 0, 0.7f));
            this.zTranslater = new TranslaterConeController(context, this, new Vector4(0, 0, 1, 0.3f),
                new Vector4(1, 1, 0, 0.7f));
            this.xTranslater.Initialize();
            this.yTranslater.Initialize();
            this.zTranslater.Initialize();
            this.xTranslater.Transformer.Scale =
                this.yTranslater.Transformer.Scale = this.zTranslater.Transformer.Scale = new Vector3(2f);
            this.xTranslater.Transformer.Rotation *= Quaternion.RotationAxis(Vector3.UnitZ, (float) (Math.PI/2));
            this.zTranslater.Transformer.Rotation *= Quaternion.RotationAxis(-Vector3.UnitX, -(float) (Math.PI/2));
            MoveTranslater(this.xTranslater);
            MoveTranslater(this.yTranslater);
            MoveTranslater(this.zTranslater);
            this.xTranslater.OnTranslated += OnTranslated;
            this.yTranslater.OnTranslated += OnTranslated;
            this.zTranslater.OnTranslated += OnTranslated;
            this.controllers.Add(this.xTranslater);
            this.controllers.Add(this.yTranslater);
            this.controllers.Add(this.zTranslater);
            this.scaler = new ScalingCubeController(context, this, new Vector4(0, 1, 1, 0.7f), new Vector4(1, 1, 0, 1));
            this.scaler.Initialize();
            this.scaler.Transformer.Scale = new Vector3(3);
            this.scaler.OnScalingChanged += OnScaling;
            this.controllers.Add(this.scaler);
            hitChecker.CheckTargets.AddRange(this.controllers);
            this.Visibility = true;
        }

        private void OnScaling(object sender, ScalingCubeController.ScalingChangedEventArgs e)
        {
            float delta = e.Delta*this.TransformScalingSensibility;
            this.sumScaling *= delta;
            if (this.targetModel != null)
            {
                this.targetModel.Transformer.Scale += new Vector3(delta);
                Transformed(this, new TransformChangedEventArgs(this.targetModel, TransformType.Scaling));
                if (this.transformChanged != null)
                    this.transformChanged(this, new TransformChangedEventArgs(this.targetModel, TransformType.Scaling));
            }
        }

        private void OnTranslated(object sender, TranslaterConeController.TranslatedEventArgs e)
        {
            Vector3 trans = e.Translation;
            setTranslationProperty(trans);
        }

        private void MoveTranslater(TranslaterConeController translater)
        {
            translater.Transformer.Position += translater.Transformer.Top*30;
        }

        private void RotationChanged(object sender, RotateRingController.RotationChangedEventArgs e)
        {
            var quat = Quaternion.RotationAxis(e.Axis, -e.Length/100f);
            setRotationProperty(quat);
        }

        private void setRotationProperty(Quaternion quat)
        {
            quat = Quaternion.Lerp(Quaternion.Identity, quat, this.TransformRotationSensibility);
            this.sumRotation *= quat;
            this.Transformer.Rotation *= quat;
            this.xRotater.Transformer.Rotation *= quat;
            this.yRotater.Transformer.Rotation *= quat;
            this.zRotater.Transformer.Rotation *= quat;
            if (this.targetModel != null)
            {
                this.targetModel.Transformer.Rotation *= quat;
                Transformed(this, new TransformChangedEventArgs(this.targetModel, TransformType.Rotation));
                if (this.transformChanged != null)
                    this.transformChanged(this, new TransformChangedEventArgs(this.targetModel, TransformType.Rotation));
            }
        }

        private void setTranslationProperty(Vector3 trans)
        {
            trans = Vector3.Lerp(Vector3.Zero, trans, this.TransformTranslationSensibility);
            this.sumTranslation += trans;
            foreach (var hitTestable in this.controllers)
            {
                hitTestable.Transformer.Position += trans;
            }
            this.cross.AddTranslation(trans);
            if (this.targetModel != null)
            {
                this.targetModel.Transformer.Position += trans;
                Transformed(this, new TransformChangedEventArgs(this.targetModel, TransformType.Translation));
                if (this.transformChanged != null)
                    this.transformChanged(this,
                        new TransformChangedEventArgs(this.targetModel, TransformType.Translation));
            }
        }

        public void Dispose()
        {
            foreach (var hitTestable in this.controllers)
            {
                this._hitChecker.CheckTargets.Remove(hitTestable);
            }
            this.xRotater.Dispose();
            this.yRotater.Dispose();
            this.zRotater.Dispose();
            this.xTranslater.Dispose();
            this.yTranslater.Dispose();
            this.zTranslater.Dispose();
            this.scaler.Dispose();
            this.cross.Dispose();
        }

        public TransformType Type
        {
            get { return this._type; }
            set
            {
                this._type = value;
                if (this._type.HasFlag(TransformType.Rotation))
                {
                    this.xRotater.Visibility = this.yRotater.Visibility = this.zRotater.Visibility = true;
                }
                else
                {
                    this.xRotater.Visibility = this.yRotater.Visibility = this.zRotater.Visibility = false;
                }
                if (this._type.HasFlag(TransformType.Translation))
                {
                    this.xTranslater.Visibility = this.yTranslater.Visibility = this.zTranslater.Visibility = true;
                }
                else
                {
                    this.xTranslater.Visibility = this.yTranslater.Visibility = this.zTranslater.Visibility = false;
                }
                if (this._type.HasFlag(TransformType.Scaling))
                {
                    this.scaler.Visibility = true;
                }
                else
                {
                    this.scaler.Visibility = false;
                }
            }
        }

        public void setTargetModel(IDrawable drawable, ITransformer transformer = null,
            Action<TransformController, TransformChangedEventArgs> del = null)
        {
            this.transformChanged = del;
            var isChanged = drawable != this.targetModel;
            this.targetModel = null;
            setRotationProperty(Quaternion.Invert(this.sumRotation));
            setTranslationProperty(-this.sumTranslation);
            if (drawable == null) return;
            var trans = transformer == null ? drawable.Transformer : transformer;
            setRotationProperty(trans.Rotation);
            setTranslationProperty(trans.Position);
            this.targetModel = drawable;
            if (isChanged) TargetModelChanged(this, new EventArgs());
        }

        public IDrawable targetModel { get; private set; }

        public bool Visibility { get; set; }
        public string FileName { get; private set; }
        public int SubsetCount { get; private set; }
        public int VertexCount { get; private set; }
        public ITransformer Transformer { get; private set; }

        public void Draw()
        {
            if (this.targetModel == null || !this.targetModel.Visibility) return;
            this.cross.Draw();
            foreach (var hitTestable in this.controllers)
            {
                if (hitTestable.Visibility) hitTestable.Draw();
            }
        }

        public void Update()
        {
        }

        private Quaternion sumRotation = Quaternion.Identity;
        private Vector3 sumTranslation;
        private float sumScaling;
        private TransformType _type;

        public Vector4 SelfShadowColor { get; set; }
        public Vector4 GroundShadowColor { get; set; }
        public bool IsLocked { get; set; }

        public void Selected()
        {
        }
    }
}
