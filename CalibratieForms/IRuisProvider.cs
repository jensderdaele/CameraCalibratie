using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalibratieForms.Annotations;
using System.Drawing;
using Calibratie;

namespace CalibratieForms {
    public interface I3DRuisProvider {
        void Apply(ref double x, ref double y, ref double z);
        void Apply(SPoint x);
    }
    public interface I2DRuisProvider {
        void Apply(ref double x, ref double y);
        void Apply(ref float x, ref float y);
        void Apply(ref PointF x);
    }

    public interface IExternalPosRuisProvider {
        void Apply(SObjectBase X);
    }

    /// <summary>
    /// ruis wordt groter naar rand van sensor, not implemented
    /// </summary>
    public class BorderRuisProvider : I2DRuisProvider {
        private int _sensorWidth, _sensorHeight;
        private GaussRuisProvider gauss = new GaussRuisProvider(0, 50);

        public BorderRuisProvider(int width, int height) {
            _sensorWidth = width;
            _sensorHeight = height;
        }
        public void Apply(ref double x, ref double y) {
            throw new NotImplementedException();
        }

        public void Apply(ref float x, ref float y) {
            throw new NotImplementedException();
        }

        public void Apply(ref PointF x) {
            throw new NotImplementedException();
            //if(x<_sensorWidth/5 || x>(_sensorWidth*4/5)
        }
    }
    
    /// <summary>
    /// Voorlopig geen ruis op orientatie, enkel pos
    /// </summary>
    public class GaussRuisProvider : I3DRuisProvider, I2DRuisProvider, IExternalPosRuisProvider {
        public double mean, standard_deviation;
        public GaussRuisProvider(double mean, double standard_deviation) {
            this.mean = mean;
            this.standard_deviation = standard_deviation;
        }
        public void Apply(ref double x) {
            x += Util.NextGaussian(mean, standard_deviation);
        }

        public void Apply(ref double x, ref double y) {
            Apply(ref x);
        }

        public void Apply(ref double x, ref double y, ref double z) {
            Apply(ref x, ref y);
            z += Util.NextGaussian(mean, standard_deviation);
        }

        public void Apply(SPoint x) {
            x.X = x.X + (float)Util.NextGaussian(mean, standard_deviation);
            x.Y = x.Y + (float)Util.NextGaussian(mean, standard_deviation);
            x.Z = x.Z + (float)Util.NextGaussian(mean, standard_deviation);
        }
        

        public void Apply(ref float x, ref float y) {
            throw new NotImplementedException();
        }

        public void Apply(ref PointF x) {
            x.X = x.X + (float)Util.NextGaussian(mean, standard_deviation);
            x.Y = x.Y + (float)Util.NextGaussian(mean, standard_deviation);
        }

        public void Apply(SObjectBase X) {
            var r = (SPoint) X;
            Apply(r);
        }
    }

    public class GaussStoredRuisProvider<T> : GaussRuisProvider {
        public GaussStoredRuisProvider(double mean, double standard_deviation) : base(mean, standard_deviation) {}
    }
}
