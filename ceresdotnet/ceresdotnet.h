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
using namespace Calibratie;
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
	// The intrinsics need to get combined into a single parameter block; use these
	// enums to index instead of numeric constants.
	enum {
		OFFSET_FOCAL_LENGTH,
		OFFSET_PRINCIPAL_POINT_X,
		OFFSET_PRINCIPAL_POINT_Y,
		OFFSET_K1,
		OFFSET_K2,
		OFFSET_K3,
		OFFSET_P1,
		OFFSET_P2,
	};
	enum {
		OFFSET2_FOCAL_LENGTH_X,
		OFFSET2_FOCAL_LENGTH_Y,
		OFFSET2_PRINCIPAL_POINT_X,
		OFFSET2_PRINCIPAL_POINT_Y,
		OFFSET2_K1,
		OFFSET2_K2,
		OFFSET2_K3,
		OFFSET2_P1,
		OFFSET2_P2,
	};

	// Returns a pointer to the camera corresponding to a image.
	EuclideanCamera *CameraForImage(vector<EuclideanCamera> *all_cameras, const int image);
    const EuclideanCamera *CameraForImage(const vector<EuclideanCamera> &all_cameras, const int image);

	// Returns maximal image number at which marker exists.
	int MaxImage(const vector<Marker> &all_markers);

	
	// Returns a pointer to the point corresponding to a track.
	EuclideanPoint *PointForTrack(vector<EuclideanPoint> *all_points,const int track);



	// Apply camera intrinsics to the normalized point to get image coordinates.
	// This applies the radial lens distortion to a point which is in normalized
	// camera coordinates (i.e. the principal point is at (0, 0)) to get image
	// coordinates in pixels. Templated for use with autodifferentiation.
	template <typename T>
	inline void ApplyRadialDistortionCameraIntrinsics(const T &focal_length_x,
		const T &focal_length_y,
		const T &principal_point_x,
		const T &principal_point_y,
		const T &k1,
		const T &k2,
		const T &k3,
		const T &p1,
		const T &p2,
		const T &normalized_x,
		const T &normalized_y,
		T *image_x,
		T *image_y) {
		T x = normalized_x;
		T y = normalized_y;

		T r2 = x*x + y*y;
		T r4 = r2 * r2;
		T r6 = r4 * r2;
		T r_coeff = (T(1) + k1*r2 + k2*r4 + k3*r6);
		T xd = x * r_coeff + T(2)*p1*x*y + p2*(r2 + T(2)*x*x);
		T yd = y * r_coeff + T(2)*p2*x*y + p1*(r2 + T(2)*y*y);

		*image_x = focal_length_x * xd + principal_point_x;
		*image_y = focal_length_y * yd + principal_point_y;
	}

	

	struct OpenCVReprojectionError {
		OpenCVReprojectionError(const double observed_x, const double observed_y)
			: observed_x(observed_x), observed_y(observed_y) {}

		template <typename T>
		bool operator()(const T* const intrinsics,const T* const R_t,const T* const X,	T* residuals) const {

			const T& focal_length = intrinsics[OFFSET_FOCAL_LENGTH];
			const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
			const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
			const T& k1 = intrinsics[OFFSET_K1];
			const T& k2 = intrinsics[OFFSET_K2];
			const T& k3 = intrinsics[OFFSET_K3];
			const T& p1 = intrinsics[OFFSET_P1];
			const T& p2 = intrinsics[OFFSET_P2];
			T x[3];
			ceres::AngleAxisRotatePoint(R_t, X, x);
			x[0] += R_t[3];
			x[1] += R_t[4];
			x[2] += R_t[5];

			// Normaliseren
			T xn = x[0] / x[2];
			T yn = x[1] / x[2];
			T predicted_x, predicted_y;

			ApplyRadialDistortionCameraIntrinsics(focal_length,	focal_length,principal_point_x,	principal_point_y,
				k1, k2, k3,	
				p1, p2,
				xn, yn,
				&predicted_x,
				&predicted_y);

			residuals[0] = predicted_x - T(observed_x);
			residuals[1] = predicted_y - T(observed_y);
			return true;
		}
		const double observed_x;
		const double observed_y;
	};

	typedef void applyOffsetRT(const double* baseR_t,double* outR_t);

	struct ReprojectionError {

		ReprojectionError(const double observed_x, const double observed_y)
			: observed_x(observed_x), observed_y(observed_y) {};

		struct linkedCameraPos{
			applyOffsetRT function;
			double* baseR_t;
		};

		template <typename T>
		bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {

			const T& focal_length_x = intrinsics[OFFSET2_FOCAL_LENGTH_X];
			const T& focal_length_y = intrinsics[OFFSET2_FOCAL_LENGTH_Y];
			const T& principal_point_x = intrinsics[OFFSET2_PRINCIPAL_POINT_X];
			const T& principal_point_y = intrinsics[OFFSET2_PRINCIPAL_POINT_Y];
			const T& k1 = intrinsics[OFFSET2_K1];
			const T& k2 = intrinsics[OFFSET2_K2];
			const T& k3 = intrinsics[OFFSET2_K3];
			const T& p1 = intrinsics[OFFSET2_P1];
			const T& p2 = intrinsics[OFFSET2_P2];

			T rotnative[9];
			

			ceres::AngleAxisToRotationMatrix(R_t, rotnative);
			const T& r0 = rotnative[0];
			const T& r1 = rotnative[1];
			const T& r2 = rotnative[2];
			const T& r3 = rotnative[3];
			const T& r4 = rotnative[4];
			const T& r5 = rotnative[5];
			const T& r6 = rotnative[6];
			const T& r7 = rotnative[7];
			const T& r8 = rotnative[8];

			T x[3];
			//ceres::AngleAxisRotatePoint(R_t, X, x);
			
			/*x[0] += R_t[3];
			x[1] += R_t[4];
			x[2] += R_t[5];*/
			/*double xx = X[0] * rotnative[0] + X[1] * rotnative[3] + X[2] * rotnative[6] + 1 * R_t[3];
			double yy = X[0] * rotnative[1] + X[1] * rotnative[4] + X[2] * rotnative[7] + 1 * R_t[4];
			double zz = X[0] * rotnative[2] + X[1] * rotnative[5] + X[2] * rotnative[8] + 1 * R_t[5];*/

			/*x[0] = (T)xx;
			x[1] = (T)yy;
			x[2] = (T)zz;*/
			
			x[0] = X[0] * r0 + X[1] * r3 + X[2] * r6 + R_t[3];
			x[1] = X[0] * r1 + X[1] * r4 + X[2] * r7 + R_t[4];
			x[2] = X[0] * r2 + X[1] * r5 + X[2] * r8 + R_t[5];

			// Normaliseren
			T xn = x[0] / x[2];
			T yn = x[1] / x[2];
			T predicted_x, predicted_y;

			ApplyRadialDistortionCameraIntrinsics(focal_length_x,
				focal_length_y,
				principal_point_x,
				principal_point_y,
				k1, k2, k3,
				p1, p2,
				xn, yn,
				&predicted_x,
				&predicted_y);

			residuals[0] = predicted_x - T(observed_x);
			residuals[1] = predicted_y - T(observed_y);
			return true;
		}
		const double observed_x;
		const double observed_y;
		linkedCameraPos link;
	};

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


}