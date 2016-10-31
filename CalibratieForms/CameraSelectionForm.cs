using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calibratie;
using WeifenLuo.WinFormsUI.Docking;
using ComponentOwl.BetterListView;


namespace CalibratieForms {
    public partial class CameraSelectionForm : DockContent {
        //private static ObservableCollection<PinholeCamera> AllCameras = new ObservableCollection<PinholeCamera>();
        private LVList<PinholeCamera> _cameras = new LVList<PinholeCamera>(); 

        public CameraSelectionForm() {
            InitializeComponent();
            ComponentOwl.BetterListView.
            _cameras.CollumnDisplay2 = (camera, item) => {
                item.
                item.Text = camera.Name;
                item.SubItems.AddRange(new[] {
                    ""
                });
            };
            _cameras.ParentLV = betterListView1;
            this.betterListView1.Items
        }
    }
}
