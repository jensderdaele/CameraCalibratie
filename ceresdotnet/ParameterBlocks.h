#pragma once
#include "Stdafx.h"
#include <vcclr.h>


#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
#pragma managed(pop)


using namespace System::Collections::Generic;
//using namespace Emgu::CV;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
using namespace System;
using namespace System::Runtime::CompilerServices;


//TODO: BUNDLEFLAGS in CeresParameterBlock
namespace ceresdotnet {

	enum {
		OFFSET_RODR1,
		OFFSET_RODR2,
		OFFSET_RODR3,
		OFFSET_T1,
		OFFSET_T2,
		OFFSET_T3,
		OFFSET_SCALE
	};

	public enum class DistortionModel :int {
		Unknown = 0,
		Standard = 1,
		AgisoftPhotoscan = 2,
		OpenCVAdvanced = 3
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
		S1 = 0x2000,
		S2 = 0x4000,

		ALL_STANDARD = FocalLength | PrincipalP | R1 | R2 | R3 | P1 | P2,
		INTERNAL_NODIST = FocalLength | PrincipalP,
		INTERNAL_R1R2 = INTERNAL_NODIST | R1 | R2,
		INTERNAL_R1R2R3 = INTERNAL_R1R2 | R3,
		INTERNAL_R1R2R3R4 = INTERNAL_R1R2R3 | R4,
		INTERNAL_R1R2T1T2 = INTERNAL_R1R2 | P1 | P2,

		ALL_PHOTOSCAN = ALL_STANDARD | SKEW | R4 | P3 | P4,

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
	[FlagsAttribute]
	public enum class  BundleTransformationFlags :int {
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		Rodr1 = 8,
		Rodr2 = 16,
		Rodr3 = 32,
		Scale = 64,
		Position = X | Y | Z,
		Orientation = Rodr1 | Rodr2 | Rodr3,
		ALL = Position | Orientation | Scale
	};
	
	
	public interface class ICeresParameterblock {
		property Enum^ BundleFlags {
			Enum^ get();
			void set(Enum^ v);
		}
	};
	public ref class CeresParameterBlock abstract {
	public:
		double* _data;
		virtual property ICeresParameterblock^ ManagedObject {
			ICeresParameterblock^ get() abstract;
		}
		virtual property int Length { virtual int get()  abstract; }
		virtual CeresParameterBlock^ Clone() override abstract;

		virtual property Enum^ BundleFlagsEnum {
			virtual Enum^ get() abstract;
			virtual void set(Enum^ value) abstract;
		};
	
	internal:

		virtual bool getBlockFullyVariable() abstract;
		virtual bool getBlockFullyConstant() abstract;
		virtual ceres::SubsetParameterization* GetPrametrization() abstract;

		virtual void UpdateManagedData() abstract;
		virtual void UpdateBundleData() abstract;

		!CeresParameterBlock() {
			if (_data != nullptr) {
				delete[] _data;
				_data = nullptr;
			}
		}
		CeresParameterBlock(int datalength) {
			_data = new double[datalength];
			for (size_t i = 0; i < datalength; i++)
			{
				_data[i] = 0;
			}
		}
		CeresParameterBlock() {
			_data = new double[Length];
			for (size_t i = 0; i < Length; i++)
			{
				_data[i] = 0;
			}
		}
		CeresParameterBlock(double* data) {
			_data = data;
		}
		~CeresParameterBlock() {
			this->!CeresParameterBlock();
		}

		property size_t Sz {size_t get() { return sizeof(double)*Length; }}

		void AddToProblem(ceres::Problem* problem) {
			problem->AddParameterBlock(_data, Length);
			setParametrization(problem);
		}

		void setParametrization(ceres::Problem* problem) {
			if (getBlockFullyConstant()) {
				problem->SetParameterBlockConstant(_data);
			} else if (getBlockFullyVariable()) {
				problem->SetParameterBlockVariable(_data);
			} else if (problem->GetParameterization(_data) == NULL) {
				// error bij 2+x gebruik SetParametrization.
				// kan block met parametrization door deze fix niet aan 2 problems toevoegen
				problem->SetParameterization(_data, GetPrametrization());
			}
		}
		
		CeresParameterBlock^ CreateCopy() {
			CeresParameterBlock^ r = (CeresParameterBlock^)this->MemberwiseClone();
			r->_data = new double[Length];
			for (size_t i = 0; i < Length; i++)
			{
				r->_data[i] = 0;
			}
			return r;
		}
	};
	
