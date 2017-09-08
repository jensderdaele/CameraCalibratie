#pragma once
#include "Stdafx.h"
#include <vcclr.h>


#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
#pragma managed(pop)

#include "Offsets.h"

using namespace System::Collections::Generic;
using namespace Emgu::CV;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
using namespace OpenTK;
using namespace System;
using namespace System::Runtime::CompilerServices;


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
	public ref class CeresParameterBlock abstract {
	private:
		bool _paramterizationset = false;
	public:
		double* _data;
	internal:

		virtual property Enum^ BundleFlagsEnum { virtual Enum^ get() abstract; };
		virtual property int Length { virtual int get()  abstract; }
		virtual bool getBlockFullyVariable() abstract;
		virtual bool getBlockFullyConstant() abstract;
		virtual ceres::SubsetParameterization* GetPrametrization() abstract;


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

			if (getBlockFullyConstant()) {
				problem->SetParameterBlockConstant(_data);
			} else if (getBlockFullyVariable()) {
				problem->SetParameterBlockVariable(_data);
			} else if (!_paramterizationset) {
				_paramterizationset = true;
				// error bij 2+x gebruik SetParametrization.
				// kan block met parametrization door deze fix niet aan 2 problems toevoegen
				problem->SetParameterization(_data, GetPrametrization());
			}
		}

		void setParametrization(ceres::Problem* problem) {
			if (getBlockFullyConstant()) {
				problem->SetParameterBlockConstant(_data);
			} else if (getBlockFullyVariable()) {
				problem->SetParameterBlockVariable(_data);
			} else if (!_paramterizationset) {
				_paramterizationset = true;
				//problem->SetParameterBlockVariable(_data);
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

	public:

		List<array<double>^>^ _iterationvalues;


	};


	generic <class T> where T : CeresParameterBlock
		public interface class ICeresParameterConvertable {
		T toCeresParameter();
		void updateFromCeres(T paramblock);
		T toCeresParameter(Enum^ BundleSettings);
	};



	public ref class CeresParameterBlockCapture {
	public:
		List<CeresParameterBlock^>^ capture;
		initonly CeresParameterBlock^ block;

		CeresCallbackReturnType captureValues(int nr) {
			if (block->BundleFlagsEnum->Equals(0)) {
				return CeresCallbackReturnType::SOLVER_CONTINUE;
			}
			if (capture == nullptr) {
				capture = gcnew List<CeresParameterBlock^>();
			}
			capture->Add(block->CreateCopy());
			return CeresCallbackReturnType::SOLVER_CONTINUE;
		}
	};


	public ref class CeresPoint : CeresParameterBlock {
	internal:
		BundleWorldCoordinatesFlags _bundleFlags;

		property int Length { int get() override { return 3; }}
		bool getBlockFullyVariable() override { return _bundleFlags.HasFlag(BundleWorldCoordinatesFlags::ALL); }
		bool getBlockFullyConstant() override { return _bundleFlags == BundleWorldCoordinatesFlags::None; }
		ceres::SubsetParameterization* GetPrametrization() override {
			return NULL;
		}
		property Enum^ BundleFlagsEnum {
			Enum^ get() override { return BundleFlags; }
		};

	public:
		property BundleWorldCoordinatesFlags BundleFlags {
			BundleWorldCoordinatesFlags get() { return _bundleFlags; }
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
			double get() {
				return _data[0];
			}
			void set(double d) {
				_data[0] = d;
			}
		}
		property double Y {
			double get() {
				return _data[1];
			}
			void set(double d) {
				_data[1] = d;
			}
		}
		property double Z {
			double get() {
				return _data[2];
			}
			void set(double d) {
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
	};


	public ref class CeresPointOrient : CeresParameterBlock, public ICeresParameterConvertable<CeresPointOrient^> {
	internal:
		BundlePointOrientFlags _bundleFlags;

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
		property int Length { int get() override { return 6; }}
		property Enum^ BundleFlagsEnum {	Enum^ get() override { return _bundleFlags; }; };

	public:

		CeresPointOrient() {};

		property BundlePointOrientFlags BundleFlags {
			BundlePointOrientFlags get() { return _bundleFlags; };
			void set(BundlePointOrientFlags flags) { _bundleFlags = flags; }
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


		virtual CeresPointOrient^ toCeresParameter() {
			return this;
		}
		virtual void updateFromCeres(CeresPointOrient^ paramblock) {
			return;
		}
		virtual CeresPointOrient^ toCeresParameter(Enum^ BundleSettings) {
			_bundleFlags = (BundlePointOrientFlags)BundleSettings;
			return this;
		}
	};

	public ref class CeresIntrinsics : CeresParameterBlock {
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

		BundleIntrinsicsFlags _bundleFlags = BundleIntrinsicsFlags::ALL;

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

			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::FocalLength))
				constant_data.push_back(OFFSET_FOCAL_LENGTH_X);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::FocalLength))
				constant_data.push_back(OFFSET_FOCAL_LENGTH_Y);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::PrincipalP))
				constant_data.push_back(OFFSET_PRINCIPAL_POINT_X);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::PrincipalP))
				constant_data.push_back(OFFSET_PRINCIPAL_POINT_Y);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R1))
				constant_data.push_back(OFFSET_K1);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R2))
				constant_data.push_back(OFFSET_K2);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R3))
				constant_data.push_back(OFFSET_K3);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::P1))
				constant_data.push_back(OFFSET_P1);
			if (!bundle_data.HasFlag(BundleIntrinsicsFlags::P2))
				constant_data.push_back(OFFSET_P2);

			if (Distortionmodel == DistortionModel::AgisoftPhotoscan) {
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::SKEW))
					constant_data.push_back(OFFSET_SKEW);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R4))
					constant_data.push_back(OFFSET_K4);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::P3))
					constant_data.push_back(OFFSET_P3);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::P4))
					constant_data.push_back(OFFSET_P4);

			} else if (Distortionmodel == DistortionModel::OpenCVAdvanced) {
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::SKEW))
					constant_data.push_back(OFFSET_SKEW);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R4))
					constant_data.push_back(OFFSET_K4);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R5))
					constant_data.push_back(OFFSET_K5);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::R6))
					constant_data.push_back(OFFSET_K6);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::S1))
					constant_data.push_back(OFFSET_P3);
				if (!bundle_data.HasFlag(BundleIntrinsicsFlags::S2))
					constant_data.push_back(OFFSET_P4);
			}

			ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(Length, constant_data);

			return subset_parameterization;
		}

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

		property Enum^ BundleFlagsEnum {	Enum^ get() override { return BundleFlags; } };

	public:
		CeresIntrinsics(DistortionModel model) : CeresParameterBlock(15) {
			this->Distortionmodel = model;
		}
		CeresIntrinsics() : CeresIntrinsics(DistortionModel::Standard) {}

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

		String^  ToString() override {
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
		property Matrix<double>^ CameraMatrixCV {Matrix<double>^ get() { return gcnew Matrix<double>(CameraMatrix); }; };
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

	};

	public ref class CeresScaledTransformation : CeresPointOrient {
	public:
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