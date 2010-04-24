/* check_disk_time.cs - Disk I/O check plugin
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

namespace check_disk_time
{
    class check_disk_time
    {
        static void Main(string[] args)
        {
            WmiPluginOptions Options = new WmiPluginOptions();
            Options.label = "Disk Time";
            Options.units = "%";
            Options.ProcessArgs(args);

            WmiPlugin plugin = new WmiPlugin(Options);
            plugin.Connect();

            plugin.Query("Select PercentDiskTime, PercentDiskTime_Base From Win32_PerfRawData_PerfDisk_PhysicalDisk where Name='_Total'");
            decimal time1 = 0;
            decimal base1 = 0;
            foreach (ManagementObject mgtObject in plugin.ResultCollection)
            {                
                time1 = Convert.ToDecimal(mgtObject["PercentDiskTime"].ToString());     
                base1 = Convert.ToDecimal(mgtObject["PercentDiskTime_Base"].ToString());
            }

            Thread.Sleep(1000);

            plugin.Query("Select PercentDiskTime, PercentDiskTime_Base From Win32_PerfRawData_PerfDisk_PhysicalDisk where Name='_Total'");
            decimal time2 = 0;
            decimal base2 = 0;
            foreach (ManagementObject mgtObject in plugin.ResultCollection)
            {                
                time2 = Convert.ToDecimal(mgtObject["PercentDiskTime"].ToString());   
                base2 = Convert.ToDecimal(mgtObject["PercentDiskTime_Base"].ToString());
            }

            plugin.AppendValue(Math.Round(((time2 - time1) / (base2 - base1)) * 100));        
            plugin.Finish();
        }
    }
}