	public interface class ICeresPoint : ICeresParameterblock {
		property BundleWorldCoordinatesFlags BundleFlags {
			BundleWorldCoordinatesFlags get();
		};

		property double X {
			double get();
			void set(double d);
		}
		property double Y {
			double get();
			void set(double d);
		}
		property double Z {
			double get();
			void set(double d);
		}

	};
	public ref class CeresPoint : CeresParameterBlock {
	public:

		property int Length { int get() override { return 3; }}



	private:
		ICeresPoint^ _managed;
	internal:
		BundleWorldCoordinatesFlags _bundleFlags = BundleWorldCoordinatesFlags::ALL;


		bool getBlockFullyVariable() override { return _bundleFlags.HasFlag(BundleWorldCoordinatesFlags::ALL); }
		bool getBlockFullyConstant() override { return _bundleFlags == BundleWorldCoordinatesFlags::None; }
		ceres::SubsetParameterization* GetPrametrization() override {
			return NULL;
		}


	public:

		property Enum^ BundleFlagsEnum {
			Enum^ get() override { return BundleFlags; }
			void set(Enum^ value) override { _bundleFlags = (BundleWorldCoordinatesFlags)value; };
		};

		CeresPoint() {};
		CeresPoint(ICeresPoint^ managedObj) {
			_managed = managedObj;
		}
		virtual CeresParameterBlock^ Clone() override
		{
			auto r = gcnew CeresPoint(_managed);
			memcpy(r->_data, _data, sizeof(double)*Length);
			return r;
		}
		/*
		virtual CeresParameterBlock^ Clone() override = CeresParameterBlock::Clone
		{
			return (CeresParameterBlock^)(gcnew CeresPoint(_managed));
		}*/


		property ICeresParameterblock^ ManagedObject {
			ICeresParameterblock^ get() override { return _managed; }
		}


		property BundleWorldCoordinatesFlags BundleFlags {
			virtual BundleWorldCoordinatesFlags get() { return _bundleFlags; }
			void set(BundleWorldCoordinatesFlags flags) { _bundleFlags = flags; }
		};

		property double default[int]{
			double get(int i) {
			return _data[i];
		}
		void set(int i, double d) {
			_data[i] = d;
		}
		}

		property double X {
		virtual double get() {
				return _data[0];
			}
		virtual void set(double d) {
				_data[0] = d;
			}
		}
		property double Y {
			virtual double get() {
				return _data[1];
			}
			virtual void set(double d) {
				_data[1] = d;
			}
		}
		property double Z {
			virtual double get() {
				return _data[2];
			}
			virtual void set(double d) {
				_data[2] = d;
			}
		}

		property array<double>^ Coordinates_arr { array<double>^ get() {
			return gcnew array < double > {_data[0], _data[1], _data[2]};
		}
		void set(array<double>^ value) {
			_data[0] = value[0];
			_data[1] = value[1];
			_data[2] = value[2];
		}
		}



		virtual void UpdateManagedData() override {
			_managed->X = _data[0];
			_managed->Y = _data[1];
			_managed->Z = _data[2];
		};
		virtual void UpdateBundleData() override {
			_data[0] = _managed->X;
			_data[1] = _managed->Y;
			_data[2] = _managed->Z;
		};


	};

