/* check_ping.cs - Simple ping check plugin
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
using NagiosPluginsNT;
using Mono.GetOptions;
using System.Net.NetworkInformation;

namespace check_alive
{
    class check_alive
    {
        static void Main(string[] args)
        {
            PingPluginOptions Options = new PingPluginOptions();
            Options.label = "Ping";
            Options.units = "%,ms";
            Options.ProcessArgs(args);

            PingPlugin plugin = new PingPlugin(Options);
            MultiPingReply reply = plugin.Ping();
            plugin.AppendValue("Packet Loss", reply.PacketLoss);
            plugin.AppendValue("RTA", reply.RoundTripTimeAverage);
            plugin.Finish();
        }
    }
}
