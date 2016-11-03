#pragma once

#include "stdafx.h"
#include "ceresdotnet.h"
#include <vcclr.h>

#pragma managed

using namespace System::Collections::Generic;
namespace ceresdotnet {

#pragma region c++embed
	template<typename T, int size>
	[System::Runtime::CompilerServices::UnsafeValueType]
	[System::Runtime::InteropServices::StructLayout
		(
		System::Runtime::InteropServices::LayoutKind::Explicit,
		Size = (sizeof(T)*size)
		)
	]
	public value struct inline_array {
	private:
		[System::Runtime::InteropServices::FieldOffset(0)]
		T dummy_item;

	public:
		T% operator[](int index) {
			return *((&dummy_item) + index);
		}

		static operator interior_ptr<T>(inline_array<T, size>% ia) {
			return &ia.dummy_item;
		}
	};

	template<typename T>
	ref class Embedded {
		T* t;

		!Embedded() {
			if (t != nullptr) {
				delete t;
				t = nullptr;
			}
		}

		~Embedded() {
			this->!Embedded();
		}

	public:
		Embedded() : t(new T) {}

		static T* operator&(Embedded% e) { return e.t; }
		static T* operator->(Embedded% e) { return e.t; }
	};
#pragma endregion
	
#pragma region enums

	public enum class IntrinsicsOffsets: int{
		OFFSET_FOCAL_LENGTH_X,
		OFFSET_FOCAL_LENGTH_Y,
		OFFSET_PRINCIPAL_POINT_X,
		OFFSET_PRINCIPAL_POINT_Y,
		OFFSET_K1,
		OFFSET_K2,
		OFFSET_P1,
		OFFSET_P2,
		OFFSET_K3,
	};
	[FlagsAttribute]
	public enum class  BundleIntrinsicsFlags :int
	{
		FocalLength = 1,
		PrincipalP = 2,
		R1 = 4,
		R2 = 8,
		P1 = 16,
		P2 = 32,
		R3 = 64,
		ALL = FocalLength & PrincipalP & R1 &R2 &P1&P2&R3,
		GeoAutomation = FocalLength & PrincipalP & R1 & R2 & R3
	};
	[FlagsAttribute]
	public enum class  BundleWorldCoordinatesFlags :int
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		ALL = X & Z & Y
	};
#pragma endregion

