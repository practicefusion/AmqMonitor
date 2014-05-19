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
// File:    AmqHost.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog, "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Xml.Linq;
using log4net;

namespace AmqMonitor
{
    /// <summary>
    /// Encapsulates access to the AMQ admin interface for a given host.
    /// </summary>
    public class AmqHost
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Stopwatch to provide second-counts for counter denominators. We don't need the real wall time (as in seconds since
        /// epoch, etc.), just a counter that increments. This stopwatch is started when the type is initialized (essentially, on
        /// service start).
        /// </summary>
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        private readonly string _host;

        /// <summary>
        /// Creates a new <see cref="AmqHost" /> object.
        /// </summary>
        /// <param name="host">
        /// The host name of the AMQ broker (e.g., "myserver", "12.0.0.11", etc.)
        /// </param>
        public AmqHost(string host)
        {
            _host = host;
        }

        /// <summary>
        /// Retrieves the queue statistics for this AMQ host and returns them as a list of <see cref="QueueInfo" />.
        /// </summary>
        /// <returns>
        /// A list of <see cref="QueueInfo" /> representing the current queue information at the current AMQ host's queue admin
        /// URL.
        /// </returns>
        public IEnumerable<QueueInfo> GetQueueInfo()
        {
            try
            {
                // Capture the timestamp.
                var timestampInSeconds = (long)_stopwatch.Elapsed.TotalSeconds;

                // Determine the queue admin URL for the given host.
                var queueAdminUrl = string.Format("http://{0}:8161/admin/xml/queues.jsp", _host);

                // Get the queue admin response as an XDocument.
                var doc = GetAdminXml(queueAdminUrl);

                // Sample AMQ response:
                // <queues>
                //   <queue name="MyQueue">
                //     <stats size="123" consumerCount="1" enqueueCount="999" dequeueCount="999"/>
                //     <feed>
                //       <atom>queueBrowse/MyQueue?view=rss&amp;feedType=atom_1.0</atom>
                //       <rss>queueBrowse/MyQueue?view=rss&amp;feedType=rss_2.0</rss>
                //     </feed>
                //   </queue>
                // </queues>

                // Transform into a list of QueueInfo objects.
                return doc
                    .Element("queues")
                    .Elements("queue")
                    .Select(element =>
                    {
                        var stats = element.Element("stats");
                        return new QueueInfo
                        {
                            HostName = _host,
                            QueueName = (string)element.Attribute("name"),
                            Depth = GetIntField(stats, "size"),
                            Enqueued = GetIntField(stats, "enqueueCount"),
                            Dequeued = GetIntField(stats, "dequeueCount"),
                            TimestampInSeconds = timestampInSeconds,
                        };
                    });
            }
            catch (AggregateException ex)
            {
                // Flatten and unwrap AggregateException.
                _logger.Error("Error obtaining queue stats.", ex.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                _logger.Error("Error obtaining queue stats.", ex);
            }

            // If we failed to get queue info, return an empty list (no data).
            return new QueueInfo[0];
        }

        /// <summary>
        /// Queries the AMQ queue admin URL and returns the response as an XDocument.
        /// </summary>
        /// <param name="queueAdminUrl">
        /// The AMQ queue admin URL to query.
        /// </param>
        /// <returns>
        /// An XDocument containing the queue admin REST response.
        /// </returns>
        private static XDocument GetAdminXml(string queueAdminUrl)
        {
            using (var handler = new HttpClientHandler())
            using (var httpClient = new HttpClient(handler))
            {
                // Default credentials for AMQ are admin/admin.
                handler.Credentials = new NetworkCredential("admin", "admin");
                using (var stream = httpClient.GetStreamAsync(queueAdminUrl).Result)
                {
                    return XDocument.Load(stream);
                }
            }
        }

        /// <summary>
        /// Gets an attribute value as an integer from an XElement. Returns null if the attribute doesn't exist or can't be parsed.
        /// </summary>
        /// <param name="element">
        /// An XElement.
        /// </param>
        /// <param name="attributeName">
        /// The name of the attribute to get.
        /// </param>
        /// <returns>
        /// The integer value of the specified attribute, or null if the attribute doesn't exist or isn't parseable as an int.
        /// </returns>
        private static int? GetIntField(XElement element, string attributeName)
        {
            if (element != null)
            {
                var stringValue = (string)element.Attribute(attributeName);
                int intValue;
                if (int.TryParse(stringValue, out intValue))
                {
                    return intValue;
                }
            }
            return null;
        }
    }
}