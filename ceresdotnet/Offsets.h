#pragma once

#include "Stdafx.h"
#include <vcclr.h>

#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
#pragma managed(pop)

using namespace System::Runtime::InteropServices;
using namespace System;
using namespace System::Runtime::CompilerServices;


namespace ceresdotnet
{

#pragma region enums
	public enum class DistortionModel :int {
		Unknown = 0,
		Standard = 1,
		AgisoftPhotoscan = 2,
		OpenCVAdvanced = 3
	};
	public enum class CeresCallbackReturnType : int {
		SOLVER_ABORT = ceres::SOLVER_ABORT,
		SOLVER_CONTINUE = ceres::SOLVER_CONTINUE,
		SOLVER_TERMINATE_SUCCESSFULLY = ceres::SOLVER_TERMINATE_SUCCESSFULLY
	};
	public enum class IntrinsicsOffsets : int {
		OFFSET_FOCAL_LENGTH_X,
		OFFSET_FOCAL_LENGTH_Y,
		OFFSET_PRINCIPAL_POINT_X,
		OFFSET_PRINCIPAL_POINT_Y,
		OFFSET_K1,
		OFFSET_K2,
		OFFSET_P1,
		OFFSET_P2,
		OFFSET_K3,
		OFFSET_SKEW,
		OFFSET_K4,
		OFFSET_P3,
		OFFSET_P4
	};

	[FlagsAttribute]
	public enum class  BundleIntrinsicsFlags :int {
		None = 0,
		FocalLength = 1,
		PrincipalP = 2,
		R1 = 4,
		R2 = 8,
		P1 = 0x10,
		P2 = 0x20,
		R3 = 0x40,

		SKEW = 0x80,
		R4 = 0x100,
		P3 = 0x200, //s1
		P4 = 0x400, //s2

		R5 = 0x800,
		R6 = 0x1000,
		S1 = 0x200,
		S2 = 0x400,

		ALL = FocalLength | PrincipalP | R1 | R2 | R3 | P1 | P2,
		ALL_STANDARD = FocalLength | PrincipalP | R1 | R2 | R3 | P1 | P2,
		INTERNAL_NODIST = FocalLength | PrincipalP,
		INTERNAL_R1R2 = INTERNAL_NODIST | R1 | R2,
		INTERNAL_R1R2R3 = INTERNAL_R1R2 | R3,
		INTERNAL_R1R2R3R4 = INTERNAL_R1R2R3 | R4,
		INTERNAL_R1R2T1T2 = INTERNAL_R1R2 | P1 | P2,

		ALL_PHOTOSCAN = ALL | SKEW | R4 | P3 | P4,

		ALL_OPENCVADVANCED = FocalLength | PrincipalP | SKEW | R1 | R2 | R3 | R4 | R5 | R6 | P1 | P2 | S1 | S2
	};
	[FlagsAttribute]
	public enum class  BundleWorldCoordinatesFlags :int {
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,

		ALL = 7
	};
	[FlagsAttribute]
	public enum class  BundlePointOrientFlags :int {
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		Rodr1 = 8,
		Rodr2 = 16,
		Rodr3 = 32,

		Position = X | Y | Z,
		Orientation = Rodr1 | Rodr2 | Rodr3,
		ALL = Position | Orientation
	};



#pragma endregion
}