#pragma region Slecht
	public ref class CeresCamera {
	private:
		Embedded<EuclideanCamera> camera;

		bool _intrinsic_linked = false;

	public:
		EuclideanCamera* n(){
			return &camera;
		};



		void LinkIntrinsicsToCamera(CeresCamera^ mainCamera){
			if (mainCamera->IntrinsicsLinked){
				throw gcnew System::ArgumentException("maincamera mag niet gelink zijn!");
			}
			EuclideanCamera* main = mainCamera->n();
			if (n()->intrinsics != nullptr){
				delete [] n()->intrinsics;
			}
			n()->intrinsics = main->intrinsics;
			_intrinsic_linked = true;

		}

		void UnlinkIntrinsics(){
			_intrinsic_linked = false;
			n()->intrinsics = new double[9];
		}


		
		
		CeresCamera(OpenTK::Matrix3d r, OpenTK::Vector3d rvec, int image){
			EuclideanCamera* c;
			c = &camera;
			Mat3* rmat = new Mat3();
			Mat3 test;
			(*rmat)(0) = r.M11;
			(*rmat)(1) = r.M12;
			(*rmat)(2) = r.M13;
			(*rmat)(3) = r.M21;
			(*rmat)(4) = r.M22;
			(*rmat)(5) = r.M23;
			(*rmat)(6) = r.M31;
			(*rmat)(7) = r.M32;
			(*rmat)(8) = r.M33;


			c->R = *rmat;

			Vec3* p = new Vec3();
			(*p)(0) = rvec.X;
			(*p)(1) = rvec.Y;
			(*p)(2) = rvec.Z;
			c->t = *p;

			c->image = image;
		}

		property array<double>^ Intrinsics {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(9);
				pin_ptr<double> p = &r[0];
				memcpy(p, n()->intrinsics, 9 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(n()->intrinsics, p, 9 * 8);
			}
		}

		

		property int bundle_intrinsics{
			int get() {
				return n()->bundle_intrinsics;
			}
			void set(int v){
				n()->bundle_intrinsics = v;
			}
		}

		property bool IntrinsicsLinked{
			bool get() {
				return _intrinsic_linked;
			}
		}
		property int id{
			int get(){
				return n()->image;
			}
			void set(int i)
			{
				n()->image = i;
			}
		}
	};

	public ref class CeresPoint{
		Embedded<EuclideanPoint> point;
	public:
		EuclideanPoint* getNativeClass(){
			return &point;
		};
		CeresPoint(Vector3d pos, int trackNr){
			double* p = &(&point)->X(0);
			p[0] = pos.X;
			p[1] = pos.Y;
			p[2] = pos.Z;

			(&point)->track = trackNr;
		}
		CeresPoint(int trackNr){
			double* p = &(&point)->X(0);
			p[0] = 0;
			p[1] = 0;
			p[2] = 0;

			(&point)->track = trackNr;
		}
		Vector3d getPos(){
			Vec3 p = (&point)->X;
			Vector3d^ r = gcnew Vector3d(p(0), p(1), p(2));
			return *r;
		}
		property int TrackNr{
			int get(){
				return (&point)->track;
			}
			void set(int value){
				(&point)->track = value;
			}
		}
	};

	public ref class CeresMarker{
	internal:
		Embedded<Marker> marker;

	public:
		CeresPoint^ Worldcoordinates;

		Marker* getNativeClass(){
			return &marker;
		};
		/*
		CeresMarker(int image, int track, double x, double y){
		Marker* m = &marker;
		m->image = image;
		m->track = track;
		m->x = x;
		m->y = y;
		}*/
		CeresMarker(double x, double y, CeresPoint^ worldpoint){
			Worldcoordinates = worldpoint;
		}
	};

#pragma endregion

#pragma region Interfaces

	public interface class ICeresMarker{
		property double x{double get(); };
		property double y{double get(); };
		property double* WorldCoordinates{double* get(); }
		property BundleWorldCoordinatesFlags BundleCoordinates{BundleWorldCoordinatesFlags get(); }
	};
	public interface class ICeresCamera{
		property String^ Name{String^ get(); }
		property double* Intrinsics{double* get(); }
		property BundleIntrinsicsFlags BundleIntrinsics{BundleIntrinsicsFlags get(); void set(BundleIntrinsicsFlags); }
		property double* Rt{double* get(); }
	};
	public interface class ICeresStereoCamera{
		property String^ Name{String^ get(); }
		property double* Intrinsics1{double* get(); }
		property double* Intrinsics2{double* get(); }
		property BundleIntrinsicsFlags BundleIntrinsics1{BundleIntrinsicsFlags get(); }
		property BundleIntrinsicsFlags BundleIntrinsics2{BundleIntrinsicsFlags get(); }
		property double* Rt1{double* get(); }
		property double* Rt2{double* get(); }
	};
	public interface class ICeresObservation{
		property ICeresCamera^ Camera{ICeresCamera^ get(); }
		property IEnumerable<ICeresMarker^>^ Markers{IEnumerable<ICeresMarker^>^ get(); }
	};
	public interface class ICeresStereoObservation{
		property ICeresStereoCamera^ StereoCamera{ICeresStereoCamera^ get(); }
		property IEnumerable<ICeresMarker^>^ MarkersFirst{IEnumerable<ICeresMarker^>^ get(); }
		property IEnumerable<ICeresMarker^>^ MarkersSecond{IEnumerable<ICeresMarker^>^ get(); }
	};

#pragma endregion

