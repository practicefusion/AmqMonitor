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
// File:    CounterHelperExtensions.cs
// Date:    05/19/2014
// Author:  Steven Padfield
// 
// From Practice Fusion technology blog, "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD

using System;
using PerformanceCounterHelper;

namespace AmqMonitor
{
    /// <summary>
    /// Helper extension methods for the <see cref="CounterHelper{T}" /> class.
    /// </summary>
    public static class CounterHelperExtensions
    {
        /// <summary>
        /// Removes the current instance name for multi-instance counters.
        /// </summary>
        /// <typeparam name="T">
        /// enum type holding performance counter details.
        /// </typeparam>
        /// <param name="counterHelper">
        /// An instance of <see cref="CounterHelper{T}" />.
        /// </param>
        public static void RemoveInstance<T>(this CounterHelper<T> counterHelper)
        {
            var counterNames = Enum.GetValues(typeof(T));
            foreach (var counterName in counterNames)
            {
                counterHelper.GetInstance((T)counterName).RemoveInstance();
            }
        }
    }
}