/* TcpPlugin.cs - Generic TCP plugin classes
 * 
 * NagiosPluginsNT - Nagios NRPE plugins for Windows NT
 * Copyright (c) 2009, Michael T. Conigliaro
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 as 
 * published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Mono.GetOptions;
using System.Net.Sockets;

namespace NagiosPluginsNT
{
    public class TcpPluginOptions : PluginOptions
    {
        [Option("Connection timeout in seconds (default: 4)", 't')]
        public Int32 timeout;

        [Option("Connection port", 'p')]
        public Int32 port; 
       
        public TcpPluginOptions()
        {
            timeout = 10;
        }
    }

    public class TcpPlugin : Plugin<TcpPluginOptions>
    {
        private TcpClient Client;
        private NetworkStream Stream;
        private Int64 StartTime;
        private Int64 EndTime;

        // *** constructor ***
        public TcpPlugin(TcpPluginOptions options) : base(options) { }

        // *** connect to host via tcp ***
        public void Connect() {
            try
            {
                // *** start timer and connect to host ***
                Verbose(VerbosityLevel.Info, "[TCP] Connecting to: " + Options.hostname + ":" + Options.port);
                StartTime = DateTime.Now.Ticks;
                Client = new TcpClient(Options.hostname, Options.port);
                Client.ReceiveTimeout = Options.timeout * 1000; // read timeout

                // *** open tcp stream ***
                Stream = Client.GetStream();
            }
            
            // *** port not specified or connection refused ***
            catch (System.Net.Sockets.SocketException e)
            {
                FatalError(StatusCode.Critical, "[TCP]", e.Message);
            }
        }

        // *** send data to the host ***
        public void Send(String message)
        {
            Verbose(VerbosityLevel.Info, "[TCP] Sending: " + message.Trim());
            Byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message + "\n");
            Stream.Write(buffer, 0, buffer.Length);        
        }

        // *** receive data from the host ***
        public String Receive()
        {
            String message = "";

            try
            {
                // *** return entire server response ***
                Verbose(VerbosityLevel.Info, "[TCP] Waiting for response");
                Byte[] buffer = new Byte[1024]; // maximum length of response
                Int32 bytes = Stream.Read(buffer, 0, buffer.Length);
                message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
                EndTime = DateTime.Now.Ticks; // stop timer

                Verbose(VerbosityLevel.Info, "[TCP] Got response: " + message);
            }

            // *** unable to read from host ***
            catch (System.IO.IOException e)
            {
                FatalError(StatusCode.Critical, "[TCP]", e.Message);
            }

            return message;
        }

        // *** receive banner from the host ***
        public String ReceiveBanner()
        {
            // *** return first line of server response ***
            String message = Receive();
            String[] messageLines = message.Split('\n');

            return messageLines[0].Trim();            
        }
        

        // *** disconnect from host ***
        public void Disconnect()
        {
            Client.Close();            
        }

        // *** calculate time it took for server to send data ***
        public Int32 GetResponseTime()
        {
            return (Int32)(((Decimal)EndTime - (Decimal)StartTime) / (Decimal)TimeSpan.TicksPerSecond * 100);
        }
    }
}