#pragma region classes goed

	public ref class CeresPointOrient{
	internal:
		double *_Rt;
	public:
		CeresPointOrient(){
			_Rt = new double[6];
		};
		!CeresPointOrient() {
			if (_Rt != nullptr) {
				delete[] _Rt;
				_Rt = nullptr;
			}
		}
		~CeresPointOrient(){
			this->!CeresPointOrient();
		};

		property array<double>^ RT {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(6);
				pin_ptr<double> p = &r[0];
				memcpy(p, _Rt, 6 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(_Rt, p, 6 * 8);
			}
		}

		property array<double>^ R_rod {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(3);
				pin_ptr<double> p = &r[0];
				memcpy(p, _Rt, 3 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(_Rt, p, 3 * 8);
			}
		}

		property array<double>^ t {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(3);
				pin_ptr<double> p = &r[0];
				memcpy(p, &_Rt[3], 3 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(&_Rt[3], p, 3 * 8);
			}
		}

		void set(Matrix4d^ worldMat){
			OpenTK::Matrix4d m = *worldMat;

			Mat3* rmat = new Mat3();
			(*rmat)(0) = m.M11;
			(*rmat)(1) = m.M12;
			(*rmat)(2) = m.M13;
			(*rmat)(3) = m.M21;
			(*rmat)(4) = m.M22;
			(*rmat)(5) = m.M23;
			(*rmat)(6) = m.M31;
			(*rmat)(7) = m.M32;
			(*rmat)(8) = m.M33;

			ceres::RotationMatrixToAngleAxis((double*)rmat, _Rt);

			_Rt[3] = m.M41;
			_Rt[4] = m.M42;
			_Rt[5] = m.M43;
		}

		static CeresPointOrient^ From(Matrix4d^ worldMat){
			CeresPointOrient^ r = gcnew CeresPointOrient();
			r->set(worldMat);
			return r;
		}
	};

	public ref class CeresIntrinsics{
	internal:
		double *_intrinsics;// = new double[9];
	public:
		CeresIntrinsics(){
			_intrinsics = new double[9];
		};
		!CeresIntrinsics() {
			if (_intrinsics != nullptr) {
				delete[] _intrinsics;
				_intrinsics = nullptr;
			}
		}
		~CeresIntrinsics(){
			this->!CeresIntrinsics();
		};


		property array<double>^ Intrinsics {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(9);
				pin_ptr<double> p = &r[0];
				memcpy(p, _intrinsics, 9 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(_intrinsics, p, 9 * 8);
			}
		}

		property double fx{
			double get(){
				return _intrinsics[OFFSET_FOCAL_LENGTH_X];
			}}
		property double fy{
			double get(){
				return _intrinsics[OFFSET_FOCAL_LENGTH_Y];
			}}
		property double ppx{
			double get(){
				return _intrinsics[OFFSET_PRINCIPAL_POINT_X];
			}}
		property double ppy{
			double get(){
				return _intrinsics[OFFSET_PRINCIPAL_POINT_Y];
			}}
		property double k1{
			double get(){
				return _intrinsics[OFFSET_K1];
			}}
		property double k2{
			double get(){
				return _intrinsics[OFFSET_K2];
			}}
		property double k3{
			double get(){
				return _intrinsics[OFFSET_K3];
			}}
		property double p1{
			double get(){
				return _intrinsics[OFFSET_P1];
			}}
		property double p2{
			double get(){
				return _intrinsics[OFFSET_P2];
			}}

	};

	public ref class CeresCamera2{
	public:
		initonly CeresPointOrient^ External = gcnew CeresPointOrient();
		CeresIntrinsics^ Internal = gcnew CeresIntrinsics();

		CeresCamera2(OpenTK::Matrix3d r, OpenTK::Vector3d rvec){
			Mat3* rmat = new Mat3();
			(*rmat)(0) = r.M11;
			(*rmat)(1) = r.M12;
			(*rmat)(2) = r.M13;
			(*rmat)(3) = r.M21;
			(*rmat)(4) = r.M22;
			(*rmat)(5) = r.M23;
			(*rmat)(6) = r.M31;
			(*rmat)(7) = r.M32;
			(*rmat)(8) = r.M33;

			ceres::RotationMatrixToAngleAxis((double*)rmat, External->_Rt);
			External->_Rt[3] = rvec.X;
			External->_Rt[4] = rvec.Y;
			External->_Rt[5] = rvec.Z;
		}

		CeresCamera2(OpenTK::Matrix4d cameraWorldMat){
			cameraWorldMat.Invert();
			OpenTK::Matrix4d m = cameraWorldMat;

			Mat3 rmat;
			(rmat)(0) = m.M11;
			(rmat)(1) = m.M12;
			(rmat)(2) = m.M13;
			(rmat)(3) = m.M21;
			(rmat)(4) = m.M22;
			(rmat)(5) = m.M23;
			(rmat)(6) = m.M31;
			(rmat)(7) = m.M32;
			(rmat)(8) = m.M33;

			ceres::RotationMatrixToAngleAxis((double*)&rmat(0), External->_Rt);


			External->_Rt[3] = m.M41;
			External->_Rt[4] = m.M42;
			External->_Rt[5] = m.M43;

		}
	};


	public ref class CeresPoint2{
	internal:
		Vec3* _vec;
	public:
		CeresPoint2(){
			_vec = new Vec3();
		}
		!CeresPoint2() {
			if (_vec != nullptr) {
				delete _vec;
				_vec = nullptr;
			}
		}
		~CeresPoint2(){
			this->!CeresPoint2();
		};

		BundleWorldCoordinatesFlags BundleCoordinates;
		property Vector3d^ Coordinates{ Vector3d^ get(){
			Vec3 c = *_vec;
			return gcnew Vector3d(c(0), c(1), c(2));
		}
		void set(Vector3d^ value){

			Vec3 c = *_vec;
			c(0) = value->X;
			c(1) = value->Y;
			c(2) = value->Z;

		}
		}
		property array<double>^ Coordinates_arr{ array<double>^ get(){
			Vec3 c = *_vec;
			return gcnew array < double > {c(0), c(1), c(2)};
		}
		void set(array<double>^ value){

			Vec3 c = *_vec;
			c(0) = value[0];
			c(1) = value[1];
			c(2) = value[2];

		}
		}
	};

	public ref class CeresMarker2{
	internal:

	public:
		CeresPoint2^ Location;
		double x, y;
		int id;
	};

	
	public ref class CeresStereoCamera{
	public:
		enum class BundleStereoCameraFlags{
			FIX_ROTATION_TRANSLATION,
			FIX_TRANSLATION
		};
		CeresIntrinsics^ Internal1, Internal2;
		CeresPointOrient^ External_camera1, External_camera2_relative;
		BundleStereoCameraFlags BundleMethod;

		void get_RTcamera2(){
		};
		


		

	};

