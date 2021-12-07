using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.Annotations;
using BundleAdjuster.ParameterBlockView;
using ceresdotnet;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace BundleAdjuster.GCPView.ViewModels
{
    [Export]
    public class GCPViewModel :  Tool,INotifyPropertyChanged, IParameterBlockSelector {
        private ObservableCollection<Calibratie.GCP> _gcps;
        public ObservableCollection<Calibratie.GCP> Gcps {
            get => _gcps;
            set {
                _gcps = value;
                OnPropertyChanged();
            }
        }

        private ceresdotnet.MultiCameraBundler _bundler;

        private Views.GCPView _view;

        public override PaneLocation PreferredLocation => PaneLocation.Left;
        public override double PreferredWidth => 500;
        public override string DisplayName => "Ground Control Points (GCP)";

        protected override void OnViewLoaded(object view)
        {
            _view = (Views.GCPView)view;
            _view.DataContext = this;
            _view.Datagrid.SelectionChanged += Datagrid_SelectionChanged;
            this.Activated += GCPViewModel_Activated;
        }

        private void GCPViewModel_Activated(object sender, Caliburn.Micro.ActivationEventArgs e) {
            Datagrid_SelectionChanged(null, null);
        }

        public event ParamblockSelected ParameterblockSelected;

        private void Datagrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            var gcp = _view.Datagrid.SelectedItem as Calibratie.GCP;
            ParameterblockSelected?.Invoke(gcp.AdjustedPosition);
        }

        public void SetBundler(ceresdotnet.MultiCameraBundler bundler) {
            _bundler = bundler;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
