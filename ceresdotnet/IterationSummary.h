#pragma once

#pragma managed(push,off)
#include "Header.h"//ceresdotnetnative
#pragma managed(pop)
namespace ceresdotnet {
	public ref class IterationSummary {
	private:
		const ceres::IterationSummary* _summary;
	public:
		IterationSummary(const ceres::IterationSummary* s) :_summary(s) {};
		property int iteration {
			int get() {
				return _summary->iteration;
			};
		};
		property double iteration_time_in_seconds {
			double get() {
				return _summary->iteration_time_in_seconds;
			}
		}
		property double cost {
			double get() {
				return _summary->cost;
			}
		}
		property double cost_change {
			double get() {
				return _summary->cost_change;
			}
		}
		property bool step_is_valid {
			bool get() {
				return _summary->step_is_valid;
			}
		}
		property double step_solver_time_in_seconds {
			double get() {
				return _summary->step_solver_time_in_seconds;
			}
		}
		property bool step_is_successful {
			bool get() {
				return _summary->step_is_successful;
			}
		}
		property double trust_region_radius {
			double get() {
				return _summary->trust_region_radius;
			}
		}
	};
}