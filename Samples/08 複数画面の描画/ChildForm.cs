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
    public partial class ChildForm :D2DSupportedRenderForm
    {
        //③ RenderContextを渡すと、そのRenderContextを用いて初期化してくれる。
        //In other words, using the same device initialization。
        public ChildForm(RenderContext context) : base(context)
        {
            InitializeComponent();
        }

        protected override void RenderSprite()
        {
            
        }
    }
}