	public interface class ICeresPointOrient : ICeresParameterblock {
		property BundleWorldCoordinatesFlags BundleFlags {
			BundleWorldCoordinatesFlags get();
		};
		property double X {
			double get();
			void set(double d);
		}
		property double Y {
			double get();
			void set(double d);
		}
		property double Z {
			double get();
			void set(double d);
		}
		/*
		property double Rodr1 {
			double get();
			void set(double d);
		}
		property double Rodr2 {
			double get();
			void set(double d);
		}
		property double Rodr3 {
			double get();
			void set(double d);
		}*/
		property array<double>^ Pos_paramblock {
			array<double>^ get();
			void set(array<double>^ value);
		}
		property array<double>^ Rodr {
			array<double>^ get();
			void set(array<double>^ value);
		}
	};
	public ref class CeresPointOrient : CeresParameterBlock {
	private:
		ICeresPointOrient^ _managed;
	internal:
		BundlePointOrientFlags _bundleFlags = BundlePointOrientFlags::ALL;

		bool getBlockFullyVariable() override { return _bundleFlags.HasFlag(BundlePointOrientFlags::ALL); }
		bool getBlockFullyConstant() override { return _bundleFlags == BundlePointOrientFlags::None; }

		ceres::SubsetParameterization* GetPrametrization() override {
			auto bundle_data = _bundleFlags;

			std::vector<int> constant_data;

			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::Rodr1))
				constant_data.push_back(0);
			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::Rodr2))
				constant_data.push_back(1);
			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::Rodr3))
				constant_data.push_back(2);
			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::X))
				constant_data.push_back(3);
			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::Y))
				constant_data.push_back(4);
			if (!_bundleFlags.HasFlag(BundlePointOrientFlags::Z))
				constant_data.push_back(5);

			ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(Length, constant_data);

			return subset_parameterization;
		}




	public:
		property Enum^ BundleFlagsEnum {
			Enum^ get() override { return _bundleFlags; };
			void set(Enum^ value) override { _bundleFlags = (BundlePointOrientFlags)value; };
		};

		property int Length { int get() override { return 6; }}

		CeresPointOrient() {};

		CeresPointOrient(ICeresPointOrient^ managed) {
			_managed = managed;
		};

		virtual CeresParameterBlock^ Clone() override
		{
			auto r = gcnew CeresPointOrient(_managed);
			memcpy(r->_data, _data, sizeof(double)*Length);
			return r;
		}

		property ICeresParameterblock^ ManagedObject {
			ICeresParameterblock^ get() override { return _managed; }
		};

		property BundlePointOrientFlags BundleFlags {
			BundlePointOrientFlags get() { return _bundleFlags; };
			void set(BundlePointOrientFlags value) { _bundleFlags = value; }
		};


		property array<double>^ RT {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(6);
				pin_ptr<double> p = &r[0];
				memcpy(p, _data, 6 * 8);
				return r;
			}
			void set(array<double>^ v) {
				pin_ptr<double> p = &v[0];
				memcpy(_data, p, 6 * 8);
			}
		}

		property array<double>^ R_rod {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(3);
				pin_ptr<double> p = &r[0];
				memcpy(p, _data, 3 * 8);
				return r;
			}
			void set(array<double>^ v) {
				pin_ptr<double> p = &v[0];
				memcpy(_data, p, 3 * 8);
			}
		}

		property array<double>^ t {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(3);
				pin_ptr<double> p = &r[0];
				memcpy(p, &_data[3], 3 * 8);
				return r;
			}
			void set(array<double>^ v) {
				pin_ptr<double> p = &v[0];
				memcpy(&_data[3], p, 3 * 8);
			}
		}

		CeresPointOrient^ CreateCopy() {
			CeresPointOrient^ r = gcnew CeresPointOrient();
			r->BundleFlags = _bundleFlags;
			memcpy(r->_data, _data, sizeof(double) * 6);
			return r;
		}


		virtual void UpdateManagedData() override {
			_managed->Pos_paramblock = gcnew array<double>{_data[3], _data[4], _data[5]};
			_managed->Rodr = gcnew array<double>{_data[0], _data[1], _data[2]};
			/*_managed->X = _data[3];
			_managed->Y = _data[4];
			_managed->Z = _data[5];*/
			/*
			_managed->Rodr1 = _data[3];
			_managed->Rodr2 = _data[4];
			_managed->Rodr3 = _data[5];*/

		};
		virtual void UpdateBundleData() override {
			pin_ptr<double> r = &_managed->Rodr[0];
			memcpy(_data, r, 3 * 8);

			pin_ptr<double> p = &_managed->Pos_paramblock[0];
			memcpy(&_data[3], p, 3 * 8);


			/*_data[3] = _managed->X;
			_data[4] = _managed->Y;
			_data[5] = _managed->Z;*/

			/*
			_data[3] = _managed->Rodr1;
			_data[4] = _managed->Rodr2;
			_data[5] = _managed->Rodr3;*/

			
		};


		virtual CeresPointOrient^ toCeresParameter() {
			return this;
		}
		virtual void updateFromCeres(CeresPointOrient^ paramblock) {
			return;
		}
		virtual CeresPointOrient^ toCeresParameter(Enum^ BundleSettings) {
			_bundleFlags = (BundlePointOrientFlags)BundleSettings;
			return this;
		};

	};

	public interface class ICeresIntrinsics : ICeresParameterblock {
		property DistortionModel Distortionmodel {DistortionModel get(); }
		
		property int ImageWidth {int get(); }
		property int ImageHeight {int get(); }

		property double fx {
			double get(); void set(double value); }
		property double fy {
			double get(); void set(double value); }
		property double ppx {
			double get(); void set(double value); }
		property double ppy {
			double get(); void set(double value); }
		property double k1 {
			double get(); void set(double value); }
		property double k2 {
			double get(); void set(double value); }
		property double k3 {
			double get(); void set(double value); }
		property double p1 {
			double get(); void set(double value); }
		property double p2 {
			double get(); void set(double value); }
		property double p3 {
			double get(); void set(double value); }
		property double p4 {
			double get(); void set(double value); }
		property double k4 {
			double get(); void set(double value); }
		property double skew {
			double get(); void set(double value); }
		property double k5 {
			double get(); void set(double value); }
		property double k6 {
			double get(); void set(double value); }
		property double s1 {
			double get(); void set(double value); }
		property double s2 {
			double get(); void set(double value); }
	};
	public ref class CeresIntrinsics : CeresParameterBlock {
	private:
		ICeresIntrinsics^ _managed;
	public:
		int _imageWidth;
		int _imageHeight;

		DistortionModel Distortionmodel;

		PointF ReprojectPoint(CeresPointOrient^ extr, double x, double y, CeresPoint^ point3d) {
			double res[2];

			switch (Distortionmodel) {
			case DistortionModel::Standard:
				ReprojectionErrorSingleCamera::Reproject(x, y, _data, extr->_data, point3d->_data, res);
				break;
			case DistortionModel::AgisoftPhotoscan:
				ReprojectionErrorSingleCameraHighDist::Reproject(x, y, _data, extr->_data, point3d->_data, res);
				break;
			case DistortionModel::OpenCVAdvanced:
				ReprojectionErrorSingleCameraOpenCVAdvancedDist::Reproject(x, y, _data, extr->_data, point3d->_data, res);
				break;
			default:
				return PointF::Empty;
				break;
			}

			return PointF((float)res[0], (float)res[1]);
		}

		PointF ReprojectPointSystemCamera(CeresPointOrient^ system, CeresPointOrient^ extr, double x, double y, CeresPoint^ point3d) {

			double res[2];

			switch (Distortionmodel) {
			case DistortionModel::Standard:
				ReprojectionErrorSingleCamera::Reproject(x, y, _data, extr->_data, point3d->_data, res);
				return PointF((float)res[0], (float)res[1]);
				break;
			case DistortionModel::AgisoftPhotoscan:
			case DistortionModel::OpenCVAdvanced:
			default:
				return PointF::Empty;
				break;
			}
		}
	internal:

		ceres::CostFunction* CreateCostFunction(double obsx, double obsy) {
			ceres::CostFunction* r;
			switch (Distortionmodel) {
			case DistortionModel::Standard:
				r = ReprojectionErrorSingleCamera::Create(obsx, obsy);
				break;
			case DistortionModel::AgisoftPhotoscan:
				r = ReprojectionErrorSingleCameraHighDist::Create(obsx, obsy);
				break;
			case DistortionModel::OpenCVAdvanced:
				r = ReprojectionErrorSingleCameraOpenCVAdvancedDist::Create(obsx, obsy);
				break;
			default:
				r = NULL;
				break;
			}
			return r;
		}
		ceres::CostFunction* CreateCostFunctionSystemCamera(double obsx, double obsy) {
			ceres::CostFunction* r;
			switch (Distortionmodel) {
			case DistortionModel::Standard:
				r = ReprojectionErrorSysteemCamera::Create(obsx, obsx);
				break;
			case DistortionModel::AgisoftPhotoscan:
				r = ReprojectionErrorSysteemCameraHighDist::Create(obsx, obsy);
				break;
			case DistortionModel::OpenCVAdvanced:
				r = ReprojectionErrorSysteemCameraOpenCVAdvancedDist::Create(obsx, obsy);
				break;
			default:
				return NULL;
				break;
			}
			return r;
		}

		BundleIntrinsicsFlags _bundleFlags = BundleIntrinsicsFlags::ALL_STANDARD;

		bool getBlockFullyVariable() override {
			switch (Distortionmodel) {
			case DistortionModel::Standard:
				return _bundleFlags.HasFlag(BundleIntrinsicsFlags::ALL_STANDARD);
				break;
			case DistortionModel::AgisoftPhotoscan:
				return _bundleFlags.HasFlag(BundleIntrinsicsFlags::ALL_PHOTOSCAN);
				break;
			case DistortionModel::OpenCVAdvanced:
				return _bundleFlags.HasFlag(BundleIntrinsicsFlags::ALL_OPENCVADVANCED);
				break;
			default:
				return _bundleFlags.HasFlag(BundleIntrinsicsFlags::ALL_STANDARD);
				break;
			}
		}

		bool getBlockFullyConstant() override { return _bundleFlags == BundleIntrinsicsFlags::None; }

		ceres::SubsetParameterization* GetPrametrization() override {
			auto bundle_data = BundleFlags;


			std::vector<int> constant_data;

#define CHECKFLAG(managedflag,offset){\
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::managedflag)) \
			constant_data.push_back(offset); \
		} \

			CHECKFLAG(FocalLength, OFFSET_FOCAL_LENGTH_X);
			CHECKFLAG(FocalLength, OFFSET_FOCAL_LENGTH_Y);
			CHECKFLAG(PrincipalP, OFFSET_PRINCIPAL_POINT_X);
			CHECKFLAG(PrincipalP, OFFSET_PRINCIPAL_POINT_Y);
			CHECKFLAG(R1, OFFSET_K1);
			CHECKFLAG(R2, OFFSET_K2);
			CHECKFLAG(R3, OFFSET_K3);
			CHECKFLAG(P1, OFFSET_P1);
			CHECKFLAG(P2, OFFSET_P2);
			if (Distortionmodel == DistortionModel::AgisoftPhotoscan) {
				CHECKFLAG(SKEW, OFFSET_SKEW);
				CHECKFLAG(R4, OFFSET_K4);
				CHECKFLAG(P3, OFFSET_P3);
				CHECKFLAG(P4, OFFSET_P4);
			} else if (Distortionmodel == DistortionModel::OpenCVAdvanced) {
				CHECKFLAG(SKEW, OFFSET_SKEW);
				CHECKFLAG(R4, OFFSET_K4);
				CHECKFLAG(R5, OFFSET_K5);
				CHECKFLAG(R6, OFFSET_K6);
				CHECKFLAG(S1, OFFSET_P3);
				CHECKFLAG(S2, OFFSET_P4);
			}

			ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(Length, constant_data);

			return subset_parameterization;
		}

		


	public:
		property Enum^ BundleFlagsEnum {
			Enum^ get() override { return BundleFlags; }
			void set(Enum^ value) override { _bundleFlags = (BundleIntrinsicsFlags)value; };
		};


		property int Length { int get() override {
			switch (Distortionmodel) {
			case DistortionModel::Standard:
				return 9;
				break;
			case DistortionModel::AgisoftPhotoscan:
				return 13;
				break;
			case DistortionModel::OpenCVAdvanced:
				return 15;
				break;
			default:
				return 9;
				break;
			}
		}}

		CeresIntrinsics(DistortionModel model) : CeresParameterBlock(15) {
			this->Distortionmodel = model;
		};
		CeresIntrinsics() : CeresIntrinsics(DistortionModel::Standard) {};
		CeresIntrinsics(ICeresIntrinsics^ managedObj) : CeresIntrinsics(managedObj->Distortionmodel) {
			_managed = managedObj;
		};


		virtual CeresParameterBlock^ Clone() override
		{
			auto r = gcnew CeresIntrinsics(_managed);
			memcpy(r->_data, _data, sizeof(double)*Length);
			return r;
		}

		property ICeresParameterblock^ ManagedObject {ICeresParameterblock^ get() override { return _managed; }}



		CeresIntrinsics(array<double>^ intr) : CeresIntrinsics() {
			Intrinsics = intr;
		}
		CeresIntrinsics(Emgu::CV::Matrix<double>^ cameraMat, array<double>^ distCoeffs) : CeresIntrinsics() {
			set(cameraMat, distCoeffs);
		}

		

		void set(Emgu::CV::Matrix<double>^ cameraMat, array<double>^ distCoeffs) {
			fx = cameraMat->default[0, 0];
			fy = cameraMat->default[1, 1];
			ppx = cameraMat->default[0, 2];
			ppy = cameraMat->default[1, 2];

			k1 = distCoeffs[0];
			k2 = distCoeffs[1];
			if (distCoeffs->Length == 4) {
				p1 = distCoeffs[2];
				p2 = distCoeffs[3];
			} else {
				p1 = distCoeffs[2];
				p2 = distCoeffs[3];
				k3 = distCoeffs[4];
			}

		};

		String^ ToString() override {
			return String::Format("fx:{0} fy:{3} cx:{1} cy:{2}", fx.ToString("0:0,00"), ppx.ToString("1:0,00"), ppy.ToString("2:0,00"), fy.ToString("3:0,00"));
		}

		property BundleIntrinsicsFlags BundleFlags {
			BundleIntrinsicsFlags get() { return _bundleFlags; }
			void set(BundleIntrinsicsFlags flags) { _bundleFlags = flags; }
		};

		void SetBundleFlags(BundleIntrinsicsFlags flags) {
			_bundleFlags = flags;
		}

		property array<double, 2>^ CameraMatrix {array<double, 2>^ get() {
			return gcnew array < double, 2 > {
				{fx, 0, ppx},
				{ 0, fy, ppy },
				{ 0, 0, 1 }
			};
		};
		}
		property array<double>^ Dist5 {array<double>^ get() {
			return gcnew array < double > {
				k1, k2, p1, p2, k3
			};
		};
		}
		property array<double>^ Dist2 {array<double>^ get() {
			return gcnew array < double > {
				k1, k2
			};
		};
		}

		property Emgu::CV::Matrix<double>^ CameraMatrixCV {
			Emgu::CV::Matrix<double>^ get() { return gcnew Emgu::CV::Matrix<double>(CameraMatrix); }; 
		};

		property array<double>^ Intrinsics {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(9);
				pin_ptr<double> p = &r[0];
				memcpy(p, _data, 9 * 8);
				return r;
			}
			void set(array<double>^ v) {
				pin_ptr<double> p = &v[0];
				memcpy(_data, p, 9 * 8);
			}
		}

		void ZeroDistortions() {
			memset(&_data[OFFSET_K1], 0, Length - OFFSET_K1);
		}

