#include "Header.h"


//STANDARD CAMERA
ReprojectionErrorSingleCamera::ReprojectionErrorSingleCamera(double observed_x, double observed_y)
		: observed_x(observed_x), observed_y(observed_y) {
	};
template <typename T>
bool ReprojectionErrorSingleCamera::operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {
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
	};
ceres::CostFunction* ReprojectionErrorSingleCamera::Create(const double obsx, const  double obsy) {
		return new ceres::AutoDiffCostFunction <ReprojectionErrorSingleCamera, 2, 9, 6, 3 >(new ReprojectionErrorSingleCamera(obsx, obsy));
	};

ReprojectionErrorSysteemCamera::ReprojectionErrorSysteemCamera(const double observed_x, const double observed_y)
	: observed_x(observed_x), observed_y(observed_y) {
};
template <typename T>
bool __cdecl ReprojectionErrorSysteemCamera::operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const {

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
};
ceres::CostFunction* ReprojectionErrorSysteemCamera::Create(double obsx, double obsy) {
	return new ceres::AutoDiffCostFunction <ReprojectionErrorSysteemCamera, 2, 9, 6, 6, 3 >(new ReprojectionErrorSysteemCamera(obsx, obsy));
};


//HIGH DIST (PHOTOSCAN)
ReprojectionErrorSingleCameraHighDist::ReprojectionErrorSingleCameraHighDist(const double observed_x, const double observed_y)
	: observed_x(observed_x), observed_y(observed_y) {
};
template <typename T>
bool __cdecl ReprojectionErrorSingleCameraHighDist::operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {

	const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
	const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
	const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
	const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
	const T& skew = intrinsics[OFFSET_SKEW];

	const T& k1 = intrinsics[OFFSET_K1];
	const T& k2 = intrinsics[OFFSET_K2];
	const T& k3 = intrinsics[OFFSET_K3];
	const T& k4 = intrinsics[OFFSET_K4];

	const T& p1 = intrinsics[OFFSET_P1];
	const T& p2 = intrinsics[OFFSET_P2];
	const T& p3 = intrinsics[OFFSET_P3];
	const T& p4 = intrinsics[OFFSET_P4];


	T x3[3];

	ceres::AngleAxisRotatePoint(R_t, X, x3);
	x3[0] += R_t[3];
	x3[1] += R_t[4];
	x3[2] += R_t[5];


	// Normaliseren
	T x = x3[0] / x3[2];
	T y = x3[1] / x3[2];
	T predicted_x, predicted_y;



	ApplyRadialDistortionCameraIntrinsicsHighDist(
		focal_length_x, focal_length_y,
		principal_point_x, principal_point_y,
		skew,
		k1, k2, k3, k4,
		p1, p2, p3, p4,
		x, y,
		&predicted_x,
		&predicted_y);

	residuals[0] = predicted_x - T(observed_x);
	residuals[1] = predicted_y - T(observed_y);
	return true;
};
ceres::CostFunction* ReprojectionErrorSingleCameraHighDist::Create(double obsx, double obsy) {
	return new ceres::AutoDiffCostFunction <ReprojectionErrorSingleCameraHighDist, 2, 13, 6, 3 >(new ReprojectionErrorSingleCameraHighDist(obsx, obsy));
};

ReprojectionErrorSysteemCameraHighDist::ReprojectionErrorSysteemCameraHighDist(const double observed_x, const double observed_y)
	: observed_x(observed_x), observed_y(observed_y) {
};
template <typename T>
bool __cdecl ReprojectionErrorSysteemCameraHighDist::operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const {

	const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
	const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
	const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
	const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
	const T& skew = intrinsics[OFFSET_SKEW];

	const T& k1 = intrinsics[OFFSET_K1];
	const T& k2 = intrinsics[OFFSET_K2];
	const T& k3 = intrinsics[OFFSET_K3];
	const T& k4 = intrinsics[OFFSET_K4];

	const T& p1 = intrinsics[OFFSET_P1];
	const T& p2 = intrinsics[OFFSET_P2];
	const T& p3 = intrinsics[OFFSET_P3];
	const T& p4 = intrinsics[OFFSET_P4];

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

	ApplyRadialDistortionCameraIntrinsicsHighDist(
		focal_length_x, focal_length_y,
		principal_point_x, principal_point_y,
		skew,
		k1, k2, k3, k4,
		p1, p2, p3, p4,
		xn, yn,
		&predicted_x,
		&predicted_y);

	residuals[0] = predicted_x - T(observed_x);
	residuals[1] = predicted_y - T(observed_y);
	return true;
};
ceres::CostFunction* ReprojectionErrorSysteemCameraHighDist::Create(double obsx, double obsy) {
	return new ceres::AutoDiffCostFunction <ReprojectionErrorSysteemCameraHighDist, 2, 13, 6, 6, 3 >(new ReprojectionErrorSysteemCameraHighDist(obsx, obsy));
};


