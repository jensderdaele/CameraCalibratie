using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.Annotations;
using BundleAdjuster;
using BundleAdjuster.CameraView.ViewModels;
using BundleAdjuster.GCPView.ViewModels;
using BundleAdjuster.IntrinsicsView.ViewModels;
using ceresdotnet;
using Calibratie;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace BundleAdjuster.ParameterBlockView.ViewModels
{
    [Export]
    public class ParameterBlockViewModel: Tool, INotifyPropertyChanged {


        [ImportMany(typeof(IParameterBlockSelector))]
        private IEnumerable<IParameterBlockSelector> _selectors;
        private ICeresParameterblock _block;
        private ceresdotnet.MultiCameraBundler _bundler => BP.Bundler;
        private CeresParameterBlock _nativeBlock;
        private Views.ParameterBlockView _view;

        public Type Type => _block?.GetType() ?? typeof(NullReferenceException);


        public Type NativeType {
            get {
                try {
                    if (_bundler.GetNativeParamblock(_block, out CeresParameterBlock nat)) {
                        return nat.GetType();
                    }
                }
                catch {
                    // ignored
                }

                return typeof(NullReferenceException);
            }
        }

        

        public IEnumerable<Enum> FlagTypes => Util.GetFlagTypesForParamblock(_block);

        public System.Collections.IList BundleFlags => FlagTypes.Where(x => _nativeBlock.BundleFlagsEnum.HasFlag(x)).ToList();
           
        

        public ICeresParameterblock Block => _block;
        

        public unsafe double[] BlockData {
             get {
                 if (_nativeBlock == null) return new double[0];
                var r = new double[BlockSize];
                Marshal.Copy(new IntPtr(_nativeBlock._data),r,0,BlockSize);
                 return r;
             }
        }

        public string DataString {
            get {

                var d = BlockData;
                string s = "";
                for (int i = 0; i < d.Length; i++) {
                    s += d[i] + (i < d.Length - 1 ? Environment.NewLine : "");
                }
                return s;
            }
        }

        public int ResidualBlockCount => _block != null ? _bundler.GetResidualBlockCountForParameterBlock(_block) : 0;


        public int BlockSize => _nativeBlock?.Length ?? 0;

        /// <summary>
        /// Native
        /// </summary>
        public double Cost => _bundler.GetCostForNativeParameterblock(_nativeBlock, true);


        protected override void OnViewLoaded(object view)
        {
            _view = (Views.ParameterBlockView)view;
            _view.DataContext = this;
            foreach (var s in _selectors) {
                s.ParameterblockSelected += ParameterblockSelected;
            }
        }

        private void ParameterblockSelected(ICeresParameterblock block) {
            _block = block;
            if (BP.Bundler.GetNativeParamblock(block, out var b)) {
                _nativeBlock = b;
            }
            else {
                _bundler.BuildProblem();
                _bundler.GetNativeParamblock(block, out var b2);
                _nativeBlock = b2;
            }
            blockChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void blockChanged()
        {
            OnPropertyChanged(nameof(Block));
            OnPropertyChanged(nameof(FlagTypes));
            OnPropertyChanged(nameof(BundleFlags));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(NativeType));
            OnPropertyChanged(nameof(BlockData));
            OnPropertyChanged(nameof(BlockSize));
            OnPropertyChanged(nameof(DataString));
            OnPropertyChanged(nameof(ResidualBlockCount));
        }

        public void SetBundleFlag(Enum flag, bool setOn)
        {
            _nativeBlock.BundleFlagsEnum = setOn ? _nativeBlock.BundleFlagsEnum.SetOn(flag) : _nativeBlock.BundleFlagsEnum.SetOff(flag);
            OnPropertyChanged(nameof(BundleFlags));
        }

        public void SetBundleFlag(Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            SetBundleFlag((Enum)e.Item,e.IsSelected);
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public override string DisplayName => "Parameterblock - Info";
        public override double PreferredWidth => 450;
    }
}
