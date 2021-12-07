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
using Calibratie;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace BundleAdjuster.IntrinsicsView.ViewModels
{
    [Export]
    public class IntrinsicsViewModel: Tool ,INotifyPropertyChanged , IParameterBlockSelector
    {
        private ObservableCollection<CameraIntrinsics> _intrinsics;
        private Views.IntrinsicsView _view;

        public ObservableCollection<CameraIntrinsics> Intrinsics {
            get => _intrinsics;
            set { _intrinsics = value; OnPropertyChanged(); }
        }
        private void Listview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ParameterblockSelected?.Invoke(_view.Datagrid.SelectedItem as ICeresParameterblock);
        }
        protected override void OnViewLoaded(object view)
        {
            _view = (Views.IntrinsicsView)view;
            _view.DataContext = this;
            _view.Datagrid.SelectionChanged += Listview_SelectionChanged;
            _view.Datagrid.MouseUp += Datagrid_MouseUp;
        }

        private void Datagrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Listview_SelectionChanged(null, null);
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override PaneLocation PreferredLocation => PaneLocation.Bottom;
        public override double PreferredHeight => 300;
        public override double PreferredWidth => 1000;
        public override string DisplayName => "Sensors - List";

        public event ParamblockSelected ParameterblockSelected;
    }
}
