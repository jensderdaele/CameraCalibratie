#pragma once

//#include "Stdafx.h"
#include <vcclr.h>


#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
//#include "opencv2\opencv.hpp"
#pragma managed(pop)

//#include <boost\filesystem.hpp>
//#include <boost\unordered_map.hpp>

#include "IterationSummary.h"
#include "ParameterBlocks.h"
#include "SolverOptions.h"

using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
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

	public interface class ICeresMarker {
		property ICeresPoint^ Location {ICeresPoint^ get(); }
		property float X {float get(); }
		property float Y {float get(); }
	};

	public ref class CeresMarker : ICeresMarker {
		private:
			ICeresPoint^ _location;
	public:
		property ICeresPoint^ Location {
			virtual ICeresPoint^ get() { return _location; }
			void set(ICeresPoint^ value) { _location = value; }
		}
		float x, y;
		int id;


		virtual property float X { float get()  { return x; } }
		virtual property float Y {float get()  { return y; } }
	};

	public interface class ICeresCamera {
		property ICeresPointOrient^ External {ICeresPointOrient^ get(); }
		property ICeresIntrinsics^ Internal {ICeresIntrinsics^ get(); }
	};

	public ref class CeresCamera : ICeresCamera {
	private:
		initonly ICeresPointOrient^ _external;// = gcnew CeresPointOrient();
		ICeresIntrinsics^ _internal;// = gcnew CeresIntrinsics();
	public:

				

		CeresCamera(ICeresIntrinsics^ intr, ICeresPointOrient^ ext){
			this->_internal = intr;
			this->_external = ext;
		}

		property ICeresPointOrient^ External {
			virtual ICeresPointOrient^ get() { return _external; };
			//void set(CeresPointOrient^ value) { _external = value; }
		};
		property ICeresIntrinsics^ Internal {
			virtual ICeresIntrinsics^ get() { return _internal; };
			virtual void set(ICeresIntrinsics^ value) { _internal = value; };
		}

	};

	public ref class CeresCameraCollection{
	public:
		List<ICeresCamera^>^ Cameras;

		CeresPointOrient^ Position;

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

	public interface class ICeresGCP {
		property ICeresScaleTransform^ Transformation {ICeresScaleTransform^ get(); }
		property ICeresPoint^ Triangulated {ICeresPoint^ get(); }
		property double observed_x {double get(); }
		property double observed_y {double get(); }
		property double observed_z {double get(); }
	};

	public ref class CeresGCP : ICeresGCP {
	private:
		ICeresPoint^ _triangulated;
		ICeresScaleTransform^ _transformation;
		double _observed_x, _observed_y, _observed_z;
	public:
		virtual property ICeresPoint^ Triangulated {
			ICeresPoint^ get() { return _triangulated; }
			void set(ICeresPoint^ v) { _triangulated = v; }
		}

		virtual property ICeresScaleTransform^ Transformation {
			ICeresScaleTransform^ get() { return _transformation; }
			void set(ICeresScaleTransform^ v) { _transformation = v; }
		}

		virtual property double observed_x {
			double get() { return _observed_x; }
		}
		virtual property double observed_y {
			double get() { return _observed_y; }
		}
		virtual property double observed_z {
			double get() { return _observed_z; }
		}
	};

#pragma endregion

#pragma region Solving

	public ref class CeresCameraMultiCollectionBundler{
	public:
		delegate List<CeresMarker^>^ MarkersFromCameraDelegate(CeresCamera^, CeresCameraCollection^);

		List<CeresCameraCollection^>^ CollectionList = gcnew List<CeresCameraCollection^>();
		List<CeresCamera^>^ StandaloneCameraList = gcnew List<CeresCamera^>();
		MarkersFromCameraDelegate^ MarkersFromCamera;
		List<CeresGCP^>^ GCPList = gcnew List<CeresGCP^>();

		
		int ScaledownMaps;
		Dictionary<CeresIntrinsics^, Emgu::CV::Matrix<double>^>^ InfluenceMaps;

		double normaldist(double x, double sigmaSq, double mean) {
			double mult = 1 / sqrt(2 * 3.14159265358979323846*sigmaSq);
			double exp = -1 * pow(x - mean,2) / (2 * sigmaSq);
			return mult * pow(2.71828182845904523536, exp);
		}

		void CalculateInfluenceMaps(float sigma,float mean,int scaledown,double maxMultiply,double minMultiply,double IgnoreRange) {
			/*
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
			}*/
		}
		


		void bundleCollections(Iteration^ callback);


	};

			
	static public ref class CeresTestFunctions abstract sealed{
	private:
		static ReprojectionErrorSingleCameraHighDist* agisoftproj;
		static ReprojectionErrorSingleCamera* standardproj;
		static ReprojectionErrorSingleCameraOpenCVAdvancedDist* opencvadvproj;

		static ReprojectionErrorSysteemCameraHighDist* agisoftproj_syst;
		static ReprojectionErrorSysteemCamera* standardproj_syst;
		static ReprojectionErrorSysteemCameraOpenCVAdvancedDist* opencvadvproj_syst;

		static GCPError* gcperror;
	public:
		static CeresTestFunctions()
		{
			agisoftproj = new ReprojectionErrorSingleCameraHighDist(0,0);
			standardproj = new ReprojectionErrorSingleCamera(0, 0);
			opencvadvproj = new ReprojectionErrorSingleCameraOpenCVAdvancedDist(0,0);

			agisoftproj_syst = new ReprojectionErrorSysteemCameraHighDist(0,0) ;
			standardproj_syst = new ReprojectionErrorSysteemCamera(0, 0);
			opencvadvproj_syst = new ReprojectionErrorSysteemCameraOpenCVAdvancedDist(0, 0);

			gcperror = new GCPError(0, 0, 0);
		}

		static array<double>^ TransformGCP(Emgu::CV::Matrix<double>^ point3d, ceresdotnet::CeresScaledTransformation^ transf) {
			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };

			array<double>^ resi = gcnew array<double>(3);
			pin_ptr<double> res = &resi[0];

			double* transformation = transf->_data;


			ceres::AngleAxisRotatePoint(transf->_data, p3, (double*)res);


			res[0] *= transformation[6];
			res[1] *= transformation[6];
			res[2] *= transformation[6];

			res[0] += transformation[3];
			res[1] += transformation[4];
			res[2] += transformation[5];

			return resi;
		}
		/*
		static array<double>^ TransformGCP(Emgu::CV::Matrix<double>^ point3d, ceresdotnet::ICeresScaledTransformation^ itransf) {

			double p3[3] = { point3d[0,0],point3d[1,0],point3d[2,0] };

			array<double>^ resi = gcnew array<double>(3);
			pin_ptr<double> res = &resi[0];

			CeresScaledTransformation^ transf = gcnew CeresScaledTransformation(itransf);

			double* transformation = transf->_data;


			ceres::AngleAxisRotatePoint(transf->_data, p3, (double*)res);


			res[0] *= transformation[6];
			res[1] *= transformation[6];
			res[2] *= transformation[6];

			res[0] += transformation[3];
			res[1] += transformation[4];
			res[2] += transformation[5];

			return resi;
		}*/
		
		static PointF ProjectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, Emgu::CV::Matrix<double>^ point3d) {
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
		
		static PointF ProjectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr,CeresPointOrient^ system, Emgu::CV::Matrix<double>^ point3d) {
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
		
		static PointF ReprojectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, PointF point2d, Emgu::CV::Matrix<double>^ point3d) {
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

		static PointF ReprojectPoint(CeresIntrinsics^ intr, CeresPointOrient^ extr, CeresPointOrient^ system,  PointF point2d, Emgu::CV::Matrix<double>^ point3d) {
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