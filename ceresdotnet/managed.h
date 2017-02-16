#pragma once

#include "stdafx.h"
#include "ceresdotnet.h"
#include "ceresfunctions.h"
#include <vcclr.h>


#pragma managed

using namespace System::Collections::Generic;
using namespace Emgu::CV;
using namespace System::Runtime::InteropServices;
namespace ceresdotnet {

	
#pragma region enums

	public enum class CeresCallbackReturnType : int{
		SOLVER_ABORT = ceres::SOLVER_ABORT,
		SOLVER_CONTINUE = ceres::SOLVER_CONTINUE,
		SOLVER_TERMINATE_SUCCESSFULLY = ceres::SOLVER_TERMINATE_SUCCESSFULLY
	};

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
		None = 0,
		FocalLength = 1,
		PrincipalP = 2,
		R1 = 4,
		R2 = 8,
		P1 = 16,
		P2 = 32,
		R3 = 64,
		ALL = 127,
		GeoAutomation = 79
	};
	[FlagsAttribute]
	public enum class  BundleWorldCoordinatesFlags :int
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		ALL = 7
	};
	[FlagsAttribute]
	public enum class  BundlePointOrientFlags :int
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		Rodr1 = 8,
		Rodr2 = 16,
		Rodr3 = 32,
		Position = 7,
		Orientation = 56,
		ALL = 63
	};


#pragma endregion
	
#pragma region IterationCallback

	public delegate CeresCallbackReturnType Iteration(int iterationNr);

#pragma unmanaged
	typedef ceres::CallbackReturnType(__stdcall *Iteration_native_callback)(int);
	class NativeIterationCallback : public ceres::IterationCallback {
	public:
		Iteration_native_callback callback;

		NativeIterationCallback(Iteration_native_callback cb){
			callback = cb;
		}
		virtual ceres::CallbackReturnType operator()(const
			ceres::IterationSummary& summary) {
			return callback(summary.iteration);
		}
	};
#pragma managed

	static void setCallbackToProblem(Iteration^ ILcallback, ceres::Solver::Options* options){
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(ILcallback);
		Iteration_native_callback cb = static_cast<Iteration_native_callback>(ip.ToPointer());
		NativeIterationCallback* natcb = new NativeIterationCallback(cb);
		options->callbacks.push_back(natcb);
	};



#pragma endregion

