/* check_snmp.cs - SNMP "get" check plugin
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

namespace check_snmp
{
    class check_snmp
    {
        static void Main(string[] args)
        {
            SnmpPluginOptions Options = new SnmpPluginOptions();
            Options.oid = "1.3.6.1.2.1.1.5.0";
            Options.ProcessArgs(args);

            SnmpPlugin plugin = new SnmpPlugin(Options);            
            String value = plugin.Get();            
            Decimal tryParse;
            if (decimal.TryParse(value, out tryParse))
            {
                plugin.AppendValue(tryParse);
            }
            else
            {
                plugin.AppendValue(value);
            }

            plugin.Finish();
        }
    }
}