#pragma endregion

#pragma region Ceres Solver imp

	public enum class CeresCallbackReturnType : int{
		SOLVER_ABORT = ceres::SOLVER_ABORT,
		SOLVER_CONTINUE = ceres::SOLVER_CONTINUE,
		SOLVER_TERMINATE_SUCCESSFULLY = ceres::SOLVER_TERMINATE_SUCCESSFULLY
	};

	public ref class BundleProblem
	{
	public:
		static void EuclideanBundleCommonIntrinsics(IEnumerable<CeresMarker^>^ all_markers
			, int bundle_intrinsics
			, int bundle_constraints
			, array<double>^ camera_intrinsics
			, IEnumerable<CeresCamera^>^ all_cameras
			, IEnumerable<CeresPoint^>^ all_points){
			IEnumerator<CeresMarker^>^ markerenum = all_markers->GetEnumerator();

			vector<Marker>* all_markers_native = new vector<Marker>();
			int c = 0;
			while (markerenum->MoveNext()){
				CeresMarker^ marker = markerenum->Current;

				Marker* m = marker->getNativeClass();
				all_markers_native->resize(c + 1);
				(*all_markers_native)[c] = *m;
				c++;
			}
			int all_markers_nativeSz = all_markers_native->size();

			vector<EuclideanCamera>* all_cameras_native = new vector<EuclideanCamera>();
			IEnumerator<CeresCamera^>^ cameraenum = all_cameras->GetEnumerator();
			c = 0;
			while (cameraenum->MoveNext()){
				CeresCamera^ camera = cameraenum->Current;
				EuclideanCamera* m = camera->n();
				all_cameras_native->resize(c + 1);
				(*all_cameras_native)[c] = *m;
				c++;
			}
			int all_cameras_nativesz = all_cameras_native->size();

			vector<EuclideanPoint>* all_points_native = new vector<EuclideanPoint>();
			IEnumerator<CeresPoint^>^ pointenum = all_points->GetEnumerator();
			c = 0;
			while (pointenum->MoveNext()){
				CeresPoint^ point = pointenum->Current;
				EuclideanPoint* m = point->getNativeClass();
				all_points_native->resize(c + 1);
				(*all_points_native)[c] = *m;
				c++;
			}
			int all_points_nativeSz = all_points_native->size();

			pin_ptr<double> camera_intrinsics_pin = &camera_intrinsics[0];
			double* camera_intrinsics_native = camera_intrinsics_pin;


			ceresdotnet::EuclideanBundleCommonIntrinsics(*all_markers_native,
				bundle_intrinsics,
				bundle_constraints,
				camera_intrinsics_native,
				all_cameras_native,
				all_points_native
				);

			delete all_markers_native;
			delete all_cameras_native;
			delete all_points_native;
		};
	};

	delegate void Iteration(int iterationNr, IEnumerable<ICeresObservation^>^ observations, IEnumerable<ICeresStereoObservation^>^ stereoObservations, CeresCallbackReturnType^% returnType);

	class NativeIterationCallback : public ceres::IterationCallback {

	public:
		virtual ceres::CallbackReturnType operator()(const
			ceres::IterationSummary& summary) {
			//int r = _problem->OnIterationStep(summary.iteration);
			return (ceres::CallbackReturnType)0;
		}
	};


	public ref class MultiCameraBundleProblem
	{
	private:
		static void ApplyRT(double* base, double* object){};

		static ceres::SubsetParameterization* GetIntrinsicsParametrization(BundleIntrinsicsFlags bundle_intrinsics){
			std::vector<int> constant_intrinsics;

#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
					if (!(bundle_enum.HasFlag(bundle_intrinsics))) { \
					constant_intrinsics.push_back(offset); \
					}
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::FocalLength, OFFSET_FOCAL_LENGTH_X);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::FocalLength, OFFSET_FOCAL_LENGTH_Y);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::PrincipalP, OFFSET_PRINCIPAL_POINT_X);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::PrincipalP, OFFSET_PRINCIPAL_POINT_Y);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R1, OFFSET_K1);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R2, OFFSET_K2);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::P1, OFFSET_P1);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::P2, OFFSET_P2);
			MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R3, OFFSET_K3);
