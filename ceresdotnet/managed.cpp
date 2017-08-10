#pragma once
#include "stdafx.h"
#include "managed.h"

void ceresdotnet::CeresCameraMultiCollectionBundler::bundleCollections(Iteration^ callback) {
	ceres::Problem::Options problem_options;
	ceres::Problem problem(problem_options);

	//iteratie over de vrijstaande camera's
	for each (CeresCamera^ camera in StandaloneCameraList) {
		List<CeresMarker^>^ obss = MarkersFromCamera(camera, nullptr);//CeresMarker bevat 2D-3D correspondentie
		for each (CeresMarker^ obs in obss) {
			//parameterblock toevoegen aan het probleem
			camera->External->AddToProblem(&problem);
			camera->Internal->AddToProblem(&problem);
			obs->Location->AddToProblem(&problem);
			ceres::LossFunction* lossFunction = NULL; //new ceres::CauchyLoss(1.0);
			//Bij oneffen verdeling van 2D punten over de sensor
			if (InfluenceMaps != nullptr) {
				auto map = InfluenceMaps[camera->Internal];
				lossFunction = new ceres::ScaledLoss(lossFunction, map[obs->y / ScaledownMaps, obs->x / ScaledownMaps], ceres::Ownership::TAKE_OWNERSHIP);
			}
			//toevoegen van het residu
			if (obs->Location != nullptr) {
				problem.AddResidualBlock(camera->Internal->CreateCostFunction(obs->x, obs->y),
					lossFunction,
					camera->Internal->_data,
					camera->External->_data,
					obs->Location->_data);
			} 
		}
	}

	//iteratie over de verschillende multicamera's
	for each (CeresCameraCollection^ Collection in CollectionList) {
		//iteratie over elke camera in de multicamera setup
		for each (CeresCamera^ camera in Collection->Cameras) {
			auto obss = MarkersFromCamera(camera, Collection); //set 2D-3D correspondenties
			for each (CeresMarker^ obs in obss) { //iteratie over elke 2D-3D correspondentie
				obs->Location->AddToProblem(&problem);
				camera->Internal->AddToProblem(&problem);
				Collection->Position->AddToProblem(&problem);
				camera->External->AddToProblem(&problem);

				ceres::LossFunction* lossFunction = NULL;// new ceres::CauchyLoss(1.0);
				if (InfluenceMaps != nullptr) {
					auto map = InfluenceMaps[camera->Internal];
					lossFunction = new ceres::ScaledLoss(lossFunction, 
														 map[obs->x / ScaledownMaps, obs->y / ScaledownMaps], 
													     ceres::Ownership::TAKE_OWNERSHIP);
				}

				if (obs->Location != nullptr) {
					auto blockid = problem.AddResidualBlock(camera->Internal->CreateCostFunctionSystemCamera(obs->x, obs->y),
						lossFunction,
						camera->Internal->_data,
						camera->External->_data,
						Collection->Position->_data,
						obs->Location->_data);
				}
			}
		}

	}

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

#pragma message( "5" )
