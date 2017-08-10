#pragma once

#include <opencv2/opencv.hpp>
#include <opencv2/xfeatures2d/nonfree.hpp>
#include <iostream>
#include <vector>
#include <cmath>
#include <msclr\marshal_cppstd.h>

using namespace std;
using namespace cv;
using namespace cv::xfeatures2d;

double euclidDistance(Mat& vec1, Mat& vec2) {
	double sum = 0.0;
	int dim = vec1.cols;
	for (int i = 0; i < dim; i++) {
		sum += (vec1.at<uchar>(0, i) - vec2.at<uchar>(0, i)) * (vec1.at<uchar>(0, i) - vec2.at<uchar>(0, i));
	}
	return sqrt(sum);
}

/**
* Find the index of nearest neighbor point from keypoints.
*/
int nearestNeighbor(Mat& vec, vector<KeyPoint>& keypoints, Mat& descriptors) {
	int neighbor = -1;
	double minDist = 1e6;

	for (int i = 0; i < descriptors.rows; i++) {
		KeyPoint pt = keypoints[i];
		Mat v = descriptors.row(i);
		double d = euclidDistance(vec, v);
		//printf("%d %f\n", v.cols, d);
		if (d < minDist) {
			minDist = d;
			neighbor = i;
		}
	}

	if (minDist < 400) {
		return neighbor;
	}

	return -1;
}

/**
* Find pairs of points with the smallest distace between them
*/
void findPairs(vector<KeyPoint>& keypoints1, Mat& descriptors1,
	vector<KeyPoint>& keypoints2, Mat& descriptors2,
	vector<Point2f>& srcPoints, vector<Point2f>& dstPoints) {
	for (int i = 0; i < descriptors1.rows; i++) {
		KeyPoint pt1 = keypoints1[i];
		Mat desc1 = descriptors1.row(i);
		int nn = nearestNeighbor(desc1, keypoints2, descriptors2);
		if (nn >= 0) {
			KeyPoint pt2 = keypoints2[nn];
			srcPoints.push_back(pt1.pt);
			dstPoints.push_back(pt2.pt);
		}
	}
}
namespace Featuress{
	static ref class SIFT
	{
	public:
		static void detectSift(System::String^ imagefile1, System::String^ imagefile2){
			std::string im1s = msclr::interop::marshal_as<std::string>(imagefile1);
			std::string im2s = msclr::interop::marshal_as<std::string>(imagefile2);
			//initialize detector and extractor
			FeatureDetector* detector;
			detector = new SiftFeatureDetector(
				/*0, // nFeatures
				4, // nOctaveLayers
				0.04, // contrastThreshold
				10, //edgeThreshold
				1.6 //sigma*/
				);

			/*
			DescriptorExtractor* extractor;
			extractor = new SiftDescriptorExtractor();


			vector<KeyPoint> keypoints1;
			Mat descriptors1;

			vector<KeyPoint> keypoints2;
			Mat descriptors2;

			Mat im1g = imread(im1s, CV_LOAD_IMAGE_GRAYSCALE);
			Mat im1c = imread(im1s, CV_LOAD_IMAGE_ANYCOLOR | CV_LOAD_IMAGE_ANYDEPTH);

			Mat im2g = imread(im2s, CV_LOAD_IMAGE_GRAYSCALE);
			Mat im2c = imread(im2s, CV_LOAD_IMAGE_ANYCOLOR | CV_LOAD_IMAGE_ANYDEPTH);


			detector->detect(im1g, keypoints2);
			extractor->compute(im1g, keypoints2, descriptors2);
			printf("original image:%d keypoints are found.\n", (int)keypoints2.size());

			vector<KeyPoint> keypoints1;
			Mat descriptors1;
			vector<DMatch> matches;

			Mat grayFrame(size, CV_8UC1);

			// Detect keypoints
			detector->detect(grayFrame, keypoints1);
			extractor->compute(grayFrame, keypoints1, descriptors1);
			printf("image1:%zd keypoints are found.\n", keypoints1.size());

			// Create a image for displaying mathing keypoints
			Size sz = Size(im2c.size().width + im1c.size().width, max(im1c.size().height, im2c.size().height));
			Mat matchingImage = Mat::zeros(sz, CV_8UC3);

			for (int i = 0; i<keypoints1.size(); i++){
			KeyPoint kp = keypoints1[i];
			circle(matchingImage, kp.pt, cvRound(kp.size*0.25), Scalar(255, 255, 0), 1, 8, 0);
			}


			// Find nearest neighbor pairs
			vector<Point2f> srcPoints;
			vector<Point2f> dstPoints;
			findPairs(keypoints1, descriptors1, keypoints2, descriptors2, srcPoints, dstPoints);
			printf("%zd keypoints are matched.\n", srcPoints.size());

			char text[256];
			sprintf(text, "%zd/%zd keypoints matched.", srcPoints.size(), keypoints2.size());
			putText(matchingImage, text, Point(0, cvRound(size.height + 30)), FONT_HERSHEY_SCRIPT_SIMPLEX, 1, Scalar(0, 0, 255));

			// Draw line between nearest neighbor pairs
			for (int i = 0; i < (int)srcPoints.size(); ++i) {
			Point2f pt1 = srcPoints[i];
			Point2f pt2 = dstPoints[i];
			Point2f from = pt1;
			Point2f to = Point(size.width + pt2.x, size.height + pt2.y);
			line(matchingImage, from, to, Scalar(0, 255, 255));
			}

			// Display mathing image
			imshow("matches", matchingImage);*/
		}
	};
}

