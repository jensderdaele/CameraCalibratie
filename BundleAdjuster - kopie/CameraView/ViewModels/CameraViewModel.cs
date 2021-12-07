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
using BundleAdjuster.CameraView.Views;
using ceresdotnet;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;


using BundleAdjuster.ParameterBlockView;

namespace BundleAdjuster.CameraView.ViewModels
{
    [Export]
    public class CameraViewModel : Tool, INotifyPropertyChanged {//, IParameterBlockSelector {
        private Views.CameraView _view;

        private ObservableCollection<Calibratie.PinholeCamera> _cameras;
        public ObservableCollection<Calibratie.PinholeCamera> Cameras {
            get => _cameras;
            set {
                _cameras = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public event ParamblockSelected ParameterblockSelected;

        private void Listview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //ParameterblockSelected?.Invoke(_view.Listview.SelectedItem as ICeresParameterblock);
        }

        protected override void OnViewLoaded(object view)
        {
            _view = (Views.CameraView)view;
            _view.DataContext = this;
            _view.Listview.SelectionChanged += Listview_SelectionChanged;
            this.Activated += CameraViewModel_Activated;
        }

        private void CameraViewModel_Activated(object sender, ActivationEventArgs e) {
            Listview_SelectionChanged(null, null);
        }

        public override PaneLocation PreferredLocation => PaneLocation.Left;

        public override string DisplayName => "Cameras (Enabled) - List";

        public override double PreferredWidth => 500;
        
    }
}
