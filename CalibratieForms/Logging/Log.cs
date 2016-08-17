using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CalibratieForms {
    public static class Log {
        public static bool ToConsole { get; set; }
        private static LinkedList<ILog> LogReaders = new LinkedList<ILog>();
        private static LinkedList<string> LogContent = new LinkedList<string>();

        static Log() {
            var p = Process.GetCurrentProcess();
            p.OutputDataReceived += p_OutputDataReceived;
            p.ErrorDataReceived += p_ErrorDataReceived;
        }

        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            WriteLine("Console error: " + e.Data);
        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            WriteLine("Console output: " + e.Data);
        }

        public static void WriteLine(string text, params object[] args) {
            var entry = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), string.Format(text, args));
            LogContent.AddLast(entry);

            if (ToConsole) Console.WriteLine(entry);
            
            foreach (var LogReader in LogReaders)
                LogReader.WriteLine(entry);
        }

        public static void Error(string info = "", [CallerMemberName]string function = "") {
            WriteLine(function + " : " + info);
        }
        public static void AddReader(ILog LogReader) {
            
            LogReaders.AddLast(LogReader);
            foreach (var LogLines in LogContent)
                LogReader.WriteLine(LogLines);
        }

        public static void RemoveReader(ILog LogReader) {
            LogReaders.Remove(LogReader);
        }

    }
    public interface ILog {
        void WriteLine(string line);
    }
}
