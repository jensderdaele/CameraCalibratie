using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Menus;
using Gemini.Framework.ToolBars;

namespace BundleAdjuster.Shell
{
    public static class GeminiExcludes
    {
        [Export]
        public static ExcludeMenuItemDefinition ExcludeOpenMenuItem = new ExcludeMenuItemDefinition(Gemini.Modules.Shell.MenuDefinitions.FileOpenMenuItem);

        [Export]
        public static ExcludeToolBarItemGroupDefinition ExcludeUndoToolBarDefinition = new ExcludeToolBarItemGroupDefinition(Gemini.Modules.UndoRedo.ToolBarDefinitions.StandardUndoRedoToolBarGroup);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeEditRedoMenuItem = new ExcludeMenuItemDefinition(Gemini.Modules.UndoRedo.MenuDefinitions.EditRedoMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeEditUndoMenuItem = new ExcludeMenuItemDefinition(Gemini.Modules.UndoRedo.MenuDefinitions.EditUndoMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewHistoryMenuItem = new ExcludeMenuItemDefinition(Gemini.Modules.UndoRedo.MenuDefinitions.ViewHistoryMenuItem);
    }
}
