using Microsoft.Win32;
using RegistryUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RegistryMonitorService
{
    public partial class RegistryMonitorService : ServiceBase
    {
        private RegistryMonitor _monitor;
        
        public RegistryMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SetRegistryValues();

            _monitor = new RegistryMonitor(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Policies\Microsoft\FVE");
            _monitor.RegChanged += new EventHandler(OnRegChanged);
            _monitor.Error += new ErrorEventHandler(OnError);
            _monitor.Start();

            EventLog.WriteEntry("Registry Monitor", "Registry Monitor started...", EventLogEntryType.Information);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            EventLog.WriteEntry("Registry Monitor", e.GetException().ToString(), EventLogEntryType.Error);
        }

        private void OnRegChanged(object sender, EventArgs e)
        {
            EventLog.WriteEntry("Registry Monitor", "Registry changed...", EventLogEntryType.Information);
            SetRegistryValues();
        }

        private void SetRegistryValues()
        {
            try
            {
                if ((int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FVE", "RDVDenyWriteAccess", 0) != 0)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FVE", "RDVDenyWriteAccess", 0, RegistryValueKind.DWord);
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Registry Monitor", e.ToString(), EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            _monitor.Stop();
            EventLog.WriteEntry("Registry Monitor", "Registry Monitor stopped...", EventLogEntryType.Information);
        }
    }
}
