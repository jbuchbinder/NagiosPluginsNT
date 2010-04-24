/* PingPlugin.cs - Generic ping plugin classes
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
using System.Threading;
using Mono.GetOptions;
using System.Net.NetworkInformation;

namespace NagiosPluginsNT
{
    public class PingPluginOptions : PluginOptions
    {
        [Option("Packets (default: 4)", 'p')]
        public Int32 packets;

        [Option("Dont fragment (default: false)", 'd')]
        public Boolean dont_fragment;

        [Option("Ping timeout in seconds (default: 4)", 't')]
        public Int32 timeout;

        [Option("Time to live (default: 128)", 'l')]
        public Int32 ttl;
        
        public PingPluginOptions()
        {
            packets = 4;
            dont_fragment = false;
            timeout = 4;
            ttl = 128;
        }
    }

    public class PingPlugin : Plugin<PingPluginOptions>
    {
        // *** constructor ***
        public PingPlugin(PingPluginOptions options) : base(options) { }

        // *** ping the host ***
        public MultiPingReply Ping()
        {            
            MultiPingReply multiPingReply;
            multiPingReply.PacketsSent = Options.packets;
            multiPingReply.PacketsReceived = 0;
            multiPingReply.RoundTripTime = 0;

            try
            {
                // *** initialize ***
                Ping pingSender = new Ping();
                PingReply pingReply = null;
                PingOptions pingOptions = new PingOptions();
                pingOptions.DontFragment = Options.dont_fragment;
                pingOptions.Ttl = Options.ttl;
                Byte[] buffer = Encoding.ASCII.GetBytes("01234567890123456789012345678901"); // 32 bytes
                Verbose(VerbosityLevel.Info, "[Ping] Pinging " + Options.hostname + " with " + buffer.Length + " bytes of data (DontFragment=" + pingOptions.DontFragment + ", TTL=" + pingOptions.Ttl + ", Timeout=" + Options.timeout * 1000 + ")");

                for (Int32 i = 0; i < Options.packets; ++i)
                {
                    // *** send echo and process reply ***
                    pingReply = pingSender.Send(Options.hostname, Options.timeout * 1000, buffer, pingOptions);                    
                    if (pingReply.Status == IPStatus.Success)
                    {
                        Verbose(VerbosityLevel.Info, "[Ping] [" + (i + 1) + "/" + Options.packets + "] Reply from " + pingReply.Address + ": bytes=" + pingReply.Buffer.Length + " time=" + pingReply.RoundtripTime + "ms TTL=" + pingReply.Options.Ttl);
                        ++multiPingReply.PacketsReceived;
                        multiPingReply.RoundTripTime += pingReply.RoundtripTime;
                        Thread.Sleep(200); // prevent flooding
                    }
                    else
                    {
                        Verbose(VerbosityLevel.Info, "[Ping] [" + (i + 1) + "/" + Options.packets + "] " + pingReply.Status.ToString());
                    }                    
                }

                // *** error out when host is unreachable ***
                if (Options.packets > 0 && multiPingReply.PacketsReceived == 0)
                {
                    FatalError(StatusCode.Critical, "[Ping]", "Host is unreachable");
                }                
            }

            // *** unknown host ***
            catch (System.Net.NetworkInformation.PingException e) {
                FatalError(StatusCode.Critical, "[Ping]", e.InnerException.Message);
            }
                                        
            return multiPingReply;
        }        
    }

    // *** ping statistics for multiple ping replies ***
    public struct MultiPingReply
    {
        public Int32 PacketsSent;
        public Int32 PacketsReceived;
        public Int64 RoundTripTime;
        public Int32 PacketLoss
        {
            get
            {
                if (PacketsSent > 0) {
                    return (Int32)((((Decimal)PacketsSent - (Decimal)PacketsReceived) / (Decimal)PacketsSent) * 100);
                }
                else
                {
                    return 0;
                }
            }
        }
        public Decimal RoundTripTimeAverage
        {            
            get
            {
                if (PacketsReceived > 0)
                {
                    return RoundTripTime / PacketsReceived;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
