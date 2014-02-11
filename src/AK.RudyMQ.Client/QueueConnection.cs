/*******************************************************************************************************************************
 * AK.RudyMQ.Client.QueueConnection
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

using AK.RudyMQ.Common;

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Internal concrete implementation of IQueueConnection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class QueueConnection : IQueueConnection
    {
        private readonly string queueAdminAddress;
        private readonly string queueAddress;

        public QueueConnection(string hostName, int port, string baseAddress)
        {
            var queueAdminAddressBuilder = new AddressBuilder
            {
                HostName = hostName,
                Port = port,
                BaseAddress = baseAddress + "/QueueAdmin.svc"
            };

            var queueAddressBuilder = new AddressBuilder
            {
                HostName = hostName,
                Port = port,
                BaseAddress = baseAddress + "/Queue.svc"
            };

            this.queueAdminAddress = queueAdminAddressBuilder.BuildAddress();
            this.queueAddress = queueAddressBuilder.BuildAddress();
        }

        public void Create(string queueName, bool isPersisted, bool isTransactional)
        {
            ServiceExecutor.Execute<IQueueAdminService>(
                this.queueAdminAddress,
                s =>
                s.Create(new QueueInfo
                {
                    Name = queueName,
                    IsPersisted = isPersisted,
                    IsTransactional = isTransactional
                }));
        }

        public void Remove(string queueName)
        {
            ServiceExecutor.Execute<IQueueAdminService>(this.queueAdminAddress, s => s.Remove(queueName));
        }

        public IQueue Get(string queueName)
        {
            var queueInfo = ServiceExecutor.Execute<IQueueAdminService, QueueInfo>(
                this.queueAdminAddress, s => s.GetInfo(queueName));

            return new Queue(this.queueAddress, this.queueAdminAddress, queueInfo);
        }
    }
}