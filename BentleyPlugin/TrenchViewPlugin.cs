using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bentley.MicroStation;
using Bentley.MicroStation.InteropServices;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace BentleyPlugin
{
    [AddIn(KeyinTree = "Commands.xml", MdlTaskID = "Photogrammetry")] //ApplicationType = MdlApplicationType.AutoDgn
    public class TrenchViewPlugin : AddIn {


        public static TrenchViewPlugin Plugin;
        internal static TrenchViewPlugin App { get; private set; }

        /// <summary>
        /// A Reference to the host applications Application object
        /// </summary>
        internal BCOM.Application Context { get; private set; }

        public BCOM.View Designview { get; set; }



        /// <summary>
        /// Creates the instance of the addin
        /// </summary>
        /// <remarks>
        /// Constructor is made private as we only want one instance that will 
        /// be created by the AddIn loader</remarks>
        /// <param name="mdlDescriptor"></param>
        private TrenchViewPlugin(IntPtr mdlDescriptor) : base(mdlDescriptor) {
            App = this;
            //Context.CadInputQueue.SendDataPoint(new BCOM.Point3d());
        }
        /// <summary>
        /// Initializes the AddIn
        /// </summary>
        /// <remarks>
        /// Called by the AddIn loader after it has created the instance
        /// of this AddIn class
        /// </remarks>
        /// <param name="commandLine">Command line arguments</param>
        /// <returns>0 on success</returns>
        protected override int Run(string[] commandLine) {
            Plugin = this;
            // Get a reference to the host MicroStation application
            Context = Utilities.ComApp;
            // Register event handlers

            foreach (BCOM.View view in Context.ActiveDesignFile.Views) {
                if (view.IsSelected) {
                    Designview = view;
                    break;
                }
            }
            

            ReloadEvent += AssemblyDataManager_ReloadEvent;
            UnloadedEvent += AssemblyDataManager_UnloadedEvent;

            return 0;
        }
        private void AssemblyDataManager_ReloadEvent(AddIn sender, AddIn.ReloadEventArgs eventArgs)
        {

        }
        protected void AssemblyDataManager_UnloadedEvent(AddIn sender, AddIn.UnloadedEventArgs eventArgs)
        {

        }
    }
}
