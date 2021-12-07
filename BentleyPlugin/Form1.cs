using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bentley.Internal.MicroStation.Elements;
using Bentley.Interop.MicroStationDGN;
using BMW = Bentley.MicroStation.WinForms;
namespace BentleyPlugin
{//
    public partial class Form1 : BMW.Adapter, ILog
    {

        public SocketServerMultipleClients _myServer;
        public TrenchViewPlugin m_addIn;
        public Bentley.Windowing.WindowContent m_windowContent;
        public static Form1 s_current;
        public delegate void SetTextCallback(string text);

        public Form1()
        {
            InitializeComponent();

            Log.AddReader(this);
        }
        public Form1(Bentley.MicroStation.AddIn addIn)
        {
            InitializeComponent();

            Log.AddReader(this);
            m_addIn = TrenchViewPlugin.Plugin;
            this.Name = "Photogrammety connection";
            // Add any initialization after the InitializeComponent() call.
            SetTextCallback f = new SetTextCallback(SetLabelText);
            // MyServer_ = New AsyncService(11000, f)
            _myServer = new SocketServerMultipleClients();

        }
        internal static void ShowForm(Bentley.MicroStation.AddIn addIn)
        {
            if (s_current != null) {
                s_current.Show();
                return;
            }

            s_current = new Form1(addIn);
            s_current.AttachAsTopLevelForm(addIn, true);

            s_current.Closing += S_current_Closing;

            s_current.NETDockable = true;
            Bentley.Windowing.WindowManager windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();
            s_current.m_windowContent = windowManager.DockPanel(s_current, s_current.Name, s_current.Name, Bentley.Windowing.DockLocation.Floating);

            
        }

        private static void S_current_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            s_current.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lblStatusText.Text = "Start Clicked";
            KeyinCommands.ConnectCommand("");
        }
        private void SetLabelText(string text)
        {
            if (this.lblStatusText.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetLabelText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblStatusText.Text = text;
            }
        }

        private int i = 0;
        private void button2_Click(object sender, EventArgs e) {
            lblStatusText.Text = "Send Objects clicked";

            var C = TrenchViewPlugin.Plugin.Context;
            var selectedElements = C.ActiveModelReference.GetSelectedElements().BuildArrayFromContents();

            int i = 0;
            foreach (var selectedElement in selectedElements) {
                Log.WriteLine(string.Format("EL{0} - type:{1} class:{2} IsComponentElement: {3} IsLineair:{4} IsValid:{5}", 
                    i++, selectedElement.Type, selectedElement.Class, selectedElement.IsComponentElement, selectedElement.IsLinear, selectedElement.IsValid));
            }
           
        }

        private string BuildStringToSend(IEnumerable<ProjectableData> projectableobjects) {
            var s = "";
            s += projectableobjects.Count() + " ";
            foreach (var po in projectableobjects)
            {
                // Perform desired processing on each item.
                //Dim po As ProjectableData = projectableobjects.Item(Index)
                s += po.color + " ";
                s += po.points.Length + " ";
                for (int i = 0; i <= po.points.Length - 1; i++)
                {
                    s += po.points[i].X + " " + po.points[i].Y + " " + po.points[i].Z + " ";
                }
            }
            return s;
        }



        public void WriteLine(string entry)
        {
            richTextBox1.Invoke((Action)(() => {
                richTextBox1.AppendText(entry + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }));
        }
    }
}
