/*******************************************************************************************************************************
 * AK.RudyMQ.Client.Queue
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Internal concrete implementation of IQueue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class Queue : IQueue
    {
        #region Fields

        private readonly string queueAddress;
        private readonly string queueAdminAddress;
        private readonly QueueInfo queueInfo;
        private readonly BinaryFormatter formatter = new BinaryFormatter();

        #endregion

        #region Constructor

        public Queue(string queueAddress, string queueAdminAddress, QueueInfo queueInfo)
        {
            this.queueAddress = queueAddress;
            this.queueAdminAddress = queueAdminAddress;
            this.queueInfo = queueInfo;
        }

        #endregion

        #region Methods (Public - IQueue)

        public void Send<T>(T message)
        {
            // TODO: Throw if we cannot construct msg

            byte[] body;
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, message);
                body = ms.ToArray();
            }

            var queueMessage = new QueueMessage
            {
                QueueName = this.queueInfo.Name,
                Type = typeof (T).FullName,
                Body = body
            };

            ServiceExecutor.Execute<IQueueService>(this.queueAddress, s => s.Enqueue(queueMessage));
        }

        public void Purge<T>()
        {
            var type = typeof (T).FullName;
            ServiceExecutor.Execute<IQueueAdminService>(this.queueAdminAddress, s => s.Purge(this.queueInfo.Name, type));
        }

        public void PurgeAll()
        {
            ServiceExecutor.Execute<IQueueAdminService>(this.queueAdminAddress, s => s.PurgeAll(this.queueInfo.Name));
        }

        public IReceiveOperation StartReceiving<T>(int pollInterval, Action<T> messageHandler, Action<QueueException> errorHandler = null)
        {
            var operation = new ReceiveOperation<T>(pollInterval, this.queueInfo, () =>
            {
                var queueResult = new QueueResult();

                try
                {
                    queueResult.Message = ServiceExecutor.Execute<IQueueService, QueueMessage>(this.queueAddress,
                        s => s.Dequeue(this.queueInfo.Name, typeof(T).FullName));
                }
                catch(QueueException queueException)
                {
                    queueResult.Exception = queueException;
                }
                catch (Exception ex)
                {
                    queueResult.Exception = new QueueException(ex);
                }

                return queueResult;

            }, 
            messageHandler, 
            errorHandler, 
            id => ServiceExecutor.Execute<IQueueService>(this.queueAddress, s => s.CommitDequeue(id)));

            operation.Start();
            return operation;
        }

        #endregion

        #region Methods (Public - Others)

        // This method is used internally by the WCF binding and does not need to be exposed as part of IQueue.
        //
        public T Receive<T>()
        {
            T returnValue;

            using (var operation = new ReceiveOperation<T>(0, this.queueInfo, () =>
            {
                var queueResult = new QueueResult();

                try
                {
                    queueResult.Message = ServiceExecutor.Execute<IQueueService, QueueMessage>(this.queueAddress,
                        s => s.Dequeue(this.queueInfo.Name, typeof(T).FullName));
                }
                catch (QueueException queueException)
                {
                    queueResult.Exception = queueException;
                }
                catch (Exception ex)
                {
                    queueResult.Exception = new QueueException(ex);
                }

                return queueResult;

            }, null, null, id => ServiceExecutor.Execute<IQueueService>(this.queueAddress, s => s.CommitDequeue(id))))
            {
                returnValue = operation.Receive();
            }

            return returnValue;
        }

        #endregion
    }
}