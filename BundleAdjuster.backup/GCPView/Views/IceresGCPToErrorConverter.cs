using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ceresdotnet;
using Emgu.CV;

namespace BundleAdjuster.GCPView.Views
{
    class IceresGCPToErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICeresGCP gcp)
            {
                var a = new ceresdotnet.CeresScaledTransformation(gcp.Transformation);
                var b = new Matrix<double>(new[] { gcp.Triangulated.X, gcp.Triangulated.Y, gcp.Triangulated.Z });

                var proj = ceresdotnet.CeresTestFunctions.TransformGCP(b, a);
                
                var error = Math.Sqrt(
                    Math.Pow(gcp.observed_x - proj[0], 2)+
                    Math.Pow(gcp.observed_y - proj[1], 2)+
                    Math.Pow(gcp.observed_z - proj[2], 2));
                return error;
            }
            return "E541793";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
