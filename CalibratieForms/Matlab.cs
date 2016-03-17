using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design.WebControls;
using MathNet.Numerics.LinearAlgebra.Complex;
using OpenTK;

using MathWorks.MATLAB.NET.Arrays;

namespace CalibratieForms {
    public static class Matlab {
        public static MLApp.MLApp ML;

        static Matlab() {
            ML = new MLApp.MLApp();
        }
        private static int count = 0;
        private static string workspacename = "workspace";
        public static void testScatterPlot(Vector2[] data) {
            count++;
            string dataName = "testScatterPlot" + count;

            ML.PutWorkspaceData("test"+count, workspacename, count*3);
            double[,] d = new double[data.Length, 2];
            for (int i = 0; i < data.Length; i++) {
                d[i, 0] = data[i].X;
                d[i, 1] = data[i].Y;
                
            }
            ML.PutWorkspaceData(dataName, workspacename, d);
            //ML.PutFullMatrix("testScatterPlotMATRIX" + count, workspacename, d, d);
            
            ML.Execute(String.Format(@" x={0}[0];
                                        y={0}[1];
                                        scatter(x,y)", dataName));
        }
    }
}
