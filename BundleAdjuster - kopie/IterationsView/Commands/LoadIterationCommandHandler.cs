using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;

namespace BundleAdjuster.IterationsView.Commands
{
    [CommandHandler]
    class LoadIterationCommandHandler : CommandHandlerBase<LoadIterationCommandDefinition>
    {
        public override void Update(Command command) {
            command.Enabled = true;
            // You can enable / disable the command here with:
            // command.Enabled = true;

            // You can also modify the command text / icon, which will affect
            // any menu items or toolbar items bound to this command.
        }

        public override async Task Run(Command command)
        {
            // ... implement command handling here
        }
    }
}
