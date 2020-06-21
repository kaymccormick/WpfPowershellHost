using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Renci.SshNet;

namespace WpfPowerShellTerminal
{
    public class SshFunctionality
    {
        public SshFunctionality()
        {

            string privateKeyLocalFilePath;
            // var cTempKey = @"c:\temp\key";
            // var key = File.ReadAllText(cTempKey);
            // var buf = new MemoryStream(Encoding.UTF8.GetBytes(key));
            
            // PrivateKeyFile x = new PrivateKeyFile(buf
            // );
            // SshClient client = new SshClient("127.0.0.1", "test", "poop");
            // client.Connect();
            Stream input = new MemoryStream();
            Stream output = new MemoryStream();
            Stream extendedoutput = new MemoryStream();
            // var shell = client.CreateShell(input, output, extendedoutput);
        }


        // string privateKeyLocalFilePath;
            // var cTempKey = @"c:\temp\key";
            // var key = File.ReadAllText(cTempKey);
            // var buf = new MemoryStream(Encoding.UTF8.GetBytes(key));
            
            // PrivateKeyFile x = new PrivateKeyFile(buf);
            // SshClient client = new SshClient("127.0.0.1", "test", "poop");
            // client.Connect();
            // Stream input = new MemoryStream();
            // Stream output = new MemoryStream();
            // Stream extendedoutput = new MemoryStream();
            // var shell = client.CreateShell(input, output, extendedoutput);
        // }
    }
}
