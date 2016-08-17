#pragma once
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2\core.hpp>
#include <opencv2\aruco.hpp>


#include <opencv2\calib3d\calib3d.hpp>

#include "stdafx.h"
#include <iostream>
using namespace std;

using namespace System::Collections::Generic;
#include <msclr\marshal_cppstd.h>
using namespace System;
using namespace std;
using namespace cv;
using namespace System::Linq;




namespace ArUcoNET {
	
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
	
	public ref class CV_Native{
	public:
		static void SolvePnP(IEnumerable<OpenCvSharp::Point3f>^ objPoints,
			IEnumerable<OpenCvSharp::Point2f>^ imPoints,
			OpenCvSharp::InputArray^ cameraMat,array<double,2>^ cameraMatArr,
			array<double>^ distcffs5, OpenCvSharp::SolvePnPFlags flags){


			std::vector<Point3f> objectpoints(0);
			std::vector<Point2f> imagepoints(0);

			std::vector<Point3f>* objectpointsp = new std::vector<Point3f>();
			std::vector<Point3f*>* objectpointspp = new std::vector<Point3f*>();

			IEnumerator<OpenCvSharp::Point3f>^ objpointsenum =  objPoints->GetEnumerator();
			IEnumerator<OpenCvSharp::Point2f>^ impointsenum = imPoints->GetEnumerator();

			int i = 0;
			while (objpointsenum->MoveNext()){
				float x, y, z;
				x = objpointsenum->Current.X;
				y = objpointsenum->Current.Y;
				z = objpointsenum->Current.Z;

				objectpoints.push_back(cv::Point3f(x, y, z));
			}
			while (impointsenum->MoveNext()){
				float x, y;
				x = impointsenum->Current.X;
				y = impointsenum->Current.Y;

				imagepoints.push_back(cv::Point2f(x, y));
			}

			cv::Mat distCoeffs(5, 1, cv::DataType<double>::type);
			distCoeffs.at<double>(0) = distcffs5[0];
			distCoeffs.at<double>(1) = distcffs5[1];
			distCoeffs.at<double>(2) = distcffs5[2];
			distCoeffs.at<double>(3) = distcffs5[3];
			distCoeffs.at<double>(4) = distcffs5[4];

			cv::Mat rvec(3, 1, cv::DataType<double>::type);
			cv::Mat tvec(3, 1, cv::DataType<double>::type);

			cv::Mat cameraMatrix(3, 3, cv::DataType<double>::type);
			cv::setIdentity(cameraMatrix);
			for (int r = 0; r < 3; r++)
			{
				for (int c = 0; c < 3; c++)
				{
					cameraMatrix.at<double>(r, c) = cameraMatArr[r,c];
				}
			}
			
			

			//cv::Mat cameraMatrix(3, 3, cv::DataType<double>::type);
			
			cv::Mat distCoeffs2(5, 1, cv::DataType<double>::type);
			distCoeffs2.at<double>(0) = 0;
			distCoeffs2.at<double>(1) = 0;
			distCoeffs2.at<double>(2) = 0;
			distCoeffs2.at<double>(3) = 0;
			distCoeffs2.at<double>(4) = 0;
			cv::solvePnP(objectpoints, imagepoints, cameraMatrix, distCoeffs, rvec, tvec);
			std::cout << "rvec: " << rvec << std::endl;
			std::cout << "tvec: " << tvec << std::endl;
			int intflags = (int)flags;
			// (cv::InputArray)cameraMat->CvPtr
			cv::solvePnP(objectpoints, imagepoints, cameraMatrix, distCoeffs, rvec, tvec);
		}

		static void TestFmat(OpenCvSharp::Mat^ impts1, OpenCvSharp::Mat^ impts2, OpenCvSharp::Mat^ cameramat){
			Mat p1(*(cv::Mat*)(void*)impts1->CvPtr);
			Mat p2(*(cv::Mat*)(void*)impts2->CvPtr);
			Mat cm(*(cv::Mat*)(void*)cameramat->CvPtr);

			Mat E = cv::findEssentialMat(p1, p2, cm, RANSAC, 0.999, 3);
			correctMatches(E, p1, p2, p1, p2);

			cv::Point2d pp(cm.at<double>(2, 0), cm.at<double>(2, 1));
			Mat R, t;
			recoverPose(E, p1, p2, R, t, cm.at<double>(0, 0), pp);
			OpenCvSharp::Mat^ mmm = gcnew OpenCvSharp::Mat(IntPtr(&R));
		}
	};


	public ref class Aruco{
	public:
		static IEnumerable<ArucoMarker^>^ FindMarkers(System::String^ fileName){
		
			
			
			std::string f = msclr::interop::marshal_as<std::string>(fileName);
			cout << "FindMarkers aruco: " << f << endl;
			int start = GetTickCount();
			Mat testImage = imread(f, IMREAD_COLOR);
			
			cv::Ptr<aruco::DetectorParameters> parameters;
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
			cout << "FindMarkers aruco complete: " << sz << " gevonden in " << timelapse << "ms" << endl;
			return markerList;
		}
		
		
	};
}

