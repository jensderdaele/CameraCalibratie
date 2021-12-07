#pragma once


#include "ParameterBlocks.h"
#include "managed.h"

//#include <boost\unordered_map.hpp>
#define RIGCAMERA Tuple<ICeresPointOrient^, ICeresPointOrient^, ICeresIntrinsics^>
#define OBSLIST List<ICeresMarker^>
//using namespace Calibratie::Util;

namespace ceresdotnet {



	public ref class MultiCameraBundler {
	public:
		delegate void BundlerEvent(MultiCameraBundler^ sender);

	private:
		Iteration^ _pIteration;
		BundlerEvent^ _pBundlerStart;
		BundlerEvent^ _pBundlerTerminate;

	internal:
		ceres::Problem* _problem;

		Dictionary<ICeresParameterblock^, CeresParameterBlock^>^ _map;
		Dictionary<RIGCAMERA^, OBSLIST^>^ _observations;
		ICeresScaleTransform^ _coordinateTransform;

		HashSet<ICeresCamera^>^ StandaloneCameraList = gcnew HashSet<ICeresCamera^>();
		HashSet<ICeresGCP^>^ GCPs = gcnew HashSet<ICeresGCP^>();


		bool _buildContinuesly;
		CeresParameterBlock^ _getParamblock(ICeresParameterblock^ block) {
			CeresParameterBlock^ r;
			if (_map->TryGetValue(block, r)) {
				return r;
			}
			return nullptr;
		}

	public:
		property bool BuildContinuesly {bool get() {
			return _buildContinuesly;
		}
		void set(bool v) { _buildContinuesly = v; }
		}

		String^ IntrinsicsSummary(ICeresIntrinsics^ intr) {
			System::String^ camerastr = "";
			CeresIntrinsics ceresintr;
			if (!GetNativeParamblock(intr,(CeresIntrinsics^%)% ceresintr));
			BundleIntrinsicsFlags flags = ceresintr.BundleFlags;


			camerastr += flags.HasFlag(BundleIntrinsicsFlags::FocalLength) ? " fx: " + ceresintr.fx + " fy: " + ceresintr.fy : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::PrincipalP) ? " cx: " + ceresintr.ppx + " cy: " + ceresintr.ppy : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::SKEW) ? " skew: " + ceresintr.skew : "";

			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R1) ? " k1: " + ceresintr.k1 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R2) ? " k2: " + ceresintr.k2 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R3) ? " k3: " + ceresintr.k3 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R4) ? " k4: " + ceresintr.k4 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R5) ? " k5: " + ceresintr.k5 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::R6) ? " k6: " + ceresintr.k6 : "";

			camerastr += flags.HasFlag(BundleIntrinsicsFlags::P1) ? " p1: " + ceresintr.p1 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::P2) ? " p2: " + ceresintr.p2 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::P3) ? " p3: " + ceresintr.p3 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::P4) ? " p4: " + ceresintr.p4 : "";

			camerastr += flags.HasFlag(BundleIntrinsicsFlags::S1) ? " s1: " + ceresintr.s1 : "";
			camerastr += flags.HasFlag(BundleIntrinsicsFlags::S2) ? " s2: " + ceresintr.s2 : "";

			return camerastr;

		}

		MultiCameraBundler() {
			_map = gcnew Dictionary<ICeresParameterblock^, CeresParameterBlock^>();
			_observations = gcnew Dictionary<RIGCAMERA^, OBSLIST^>();
			ceres::Problem::Options opts;
			opts.cost_function_ownership = ceres::TAKE_OWNERSHIP;
			opts.loss_function_ownership = ceres::TAKE_OWNERSHIP;
			opts.local_parameterization_ownership = ceres::TAKE_OWNERSHIP;
			opts.enable_fast_removal = false;
			
			_problem = new ceres::Problem(opts);
			/*
			_map_CeresPointOrient = gcnew BiDictionary<ICeresPointOrient^, CeresPointOrient^>();
			_map_CeresIntrinsics = gcnew BiDictionary<ICeresIntrinsics^, CeresIntrinsics^>();
			_map_CeresPoint = gcnew BiDictionary<ICeresPoint^, CeresPoint^>();*/
		};

		bool AddCamera(ICeresPointOrient^ ext, ICeresIntrinsics^ intr) {
			auto r = false;
			if (!_map->ContainsKey((ICeresParameterblock^)ext)) {
				_map->Add(ext, gcnew CeresPointOrient(ext));
				r = true;
			}

			if (!_map->ContainsKey(intr)) {
				_map->Add(intr, gcnew CeresIntrinsics(intr));
				r = true;
			}
			if (r) {
				StandaloneCameraList->Add(gcnew CeresCamera(intr, ext));
				_observations->Add(gcnew RIGCAMERA(nullptr, ext, intr), gcnew OBSLIST());
			}
			return r;
		};
		bool AddCamera(ICeresCamera^ cam) {
			auto r = false;
			if (!_map->ContainsKey((ICeresParameterblock^)cam->External)) {
				_map->Add(cam->External, gcnew CeresPointOrient(cam->External));
				r = true;
			}

			if (!_map->ContainsKey(cam->Internal)) {
				_map->Add(cam->Internal, gcnew CeresIntrinsics(cam->Internal));
				r = true;
			}
			if (r) {
				StandaloneCameraList->Add(cam);
				_observations->Add(gcnew RIGCAMERA(nullptr, cam->External, cam->Internal), gcnew OBSLIST());
			}
			return r;
		};
		bool AddParameterBlock(ICeresPointOrient^ block) {
			if (_map->ContainsKey(block)) {
				return false;
			} else {
				auto nat = gcnew CeresPointOrient(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddParameterBlock(ICeresPointOrient^ block, [Out] CeresPointOrient^% nat) {
			if (_map->ContainsKey(block)) {
				nat = nullptr;//(CeresPointOrient^)_map->GetByFirst(block);
				return false;
			} else {
				nat = gcnew CeresPointOrient(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddParameterBlock(ICeresPoint^ block) {
			if (_map->ContainsKey(block)) {
				return false;
			} else {
				auto nat = gcnew CeresPoint(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddParameterBlock(ICeresPoint^ block, [Out] CeresPoint^% nat) {
			if (_map->ContainsKey(block)) {
				nat = nullptr;//(CeresPointOrient^)_map->GetByFirst(block);
				return false;
			} else {
				nat = gcnew CeresPoint(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddParameterBlock(ICeresScaleTransform^ block) {
			if (_map->ContainsKey(block)) {
				return false;
			} else {
				auto nat = gcnew CeresScaledTransformation(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddParameterBlock(ICeresScaleTransform^ block, [Out] CeresScaledTransformation^% nat) {
			if (_map->ContainsKey(block)) {
				nat = nullptr;//(CeresScaledTransformation^)_map->GetByFirst(block);
				return false;
			} else {
				nat = gcnew CeresScaledTransformation(block);
				_map->Add(block, nat);
				return true;
			}
		}
		bool AddGCPObservation(ICeresPointOrient^ ext, ICeresIntrinsics^ intr, ICeresGCP^ GCP, double image_x, double image_y) {
			throw gcnew NotImplementedException();
		}
		bool AddGCP(ICeresGCP^ GCP) {
			AddParameterBlock(GCP->Transformation);
			AddParameterBlock(GCP->Triangulated);
			
			return GCPs->Add(GCP);
		}

		int GetResidualBlockCountForParameterBlock(ICeresParameterblock^ block)
		{
			std::vector<ceres::ResidualBlockId> v;
			this->_problem->GetResidualBlocksForParameterBlock(_map[block]->_data, &v);
			return v.size();
		}
		void GetLossFunctionForResidualBlock()
		{

		}

		OBSLIST^ GetObservationList(ICeresPointOrient^ ext, ICeresIntrinsics^ intr) {
			RIGCAMERA^ key = gcnew RIGCAMERA(nullptr, ext, intr);
			if (_observations->ContainsKey(key)) {
				return _observations[key];
			}
			return nullptr;
		}
		OBSLIST^ GetObservationList(ICeresCamera^ cam) {
			return GetObservationList(cam->External, cam->Internal);
		}
		bool GetNativeParamblock(ICeresParameterblock^ managed, [Out] CeresParameterBlock^% nat) {
			nat = _getParamblock(managed);
			if (nat == nullptr)
				return false;
			return true;
		}
		bool GetNativeParamblock(ICeresIntrinsics^ managed, [Out] CeresIntrinsics^% nat) {
			return GetNativeParamblock((ICeresParameterblock^)managed, (CeresParameterBlock^%)nat);
		}
		bool GetNativeParamblock(ICeresPointOrient^ managed, [Out] CeresPointOrient^% nat) {
			return GetNativeParamblock((ICeresParameterblock^)managed, (CeresParameterBlock^%)nat);
		}
		bool GetNativeParamblock(ICeresPoint^ managed, [Out] CeresPoint^% nat) {
			return GetNativeParamblock((ICeresParameterblock^)managed, (CeresParameterBlock^%)nat);
		}

		bool IsParameterBlockConstant(ICeresParameterblock^ managed)
		{
			CeresParameterBlock^ n;
			if (GetNativeParamblock(managed, n)) {
				return _problem->IsParameterBlockConstant(n->_data);
			}
			else
				return false;
		}
		bool HasParametrization(ICeresParameterblock^ managed)
		{
			CeresParameterBlock^ n;
			if (GetNativeParamblock(managed, n)) {
				return _problem->GetParameterization(n->_data) != NULL;
			}
			else
				return false;
		}
		
		/// <summary>
		/// Evaluates entire problem
		///</summary>
		double GetTotalCost() {
			ceres::Problem::EvaluateOptions options;
			options.apply_loss_function = true;
			options.num_threads = 1;

			double cost;
			_problem->Evaluate(options, &cost, NULL, NULL, NULL);
			return cost;
		}
		double GetCostForParameterblock(ICeresParameterblock^ block, bool applyLossFunction) {
			return GetCostForNativeParameterblock(_getParamblock(block), applyLossFunction);
		}
		double GetCostForParameterblock(ICeresParameterblock^ block) {
			return GetCostForParameterblock(block,true);
		}
		/// <summary>
		/// Bad things will happen is the block is unknown
		///</summary>
		double GetCostForNativeParameterblock(CeresParameterBlock^ block,bool applyLossFunction)
		{
			std::vector<double*> nativeblocks; //= new std::vector<double*>();
			//std::vector < ceres::ResidualBlockId> * residualblocks = new std::vector< ceres::ResidualBlockId>();
			
			std::vector < ceres::ResidualBlockId> residualblocks;


			auto blk = block;
			if (blk == nullptr) {
				return -1;
			}
			nativeblocks.push_back((double*)(blk->_data));

			ceres::Problem::EvaluateOptions options;
			_problem->GetResidualBlocksForParameterBlock(blk->_data, &residualblocks);
			options.parameter_blocks = nativeblocks;
			options.residual_blocks = residualblocks;
			options.apply_loss_function = applyLossFunction;
			options.num_threads = 1;

			double cost;
			_problem->Evaluate(options, &cost, NULL, NULL, NULL);

			//free(nativeblocks);
			return cost;
		}
		
		void UpdateManagedData() {
			for each (auto var in _map) {
				var.Value->UpdateManagedData();
			}
			/*for each (auto var in _map_CeresPointOrient) {
				var.Second->UpdateManagedData();
			}
			for each (auto var in _map_CeresIntrinsics) {
				var.Second->UpdateManagedData();
			}
			for each (auto var in _map_CeresPoint) {
				var.Second->UpdateManagedData();
			}*/
		}; 
		void UpdateNativeData() {
			for each (auto var in _map) {
				var.Value->UpdateBundleData();
			}
		};  
		void UpdateObservations() {
			for each (auto var in _observations) {
				for each (auto obs in var.Value) {
					if (!_map->ContainsKey(obs->Location)) {
						_map->Add(obs->Location, gcnew CeresPoint(obs->Location));
					}
				}
			}
		}

		void BuildProblem() {
			UpdateObservations();
			UpdateNativeData();

			//iteratie over de vrijstaande camera's
			for each (ICeresCamera^ camera in StandaloneCameraList) {
				auto extr = (CeresPointOrient^)_map[camera->External];
				auto intr = (CeresIntrinsics^)_map[camera->Internal];

				extr->AddToProblem(_problem);
				intr->AddToProblem(_problem);

				List<ICeresMarker^>^ obss = GetObservationList(camera);
				for each (ICeresMarker^ obs in obss) {
					auto observation = _map[obs->Location];
					observation->AddToProblem(_problem);

					ceres::LossFunction* lossFunction = NULL;
					//toevoegen van het residu
					if (obs->Location != nullptr) {
						_problem->AddResidualBlock(intr->CreateCostFunction(obs->X, obs->Y),
							lossFunction,
							intr->_data,
							extr->_data,
							observation->_data);
					}
				}
			}

			for each (ICeresGCP^ gcp in GCPs) {
				auto paramblockX = _getParamblock(gcp->Triangulated);
				auto paramblockTransf = _getParamblock(gcp->Transformation);
				_problem->AddResidualBlock(GCPError::Create(gcp->observed_x, gcp->observed_y, gcp->observed_z), NULL, paramblockX->_data, paramblockTransf->_data);
			}

		}

		void SolveProblem(SolverOptions^ options) {
			
			ceres::Solver::Summary summary;
			BundlerStart::raise(this);

			setCallbackToProblem(_pIteration, options->_options, this);

			ceres::Solve(*options->_options, _problem, &summary);
			BundlerTerminate::raise(this);
			std::cout << "Final report:\n" << summary.FullReport();
		}
		void SolveProblem() {
			auto opts = gcnew SolverOptions();
			ceres::Solver::Summary summary;
			ceres::Solve(*opts->_options, _problem, &summary);
			BundlerTerminate::raise(this);
			std::cout << "Final report:\n" << summary.FullReport();
		}

		void SetBundleFlags(ICeresParameterblock^ block, Enum^ flags) {
			_getParamblock(block)->BundleFlagsEnum = flags;
		}
		void SetBundleFlags(ICeresPointOrient^ block, BundlePointOrientFlags flags) {
			SetBundleFlags(block, (Enum^)flags);
		}
		void SetBundleFlags(ICeresIntrinsics^ block, BundleIntrinsicsFlags flags) {
			SetBundleFlags(block, (Enum^)flags);
		}
		void SetBundleFlags(ICeresPoint^ block, BundleWorldCoordinatesFlags flags) {
			SetBundleFlags(block, flags);
		}

		void SetBlockConstant(ICeresParameterblock^ block) {
			auto nativeblock = _getParamblock(block);
			double* values = nativeblock->_data;
			if (!_problem->HasParameterBlock(values)) {
				_problem->AddParameterBlock(values, nativeblock->Length);
			}
			_problem->SetParameterBlockConstant(values);
		}
		void SetBlockVariable(ICeresParameterblock^ block) {
			auto nativeblock = _getParamblock(block);
			double* values = nativeblock->_data;
			if (!_problem->HasParameterBlock(values)) {
				_problem->AddParameterBlock(values, nativeblock->Length);
			}
			_problem->SetParameterBlockVariable(values);
		}

		void SetBlockTypeVariable(Type^ t) {
			if (!t->IsAssignableFrom(ICeresParameterblock::typeid)) {
				return;
			}
			for each (auto var in _map) {
				if (var.Key->GetType()->IsAssignableFrom(t)) {
					_problem->SetParameterBlockVariable(var.Value->_data);
				}
			}
		}
		void SetBlockTypeConstant(Type^ t) {
			if (!t->IsAssignableFrom(ICeresParameterblock::typeid)) {
				return;
			}
			for each (auto var in _map) {
				if (var.Key->GetType()->IsAssignableFrom(t)) {
					_problem->SetParameterBlockConstant(var.Value->_data);
				}
			}
		}

		property List<CeresParameterBlock^>^ NativeBlocks{
			List<CeresParameterBlock^>^ get() {
				auto r = gcnew List<CeresParameterBlock^>();
				for each (auto var in _map) {
					r->Add(var.Value);
				}
				return r;
			}
		}

		event Iteration^ Iteration{
			void add(ceresdotnet::Iteration^ p)
			{
				_pIteration = static_cast<ceresdotnet::Iteration^> (Delegate::Combine(_pIteration, p));
			}
			void remove(ceresdotnet::Iteration^ p) {
				_pIteration = static_cast<ceresdotnet::Iteration^> (Delegate::Remove(_pIteration, p));
			}
			CeresCallbackReturnType raise(Object^ sender, IterationSummary^ summary) {
				if (_pIteration != nullptr)
					return _pIteration->Invoke(sender,summary);
				return CeresCallbackReturnType::SOLVER_CONTINUE;
			}
		}
		event BundlerEvent^ BundlerStart {
			void add(BundlerEvent^ p)
			{
				_pBundlerStart = static_cast<BundlerEvent^> (Delegate::Combine(_pBundlerStart, p));
			}
			void remove(BundlerEvent^ p) {
				_pBundlerStart = static_cast<BundlerEvent^> (Delegate::Remove(_pBundlerStart, p));
			}
			void raise(MultiCameraBundler^ sender) {
				if (_pBundlerStart != nullptr)
					_pBundlerStart->Invoke(sender);
			}
		}
		event BundlerEvent^ BundlerTerminate {
			void add(BundlerEvent^ p)
			{
				_pBundlerTerminate = static_cast<BundlerEvent^> (Delegate::Combine(_pBundlerTerminate, p));
			}
			void remove(BundlerEvent^ p) {
				_pBundlerTerminate = static_cast<BundlerEvent^> (Delegate::Remove(_pBundlerTerminate, p));
			}
			void raise(MultiCameraBundler^ sender) {
				if (_pBundlerTerminate != nullptr)
					_pBundlerTerminate->Invoke(sender);
			}
		}
	};
};