#undef MAYBE_SET_CONSTANT

			ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
			return subset_parameterization;
		}

	internal:
		IEnumerable<ICeresObservation^>^ observations_currentSolve;
		IEnumerable<ICeresStereoObservation^>^ stereoObservations_currentSolve;
		int OnIterationStep(int iteration){
			CeresCallbackReturnType r;
			//IterationStep(iteration, observations_currentSolve, stereoObservations_currentSolve, r);
			return (int)r;
		}

	public:

		List<CeresCamera^>^ cameras = gcnew List<CeresCamera^>();
		List<CeresMarker^>^ markers = gcnew List<CeresMarker^>();
		List<CeresPoint^>^ all_points_managed = gcnew List<CeresPoint^>();


		event Iteration^ IterationStep;



		void SolveInterface(IEnumerable<ICeresObservation^>^ observations, IEnumerable<ICeresStereoObservation^>^ stereoObservations) {
			if (observations != nullptr){

			}
			ceres::Solver::Options solver_options;
			ceres::Problem::Options problem_options;
			ceres::Problem problem(problem_options);

			ceres::Solver::Options options;

			/*NativeIterationCallback callback(this);
			options.callbacks.push_back(&callback);
			options.update_state_every_iteration = true;*/


			auto obsenum = observations->GetEnumerator();
			
			vector<double*> intrinsicsList();
			vector<double*> worldPointList();



			while (obsenum->MoveNext()){
				auto obs = obsenum->Current;
				ICeresCamera^ camera = obs->Camera;
				IEnumerator<ICeresMarker^>^ markerenum = obs->Markers->GetEnumerator();

				double* intrinsics = camera->Intrinsics;

				while (markerenum->MoveNext()){
					ICeresMarker^ marker = markerenum->Current;

					problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
						ReprojectionError, 2, 9, 6, 3 > (new ReprojectionError(marker->x, marker->y)),
						NULL,
						camera->Intrinsics,
						camera->Rt,
						marker->WorldCoordinates);

					if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::None){
						problem.SetParameterBlockConstant(marker->WorldCoordinates);
					}
					else if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::ALL)
					{}
					else
					{
						std::vector<int> constant_coordinates;
						if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::X)) constant_coordinates.push_back(0);
						if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Y)) constant_coordinates.push_back(1);
						if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Z)) constant_coordinates.push_back(2);

						ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(3, constant_coordinates);
						problem.SetParameterization(marker->WorldCoordinates, subset_parameterization);
					}
				}
				ceres::SubsetParameterization* subset_parameterization = GetIntrinsicsParametrization(camera->BundleIntrinsics);
				problem.SetParameterization(intrinsics, subset_parameterization);

			}
		}
	


		static Vector2d testProjectPoint(Matrix3d mat, Vector3d trans, double obx, double oby, array<double>^ camera_intrinsics,array<double>^ Rt, Vector3d vec){

			double* rt = (double*)malloc((size_t)(sizeof(double) * 3 * 2));
			double* rotnative = (double*)malloc((size_t)(sizeof(double) * 9));
			rotnative[0] = mat.M11;
			rotnative[1] = mat.M12;
			rotnative[2] = mat.M13;
			rotnative[3] = mat.M21;
			rotnative[4] = mat.M22;
			rotnative[5] = mat.M23;
			rotnative[6] = mat.M31;
			rotnative[7] = mat.M32;
			rotnative[8] = mat.M33;
			ceres::RotationMatrixToAngleAxis<double>(rotnative, rt);
			rt[3] = trans.X;
			rt[4] = trans.Y;
			rt[5] = trans.Z;

			pin_ptr<double> camera_intrinsics_pin = &camera_intrinsics[0];
			double* intrinsics = camera_intrinsics_pin;

			pin_ptr<double> Rt_pin = &Rt[0];
			double* R_t = Rt_pin;

			
			double* X = (double*)malloc((size_t)(sizeof(double) * 3));
			X[0] = vec.X;
			X[1] = vec.Y;
			X[2] = vec.Z;


			const double focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
			const double focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
			const double principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
			const double principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
			const double k1 = intrinsics[OFFSET_K1];
			const double k2 = intrinsics[OFFSET_K2];
			const double k3 = intrinsics[OFFSET_K3];
			const double p1 = intrinsics[OFFSET_P1];
			const double p2 = intrinsics[OFFSET_P2];

			ceres::AngleAxisToRotationMatrix(rt, rotnative);


			double x[3];
			ceres::AngleAxisRotatePoint(R_t, X, x);

			double xx = X[0] * rotnative[0] + X[1] * rotnative[3] + X[2] * rotnative[6] + 1 * rt[3];
			double yy = X[0] * rotnative[1] + X[1] * rotnative[4] + X[2] * rotnative[7] + 1 * rt[4];
			double zz = X[0] * rotnative[2] + X[1] * rotnative[5] + X[2] * rotnative[8] + 1 * rt[5];

			double xxm = vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X + 1 * trans.X;
			double yym = vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y + 1 * trans.Y;
			double zzm = vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z + 1 * trans.Z;
			x[0] += rt[3];
			x[1] += rt[4];
			x[2] += rt[5];

			x[0] = xx;
			x[1] = yy;
			x[2] = zz;



			// Normaliseren
			double xn = x[0] / x[2];
			double yn = x[1] / x[2];
			double predicted_x, predicted_y;

			ApplyRadialDistortionCameraIntrinsics(focal_length_x,
				focal_length_y,
				principal_point_x,
				principal_point_y,
				k1, k2, k3,
				p1, p2,
				xn, yn,
				&predicted_x,
				&predicted_y);

			return *(gcnew Vector2d(predicted_x, predicted_y));

			//residuals[0] = predicted_x - T(observed_x);
			//residuals[1] = predicted_y - T(observed_y);
		}
		void Solve2(){

		}

		//MultiCameraBundleProblem.SolveProblem()
		//Data gekend:	CeresCameras (1 per foto) bevat interne parameters
		//								en initiele waarden voor de externe (via SolvePnP())
		//				CeresMarkers (1 per marker per foto) bevat x,y fotocoordinaten & bijhorende CeresCamera
		//				CeresPoints (1 per marker) bevat x,y,z data in wereldcoordinaten
		void SolveProblem(){
			vector<EuclideanPoint> all_points;
			IEnumerator<CeresPoint^>^ pointenum = all_points_managed->GetEnumerator();
			int c = 0;
			//omzetten van managed c# naar native c++
			while (pointenum->MoveNext()){
				CeresPoint^ p = pointenum->Current;
				EuclideanPoint* m = p->getNativeClass();
				m->track = p->TrackNr;
				all_points.push_back(*m);
				c++;
			}

			ceres::Problem::Options problem_options;
			ceres::Problem problem(problem_options);


			for each(CeresCamera^ cam in *cameras){
				const EuclideanCamera *camera = cam->n();
				if (!camera) {
					continue;
				}
				//omzetten van externe parameters (wereldcoord->cameracoord) naar double[6] vorm
				
				ceres::RotationMatrixToAngleAxis(&camera->R(0, 0), &(*camera->Rt)(0));
				camera->Rt->tail<3>() = camera->t;
			}

			ceres::SubsetParameterization *subset_parameterization;
			double* camera_intrinsics;
			vector<double*> intrinsicsList;
			for each(CeresMarker^ marker_managed in *markers)
			{
				Marker* marker = marker_managed->getNativeClass();
				CeresCamera^ camera_managed;
				EuclideanCamera* camera = camera_managed->n();

				camera_intrinsics = camera->intrinsics;

				//3D punt voor marker
				EuclideanPoint *point = PointForTrack(&all_points, marker->track);

				double *current_camera_R_t = (double*)camera->Rt;

				double x = marker->x;
				double y = marker->y;
				
				//1 RESIDUAL BLOCK / MARKER
				problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
					ReprojectionError, 2, 9, 6, 3>(new ReprojectionError(marker->x, marker->y)),
					NULL,
					camera_intrinsics,
					current_camera_R_t,
					&point->X(0));

				//De 3D coordinaten worden constant gehouden 
				problem.SetParameterBlockConstant(&point->X(0));

				//INTERNE PARAMETERS
				//zet interne parameters constant voor diegene die niet gebundled worden
				bool intr_used = std::find(intrinsicsList.begin(), intrinsicsList.end(), camera_intrinsics) != intrinsicsList.end();
				if (!camera_managed->IntrinsicsLinked && !intr_used){ //1x per interne parameters
					PrintCameraIntrinsics("Original intrinsics: ", camera->intrinsics);
					int bundle_intrinsics = camera->bundle_intrinsics;
					std::vector<int> constant_intrinsics;
			#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
					if (!(bundle_intrinsics & bundle_enum)) { \
					constant_intrinsics.push_back(offset); \
															}
					MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_X);
					MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_Y);
					MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_X);
					MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_Y);
					MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K1, OFFSET_K1);
					MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K2, OFFSET_K2);
					MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P1, OFFSET_P1);
					MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P2, OFFSET_P2);
			#undef MAYBE_SET_CONSTANT
					//subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
					//problem.SetParameterization(camera_intrinsics, subset_parameterization);
					intrinsicsList.push_back(camera_intrinsics);
				}
			}
			ceres::Solver::Options options;
			options.use_nonmonotonic_steps = true;
			options.preconditioner_type = ceres::SCHUR_JACOBI;
			options.linear_solver_type = ceres::ITERATIVE_SCHUR;
			options.use_inner_iterations = true;
			options.max_num_iterations = 100;
			options.minimizer_progress_to_stdout = true;

			ceres::Solver::Summary summary;
			ceres::Solve(options, &problem, &summary);
			std::cout << "Final report:\n" << summary.FullReport();

			for (int i = 0; i < intrinsicsList.size(); i++){
				PrintCameraIntrinsics2("Final intrinsics: ", intrinsicsList[i]);
			}
		}

		void Solve(){

			vector<EuclideanPoint> all_points;
			IEnumerator<CeresPoint^>^ pointenum = all_points_managed->GetEnumerator();
			int c = 0;
			while (pointenum->MoveNext()){
				CeresPoint^ p = pointenum->Current;
				EuclideanPoint* m = p->getNativeClass();
				all_points.resize(c + 1);
				all_points[c] = *m;
				c++;
			}

			ceres::Problem::Options problem_options;
			ceres::Problem problem(problem_options);

			
			for each(CeresCamera^ cam in *cameras){
				const EuclideanCamera *camera = cam->n();
				if (!camera) {
					continue;
				}
				
				
				PrintCameraIntrinsics("Original intrinsics: ", camera->intrinsics);
				ceres::RotationMatrixToAngleAxis(&camera->R(0, 0), &(*camera->Rt)(0));
				//TEMP FIX
				double* rt(&(*camera->Rt)(0));
				//rt[0] = -rt[0];
				//rt[1] = -rt[1];
				//rt[2] = -rt[2];
				camera->Rt->tail<3>() = camera->t;
			}

			ceres::SubsetParameterization *subset_parameterization;
			double* camera_intrinsics;
			for each(CeresMarker^ marker_managed in *markers)
			{
				
				Marker* marker = marker_managed->getNativeClass();
				CeresCamera^ camera_managed;// = marker_managed->parentCamera;
				EuclideanCamera* camera = camera_managed->n();
				
				camera_intrinsics = camera->intrinsics;

				//3D punt voor marker
				EuclideanPoint *point = PointForTrack(&all_points, marker->track);
				
				
				//EXTERNE PARAMETERS//2 mogelijkheden
				//1: camera R & t vrij - easy
				double *current_camera_R_t = (double*)camera->Rt;


				//2: camera R & t locked tov andere camera

				
				//INTERNE PARAMETERS
				//zet interne parameters constant voor diegene die niet gebundled worden
				if (!camera_managed->IntrinsicsLinked){ //1x per interne parameters

					int bundle_intrinsics = camera->bundle_intrinsics;

					std::vector<int> constant_intrinsics;
#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
					if (!(bundle_intrinsics & bundle_enum)) { \
					constant_intrinsics.push_back(offset); \
					}
					MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_X);
					MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_Y);
					MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_X);
					MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_Y);
					MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K1, OFFSET_K1);
					MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K2, OFFSET_K2);
					MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P1, OFFSET_P1);
					MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P2, OFFSET_P2);
