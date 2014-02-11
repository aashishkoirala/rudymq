/*******************************************************************************************************************************
 * AK.RudyMQ.Service.Host.ServiceInstaller
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of RudyMQ.
 *  
 * RudyMQ is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RudyMQ is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RudyMQ.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

#endregion

namespace AK.RudyMQ.Service.Host
{
    /// <summary>
    /// Windows service installer for RudyMQ Message Queue Service.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        public ServiceInstaller()
        {
            const string serviceName = "RudyMQ Message Queuing Service";

            var serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.NetworkService,
                Password = null,
                Username = null
            };
            
            var serviceInstaller = new System.ServiceProcess.ServiceInstaller
            {
                Description = serviceName,
                DisplayName = serviceName,
                ServiceName = serviceName,
                StartType = ServiceStartMode.Automatic
            };

            this.Installers.AddRange(new Installer[]
            {
                serviceProcessInstaller,
                serviceInstaller
            });
        }
    }
}