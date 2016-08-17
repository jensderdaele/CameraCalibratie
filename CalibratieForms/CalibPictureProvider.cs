using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace cameracallibratie {
    public abstract class CalibPictureProvider {
        
        public abstract IEnumerable<Bitmap> pictures8bit { get; }

        public IEnumerable<InputArray> picturesCvArray {
            get {
                return pictures8bit.Select(x => {
                    var m = new MatOfByte(x.Height, x.Width);
                    x.ToMat(m);
                    return (InputArray) m;
                }
                    );
            }
        }
    }

    public class PhotoProvider : CalibPictureProvider {
        public static InputArray getSingleImage(string path, int scaleDown = 1) {
            var image = new Bitmap(path);
            if (scaleDown != 1) {
                image = ResizeImage(image, image.Width/scaleDown, image.Height/scaleDown);
            }
            
            //var newpathname = path +scaleDown+ "temp.bmp";
            //image.Save(newpathname,ImageFormat.Bmp);
            //var mat = Cv2.ImRead(newpathname);
            //File.Delete(newpathname);
            //return mat;
            var image8bit = CopyToBpp(image, 8);
            var m = new MatOfByte(image8bit.Height, image8bit.Width);
            image8bit.ToMat(m);
            
            return m;
        }
        public static InputArray getSingleImage(string path,out OpenCvSharp.Size imSize, int scaleDown = 1) {
            var image = new Bitmap(path);
            imSize = new OpenCvSharp.Size(image.Width, image.Height);
            if (scaleDown != 1) {
                image = ResizeImage(image, image.Width / scaleDown, image.Height / scaleDown);
            }
            
            //var newpathname = path +scaleDown+ "temp.bmp";
            //image.Save(newpathname,ImageFormat.Bmp);
            //var mat = Cv2.ImRead(newpathname);
            //File.Delete(newpathname);
            //return mat;
            var image8bit = CopyToBpp(image, 8);
            var m = new MatOfByte(image8bit.Height, image8bit.Width);
            image8bit.ToMat(m);
            return m;
        }

        public static Bitmap  getSingleBitmap(string path, int scaleDown = 1) {
            var image = new Bitmap(path);
            if (scaleDown > 1) {
                image = ResizeImage(image, image.Width / scaleDown, image.Height / scaleDown);
            }
            return image;
        }

        public string FolderName { get; private set; }
        public int ScaleDown { get; set; }


        public PhotoProvider(string folderName) : this(folderName,1) {}
        public PhotoProvider(string folderName, int scaleDown) {
            FolderName = folderName;
            ScaleDown = scaleDown;
        }

        public IEnumerable<Bitmap> pictures {
            get {
                return ScaleDown == 1 ? 
                    getImageFiles().Select(imageFile => new Bitmap(imageFile)):
                    getImageFiles().Select(imageFile => {
                        var original = new Bitmap(imageFile);
                        return ResizeImage(original, original.Width / ScaleDown, original.Height / ScaleDown);
                    });
            }
        }
        public override IEnumerable<Bitmap> pictures8bit {
            get { return pictures.Select(imageFile => CopyToBpp(imageFile, 8)); }
        }

        public IEnumerable<string> getImageFiles() {
            var Folder = new DirectoryInfo(FolderName);
            var Images = Folder.GetFiles();
            return Images.Select(t => String.Format(@"{0}/{1}", FolderName, t.Name));
        }

        /// <summary>
        /// Resize the image to the specified width and height. 
        /// van www.stackoverflow.com
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #region convert to 8bit
        private static Bitmap CopyToBpp(Bitmap b, int bpp) {
            if (bpp != 1 && bpp != 8)
                throw new ArgumentException("1 or 8", "bpp");

            // Plan: built into Windows GDI is the ability to convert
            // bitmaps from one format to another. Most of the time, this
            // job is actually done by the graphics hardware accelerator card
            // and so is extremely fast. The rest of the time, the job is done by
            // very fast native code.
            // We will call into this GDI functionality from C#. Our plan:
            // (1) Convert our Bitmap into a GDI hbitmap (ie. copy unmanaged->managed)
            // (2) Create a GDI monochrome hbitmap
            // (3) Use GDI "BitBlt" function to copy from hbitmap into monochrome (as above)
            // (4) Convert the monochrone hbitmap into a Bitmap (ie. copy unmanaged->managed)

            int w = b.Width, h = b.Height;
            IntPtr hbm = b.GetHbitmap(); // this is step (1)
            //
            // Step (2): create the monochrome bitmap.
            // "BITMAPINFO" is an interop-struct which we define below.
            // In GDI terms, it's a BITMAPHEADERINFO followed by an array of two RGBQUADs
            BITMAPINFO bmi = new BITMAPINFO();
            bmi.biSize = 40; // the size of the BITMAPHEADERINFO struct
            bmi.biWidth = w;
            bmi.biHeight = h;
            bmi.biPlanes = 1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
            bmi.biBitCount = (short)bpp; // ie. 1bpp or 8bpp
            bmi.biCompression = BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
            bmi.biSizeImage = (uint)(((w + 7) & 0xFFFFFFF8) * h / 8);
            bmi.biXPelsPerMeter = 1000000; // not really important
            bmi.biYPelsPerMeter = 1000000; // not really important
            // Now for the colour table.
            uint ncols = (uint)1 << bpp; // 2 colours for 1bpp; 256 colours for 8bpp
            bmi.biClrUsed = ncols;
            bmi.biClrImportant = ncols;
            bmi.cols = new uint[256]; // The structure always has fixed size 256, even if we end up using fewer colours
            if (bpp == 1) {
                bmi.cols[0] = MAKERGB(0, 0, 0);
                bmi.cols[1] = MAKERGB(255, 255, 255);
            }
            else {
                for (int i = 0; i < ncols; i++)
                    bmi.cols[i] = MAKERGB(i, i, i);
            }
            // For 8bpp we've created an palette with just greyscale colours.
            // You can set up any palette you want here. Here are some possibilities:
            // greyscale: for (int i=0; i<256; i++) bmi.cols[i]=MAKERGB(i,i,i);
            // rainbow: bmi.biClrUsed=216; bmi.biClrImportant=216; int[] colv=new int[6]{0,51,102,153,204,255};
            //          for (int i=0; i<216; i++) bmi.cols[i]=MAKERGB(colv[i/36],colv[(i/6)%6],colv[i%6]);
            // optimal: a difficult topic: http://en.wikipedia.org/wiki/Color_quantization
            // 
            // Now create the indexed bitmap "hbm0"
            IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
            IntPtr hbm0 = CreateDIBSection(IntPtr.Zero, ref bmi, DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
            //
            // Step (3): use GDI's BitBlt function to copy from original hbitmap into monocrhome bitmap
            // GDI programming is kind of confusing... nb. The GDI equivalent of "Graphics" is called a "DC".
            IntPtr sdc = GetDC(IntPtr.Zero); // First we obtain the DC for the screen
            // Next, create a DC for the original hbitmap
            IntPtr hdc = CreateCompatibleDC(sdc);
            SelectObject(hdc, hbm);
            // and create a DC for the monochrome hbitmap
            IntPtr hdc0 = CreateCompatibleDC(sdc);
            SelectObject(hdc0, hbm0);
            // Now we can do the BitBlt:
            BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, SRCCOPY);
            // Step (4): convert this monochrome hbitmap back into a Bitmap:
            Bitmap b0 = Bitmap.FromHbitmap(hbm0);
            //
            // Finally some cleanup.
            DeleteDC(hdc);
            DeleteDC(hdc0);
            ReleaseDC(IntPtr.Zero, sdc);
            DeleteObject(hbm);
            DeleteObject(hbm0);
            //
            return b0;
        }

        /// <summary>
        /// Draws a bitmap onto the screen. Note: this will be overpainted
        /// by other windows when they come to draw themselves. Only use it
        /// if you want to draw something quickly and can't be bothered with forms.
        /// </summary>
        /// <param name="b">the bitmap to draw on the screen</param>
        /// <param name="x">x screen coordinate</param>
        /// <param name="y">y screen coordinate</param>
        private static void SplashImage(Bitmap b, int x, int y) {
            // Drawing onto the screen is supported by GDI, but not by the Bitmap/Graphics class.
            // So we use interop:
            // (1) Copy the Bitmap into a GDI hbitmap
            IntPtr hbm = b.GetHbitmap();
            // (2) obtain the GDI equivalent of a "Graphics" for the screen
            IntPtr sdc = GetDC(IntPtr.Zero);
            // (3) obtain the GDI equivalent of a "Graphics" for the hbitmap
            IntPtr hdc = CreateCompatibleDC(sdc);
            SelectObject(hdc, hbm);
            // (4) Draw from the hbitmap's "Graphics" onto the screen's "Graphics"
            BitBlt(sdc, x, y, b.Width, b.Height, hdc, 0, 0, SRCCOPY);
            // and do boring GDI cleanup:
            DeleteDC(hdc);
            ReleaseDC(IntPtr.Zero, sdc);
            DeleteObject(hbm);
        }


        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);


        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hdcDst, int xDst, int yDst, int w, int h, IntPtr hdcSrc, int xSrc,
            int ySrc, int rop);

        private static int SRCCOPY = 0x00CC0020;

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits,
            IntPtr hSection, uint dwOffset);

        private static uint BI_RGB = 0;
        private static uint DIB_RGB_COLORS = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO {
            public uint biSize;
            public int biWidth, biHeight;
            public short biPlanes, biBitCount;
            public uint biCompression, biSizeImage;
            public int biXPelsPerMeter, biYPelsPerMeter;
            public uint biClrUsed, biClrImportant;

            [MarshalAs(UnmanagedType.ByValArray,
                SizeConst = 256)]
            public uint[] cols;
        }

        private static uint MAKERGB(int r, int g, int b) {
            return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
        }
        #endregion
    }
}