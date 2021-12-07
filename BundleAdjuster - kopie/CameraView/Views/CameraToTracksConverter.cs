using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Calibratie;

namespace BundleAdjuster.CameraView.Views
{
    public class CameraToTracksConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cam = value as PinholeCamera;
            if (cam != null)
            {
                return BP.Bundler.GetObservationList(cam).Count;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
