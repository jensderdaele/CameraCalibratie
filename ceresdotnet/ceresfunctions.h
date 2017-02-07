#pragma once


#include "ceres/ceres.h"
#include "ceres/rotation.h"
#include "glog/logging.h"
#include <Eigen/StdVector>


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

template <typename T>
inline void CombineRT(const T* R_ts, const T* R_tc, T* result){

	T rots[9];
	T rotc[9];


	ceres::AngleAxisToRotationMatrix(R_ts, rots);
	ceres::AngleAxisToRotationMatrix(R_tc, rotc);

	//Eigen::Matrix<T, 3, 3, Eigen::RowMajor> Rs = Eigen::Map < const Eigen::Matrix< T, 3, 3, Eigen::RowMajor> >(rots);
	//Eigen::Matrix<T, 3, 3, Eigen::RowMajor> Rc = Eigen::Map < const Eigen::Matrix< T, 3, 3, Eigen::RowMajor> >(rotc);
	


		
	Eigen::Matrix<T, 3, 1> ts(R_ts[3], R_ts[4], R_ts[5]);
	Eigen::Matrix<T, 3, 1> tc(R_tc[3], R_tc[4], R_tc[5]);

	Eigen::Matrix<T, 3, 3> Rc2(rotc);
	
	
	Eigen::Matrix<T, 3, 3> Rs;
	Eigen::Matrix<T, 3, 3> Rc;

	for (int i = 0; i < 3; ++i)
	{
		for (int j = 0; j < 3; ++j)
		{
			
			Rs(i, j) = rots[3 * j + i];
			Rc(i, j) = rotc[3 * j + i];
		}
	}
	

	Eigen::Matrix<T, 3, 3> R;
	Eigen::Matrix<T, 3, 1> t;


	R = Rc*Rs;
	t = tc + Rc*ts;

	
	result[3] = T(t.x());
	result[4] = T(t.y());
	result[5] = T(t.z());

	T RR[9];
	
	for (int i = 0; i < 3; ++i)
	{
		for (int j = 0; j < 3; ++j)
		{
			RR[3 * j + i] = R(i, j);
		}
	}


	ceres::RotationMatrixToAngleAxis(RR, result);

	
	
}



struct OpenCVReprojectionError {
	OpenCVReprojectionError(const double observed_x, const double observed_y)
		: observed_x(observed_x), observed_y(observed_y) {}

	template <typename T>
	bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {

		const T& focal_length = intrinsics[OFFSET_FOCAL_LENGTH_X];
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

		ApplyRadialDistortionCameraIntrinsics(focal_length, focal_length, principal_point_x, principal_point_y,
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


struct ReprojectionError {

	ReprojectionError(const double observed_x, const double observed_y)
		: observed_x(observed_x), observed_y(observed_y) {};
	template <typename T>
	bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {

		const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
		const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
		const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
		const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
		const T& k1 = intrinsics[OFFSET_K1];
		const T& k2 = intrinsics[OFFSET_K2];
		const T& k3 = intrinsics[OFFSET_K3];
		const T& p1 = intrinsics[OFFSET_P1];
		const T& p2 = intrinsics[OFFSET_P2];

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

struct ReprojectionErrorSingleCamera {

	ReprojectionErrorSingleCamera(const double observed_x, const double observed_y)
		: observed_x(observed_x), observed_y(observed_y) {};
	template <typename T>
	bool operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {

		const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
		const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
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
};

struct ReprojectionErrorSysteemCamera {

	ReprojectionErrorSysteemCamera(const double observed_x, const double observed_y)
		: observed_x(observed_x), observed_y(observed_y) {};
	template <typename T>
	bool operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const {
		
		const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
		const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
		const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
		const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
		const T& k1 = intrinsics[OFFSET_K1];
		const T& k2 = intrinsics[OFFSET_K2];
		const T& k3 = intrinsics[OFFSET_K3];
		const T& p1 = intrinsics[OFFSET_P1];
		const T& p2 = intrinsics[OFFSET_P2];

		/*
		T R_t[6];
		CombineRT(R_t_systeem, R_t_camera, R_t);
		T x2[3];
		ceres::AngleAxisRotatePoint(R_t, X, x2);
		x2[0] += R_t[3];
		x2[1] += R_t[4];
		x2[2] += R_t[5];*/

		T x[3];

		ceres::AngleAxisRotatePoint(R_t_systeem, X, x);
		x[0] += R_t_systeem[3];
		x[1] += R_t_systeem[4];
		x[2] += R_t_systeem[5];

		ceres::AngleAxisRotatePoint(R_t_camera, x, x);
		x[0] += R_t_camera[3];
		x[1] += R_t_camera[4];
		x[2] += R_t_camera[5];


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
};
