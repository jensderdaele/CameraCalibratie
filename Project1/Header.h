#pragma once
#include <ceres\ceres.h>
#include <ceres\rotation.h>


typedef bool(*COSTOPERATOR_SINGLECAMERA)(const double* const intrinsics, const double* const R_t, const double* const X, double* residuals);
typedef bool(*COSTOPERATOR_SYSTEMCAMERA)(const double* const intrinsics, const double* const R_t_systeem, const double* const R_t, const double* const X, double* residuals);

enum {
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
	OFFSET_P3, //s1
	OFFSET_P4, //s2
	OFFSET_K5,
	OFFSET_K6
};


template <typename T> inline
void ApplyRadialDistortionCameraIntrinsics(
	const T &focal_length_x,	const T &focal_length_y,
	const T &principal_point_x,	const T &principal_point_y,
	const T &k1,	const T &k2,	const T &k3,
	const T &p1,	const T &p2,
	const T &normalized_x,	const T &normalized_y,
	T *image_x,	T *image_y) {
	T x = normalized_x;
	T y = normalized_y;

	T r2 = x*x + y*y;
	T r4 = r2 * r2;
	T r6 = r4 * r2;
	T r_coeff = (T(1) + k1*r2 + k2*r4 + k2*r6);
	T xd = x * r_coeff + T(2)*p1*x*y + p2*(r2 + T(2)*x*x);
	T yd = y * r_coeff + T(2)*p2*x*y + p1*(r2 + T(2)*y*y);

	*image_x = focal_length_x * xd + principal_point_x;
	*image_y = focal_length_y * yd + principal_point_y;
}

template <typename T> inline
void ApplyRadialDistortionCameraIntrinsicsHighDist(
	const T &focal_length_x,	const T &focal_length_y,
	const T &principal_point_x,	const T &principal_point_y,
	const T &skew,
	const T &k1,	const T &k2,	const T &k3,	const T &k4,
	const T &p1,	const T &p2,	const T &p3,	const T &p4,
	const T &normalized_x,	const T &normalized_y,
	T *image_x,	T *image_y) {


	T x = normalized_x;
	T y = normalized_y;

	T r2 = x*x + y*y;
	T r4 = r2 * r2;
	T r6 = r4 * r2;
	T r8 = r6 * r2;
	T r_coeff = (T(1) + k1*r2 + k2*r4 + k3*r6 + k4*r8);

	T xd = x * r_coeff + (T(2)*p2*x*y + p1*(r2 + T(2)*x*x))*(T(1) + p3*r2 + p4*r4); //Photoscan P1 & P2 geswitched
	T yd = y * r_coeff + (T(2)*p1*x*y + p2*(r2 + T(2)*y*y))*(T(1) + p3*r2 + p4*r4);

	*image_x = focal_length_x * xd + principal_point_x + yd * skew; //volgens "Agisoft Lens User Manual"
	*image_y = focal_length_y * yd + principal_point_y;
}

template <typename T> inline
void ApplyRadialDistortionOpenCVAdvancedDist(
	const T &focal_length_x,const T &focal_length_y,
	const T &principal_point_x,	const T &principal_point_y,
	const T &skew,const T &k1,const T &k2,const T &k3,const T &k4,const T &k5,const T &k6,
	const T &p1,const T &p2,
	const T &s1,const T &s2,
	const T &normalized_x,const T &normalized_y,
	T *image_x,	T *image_y) {


	T x = normalized_x;
	T y = normalized_y;

	T r2 = x*x + y*y;
	T r4 = r2 * r2;
	T r6 = r4 * r2;
	T r8 = r6 * r2;

	T r_coeff = (T(1) + k1*r2 + k2*r4 + k3*r6) / (T(1) + k4*r2 + k5*r4 + k6*r6);

	T xd = x * r_coeff + (T(2)*p1*x*y + p2*(r2 + T(2)*x*x)) + s1*r2 + s2*r4;
	T yd = y * r_coeff + (T(2)*p2*x*y + p1*(r2 + T(2)*y*y)) + s1*r2 + s2*r4;

	*image_x = focal_length_x * xd + principal_point_x + yd * skew;
	*image_y = focal_length_y * yd + principal_point_y;
}


