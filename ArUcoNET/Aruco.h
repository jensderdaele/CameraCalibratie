#pragma once
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2\core.hpp>
#include <opencv2\aruco.hpp>
#include <opencv2\calib3d\calib3d.hpp>
#include <opencv2\xfeatures2d\nonfree.hpp>

#include "stdafx.h"
#include <iostream>

using namespace System::Collections::Generic;
#include <msclr\marshal_cppstd.h>
using namespace System;
//using namespace std;
using namespace cv;
using namespace System::Linq;
using namespace System::Drawing; 
using namespace System::Collections::Generic;
using namespace cv::xfeatures2d;

//#include "CVNative.h"

namespace ArUcoNET {
	static public ref class CVNative abstract sealed {
	public:
		static void triangulateNViews(array<System::Drawing::PointF>^ pts, array<Emgu::CV::Mat^>^ Ps, Emgu::CV::Matrix<double>^ triangulated) {
			//triangulateNViews(pts, Ps, triangulated);
			
		}
	};

	[FlagsAttribute]
	public enum class CornerFlags : int {
		Corner1_TOPLEFT = 0,
		Corner2_TOPRIGHT = 0x10000,
		Corner3_BOTTOMRIGHT =  0x20000,
		Corner4_BOTTOMLEFT =  0x40000,
	};
	[Serializable]
	public ref class ArucoMarker : Calibratie::Marker2d{
	public:
		System::Drawing::PointF Corner1;
		System::Drawing::PointF Corner2;
		System::Drawing::PointF Corner3;
		System::Drawing::PointF Corner4;
		int _id;

		virtual property int ID{ 
			int get()override{ return _id; }
			void set(int v){ _id = v; }
		};
		virtual property float X{ float get() override{ return Corner1.X; }};
		virtual property float Y{ float get() override{ return Corner1.Y; }};

		ArucoMarker(){}
		ArucoMarker(System::Drawing::PointF^ Corner1, System::Drawing::PointF^ Corner2, System::Drawing::PointF^ Corner3, System::Drawing::PointF^ Corner4, int id){
			this->Corner1 = *Corner1;
			this->Corner2 = *Corner2;
			this->Corner3 = *Corner3;
			this->Corner4 = *Corner4;
			ID = id;
		}
	};

	/* GARBAGE
	public ref class CV_Native{
	public:
		static void StereoCalibrate(List<List<System::Drawing::PointF>^>^ imagesleft, List<List<System::Drawing::PointF>^>^ imagesright, List<List<System::Drawing::PointF>^>^ worldpoints){
			

			
			vector<vector<Point2f> > imagePoints[2];
			imagePoints[0].resize(imagesleft->Count);
			imagePoints[1].resize(imagesleft->Count);

			vector<vector<Point3f> > objectPoints;
			objectPoints.resize(worldpoints->Count);

			for (size_t i = 0; i < imagesleft->Count; i++)
			{
				int leftcount = (*imagesleft)[i]->Count;
				int rightcount = (*imagesright)[i]->Count;
				int worldcount = (*worldpoints)[i]->Count;

				imagePoints[0][i].resize(leftcount);
				imagePoints[1][i].resize(rightcount);
				objectPoints[i].resize(worldcount);

				for (size_t j = 0; j < leftcount; j++)
				{
					imagePoints[0][i][j].x = (*(*imagesleft)[i])[j].X;
					imagePoints[0][i][j].y = (*(*imagesleft)[i])[j].Y;
				}
				
				for (size_t j = 0; j < rightcount; j++)
				{
					imagePoints[1][i][j].x = (*(*imagesright)[i])[j].X;
					imagePoints[1][i][j].y = (*(*imagesright)[i])[j].Y;
				}
				
				for (size_t j = 0; j < worldcount; j++)
				{
					objectPoints[i][j].x = (*(*worldpoints)[i])[j].X;
					objectPoints[i][j].y = (*(*worldpoints)[i])[j].Y;
					objectPoints[i][j].z = (*(*worldpoints)[i])[j].Z;
				}
			}

			Mat cameraMatrix[2], distCoeffs[2];

			Mat R, T, E, F;

			cv::Size size(1920, 1080);

			double r = cv::stereoCalibrate(objectPoints, imagePoints[0], imagePoints[1], cameraMatrix[0], distCoeffs[0], cameraMatrix[1], distCoeffs[1], size,
				R, T, E, F, 0);
			
			int ef = 4;
			int efs = 4;
		}
		static void SolvePnP(IEnumerable<OpenCvSharp::Point3f>^ objPoints,
			IEnumerable<System::Drawing::PointF>^ imPoints,
			OpenCvSharp::InputArray^ cameraMat,array<double,2>^ cameraMatArr,
			array<double>^ distcffs5, OpenCvSharp::SolvePnPFlags flags){


			std::vector<Point3f> objectpoints(0);
			std::vector<Point2f> imagepoints(0);

			std::vector<Point3f>* objectpointsp = new std::vector<Point3f>();
			std::vector<Point3f*>* objectpointspp = new std::vector<Point3f*>();

			IEnumerator<OpenCvSharp::Point3f>^ objpointsenum =  objPoints->GetEnumerator();
			IEnumerator<System::Drawing::PointF>^ impointsenum = imPoints->GetEnumerator();

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
	*/

