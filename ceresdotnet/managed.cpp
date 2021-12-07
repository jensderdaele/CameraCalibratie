#pragma once
#include "stdafx.h"
#include "managed.h"

void ceresdotnet::CeresCameraMultiCollectionBundler::bundleCollections(Iteration^ callback) {
	ceres::Problem::Options problem_options;
	problem_options.cost_function_ownership = ceres::Ownership::DO_NOT_TAKE_OWNERSHIP;
	problem_options.enable_fast_removal = true;
	ceres::Problem problem(problem_options);
	ceres::Solver::Options options;

	//algemene opties voor de solver
	options.dense_linear_algebra_library_type = ceres::DenseLinearAlgebraLibraryType::LAPACK;
	options.linear_solver_type = ceres::LinearSolverType::SPARSE_SCHUR;
	options.sparse_linear_algebra_library_type = ceres::SparseLinearAlgebraLibraryType::SUITE_SPARSE;
	options.trust_region_strategy_type = ceres::TrustRegionStrategyType::LEVENBERG_MARQUARDT;
	options.num_linear_solver_threads = 8;
	options.num_threads = 8;
	options.minimizer_progress_to_stdout = true;

	//opties voor solver_termination
	options.max_num_iterations = 70;
	options.function_tolerance = 0.000005; // = |cost_change| / cost

	options.update_state_every_iteration = true; 
	setCallbackToProblem(callback, &options,this); //na elke iteratie callback naar C# code

	ceres::Solver::Summary summary;
	ceres::Solve(options, &problem, &summary);
	std::cout << "Final report:\n" << summary.FullReport();
};
