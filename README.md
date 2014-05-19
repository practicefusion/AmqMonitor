AmqMonitor
==========

A simple .NET Windows service to publish ActiveMQ queue stats as performance counters

#### Requirements

To install:
* .NET Framework 4.5

To build the source:
* Visual Studio 2012

#### Getting started

1. Build the **AmqMonitor.sln** solution with Visual Studio or MSBuild.

2. Edit the **AmqMonitor.exe.config** file and specify values for the following settings:
   ```
   <appSettings>
      <add key="AmqHosts" value="host1,host2,host3"/>
      <add key="SampleInterval" value="0:01:00"/>
   </appSettings>
   ```
   * **AmqHosts** is a comma-delimited list of brokers to monitor.
   * **SampleInterval** is the interval at which to poll all the brokers, specified as _h:mm:ss_. A value of 1 minute is appropriate for long-running production monitoring.

3. In an elevated command prompt, run the following:
   ```
   AmqMonitor.exe install
   ```

4. Start the service using the service control panel, or run the following:
   ```
   AmqMonitor.exe start
   ```

5. Fire up Performance Monitor and add counters from the **AmqMonitor** category.

#### More information

For an explanation of this project, please visit the Practice Fusion tech blog article about it here: "PerfMon Counters for ActiveMQ", http://www.practicefusion.com/blog/TBD
