using System;
using System.Windows.Forms;
using MMF;
using SlimDX.Windows;
using RenderForm = MMF.Controls.Forms.RenderForm;

namespace MMF
{
    /// <summary>
    /// Class that provides methods for turning the draw loop
    /// </summary>
    public static class MessagePump
    {

        public static void Run(RenderForm form)
        {
            SlimDX.Windows.MessagePump.Run(form,form.Render);
        }

        public static void Run(Form form, MainLoop renderMethod)
        {
            SlimDX.Windows.MessagePump.Run(form,renderMethod);
        }
    }
}
