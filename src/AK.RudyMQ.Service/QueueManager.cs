/*******************************************************************************************************************************
 * AK.RudyMQ.Service.QueueManager
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// Singleton that handles the queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class QueueManager : IDisposable
    {
        #region Fields

        private static QueueManager instance;
        private readonly static object instanceLock = new object();

        private readonly string catalogLocation;
        private readonly string persistLocation;
        private readonly string transitLocation;

        private readonly Locked<IDictionary<string, MessageQueue>> queueMap =
            new Locked<IDictionary<string, MessageQueue>>(new Dictionary<string, MessageQueue>());
        private readonly BinaryFormatter formatter = new BinaryFormatter();
        private readonly TransitCleanupOperation transitCleanupOperation;

        #endregion

        #region Constructor

        private QueueManager(string catalogLocation, string persistLocation, string transitLocation, 
            int transitCleanupInterval, int transitMaximumAge)
        {
            this.catalogLocation = catalogLocation;
            this.persistLocation = persistLocation;
            this.transitLocation = transitLocation;
            this.LoadQueueInfos();
            this.LoadPersistedQueues();
            this.transitCleanupOperation = TransitCleanupOperation.Start(this.transitLocation,
                transitCleanupInterval, transitMaximumAge,
                message => this.Get(message.QueueName).Enqueue(message));
        }

        #endregion

        #region IDisposable

        ~QueueManager()
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

            this.queueMap.Dispose();
            this.transitCleanupOperation.Stop();
            this.transitCleanupOperation.Dispose();
        }

        #endregion

        #region Properties/Methods - Public Static

        /// <summary>
        /// Gets the one and only instance. Initialize must be called before referencing this.
        /// </summary>
        public static QueueManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        throw new FaultException<QueueErrorInfo>(
                            QueueErrorInfo.FromErrorCode(QueueErrorCode.ServerNotInitialized));
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// Initializes the singleton queue manager.
        /// </summary>
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
        public static void Initialize(string catalogLocation, string persistLocation, string transitLocation, 
            int transitCleanupInterval, int transitMaximumAge)
        {
            lock (instanceLock)
            {
                if (instance != null) return;

                try
                {
                    instance = new QueueManager(catalogLocation, persistLocation, transitLocation,
                        transitCleanupInterval, transitMaximumAge);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex);

                    throw new FaultException<QueueErrorInfo>(
                        QueueErrorInfo.FromErrorCode(QueueErrorCode.ServerInitializationFailed));
                }
            }
        }

        /// <summary>
        /// Shuts down the singleton instance.
        /// </summary>
        public static void ShutDown()
        {
            lock (instanceLock)
            {
                if (instance == null) return;

                instance.Dispose();
                instance = null;
            }
        }

        #endregion

        #region Methods - Public

        public MessageQueue Get(string queueName)
        {
            MessageQueue queue = null;

            this.queueMap.ExecuteWithinReadLock(value =>
            {
                if (value.TryGetValue(queueName, out queue)) return;

                var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.QueueDoesNotExist);
                errorInfo.QueueInfo = new QueueInfo {Name = queueName};

                throw new FaultException<QueueErrorInfo>(errorInfo);
            });

            return queue;
        }

        public void Create(QueueInfo queueInfo)
        {
            if (queueInfo.IsTransactional && !queueInfo.IsPersisted)
            {
                throw new FaultException<QueueErrorInfo>(
                    QueueErrorInfo.FromErrorCode(QueueErrorCode.TransactionalQueueMustBePersisted));
            }

            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                if (value.ContainsKey(queueInfo.Name))
                {
                    var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.QueueAlreadyExists);
                    errorInfo.QueueInfo = queueInfo;

                    throw new FaultException<QueueErrorInfo>(errorInfo);
                }

                value[queueInfo.Name] = new MessageQueue(queueInfo, this.persistLocation, this.transitLocation);

                this.SaveQueueInfos(value.Values.Select(x => x.QueueInfo).ToArray());
            });
        }

        public void Remove(string queueName)
        {
            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                if (!value.ContainsKey(queueName))
                {
                    var errorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.QueueDoesNotExist);
                    errorInfo.QueueInfo = new QueueInfo { Name = queueName };
                }

                value[queueName].PurgeAll();
                value.Remove(queueName);

                this.SaveQueueInfos(value.Values.Select(x => x.QueueInfo).ToArray());
            });
        }

        public void CommitDequeue(Guid messageId)
        {
            var file = Path.Combine(this.transitLocation, string.Format("{0}.qm", messageId));
            if (File.Exists(file)) File.Delete(file);
        }

        #endregion

        #region Methods - Private

        private void LoadQueueInfos()
        {
            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                value.Clear();
                if (!File.Exists(catalogLocation)) return;

                QueueInfo[] infos;
                using (var fs = File.OpenRead(this.catalogLocation))
                {
                    infos = (QueueInfo[]) formatter.Deserialize(fs);
                }
                
                foreach (var info in infos)
                {
                    value[info.Name] = new MessageQueue(info, this.persistLocation, this.transitLocation);
                }
            });
        }

        private void SaveQueueInfos(IEnumerable infos)
        {
            using (var fs = File.OpenWrite(this.catalogLocation))
            {
                formatter.Serialize(fs, infos);
                fs.Flush();
            }
        }

        private void LoadPersistedQueues()
        {
            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                var files = Directory.GetFiles(this.persistLocation, "*.qm");
                var messages = new List<QueueMessage>();
                foreach (var file in files)
                {
                    using (var fs = File.OpenRead(file))
                    {
                        var message = (QueueMessage) formatter.Deserialize(fs);
                        messages.Add(message);
                    }
                }

                foreach (var queue in value.Values) queue.Load(messages);
            });
        }

        #endregion
    }
}