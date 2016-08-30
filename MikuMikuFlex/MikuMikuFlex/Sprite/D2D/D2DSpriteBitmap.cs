using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.DXGI;
using Bitmap = System.Drawing.Bitmap;
using PixelFormat = SlimDX.Direct2D.PixelFormat;

namespace MMF.Sprite.D2D
{
    /// <summary>
    /// Bitmap import helper class
    /// </summary>
    public class D2DSpriteBitmap:IDisposable
    {
        ~D2DSpriteBitmap()
        {
            Debug.WriteLine("D2DSpriteBitmapはIDisposableですが、Disposeされませんでした。");
            if (this.SpriteBitmap != null && !this.SpriteBitmap.Disposed) this.SpriteBitmap.Dispose();
        }

        private Bitmap orgBitmap;
        private D2DSpriteBatch batch;
        public SlimDX.Direct2D.Bitmap SpriteBitmap { get; private set; }



        internal D2DSpriteBitmap(D2DSpriteBatch batch,Stream fs)
        {
            this.batch = batch;
            batch.BatchDisposing += batch_BatchDisposing;
            this.orgBitmap = (Bitmap)Image.FromStream(fs);
            CreateBitmap();
        }

        void batch_BatchDisposing(object sender, EventArgs e)
        {
            Dispose();
        }


        private void CreateBitmap()
        {
            BitmapData bitmapData = this.orgBitmap.LockBits(new Rectangle(0, 0, this.orgBitmap.Width, this.orgBitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (DataStream dataStream = new DataStream(bitmapData.Scan0, bitmapData.Stride*bitmapData.Height, true, false))
            {
                PixelFormat format = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
                BitmapProperties properties = new BitmapProperties();
                properties.HorizontalDpi = properties.VerticalDpi = 96;
                properties.PixelFormat = format;
                if (this.SpriteBitmap != null && !this.SpriteBitmap.Disposed) this.SpriteBitmap.Dispose();
                this.SpriteBitmap = new SlimDX.Direct2D.Bitmap(this.batch.DWRenderTarget, new Size(this.orgBitmap.Width, this.orgBitmap.Height),
                    dataStream, bitmapData.Stride, properties);
                this.orgBitmap.UnlockBits(bitmapData);
            }
        }

        public void Dispose()
        {
            if (this.SpriteBitmap != null && !this.SpriteBitmap.Disposed) this.SpriteBitmap.Dispose();
            GC.SuppressFinalize(this);
        }

        public static implicit operator SlimDX.Direct2D.Bitmap(D2DSpriteBitmap bitmap)
        {
            return bitmap.SpriteBitmap;
        }
    }
}