//6 PARAM RADIAL + PRISM DISTORTION
ReprojectionErrorSingleCameraOpenCVAdvancedDist::ReprojectionErrorSingleCameraOpenCVAdvancedDist(const double observed_x, const double observed_y)
	: observed_x(observed_x), observed_y(observed_y) {
};
template <typename T>
bool ReprojectionErrorSingleCameraOpenCVAdvancedDist::operator()(const T* const intrinsics, const T* const R_t, const T* const X, T* residuals) const {
		const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
		const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
		const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
		const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
		const T& skew = intrinsics[OFFSET_SKEW];

		const T& k1 = intrinsics[OFFSET_K1];
		const T& k2 = intrinsics[OFFSET_K2];
		const T& k3 = intrinsics[OFFSET_K3];
		const T& k4 = intrinsics[OFFSET_K4];
		const T& k5 = intrinsics[OFFSET_K5];
		const T& k6 = intrinsics[OFFSET_K6];

		const T& p1 = intrinsics[OFFSET_P1];
		const T& p2 = intrinsics[OFFSET_P2];

		//s1&2 = p3p4
		const T& s1 = intrinsics[OFFSET_P3];
		const T& s2 = intrinsics[OFFSET_P4];

		T x[3];

		ceres::AngleAxisRotatePoint(R_t, X, x);
		x[0] += R_t[3];
		x[1] += R_t[4];
		x[2] += R_t[5];

		T xn = x[0] / x[2];
		T yn = x[1] / x[2];
		T predicted_x, predicted_y;

		ApplyRadialDistortionOpenCVAdvancedDist(
			focal_length_x, focal_length_y, 
			principal_point_x, principal_point_y, 
			skew, 
			k1, k2, k3, k4, k5, k6, 
			p1, p2, 
			s1, s2, 
			xn, yn,
			&predicted_x, &predicted_y);

		residuals[0] = predicted_x - T(observed_x);
		residuals[1] = predicted_y - T(observed_y);
		return true;
	};
ceres::CostFunction* ReprojectionErrorSingleCameraOpenCVAdvancedDist::Create(double obsx, double obsy) {
		return new ceres::AutoDiffCostFunction <ReprojectionErrorSingleCameraOpenCVAdvancedDist, 2, 15, 6, 3 >(new ReprojectionErrorSingleCameraOpenCVAdvancedDist(obsx, obsy));
	};

ReprojectionErrorSysteemCameraOpenCVAdvancedDist::ReprojectionErrorSysteemCameraOpenCVAdvancedDist(const double observed_x, const double observed_y)
	: observed_x(observed_x), observed_y(observed_y) {
};
template <typename T>
bool __cdecl ReprojectionErrorSysteemCameraOpenCVAdvancedDist::operator()(const T* const intrinsics, const T* const R_t_systeem, const T* const R_t_camera, const T* const X, T* residuals) const {
	const T& focal_length_x = intrinsics[OFFSET_FOCAL_LENGTH_X];
	const T& focal_length_y = intrinsics[OFFSET_FOCAL_LENGTH_Y];
	const T& principal_point_x = intrinsics[OFFSET_PRINCIPAL_POINT_X];
	const T& principal_point_y = intrinsics[OFFSET_PRINCIPAL_POINT_Y];
	const T& skew = intrinsics[OFFSET_SKEW];

	const T& k1 = intrinsics[OFFSET_K1];
	const T& k2 = intrinsics[OFFSET_K2];
	const T& k3 = intrinsics[OFFSET_K3];
	const T& k4 = intrinsics[OFFSET_K4];
	const T& k5 = intrinsics[OFFSET_K5];
	const T& k6 = intrinsics[OFFSET_K6];

	const T& p1 = intrinsics[OFFSET_P1];
	const T& p2 = intrinsics[OFFSET_P2];

	//s1&2 = p3p4
	const T& s1 = intrinsics[OFFSET_P3];
	const T& s2 = intrinsics[OFFSET_P4];

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

	ApplyRadialDistortionOpenCVAdvancedDist(
		focal_length_x, focal_length_y,
		principal_point_x, principal_point_y,
		skew,
		k1, k2, k3, k4, k5, k6,
		p1, p2,
		s1, s2,
		xn, yn,
		&predicted_x, &predicted_y);

	residuals[0] = predicted_x - T(observed_x);
	residuals[1] = predicted_y - T(observed_y);
	return true;
};
ceres::CostFunction* ReprojectionErrorSysteemCameraOpenCVAdvancedDist::Create(double obsx, double obsy) {
	return new ceres::AutoDiffCostFunction <ReprojectionErrorSysteemCameraOpenCVAdvancedDist, 2, 15, 6, 6, 3 >(new ReprojectionErrorSysteemCameraOpenCVAdvancedDist(obsx, obsy));
};


//GCP ERROR
GCPError::GCPError(const double observed_x, const double observed_y, const double observed_z)
 : observed_x(observed_x),observed_y(observed_y),observed_z(observed_z){
	
}
template <typename T>
bool __cdecl GCPError::operator()(const T* const X, const T* const transformation, T* residuals) const {
	T x[3];
	//transformation(7) = angleaxis(3) + translation(3) + scale(1)
	ceres::AngleAxisRotatePoint(transformation, X, x);
	x[0] += transformation[3];
	x[1] += transformation[4];
	x[2] += transformation[5];

	x[0] *= transformation[6];
	x[1] *= transformation[6];
	x[2] *= transformation[6];

	residuals[0] = x[0] - T(observed_x);
	residuals[1] = x[1] - T(observed_y);
	residuals[2] = x[2] - T(observed_z);
	return true;
};
ceres::CostFunction* GCPError::Create(double obsx, double obsy, double obsz) {
	return new ceres::AutoDiffCostFunction <GCPError, 3, 3, 7>(new GCPError(obsx, obsy, obsz));
}