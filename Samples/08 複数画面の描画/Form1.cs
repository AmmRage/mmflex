using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMF;
using MMF.Controls.Forms;

namespace _08_MultiScreenRendering
{
    public partial class Form1 : D2DSupportedRenderForm
    {
        private ChildForm childForm;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            //①-A MMFを利用する別のフォームをのインスタンスを作成する。
            childForm = new ChildForm(RenderContext);
            childForm.Show();
            //①-B ワールド空間を更新対象としてRenderContextに追加する。このとき、WorldSpaceはOnLoadでインスタンスが作成されているので、Show以降に追加する必要がある。
            
            if (MessageBox.Show("共通のワールド空間を利用しますか?", "", MessageBoxButtons.YesNo)==DialogResult.Yes)
            {
                childForm.ScreenContext.WorldSpace = ScreenContext.WorldSpace;//childSpace world space to world space this Form1 of the same。
                //This Form1 in the form initialized this if you do not specify a RenderContext WorldSpace is standard and will be updated for
                //You don't need to add UpdateReqireWorlds。
                //So if you view the same world space has two screens。
            }
            else
            {
                RenderContext.UpdateRequireWorlds.Add(childForm.WorldSpace);//Is required to update the standard non-world space。
            }
            ControllerForm controller=new ControllerForm(this
                ,childForm);
            controller.Show();
        }

        protected override void RenderSprite()
        {
            
        }

        protected override void OnPresented()
        {
            base.OnPresented();
            //①-C 描画の瞬間はここでは、Form1の描画直後にしておく。
            //However, on a drawing done in same RenderContext cannot call render at the same time。
            childForm.Render();//override the OnPresented method that is called after rendering to call the childForm Render。
        }
    }
}
