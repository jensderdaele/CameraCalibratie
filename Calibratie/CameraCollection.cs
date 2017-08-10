using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Emgu.CV;
using OpenTK;
using OpenTK.Graphics;

namespace Calibratie {
    
    public class CameraCollection : SObject, IEnumerable<PinholeCamera> {
        public static CameraCollection readTransfromXML(XmlReader r) {
            return SObject.readTransformXML<CameraCollection>(r);
        }
        private bool _cameraCountLocked = false;
        protected List<PinholeCamera> _cameras;

        public void SetCollectionCenter(PinholeCamera camera) {
            if (!_cameras.Contains(camera)) {
                throw new ArgumentException("camera niet in collectie");
            }
            SetCollectionCenter(this.WorldMat*camera.WorldMat);
        }
        public void SetCollectionCenter(Matrix<double> newWorldMat) {
            var transf = newWorldMat.Inverted()*this.WorldMat;
            foreach (var pinholeCamera in _cameras) {
                pinholeCamera.WorldMat = transf*pinholeCamera.WorldMat;
            }
            this.WorldMat = newWorldMat;
        }

        public void SetCollectionCenter_MidCameras() {
            throw new NotImplementedException();
        }


        public CameraCollection() {
            _cameras = new List<PinholeCamera>();
        }

        public CameraCollection(int lockedCameraCount) {
            _cameraCountLocked = true;
            _cameras = new List<PinholeCamera>(lockedCameraCount);
            for (int i = 0; i < lockedCameraCount; i++) {
                _cameras.Add(new PinholeCamera());
            }
        }

        public CameraCollection(IEnumerable<PinholeCamera> cameras) {
            _cameraCountLocked = true;
            _cameras = new List<PinholeCamera>(cameras);
        }

        public int Count { get { return _cameras.Count; } }

        public PinholeCamera this[int index] {
            get { return _cameras[index]; }
        }

        public void Add(PinholeCamera camera) {
            camera.PropertyChanged += camera_PropertyChanged;
            _cameras.Add(camera);
        }

        public void Remove(PinholeCamera camera) {
            camera.PropertyChanged -= camera_PropertyChanged;
            _cameras.Remove(camera);
        }

        void camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        public IEnumerator<PinholeCamera> GetEnumerator() {
            return _cameras.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
