// ArUcoNET.h

#pragma once
//#include <opencv2\opencv.hpp>
//#include <opencv2\aruco.hpp>
//#include <aruco.h>


/*
using namespace std;
using namespace System::Collections::Generic;
using namespace System;*/
/*

int main(int argc, char **argv){
	Version^ version = Environment::Version;
	if (version)
	{
		int build = version->Build;
		int major = version->Major;
		int minor = version->Minor;
		int revision = Environment::Version->Revision;
		Console::Write(".NET Framework version: ");
		Console::WriteLine("{0}.{1}.{2}.{3}",
			build, major, minor, revision);
	}
	return 0;
}
*/

namespace ArUcoNET {

	public ref class ArucoTEST
	{
	public:
		static void findAruco(){}
		/*static void findAruco(IntPtr image_InputArray){
			try{
				CameraParameters CamParam;
				MarkerDetector MDetector;
				vector<Marker> Markers;
				Mat inImage = imread("C:\\Users\\jens\\Desktop\\calibratie\\canon 60d\\IMG_3198.JPG");
				CamParam.resize(inImage.size());

				MDetector.detect(inImage, CamParam, 0.150f);
				for (size_t i = 0; i < Markers.size(); i++)
				{
					cout << Markers[i] << endl;
					Markers[i].draw(inImage, Scalar(0, 0, 255), 2);
					CvDrawingUtils::draw3dCube(inImage, Markers[i], CamParam);
				}

				cv::namedWindow("in");
			cv:imshow("in", inImage);
				cv::waitKey(0);
			}
			catch (std::exception &ex){
				cout << "Exception: " << ex.what() << endl;
			}
			
			
		};*/
		/*
			Ptr<aruco::DetectorParameters> detectorParams = aruco::DetectorParameters::create();
			detectorParams->doCornerRefinement = true; // do corner refinement in markers

			
			Ptr<aruco::Dictionary> dictionary =	aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::Mat Image = cv::imread("");

			vector< vector< Point2f > > corners, rejected;
			vector< int > ids;

			cv::aruco::detectMarkers(Image, dictionary, corners, ids, detectorParams, rejected);*/
	};
}
