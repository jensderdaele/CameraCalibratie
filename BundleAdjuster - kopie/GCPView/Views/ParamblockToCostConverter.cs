using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Calibratie;

namespace BundleAdjuster.GCPView.Views
{
    public class ParamblockToCostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is GCP gcp) {
                var r =  BP.Bundler.GetCostForParameterblock(gcp.AdjustedPosition);
                return (double)r;
            }
            return "E541893";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
