/* SnmpPlugin.cs - Generic SNMP plugin classes
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
using System.Threading;
using Snmp;
using X690;

namespace NagiosPluginsNT
{
    public class SnmpPluginOptions : PluginOptions
    {
        //[Option("Connection port", 'p')]
        //public Int32 port;

        [Option("Response timeout in seconds (default: 10)", 't')]
        public Int32 timeout;

        [Option("SNMP community (default: public)", 'C')]
        public String community;

        /*
        [Option("Minimum value of check", 'm')]
        public Decimal minvalue;

        [Option("Maximum value of check", 'M')]
        public Decimal maxvalue;
        */
         
        [Option("SNMP oid", 'o')]
        public String oid;        

        public SnmpPluginOptions() {
            timeout = 10;
            community = "public";
            //port = 161;
            //maxvalue = Decimal.MaxValue;
        }
    }

    public class SnmpPlugin : Plugin<SnmpPluginOptions>
    {        
        private Thread GetThread;
        private String Value;

        // *** constructor ***
        public SnmpPlugin(SnmpPluginOptions options) : base(options) {
            /*
            if (Options.units == "%")
            {
                Options.minvalue = 0;
                Options.maxvalue = 100;
            }
            */
        }                  

        // *** perform an snmpget and return results ***
        public String Get()
        {
            try
            {                                
                // *** do snmpget with timeout ***
                GetThread = new Thread(new ThreadStart(doGet));                
                GetThread.Start();
                if (!GetThread.Join(Options.timeout * 1000))
                {
                    FatalError(StatusCode.Critical, "[SNMP]", "SNMP request timed out");
                }
            }

            // *** handle all exceptions ***
            catch (Exception e)
            {
                FatalError(StatusCode.Critical, "[SNMP]", e.Message);
            }

            return Value;
        }        

        // *** callback to implement snmpget ***
        private void doGet()
        {
            try
            {
                if (Options.oid.Contains("."))
                {
                    // *** convert oid string to array of unsigned ints ***
                    String[] oidString = Options.oid.Split('.');
                    Int32 oidLen = oidString.Length;
                    UInt32[] oidUInt = new UInt32[oidLen];
                    for (Int32 i = 0; i < oidLen; ++i)
                    {
                        oidUInt[i] = Convert.ToUInt32(oidString[i]);
                    }

                    // *** get value ***
                    Verbose(VerbosityLevel.Info, "[SNMP] Get: " + Options.oid);
                    ManagerItem mi = new ManagerItem(new ManagerSession(Options.hostname, Options.community), oidUInt);
                    String rawValue = mi.Value.ToString();

                    // *** trim enclosing brackets ***
                    if ((rawValue.StartsWith("\"") && rawValue.EndsWith("\"")) || (rawValue.StartsWith("[") && rawValue.EndsWith("]")))
                    {
                        Value = rawValue.Substring(1, rawValue.Length - 2);
                    }
                    else
                    {
                        Value = rawValue;
                    }

                    Verbose(VerbosityLevel.Info, "[SNMP] Got response: " + Value);
                }

                else
                {
                    FatalError(StatusCode.Critical, "[SNMP]", "Object ID not in correct format: " + Options.oid);
                }
            }

            // *** handle all exceptions ***
            catch (Exception e)
            {
                FatalError(StatusCode.Critical, "[SNMP]", e.Message);
            }
        }
    }
}