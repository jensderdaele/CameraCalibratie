using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using ceresdotnet;
using Calibratie;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Modules.Inspector;
using Gemini.Modules.Output;
using Gemini.Modules.PropertyGrid;
using Gemini.Modules.PropertyGrid.ViewModels;

namespace BundleAdjuster.Startup
{
    
    [Export(typeof(IModule))]
    public class Module : ModuleBase
    {
        private readonly IOutput _output;
        private readonly IInspectorTool _inspectorTool;

        [Import]
        private IPropertyGrid _propertyGrid;


        public override IEnumerable<Type> DefaultTools {
            get { yield return typeof(IInspectorTool); }
       }

        [ImportingConstructor]
        public Module(IOutput output, IInspectorTool inspectorTool)
        {
            _output = output;
            _inspectorTool = inspectorTool;
        }

        public override void Initialize()
        {
            Shell.ShowFloatingWindowsInTaskbar = true;
            /*Shell.ToolBars.Visible = true;

            MainWindow.WindowState = WindowState.Maximized;
            MainWindow.Title = "Bundle Adjuster";

            Shell.StatusBar.AddItem("Bundle Adjuster - testing", new GridLength(1, GridUnitType.Star));
            //Shell.StatusBar.AddItem("", new GridLength(100));
            //Shell.StatusBar.AddItem("Col 79", new GridLength(100));

            _output.AppendLine("Started up");

            Shell.ActiveDocumentChanged += (sender, e) => RefreshInspector();
            var output = IoC.Get<IOutput>();
            Shell.ShowTool(output);
            RefreshInspector();
            
            _propertyGrid.SelectedObject = BP.SolverOptions;
            Shell.ShowTool(_propertyGrid);*/
            
        }

        private void RefreshInspector()
        {
            
            if (Shell.ActiveItem != null)
                _inspectorTool.SelectedObject = new InspectableObjectBuilder()
                    .WithObjectProperties(Shell.ActiveItem, pd => pd.ComponentType == Shell.ActiveItem.GetType())
                    .ToInspectableObject();
            else
                _inspectorTool.SelectedObject = null;
        }
    }
    
}
