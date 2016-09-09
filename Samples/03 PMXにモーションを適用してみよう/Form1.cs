using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMF;
using MMF.Controls.Forms;
using MMF.Model.PMX;
using MMF.Motion;

namespace _03_ApplyVMDToPMX
{
    public partial class Form1 : RenderForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e); 

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                PMXModel model = PMXModelWithPhysics.OpenLoad(ofd.FileName, RenderContext);

                //Establishment of preclusion to load dialog
                OpenFileDialog ofd2=new OpenFileDialog();
                ofd2.Filter = "vmdモーションファイル(*.vmd)|*.vmd";
                if (ofd2.ShowDialog() == DialogResult.OK)
                {
                    //OK if the return value of the dialog to model loading process

                    //①モーションファイルを読み込む
                    IMotionProvider motion = model.MotionManager.AddMotionFromFile(ofd2.FileName, true);
                    //Add the models for which you want to apply the motion Manager。
                    //IMotionProvider AddMotionFromFile(string ファイル名,bool すべての親ボーンを無視するかどうか);
                    //Walking motion as the second argument to ignore the motion of all parent bones when you want to programmatically specify the movement itself, the
                    //Move the entire model is in motion to prevent。

                    //②モーションファイルをモデルに対して適用する。
                    model.MotionManager.ApplyMotion(motion,0,ActionAfterMotion.Replay);
                    //Whether or not the second argument, replaying plays frame numbers begin to play, and the third argument is。
                    //If you leave without replay is Yctionyftermotion..Nothingを指定する

                    //ｵﾏｹ
                    //(1) モーションをとめるときは?
                    //model.MotionManager.StopMotion();と記述すれば止まります
                    //(2) 現在何フレーム目なの?
                    //model.MotionManager.CurrentFrameによって取得できます。
                }
                WorldSpace.AddResource(model);
            }
        }
    }
}
