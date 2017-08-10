// arucodotnet.h


#include "Stdafx.h"
#include <opencv2\core\core.hpp>
#include <opencv2\highgui\highgui.hpp>
#include <opencv2\core.hpp>
#include <opencv2\aruco.hpp>

#include <iostream>
#include <msclr\marshal_cppstd.h>



using namespace std;

using namespace System;
using namespace System::Linq;
using namespace System::Collections::Generic;
#include <msclr\marshal_cppstd.h>

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
	cv::Mat testImage = cv::imread("C:\\Users\\jens\\Desktop\\calibratie\\fiiw_template_masterproef_gent\\afb\\arucoSetup.jpg", cv::IMREAD_COLOR);

	cv::Ptr<cv::aruco::DetectorParameters> parameters;
	cv::Ptr<cv::aruco::Dictionary> markerDictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
	cv::Ptr<cv::aruco::DetectorParameters> detectorParams = cv::aruco::DetectorParameters::create();
	detectorParams->doCornerRefinement = true;

	std::vector< std::vector<cv::Point2f> > markerCorners;
	std::vector< std::vector<cv::Point2f> > rejected;
	std::vector<int> markerIDs;

	cv::aruco::detectMarkers(testImage, markerDictionary, markerCorners, markerIDs, detectorParams, rejected);

	return 0;

	
}

namespace arucodotnet {

	public ref class ArucoMarker{
	public:
		OpenCvSharp::Point2f Corner1;
		OpenCvSharp::Point2f Corner2;
		OpenCvSharp::Point2f Corner3;
		int ID;

		ArucoMarker(){

		}
		ArucoMarker(OpenCvSharp::Point2f^ Corner1, OpenCvSharp::Point2f^ Corner2, OpenCvSharp::Point2f^ Corner3, int id){
			
			this->Corner1 = *Corner1;
			this->Corner2 = *Corner2;
			this->Corner3 = *Corner3;
			ID = id;
		}
	};

	public ref class Aruco{
	public:
		static IEnumerable<ArucoMarker^>^ FindMarkers(System::String^ fileName){



			//std::string f = msclr::interop::marshal_as<std::string>(fileName);
			//cout << "FindMarkers aruco: " << "f" << endl;
			int start = GetTickCount();
			/*cv::Mat testImage = cv::imread(f, cv::IMREAD_COLOR);

			cv::Ptr<cv::aruco::DetectorParameters> parameters;
			cv::Ptr<cv::aruco::Dictionary> markerDictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::Ptr<cv::aruco::DetectorParameters> detectorParams = cv::aruco::DetectorParameters::create();
			detectorParams->doCornerRefinement = true;

			std::vector< std::vector<cv::Point2f> > markerCorners;
			std::vector< std::vector<cv::Point2f> > rejected;
			std::vector<int> markerIDs;

			cv::aruco::detectMarkers(testImage, markerDictionary, markerCorners, markerIDs, detectorParams, rejected);

			List<ArucoMarker^>^ markerList = gcnew List<ArucoMarker^>();
			int sz = markerCorners.size();
			for (size_t i = 0; i < markerCorners.size(); i++)
			{
				OpenCvSharp::Point2f^ corner1 = gcnew OpenCvSharp::Point2f(markerCorners[i][0].x, markerCorners[i][0].y);
				OpenCvSharp::Point2f^ corner2 = gcnew OpenCvSharp::Point2f(markerCorners[i][1].x, markerCorners[i][1].y);
				OpenCvSharp::Point2f^ corner3 = gcnew OpenCvSharp::Point2f(markerCorners[i][2].x, markerCorners[i][2].y);
				markerList->Add(gcnew ArucoMarker(corner1, corner2, corner3, markerIDs[i]));
			}
			int timelapse = GetTickCount() - start;
			//cout << "FindMarkers aruco complete: " << sz << " gevonden in " << timelapse << "ms" << endl;
			return markerList;*/

			return gcnew List<ArucoMarker^>();
		}
	};
}