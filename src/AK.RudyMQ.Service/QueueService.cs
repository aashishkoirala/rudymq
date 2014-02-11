/*******************************************************************************************************************************
 * AK.RudyMQ.Service.QueueService
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
using System.ServiceModel;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// WCF service implementation of IQueueService. Singleton.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class QueueService : IQueueService
    {
        private readonly QueueManager manager = QueueManager.Instance;

        /// <summary>
        /// See IQueueService.
        /// </summary>
        public void Enqueue(QueueMessage message)
        {
            try
            {
                message.Id = Guid.NewGuid();
                this.manager.Get(message.QueueName).Enqueue(message);
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotEnqueue);
                errorInfo.QueueMessage = message;

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueService.
        /// </summary>
        public QueueMessage Dequeue(string queueName, string type)
        {
            try
            {
                return this.manager.Get(queueName).Dequeue(type);
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotDequeue);
                errorInfo.QueueMessage = new QueueMessage {QueueName = queueName, Type = type};

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueService.
        /// </summary>
        public void CommitDequeue(Guid messageId)
        {
            this.manager.CommitDequeue(messageId);
        }
    }
}