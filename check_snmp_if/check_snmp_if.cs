/* check_snmp_if.cs - SNMP interface check plugin
 * 
 * NagiosPluginsNT - Nagios NRPE plugins for Windows NT
 * Copyright (c) 2008, Michael T. Conigliaro
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
using System.Threading;

namespace check_snmp_if
{
    public class SnmpIfPluginOptions : SnmpPluginOptions
    {
        [Option("Interface (default: 1)", 'i')]
        public Int32 checkInterface;

        [Option("Interface speed in,out (default: auto,auto)", 's')]
        public String ifSpeed;

        public Int32 interval;
        public String oidIfName;
        public String oidIfSpeed;
        public String oidIfInOctets;
        public String oidIfOutOctets;

        public SnmpIfPluginOptions()
        {
            checkInterface = 1;
            ifSpeed = "auto,auto";

            interval = 6; // polling interval in seconds (note: < 5 seems to be bad news)
            oidIfName = "1.3.6.1.2.1.31.1.1.1.1";
            oidIfSpeed = "1.3.6.1.2.1.2.2.1.5";
            oidIfInOctets = "1.3.6.1.2.1.2.2.1.10";
            oidIfOutOctets = "1.3.6.1.2.1.2.2.1.16";
        }
    }

    class check_snmp_if
    {
        static void Main(string[] args)
        {
            SnmpIfPluginOptions Options = new SnmpIfPluginOptions();
            Options.label = "Interface";            
            Options.units = "%";
            Options.ProcessArgs(args);            
            SnmpPlugin plugin = new SnmpPlugin(Options);

            String checkInterface = Options.checkInterface.ToString();

            // *** get interface description ***            
            Options.oid = Options.oidIfName + "." + checkInterface;
            Options.label += " '" + plugin.Get() + "'";
            
            // *** get interface speeds (0 = in, 1 = out) ***
            Int32[] ifSpeed = new Int32[2];
            Int32 tryParse;
            for (Int32 i = 0; i < 2; ++i)
            {
                String optionIfSpeed = plugin.GetNthOption(Options.ifSpeed, i);
                if (optionIfSpeed == "auto" || !Int32.TryParse(optionIfSpeed, out tryParse))
                {
                    Options.oid = Options.oidIfSpeed + "." + checkInterface;
                    ifSpeed[i] = Convert.ToInt32(plugin.Get());
                    if (ifSpeed[i] < 1)
                    {
                        plugin.FatalError(Plugin<NagiosPluginsNT.SnmpPluginOptions>.StatusCode.Critical, "", "Unable to determine interface speed");
                    }
                }
                else
                {
                    ifSpeed[i] = Convert.ToInt32(optionIfSpeed);
                }
            }
            
            // *** poll counters ***            
            Options.oid = Options.oidIfInOctets + "." + checkInterface;
            Int64 startTimeIn = DateTime.Now.Ticks;
            Decimal ifInOctets1 = Convert.ToDecimal(plugin.Get());            
            Options.oid = Options.oidIfOutOctets + "." + checkInterface;
            Int64 startTimeOut = DateTime.Now.Ticks;
            Decimal ifOutOctets1 = Convert.ToDecimal(plugin.Get());

            Thread.Sleep(Options.interval * 1000);

            // *** poll counters ***                        
            Options.oid = Options.oidIfInOctets + "." + checkInterface;
            Decimal ifInOctets2 = Convert.ToDecimal(plugin.Get());
            Decimal realIntervalIn = (((Decimal)DateTime.Now.Ticks - (Decimal)startTimeIn) / (Decimal)TimeSpan.TicksPerSecond);
            Options.oid = Options.oidIfOutOctets + "." + checkInterface;
            Decimal ifOutOctets2 = Convert.ToDecimal(plugin.Get());
            Decimal realIntervalOut = (((Decimal)DateTime.Now.Ticks - (Decimal)startTimeOut) / (Decimal)TimeSpan.TicksPerSecond);                        

            // *** get traffic statistics ***
            plugin.AppendValue("Traffic In", Math.Round((Math.Abs(ifInOctets2 - ifInOctets1) * 8 * 100) / (realIntervalIn * ifSpeed[0])));
            plugin.AppendValue("Traffic Out", Math.Round((Math.Abs(ifOutOctets2 - ifOutOctets1) * 8 * 100) / (realIntervalOut * ifSpeed[1])));

            plugin.Finish();
        }
    }
}
