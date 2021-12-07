#pragma once
#include <ceres\ceres.h>
namespace ceresdotnet {
	public enum class CeresCallbackReturnType : int {
		SOLVER_ABORT = ceres::SOLVER_ABORT,
		SOLVER_CONTINUE = ceres::SOLVER_CONTINUE,
		SOLVER_TERMINATE_SUCCESSFULLY = ceres::SOLVER_TERMINATE_SUCCESSFULLY
	};
	
	public enum class DenseLinearAlgebraLibraryType :int {
		EIGEN = ceres::DenseLinearAlgebraLibraryType::EIGEN,
		LAPACK = ceres::DenseLinearAlgebraLibraryType::LAPACK
	};
	
	public enum class DoglegType : int {
		SUBSPACE_DOGLEG = ceres::DoglegType::SUBSPACE_DOGLEG,
		TRADITIONAL_DOGLEG = ceres::DoglegType::TRADITIONAL_DOGLEG
	};
	
	public enum class LinearSolverType : int {
		CGNR = ceres::LinearSolverType::CGNR,
		DENSE_NORMAL_CHOLESKY = ceres::LinearSolverType::DENSE_NORMAL_CHOLESKY,
		DENSE_QR = ceres::LinearSolverType::DENSE_QR,
		DENSE_SCHUR = ceres::LinearSolverType::DENSE_SCHUR,
		ITERATIVE_SCHUR = ceres::LinearSolverType::ITERATIVE_SCHUR,
		SPARSE_NORMAL_CHOLESKY = ceres::LinearSolverType::SPARSE_NORMAL_CHOLESKY,
		SPARSE_SCHUR = ceres::LinearSolverType::SPARSE_SCHUR
	};
	
	public enum class LineSearchDirectionType : int {
		BFGS = ceres::LineSearchDirectionType::BFGS,
		LBFGS = ceres::LineSearchDirectionType::LBFGS,
		NONLINEAR_CONJUGATE_GRADIENT = ceres::LineSearchDirectionType::NONLINEAR_CONJUGATE_GRADIENT,
		STEEPEST_DESCENT = ceres::LineSearchDirectionType::STEEPEST_DESCENT,
	};
	
	public enum class LineSearchType : int {
		ARMIJO = ceres::LineSearchType::ARMIJO,
		WOLFE = ceres::LineSearchType::WOLFE,
	};

	public enum class LoggingType : int {
		PER_MINIMIZER_ITERATION = ceres::LoggingType::PER_MINIMIZER_ITERATION,
		SILENT = ceres::LoggingType::SILENT,
	};

	public enum class MinimizerType : int {
		LINE_SEARCH = ceres::MinimizerType::LINE_SEARCH,
		TRUST_REGION = ceres::MinimizerType::TRUST_REGION,
	};

	public enum class NonlinearConjugateGradientType : int {
		FLETCHER_REEVES = ceres::NonlinearConjugateGradientType::FLETCHER_REEVES,
		HESTENES_STIEFEL = ceres::NonlinearConjugateGradientType::HESTENES_STIEFEL,
		POLAK_RIBIERE = ceres::NonlinearConjugateGradientType::POLAK_RIBIERE,
	};

	public enum class PreconditionerType : int {
		CLUSTER_JACOBI = ceres::PreconditionerType::CLUSTER_JACOBI,
		CLUSTER_TRIDIAGONAL = ceres::PreconditionerType::CLUSTER_TRIDIAGONAL,
		IDENTITY = ceres::PreconditionerType::IDENTITY,
		JACOBI = ceres::PreconditionerType::JACOBI,
		SCHUR_JACOBI = ceres::PreconditionerType::SCHUR_JACOBI
	};

	public enum class SparseLinearAlgebraLibraryType : int {
		CX_SPARSE = ceres::SparseLinearAlgebraLibraryType::CX_SPARSE,
		EIGEN_SPARSE = ceres::SparseLinearAlgebraLibraryType::EIGEN_SPARSE,
		NO_SPARSE = ceres::SparseLinearAlgebraLibraryType::NO_SPARSE,
		SUITE_SPARSE = ceres::SparseLinearAlgebraLibraryType::SUITE_SPARSE
	};

	public enum class DumpFormatType : int {
		CONSOLE = ceres::DumpFormatType::CONSOLE,
		TEXTFILE = ceres::DumpFormatType::TEXTFILE
	};

	public enum class TrustRegionStrategyType : int {
		DOGLEG = ceres::TrustRegionStrategyType::DOGLEG,
		LEVENBERG_MARQUARDT = ceres::TrustRegionStrategyType::LEVENBERG_MARQUARDT
	};

	public enum class VisibilityClusteringType : int {
		CANONICAL_VIEWS = ceres::VisibilityClusteringType::CANONICAL_VIEWS,
		SINGLE_LINKAGE = ceres::VisibilityClusteringType::SINGLE_LINKAGE
	};
}