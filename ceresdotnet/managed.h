#pragma once

#include "Stdafx.h"
#include <vcclr.h>


#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
#pragma managed(pop)



#include "IterationSummary.h"
#include "ParameterBlocks.h"
#include "Offsets.h"


/// TODO:
/// new ceres 1.12.0 -> bool problem::getblockconstant(void*) -> logica CeresParameterBlock kan beter
/// fix names & exports in ceresdotnetnative
/// CreateCostFunctionSystemCamera

using namespace System::Collections::Generic;
using namespace Emgu::CV;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
using namespace OpenTK;
using namespace System;
using namespace System::Runtime::CompilerServices;

namespace ceresdotnet {



	
#pragma region IterationCallback
	
	public delegate CeresCallbackReturnType Iteration(Object^ sender, IterationSummary^ summary);
	typedef ceres::CallbackReturnType (__stdcall *Iteration_native_callback)(Object^, IterationSummary^);

//#pragma unmanaged
	class NativeIterationCallback : public ceres::IterationCallback {
	public:
		gcroot<Object^> sender;
		Iteration_native_callback callback;

		NativeIterationCallback(Iteration_native_callback cb, gcroot<Object^> senderbundler){
			callback = cb;
			sender = senderbundler;
		}
		virtual ceres::CallbackReturnType operator()(const
			ceres::IterationSummary& summary) {
			return callback(sender,gcnew IterationSummary(&summary));
		}
	};
#pragma managed

	static void setCallbackToProblem(Iteration^ ILcallback, ceres::Solver::Options* options, Object^callbacksender){
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(ILcallback);
		Iteration_native_callback cb = static_cast<Iteration_native_callback>(ip.ToPointer());
		NativeIterationCallback* natcb = new NativeIterationCallback(cb, callbacksender);
		options->callbacks.push_back(natcb);
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
			r->Position = this->Position->CreateCopy();
			r->Position->BundleFlags = BundlePointOrientFlags::ALL;

			return r;
		}CeresCameraCollection^ CreateSecondPositionCopy(CeresPointOrient^ newpos){
			CeresCameraCollection^ r = gcnew CeresCameraCollection();
			r->Cameras = this->Cameras;
			r->Position = newpos;
			return r;
		}
	};

#pragma endregion

