/* check_http.cs - HTTP check plugin
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

namespace check_http
{
    public class CheckHttpTcpPluginOptions : TcpPluginOptions
    {
        [Option("URL (default: /)", 'u')]
        public string url;
    }

    class check_http
    {
        static void Main(string[] args)
        {
            CheckHttpTcpPluginOptions Options = new CheckHttpTcpPluginOptions();
            Options.label = "HTTP";
            Options.units = "ms";
            Options.port = 80;
            Options.url = "/";
            Options.ProcessArgs(args);

            TcpPlugin plugin = new TcpPlugin(Options);
            plugin.Connect();
            
            String requestString = String.Format("GET {0} HTTP/1.0\r\nAccept:*.*\r\n Host:{1}\r\n\r\n", Options.url, Options.hostname);
            //plugin.Send(String.Format("GET {0} HTTP/1.0\n", Options.url));
            plugin.Send(String.Format(requestString);
            String value = plugin.ReceiveBanner();            
            plugin.Disconnect();

            // always warn for level 400-500 http status codes
            plugin.GetStatus(value, @"HTTP/1\.[01] (4\d\d|5\d\d)", NagiosPluginsNT.Plugin<TcpPluginOptions>.StatusCode.Critical);

            plugin.AppendValue(value);
            plugin.AppendValue("Response Time", plugin.GetResponseTime());                                    
            plugin.Finish();
        }
    }
}
