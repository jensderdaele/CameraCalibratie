using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;
using OpenCvSharp.XFeatures2D;

namespace CalibratieForms {
    public static class StereoCalc {
        public static void test(string im1path, string im2path, string featureSave) {

            var im1c = Cv2.ImRead(im1path);
            var im1g = Cv2.ImRead(im1path, ImreadModes.GrayScale);

            var im2c = Cv2.ImRead(im2path);
            var im2g = Cv2.ImRead(im2path, ImreadModes.GrayScale);

            
            SIFT sift = SIFT.Create();

            //keypoints detection
            var im1kp = sift.Detect(im1g);
            var im2kp = sift.Detect(im2g);
            
            //cv discriptors
            Mat im1discriptors = new Mat();
            Mat im2discriptors = new Mat();
            sift.Compute(im1g,ref im1kp,im1discriptors);
            sift.Compute(im2g,ref im2kp,im2discriptors);

            //draw kp
            Mat im1_kp_draw = new Mat();
            Cv2.DrawKeypoints(im1g,im1kp,im1_kp_draw);

            im1_kp_draw.SaveImage(featureSave);

            //Matching
            var flann = new FlannBasedMatcher();
            var bruteforce = new BFMatcher();

            var matches = flann.KnnMatch(im1discriptors, im2discriptors, 2);
            
        }

    }
}
