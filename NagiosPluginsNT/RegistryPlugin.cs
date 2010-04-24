/* RegistryPlugin.cs - Generic registry plugin classes
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
using Microsoft.Win32;

namespace NagiosPluginsNT
{
    public class RegistryPluginOptions : PluginOptions
    {        
        [Option("Registry key", 'k')]
        public String  key;

        public RegistryPluginOptions()
        {
            key = "";
        }
    }

    public class RegistryPlugin : Plugin<RegistryPluginOptions>
    {        
        // *** constructor ***
        public RegistryPlugin(RegistryPluginOptions options) : base(options) { }        

        // *** read a registry subkey ***
        public String Read()
        {
            String value = "";

            try
            {
                Options.key = Options.key.Replace("\\", "/");
                String  regRootKey = Options.key.Substring(0, Options.key.IndexOf("/"));
                String  regSubKey = Options.key.Substring(Options.key.IndexOf("/") + 1, Options.key.LastIndexOf("/") - regRootKey.Length - 1);
                String  regValue = Options.key.Substring(Options.key.LastIndexOf("/") + 1);
                regSubKey = regSubKey.Replace("/", "\\");

                Verbose(VerbosityLevel.Info, "[Registry] Reading " + regRootKey  + " => " + regSubKey + " => " + regValue);                
                RegistryKey reg = null;
                switch (regRootKey) {
                    case "HKEY_CLASSES_ROOT":
                        reg = Registry.ClassesRoot.OpenSubKey(regSubKey);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        reg = Registry.CurrentConfig.OpenSubKey(regSubKey);
                        break;
                    case "HKEY_CURRENT_USER":
                        reg = Registry.CurrentUser.OpenSubKey(regSubKey);
                        break;
                    case "HKEY_LOCAL_MACHINE":
                        reg = Registry.LocalMachine.OpenSubKey(regSubKey);
                        break;
                    case "HKEY_USERS":
                        reg = Registry.Users.OpenSubKey(regSubKey);
                        break;
                }
                
                value = reg.GetValue(regValue).ToString();
            }

            catch (System.ArgumentOutOfRangeException)
            {
                FatalError(StatusCode.Unknown, "[Registry]", "Registry key was not in correct format");
            }

            catch (System.NullReferenceException)
            {
                FatalError(StatusCode.Critical, "[Registry]", "Registry key does not exist");
            }

            return value;
        }        
    }
}
