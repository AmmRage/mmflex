using System;
using System.Windows.Forms;
using MMF;
using MMF.Controls.Forms;
using MMF.Model.PMX;

namespace _02_SimpleRenderPMX
{
    public partial class Form1 : RenderForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        //①Form.OnLoadをオーバーライドする。
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e); //The RenderForm Initializes the 3D space within the OnLoad for the base.OnLoadがOnLoad内で一番初めに呼ぶべきである。

            //The file open dialog
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
            //OK if the return value of the dialog to model loading process


                //②モデルを読み込む
                PMXModel model = PMXModelWithPhysics.OpenLoad(ofd.FileName, RenderContext);//MMDModel MMDModelWithPhysics.OpenLoad(string ファイル名,RenderContext);となっています
                //Possible to disable physics if you write the following in the load
                //MMDModel model=MMDModel.OpenLoad(ofd.FileName, RenderContext);
                //RenderContext is required to draw the camera or device information, including 3D variable。RenderFormを継承している場合、メンバー変数として利用可能である。
                //To request the value of RenderContext DirectX11 device information to hold a basically loads, such as those that require a lot。

                //③ワールド空間にモデルを追加する
                WorldSpace.AddResource(model);
                //WorldSpace shows the 3D space to draw on this form.。ここにモデルなど(IDrawableを実装している)ものを渡すと、描画してくれる。
                //WorldSpace is a ScreenContext.WorldSpaceと常に等しい。ウィンドウごとに必要な3DCG描画に必要な情報はScreenContextに保管されている。
            }
        }
    }
}