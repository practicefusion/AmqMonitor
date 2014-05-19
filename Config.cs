// The MIT License (MIT)
// Copyright (c) 2014 Practice Fusion
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// File:    Config.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog, "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using log4net;

namespace AmqMonitor
{
    /// <summary>
    /// Provides access to configuration values.
    /// </summary>
    public static class Config
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Config()
        {
            // Obtain configuration information from the default app config file.
            SampleInterval = GetTimeSpanValue("SampleInterval");
            var amqHostsString = GetStringValue("AmqHosts");
            AmqHosts = amqHostsString.Split(',').Select(h => new AmqHost(h));
            _logger.InfoFormat("Configuration loaded. AmqHosts={0} SampleInterval={1}", amqHostsString, SampleInterval);
        }

        /// <summary>
        /// Gets the interval at which to sample and publish AMQ queue statistics to performance counters.
        /// </summary>
        public static TimeSpan SampleInterval { get; private set; }

        /// <summary>
        /// Gets a list of AMQ hosts to monitor.
        /// </summary>
        public static IEnumerable<AmqHost> AmqHosts { get; private set; }

        /// <summary>
        /// Gets the specified app setting value as a string. Returns "" if the app setting is not present.
        /// </summary>
        /// <param name="key">
        /// The key of the app setting to get.
        /// </param>
        /// <returns>
        /// The value of the specified app setting, or "" if not found.
        /// </returns>
        private static string GetStringValue(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? "";
        }

        /// <summary>
        /// Gets the specified app setting value as a TimeSpan. Returns TimeSpan.Zero if the app setting is not present or can't be
        /// parsed.
        /// </summary>
        /// <param name="key">
        /// The key of the app setting to get.
        /// </param>
        /// <returns>
        /// The value of the specified app setting, or TimeSpan.Zero if not found.
        /// </returns>
        private static TimeSpan GetTimeSpanValue(string key)
        {
            TimeSpan value;
            TimeSpan.TryParse(ConfigurationManager.AppSettings[key], out value);
            return value;
        }

        /// <summary>
        /// Validates that the configuration has legal values.
        /// </summary>
        public static void Validate()
        {
            if (SampleInterval < TimeSpan.Zero)
            {
                throw new Exception("Configured sample interval cannot be less than zero.");
            }
            if (!AmqHosts.Any())
            {
                throw new Exception("At least one AMQ host must be configured.");
            }
        }
    }
}