/*******************************************************************************************************************************
 * AK.RudyMQ.Service.QueueAdminService
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
    /// WCF service implementation of IQueueAdminService. Singleton.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class QueueAdminService : IQueueAdminService
    {
        private readonly QueueManager manager = QueueManager.Instance;

        /// <summary>
        /// See IQueueAdminService.
        /// </summary>
        public QueueInfo GetInfo(string queueName)
        {
            try
            {
                return this.manager.Get(queueName).QueueInfo;
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotGetQueueInfo);
                errorInfo.QueueInfo = new QueueInfo {Name = queueName};

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueAdminService.
        /// </summary>
        public void Create(QueueInfo queueInfo)
        {
            try
            {
                this.manager.Create(queueInfo);
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotCreateQueue);
                errorInfo.QueueInfo = queueInfo;

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueAdminService.
        /// </summary>
        public void Remove(string queueName)
        {
            try
            {
                this.manager.Remove(queueName);
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotRemoveQueue);
                errorInfo.QueueInfo = new QueueInfo {Name = queueName};

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueAdminService.
        /// </summary>
        public void Purge(string queueName, string type)
        {
            try
            {
                this.manager.Get(queueName).Purge(type);
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotPurgeQueue);
                errorInfo.QueueInfo = new QueueInfo { Name = queueName };
                errorInfo.QueueMessage = new QueueMessage {Type = type};

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }

        /// <summary>
        /// See IQueueAdminService.
        /// </summary>
        public void PurgeAll(string queueName)
        {
            try
            {
                this.manager.Get(queueName).PurgeAll();
            }
            catch (FaultException<QueueErrorInfo>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.CannotPurgeQueue);
                errorInfo.QueueInfo = new QueueInfo { Name = queueName };

                throw new FaultException<QueueErrorInfo>(errorInfo);
            }
        }
    }
}