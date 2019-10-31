/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2019 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Xml;
using Myrtille.Helpers;

namespace Myrtille.Admin.Services
{
    [RunInstaller(true)]
    public class ServicesInstaller : Installer
	{
        // required designer variable
        private Container components = null;
        
        private ServiceProcessInstaller serviceProcessInstaller;
		private ServiceInstaller serviceInstaller;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new ServiceProcessInstaller();
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;

            this.serviceInstaller = new ServiceInstaller();
            this.serviceInstaller.ServiceName = "Myrtille.Admin.Services";
            this.serviceInstaller.Description = "Myrtille Admin API";
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;

            this.Installers.AddRange(new Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller});
        }

        #endregion

        public ServicesInstaller()
        {
            // This call is required by the Designer.
            InitializeComponent();
        }

        public override void Install(
            IDictionary stateSaver)
        {
            // enable the line below to debug this installer; disable otherwise
            //MessageBox.Show("Attach the .NET debugger to the 'MSI Debug' msiexec.exe process now for debug. Click OK when ready...", "MSI Debug");

            // if the installer is running in repair mode, it will try to re-install Myrtille... which is fine
            // problem is, it won't uninstall it first... which is not fine because some components can't be installed twice!
            // thus, prior to any install, try to uninstall first

            Context.LogMessage("Myrtille.Admin.Services is being installed, cleaning first");

            try
            {
                Uninstall(null);
            }
            catch (Exception exc)
            {
               Context.LogMessage(string.Format("Failed to clean Myrtille.Admin.Services ({0})", exc));
            }

            Context.LogMessage("Installing Myrtille.Admin.Services");

            base.Install(stateSaver);

            try
            {
                // load config
                var config = new XmlDocument();
                var configPath = Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.exe.config");
                config.Load(configPath);

                var navigator = config.CreateNavigator();

                // admin services port
                int adminServicesPort = 8008;
                if (!string.IsNullOrEmpty(Context.Parameters["ADMINSERVICESPORT"]))
                {
                    int.TryParse(Context.Parameters["ADMINSERVICESPORT"], out adminServicesPort);
                }

                if (adminServicesPort != 8008)
                {
                    // application settings
                    var settings = XmlTools.GetNode(navigator, "/configuration/applicationSettings/Myrtille.Admin.Services.Properties.Settings");
                    if (settings != null)
                    {
                        settings.InnerXml = settings.InnerXml.Replace("8008", adminServicesPort.ToString());
                    }
                }

                // save config
                config.Save(configPath);

                Context.LogMessage("Installed Myrtille.Admin.Services");
            }
            catch (Exception exc)
            {
                Context.LogMessage(string.Format("Failed to install Myrtille.Admin.Services ({0})", exc));
                throw;
            }
        }

        public override void Commit(
            IDictionary savedState)
        {
            base.Commit(savedState);
            StartService();
        }

        public override void Rollback(
            IDictionary savedState)
        {
            StopService();
            base.Rollback(savedState);
            DoUninstall();
        }

        public override void Uninstall(
            IDictionary savedState)
        {
            StopService();
            base.Uninstall(savedState);
            DoUninstall();
        }

        private void StartService()
        {
            Context.LogMessage("Starting Myrtille.Admin.Services");

            // try to start the service
            // in case of failure, ask for a manual start after install

            try
            {
                var sc = new ServiceController(serviceInstaller.ServiceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    Context.LogMessage("Started Myrtille.Admin.Services");
                }
                else
                {
                    Context.LogMessage(string.Format("Myrtille.Admin.Services is not stopped (status: {0})", sc.Status));
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    ActiveWindow.Active,
                    serviceInstaller.ServiceName + " windows service could not be started by this installer. Please do it manually once the installation is complete",
                    serviceInstaller.ServiceName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Context.LogMessage(string.Format("Failed to start Myrtille.Admin.Services ({0})", exc));
            }
        }

        private void StopService()
        {
            Context.LogMessage("Stopping Myrtille.Admin.Services");

            // if the service is running while uninstall is going on, the user is asked wether to stop it or not
            // problem is, if the user choose "no", the service is not stopped thus won't be removed
            // force stop it at this step, if not already done

            try
            {
                var sc = new ServiceController(serviceInstaller.ServiceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    Context.LogMessage("Stopped Myrtille.Admin.Services");
                }
                else
                {
                    Context.LogMessage(string.Format("Myrtille.Admin.Services is not running (status: {0})", sc.Status));
                }
            }
            catch (Exception exc)
            {
                Context.LogMessage(string.Format("Failed to stop Myrtille.Admin.Services ({0})", exc));
            }
        }

        private void DoUninstall()
        {
            // enable the line below to debug this installer; disable otherwise
            //MessageBox.Show("Attach the .NET debugger to the 'MSI Debug' msiexec.exe process now for debug. Click OK when ready...", "MSI Debug");

            Context.LogMessage("Uninstalling Myrtille.Admin.Services");

            try
            {
                // if needed
            }
            catch (Exception exc)
            {
                Context.LogMessage(string.Format("Failed to uninstall Myrtille.Admin.Services ({0})", exc));
                throw;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(
            bool disposing)
		{
            if (disposing)
			{
                if (components != null)
				{
					components.Dispose();
				}
			}
            base.Dispose(disposing);
		}        
	}
}