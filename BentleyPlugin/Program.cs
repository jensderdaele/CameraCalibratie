using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Bentley.Interop;
//using Bentley.Interop.MicroStationDGN;
using MicroStationDGN;

namespace BentleyPlugin {
    class Program {
        static void Main(string[] args) {
            var appclass = new MicroStationDGN.Application();
            var commandstate = appclass.CommandState;
            
            Point3d testpoint = new Point3d() {
                X = 0,
                Y = 0,
                Z = 0
            };
        }
    }
    
}
