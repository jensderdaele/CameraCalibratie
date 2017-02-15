// ceresdotnet.h

#include <cstdio>
#include <fcntl.h>
#include <sstream>
#include <string>
#include <vector>

#pragma comment(lib, "ceres.lib")
#pragma comment(lib, "libglog_static.lib")
#ifdef _MSC_VER
#  include <io.h>
#  define open _open
#  define close _close
typedef unsigned __int32 uint32_t;
#else
#include <stdint.h>
#include <unistd.h>
// O_BINARY is not defined on unix like platforms, as there is no
// difference between binary and text files.
#define O_BINARY 0
#endif



#include "ceres/ceres.h"
#include "ceres/rotation.h"
//#include "gflags/gflags.h"
#include "glog/logging.h"
#include <Eigen/StdVector>

#pragma once


using namespace OpenTK;
using namespace System;
using namespace System::Collections::Generic;
using namespace SceneManager;
using namespace System::Runtime::CompilerServices;


typedef Eigen::Matrix<double, 3, 3> Mat3;
typedef Eigen::Matrix<double, 6, 1> Vec6;
typedef Eigen::Vector3d Vec3;
typedef Eigen::Vector4d Vec4;
using std::vector;

//#define EIGEN_DONT_ALIGN_STATICALLY
//EIGEN_DEFINE_STL_VECTOR_SPECIALIZATION(Eigen::Matrix<double, 6, 1>)

namespace ceresdotnet{
	/*
	enum BundleIntrinsics {
		BUNDLE_NO_INTRINSICS = 0,
		BUNDLE_FOCAL_LENGTH = 1,
		BUNDLE_PRINCIPAL_POINT = 2,
		BUNDLE_RADIAL_K1 = 4,
		BUNDLE_RADIAL_K2 = 8,
		BUNDLE_RADIAL = 12,
		BUNDLE_TANGENTIAL_P1 = 16,
		BUNDLE_TANGENTIAL_P2 = 32,
		BUNDLE_TANGENTIAL = 48,
		BUNDLE_ALL = 1 | 2 | 12 | 48,
	};






#pragma region unmanagedStructs

	// A EuclideanCamera is the location and rotation of the camera
	// viewing an image.
	//
	// image identifies which image this camera represents.
	// R is a 3x3 matrix representing the rotation of the camera.
	// t is a translation vector representing its positions.
	struct EuclideanCamera {
		EuclideanCamera() : image(-1) {
			Rt = new Vec6();
			intrinsics = new double[9];
		}
		
		~EuclideanCamera(){
			//delete [] intrinsics; crasht soms
		}
		
		///EuclideanCamera(const EuclideanCamera ^c) : image(c->image), R(c->R), t(c->t) {}
		int image;
		Mat3 R;
		Vec3 t;
		double* intrinsics;
		int bundle_intrinsics;
		Vec6* Rt;
	};


	// A Marker is the 2D location of a tracked point in an image.
	//
	// x and y is the position of the marker in pixels from the top left corner
	// in the image identified by an image. All markers for to the same target
	// form a track identified by a common track number.
	struct Marker {
		int image;
		int track;
		double x, y;
	};


	// A Point is the 3D location of a track.
	//
	// track identifies which track this point corresponds to.
	// X represents the 3D position of the track.
	struct EuclideanPoint {
		EuclideanPoint() : track(-1) {}
		EuclideanPoint(const EuclideanPoint &p) : track(p.track), X(p.X) {}
		int track;
		Vec3 X;
	};
#pragma endregion




	// Denotes which blocks to keep constant during bundling.
	// For example it is useful to keep camera translations constant
	// when bundling tripod motions.
	enum BundleConstraints {
		BUNDLE_NO_CONSTRAINTS = 0,
		BUNDLE_NO_TRANSLATION = 1,
	};
	enum {
		OFFSET_FOCAL_LENGTH_X,
		OFFSET_FOCAL_LENGTH_Y,
		OFFSET_PRINCIPAL_POINT_X,
		OFFSET_PRINCIPAL_POINT_Y,
		OFFSET_K1,
		OFFSET_K2,
		OFFSET_P1,
		OFFSET_P2,
		OFFSET_K3
	};

	// Returns a pointer to the camera corresponding to a image.
	EuclideanCamera *CameraForImage(vector<EuclideanCamera> *all_cameras, const int image);
    const EuclideanCamera *CameraForImage(const vector<EuclideanCamera> &all_cameras, const int image);

	// Returns maximal image number at which marker exists.
	int MaxImage(const vector<Marker> &all_markers);

	
	// Returns a pointer to the point corresponding to a track.
	EuclideanPoint *PointForTrack(vector<EuclideanPoint> *all_points,const int track);




	// Print a message to the log which camera intrinsics are gonna to be optimized.
	void BundleIntrinsicsLogMessage(const int bundle_intrinsics);

	void PrintCameraIntrinsics(const char *, const double *);
	void PrintCameraIntrinsics2(const char *text, const double *camera_intrinsics);


	Vec6* PackCamerasRotationAndTranslation(const vector<Marker> &all_markers, const vector<EuclideanCamera> &all_cameras);

	void UnpackCamerasRotationAndTranslation(const vector<Marker> &all_markers, const vector<Vec6> &all_cameras_R_t, vector<EuclideanCamera> *all_cameras);


	void EuclideanBundleCommonIntrinsics(const vector<Marker> &all_markers,
		const int bundle_intrinsics,
		const int bundle_constraints,
		double *camera_intrinsics,
		vector<EuclideanCamera> *all_cameras,
		vector<EuclideanPoint> *all_points);


	int main(int argc, char **argv);

	*/
}