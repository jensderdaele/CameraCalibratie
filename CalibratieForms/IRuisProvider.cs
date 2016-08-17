using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalibratieForms.Annotations;

namespace CalibratieForms {
    public interface I3DRuisProvider {
        void Apply(ref double x, ref double y, ref double z);
    }
    public interface IPixelRuisProvider {
        void Apply(ref double x, ref double y, double width, double height);
    }
}