#pragma region props
		property double fx {
			double get() {
				return _data[OFFSET_FOCAL_LENGTH_X];
			}void set(double value) {
				_data[OFFSET_FOCAL_LENGTH_X] = value;
			}}
		property double fy {
			double get() {
				return _data[OFFSET_FOCAL_LENGTH_Y];
			}void set(double value) {
				_data[OFFSET_FOCAL_LENGTH_Y] = value;
			}}
		property double ppx {
			double get() {
				return _data[OFFSET_PRINCIPAL_POINT_X];
			}void set(double value) {
				_data[OFFSET_PRINCIPAL_POINT_X] = value;
			}}
		property double ppy {
			double get() {
				return _data[OFFSET_PRINCIPAL_POINT_Y];
			}void set(double value) {
				_data[OFFSET_PRINCIPAL_POINT_Y] = value;
			}}
		property double k1 {
			double get() {
				return _data[OFFSET_K1];
			}void set(double value) {
				_data[OFFSET_K1] = value;
			}}
		property double k2 {
			double get() {
				return _data[OFFSET_K2];
			}void set(double value) {
				_data[OFFSET_K2] = value;
			}}
		property double k3 {
			double get() {
				return _data[OFFSET_K3];
			}void set(double value) {
				_data[OFFSET_K3] = value;
			}}
		property double p1 {
			double get() {
				return _data[OFFSET_P1];
			}void set(double value) {
				_data[OFFSET_P1] = value;
			}}
		property double p2 {
			double get() {
				return _data[OFFSET_P2];
			}void set(double value) {
				_data[OFFSET_P2] = value;
			}}
		property double p3 {
			double get() {
				return Distortionmodel == DistortionModel::AgisoftPhotoscan ? _data[OFFSET_P3] : 0;
			}void set(double value) {
				_data[OFFSET_P3] = value;
			}}
		property double p4 {
			double get() {
				return Distortionmodel == DistortionModel::AgisoftPhotoscan ? _data[OFFSET_P4] : 0;
			}void set(double value) {
				_data[OFFSET_P4] = value;
			}}
		property double k4 {
			double get() {
				return (Distortionmodel == DistortionModel::AgisoftPhotoscan || Distortionmodel == DistortionModel::OpenCVAdvanced) ? _data[OFFSET_K4] : 0;
			}void set(double value) {
				_data[OFFSET_K4] = value;
			}}
		property double skew {
			double get() {
				return (Distortionmodel == DistortionModel::AgisoftPhotoscan || Distortionmodel == DistortionModel::OpenCVAdvanced) ? _data[OFFSET_SKEW] : 0;
			}void set(double value) {
				_data[OFFSET_SKEW] = value;
			}}
		property double k5 {
			double get() {
				return (Distortionmodel == DistortionModel::AgisoftPhotoscan || Distortionmodel == DistortionModel::OpenCVAdvanced) ? _data[OFFSET_K5] : 0;
			}void set(double value) {
				_data[OFFSET_K5] = value;
			}}
		property double k6 {
			double get() {
				return Distortionmodel == DistortionModel::OpenCVAdvanced ? _data[OFFSET_K6] : 0;
			}void set(double value) {
				_data[OFFSET_K6] = value;
			}}
		property double s1 {
			double get() {
				return Distortionmodel == DistortionModel::OpenCVAdvanced ? _data[OFFSET_P3] : 0;
			}void set(double value) {
				_data[OFFSET_P3] = value;
			}}
		property double s2 {
			double get() {
				return Distortionmodel == DistortionModel::OpenCVAdvanced ? _data[OFFSET_P4] : 0;
			}void set(double value) {
				_data[OFFSET_P4] = value;
			}}
