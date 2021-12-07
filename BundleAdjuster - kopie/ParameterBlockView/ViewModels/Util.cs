using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ceresdotnet;
namespace BundleAdjuster.ParameterBlockView.ViewModels
{
    public static class Util {
        public static readonly IEnumerable<BundleIntrinsicsFlags> _intrFlagTypes = new[] {
            BundleIntrinsicsFlags.FocalLength, BundleIntrinsicsFlags.PrincipalP,
            BundleIntrinsicsFlags.R1, BundleIntrinsicsFlags.R2, BundleIntrinsicsFlags.R3, BundleIntrinsicsFlags.R4,
            BundleIntrinsicsFlags.R5, BundleIntrinsicsFlags.R6,
            BundleIntrinsicsFlags.P1, BundleIntrinsicsFlags.P2, BundleIntrinsicsFlags.P3, BundleIntrinsicsFlags.P4,
            BundleIntrinsicsFlags.S1, BundleIntrinsicsFlags.S2, BundleIntrinsicsFlags.SKEW
        };

        public static IEnumerable<Enum> GetFlagTypesForParamblock() {
            return _intrFlagTypes.Cast<Enum>();
        }

        //public static readonly IEnumerable<BundlePointOrientFlags> _pointOrientFlagTypes = Enum.GetValues(typeof(BundlePointOrientFlags)).Cast<BundlePointOrientFlags>();
        //public static readonly IEnumerable<BundleWorldCoordinatesFlags> _worldCFlagTypes = Enum.GetValues(typeof(BundleWorldCoordinatesFlags)).Cast<BundleWorldCoordinatesFlags>();
        //public static readonly IEnumerable<BundleTransformationFlags> _bundleTransformationFlagTypes = Enum.GetValues(typeof(BundleTransformationFlags)).Cast<BundleTransformationFlags>();
    }
}
