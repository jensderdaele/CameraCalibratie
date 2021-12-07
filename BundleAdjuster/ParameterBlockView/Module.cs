﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.ParameterBlockView.ViewModels;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;

namespace BundleAdjuster.ParameterBlockView
{
    [Export(typeof(IModule))]
    public class Module : ModuleBase
    {
        [Export]
        public static MenuItemGroupDefinition ViewDemoMenuGroup = new MenuItemGroupDefinition(
            Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 10);

        public override void PostInitialize()
        {
            var gcpvm = IoC.Get<ParameterBlockViewModel>();
            Shell.ShowTool(gcpvm);
        }

    }
}