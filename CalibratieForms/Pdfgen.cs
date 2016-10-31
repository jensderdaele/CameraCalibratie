using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace CalibratieForms {

    public static partial class EXT {
        public static void ApplyDrawActions(this XGraphics gfx, IEnumerable<Pdfgen.drawAction> actions,int offxmm,int offymm) {
            foreach (var a in actions) {
                a(gfx, offxmm, offymm);
            }
        }
    }

    public class Pdfgen {
        public const int DPI = 72;
        public const int DPIIMAGE = 96;
        public const double MMTOPT = DPI / 25.4;
        public const double PTTOMM = 25.4 / DPI;
        public const string DIR = @"\tempmarkers\";

        
            public delegate void drawAction(XGraphics gfx, int offxmm,int offymm);
        public class MarkerPage {
            public PdfPage Page = new PdfPage();
            internal readonly List<drawAction> _draws = new List<drawAction>(); 
        }
        

        static Pdfgen() {
            Directory.CreateDirectory(DIR);
        }


        public static List<drawAction> MarkerLayoutA3(ref int markerId) {
            List<drawAction> r = new List<drawAction> {
                drawMarkerAction(20, 27, 250, markerId++),
                drawMarkerAction(20 + 250 + 20, 20, 120, markerId++),
                drawMarkerAction(20 + 250 + 20, 20 + 120 + 20, 120, markerId++)
            };
            return r;
        } 

        public static void createMarker(ref int id) {
            var startId = id;
            var doc = new PdfDocument();

            for (int i = 0; i < 24; i++) {
                var page = doc.AddPage();

                page.Size = PageSize.A3;

                page.Orientation = PageOrientation.Landscape;
                var gfx = XGraphics.FromPdfPage(page);

                drawMarker(gfx, 20, 27, 250, id++);
                if (i < 20) {
                    drawMarker(gfx, 20 + 250 + 20, 20, 120, id++);
                    drawMarker(gfx, 20 + 250 + 20, 20 + 120 + 20, 120, id++);
                }
            }
            var f = string.Format(@"C:\Users\jens\Desktop\calibratie\aruco markers\A3\A3.24p.{1}-{0}.pdf", id, startId);
            try { File.Delete(f); }
            catch {
                // ignored
            }
            doc.Save(f);
        }

        public static void createMarkerLayout() {
            var doc = new PdfDocument();
            var markerid = 1;

            var page = doc.AddPage();

            page.Width = 2440 * MMTOPT;
            page.Height = 1220 * MMTOPT;
            //page.Size = PageSize.Undefined;

            //page.Orientation = PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page);

            for (int xoff = 0; xoff < page.Width * PTTOMM; xoff += 420) {
                for (int yoff = 0; yoff < page.Height * PTTOMM; yoff += 297) {
                    gfx.ApplyDrawActions(MarkerLayoutA3(ref markerid), xoff, yoff);
                }
            }


            var f = string.Format(@"C:\Users\jens\Desktop\calibratie\aruco markers\A3\MDF2440x1220LAYOUT.pdf");
            try { File.Delete(f); }
            catch {
                // ignored
            }
            doc.Save(f);
            
        }
        private static XFont _font = new XFont("Verdana", 20, XFontStyle.Bold);

        private static drawAction drawMarkerAction(int x_mm, int y_mm, int sz_mm, int id) {
            int szpt = topxim(sz_mm);
            int xpt = topx(x_mm);
            int ypt = topx(y_mm);

            

            string jpgs = string.Format("{0}Aruco.{1}.{2}mm.jpg", DIR, id, sz_mm);
            string pngs = string.Format("{0}Aruco.{1}.{2}mm.png", DIR, id, sz_mm);
            ArUcoNET.Aruco.CreateMarker(id, szpt, jpgs);
            Image.FromFile(jpgs).Save(pngs, ImageFormat.Png);
            var img = XImage.FromFile(pngs);

            int fontsz = szpt / 30;
            XFont font = new XFont("Verdana", fontsz, XFontStyle.Bold);

            drawAction a = (gfx, xoff, yoff) => {
                xoff = (int)(xoff * MMTOPT);
                yoff = (int)(yoff * MMTOPT);
                gfx.DrawImage(img, xpt + xoff, ypt + yoff);
                gfx.DrawString(string.Format("aruco {0}", id), font, XBrushes.Black, xpt + szpt / 2 - 10 * fontsz * xoff, ypt - fontsz + yoff);
            };

            return a;
        }
        private static void drawMarker(XGraphics gfx, int x_mm, int y_mm, int sz_mm, int id) {
            drawMarkerAction(x_mm, y_mm, sz_mm, id)(gfx,0,0);
        }

        private static int topx(int mm) {
            return (int)((mm*DPI)/25.4);
        }
        private static int topxim(int mm) {
            return (int)((mm * DPIIMAGE) / 25.4);
        }

    }
}
