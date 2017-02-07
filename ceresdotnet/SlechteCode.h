#pragma region c++embed
template<typename T, int size>
[System::Runtime::CompilerServices::UnsafeValueType]
[System::Runtime::InteropServices::StructLayout
(
System::Runtime::InteropServices::LayoutKind::Explicit,
Size = (sizeof(T)*size)
)
]
public value struct inline_array {
private:
	[System::Runtime::InteropServices::FieldOffset(0)]
	T dummy_item;

public:
	T% operator[](int index) {
		return *((&dummy_item) + index);
	}

	static operator interior_ptr<T>(inline_array<T, size>% ia) {
		return &ia.dummy_item;
	}
};

template<typename T>
ref class Embedded {
	T* t;

	!Embedded() {
		if (t != nullptr) {
			delete t;
			t = nullptr;
		}
	}

	~Embedded() {
		this->!Embedded();
	}

public:
	Embedded() : t(new T) {}

	static T* operator&(Embedded% e) { return e.t; }
	static T* operator->(Embedded% e) { return e.t; }
};
#pragma endregion

/*
#pragma region Slecht
public ref class CeresCamera {
private:
	Embedded<EuclideanCamera> camera;

	bool _intrinsic_linked = false;

public:
	EuclideanCamera* n(){
		return &camera;
	};



	void LinkIntrinsicsToCamera(CeresCamera^ mainCamera){
		if (mainCamera->IntrinsicsLinked){
			throw gcnew System::ArgumentException("maincamera mag niet gelink zijn!");
		}
		EuclideanCamera* main = mainCamera->n();
		if (n()->intrinsics != nullptr){
			delete[] n()->intrinsics;
		}
		n()->intrinsics = main->intrinsics;
		_intrinsic_linked = true;

	}

	void UnlinkIntrinsics(){
		_intrinsic_linked = false;
		n()->intrinsics = new double[9];
	}




	CeresCamera(OpenTK::Matrix3d r, OpenTK::Vector3d rvec, int image){
		EuclideanCamera* c;
		c = &camera;
		Mat3* rmat = new Mat3();
		Mat3 test;
		(*rmat)(0) = r.M11;
		(*rmat)(1) = r.M12;
		(*rmat)(2) = r.M13;
		(*rmat)(3) = r.M21;
		(*rmat)(4) = r.M22;
		(*rmat)(5) = r.M23;
		(*rmat)(6) = r.M31;
		(*rmat)(7) = r.M32;
		(*rmat)(8) = r.M33;


		c->R = *rmat;

		Vec3* p = new Vec3();
		(*p)(0) = rvec.X;
		(*p)(1) = rvec.Y;
		(*p)(2) = rvec.Z;
		c->t = *p;

		c->image = image;
	}

	property array<double>^ Intrinsics {
		array<double>^ get() {
			array<double>^ r = gcnew array<double>(9);
			pin_ptr<double> p = &r[0];
			memcpy(p, n()->intrinsics, 9 * 8);
			return r;
		}
		void set(array<double>^ v){
			pin_ptr<double> p = &v[0];
			memcpy(n()->intrinsics, p, 9 * 8);
		}
	}



	property int bundle_intrinsics{
		int get() {
			return n()->bundle_intrinsics;
		}
		void set(int v){
			n()->bundle_intrinsics = v;
		}
	}

	property bool IntrinsicsLinked{
		bool get() {
			return _intrinsic_linked;
		}
	}
	property int id{
		int get(){
			return n()->image;
		}
		void set(int i)
		{
			n()->image = i;
		}
	}
};

public ref class CeresPoint{
	Embedded<EuclideanPoint> point;
public:
	EuclideanPoint* getNativeClass(){
		return &point;
	};
	CeresPoint(Vector3d pos, int trackNr){
		double* p = &(&point)->X(0);
		p[0] = pos.X;
		p[1] = pos.Y;
		p[2] = pos.Z;

		(&point)->track = trackNr;
	}
	CeresPoint(int trackNr){
		double* p = &(&point)->X(0);
		p[0] = 0;
		p[1] = 0;
		p[2] = 0;

		(&point)->track = trackNr;
	}
	Vector3d getPos(){
		Vec3 p = (&point)->X;
		Vector3d^ r = gcnew Vector3d(p(0), p(1), p(2));
		return *r;
	}
	property int TrackNr{
		int get(){
			return (&point)->track;
		}
		void set(int value){
			(&point)->track = value;
		}
	}
};

public ref class CeresMarker{
internal:
	Embedded<Marker> marker;

public:
	CeresPoint^ Worldcoordinates;

	Marker* getNativeClass(){
		return &marker;
	};
	CeresMarker(double x, double y, CeresPoint^ worldpoint){
		Worldcoordinates = worldpoint;
	}
};

#pragma endregion

#pragma region Interfaces

public interface class ICeresMarker{
	property double x{double get(); };
	property double y{double get(); };
	property double* WorldCoordinates{double* get(); }
	property BundleWorldCoordinatesFlags BundleCoordinates{BundleWorldCoordinatesFlags get(); }
};
public interface class ICeresCamera{
	property String^ Name{String^ get(); }
	property double* Intrinsics{double* get(); }
	property BundleIntrinsicsFlags BundleIntrinsics{BundleIntrinsicsFlags get(); void set(BundleIntrinsicsFlags); }
	property double* Rt{double* get(); }
};
public interface class ICeresStereoCamera{
	property String^ Name{String^ get(); }
	property double* Intrinsics1{double* get(); }
	property double* Intrinsics2{double* get(); }
	property BundleIntrinsicsFlags BundleIntrinsics1{BundleIntrinsicsFlags get(); }
	property BundleIntrinsicsFlags BundleIntrinsics2{BundleIntrinsicsFlags get(); }
	property double* Rt1{double* get(); }
	property double* Rt2{double* get(); }
};
public interface class ICeresObservation{
	property ICeresCamera^ Camera{ICeresCamera^ get(); }
	property IEnumerable<ICeresMarker^>^ Markers{IEnumerable<ICeresMarker^>^ get(); }
};
public interface class ICeresStereoObservation{
	property ICeresStereoCamera^ StereoCamera{ICeresStereoCamera^ get(); }
	property IEnumerable<ICeresMarker^>^ MarkersFirst{IEnumerable<ICeresMarker^>^ get(); }
	property IEnumerable<ICeresMarker^>^ MarkersSecond{IEnumerable<ICeresMarker^>^ get(); }
};

#pragma endregion
*/



