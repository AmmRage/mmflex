using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MMF.Utility
{
    /// <summary>
    /// tgaRead the DX11 files could not be loaded because once converted to png from
    /// </summary>
    public static class TargaSolver
    {
        public static Stream LoadTargaImage(string filePath,ImageFormat rootFormat=null)
        {
            Bitmap tgaFile = null;
            if (rootFormat == null) rootFormat = ImageFormat.Png;
            try
            {
                tgaFile = Paloma.TargaImage.LoadTargaImage(filePath);
            }
            catch (Exception ns)
            {//When the TGA format
                return File.OpenRead(filePath);
            }
            MemoryStream ms=new MemoryStream();
                tgaFile.Save(ms,rootFormat);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
        }
    }
}
