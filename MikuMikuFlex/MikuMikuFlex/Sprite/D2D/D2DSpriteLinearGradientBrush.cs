using System;
using System.Diagnostics;
using SlimDX.Direct2D;

namespace MMF.Sprite.D2D
{
    public class D2DSpriteLinearGradientBrush:IDisposable
    {
        public LinearGradientBrush Brush;
        private D2DSpriteBatch _batch;
        private D2DSpriteGradientStopCollection _gradientStops;
        private LinearGradientBrushProperties _linearGradientBrushProperties;

        public D2DSpriteLinearGradientBrush(D2DSpriteBatch batch, D2DSpriteGradientStopCollection gradientStops,
            LinearGradientBrushProperties lgbp)
        {
            this._linearGradientBrushProperties = lgbp;
            this._gradientStops = gradientStops;
            this._batch = batch;
            batch.BatchDisposing += batch_BatchDisposing;
            this.Brush = new LinearGradientBrush(this._batch.DWRenderTarget, this._gradientStops.GradientStopCollection, this._linearGradientBrushProperties);
        }

        ~D2DSpriteLinearGradientBrush()
        {
            Debug.WriteLine("D2DSpriteLinearGradientBrushはIDisposableですが、Disposeされませんでした。");
            Dispose();
        }

        void batch_BatchDisposing(object sender, EventArgs e)
        {
            Dispose();
        }



        public void Dispose()
        {
            if (this.Brush != null && !this.Brush.Disposed) this.Brush.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