struct __declspec(dllexport) ReprojectionErrorSingleCamera {
		ReprojectionErrorSingleCamera(const double observed_x, const double observed_y);
		template <typename T>
		__declspec(dllexport) bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const;

		static ceres::CostFunction* Create(double obsx, double obsy);

		template <typename T>
		static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) {
			ReprojectionErrorSingleCamera func(obsx, obsy);
			func(intrinsics, R_t, X, residuals);
		}

		const double observed_x;
		const double observed_y;
	};
struct __declspec(dllexport) ReprojectionErrorSysteemCamera {
	ReprojectionErrorSysteemCamera(const double observed_x, const double observed_y);

	template <typename T>
	__declspec(dllexport) bool __cdecl operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const;

	static ceres::CostFunction* Create(double obsx, double obsy);

	template <typename T>
	static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t_systeem, const T* const R_t, const T* const X, T* residuals) {
		ReprojectionErrorSysteemCamera func(obsx, obsy);
		func(intrinsics, R_t_systeem, R_t, X, residuals);
	}

	const double observed_x;
	const double observed_y;
};

struct __declspec(dllexport) ReprojectionErrorSingleCameraHighDist {
	ReprojectionErrorSingleCameraHighDist(const double observed_x, const double observed_y);

	template <typename T>
	__declspec(dllexport) bool __cdecl operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const;

	static ceres::CostFunction* Create(double obsx, double obsy);

	template <typename T>
	static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) {
		ReprojectionErrorSingleCameraHighDist func(obsx, obsy);
		func(intrinsics, R_t, X, residuals);
	}

	const double observed_x;
	const double observed_y;
};
struct __declspec(dllexport) ReprojectionErrorSysteemCameraHighDist {
	ReprojectionErrorSysteemCameraHighDist(const double observed_x, const double observed_y);

	template <typename T>
	__declspec(dllexport) bool __cdecl operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const;

	static ceres::CostFunction* Create(double obsx, double obsy);

	template <typename T>
	static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t_systeem, const T* const R_t, const T* const X, T* residuals) {
		ReprojectionErrorSysteemCameraHighDist func(obsx, obsy);
		func(intrinsics, R_t_systeem, R_t, X, residuals);
	}

	const double observed_x;
	const double observed_y;
};

struct __declspec(dllexport) ReprojectionErrorSingleCameraOpenCVAdvancedDist {
	ReprojectionErrorSingleCameraOpenCVAdvancedDist(const double observed_x, const double observed_y);
	template <typename T>
	__declspec(dllexport) bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const;


	static ceres::CostFunction* Create(double obsx, double obsy);

	template <typename T>
	static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) {
		ReprojectionErrorSingleCameraOpenCVAdvancedDist func(obsx, obsy);
		func(intrinsics, R_t, X, residuals);
	}

	const double observed_x;
	const double observed_y;
};
struct __declspec(dllexport) ReprojectionErrorSysteemCameraOpenCVAdvancedDist {
	ReprojectionErrorSysteemCameraOpenCVAdvancedDist(const double observed_x, const double observed_y);

	template <typename T>
	__declspec(dllexport) bool __cdecl operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const;

	static ceres::CostFunction* Create(double obsx, double obsy);

	template <typename T>
	static void Reproject(double obsx, double obsy, const T* const intrinsics, const T* const R_t_systeem, const T* const R_t, const T* const X, T* residuals) {
		ReprojectionErrorSysteemCameraOpenCVAdvancedDist func(obsx, obsy);
		func(intrinsics, R_t_systeem, R_t, X, residuals);
	}

	const double observed_x;
	const double observed_y;
};

struct __declspec(dllexport) GCPError {
	GCPError(const double obsx, const double obsy, const double obsz);

	template <typename T>
	__declspec(dllexport) bool __cdecl operator()(const T* const X, const T* const transformation, T* residuals3D) const;

	static ceres::CostFunction* Create(double obsx, double obsy, double obsz);

	const double observed_x, observed_y, observed_z; //GPSpunt in lambert
};