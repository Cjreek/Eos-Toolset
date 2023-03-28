using Eos.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Services
{
    public static class Log
    {
        private const string LogFileName = "eos.log";
        
        private static FileStream logStream;
        private static StreamWriter logWriter;

        static Log()
        {
            Directory.CreateDirectory(Constants.AppDataPath);

            logStream = new FileStream(Constants.AppDataPath + LogFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            logWriter = new StreamWriter(logStream);
        }

        public static void Info(string message, params object?[] args)
        {
            message = string.Format(message, args);
            logWriter.WriteLine($"[INFO]: {message}");
            logWriter.Flush();
        }

        public static void Error(string message, params object?[] args)
        {
            message = string.Format(message, args);
            logWriter.WriteLine($"[ERROR]: {message}");
            logWriter.Flush();
        }

        public static void Warning(string message, params object?[] args)
        {
            message = string.Format(message, args);
            logWriter.WriteLine($"[WARNING]: {message}");
            logWriter.Flush();
        }
    }
}
