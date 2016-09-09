using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BasicSound;
using MmfTest.Properties;
using MMF;
using MMF.Bone;
using MMF.DeviceManager;
using MMF.Matricies.Camera.CameraMotion;
using MMF.Model.Assimp;
using MMF.Model.Controller;
using MMF.Model.PMX;
using MMF.Motion;
using SlimDX;

namespace CGTest
{
    public partial class ControlForm : Form
    {
        public RenderContext Context { get; private set; }

        public PMXModel Model { get; private set; }

        public IMotionProvider CurrentMotion { get; private set; }

        public bool IsPlaying { get; private set; }

        private readonly ScreenContext _scContext;
        private readonly ITargetContext _sccContext;

        private readonly float _lightTransvalue = 1.0f;

        public ControlForm(RenderContext context, ScreenContext scContext, ITargetContext sccContext)
        {
            this.Context = context;
            this._scContext = scContext;
            this._sccContext = sccContext;
            InitializeComponent();
            this.frameSelector.ValueChanged += frameSelector_ValueChanged;
            this.play.Click += play_Click;
            this.stop.Click += stop_Click;
            UpdatePositionData();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 800);
        }

        private void UpdatePositionData()
        {
            Vector3 position = this.Context.LightManager.Position;
            this.light_position.Text = string.Format("POSITION:{0},DIRECTION:{1}", position.ToString(),
                this.Context.LightManager.Direction);
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (this.CurrentMotion != null)
            {
                this.CurrentMotion.Stop();
                this.IsPlaying = false;
            }
        }

        private void play_Click(object sender, EventArgs e)
        {
            if (this.CurrentMotion != null)
            {
                this.CurrentMotion.Start(this.CurrentMotion.CurrentFrame, ActionAfterMotion.Replay);
                this.IsPlaying = true;
                AudioPlayer.PlayMp3Async(@"C:\Users\ZhiYong\Music\ge\ts\1989\06 Shake It Off.mp3");
            }
        }

        private void frameSelector_ValueChanged(object sender, EventArgs e)
        {
            if (!this.IsPlaying && this.CurrentMotion != null)
            {
                this.CurrentMotion.CurrentFrame = this.frameSelector.Value;
                this.frameLabel.Text = string.Format("{0}Frame in、{1}Th frame", this.CurrentMotion.FinalFrame,
                    this.CurrentMotion.CurrentFrame.ToString("#.0"));
            }
        }

        private void Model_Load_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                RestoreDirectory = true,
                Title = "PMX file of the target to open",
                Filter = "PMX model file(*.pmx)|*.pmx"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (this.Model != null)
                {
                    this._scContext.WorldSpace.RemoveResource(this.Model);
                }
                this.Model = PMXModelWithPhysics.OpenLoad(ofd.FileName, this.Context);
                this.Model.Transformer.Position = new Vector3(0, 0, 0);
                this._scContext.WorldSpace.AddResource(this.Model);
                MainForm.Controller.setTargetModel(this.Model);
                MainForm.Controller.Type = TransformController.TransformType.Translation;
                this.Motion_Load.Enabled = true;
                //this.IsPlaying = true;
                Settings.Default.InitLoadModel = ofd.FileName;
            }
        }

        private void Motion_Load_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                RestoreDirectory = true,
                Title = "VMD / VME file of the target to open",
                Filter = "VMD motion files (*.vmd)|*.vmd| VME motion files(*.vme)|*.vme",
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.CurrentMotion = this.Model.MotionManager.AddMotionFromFile(ofd.FileName, false);
                this.Model.MotionManager.ApplyMotion(this.CurrentMotion, 0, ActionAfterMotion.Replay);
                this.frameSelector.Maximum = this.CurrentMotion.FinalFrame;
                this.frameSelector.Minimum = 0;
                this.CurrentMotion.FrameTicked += CurrentMotion_FrameTicked;
                Settings.Default.InitLoadMotion = ofd.FileName;

                #region VMDCamera motion test code
                //VMDCameraMotionProvider provider = new VMDCameraMotionProvider(MMDFileParser.MotionParser.MotionData.getMotion(File.OpenRead(@"C:\Users\Lime\Desktop\ハレ晴レユカイ\camera.vmd")));
                //this._scContext.CameraMotionProvider = provider;
                //provider.Start();
                #endregion
            }
        }

        private void CurrentMotion_FrameTicked(object sender, EventArgs e)
        {
            if (this.CurrentMotion == null) return;
            Invoke((MethodInvoker) delegate
            {
                if (this.frameLabel.IsDisposed || this.frameSelector.IsDisposed) return;
                this.frameLabel.Text = string.Format("{0}In the frame, {1} th frame", this.CurrentMotion.FinalFrame,
                    this.CurrentMotion.CurrentFrame.ToString("#.0"));
                this.frameSelector.Value = (int) this.CurrentMotion.CurrentFrame;
            });
        }

        private void ControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
        
        private void doTrack_Click(object sender, EventArgs e)
        {
        }

        private void reload_effect_Click(object sender, EventArgs e)
        {
            if (this.Model != null)
            {
                this.Model.LoadEffect("");
            }
        }

        private void trans_x_plus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(1, 0, 0)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void trans_y_plus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(0, 1, 0)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void trans_z_plus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(0, 0, 1)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void trans_x_minus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(-1, 0, 0)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void trans_y_minus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(0, -1, 0)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void trans_z_minus_Click(object sender, EventArgs e)
        {
            this.Context.LightManager.Position += new Vector3(0, 0, -1)*this._lightTransvalue;
            UpdatePositionData();
        }

        private void Add2Child_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "PMX file of the target to open";
            ofd.Filter = "PMX model file (*.pmx)|*.pmx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (this.Model != null)
                {
                    this._sccContext.WorldSpace.RemoveResource(this.Model);
                }
                this.Model = PMXModelWithPhysics.OpenLoad(ofd.FileName, this.Context);
                this.Model.Transformer.Position = new Vector3(0, 0, 0);

                this._sccContext.WorldSpace.AddResource(this.Model);
                this.Motion_Load.Enabled = true;
                this.IsPlaying = true;
            }
        }

        private void AddAssimpButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AssimpModel assimp = new AssimpModel(this.Context, ofd.FileName);
                this._scContext.WorldSpace.AddResource(assimp);
            }
        }
    }
}
