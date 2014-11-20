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
// File:    Program.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog post, "Windows Performance Counters for ActiveMQ", http://www.practicefusion.com/blog/performance-counters-for-activemq/

using System.Reflection;
using log4net;
using log4net.Config;
using PerformanceCounterHelper;
using Topshelf;

namespace AmqMonitor
{
    public static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main()
        {
            // Configure log4net.
            XmlConfigurator.Configure();

            // Log the application start.
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            _logger.InfoFormat("Starting {0} version {1}", assemblyName.Name, assemblyName.Version);

            // Run the service.
            HostFactory.Run(h =>
            {
                h.Service<AmqMonitorService>(s =>
                {
                    s.ConstructUsing(() => new AmqMonitorService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                h.AfterInstall(() => PerformanceHelper.Install(typeof(PerfCounters)));
                h.AfterUninstall(() => PerformanceHelper.Uninstall(typeof(PerfCounters)));
                h.RunAsLocalService();
                h.SetServiceName("AmqMonitor");
                h.SetDescription("Monitor service that publishes AMQ queue statistics as performance counters.");
                h.UseLog4Net();
            });
        }
    }
}