#pragma region Solving

	public ref class CustomBundler{
	internal:
		ceres::Problem::Options* problem_options;
		ceres::Problem* problem;

		Dictionary < IntPtr, ICeresParameterConvertable<CeresParameterBlock^>^>^ ptrtoparamblock;

	private:
		void addBlock(ICeresParameterConvertable<CeresParameterBlock^>^ b){
		    
			ptrtoparamblock->Add(IntPtr(b->toCeresParameter()->_data), b);
		}

	public:

		void AddSysteemCameraBlock(PointF^ feature,
			ICeresParameterConvertable<CeresIntrinsics^>^ cameraintr,
			ICeresParameterConvertable<CeresPointOrient^>^ systemextr,
			ICeresParameterConvertable<CeresPointOrient^>^ cameraextr,
			ICeresParameterConvertable<CeresPoint^>^ marker3d){

			auto internal = cameraintr->toCeresParameter();
			auto external = cameraextr->toCeresParameter();
			auto location = marker3d->toCeresParameter();
			auto systeem = systemextr->toCeresParameter();

			internal->AddToProblem(problem);
			external->AddToProblem(problem);
			location->AddToProblem(problem);
			systeem->AddToProblem(problem);

			problem->AddResidualBlock(new ceres::AutoDiffCostFunction <
				ReprojectionErrorSysteemCamera, 2, 9, 6, 6, 3 > (new ReprojectionErrorSysteemCamera(feature->X, feature->Y)),
				NULL,
				internal->_data,
				systeem->_data,
				external->_data,
				location->_data);
		}
		void AddSingleCameraBlock(PointF^ feature,
			ICeresParameterConvertable<CeresIntrinsics^>^ cameraintr,
			ICeresParameterConvertable<CeresPointOrient^>^ cameraextr,
			ICeresParameterConvertable<CeresPoint^>^ marker3d){

			auto internal = cameraintr->toCeresParameter();
			auto external = cameraextr->toCeresParameter();
			auto location = marker3d->toCeresParameter();

			internal->AddToProblem(problem);
			external->AddToProblem(problem);
			location->AddToProblem(problem);

			problem->AddResidualBlock(new ceres::AutoDiffCostFunction <
				ReprojectionErrorSingleCamera, 2, 9, 6, 3 >(new ReprojectionErrorSingleCamera(feature->X, feature->Y)),
				NULL,
				internal->_data,
				external->_data,
				location->_data);
		}

		void Bundle(){
			ceres::Solver::Options options;
			options.use_nonmonotonic_steps = true;
			options.preconditioner_type = ceres::SCHUR_JACOBI;
			options.linear_solver_type = ceres::ITERATIVE_SCHUR;
			options.use_inner_iterations = true;
			options.max_num_iterations = 100;
			options.minimizer_progress_to_stdout = true;


			options.num_threads = 8;
			options.num_linear_solver_threads = 8;

			ceres::Solver::Summary summary;
			ceres::Solve(options, problem, &summary);

			std::cout << "Final report:\n" << summary.FullReport();
		}
	};
	

	public ref class CeresCameraMultiCollectionBundler{
	public:
		delegate List<CeresMarker^>^ MarkersFromCameraDelegate(CeresCamera^, CeresCameraCollection^);

		List<CeresCameraCollection^>^ CollectionList = gcnew List<CeresCameraCollection^>();
		List<CeresCamera^>^ StandaloneCameraList = gcnew List<CeresCamera^>();
		MarkersFromCameraDelegate^ MarkersFromCamera;

		
		int ScaledownMaps;
		Dictionary<CeresIntrinsics^,Matrix<double>^>^ InfluenceMaps;

		double normaldist(double x, double sigmaSq, double mean) {
			double mult = 1 / sqrt(2 * 3.14159265358979323846*sigmaSq);
			double exp = -1 * pow(x - mean,2) / (2 * sigmaSq);
			return mult * pow(2.71828182845904523536, exp);
		}

		void CalculateInfluenceMaps(float sigma,float mean,int scaledown,double maxMultiply,double minMultiply,double IgnoreRange) {
			ScaledownMaps = scaledown;
			double sigmaSq = sigma*sigma;
			Dictionary<CeresIntrinsics^, List<CeresMarker^>^>^ allobs = gcnew Dictionary<CeresIntrinsics^, List<CeresMarker^>^>();


			for each (CeresCamera^ camera in StandaloneCameraList) {
				List<CeresMarker^>^ obss = MarkersFromCamera(camera, nullptr);
				CeresIntrinsics^ intr = camera->Internal;
				
				if (!allobs->ContainsKey(intr)) {
					allobs->Add(intr, gcnew  List<CeresMarker^>());
				}
				allobs[intr]->AddRange(obss);
			}

			for each (CeresCameraCollection^ Collection in CollectionList) {
				for each (CeresCamera^ camera in Collection->Cameras) {
					auto obss = MarkersFromCamera(camera, Collection);
					CeresIntrinsics^ intr = camera->Internal;
					
					if (!allobs->ContainsKey(intr)) {
						allobs->Add(intr, gcnew  List<CeresMarker^>());
					}
					allobs[intr]->AddRange(obss);
				}
			}

			InfluenceMaps = gcnew Dictionary<CeresIntrinsics^, Matrix<double>^>();

			for each (CeresIntrinsics^ intr in allobs->Keys) {
				auto map = gcnew Matrix<double>(intr->_imageHeight/scaledown, intr->_imageWidth/scaledown);
				InfluenceMaps->Add(intr, map);

				double max, min;

				for (size_t r = 0; r < map->Height; r++) {
					for (size_t c = 0; c < map->Width; c++) {
						for each (CeresMarker^ marker in allobs[intr]) {
							float dist = sqrt(pow(marker->y - r*scaledown, 2) + pow(marker->x - c*scaledown, 2));
							if (dist < sigma * 3) { //99.7%
								map[r, c] += normaldist(dist, sigmaSq, mean);
							}
						}
					}
				}
				for (size_t r = 0; r < map->Height; r++) {
					for (size_t c = 0; c < map->Width; c++) {
						max = map[r, c] > max ? map[r, c] : max;
						min = map[r, c] < min ? map[r, c] : min;
					}
				}

				double upperbound = max - (max - min)*IgnoreRange;
				double lowerbound = min + (max - min)*IgnoreRange;

				//zoek x*a+b=y

				double a = (maxMultiply - minMultiply) / (lowerbound - upperbound);
				double b = minMultiply - upperbound*a;

				for (size_t r = 0; r < map->Height; r++) {
					for (size_t c = 0; c < map->Width; c++) {
						map[r, c] = map[r, c] * a + b;
						map[r, c] = map[r, c] > maxMultiply ? maxMultiply : map[r, c];
						map[r, c] = map[r, c] < minMultiply ? minMultiply : map[r, c];
					}
				}
			}
		}
		


		void bundleCollections(Iteration^ callback);


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
				setCallbackToProblem(callback, &options,this);
			}


			int numparams = problem.NumParameters();

			ceres::Solver::Summary summary;
			ceres::Solve(options, &problem, &summary);
			std::cout << "Final report:\n" << summary.FullReport();

		};

	};
	
	static public ref class CeresTestFunctions abstract sealed{
	private:
		static ReprojectionErrorSingleCameraHighDist* agisoftproj;
		static ReprojectionErrorSingleCamera* standardproj;
		static ReprojectionErrorSingleCameraOpenCVAdvancedDist* opencvadvproj;

		static ReprojectionErrorSysteemCameraHighDist* agisoftproj_syst;
		static ReprojectionErrorSysteemCamera* standardproj_syst;
		static ReprojectionErrorSysteemCameraOpenCVAdvancedDist* opencvadvproj_syst;
	public:

		static CeresTestFunctions()
		{
			agisoftproj = new ReprojectionErrorSingleCameraHighDist(0,0);
			standardproj = new ReprojectionErrorSingleCamera(0, 0);
			opencvadvproj = new ReprojectionErrorSingleCameraOpenCVAdvancedDist(0,0);

			agisoftproj_syst = new ReprojectionErrorSysteemCameraHighDist(0,0) ;
			standardproj_syst = new ReprojectionErrorSysteemCamera(0, 0);
			opencvadvproj_syst = new ReprojectionErrorSysteemCameraOpenCVAdvancedDist(0, 0);
		}


		static PointF ProjectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, Matrix<double>^ point3d) {
			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b;

			switch (intr->Distortionmodel)
			{
			case DistortionModel::OpenCVAdvanced:
				b = (*opencvadvproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			case DistortionModel::AgisoftPhotoscan:
				b = (*agisoftproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			case DistortionModel::Standard:
			default:
				b = (*standardproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			}
			return PointF((float)resi[0], (float)resi[1]);
		}
		
		static PointF ProjectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr,CeresPointOrient^ system, Matrix<double>^ point3d) {
			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b;

			switch (intr->Distortionmodel)
			{
			case DistortionModel::OpenCVAdvanced:
				b = (*opencvadvproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
					break;
			case DistortionModel::AgisoftPhotoscan:
				b = (*agisoftproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
				break;
			case DistortionModel::Standard:
			default:
				b = (*standardproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
				break;
			}
			return PointF((float)resi[0], (float)resi[1]);
		}
		
		static PointF ReprojectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, PointF point2d, Matrix<double>^ point3d) {
			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];

			bool b;
			switch (intr->Distortionmodel)
			{
			case DistortionModel::OpenCVAdvanced:
				b = (*opencvadvproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			case DistortionModel::AgisoftPhotoscan:
				b = (*agisoftproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			case DistortionModel::Standard:
			default:
				b = (*standardproj)(intr->_data, extr->_data, p3, (double*)res);
				break;
			}
			return PointF((float)resi[0] - point2d.X, (float)resi[1] - point2d.Y);
		}

		static PointF ReprojectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, CeresPointOrient^ system,  PointF point2d, Matrix<double>^ point3d) {
			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };
			array<double>^ resi = gcnew array<double>(2);
			pin_ptr<double> res = &resi[0];
			bool b;

			switch (intr->Distortionmodel)
			{
			case DistortionModel::OpenCVAdvanced:
				b = (*opencvadvproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
				break;
			case DistortionModel::AgisoftPhotoscan:
				b = (*agisoftproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
				break;
			case DistortionModel::Standard:
			default:
				b = (*standardproj_syst)(intr->_data, extr->_data, system->_data, p3, (double*)res);
				break;
			}
			return PointF((float)resi[0] - point2d.X, (float)resi[1] - point2d.Y);
		}

	};

#pragma endregion
		
};