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
// File:    QueueInfo.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog, "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD

namespace AmqMonitor
{
    /// <summary>
    /// Contains information about a queue.
    /// </summary>
    public class QueueInfo
    {
        /// <summary>
        /// The name of the broker where the queue is hosted.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The name of the queue.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The current number of pending messages in the queue.
        /// </summary>
        public int? Depth { get; set; }

        /// <summary>
        /// The total number of messages added to this queue.
        /// </summary>
        public int? Enqueued { get; set; }

        /// <summary>
        /// The total number of messages removed from this queue.
        /// </summary>
        public int? Dequeued { get; set; }

        /// <summary>
        /// The timestamp of this sample, as seconds since the start of the service.
        /// </summary>
        public long TimestampInSeconds { get; set; }
    }
}