/*
public ref class MultiCameraBundleProblem
{
private:

static ceres::SubsetParameterization* GetIntrinsicsParametrization(BundleIntrinsicsFlags bundle_intrinsics){
std::vector<int> constant_intrinsics;

#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
if (!(bundle_enum.HasFlag(bundle_intrinsics))) { \
constant_intrinsics.push_back(offset); \
}
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::FocalLength, OFFSET_FOCAL_LENGTH_X);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::FocalLength, OFFSET_FOCAL_LENGTH_Y);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::PrincipalP, OFFSET_PRINCIPAL_POINT_X);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::PrincipalP, OFFSET_PRINCIPAL_POINT_Y);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R1, OFFSET_K1);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R2, OFFSET_K2);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::P1, OFFSET_P1);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::P2, OFFSET_P2);
MAYBE_SET_CONSTANT(BundleIntrinsicsFlags::R3, OFFSET_K3);
#undef MAYBE_SET_CONSTANT

ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
return subset_parameterization;
}

internal:
IEnumerable<ICeresObservation^>^ observations_currentSolve;
IEnumerable<ICeresStereoObservation^>^ stereoObservations_currentSolve;
int OnIterationStep(int iteration){
CeresCallbackReturnType r;
//IterationStep(iteration, observations_currentSolve, stereoObservations_currentSolve, r);
return (int)r;
}

public:

List<CeresCamera^>^ cameras = gcnew List<CeresCamera^>();
List<CeresMarker^>^ markers = gcnew List<CeresMarker^>();
List<CeresPoint^>^ all_points_managed = gcnew List<CeresPoint^>();


event Iteration^ IterationStep;

void Solve_New_Classes(IEnumerable<CeresCamera2^>^ cameras, IEnumerable<CeresStereoCamera^>^ stereocameras, Iteration^ callback){
Iteration_native_callback* cb((Iteration_native_callback*)Marshal::GetFunctionPointerForDelegate(callback).ToPointer());

ceres::Solver::Options solver_options;
ceres::Problem::Options problem_options;
ceres::Problem problem(problem_options);

ceres::Solver::Options options;


auto obsenum = cameras->GetEnumerator();

vector<double*> intrinsicsList();
vector<double*> worldPointList();



while (obsenum->MoveNext()){
auto obs = obsenum->Current;
ICeresCamera^ camera = obs->Camera;
IEnumerator<ICeresMarker^>^ markerenum = obs->Markers->GetEnumerator();

double* intrinsics = camera->Intrinsics;

while (markerenum->MoveNext()){
ICeresMarker^ marker = markerenum->Current;

problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
ReprojectionError, 2, 9, 6, 3 >(new ReprojectionError(marker->x, marker->y)),
NULL,
camera->Intrinsics,
camera->Rt,
marker->WorldCoordinates);

if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::None){
problem.SetParameterBlockConstant(marker->WorldCoordinates);
}
else if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::ALL)
{
}
else
{
std::vector<int> constant_coordinates;
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::X)) constant_coordinates.push_back(0);
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Y)) constant_coordinates.push_back(1);
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Z)) constant_coordinates.push_back(2);

ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(3, constant_coordinates);
problem.SetParameterization(marker->WorldCoordinates, subset_parameterization);
}
}
ceres::SubsetParameterization* subset_parameterization = GetIntrinsicsParametrization(camera->BundleIntrinsics);
problem.SetParameterization(intrinsics, subset_parameterization);

}
}

void SolveInterface(IEnumerable<ICeresObservation^>^ observations, IEnumerable<ICeresStereoObservation^>^ stereoObservations) {
if (observations != nullptr){

}
ceres::Solver::Options solver_options;
ceres::Problem::Options problem_options;
ceres::Problem problem(problem_options);

ceres::Solver::Options options;



auto obsenum = observations->GetEnumerator();

vector<double*> intrinsicsList();
vector<double*> worldPointList();



while (obsenum->MoveNext()){
auto obs = obsenum->Current;
ICeresCamera^ camera = obs->Camera;
IEnumerator<ICeresMarker^>^ markerenum = obs->Markers->GetEnumerator();

double* intrinsics = camera->Intrinsics;

while (markerenum->MoveNext()){
ICeresMarker^ marker = markerenum->Current;

problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
ReprojectionError, 2, 9, 6, 3 > (new ReprojectionError(marker->x, marker->y)),
NULL,
camera->Intrinsics,
camera->Rt,
marker->WorldCoordinates);

if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::None){
problem.SetParameterBlockConstant(marker->WorldCoordinates);
}
else if (marker->BundleCoordinates == BundleWorldCoordinatesFlags::ALL)
{}
else
{
std::vector<int> constant_coordinates;
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::X)) constant_coordinates.push_back(0);
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Y)) constant_coordinates.push_back(1);
if (!marker->BundleCoordinates.HasFlag(BundleWorldCoordinatesFlags::Z)) constant_coordinates.push_back(2);

ceres::SubsetParameterization* subset_parameterization = new ceres::SubsetParameterization(3, constant_coordinates);
problem.SetParameterization(marker->WorldCoordinates, subset_parameterization);
}
}
ceres::SubsetParameterization* subset_parameterization = GetIntrinsicsParametrization(camera->BundleIntrinsics);
problem.SetParameterization(intrinsics, subset_parameterization);

}
}



//MultiCameraBundleProblem.SolveProblem()
//Data gekend:	CeresCameras (1 per foto) bevat interne parameters
//								en initiele waarden voor de externe (via SolvePnP())
//				CeresMarkers (1 per marker per foto) bevat x,y fotocoordinaten & bijhorende CeresCamera
//				CeresPoints (1 per marker) bevat x,y,z data in wereldcoordinaten
void SolveProblem(){
vector<EuclideanPoint> all_points;
IEnumerator<CeresPoint^>^ pointenum = all_points_managed->GetEnumerator();
int c = 0;
//omzetten van managed c# naar native c++
while (pointenum->MoveNext()){
CeresPoint^ p = pointenum->Current;
EuclideanPoint* m = p->getNativeClass();
m->track = p->TrackNr;
all_points.push_back(*m);
c++;
}

ceres::Problem::Options problem_options;
ceres::Problem problem(problem_options);


for each(CeresCamera^ cam in *cameras){
const EuclideanCamera *camera = cam->n();
if (!camera) {
continue;
}
//omzetten van externe parameters (wereldcoord->cameracoord) naar double[6] vorm

ceres::RotationMatrixToAngleAxis(&camera->R(0, 0), &(*camera->Rt)(0));
camera->Rt->tail<3>() = camera->t;
}

ceres::SubsetParameterization *subset_parameterization;
double* camera_intrinsics;
vector<double*> intrinsicsList;
for each(CeresMarker^ marker_managed in *markers)
{
Marker* marker = marker_managed->getNativeClass();
CeresCamera^ camera_managed;
EuclideanCamera* camera = camera_managed->n();

camera_intrinsics = camera->intrinsics;

//3D punt voor marker
EuclideanPoint *point = PointForTrack(&all_points, marker->track);

double *current_camera_R_t = (double*)camera->Rt;

double x = marker->x;
double y = marker->y;

//1 RESIDUAL BLOCK / MARKER
problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
ReprojectionError, 2, 9, 6, 3>(new ReprojectionError(marker->x, marker->y)),
NULL,
camera_intrinsics,
current_camera_R_t,
&point->X(0));

//De 3D coordinaten worden constant gehouden
problem.SetParameterBlockConstant(&point->X(0));

//INTERNE PARAMETERS
//zet interne parameters constant voor diegene die niet gebundled worden
bool intr_used = std::find(intrinsicsList.begin(), intrinsicsList.end(), camera_intrinsics) != intrinsicsList.end();
if (!camera_managed->IntrinsicsLinked && !intr_used){ //1x per interne parameters
PrintCameraIntrinsics("Original intrinsics: ", camera->intrinsics);
int bundle_intrinsics = camera->bundle_intrinsics;
std::vector<int> constant_intrinsics;
#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
if (!(bundle_intrinsics & bundle_enum)) { \
constant_intrinsics.push_back(offset); \
}
MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_X);
MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_Y);
MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_X);
MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_Y);
MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K1, OFFSET_K1);
MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K2, OFFSET_K2);
MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P1, OFFSET_P1);
MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P2, OFFSET_P2);
#undef MAYBE_SET_CONSTANT
//subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
//problem.SetParameterization(camera_intrinsics, subset_parameterization);
intrinsicsList.push_back(camera_intrinsics);
}
}
ceres::Solver::Options options;
options.use_nonmonotonic_steps = true;
options.preconditioner_type = ceres::SCHUR_JACOBI;
options.linear_solver_type = ceres::ITERATIVE_SCHUR;
options.use_inner_iterations = true;
options.max_num_iterations = 100;
options.minimizer_progress_to_stdout = true;

ceres::Solver::Summary summary;
ceres::Solve(options, &problem, &summary);
std::cout << "Final report:\n" << summary.FullReport();

for (int i = 0; i < intrinsicsList.size(); i++){
PrintCameraIntrinsics2("Final intrinsics: ", intrinsicsList[i]);
}
}

void Solve(){

vector<EuclideanPoint> all_points;
IEnumerator<CeresPoint^>^ pointenum = all_points_managed->GetEnumerator();
int c = 0;
while (pointenum->MoveNext()){
CeresPoint^ p = pointenum->Current;
EuclideanPoint* m = p->getNativeClass();
all_points.resize(c + 1);
all_points[c] = *m;
c++;
}

ceres::Problem::Options problem_options;
ceres::Problem problem(problem_options);


for each(CeresCamera^ cam in *cameras){
const EuclideanCamera *camera = cam->n();
if (!camera) {
continue;
}


PrintCameraIntrinsics("Original intrinsics: ", camera->intrinsics);
ceres::RotationMatrixToAngleAxis(&camera->R(0, 0), &(*camera->Rt)(0));
//TEMP FIX
double* rt(&(*camera->Rt)(0));
//rt[0] = -rt[0];
//rt[1] = -rt[1];
//rt[2] = -rt[2];
camera->Rt->tail<3>() = camera->t;
}

ceres::SubsetParameterization *subset_parameterization;
double* camera_intrinsics;
for each(CeresMarker^ marker_managed in *markers)
{

Marker* marker = marker_managed->getNativeClass();
CeresCamera^ camera_managed;// = marker_managed->parentCamera;
EuclideanCamera* camera = camera_managed->n();

camera_intrinsics = camera->intrinsics;

//3D punt voor marker
EuclideanPoint *point = PointForTrack(&all_points, marker->track);


//EXTERNE PARAMETERS//2 mogelijkheden
//1: camera R & t vrij - easy
double *current_camera_R_t = (double*)camera->Rt;


//2: camera R & t locked tov andere camera


//INTERNE PARAMETERS
//zet interne parameters constant voor diegene die niet gebundled worden
if (!camera_managed->IntrinsicsLinked){ //1x per interne parameters

int bundle_intrinsics = camera->bundle_intrinsics;

std::vector<int> constant_intrinsics;
#define MAYBE_SET_CONSTANT(bundle_enum, offset) \
if (!(bundle_intrinsics & bundle_enum)) { \
constant_intrinsics.push_back(offset); \
}
MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_X);
MAYBE_SET_CONSTANT(BUNDLE_FOCAL_LENGTH, OFFSET_FOCAL_LENGTH_Y);
MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_X);
MAYBE_SET_CONSTANT(BUNDLE_PRINCIPAL_POINT, OFFSET_PRINCIPAL_POINT_Y);
MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K1, OFFSET_K1);
MAYBE_SET_CONSTANT(BUNDLE_RADIAL_K2, OFFSET_K2);
MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P1, OFFSET_P1);
MAYBE_SET_CONSTANT(BUNDLE_TANGENTIAL_P2, OFFSET_P2);
#undef MAYBE_SET_CONSTANT
constant_intrinsics.push_back(OFFSET_K3);
subset_parameterization = new ceres::SubsetParameterization(9, constant_intrinsics);
}



//1 RESIDUAL BLOCK / MARKER
problem.AddResidualBlock(new ceres::AutoDiffCostFunction <
ReprojectionError, 2, 9, 6, 3 >(new ReprojectionError(marker->x,marker->y)),
NULL,
camera_intrinsics,
current_camera_R_t,
&point->X(0));

problem.SetParameterBlockConstant(&point->X(0));


}
//problem.SetParameterization(camera_intrinsics, subset_parameterization); ERROR??
problem.SetParameterBlockConstant((double*)cameras[0]->n()->Rt);


ceres::Solver::Options options;
options.use_nonmonotonic_steps = true;
options.preconditioner_type = ceres::SCHUR_JACOBI;
options.linear_solver_type = ceres::ITERATIVE_SCHUR;
options.use_inner_iterations = true;
options.max_num_iterations = 100;
options.minimizer_progress_to_stdout = true;

ceres::Solver::Summary summary;
ceres::Solve(options, &problem, &summary);
std::cout << "Final report:\n" << summary.FullReport();

//UnpackCamerasRotationAndTranslation(all_markers,			all_cameras_R_t,			all_cameras);
PrintCameraIntrinsics2("Final intrinsics: ", camera_intrinsics);
}
};
*/
