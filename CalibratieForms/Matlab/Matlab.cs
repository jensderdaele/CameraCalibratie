﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design.WebControls;
using Calibratie;
using CalibratieForms.Properties;
using Emgu.CV;
using MathNet.Numerics.LinearAlgebra.Complex;
using OpenTK;

using Point2d = Emgu.CV.Structure.MCvPoint2D64f;


namespace CalibratieForms {

    public class MatlabResource<T> {
        
    }
    public static class Matlab {
        
        public static MLApp.MLApp ML;
        private static string byteToString(byte[] data) {
            return System.Text.Encoding.ASCII.GetString(data);
            
        }
        public static void VisualizeDistortions(PinholeCamera camera) {
            var script = byteToString(Resources.visualize_distortions);
            const string fx = "%fx%";

        }

        static Matlab() {
            ML = new MLApp.MLApp();
           ML.Execute("workspace");
            
        }

        public static void ViewReprojectionError(List<PointF> error) {
            
        }

        private static int count = 0;
        public const string WORKSPACENAME = "base";

        public static void SendMatrix(Matrix<double> mat, string matlabname, string workspacename = WORKSPACENAME) {
            ML.PutWorkspaceData(matlabname, workspacename, mat.Data);
        }
        public static double[,] GetMatrix(string matlabname, string workspacename = WORKSPACENAME) {
            ML.GetWorkspaceData(matlabname, workspacename,out object r);
            return (double[,]) r;
        }


        public static void ScatterPlot(Point2d[] data) {
            double[,] dataArray = new double[2,data.Length];
            double[] xdata = new double[data.Length];
            double[] ydata = new double[data.Length];
            for (int i = 0; i < data.Length; i++) {
                dataArray[0, i] = data[i].X;
                dataArray[1, i] = data[i].Y;
                xdata[i] = data[i].X;
                ydata[i] = data[i].Y;

            }
            
            count++;
            string dataName = "scatterPlot" + count;
            ML.PutWorkspaceData(dataName,WORKSPACENAME,dataArray);
            ML.PutWorkspaceData(dataName + "x", WORKSPACENAME, xdata);
            ML.PutWorkspaceData(dataName + "y", WORKSPACENAME, ydata);


            double[,] d = new double[data.Length, 2];
            for (int i = 0; i < data.Length; i++) {
                d[i, 0] = data[i].X;
                d[i, 1] = data[i].Y;
            }
            
            ML.PutWorkspaceData(dataName, WORKSPACENAME, d);
            ML.PutFullMatrix("testScatterPlotMATRIX" + count, WORKSPACENAME, d, d);
            
            ML.Execute(String.Format(@" clear x;clear y;
                                        x={0}[0];
                                        y={0}[1];
                                        scatter(x,y,'x')", dataName));

        }
        public static void ScatterPlot(List<Point2d[]> dataList,string name) {
            int index = 1;
            string dataName = String.Format("scatter_{0}", name);
            ML.Execute(String.Format(@"figure('{0}','HOLD ON approach')", name));
            
            foreach (var data in dataList) {
                double[,] dataArray = new double[2, data.Length];
                double[] xdata = new double[data.Length];
                double[] ydata = new double[data.Length];
                for (int i = 0; i < data.Length; i++) {
                    dataArray[0, i] = data[i].X;
                    dataArray[1, i] = data[i].Y;
                    xdata[i] = data[i].X;
                    ydata[i] = data[i].Y;
                }
                var xstr = dataName + "_x_" + index;
                var ystr = dataName + "_y_" + index;
                ML.PutWorkspaceData(xstr, WORKSPACENAME, xdata);
                ML.PutWorkspaceData(ystr, WORKSPACENAME, ydata);
                ML.Execute(String.Format(@"scatter({0},{1},'x'); hold on", xstr, ystr));
                index++;
            }

            
            
        }
        public static void ScatterPlot(double[,] data) {
            count++;
            string dataName = "testScatterPlot" + count;

            ML.PutWorkspaceData("test" + count, WORKSPACENAME, count * 3);
            //ML.PutFullMatrix("testScatterPlotMATRIX" + count, workspacename, d, d);

            ML.Execute(String.Format(@" x={0}[0];
                                        y={0}[1];
                                        scatter(x,y)", dataName));
            
        }
    }
}
