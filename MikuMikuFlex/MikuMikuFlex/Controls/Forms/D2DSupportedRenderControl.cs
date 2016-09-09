#define VSG_DEBUG
using System;
using MMF.Sprite;
using SlimDX.DXGI;

namespace MMF.Controls.Forms
{
    /// <summary>
    ///     Direct2D/DirectWriteによる2Dを3DCG画面に重ねる描画をするクラス
    /// </summary>
    public class D2DSupportedRenderControl : RenderControl
    {
        /// <summary>
        ///     Gets the class to use to display the sprite
        /// </summary>
        /// <value>
        ///     The return is<see cref="D2DSpriteBatch" />をご覧ください。
        /// </value>
        public D2DSpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        ///     Is the handler to update the sprite
        /// </summary>
        /// <value>
        ///     void XXX(D2DSpriteBatch hoge);类型代表
        /// </value>
        public Action<D2DSpriteBatch> DrawSpriteHandler { get; set; }

        public override void Render()
        {
            if (!this.IsInitialized || !this.Visible) return;
            this.RenderContext.SetRenderScreen(this.ScreenContext);
            this.ScreenContext.MoveCameraByCameraMotionProvider();
            this.RenderContext.Timer.TickUpdater();
            this.FpsCounter.CountFrame();
            ClearViews();
            this.ScreenContext.WorldSpace.DrawAllResources(this.ScreenContext.HitChekcer);

#if VSG_DEBUG
#else
            SpriteBatch.Begin();
            if (DrawSpriteHandler != null) DrawSpriteHandler(SpriteBatch);
            SpriteBatch.End();
#endif

            this.ScreenContext.SwapChain.Present(0, PresentFlags.None);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

#if VSG_DEBUG
#else
if(SpriteBatch!=null)SpriteBatch.Resize();
#endif
        }

        public override void Initialize(RenderContext context = null)
        {
            base.Initialize(context);

#if VSG_DEBUG
#else
SpriteBatch=new D2DSpriteBatch(RenderContext);
#endif
        }

        protected override void Dispose(bool disposing)
        {
            if (this.SpriteBatch != null) this.SpriteBatch.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // D2DSupportedRenderControl
            // 
            this.Name = "D2DSupportedRenderControl";
            this.ResumeLayout(false);

        }
    }
}