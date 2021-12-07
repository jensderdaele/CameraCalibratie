using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;

using Bentley.MicroStation.InteropServices;
using BCOM = Bentley.Interop.MicroStationDGN;
using Bentley.MicroStation;

namespace BentleyPlugin
{

    public delegate void gotmessage(string message, ConnectedClient client);
    public delegate void disconnected(ConnectedClient client);
    
    public class ConnectedClient {
        private TcpClient _cli;
        private string _uniqueId;

        public event gotmessage GotMessage;
        public event disconnected Disconnected;

        public string Name {
            get => _uniqueId;
            set => _uniqueId = value;
        }
        
        public ConnectedClient(TcpClient client) {
            
            var r = new Random();
            var x = "";
            for (int i = 0; i < 7; i++) {
                x += (char)r.Next(65, 89);
            }

            Name = client.Client.RemoteEndPoint.ToString()
                       .Remove(client.Client.RemoteEndPoint.ToString().LastIndexOf(":")) + " - " + x;

            _cli = client;
            _cli.GetStream().BeginRead(new byte[] {0}, 0, 0, read, null);
        }

        public void read(IAsyncResult ar) {
            try {
                var sr = new StreamReader(_cli.GetStream());
                var msg = sr.ReadLine();
                OnGotMessage(msg, this);
                _cli.GetStream().BeginRead(new byte[] { 0 }, 0, 0, read, null);
            }
            catch (Exception e) {
                try {
                    var sr = new StreamReader(_cli.GetStream());
                    var msg = sr.ReadLine();
                    OnGotMessage(msg, this);
                    _cli.GetStream().BeginRead(new byte[] { 0 }, 0, 0, read, null);
                }
                catch (Exception e2) {
                    OnDisconnected(this);
                }
            }
        }

        public void sendData(string msg) {
            var sw = new StreamWriter(_cli.GetStream());
            sw.Write(msg);
            sw.Flush();
        }

        protected virtual void OnGotMessage(string message, ConnectedClient client) {
            GotMessage?.Invoke(message, client);
        }

        protected virtual void OnDisconnected(ConnectedClient client) {
            Disconnected?.Invoke(client);
        }
    }
}
