using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ceresdotnet;
using Calibratie;
using Emgu.CV;

namespace BundleAdjuster.GCPView.Views
{
    class ICeresGCPToAdjustedPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICeresGCP gcp) {
                var a = new ceresdotnet.CeresScaledTransformation(gcp.Transformation);
                var b = new Matrix<double>(new [] {gcp.Triangulated.X, gcp.Triangulated.Y, gcp.Triangulated.Z});

                var proj = ceresdotnet.CeresTestFunctions.TransformGCP(b, a);
                return $"{proj[0]:0.00} - {proj[1]:0.00} - {proj[2]:0.00}";
            }
            return "E541893";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