#pragma region ParameterBlocks
	public ref class CeresParameterBlock abstract{
	private:
		bool _paramterizationset = false;
	internal:
		double* _data;
		//Enum^ _bundleFlags;

		virtual property Enum^ BundleFlagsEnum{ virtual Enum^ get() abstract; };
		virtual property int Length{ virtual int get()  abstract; }
		virtual bool getBlockFullyVariable() abstract;
		virtual bool getBlockFullyConstant() abstract;
		virtual ceres::SubsetParameterization* GetPrametrization() abstract;

		
		!CeresParameterBlock(){
			if (_data != nullptr) {
				delete[] _data;
				_data = nullptr;
			}
		}
		CeresParameterBlock(){
			_data = new double[Length];
			for (size_t i = 0; i < Length; i++)
			{
				_data[i] = 0;
			}
		}
		CeresParameterBlock(double* data){
			_data = data;
		}
		~CeresParameterBlock(){
			this->!CeresParameterBlock();
		}
		
		property size_t Sz{size_t get(){ return sizeof(double)*Length; }}

		void AddToProblem(ceres::Problem* problem){
			problem->AddParameterBlock(_data, Length);

			if (getBlockFullyConstant()){
				problem->SetParameterBlockConstant(_data);
			}
			else if (getBlockFullyVariable()){
				problem->SetParameterBlockVariable(_data);
			}
			else if (!_paramterizationset){
				_paramterizationset = true;
				problem->SetParameterization(_data, GetPrametrization());
			}
		}

		CeresParameterBlock^ CreateCopy(){
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

		List<Tuple<System::Reflection::PropertyInfo^, Object^>^>^ get_propertiesValues(){
			List<Tuple<System::Reflection::PropertyInfo^, Object^>^>^ r = gcnew List<Tuple<System::Reflection::PropertyInfo^, Object^>^>();
			Type^ t = this->GetType();
			for each (System::Reflection::PropertyInfo^ info in t->GetProperties())
			{
				Object^ o = info->GetMethod->Invoke(this, nullptr);
				r->Add(gcnew Tuple<System::Reflection::PropertyInfo^, Object^>(info, o));
			}
			return r;
		}
	};

	public ref class CeresParameterBlockCapture{
	public:
		List<CeresParameterBlock^>^ capture;
		initonly CeresParameterBlock^ block;

		CeresCallbackReturnType captureValues(int nr){
			if (block->BundleFlagsEnum->Equals(0)){
				return CeresCallbackReturnType::SOLVER_CONTINUE;
			}
			if (capture == nullptr){
				capture = gcnew List<CeresParameterBlock^>();
			}
			capture->Add(block->CreateCopy());
			return CeresCallbackReturnType::SOLVER_CONTINUE;
		}
	};

	public ref class CeresPointOrient : CeresParameterBlock {
	internal:
		BundlePointOrientFlags _bundleFlags;

		bool getBlockFullyVariable() override{ return _bundleFlags.HasFlag(BundlePointOrientFlags::ALL); }
		bool getBlockFullyConstant() override{ return _bundleFlags == BundlePointOrientFlags::None; }

		ceres::SubsetParameterization* GetPrametrization() override{
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
		property int Length{ int get() override{ return 6; }}
		property Enum^ BundleFlagsEnum{	Enum^ get() override { return _bundleFlags; };};
		
	public:

		 property BundlePointOrientFlags BundleFlags{
			BundlePointOrientFlags get()  { return _bundleFlags; };
			void set(BundlePointOrientFlags flags){ _bundleFlags = flags; }
		 };


		property array<double>^ RT {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(6);
				pin_ptr<double> p = &r[0];
				memcpy(p, _data, 6 * 8);
				return r;
			}
			void set(array<double>^ v){
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
			void set(array<double>^ v){
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
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(&_data[3], p, 3 * 8);
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

			ceres::RotationMatrixToAngleAxis((double*)rmat, _data);

			_data[3] = m.M41;
			_data[4] = m.M42;
			_data[5] = m.M43;
		}

		CeresPointOrient^ CreateCopy(){
			CeresPointOrient^ r = gcnew CeresPointOrient();
			r->BundleFlags = this->BundleFlags;
			memcpy(r->_data, this->_data, sizeof(double) * 6);
			return r;
		}

		static CeresPointOrient^ From(Matrix4d^ worldMat){
			CeresPointOrient^ r = gcnew CeresPointOrient();
			r->set(worldMat);
			return r;
		}


	};

	public ref class CeresIntrinsics : CeresParameterBlock{
	internal:
		BundleIntrinsicsFlags _bundleFlags;

		bool getBlockFullyVariable() override{ return _bundleFlags.HasFlag(BundleIntrinsicsFlags::ALL); }
		bool getBlockFullyConstant() override{ return _bundleFlags == BundleIntrinsicsFlags::None; }
		ceres::SubsetParameterization* GetPrametrization() override{
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

			ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(9, constant_data);

			return subset_parameterization;
		}
		property int Length{ int get() override{ return 9; }}
		property Enum^ BundleFlagsEnum{	Enum^ get() override { return BundleFlags; } };

	public:
		CeresIntrinsics(){};
		CeresIntrinsics(array<double>^ intr){
			Intrinsics = intr;
		}
		CeresIntrinsics(Emgu::CV::Matrix<double>^ cameraMat, array<double>^ distCoeffs){
			fx = cameraMat->default[0, 0];
			fy = cameraMat->default[0, 1];
			ppx = cameraMat->default[2, 0];
			ppy = cameraMat->default[2, 1];

			k1 = distCoeffs[0];
			k2 = distCoeffs[1];
			if (distCoeffs->Length == 4){
				p1 = distCoeffs[2];
				p2 = distCoeffs[3];
			}
			else{
				p1 = distCoeffs[2];
				p2 = distCoeffs[3];
				k3 = distCoeffs[4];
			}

		}

		property BundleIntrinsicsFlags BundleFlags{
			BundleIntrinsicsFlags get() override { return _bundleFlags; }
			void set(BundleIntrinsicsFlags flags){ _bundleFlags = flags; }
		};


		property array<double>^ Intrinsics {
			array<double>^ get() {
				array<double>^ r = gcnew array<double>(9);
				pin_ptr<double> p = &r[0];
				memcpy(p, _data, 9 * 8);
				return r;
			}
			void set(array<double>^ v){
				pin_ptr<double> p = &v[0];
				memcpy(_data, p, 9 * 8);
			}
		}
		
		property double fx{
			double get(){
				return _data[OFFSET_FOCAL_LENGTH_X];
			}void set(double value){
				_data[OFFSET_FOCAL_LENGTH_X] = value;
			}}
		property double fy{
			double get(){
				return _data[OFFSET_FOCAL_LENGTH_Y];
			}void set(double value){
				_data[OFFSET_FOCAL_LENGTH_Y] = value;
			}}
		property double ppx{
			double get(){
				return _data[OFFSET_PRINCIPAL_POINT_X];
			}void set(double value){
				_data[OFFSET_PRINCIPAL_POINT_X] = value;
			}}
		property double ppy{
			double get(){
				return _data[OFFSET_PRINCIPAL_POINT_Y];
			}void set(double value){
				_data[OFFSET_PRINCIPAL_POINT_Y] = value;
			}}
		property double k1{
			double get(){
				return _data[OFFSET_K1];
			}void set(double value){
				_data[OFFSET_K1] = value;
			}}
		property double k2{
			double get(){
				return _data[OFFSET_K2];
			}void set(double value){
				_data[OFFSET_K2] = value;
			}}
		property double k3{
			double get(){
				return _data[OFFSET_K3];
			}void set(double value){
				_data[OFFSET_K3] = value;
			}}
		property double p1{
			double get(){
				return _data[OFFSET_P1];
			}void set(double value){
				_data[OFFSET_P1] = value;
			}}
		property double p2{
			double get(){
				return _data[OFFSET_P2];
			}void set(double value){
				_data[OFFSET_P2] = value;
			}}

	};

	public ref class CeresPoint : CeresParameterBlock{
	internal:
		BundleWorldCoordinatesFlags _bundleFlags;

		property int Length{ int get() override{ return 3; }}
		bool getBlockFullyVariable() override{ return _bundleFlags.HasFlag(BundleWorldCoordinatesFlags::ALL); }
		bool getBlockFullyConstant() override{ return _bundleFlags == BundleWorldCoordinatesFlags::None; }
		ceres::SubsetParameterization* GetPrametrization() override{
			return NULL;
		}
		property Enum^ BundleFlagsEnum{
			Enum^ get() override { return BundleFlags; }
		};

	public:
		property BundleWorldCoordinatesFlags BundleFlags{
			BundleWorldCoordinatesFlags get() { return _bundleFlags; }
			void set(BundleWorldCoordinatesFlags flags){ _bundleFlags = flags; }
		};

		property double default[int] {
			double get(int i){
				return _data[i];
			}
			void set(int i, double d){
				_data[i] = d;
			}
		}

		property double X {
			double get(){
				return _data[0];
			}
			void set(double d){
				_data[0] = d;
			}
		}
		property double Y {
			double get(){
				return _data[1];
			}
			void set(double d){
				_data[1] = d;
			}
		}
		property double Z {
			double get(){
				return _data[2];
			}
			void set(double d){
				_data[2] = d;
			}
		}

		property array<double>^ Coordinates_arr{ array<double>^ get(){
			return gcnew array < double > {_data[0], _data[1], _data[2]};
		}
		void set(array<double>^ value){
			_data[0] = value[0];
			_data[1] = value[1];
			_data[2] = value[2];
		}
		}
	};

	generic <class T> where T : CeresParameterBlock
	public interface class ICeresParameterConvertable{
			T toCeresParameter();
			void updateFromCeres(T paramblock);
			T toCeresParameter(Enum^ BundleSettings);
		};

#pragma endregion
	

#pragma region "Calibratie" CSharp wrappers


	public ref class CeresMarker{
	public:
		CeresPoint^ Location;
		double x, y;
		int id;
	};

	public ref class CeresCamera{
	public:
		initonly CeresPointOrient^ External = gcnew CeresPointOrient();
		CeresIntrinsics^ Internal = gcnew CeresIntrinsics();
		


		CeresCamera(OpenTK::Matrix3d r, OpenTK::Vector3d rvec){
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

			ceres::RotationMatrixToAngleAxis((double*)rmat, External->_data);
			External->_data[3] = rvec.X;
			External->_data[4] = rvec.Y;
			External->_data[5] = rvec.Z;
		}

		CeresCamera(OpenTK::Matrix4d cameraWorldMat){
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

			ceres::RotationMatrixToAngleAxis((double*)&rmat(0), External->_data);


			External->_data[3] = m.M41;
			External->_data[4] = m.M42;
			External->_data[5] = m.M43;

		}
		CeresCamera(CeresIntrinsics^ intr, CeresPointOrient^ ext){
			this->Internal = intr;
			this->External = ext;
		}
	};

	public ref class CeresCameraCollection{
	public:
		List<CeresCamera^>^ Cameras;

		CeresPointOrient^ Position;

		void lockCameraExternals(){
			for each (CeresCamera^ cam in Cameras)
			{
				cam->External->BundleFlags = BundlePointOrientFlags::None;
			}
		}
		void UnlockCameraExternals(){
			for each (CeresCamera^ cam in Cameras)
			{
				cam->External->BundleFlags = BundlePointOrientFlags::ALL;
			}
		}
		void BindFirstCameraToCollectionPosition(){
			if (Cameras->Count > 0){
				UnlockCameraExternals();
				Position->BundleFlags = BundlePointOrientFlags::ALL;
				((*Cameras)[0])->External->BundleFlags = BundlePointOrientFlags::None;

			}
			else{
				throw gcnew Exception("# cameras = 0");
			}
		}

		CeresCameraCollection^ CreateSecondPositionCopy(){
			CeresCameraCollection^ r = gcnew CeresCameraCollection();
			r->Cameras = this->Cameras;
			//r->lockCameraExternals();
			r->Position = this->Position->CreateCopy();
			r->Position->BundleFlags = BundlePointOrientFlags::ALL;

			return r;
		}
	};

#pragma endregion


#pragma region Solving
	public ref class CeresBundler abstract{

		ceres::Problem* _problem;
		virtual void Bundle() abstract;
		event Iteration^ IterationE;

	};
	
	public ref class CeresCameraMultiCollectionBundler{
	public:
		delegate List<CeresMarker^>^ MarkersFromCameraDelegate(CeresCamera^, CeresCameraCollection^);

		List<CeresCameraCollection^>^ CollectionList = gcnew List<CeresCameraCollection^>();
		List<CeresCamera^>^ StandaloneCameraList = gcnew List<CeresCamera^>();
		MarkersFromCameraDelegate^ MarkersFromCamera;
		
		
		
		void bundleCollections(Iteration^ callback){

		    ceres::Problem::Options problem_options;
			ceres::Problem problem(problem_options);

			
			for each (CeresCamera^ camera in StandaloneCameraList){
				List<CeresMarker^>^ obss = MarkersFromCamera(camera, nullptr);
				for each (CeresMarker^ obs in obss)
				{
					camera->External->AddToProblem(&problem);
					camera->Internal->AddToProblem(&problem);
					obs->Location->AddToProblem(&problem);

					
					if (obs->Location != nullptr){
						problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
							ReprojectionErrorSingleCamera, 2, 9, 6, 3 >(new ReprojectionErrorSingleCamera(obs->x, obs->y)),
							NULL,
							camera->Internal->_data,

							camera->External->_data,
							(double*)obs->Location->_data);
					}
					else{
						int a = 4;
					}

					problem.SetParameterBlockConstant(obs->Location->_data);

				}
			}
			

			for each (CeresCameraCollection^ coll in CollectionList){
				coll->BindFirstCameraToCollectionPosition();
			}
			for each (CeresCameraCollection^ Collection in CollectionList){
				for each (CeresCamera^ camera in Collection->Cameras){
					auto obss = MarkersFromCamera(camera, Collection);
					for each (CeresMarker^ obs in obss)
					{
						obs->Location->AddToProblem(&problem);
						camera->Internal->AddToProblem(&problem);
						Collection->Position->AddToProblem(&problem);
						camera->External->AddToProblem(&problem);

						
						if (obs->Location != nullptr){
							problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
								ReprojectionErrorSysteemCamera, 2, 9, 6, 6, 3 >(new ReprojectionErrorSysteemCamera(obs->x, obs->y)),
								NULL,
								camera->Internal->_data,
								Collection->Position->_data,
								camera->External->_data,
								(double*)obs->Location->_data);
						}

						problem.SetParameterBlockConstant(obs->Location->_data);
					}
				}
			}



			ceres::Solver::Options options;
			options.use_nonmonotonic_steps = true;
			options.preconditioner_type = ceres::SCHUR_JACOBI;
			options.linear_solver_type = ceres::ITERATIVE_SCHUR;
			options.use_inner_iterations = true;
			options.max_num_iterations = 100;
			options.minimizer_progress_to_stdout = true;


			setCallbackToProblem(callback, &options);
			vector<double*>* pblocks = new vector<double*>();
			problem.GetParameterBlocks(pblocks);

			for each (auto collection in this->CollectionList)
			{
				
			}




			ceres::Solver::Summary summary;
			ceres::Solve(options, &problem, &summary);

			std::cout << "Final report:\n" << summary.FullReport();

		};

	};



	
	
	public ref class CeresCameraCollectionBundler{
	public:
		static array<double>^ ceresrotatepoint(array<double>^ rodr, array<double>^ t, array<double>^ point){
			array<double>^ r = gcnew array<double>(3);
			pin_ptr<double> rpin = &r[0];
			double* rn = rpin;

			pin_ptr<double> rodrpin = &rodr[0];
			double* rodrn = rodrpin;

			pin_ptr<double> tpin = &t[0];
			double* tn = tpin;

			pin_ptr<double> pointpin = &point[0];
			double* pointn = pointpin;

			ceres::AngleAxisRotatePoint(rodrn, pointn, rn);

			rn[0] += t[0];
			rn[1] += t[1];
			rn[2] += t[2];

			return r;
		}
		
		
		delegate List<CeresMarker^>^ MarkersFromCameraDelegate(CeresCamera^);

		CeresCameraCollection^ Collection;
		List<CeresCameraCollection^>^ CollectionList;
		Dictionary <CeresCamera^, List<CeresMarker^>^>^ Observations;
		MarkersFromCameraDelegate^ MarkersFromCamera; //enkel als Observations niet bestaat

		void bundleCollection(){
			bundleCollection(nullptr);
		}
		void bundleCollection(Iteration^ callback){
			Collection->BindFirstCameraToCollectionPosition();
			ceres::Problem::Options problem_options;
			ceres::Problem problem(problem_options);


			for each (CeresCamera^ camera in Collection->Cameras)
			{
				if (Observations != nullptr && Observations->ContainsKey(camera)){
					for each (CeresMarker^ obs in Observations[camera])
					{
						if (obs->Location != nullptr){
							problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
								ReprojectionErrorSysteemCamera, 2, 9, 6, 6, 3 >(new ReprojectionErrorSysteemCamera(obs->x, obs->y)),
								NULL,
								camera->Internal->_data,
								Collection->Position->_data,
								camera->External->_data,
								(double*)obs->Location->_data);
						}
						if (obs->Location->BundleFlags == BundleWorldCoordinatesFlags::None){
							problem.SetParameterBlockConstant((double*)obs->Location->_data);
						}
						else if (obs->Location->BundleFlags == BundleWorldCoordinatesFlags::ALL)
						{
						}
						else
						{
							std::vector<int> constant_coordinates;
							//if (!obs->Location->BundleFlags.HasFlag(BundleWorldCoordinatesFlags::X)) constant_coordinates.push_back(0);
							//if (!obs->Location->BundleFlags.HasFlag(BundleWorldCoordinatesFlags::Y)) constant_coordinates.push_back(1);
							//if (!obs->Location->BundleFlags.HasFlag(BundleWorldCoordinatesFlags::Z)) constant_coordinates.push_back(2);

							ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(3, constant_coordinates);
							problem.SetParameterization((double*)obs->Location->_data, subset_parameterization);
						}


						if (problem.GetParameterization(camera->Internal->_data) == NULL){

						}

						auto bundle_intrinsics = camera->Internal->BundleFlags;

						std::vector<int> constant_intrinsics;




						//ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
						//problem.SetParameterization(camera->Internal->_intrinsics, subset_parameterization);

					}



				}


			}
			ceres::Solver::Options options;
			options.use_nonmonotonic_steps = true;
			options.preconditioner_type = ceres::SCHUR_JACOBI;
			options.linear_solver_type = ceres::ITERATIVE_SCHUR;
			options.use_inner_iterations = true;
			options.max_num_iterations = 100;
			options.minimizer_progress_to_stdout = true;

			if (callback != nullptr){
				setCallbackToProblem(callback, &options);
			}

			vector<double*>* pramblcks = new vector<double*>();
			problem.GetParameterBlocks(pramblcks);
			int numparams = problem.NumParameters();

			ceres::Solver::Summary summary;
			ceres::Solve(options, &problem, &summary);
			std::cout << "Final report:\n" << summary.FullReport();

		};

	};

	public ref class CeresTestFunctions{
	public:
		static array<double>^ testProjectPoint(CeresCamera^ camera, CeresPointOrient^ systeem, CeresMarker^ marker){
			ReprojectionErrorSysteemCamera test(marker->x, marker->y);
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b = test(camera->Internal->_data, systeem->_data, camera->External->_data, marker->Location->_data, (double*)res);
			return resi;
		}
		static array<double>^ testProjectPointSingle(CeresCamera^ camera, CeresMarker^ marker){
			ReprojectionErrorSingleCamera test(marker->x, marker->y);
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b = test(camera->Internal->_data, camera->External->_data, marker->Location->_data, (double*)res);
			return resi;
		}
		static array<double>^ testProjectPoint3(CeresCamera^ camera, CeresPointOrient^ systeem, CeresMarker^ marker){
			ReprojectionErrorSysteemCamera test(marker->x, marker->y);
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b = test(camera->Internal->_data, systeem->_data, camera->External->_data, marker->Location->_data, (double*)res);
			return resi;
		}
		static array<double>^ testProjectPoint2(CeresCamera^ camera, Matrix4d worldMat, array<double>^ angleaxis, CeresPointOrient^ systeem, CeresMarker^ marker){
			ReprojectionErrorSysteemCamera test(marker->x, marker->y);
			worldMat.Invert();

			pin_ptr<double> axis = &angleaxis[0];
			double* axisp = axis;
			double proj[3];
			ceres::AngleAxisRotatePoint(axisp, marker->Location->_data, proj);

			for (size_t i = 0; i < 3; i++)
			{
				axisp[i] = -axisp[i];
			}
			ceres::AngleAxisRotatePoint(axisp, marker->Location->_data, proj);

			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b = test(camera->Internal->_data, systeem->_data, camera->External->_data, marker->Location->_data, (double*)res);
			return resi;
		}
	};

#pragma endregion
		
};