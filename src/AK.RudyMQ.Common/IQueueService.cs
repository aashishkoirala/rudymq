/*******************************************************************************************************************************
 * AK.RudyMQ.Common.IQueueService
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
using System.ServiceModel;

#endregion

namespace AK.RudyMQ.Common
{
    /// <summary>
    /// WCF service contract for queue operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ServiceContract(Namespace = "http://rudymq.aashishkoirala.com")]
    public interface IQueueService
    {
        /// <summary>
        /// Adds the given message to the queue.
        /// </summary>
        /// <param name="message">Message to add.</param>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        void Enqueue(QueueMessage message);

        /// <summary>
        /// Removes and returns the next message from the queue.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <param name="type">Type of message.</param>
        /// <returns>QueueMessage, or NULL if there's nothing in the queue.</returns>
        [OperationContract]
        [FaultContract(typeof(QueueErrorInfo))]
        QueueMessage Dequeue(string queueName, string type);

        /// <summary>
        /// For persisted and/or transactional queues, commits the dequeue operation
        /// so that the queue server knows that the message has definitely been received
        /// by the client.
        /// </summary>
        /// <param name="messageId">Id of message.</param>
        [OperationContract(IsOneWay = true)]
        void CommitDequeue(Guid messageId);
    }
}