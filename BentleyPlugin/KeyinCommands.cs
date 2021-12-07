using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BentleyPlugin
{
    /// <summary>Class used for running key-ins.  The key-ins
    /// XML file commands.xml provides the class name and the method names.
    /// </summary>
    internal class KeyinCommands
    {
        public static void ShowCommand(String unparsed)
        {
            Form1.ShowForm(TrenchViewPlugin.Plugin);
        }
        
        public static void ConnectCommand(String unparsed) {
            Form1.s_current._myServer.start(11000);
        }
    }  // End of KeyinCommands
}
