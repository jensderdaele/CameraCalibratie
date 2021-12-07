using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.GCPView.ViewModels;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;



namespace BundleAdjuster.GCPView
{
    [Export(typeof(IModule))]
    public class Module : ModuleBase
    {
        [Export]
        public static MenuItemGroupDefinition ViewDemoMenuGroup = new MenuItemGroupDefinition(
            Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 10);

        public override void PostInitialize()
        {
            var gcpvm = IoC.Get<GCPViewModel>();
            Shell.ShowTool(gcpvm);
            
        }
    }
}
