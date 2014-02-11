/*******************************************************************************************************************************
 * AK.RudyMQ.Service.Host.MessageQueueService
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

using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

#endregion

namespace AK.RudyMQ.Service.Host
{
    /// <summary>
    /// Windows service that hosts the message queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class MessageQueueService : ServiceBase
    {
        private QueueHost queueHost;

        public static void Main()
        {
            Run(new ServiceBase[] { new MessageQueueService() });
        }

        public MessageQueueService()
        {
            this.ServiceName = "RudyMQ.Service";
        }

        protected override void OnStart(string[] args)
        {
            var catalogLocation = GetConfig("catalogLocation", "queues.cat");
            var persistLocation = GetConfig("persistLocation", @"messages\persisted");
            var transitLocation = GetConfig("transitLocation", @"messages\transit");

            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RudyMQ");

            if (!Path.IsPathRooted(catalogLocation)) catalogLocation = Path.Combine(dir, catalogLocation);
            if (!Path.IsPathRooted(persistLocation)) persistLocation = Path.Combine(dir, persistLocation);
            if (!Path.IsPathRooted(transitLocation)) transitLocation = Path.Combine(dir, transitLocation);

            if (!Directory.Exists(persistLocation)) Directory.CreateDirectory(persistLocation);
            if (!Directory.Exists(transitLocation)) Directory.CreateDirectory(transitLocation);

            this.queueHost = new QueueHost(
                GetConfig("hostName", "localhost"), 
                GetConfig("port", 8377), 
                GetConfig("baseAddress", ""), 
                catalogLocation, 
                persistLocation, 
                transitLocation, 
                GetConfig("transitCleanupInterval", 600000), 
                GetConfig("transitMaximumAge", 600000));

            this.queueHost.Open();
        }

        protected override void OnStop()
        {
            if (this.queueHost == null) return;

            this.queueHost.Dispose();
            this.queueHost = null;
        }

        private static T GetConfig<T>(string key, T defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : (T) Convert.ChangeType(value, typeof (T));
        }
    }
}