#pragma endregion


		virtual void UpdateManagedData() override {
			_managed->fx = fx;
			_managed->fy = fy;

			_managed->ppx = ppx;
			_managed->ppy = ppy;

			_managed->skew = skew;

			_managed->k1 = k1;
			_managed->k2 = k2;
			_managed->k3 = k3;

			_managed->p1 = p1;
			_managed->p2 = p2;

			if (Distortionmodel == DistortionModel::AgisoftPhotoscan) {
				_managed->k4 = k4;

				_managed->p3 = p3;
				_managed->p4 = p4;
			}
			if (Distortionmodel == DistortionModel::OpenCVAdvanced) {
				_managed->k4 = k4;
				_managed->k5 = k5;
				_managed->k6 = k6;

				_managed->s1 = s1;
				_managed->s2 = s2;
			}
		};
		virtual void UpdateBundleData() override {
			fx = _managed->fx;
			fy= _managed->fy;

			ppx = _managed->ppx;
			ppy = _managed->ppy;


			k1 = _managed->k1;
			k2 = _managed->k2;
			k3 = _managed->k3;

			p1 = _managed->p1;
			p2 = _managed->p2;

			if (Distortionmodel == DistortionModel::AgisoftPhotoscan) {

				skew = _managed->skew;

				k4 = _managed->k4;

				p3 = _managed->p3;
				p4 = _managed->p4;
			}
			if (Distortionmodel == DistortionModel::OpenCVAdvanced) {

				skew = _managed->skew;

				k4 = _managed->k4;
				k5 = _managed->k5;
				k6 = _managed->k6;

				s1 = _managed->s1;
				s2 = _managed->s2;
			}
		};


	};

	public interface class ICeresScaleTransform : ICeresParameterblock{
		property double XOffset {
			double get();
			void set(double v);
		}
		property double YOffset {
			double get();
			void set(double v);
		}
		property double ZOffset {
			double get();
			void set(double v);
		}


		property array<double>^ Rot_rodr {
			array<double>^ get();
			void set(array<double>^ value);
		}

		property double scale {
			double get();
			void set(double v);
		}
	};
	public ref class CeresScaledTransformation : CeresPointOrient {
	public:
		ICeresScaleTransform^ _managed;

		

		virtual void UpdateManagedData() override {
			_managed->Rot_rodr = gcnew array<double> { _data[OFFSET_RODR1],_data[OFFSET_RODR2],_data[OFFSET_RODR3] };

			_managed->XOffset = _data[3];
			_managed->YOffset = _data[4];
			_managed->ZOffset = _data[5];

			_managed->scale = _data[OFFSET_SCALE];
		};
		virtual void UpdateBundleData() override {
			pin_ptr<double> p = &_managed->Rot_rodr[0];
			memcpy(_data, p, 3 * 8);


			_data[3] = _managed->XOffset;
			_data[4] = _managed->YOffset;
			_data[5] = _managed->ZOffset;

			_data[6] = _managed->scale;
		};
		CeresScaledTransformation(ICeresScaleTransform^ managedObj) {
			_managed = managedObj;
			UpdateBundleData();
		};

		virtual CeresParameterBlock^ Clone() override
		{
			auto r = gcnew CeresScaledTransformation(_managed);
			memcpy(r->_data, _data, sizeof(double)*Length);
			return r;
		}


		property int Length { int get() override { return 7; }}
		//bool getBlockFullyVariable() override { return true; }
		//bool getBlockFullyConstant() override { return false; }
		//ceres::SubsetParameterization* GetPrametrization() override {return NULL;}



		property double scale {
			double get() {
				return _data[OFFSET_SCALE];
			}
			void set(double v) {
				_data[OFFSET_SCALE] = v;
			}
		}
	};
}