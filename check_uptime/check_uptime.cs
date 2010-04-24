/* check_uptime.cs - System uptime check plugin
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
using System.Management;
using System.Threading;
using System.Globalization;

namespace check_uptime
{
    class check_uptime
    {
        static void Main(string[] args)
        {
            WmiPluginOptions Options = new WmiPluginOptions();
            Options.label = "Uptime";
            Options.units = "s";
            Options.multiplier = 1.0M / TimeSpan.TicksPerSecond;
            Options.ProcessArgs(args);

            WmiPlugin plugin = new WmiPlugin(Options);
            plugin.Connect();

            plugin.Query("Select LastBootUpTime From Win32_OperatingSystem where LastBootUpTime is not null");
            string lastBootUpTime = null;
            foreach (ManagementObject mgtObject in plugin.ResultCollection)
            {
                lastBootUpTime = mgtObject["LastBootUpTime"].ToString();
                lastBootUpTime = lastBootUpTime.Substring(4, 2)
                 + "/" + lastBootUpTime.Substring(6, 2)
                 + "/" + lastBootUpTime.Substring(0, 4)
                 + " " + lastBootUpTime.Substring(8, 2)
                 + ":" + lastBootUpTime.Substring(10, 2)
                 + ":" + lastBootUpTime.Substring(12, 2);
            }
            if (lastBootUpTime == null)
            {
                plugin.FatalError(NagiosPluginsNT.Plugin<WmiPluginOptions>.StatusCode.Critical, "", "Win32_OperatingSystem.LastBootUpTime was null (maybe unsupported by OS)");
            }
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); // force US format for dates
            plugin.AppendValue(Convert.ToDecimal((DateTime.Now.Ticks - Convert.ToDateTime(lastBootUpTime).Ticks)));            
            plugin.Finish();
        }
    }
}
