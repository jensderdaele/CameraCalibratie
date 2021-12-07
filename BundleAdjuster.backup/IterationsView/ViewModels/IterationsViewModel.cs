using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.Annotations;
using ceresdotnet;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace BundleAdjuster.IterationsView.ViewModels
{
    [Export]
    public class IterationsViewModel : Tool,  INotifyPropertyChanged
    { 
        private ceresdotnet.BundlerIterationTracker _tracker;

        public IObservableCollection<int> Nrs { get; set; }

        public bool TrackEveryIteration {
            get => _tracker.TrackEveryIteration;
            set {
                _tracker.TrackEveryIteration = value;
                OnPropertyChanged();
            }
        }

        private Views.IterationsView _view;
        public event PropertyChangedEventHandler PropertyChanged;

        IterationsViewModel() {
            Nrs = new BindableCollection<int>();
            _tracker = new BundlerIterationTracker(BP.Bundler);
        }
        protected override void OnViewLoaded(object view)
        {
            BP.Tracker = new BundlerIterationTracker(BP.Bundler);
            _tracker = BP.Tracker;
            _view = (Views.IterationsView)view;
            _view.DataContext = this;
            _tracker.PropertyChanged += _tracker_PropertyChanged;
            _view.Datagrid.MouseDoubleClick += Datagrid_MouseDoubleClick;
        }

        public override string DisplayName => "Iterations";

        private void Datagrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var nr = (int)_view.Datagrid.SelectedItem;
            _tracker.UpdateBundleData(nr);
        }

        private void _tracker_PropertyChanged(object sender, PropertyChangedEventArgs e) {
           // nrs.IsNotifying = false;
            Nrs.Clear();
            
            Nrs.IsNotifying = true;
            Nrs.AddRange(_tracker.StoredData.Keys);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;
    }
}