#undef MAYBE_SET_CONSTANT
					constant_intrinsics.push_back(OFFSET_K3);
					subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
				}


				
				//1 RESIDUAL BLOCK / MARKER
				problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
					ReprojectionError, 2, 9, 6, 3 >(new ReprojectionError(marker->x,marker->y)),
					NULL,
					camera_intrinsics,
					current_camera_R_t,
					&point->X(0));

				problem.SetParameterBlockConstant(&point->X(0));

				
			}
			//problem.SetParameterization(camera_intrinsics, subset_parameterization); ERROR??
			problem.SetParameterBlockConstant((double*)cameras[0]->n()->Rt);
			

			ceres::Solver::Options options;
			options.use_nonmonotonic_steps = true;
			options.preconditioner_type = ceres::SCHUR_JACOBI;
			options.linear_solver_type = ceres::ITERATIVE_SCHUR;
			options.use_inner_iterations = true;
			options.max_num_iterations = 100;
			options.minimizer_progress_to_stdout = true;

			ceres::Solver::Summary summary;
			ceres::Solve(options, &problem, &summary);
			std::cout << "Final report:\n" << summary.FullReport();

			//UnpackCamerasRotationAndTranslation(all_markers,			all_cameras_R_t,			all_cameras);
			PrintCameraIntrinsics2("Final intrinsics: ", camera_intrinsics);
		}
	};

#pragma endregion
};