/*******************************************************************************************************************************
 * AK.RudyMQ.Service.MessageQueue
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// In-memory queue of messages where all messages go and are retrieved from. Essentially a glorified wrapper around
    /// a bunch of ConcurrentQueue's keyed by queue name and message type.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class MessageQueue : IDisposable
    {
        #region Fields

        private readonly QueueInfo queueInfo;
        private readonly string persistLocation;
        private readonly string transitLocation;
        private readonly BinaryFormatter formatter = new BinaryFormatter();

        private readonly Locked<IDictionary<string, ConcurrentQueue<QueueMessage>>> queueMap =
            new Locked<IDictionary<string, ConcurrentQueue<QueueMessage>>>(
                new Dictionary<string, ConcurrentQueue<QueueMessage>>());

        #endregion

        #region Constructor

        public MessageQueue(QueueInfo queueInfo, string persistLocation, string transitLocation)
        {
            this.queueInfo = queueInfo;
            this.persistLocation = persistLocation;
            this.transitLocation = transitLocation;
        }

        #endregion

        #region IDisposable

        ~MessageQueue()
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
            if (disposing) this.queueMap.Dispose();
        }

        #endregion

        #region Properties

        public QueueInfo QueueInfo
        {
            get { return this.queueInfo; }
        }

        #endregion

        #region Methods - Public

        public void Load(IEnumerable<QueueMessage> messages)
        {
            var filteredMessages = messages.Where(x => x.QueueName == this.queueInfo.Name).ToArray();
            var types = filteredMessages.Select(x => x.Type).Distinct().ToArray();

            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                value.Clear();
                foreach (var type in types)
                    value[type] =
                        new ConcurrentQueue<QueueMessage>(filteredMessages.Where(x => x.Type == type).ToArray());
            });
        }

        public void Enqueue(QueueMessage message)
        {
            var messageCopy = message;
            var messageType = messageCopy.Type;

            var queueExists = false;
            this.queueMap.ExecuteWithinReadLock(value => queueExists = value.ContainsKey(messageType));

            if (!queueExists)
            {
                this.queueMap.ExecuteWithinWriteLock(value =>
                {
                    if (value.ContainsKey(message.Type)) return;

                    value[message.Type] = new ConcurrentQueue<QueueMessage>();
                });
            }

            this.queueMap.ExecuteWithinReadLock(value =>
            {
                value[messageCopy.Type].Enqueue(messageCopy);
                if (this.queueInfo.IsPersisted) this.Persist(messageCopy);
            });
        }

        public QueueMessage Dequeue(string type)
        {
            var queueExists = false;
            var typeCopy = type;
            this.queueMap.ExecuteWithinReadLock(value => queueExists = value.ContainsKey(typeCopy));

            if (!queueExists) return null;

            QueueMessage message = null;

            this.queueMap.ExecuteWithinReadLock(value =>
            {
                var queue = value[type];
                if (queue.Count == 0) return;

                if (!queue.TryDequeue(out message)) return;

                if (message != null && this.queueInfo.IsPersisted) this.MoveToTransit(message);
            });

            return message;
        }

        public void Purge(string type)
        {
            var queueExists = false;
            var typeCopy = type;
            this.queueMap.ExecuteWithinReadLock(value => queueExists = value.ContainsKey(typeCopy));
            if (!queueExists) return;

            IEnumerable<Guid> messageIdList = null;

            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                if (!value.ContainsKey(type)) return;

                if (this.queueInfo.IsPersisted)
                {
                    messageIdList = value[type]
                        .ToArray()
                        .Select(x => x.Id);
                }

                value[type] = new ConcurrentQueue<QueueMessage>();
            });

            if (this.queueInfo.IsPersisted) this.DeletePersistedMessages(messageIdList);
        }

        public void PurgeAll()
        {
            IEnumerable<Guid> messageIdList = null;

            this.queueMap.ExecuteWithinWriteLock(value =>
            {
                if (this.queueInfo.IsPersisted)
                {
                    messageIdList = value.Values
                        .SelectMany(x => x.ToArray())
                        .Select(x => x.Id);
                }

                value.Clear();
            });

            if (this.queueInfo.IsPersisted) this.DeletePersistedMessages(messageIdList);
        }

        #endregion

        #region Methods - Private

        private void Persist(QueueMessage message)
        {
            var file = Path.Combine(this.persistLocation, string.Format("{0}.qm", message.Id));
            using (var fs = File.OpenWrite(file))
            {
                formatter.Serialize(fs, message);
                fs.Flush();
            }
        }

        private void MoveToTransit(QueueMessage message)
        {
            var fileName = string.Format("{0}.qm", message.Id);
            var sourceFile = Path.Combine(this.persistLocation, fileName);
            var targetFile = Path.Combine(this.transitLocation, fileName);

            File.Move(sourceFile, targetFile);
        }

        private void DeletePersistedMessages(IEnumerable<Guid> messageIdList)
        {
            foreach (var file in messageIdList
                .Select(x => Path.Combine(this.persistLocation, string.Format("{0}.qm", x)))
                .Where(File.Exists))
            {
                File.Delete(file);
            }
        }

        #endregion
    }
}