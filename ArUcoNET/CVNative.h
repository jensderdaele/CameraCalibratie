#pragma once
#include "stdafx.h"
// Eigen
#include <Eigen/Core>

// OpenCV
#include <opencv2/core/eigen.hpp>
#include <opencv2/sfm/triangulation.hpp>
#include <opencv2/sfm/projection.hpp>

using namespace cv;


namespace native {
	/** @brief Triangulates the a 3d position between two 2d correspondences, using the DLT.
	@param xl Input vector with first 2d point.
	@param xr Input vector with second 2d point.
	@param Pl Input 3x4 first projection matrix.
	@param Pr Input 3x4 second projection matrix.
	@param objectPoint Output vector with computed 3d point.
	Reference: @cite HartleyZ00 12.2 pag.312
	*/
	void
		triangulateDLT(const Vec2d &xl, const Vec2d &xr,
			const Matx34d &Pl, const Matx34d &Pr,
			Vec3d &point3d) {
		Matx44d design;
		for (int i = 0; i < 4; ++i)
		{
			design(0, i) = xl(0) * Pl(2, i) - Pl(0, i);
			design(1, i) = xl(1) * Pl(2, i) - Pl(1, i);
			design(2, i) = xr(0) * Pr(2, i) - Pr(0, i);
			design(3, i) = xr(1) * Pr(2, i) - Pr(1, i);
		}

		Vec4d XHomogeneous;
		cv::SVD::solveZ(design, XHomogeneous);

		cv::sfm::homogeneousToEuclidean(XHomogeneous, point3d);
	}

	/** @brief Triangulates the 3d position of 2d correspondences between n images, using the DLT
	* @param x Input vectors of 2d points (the inner vector is per image). Has to be 2xN
	* @param Ps Input vector with 3x4 projections matrices of each image.
	* @param X Output vector with computed 3d point.
	* Reference: it is the standard DLT; for derivation see appendix of Keir's thesis
	*/
	void
		triangulateNViews(const Mat_<double> &x, const std::vector<Matx34d> &Ps, Vec3d &X) {
		CV_Assert(x.rows == 2);
		unsigned nviews = x.cols;
		CV_Assert(nviews == Ps.size());

		cv::Mat_<double> design = cv::Mat_<double>::zeros(3 * nviews, 4 + nviews);
		for (unsigned i = 0; i < nviews; ++i) {
			for (char jj = 0; jj < 3; ++jj)
				for (char ii = 0; ii < 4; ++ii)
					design(3 * i + jj, ii) = -Ps[i](jj, ii);
			design(3 * i + 0, 4 + i) = x(0, i);
			design(3 * i + 1, 4 + i) = x(1, i);
			design(3 * i + 2, 4 + i) = 1.0;
		}

		Mat X_and_alphas;
		cv::SVD::solveZ(design, X_and_alphas);
		cv::sfm::homogeneousToEuclidean(X_and_alphas.rowRange(0, 4), X);
	};
	void
		triangulateNViews(array<System::Drawing::PointF>^ x, array<Emgu::CV::Matrix<double>^>^Ps, Emgu::CV::Matrix<double>^ X) {
		//CV_Assert(x.rows == 2);
		unsigned nviews = x->Length;
		//CV_Assert(nviews == Ps.size());
		cv::Mat_<double> design = cv::Mat_<double>::zeros(3 * nviews, 4 + nviews);
		for (unsigned i = 0; i < nviews; ++i) {
			for (char jj = 0; jj < 3; ++jj)
				for (char ii = 0; ii < 4; ++ii)
					design(3 * i + jj, ii) = -Ps[i][jj, ii];
			design(3 * i + 0, 4 + i) = x[i].X;
			design(3 * i + 1, 4 + i) = x[i].Y;
			design(3 * i + 2, 4 + i) = 1.0;
		}

		Mat X_and_alphas;
		cv::SVD::solveZ(design, X_and_alphas);
		auto outparr = *(cv::debug_build_guard::_OutputArray*)(X->GetOutputArray()->Ptr.ToPointer());
		cv::sfm::homogeneousToEuclidean(X_and_alphas.rowRange(0, 4), outparr);
	};
}
namespace CVNative {
	static public ref class CVNative abstract sealed {
	public:
		static void triangulateNViews(array<System::Drawing::PointF>^ pts, array<Emgu::CV::Matrix<double>^>^ Ps, Emgu::CV::Matrix<double>^ triangulated) {
			native::triangulateNViews(pts, Ps, triangulated);
		}
	};
}

