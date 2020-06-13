using System;
using System.Collections.Generic;

namespace Terminal1
{
    public class MyInformationRecord
    {
        public MyInformationRecord(object messageData, string source, DateTime timeGenerated, List<string> tags,
            string user, string computer, uint processId, uint nativeThreadId, uint managedThreadId)
        {
            MessageData = messageData;
            Source = source;
            TimeGenerated = timeGenerated;
            Tags = tags;
            User = user;
            Computer = computer;
            ProcessId = processId;
            NativeThreadId = nativeThreadId;
            ManagedThreadId = managedThreadId;
        }

        public object MessageData { get; set; }

        public string Source { get; set; }

        public DateTime TimeGenerated { get; set; }

        public List<string> Tags { get; set; }

        public string User { get; set; }

        public string Computer { get; set; }

        public uint ProcessId { get; set; }

        public uint NativeThreadId { get; set; }

        public uint ManagedThreadId { get; set; }
    }
}