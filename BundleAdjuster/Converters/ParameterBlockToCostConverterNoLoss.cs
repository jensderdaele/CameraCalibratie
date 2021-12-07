using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ceresdotnet;

namespace BundleAdjuster.Converters
{
    class ParameterBlockToCostConverterNoLoss : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICeresParameterblock p)
            {
                var r = BP.Bundler.GetCostForParameterblock(p,false);
                return (double)r;
            }
            return (double)0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
