using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;

namespace BundleAdjuster.IterationsView.Commands
{
    [CommandDefinition]
    class LoadIterationCommandDefinition : CommandDefinition 
    {
        public const string CommandName = "File.OpenFile";

        public override string Name {
            get { return CommandName; }
        }

        public override string Text {
            get { return "_LoadIteration"; }
        }

        public override string ToolTip {
            get { return "Load Iteration"; }
        }

    }
}
