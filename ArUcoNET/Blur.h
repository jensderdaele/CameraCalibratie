#pragma once
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2\core.hpp>

#include "stdafx.h"
#include <iostream>
using namespace std;
//#include <msclr\marshal_cppstd.h>

#include "blur_detect_haar_wavelet.h"

using namespace Emgu;

#pragma managed
static public ref class Blur
{
public:
	static float BlurDetect(Emgu::CV::Mat^ image, int edgeThresh){

		cv::Mat img = *((cv::Mat*)image->Ptr.ToPointer());//cv::imread(argv[1], 0);
		float conf = 0;



		//cv::resize(img, img, cv::Size(FIXED_SIZE, FIXED_SIZE));

		int rows = img.rows;
		int cols = img.cols;

		HuMat src;

		create_humat(src, cols, rows);

		uchar *ptrImg = img.data;
		int *ptrSrc = src.data;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < cols; x++)
				ptrSrc[x] = ptrImg[x];

			ptrSrc += src.stride;
			ptrImg += img.step;
		}

		float confidence = 0;

		int ret = blur_detect(src, &confidence, edgeThresh);

		free_humat(src);

		return confidence;
	}

};







