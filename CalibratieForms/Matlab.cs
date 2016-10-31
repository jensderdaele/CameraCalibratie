using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design.WebControls;
using MathNet.Numerics.LinearAlgebra.Complex;
using OpenTK;
using MathWorks.MATLAB.NET.Utility;
using MathWorks.MATLAB.NET.Arrays;
using OpenCvSharp;
using MathWorks.MATLAB.NET;



namespace CalibratieForms {
    public static class Matlab {
        //public static MLApp.MLApp ML;

        static Matlab() {
           // ML = new MLApp.MLApp();
            //ML.Execute("workspace");
            
        }

        private static int count = 0;
        private static string workspacename = "base";
        public static void ScatterPlot(Point2d[] data) {
           /* double[,] dataArray = new double[2,data.Length];
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
            ML.PutWorkspaceData(dataName,workspacename,dataArray);
            ML.PutWorkspaceData(dataName + "x", workspacename, xdata);
            ML.PutWorkspaceData(dataName + "y", workspacename, ydata);


            double[,] d = new double[data.Length, 2];
            for (int i = 0; i < data.Length; i++) {
                d[i, 0] = data[i].X;
                d[i, 1] = data[i].Y;
            }
            
            //ML.PutWorkspaceData(dataName, workspacename, d);
            //ML.PutFullMatrix("testScatterPlotMATRIX" + count, workspacename, d, d);
            
            ML.Execute(String.Format(@" clear x;clear y;
                                        x={0}[0];
                                        y={0}[1];
                                        scatter(x,y,'x')", dataName));*/

        }
        public static void ScatterPlot(List<Point2d[]> dataList,string name) {
            /*int index = 1;
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
                ML.PutWorkspaceData(xstr, workspacename, xdata);
                ML.PutWorkspaceData(ystr, workspacename, ydata);
                ML.Execute(String.Format(@"scatter({0},{1},'x'); hold on", xstr, ystr));
                index++;
            }

            */
            
        }

        public static void ScatterPlot(double[,] data) {
            /*count++;
            string dataName = "testScatterPlot" + count;

            ML.PutWorkspaceData("test" + count, workspacename, count * 3);
            //ML.PutFullMatrix("testScatterPlotMATRIX" + count, workspacename, d, d);

            ML.Execute(String.Format(@" x={0}[0];
                                        y={0}[1];
                                        scatter(x,y)", dataName));
            */
        }
    }
}
