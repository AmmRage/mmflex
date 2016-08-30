using System;
using System.Diagnostics;
using SlimDX;
using SlimDX.Direct2D;

namespace MMF.Sprite.D2D
{
    public class D2DSpriteSolidColorBrush : IDisposable
    {
        private readonly D2DSpriteBatch batch;
        private Color4 color;

        internal D2DSpriteSolidColorBrush(D2DSpriteBatch batch, Color4 color)
        {
            this.batch = batch;
            this.color = color;
            batch.BatchDisposing += batch_BatchDisposing;
            this.Brush = new SolidColorBrush(batch.DWRenderTarget, color);
        }

        public Color4 Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                if (this.Brush != null) this.Brush.Color = this.color;
            }
        }

        public SolidColorBrush Brush { get; private set; }

        public void Dispose()
        {
            if (this.Brush != null && !this.Brush.Disposed) this.Brush.Dispose();
            this.batch.BatchDisposing -= batch_BatchDisposing;
            GC.SuppressFinalize(this);
        }

        void batch_BatchDisposing(object sender, EventArgs e)
        {
            Dispose();
        }

        ~D2DSpriteSolidColorBrush()
        {
            if (this.Brush != null && !this.Brush.Disposed) this.Brush.Dispose();
            Debug.WriteLine("D2DSpriteSolidColorBrushはDisposableですがDisposeされませんでした。");
        }

        public static implicit operator SolidColorBrush(D2DSpriteSolidColorBrush brush)
        {
            return brush.Brush;
        }
    }
}