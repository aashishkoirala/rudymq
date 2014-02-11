/*******************************************************************************************************************************
 * AK.RudyMQ.Common.IQueueAdminService
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

using System.ServiceModel;

namespace AK.RudyMQ.Common
{
    /// <summary>
    /// WCF service contract for queue administration.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ServiceContract(Namespace = "http://rudymq.aashishkoirala.com")]
    public interface IQueueAdminService
    {
        /// <summary>
        /// Gets metadata about an existing queue.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <returns>QueueInfo with info about queue.</returns>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        QueueInfo GetInfo(string queueName);

        /// <summary>
        /// Creates a new queue with the given properties, throws if a queue with that name already exists.
        /// </summary>
        /// <param name="queueInfo">QueueInfo instance with queue properties including name.</param>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        void Create(QueueInfo queueInfo);

        /// <summary>
        /// Removes an existing queue.
        /// </summary>
        /// <param name="queueName">Name of queue to remove.</param>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        void Remove(string queueName);

        /// <summary>
        /// Purges messages of the given type from the given queue.
        /// </summary>
        /// <param name="queueName">Name of queue to purge.</param>
        /// <param name="type">Type of messages to purge.</param>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        void Purge(string queueName, string type);

        /// <summary>
        /// Purges all messages from the given queue.
        /// </summary>
        /// <param name="queueName">Name of queue to purge.</param>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        void PurgeAll(string queueName);
    }
}