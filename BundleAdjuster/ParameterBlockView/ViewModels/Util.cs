using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ceresdotnet;
namespace BundleAdjuster.ParameterBlockView.ViewModels
{
    public static class Util {
        public static readonly IEnumerable<Enum> _intrFlagTypes = new[] {
            BundleIntrinsicsFlags.FocalLength, BundleIntrinsicsFlags.PrincipalP,
            BundleIntrinsicsFlags.R1, BundleIntrinsicsFlags.R2, BundleIntrinsicsFlags.R3, BundleIntrinsicsFlags.R4,
            BundleIntrinsicsFlags.R5, BundleIntrinsicsFlags.R6,
            BundleIntrinsicsFlags.P1, BundleIntrinsicsFlags.P2, BundleIntrinsicsFlags.P3, BundleIntrinsicsFlags.P4,
            BundleIntrinsicsFlags.S1, BundleIntrinsicsFlags.S2, BundleIntrinsicsFlags.SKEW
        }.Cast<Enum>();

        public static readonly IEnumerable<Enum> _posFlagTypes = new[] {
            BundleWorldCoordinatesFlags.X,BundleWorldCoordinatesFlags.Y,BundleWorldCoordinatesFlags.Z
        }.Cast<Enum>();

        public static readonly IEnumerable<Enum> _posOrientFlagTypes = new[] {
            BundlePointOrientFlags.X,BundlePointOrientFlags.Y,BundlePointOrientFlags.Z,BundlePointOrientFlags.Rodr1,BundlePointOrientFlags.Rodr2,BundlePointOrientFlags.Rodr3
        }.Cast<Enum>();

        public static readonly IEnumerable<Enum> _transfFlagTypes = new[] {
            BundleTransformationFlags.X,BundleTransformationFlags.Y,BundleTransformationFlags.Z,
            BundleTransformationFlags.Rodr1,BundleTransformationFlags.Rodr2,BundleTransformationFlags.Rodr3,
            BundleTransformationFlags.Scale
        }.Cast<Enum>();


        public static IEnumerable<Enum> GetFlagTypesForParamblock(ICeresParameterblock block) {
            if (block is ICeresIntrinsics) {
                return _intrFlagTypes;
            }
            if (block is ICeresScaleTransform) {
                return _transfFlagTypes;
            }
            if (block is ICeresPointOrient) {
                return _posOrientFlagTypes;
            }
            if(block is ICeresPoint) {
                return _posFlagTypes;
            }
            throw new Exception("UNKN BLOCKTYPE");
        }

        //public static readonly IEnumerable<BundlePointOrientFlags> _pointOrientFlagTypes = Enum.GetValues(typeof(BundlePointOrientFlags)).Cast<BundlePointOrientFlags>();
        //public static readonly IEnumerable<BundleWorldCoordinatesFlags> _worldCFlagTypes = Enum.GetValues(typeof(BundleWorldCoordinatesFlags)).Cast<BundleWorldCoordinatesFlags>();
        //public static readonly IEnumerable<BundleTransformationFlags> _bundleTransformationFlagTypes = Enum.GetValues(typeof(BundleTransformationFlags)).Cast<BundleTransformationFlags>();
    }
}
