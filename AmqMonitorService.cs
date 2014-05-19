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
// File:    AmqMonitorService.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog, "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using PerformanceCounterHelper;

namespace AmqMonitor
{
    /// <summary>
    /// The actual service.
    /// </summary>
    public class AmqMonitorService
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Dictionary of counter helpers keyed by instance name.
        /// </summary>
        private readonly IDictionary<string, CounterHelper<PerfCounters>> _counterHelpers = new Dictionary<string, CounterHelper<PerfCounters>>();

        /// <summary>
        /// Mutex for the timer callback to ensure multiple calls don't execute simultaneously.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Callback timer to perform the queue sampling and counter publication.
        /// </summary>
        private Timer _queueSampleTimer;

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void Start()
        {
            // Validate the configuration.
            Config.Validate();

            // Create counters using default instance name to test that they are correctly installed.
            using (var counterHelper = PerformanceHelper.CreateCounterHelper<PerfCounters>())
            {
                if (counterHelper == null)
                {
                    var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    var message = string.Format(
                        "Unable to create performance counters. Please make sure the service is properly installed by running '{0}.exe install'.",
                        assemblyName);
                    throw new Exception(message);
                }

                // Remove default instance name so that it doesn't appear in PerfMon.
                counterHelper.RemoveInstance();
            }

            // Start the queue sampling timer.
            _queueSampleTimer = new Timer(SampleAndPublishQueueInfo, null, TimeSpan.Zero, Config.SampleInterval);
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop()
        {
            _logger.Info("Stopping service");

            // Wait for any ongoing sample to complete.
            lock (_lock)
            {
                // Shut down the queue sampling timer.
                _queueSampleTimer.Dispose();

                // Dispose all the counters in the counter dictionary and clear it.
                foreach (var counterHelper in _counterHelpers.Values)
                {
                    if (counterHelper != null)
                    {
                        counterHelper.Dispose();
                    }
                }
                _counterHelpers.Clear();
            }
        }

        private void SampleAndPublishQueueInfo(object state)
        {
            if (!Monitor.TryEnter(_lock))
            {
                // If another thread is already executing, just skip this iteration. This would occur if the timer period is smaller than
                // the sampling execution time, or if the service is being stopped.
                return;
            }

            try
            {
                // Get the current queue info for each configured AMQ host and update the corresponding counters.
                foreach (var queueInfo in Config.AmqHosts.SelectMany(host => host.GetQueueInfo()))
                {
                    PublishQueueInfo(queueInfo);
                }
            }
            catch (AggregateException ex)
            {
                // Flatten and unwrap AggregateException.
                _logger.Error("Error publishing queue stats.", ex.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                _logger.Error("Error publishing queue stats.", ex);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// Ensures the appropriate counters are created for the given host and queue, and publishes the info.
        /// </summary>
        private void PublishQueueInfo(QueueInfo queueInfo)
        {
            // Get or create counter helper for this host/queue.
            var instanceName = string.Format("{0}\\{1}", queueInfo.HostName, queueInfo.QueueName);
            var counterHelper = GetOrCreateCounterHelper(instanceName);
            if (counterHelper == null)
            {
                _logger.ErrorFormat("Unable to create performance counter helper for {0}.", instanceName);
                return;
            }

            // Publish the queue info...
            // The rate counters are in units of msgs per min, which is derived by the following:
            // numerator   = msgs * (sec / min)
            // denominator = sec
            // num/den     = msgs / min
            // The granularity is seconds for simplicity and accuracy. Tick granularity doesn't work because it exceeds 2^31 and
            // causes the graph to go haywire.
            const long secondsPerMinute = 60L;
            if (queueInfo.Depth.HasValue)
            {
                counterHelper.RawValue(PerfCounters.QueueDepth, queueInfo.Depth.Value);
            }
            if (queueInfo.Enqueued.HasValue)
            {
                counterHelper.RawValue(PerfCounters.EnqueuedPerMin, queueInfo.Enqueued.Value * secondsPerMinute);
                counterHelper.BaseRawValue(PerfCounters.EnqueuedPerMin, queueInfo.TimestampInSeconds);
            }
            if (queueInfo.Dequeued.HasValue)
            {
                counterHelper.RawValue(PerfCounters.DequeuedPerMin, queueInfo.Dequeued.Value * secondsPerMinute);
                counterHelper.BaseRawValue(PerfCounters.DequeuedPerMin, queueInfo.TimestampInSeconds);
            }
        }

        /// <summary>
        /// Gets a counter helper from the dictionary if it already exists, otherwise creates it and adds it to the counter helper
        /// dictionary.
        /// </summary>
        private CounterHelper<PerfCounters> GetOrCreateCounterHelper(string instanceName)
        {
            if (_counterHelpers.ContainsKey(instanceName))
            {
                return _counterHelpers[instanceName];
            }
            var counterHelper = PerformanceHelper.CreateCounterHelper<PerfCounters>(instanceName);
            _counterHelpers.Add(instanceName, counterHelper);
            return counterHelper;
        }
    }
}