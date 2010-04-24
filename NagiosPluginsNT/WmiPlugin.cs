/* WmiPlugin.cs - Generic WMI plugin classes
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
using Mono.GetOptions;
using System.Net.NetworkInformation;
using System.Management;

namespace NagiosPluginsNT
{
    public class WmiPluginOptions : PluginOptions
    {
        [Option("Connection timeout in seconds (default: 10)", 't')]
        public Int32 timeout;

        [Option("Login name", 'l')]
        public String logname;

        [Option("Authentication password", 'a')]
        public String authentication;

        public WmiPluginOptions()
        {            
            timeout = 10;            
        }
    }

    public class WmiPlugin : Plugin<WmiPluginOptions>
    {
        public ManagementObjectCollection ResultCollection;
        private System.Management.ManagementScope ManagementScope;

        // *** constructor ***
        public WmiPlugin(WmiPluginOptions options) : base(options) { }

        // *** connect to host via wmi ***
        public void Connect() { Connect("\\root\\cimv2"); }
        public void Connect(String connectNamespace)
        {            
            // *** rewrite connection options when this host is connecting to itself ***
            if (IsLocalConnection())
            {
                Verbose(VerbosityLevel.Debug, "[WMI] Local connection attempt detected (ignoring authentication options)");
                Options.hostname = "127.0.0.1";
                Options.logname = null;
                Options.authentication = null;
            }

            // *** if netbios domain is not specified in username, default to local domain ***
            else if (Options.logname != null && !Options.logname.Contains(@"\"))
            {
                Verbose(VerbosityLevel.Debug, "[WMI] No netbios domain specified in login name (using local domain " + System.Environment.UserDomainName.ToString() + ")");
                Options.logname = System.Environment.UserDomainName.ToString() + @"\" + Options.logname;
            }

            // *** set connection options ***            
            ConnectionOptions conn = new ConnectionOptions();
            conn.Username = Options.logname;
            conn.Password = Options.authentication;
            conn.Timeout = new TimeSpan(0, 0, Options.timeout);

            // *** connect ***
            Verbose(VerbosityLevel.Info, @"[WMI] Connecting to: \\" + Options.hostname + connectNamespace + " (Username=" + Options.logname + ", Password=" + Options.authentication + ", Timeout=" + Options.timeout + ")");
            ManagementScope = new System.Management.ManagementScope(@"\\" + Options.hostname + connectNamespace, conn);         
        }        

        // *** execute a query and return results ***
        public void Query(String wql)
        {
            try
            {
                // *** execute query ***
                Verbose(VerbosityLevel.Info, "[WMI] Executing query: " + wql);
                System.Management.ObjectQuery query = new System.Management.ObjectQuery(wql);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(ManagementScope, query);

                // *** get results ***            
                ResultCollection = searcher.Get();
                if (ResultCollection.Count > 0)
                {
                    Verbose(VerbosityLevel.Info, "[WMI] Query succeeded");
                }                
            }

            // *** rpc server unavailable ***
            catch (System.Runtime.InteropServices.COMException e)
            {                
                FatalError(StatusCode.Critical, "[WMI]", e.Message);
            }

            // *** bad username/password ***
            catch (System.UnauthorizedAccessException e)
            {                
                FatalError(StatusCode.Critical, "[WMI]", e.Message);
            }

            // *** bad query ***
            catch (System.Management.ManagementException e)
            {
                FatalError(StatusCode.Critical, "[WMI]", e.Message);
            }
        }

        // *** check if host is connecting to itself ***
        private Boolean IsLocalConnection()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                IPInterfaceProperties nicProperties = nic.GetIPProperties();
                UnicastIPAddressInformationCollection ips = nicProperties.UnicastAddresses;
                foreach (UnicastIPAddressInformation ip in ips)
                {
                    if (ip.Address.ToString() == Options.hostname)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
