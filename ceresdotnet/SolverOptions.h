#pragma once
#include "CeresEnums.h"

using namespace System::ComponentModel;
namespace ceresdotnet {
	public ref class SolverOptions : public INotifyPropertyChanged
	{

#define WRAP_PRIMITIVE(type,propname) \
		property type propname { \
			type get() {\
				return _options->propname; \
			} \
			void set(type v) {\
				_options->propname = v; \
				OnPropertyChanged("propname");\
			} \
		} \

#define WRAP_ENUM(type,propname) \
		property type propname { \
			type get() {\
				return (type)_options->propname; \
			} \
			void set(type v) {\
				_options->propname = (ceres::type)v; \
				OnPropertyChanged("propname");\
			} \
		} \

	internal:
		ceres::Solver::Options* _options;

	public:
		SolverOptions()
		{
			_options = new ceres::Solver::Options;
			//algemene opties voor de solver			
			_options->dense_linear_algebra_library_type = ceres::DenseLinearAlgebraLibraryType::LAPACK;
			_options->linear_solver_type = ceres::LinearSolverType::SPARSE_SCHUR;
			_options->sparse_linear_algebra_library_type = ceres::SparseLinearAlgebraLibraryType::SUITE_SPARSE;
			_options->trust_region_strategy_type = ceres::TrustRegionStrategyType::LEVENBERG_MARQUARDT;
			_options->num_linear_solver_threads = 8;
			_options->num_threads = 8;
			_options->minimizer_progress_to_stdout = true;

			//opties voor solver_termination
			_options->max_num_iterations = 70;
			_options->function_tolerance = 0.000005; // = |cost_change| / cost

			_options->update_state_every_iteration = true;
		}
		!SolverOptions()
		{
			free(_options);
		}
		~SolverOptions()
		{
			free(_options);
		}

		virtual event PropertyChangedEventHandler^ PropertyChanged;
	private:
		void OnPropertyChanged(String^ propname){
			PropertyChanged(this, gcnew PropertyChangedEventArgs(propname));
		}
	public:

		WRAP_PRIMITIVE(bool, check_gradients);
		WRAP_ENUM(DenseLinearAlgebraLibraryType, dense_linear_algebra_library_type);
		WRAP_ENUM(DoglegType, dogleg_type);
		WRAP_PRIMITIVE(bool, dynamic_sparsity);
		WRAP_PRIMITIVE(double, eta);
		WRAP_PRIMITIVE(double, function_tolerance);
		WRAP_PRIMITIVE(double, gradient_check_numeric_derivative_relative_step_size);
		WRAP_PRIMITIVE(double, gradient_check_relative_precision);
		WRAP_PRIMITIVE(double, gradient_tolerance);
		WRAP_PRIMITIVE(double, initial_trust_region_radius);
		WRAP_PRIMITIVE(double, inner_iteration_tolerance);
		bool IsValid(System::String^% error){
			std::string s;
			return _options->IsValid(&s);
			error = gcnew System::String(s.c_str());
		}
		WRAP_PRIMITIVE(bool, jacobi_scaling);
		WRAP_ENUM(LinearSolverType, linear_solver_type);
		WRAP_ENUM(LineSearchDirectionType, line_search_direction_type);
		WRAP_PRIMITIVE(double, line_search_sufficient_curvature_decrease);
		WRAP_PRIMITIVE(double, line_search_sufficient_function_decrease);
		WRAP_ENUM(LineSearchType, line_search_type);
		WRAP_ENUM(LoggingType, logging_type);
		WRAP_PRIMITIVE(int, max_consecutive_nonmonotonic_steps);
		WRAP_PRIMITIVE(int, max_lbfgs_rank);
		WRAP_PRIMITIVE(int, max_linear_solver_iterations);
		WRAP_PRIMITIVE(double, max_line_search_step_contraction);
		WRAP_PRIMITIVE(double, max_solver_time_in_seconds);
		WRAP_PRIMITIVE(double, max_trust_region_radius);
		WRAP_PRIMITIVE(bool, minimizer_progress_to_stdout);
		WRAP_ENUM(MinimizerType, minimizer_type);
		WRAP_PRIMITIVE(int, min_linear_solver_iterations);
		WRAP_PRIMITIVE(double, min_line_search_step_contraction);
		WRAP_PRIMITIVE(double, min_line_search_step_size);
		WRAP_PRIMITIVE(double, min_lm_diagonal);
		WRAP_PRIMITIVE(double, min_relative_decrease);
		WRAP_PRIMITIVE(double, min_trust_region_radius);
		WRAP_ENUM(NonlinearConjugateGradientType, nonlinear_conjugate_gradient_type);
		WRAP_PRIMITIVE(int, num_linear_solver_threads);
		WRAP_PRIMITIVE(int, num_threads);
		WRAP_PRIMITIVE(double, parameter_tolerance);
		WRAP_ENUM(PreconditionerType, preconditioner_type);
		WRAP_ENUM(SparseLinearAlgebraLibraryType, sparse_linear_algebra_library_type);
		WRAP_ENUM(DumpFormatType, trust_region_problem_dump_format_type);
		WRAP_ENUM(TrustRegionStrategyType, trust_region_strategy_type);
		WRAP_PRIMITIVE(bool, update_state_every_iteration);
		WRAP_PRIMITIVE(bool, use_approximate_eigenvalue_bfgs_scaling);
		WRAP_PRIMITIVE(bool, use_explicit_schur_complement);
		WRAP_PRIMITIVE(bool, use_inner_iterations);
		WRAP_PRIMITIVE(bool, use_nonmonotonic_steps);
		WRAP_PRIMITIVE(bool, use_postordering);
		WRAP_ENUM(VisibilityClusteringType, visibility_clustering_type);
	};
}
