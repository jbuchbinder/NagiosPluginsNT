/* Plugin.cs - Generic plugin classes
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
using System.Text.RegularExpressions;
using Mono.GetOptions;

namespace NagiosPluginsNT
{
    public class PluginOptions : Mono.GetOptions.Options
    {
        [Option("Hostname (default: 127.0.0.1)", 'H')]
        public String hostname;

        private String privWarning;
        [Option("Warning range/threshold", 'w')]
        public String warning
        {
            set
            {
                privWarning = Regex.Replace(value, @"(\^)([^\^])", "$2", RegexOptions.Compiled);                
            }
            get
            {
                return privWarning;
            }
        }

        private String privCritical;
        [Option("Critical range/threshold", 'c')]
        public String critical
        {
            set
            {
                privCritical = Regex.Replace(value, @"(\^)([^\^])", "$2", RegexOptions.Compiled);
            }
            get
            {
                return privCritical;
            }
        }

        [Option("Unit of measurement (s, %, B, c)", 'U')]
        public String units;

        [Option("Value multiplier", 'x')]
        public Decimal multiplier;

        [Option("Verbosity (0-3)", 'v')]
        public Int32 verbose;                

        [Option("Label (default: Service)", 'L')]
        public String label;

        public PluginOptions()
        {            
            hostname = "127.0.0.1";
            multiplier = 1;            
            label = "Service";
            ParsingMode = OptionsParsingMode.GNU_DoubleDash;
        }
    }

    public class Plugin<PluginOptionsType> where PluginOptionsType : PluginOptions
    {
        public PluginOptionsType Options;
        public StatusCode Status
        {
            set
            {
                privStatus = value;
                Verbose(VerbosityLevel.Debug, "Status is: " + privStatus);
            }
            get
            {
                return privStatus;
            }
        }
        public enum VerbosityLevel { None, Error, Info, Debug }
        public enum StatusCode { Ok, Warning, Critical, Unknown }
        private String TextValue;
        private String PerfValue;
        private Int32 ValueCount;
        private Int32 PerfValueCount;
        private Int32 precision; // number of decimal places to show in numeric values
        private StatusCode privStatus;

        // *** constructor ***
        public Plugin(PluginOptionsType options)
        {
            Options = options;
            Status = StatusCode.Ok;
            precision = 2;            
        }        

        // *** append numeric value and string label to result output ***
        public void AppendValue(String label, Decimal value)
        {
            // *** work with human readable values ***
            CookValue(ref value);
            //CookValue(ref min);
            //CookValue(ref max);            

            // *** parse option lists ***
            String warning = GetNthOption(Options.warning, ValueCount);
            String critical = GetNthOption(Options.critical, ValueCount);
            String units = GetNthOption(Options.units, PerfValueCount);

            // *** strip invalid units ***
            if (units != null)
            {
                StripInvalidUnits(ref units);
            }

            Verbose(VerbosityLevel.Debug, "Appending value (Label=" + label + ", Value=" + value + units + ")");

            // *** check ranges and get status ***                        
            GetStatus(value, warning, StatusCode.Warning);
            GetStatus(value, critical, StatusCode.Critical);
            
            // *** append perf/text output ***
            if (PerfValue != null)
            {                
                PerfValue += " ";
            }
            if (TextValue != null)
            {
                TextValue += ", ";
            }
            PerfValue += "'" + label.Replace("'", "''") + "'=" + value + units + ";" + warning + ";" + critical + ";" /* + min + Options.units */ + ";" /* +max + Options.units */;
            if (label != "")
            {
                TextValue += label.Replace("'", "''") + " = ";
            }
            TextValue += value + units;

            // *** count values for GetNthOption() ***
            ++ValueCount;
            ++PerfValueCount; // only used for units option
        }

        // *** append numeric value to result output ***
        public void AppendValue(Decimal value)
        {
            // *** work with human readable values ***
            CookValue(ref value);
            //CookValue(ref min);
            //CookValue(ref max);            

            // *** parse option lists ***
            String warning = GetNthOption(Options.warning, ValueCount);
            String critical = GetNthOption(Options.critical, ValueCount);
            String units = GetNthOption(Options.units, PerfValueCount);

            // *** strip invalid units ***
            if (units != null)
            {
                StripInvalidUnits(ref units);
            }

            Verbose(VerbosityLevel.Debug, "Appending value: " + value + units);

            // *** check ranges and get status ***                        
            GetStatus(value, warning, StatusCode.Warning);
            GetStatus(value, critical, StatusCode.Critical);

            // *** append perf/text output ***
            if (PerfValue != null)
            {
                PerfValue += " ";
            }
            if (TextValue != null)
            {
                TextValue += ", ";
            }
            PerfValue += "'" + Options.label.Replace("'", "''") + "'=" + value + units + ";" + warning + ";" + critical + ";" /* + min + Options.units */ + ";" /* +max + Options.units */;            
            TextValue += value + units;

            // *** count values for GetNthOption() ***
            ++ValueCount;
            ++PerfValueCount; // only used for units option
        }

        // *** append string value to result output ***
        public void AppendValue(String value)
        {
            // *** parse option lists ***
            String warning = GetNthOption(Options.warning, ValueCount);
            String critical = GetNthOption(Options.critical, ValueCount);

            Verbose(VerbosityLevel.Debug, "Appending value: " + value);

            // *** check ranges and get status ***            
            GetStatus(value, warning, StatusCode.Warning);
            GetStatus(value, critical, StatusCode.Critical);

            // *** append text output ***
            if (TextValue != null)
            {
                TextValue += ", ";
            }
            TextValue += value;

            // *** count values for GetNthOption() ***
            ++ValueCount;
        }

        // *** write plugin output and exit ***
        public void Finish()
        {
            String output = Options.label + " " + Status.ToString().ToUpper();
            if (TextValue != null)
            {
                output += " - " + TextValue;
            }
            if (PerfValue != null)
            {
                output += "|" + PerfValue;
            }
            Console.WriteLine(output);            
            Environment.Exit((int)Status);
        }

        // *** print verbose output ***
        public void Verbose(VerbosityLevel level, String message)
        {
            if (Convert.ToInt32(level) <= Options.verbose)
            {
                Console.WriteLine("[{0}] {1}", level, message);
            }
        }

        // *** print verbose output and exit ***
        public void FatalError(StatusCode status, String prefix, String message)
        {
            if (prefix != "")
            {
                Verbose(VerbosityLevel.Error, prefix + " " + message);
            }                
            else {
                Verbose(VerbosityLevel.Error, message);
            }
            Status = status;
            TextValue = message;
            Finish();
        }                

        // *** obtain status through numeric comparison ***
        public void GetStatus(Decimal numValue, String range, StatusCode statusCode)
        {            
            // *** dont bother if range is null/empty or status is already critical ***
            if (range != null && range != "" && Status != StatusCode.Critical)
            {                
                // *** make sure range is in correct format ***
                Regex regex = new Regex(@"^@?(\d+(\.\d+)?|((~|(-?\d+(\.\d+)?)):(-?\d+(\.\d+)?)?))$", RegexOptions.Compiled);
                if (regex.IsMatch(range))
                {
                    // *** parse nagios threshold format ***
                    Verbose(VerbosityLevel.Debug, "Parsing " + statusCode + " range: " + range);
                    Boolean alertInside = false;
                    Decimal tryParse;
                    String rangeMin = "";
                    String rangeMax = "";
                    if (range.Substring(0, 1) == "@")
                    {
                        alertInside = true; // alert when inside of threshold
                        range = range.Substring(1, range.Length - 1); // remove leading '@'
                    }
                    if (decimal.TryParse(range, out tryParse))
                    {
                        rangeMin = "0";
                        rangeMax = range;
                    }
                    else if (range.Contains(":"))
                    {
                        String[] split = range.Split(':');
                        rangeMin = split[0];
                        if (rangeMin == "~")
                        {
                            rangeMin = decimal.MinValue.ToString(); // start is negative infinity
                        }
                        rangeMax = split[1];
                        if (rangeMax == "")
                        {
                            rangeMax = decimal.MaxValue.ToString(); // end is infinity
                        }
                    }
                    Decimal thresholdStart = Convert.ToDecimal(rangeMin);
                    Decimal thresholdEnd = Convert.ToDecimal(rangeMax);

                    // *** do comparison and get status ***            
                    if (!alertInside)
                    {
                        Verbose(VerbosityLevel.Debug, statusCode + " if " + numValue + " OUTSIDE range " + thresholdStart + " - " + thresholdEnd);
                        if (numValue < thresholdStart || numValue > thresholdEnd)
                        {                            
                            Status = statusCode;
                        }
                    }
                    else if (alertInside)
                    {
                        Verbose(VerbosityLevel.Debug, statusCode + " if " + numValue + " WITHIN range " + thresholdStart + " - " + thresholdEnd);
                        if (numValue >= thresholdStart && numValue <= thresholdEnd)
                        {                            
                            Status = statusCode;
                        }
                    }                    
                }
                else
                {
                    Verbose(VerbosityLevel.Debug, "NOT parsing " + statusCode + " range (incorrect format): " + range);
                }
            }            
        }

        // *** obtain status through string comparison ***
        public void GetStatus(String value, String range, StatusCode statusCode)
        {            
            // *** dont bother if range is null/empty or status is already critical ***
            if (range != null && range != "" && Status != StatusCode.Critical)
            {                
                String[] patterns = range.Split(':'); // multiple regular expressions separated by colons
                foreach (String pattern in patterns)
                {
                    Verbose(VerbosityLevel.Debug, statusCode + " if match: " + pattern);
                    Regex regex = new Regex(pattern);
                    if (regex.IsMatch(value))
                    {
                        Status = statusCode;
                        break;
                    }
                }                
            }            
        }

        // *** parse an option list and return the desired index ***
        public String GetNthOption(String options, Int32 index)
        {
            String result;
            if (options != null && options.Contains(","))
            {
                String[] splitOptions = options.Split(',');
                if (splitOptions.Length >= index)
                {
                    result = splitOptions[index];
                }
                else
                {
                    result = "";
                }
            }
            else
            {
                result = options;
            }
            return result;
        }

        // *** make numeric values human readable ***
        private void CookValue(ref Decimal value)
        {
            value = Math.Round(value * Options.multiplier, precision);
        }

        // *** strip invalid units ***
        private void StripInvalidUnits(ref String units)
        {
            Regex nonAlpha = new Regex("[^a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (units != "%" && nonAlpha.IsMatch(units))
            {
                Verbose(VerbosityLevel.Debug, "Ignoring invalid units option: " + units);
                units = null;
            }
        }        
    }
}