	static std::string format(const char* fmt, ...){
		int size = 512;
		char* buffer = 0;
		buffer = new char[size];
		va_list vl;
		va_start(vl, fmt);
		int nsize = vsnprintf(buffer, size, fmt, vl);
		if (size <= nsize){ //fail delete buffer and try again
			delete[] buffer;
			buffer = 0;
			buffer = new char[nsize + 1]; //+1 for /0
			nsize = vsnprintf(buffer, size, fmt, vl);
		}
		std::string ret(buffer);
		va_end(vl);
		delete[] buffer;
		return ret;
	}
	public ref class SIFTTEST
	{
	public:
		static void test()
		{
			auto img1 = cv::imread("D:\\3dtrenchview\\autofeaturedetection\\testdata\\G0026845.JPG");
			auto img2 = cv::imread("D:\\3dtrenchview\\autofeaturedetection\\testdata\\G0026846.JPG");
			Mat roi = img1(Rect(1125, 1941, 517, 321)); 
			namedWindow("Example1");
			imshow("Example1", roi);
			cvSaveImage("D:\\3dtrenchview\\autofeaturedetection\\testdata\\G0026845_saveroi.JPG", &img1);

		}
	};

	public ref class Aruco{
	private:
		
	public:
		static void CreateMarkerToFile(int id, std::string path, int pixelSz){
			cv::Mat markerImage; 
			cv::Ptr<cv::aruco::Dictionary> dictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::aruco::drawMarker(dictionary, id, pixelSz, markerImage, 1);
			imwrite(path, markerImage);
		}
		
		static System::IntPtr CreateMarker(int id, int pixelSz){
			cv::Mat markerImage;
			cv::Ptr<cv::aruco::Dictionary> dictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::aruco::drawMarker(dictionary, id, pixelSz, markerImage, 1);

			
			return IntPtr(&markerImage);
			
		}

		static System::IntPtr CreateMarker(int id, int pixelSz, System::String^ saveFile){
			std::string f = msclr::interop::marshal_as<std::string>(saveFile);
			cv::Mat markerImage;
			cv::Ptr<cv::aruco::Dictionary> dictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::aruco::drawMarker(dictionary, id, pixelSz, markerImage, 1);

			cv::imwrite(f, markerImage);

			//System::Drawing::Bitmap^ bitmap = gcnew System::Drawing::Bitmap(pixelSz, pixelSz, 32 * pixelSz, System::Drawing::Imaging::PixelFormat::Format32bppRgb, IntPtr(&markerImage));

			return IntPtr(&markerImage);

		}
		
		static IEnumerable<ArucoMarker^>^ FindMarkers(System::String^ fileName){
			return FindMarkers(fileName, "");
			
		}
		static IEnumerable<ArucoMarker^>^ FindMarkers(System::String^ fileName, System::String^ detectedFile){
			std::string f = msclr::interop::marshal_as<std::string>(fileName);
			std::string df = msclr::interop::marshal_as<std::string>(detectedFile);
			
			//cout << "FindMarkers aruco: " << f << endl;
			int start = GetTickCount();
			Mat testImage = imread(f, IMREAD_COLOR);
			return FindMarkers(&testImage, detectedFile);
		
		}
		static IEnumerable<ArucoMarker^>^ FindMarkers(Emgu::CV::Mat^ image, System::String^ detectedFile){
			return FindMarkers((Mat*)(image->Ptr.ToPointer()), detectedFile);
			
		}

		static void DrawMarkers(Emgu::CV::Mat^ image, IEnumerable<ArucoMarker^>^ markers) {
			std::vector< std::vector<cv::Point2f> > markerCorners;
			std::vector<int> markerIDs;
			for each (ArucoMarker^ marker in markers) {
				std::vector<cv::Point2f> corners;
				markerIDs.push_back(marker->ID);
				corners.push_back(cv::Point2f(marker->Corner1.X, marker->Corner1.Y));
				corners.push_back(cv::Point2f(marker->Corner2.X, marker->Corner2.Y));
				corners.push_back(cv::Point2f(marker->Corner3.X, marker->Corner3.Y));
				corners.push_back(cv::Point2f(marker->Corner4.X, marker->Corner4.Y));
				markerCorners.push_back(corners);
			}

			cv::aruco::drawDetectedMarkers(*(Mat*)(image->Ptr.ToPointer()), markerCorners, markerIDs);
		}
		static void DrawMarkers(Emgu::CV::Mat^ image, IEnumerable<ArucoMarker^>^ markers, System::String^ outDir) {
			DrawMarkers(image, markers);
			cv::imwrite(msclr::interop::marshal_as<std::string>(outDir), *(Mat*)(image->Ptr.ToPointer()));
		}

		//IGNORES MARKERS DIE VOOR 2E KEER GEVONDEN WORDEN 
		static IEnumerable<ArucoMarker^>^ FindMarkers(Mat* image, System::String^ detectedFile){
			//std::string f = msclr::interop::marshal_as<std::string>(fileName);
			std::string df = msclr::interop::marshal_as<std::string>(detectedFile);
			//cout << "FindMarkers aruco from intptr " << endl;
			int start = GetTickCount();

			Mat testImage = *image;

			cv::Ptr<aruco::DetectorParameters> parameters;
			cv::Ptr<cv::aruco::Dictionary> markerDictionary = cv::aruco::getPredefinedDictionary(cv::aruco::DICT_ARUCO_ORIGINAL);
			cv::Ptr<cv::aruco::DetectorParameters> detectorParams = cv::aruco::DetectorParameters::create();
			
			detectorParams->cornerRefinementMethod = cv::aruco::CornerRefineMethod::CORNER_REFINE_SUBPIX;
			//detectorParams->doCornerRefinement = true;
			detectorParams->adaptiveThreshWinSizeMin = 140;
			detectorParams->adaptiveThreshWinSizeStep = 30;
			detectorParams->adaptiveThreshWinSizeMax = 230;
			detectorParams->minMarkerPerimeterRate = 0.08;
			detectorParams->minMarkerDistanceRate = 0.005;
			detectorParams->cornerRefinementWinSize = 6;
			detectorParams->cornerRefinementMinAccuracy = 0.08;
			detectorParams->cornerRefinementMaxIterations = 50;
			detectorParams->polygonalApproxAccuracyRate = 0.05;

			std::vector< std::vector<cv::Point2f> > markerCorners;
			std::vector< std::vector<cv::Point2f> > rejected;
			std::vector<int> markerIDs;

			cv::aruco::detectMarkers(testImage, markerDictionary, markerCorners, markerIDs, detectorParams, rejected);
			cv::aruco::drawDetectedMarkers(testImage, markerCorners, markerIDs);



			if (!System::String::IsNullOrEmpty(detectedFile))
				cv::imwrite(msclr::interop::marshal_as<std::string>(detectedFile), testImage);

			List<ArucoMarker^>^ markerList = gcnew List<ArucoMarker^>();
			int sz = markerCorners.size();
			for (size_t i = 0; i < markerCorners.size(); i++)
			{
				bool cont = false;
				for each (ArucoMarker^ m in markerList)
				{
					if (m->_id == markerIDs[i])
						cont = true;
				}
				if (cont){
					continue;
				}
				System::Drawing::PointF^ corner1 = gcnew System::Drawing::PointF(markerCorners[i][0].x, markerCorners[i][0].y);
				System::Drawing::PointF^ corner2 = gcnew System::Drawing::PointF(markerCorners[i][1].x, markerCorners[i][1].y);
				System::Drawing::PointF^ corner3 = gcnew System::Drawing::PointF(markerCorners[i][2].x, markerCorners[i][2].y);
				System::Drawing::PointF^ corner4 = gcnew System::Drawing::PointF(markerCorners[i][3].x, markerCorners[i][3].y);
				markerList->Add(gcnew ArucoMarker(corner1, corner2, corner3, corner4, markerIDs[i]));
			}

			int timelapse = GetTickCount() - start;
			return markerList;
		}
		
	};
}

