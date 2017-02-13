using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace SceneManager {
    public abstract class SObject : SObjectBase {
        public string Name = "";

        protected SObject() : base() {
			Name = String.Format("Unnamed:{0}",this.GetHashCode());	
		}
        
    }
    public abstract class SObjectBase {
        protected Vector3d _pos;
        public Vector3d Pos {
            get { return _pos; }
            set { _pos = value; this.calcMatFromState(); }
        }
        
        protected Vector3d _dir;
        public Vector3d Dir { get { return _dir; } }
        protected Vector3d _up;
        public Vector3d Up { get { return _up; } }
        protected Vector3d _right;
        public Vector3d Right { get { return _right; } }
        

        protected Vector3d _scale = new Vector3d(1.0f);
        public Vector3d Scale {
            get { return _scale; }
            set { _scale = value; this.calcMatFromState(); }
        }



        public Matrix4d localMat;
        public Matrix4d worldMat;

        private SObject parent;

        

        public void Orient(Quaternion orientation) {
            Matrix4 newOrientation = Matrix4.CreateFromQuaternion(orientation);
            this._dir = new Vector3d(newOrientation.M31, newOrientation.M32, newOrientation.M33);
            this._up = new Vector3d(newOrientation.M21, newOrientation.M22, newOrientation.M23);
            this._right = Vector3d.Cross(this._up, this._dir).Normalized();
            this.calcMatFromState();
        }
        public void Orient(Quaterniond orientation) {
            Matrix4d newOrientation = Matrix4d.CreateFromQuaternion(orientation);
            this._dir = new Vector3d(newOrientation.M31, newOrientation.M32, newOrientation.M33);
            this._up = new Vector3d(newOrientation.M21, newOrientation.M22, newOrientation.M23);
            this._right = Vector3d.Cross(this._up, this._dir).Normalized();
            this.calcMatFromState();
        }
        public void Orient(Vector3d dir, Vector3d up) {
            this._dir = dir;
            this._up = up;
            this._right = Vector3d.Cross(this._up, this._dir).Normalized();
            this.calcMatFromState();
        }

        private float DegreeToRadian(float angleInDegrees) {
            return (float)Math.PI * angleInDegrees / 180.0f;
        }

        public void updateMat(ref Vector3d pos, ref Quaterniond orient) {
            this._pos = pos;

            Matrix4d mat = Matrix4d.CreateFromQuaternion(orient);
            this._right = new Vector3d(mat.M11, mat.M12, mat.M13);
            this._up = new Vector3d(mat.M21, mat.M22, mat.M23);
            this._dir = new Vector3d(mat.M31, mat.M32, mat.M33);
            calcMatFromState();
        }
        protected void updateMat(ref Vector3d dir, ref Vector3d up, ref Vector3d right, ref Vector3d pos) {
            this._pos = pos;
            this._dir = dir;
            this._right = right;
            this._up = up;
            calcMatFromState();
        }

        protected void calcMatFromState() {
            Matrix4d newLocalMat = Matrix4d.Identity;

            // rotation..
            newLocalMat.M11 = _right.X;
            newLocalMat.M12 = _right.Y;
            newLocalMat.M13 = _right.Z;

            newLocalMat.M21 = _up.X;
            newLocalMat.M22 = _up.Y;
            newLocalMat.M23 = _up.Z;

            newLocalMat.M31 = _dir.X;
            newLocalMat.M32 = _dir.Y;
            newLocalMat.M33 = _dir.Z;

            newLocalMat *= Matrix4d.Scale(this._scale);

            // position
            newLocalMat.M41 = _pos.X;
            newLocalMat.M42 = _pos.Y;
            newLocalMat.M43 = _pos.Z;

            // compute world transformation
            Matrix4d newWorldMat;

            if (this.parent == null) {
                newWorldMat = newLocalMat;
            }
            else {
                newWorldMat = newLocalMat * this.parent.worldMat;
            }

            // apply the transformations
            this.localMat = newLocalMat;
            this.worldMat = newWorldMat;

            NotifyPositionOrSizeChanged();
        }
        protected virtual void NotifyPositionOrSizeChanged() { }
        public virtual void Update(float fElapsedSecs) { }

        // constructor
		public SObjectBase() { 
			// position at the origin...
			this._pos = new Vector3d(0.0f,0.0f,0.0f);
			
			// base-scale
			this._dir = new Vector3d(0.0f,0.0f,1.0f);    // Z+  front
			this._up = new Vector3d(0.0f,1.0f,0.0f);     // Y+  up
			this._right = new Vector3d(1.0f,0.0f,0.0f);  // X+  right
			
			this.calcMatFromState();
			
			// rotate here if we want to.
		}
    }
}
