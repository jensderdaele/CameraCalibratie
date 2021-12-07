using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Bentley.Interop.MicroStationDGN;
using BentleyPlugin;


using BCOM = Bentley.Interop.MicroStationDGN;

namespace BentleyPlugin
{


    using Microsoft.VisualBasic;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Net;
    using System.IO;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    


public class SocketServerMultipleClients
    {

        
        private struct cameraangledata
        {
            public double X;
            public double Y;
            public double Z;
            public double openingangle;
            public double horizontalrotation;
        }

        //new database (hashtable) to hold the clients
        //HashSet<ConnectedClient> clients = new Hashtable();
        HashSet<ConnectedClient> clients = new HashSet<ConnectedClient>();
        //Bentley.Windowing
        //private WindowsFormsSynchronizationContext uiContext = new WindowsFormsSynchronizationContext();
        //Private uiContext As WindowsFormsSynchronizationContext = SynchronizationContext.Current

        public void recieved(string msg, ConnectedClient client){
            string[] message = msg.Split(' ');
            //make an array with elements of the message recieved
            double X = 0;
            double Y = 0;
            double Z = 0;
            var context = TrenchViewPlugin.Plugin.Context;
            //Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;
            string command = message[0];
            Log.WriteLine("I got message " + msg);
            //uiContext.Post(EditorWriteMessage("I got message " & msg))
            //uiContext.Post(ed.WriteMessage("I got message " & msg))
            //System.Windows.Forms.Application.DoEvents()


            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            switch (message[0])
            {
                //process by the first element in the array

                case "login":
                    //A client has connected
                    clients.Add(client);
                    //add the client to our database (a hashtable)
                    //ed.WriteMessage("I got a new client: " & client.name)
                    Log.WriteLine( "I got a new client " + client.Name);
                    this.sendsingle("Logged in", client.Name);
                    break;
                //ListBox1.Items.Add(client.name) 'add the client to the listbox to display the new user
                case "pointcoords":
                    // First check if we have enough left in the string
                    string reststring1 = msg.Substring(command.Length);
                    if ((reststring1.Length == 0))
                    {
                        this.sendsingle("expected 3 coordinates, but got none", client.Name);
                        return;
                    }
                    string[] values1 = msg.Substring(command.Length + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    CultureInfo culture1 = CultureInfo.CreateSpecificCulture("en-US");
                    if ((values1.Length == 3))
                    {

                        //how about snapping?
                        X = double.Parse(values1[0], culture);
                        Y = double.Parse(values1[1], culture);
                        Z = double.Parse(values1[2], culture);

                        Point3d point = new Point3d() {
                            X = X,
                            Y = Y,
                            Z = Z
                        };
                        Log.WriteLine($"start SendDataPoint for: {point.X} {point.Y} {point.Z}");
                        try {
                            TrenchViewPlugin.Plugin.Context.CadInputQueue.SendDataPoint(ref point,
                                TrenchViewPlugin.Plugin.Designview);
                        }
                        catch(Exception e) {

                            Log.WriteLine(string.Format("SendDataPoint failed: {0} ", e.Message));
                        }
                        //ed.WriteMessage("#" & X & "," & Y & "," & Z) */
                        var p = point;
                        string cmd = string.Format(CultureInfo.InvariantCulture, "gotpoint: ({0} {1} {2}) ", p.X, p.Y, p.Z);
                        Log.WriteLine(cmd);
                        this.sendsingle("listhowdy " + X + " " + Y + " " + Z, client.Name);
                    }
                    else
                    {
                        this.sendsingle("Expected 3 coordinates and got " + values1.Length, client.Name);
                    }
                    break;
                case "temppointcoords":
                    // First check if we have enough left in the string
                    string reststring2 = msg.Substring(command.Length);
                    if ((reststring2.Length == 0))
                    {
                        this.sendsingle("expected 3 coordinates, but got none", client.Name);
                        return;
                    }
                    string[] values2 = msg.Substring(command.Length + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if ((values2.Length == 3))
                    {
                        X = double.Parse(values2[0], culture);
                        Y = double.Parse(values2[1], culture);
                        Z = double.Parse(values2[2], culture);
                    }
                    //uiContext.Send(UIDrawCircles, point);

                    this.sendsingle("Drew circles", client.Name);

                    break;
                case "cameraview":
                    // First check if we have enough left in the string
                    string reststring = msg.Substring(command.Length);
                    //Me.sendsingle("You sent cameraview with " & command.Length & " rest", client.name)
                    if ((reststring.Length == 0))
                    {
                        this.sendsingle("expected 3 coordinates, but got none", client.Name);
                    }
                    double openingangle = 10;
                    double horizontalrotation = 0;
                    string[] values = msg.Substring(command.Length + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    cameraangledata cad = new cameraangledata();
                    if ((values.Length == 5))
                    {
                        X = double.Parse(values[0], culture);
                        Y = double.Parse(values[1], culture);
                        Z = double.Parse(values[2], culture);

                        cad.openingangle = double.Parse(values[3], culture);
                        cad.horizontalrotation = double.Parse(values[4], culture);
                    }
                    else
                    {
                        this.sendsingle("Expected 5 doubles and got " + values.Length, client.Name);
                        return;
                    }
                    Log.WriteLine("Having to draw camera angle " + cad.X + " " + cad.horizontalrotation);
                    //uiContext.Send(UIDrawCameraAngle, cad);
                    this.sendsingle("Got cameraview: ", client.Name);
                    break;
                default:
                    this.sendsingle("command not recognized", client.Name);

                    break;
            }

        }
        //this sends to all clients except the one specified
        public void sendallbutone(string message, string exemptclientname)
        {
            //declare a variable of type dictionary entry
            try
            {
                //for each dictionary entry in the hashtable with all clients (clients)
                foreach (var entry in clients)
                {
                    //if the entry IS NOT the exempt client name
                    if (entry.Name != exemptclientname)
                    {
                        // cast the hashtable entry to a connection class
                        entry.sendData(message);
                        //send the message to it
                    }
                }
            }
            catch
            {
            }
        }
        public void sendsingle(string message, string clientname)
        {
            //declare a variable of type dictionary entry
            try
            {
                //for each dictionary entry in the hashtable with all clients (clients)
                foreach (var entry in clients)
                {
                    //if the entry is belongs to the client specified
                    if (entry.Name == clientname)
                    {
                        entry.sendData(message);
                        //send the message to it
                        Log.WriteLine(string.Format("sendsingle - {0}: {1}", clientname, message));
                    }
                }
            }
            catch
            {
            }

        }
        //this sends a message to all connected clients
        public void senddata(string message)
        {
            try
            {
                //for each dictionary entry in the hashtable with all clients (clients)
                foreach (var entry in clients)
                {
                    // cast the hashtable entry to a connection class
                    entry.sendData(message);
                    //send the message to it
                }
                //go to the next client
            }
            catch
            {
            }

        }
        //if a client is disconnected, this is raised
        public void disconnected(ConnectedClient client)
        {
            clients.Remove(client);
            //remove the client from the hashtable
            //ListBox1.Items.Remove(client.name) 'remove it from our listbox
        }
        public void listen(int port)
        {
            try
            {
                TcpListener t = new TcpListener(IPAddress.Any, port);
                //declare a new tcplistener
                t.Start();
                //start the listener
                do
                {
                    
                    ConnectedClient client = new ConnectedClient(t.AcceptTcpClient());
                    //initialize a new connected client
                    client.GotMessage += recieved;
                    //add the handler which will raise an event when a message is recieved
                    client.Disconnected += disconnected;
                    //add the handler which will raise an event when the client disconnects

                } while (!(false));
            }
            catch
            {
            }

        }
        public void start(int port)
        {
            //this.uiContext = SynchronizationContext.Current;
            //SynchronizationContext.Current
            System.Threading.Thread listener = new System.Threading.Thread(() => listen(port));
            //initialize a new thread for the listener so our GUI doesn't lag
            listener.IsBackground = true;
            listener.Start();
            //start the listener, with the port specified as a parameter (textbox1 is our port textbox)
            //Button1.Enabled = False 'disable our button so the user cannot try to make any further listeners which will result in errors
        }

        [Obsolete("NOT IMPLEMENTED")]
        private void EditorWriteMessage(object s)
        {
           /* Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;
            string ss = s;
            ed.WriteMessage(ss);*/
        }

        [Obsolete("NOT IMPLEMENTED")]
        private void UIDrawCircles(object s)
        {
            
           /* Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;

            object names = Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("CMDNAMES");
            string strText = names;
            if ((strText.Length == 0))
            {
                Autodesk.AutoCAD.Geometry.Point3d center = s;
                Autodesk.AutoCAD.Geometry.Point3d[][] circles = new Autodesk.AutoCAD.Geometry.Point3d[][];
                GenerateCircles(center, ref circles);
                for (int i = 0; i <= circles.Length - 1; i++)
                {
                    //Dim resbuf As ResultBuffer = New ResultBuffer
                    //resbuf.Add(New TypedValue(Autodesk.AutoCAD.Runtime.LispDataType.Int16, 1))
                    for (int j = 1; j <= circles(i).Length - 1; j++)
                    {
                        //resbuf.Add(New TypedValue(Autodesk.AutoCAD.Runtime.LispDataType.Point3d, circles(i)(j - 1)))
                        //resbuf.Add(New TypedValue(Autodesk.AutoCAD.Runtime.LispDataType.Point3d, circles(i)(j)))
                        //ed.WriteMessage("Circle " & i & " point " & j & " is " & circles(i)(j).ToString)
                        ed.DrawVector(circles(i)(j - 1), circles(i)(j), 1, false);
                    }
                    //ed.DrawVectors(resbuf, Autodesk.AutoCAD.Geometry.Matrix3d.Identity)
                }
            }*/
        }

        [Obsolete("NOT IMPLEMENTED")]
        private void UIUseCoordinates(object s)
        {
            /*Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;
            Autodesk.AutoCAD.Geometry.Point3d p = s;
            string cmd = string.Format(CultureInfo.InvariantCulture, "(list {0} {1} {2}) ", p.X, p.Y, p.Z);
            doc.SendStringToExecute(cmd, true, false, true);
            ed.WriteMessage("#" & X & "," & Y & "," & Z)*/
        }

        [Obsolete("NOT IMPLEMENTED")]
        private void UIDrawCameraAngle(object s)
        {
            /*Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;
            cameraangledata cad = s;
            // center line = from XYZ, 10m towards horizontalrotation
            // left line = openingangle/2 left
            // right line = openingangle/2 right
            //We only update the view if there is no active command
            object names = Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("CMDNAMES");
            string strText = names;
            ed.WriteMessage("DrawCameraAngle " + cad.X + " " + cad.openingangle);
            if ((strText.Length == 0))
            {
                ed.WriteMessage("No command is running\\n");
                Autodesk.AutoCAD.Geometry.Point3d center = new Autodesk.AutoCAD.Geometry.Point3d(cad.X, cad.Y, cad.Z);
                double Xl = 0;
                double Yl = 0;
                double Xr = 0;
                double Yr = 0;
                double radius = 10;
                Xl = cad.X + radius * Math.Cos((cad.horizontalrotation + cad.openingangle / 2) * Math.PI / 180f);
                Yl = cad.Y + radius * Math.Sin((cad.horizontalrotation + cad.openingangle / 2) * Math.PI / 180f);
                Xr = cad.X + radius * Math.Cos((cad.horizontalrotation - cad.openingangle / 2) * Math.PI / 180f);
                Yr = cad.Y + radius * Math.Sin((cad.horizontalrotation - cad.openingangle / 2) * Math.PI / 180f);

                //Move to center
                ViewTableRecord vtr = ed.GetCurrentView.Clone;
                Autodesk.AutoCAD.Geometry.Point3d currenttarget = vtr.Target;
                vtr.Target = new Autodesk.AutoCAD.Geometry.Point3d(center.X, center.Y, currenttarget.Z);
                vtr.CenterPoint = new Autodesk.AutoCAD.Geometry.Point2d(0, 0);
                ed.SetCurrentView(vtr);

                //Draw line
                Autodesk.AutoCAD.Geometry.Point3d Left = new Autodesk.AutoCAD.Geometry.Point3d(Xl, Yl, cad.Z);
                Autodesk.AutoCAD.Geometry.Point3d right = new Autodesk.AutoCAD.Geometry.Point3d(Xr, Yr, cad.Z);
                ed.DrawVector(Left, center, 1, false);
                ed.DrawVector(right, center, 1, false);
            }*/
        }

        [Obsolete("NOT IMPLEMENTED")]
        private static void GenerateCircles()//Autodesk.AutoCAD.Geometry.Point3d center, ref Autodesk.AutoCAD.Geometry.Point3d[][] result)
        {
            /*result = new Autodesk.AutoCAD.Geometry.Point3d[4][];
            result(0) = new Autodesk.AutoCAD.Geometry.Point3d[37];
            int index = 0;
            foreach (double radius in {
                0.05,
			0.15,
			0.3,
			0.5
    
        }) {
                result(index) = new Autodesk.AutoCAD.Geometry.Point3d[37];
                for (int i = 0; i <= 36; i++)
                {
                    double[] xyz = new double[4];
                    xyz(2) = center(2);
                    xyz(0) = center(0) + radius * Math.Cos(i * 10 * Math.PI / 180f);
                    xyz(1) = center(1) + radius * Math.Sin(i * 10 * Math.PI / 180f);
                    result(index)(i) = new Autodesk.AutoCAD.Geometry.Point3d(xyz);
                }
                index += 1;
            }*/
        }


    }


    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================


}

