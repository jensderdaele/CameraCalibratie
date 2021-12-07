#pragma once

#include "ParameterBlocks.h"
#include "CeresEnums.h"
#include "MultiCameraBundler.h"

using namespace System::ComponentModel;

namespace ceresdotnet {
	public ref class BundlerIterationTracker : INotifyPropertyChanged {
	private:
		MultiCameraBundler^ _bundler;
		bool _trackEveryIteration = true;
		HashSet<ICeresParameterblock^>^ _trackData; //Blocks to track
		Dictionary<int, HashSet<CeresParameterBlock^>^>^ _storedData;
		int _lastIterationNr;

	public:

		virtual event PropertyChangedEventHandler^ PropertyChanged;
		void OnPropertyChanged(String^ info)
		{
			PropertyChanged(this, gcnew PropertyChangedEventArgs(info));
		}
		property bool TrackEveryIteration {
			bool get() { return _trackEveryIteration; }
			void set(bool v) { _trackEveryIteration = v; }
		}
		property HashSet<ICeresParameterblock^>^ TrackData {
			HashSet<ICeresParameterblock^>^ get() { return _trackData; }
		}
		property Dictionary<int, HashSet<CeresParameterBlock^>^>^ StoredData {
			Dictionary<int, HashSet<CeresParameterBlock^>^>^ get() { return _storedData; }
		}
		ceresdotnet::CeresCallbackReturnType OnIteration(System::Object ^sender, ceresdotnet::IterationSummary ^summary) {
			_lastIterationNr = summary->iteration;
			if (!summary->step_is_successful){
				_storedData->Add(summary->iteration, gcnew HashSet<CeresParameterBlock^>());
				return CeresCallbackReturnType::SOLVER_CONTINUE;
			}
			if (_trackEveryIteration && summary->iteration != 0)
			{
				auto hs = gcnew HashSet<CeresParameterBlock^>;
				for each (auto var in _bundler->_map)
				{
					if (_trackData == nullptr || _trackData->Contains(var.Key))
					{
						hs->Add(var.Value->Clone());
					}
				}
				if (hs->Count > 0)
					_storedData->Add(summary->iteration, hs);
			}
			OnPropertyChanged("");
			return CeresCallbackReturnType::SOLVER_CONTINUE;
		};
		HashSet<CeresParameterBlock^>^ CopyBundlerData(MultiCameraBundler^ bundler)
		{
			auto hs = gcnew HashSet<CeresParameterBlock^>;
			for each (auto var in _bundler->_map)
			{
				hs->Add(var.Value->Clone());
			}
			return hs;
		}
		void OnBundlerStart(MultiCameraBundler ^sender) {
			_storedData = gcnew Dictionary<int, HashSet<CeresParameterBlock^>^>();
			auto hs = gcnew HashSet<CeresParameterBlock^>;
			for each (auto var in _bundler->_map)
			{
				if (_trackData == nullptr || _trackData->Contains(var.Key))
				{
					hs->Add(var.Value->Clone());
				}
			}
			if (hs->Count > 0)
				_storedData->Add(0, hs);
			OnPropertyChanged("");
		};
		void OnBundlerTerminate(MultiCameraBundler ^sender) {
			return;
			auto hs = gcnew HashSet<CeresParameterBlock^>;
			for each (auto var in _bundler->_map){
				if (_trackData == nullptr || _trackData->Contains(var.Key)){
					hs->Add(var.Value->Clone());
				}
			}
			if (hs->Count > 0)
				_storedData->Add(_lastIterationNr+1, hs);
			OnPropertyChanged("");
		};
		BundlerIterationTracker(MultiCameraBundler^ bundler) {
			_bundler = bundler;
			_storedData = gcnew Dictionary<int, HashSet<CeresParameterBlock^>^>();
			
			bundler->Iteration += gcnew ceresdotnet::Iteration(this, &ceresdotnet::BundlerIterationTracker::OnIteration);
			bundler->BundlerTerminate += gcnew ceresdotnet::MultiCameraBundler::BundlerEvent(this, &ceresdotnet::BundlerIterationTracker::OnBundlerTerminate);
			bundler->BundlerStart += gcnew ceresdotnet::MultiCameraBundler::BundlerEvent(this, &ceresdotnet::BundlerIterationTracker::OnBundlerStart);
		};
		void UpdateBundleData(int iterationNr)
		{
			for each (CeresParameterBlock^ b in _storedData[iterationNr])
			{
				b->UpdateManagedData();
			}
		}
	};
};