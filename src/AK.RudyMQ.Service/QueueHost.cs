/*******************************************************************************************************************************
 * AK.RudyMQ.Service.QueueHost
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

using AK.RudyMQ.Common;
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// Lets you host the message queue and listen for messages/commands. Internally opens two WCF hosts, one for
    /// the queue service and another for queue administration.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class QueueHost : IDisposable
    {
        /// <summary>
        /// The WCF service host for queue service.
        /// </summary>
        public ServiceHost QueueServiceHost { get; private set; }

        /// <summary>
        /// The WCF service host for queue administration service.
        /// </summary>
        public ServiceHost QueueAdminServiceHost { get; private set; }

        /// <summary>
        /// Creates a new instance of the queue host using the given parameters.
        /// </summary>
        /// <param name="hostName">Name/address of machine where queue will be hosted.</param>
        /// <param name="port">TCP port that queue will listen on.</param>
        /// <param name="baseAddress">Base address, if any (will be part of the queue address).</param>
        /// <param name="catalogLocation">File name where the queue metadata is stored.</param>
        /// <param name="persistLocation">Folder where messages are persisted.</param>
        /// <param name="transitLocation">
        /// Folder where messages are temporarily kept 
        /// during dequeue until committed.
        /// </param>
        /// <param name="transitCleanupInterval">
        /// Period in milliseconds used to scan and 
        /// cleanup (i.e. re-queue) uncommitted messages.
        /// </param>
        /// <param name="transitMaximumAge">
        /// For uncommitted messages, the maximum number of 
        /// milliseconds they stay that way before being cleaned up (i.e. re-queued).
        /// </param>
        public QueueHost(string hostName, int port, string baseAddress, string catalogLocation,
                         string persistLocation, string transitLocation, int transitCleanupInterval,
                         int transitMaximumAge)
        {
            QueueManager.Initialize(catalogLocation, persistLocation,
                                    transitLocation, transitCleanupInterval, transitMaximumAge);

            var queueAddressBuilder = new AddressBuilder
            {
                HostName = hostName,
                Port = port,
                BaseAddress = baseAddress + "/Queue.svc"
            };

            var queueAdminAddressBuilder = new AddressBuilder
            {
                HostName = hostName,
                Port = port,
                BaseAddress = baseAddress + "/QueueAdmin.svc"
            };

            var queueAddress = queueAddressBuilder.BuildAddress();
            var queueAdminAddress = queueAdminAddressBuilder.BuildAddress();

            try
            {
                this.QueueServiceHost = new ServiceHost(typeof (QueueService));
                this.QueueAdminServiceHost = new ServiceHost(typeof (QueueAdminService));

                this.QueueServiceHost.AddServiceEndpoint(typeof (IQueueService), new NetTcpBinding(), queueAddress);
                this.QueueAdminServiceHost.AddServiceEndpoint(typeof (IQueueAdminService),
                                                              new NetTcpBinding(), queueAdminAddress);

                foreach (var serviceMetadataBehavior in new[] {this.QueueServiceHost, this.QueueAdminServiceHost}
                    .Select(x => x.Description.Behaviors.Find<ServiceMetadataBehavior>())
                    .Where(x => x != null))
                {
                    serviceMetadataBehavior.HttpGetEnabled = false;
                    serviceMetadataBehavior.HttpsGetEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                throw new FaultException<QueueErrorInfo>(
                    QueueErrorInfo.FromErrorCode(QueueErrorCode.ServerHostInitializationFailed));
            }
        }

        /// <summary>
        /// Starts listening.
        /// </summary>
        public void Open()
        {
            try
            {
                this.QueueServiceHost.Open();
                this.QueueAdminServiceHost.Open();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                throw new FaultException<QueueErrorInfo>(
                    QueueErrorInfo.FromErrorCode(QueueErrorCode.ServerHostInitializationFailed));
            }
        }

        #region IDisposable

        ~QueueHost()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.QueueServiceHost.Close();
            this.QueueAdminServiceHost.Close();

            QueueManager.ShutDown();
        }

        #endregion
    }
}