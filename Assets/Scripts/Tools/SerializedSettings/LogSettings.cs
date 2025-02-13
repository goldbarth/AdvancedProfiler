using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.SerializedSettings
{
    public class LogSettings : BaseSettings
    {
        [LabelText("Log Level")]
        public string LogLevel = "Info";
        
        [LabelText("Detailed Logs active")]
        public bool DetailedLogs = false;
        
        [LabelText("Log to File")]
        public bool LogToFile = false;
        
        [LabelText("Log File Path")]
        public string LogFilePath = "Logs/";
        
        [LabelText("Log File Name")]
        public string LogFileName = "log.txt";
        
        [LabelText("Log File Extension")]
        public string LogFileExtension = ".txt";
        
        [LabelText("Log File Max Size")]
        public int LogFileMaxSize = 1024;
        
        [LabelText("Log File Max Count")]
        public int LogFileMaxCount = 5;
        
        [LabelText("Log File Auto Flush")]
        public bool LogFileAutoFlush = true;
    }
}