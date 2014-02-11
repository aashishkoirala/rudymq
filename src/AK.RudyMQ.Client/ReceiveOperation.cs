/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ReceiveOperation
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Internal concrete implementation of IReceiveOperation.
    /// </summary>
    /// <typeparam name="T">Type of message that we're handling the receipt for.</typeparam>
    /// <author>Aashish Koirala</author>
    internal class ReceiveOperation<T> : IReceiveOperation
    {
        #region Fields

        private readonly int pollInterval;
        private readonly QueueInfo queueInfo;
        private readonly Func<QueueResult> messageReceiver;
        private readonly Action<T> messageHandler;
        private readonly Action<QueueException> errorHandler;
        private readonly Action<Guid> commitHandler;
        private readonly IList<QueueException> exceptions = new List<QueueException>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IList<Guid> receivedMessageIds = new List<Guid>();
        private readonly BinaryFormatter formatter = new BinaryFormatter();
        private Task task;

        #endregion

        #region Constructor

        public ReceiveOperation(int pollInterval, QueueInfo queueInfo, Func<QueueResult> messageReceiver, 
            Action<T> messageHandler, Action<QueueException> errorHandler, Action<Guid> commitHandler)
        {
            this.pollInterval = pollInterval;
            this.queueInfo = queueInfo;
            this.messageReceiver = messageReceiver;
            this.messageHandler = messageHandler;
            this.errorHandler = errorHandler;
            this.commitHandler = commitHandler;
        }

        #endregion

        #region IDisposable

        ~ReceiveOperation()
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

            this.cancellationTokenSource.Dispose();
        }

        #endregion

        #region Properties (Public - IReceiveOperation)

        public IEnumerable<QueueException> Exceptions
        {
            get { return this.exceptions.ToArray(); }
        }

        #endregion

        #region Methods (Public - IReceiveOperation)

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
            this.task.Wait();
            this.task.Dispose();
            this.task = null;
        }

        #endregion

        #region Methods (Public - Others)

        public void Start()
        {
            var token = this.cancellationTokenSource.Token;

            this.task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) return;

                    var result = this.messageReceiver();

                    if (result.Message == null)
                    {
                        this.HandleEmptyMessage(result.Exception);
                        Thread.Sleep(this.pollInterval);
                        continue;
                    }

                    this.HandleMessage(result.Message);
                }

            }, token).ContinueWith(x =>
            {
                if (x.Exception == null || x.Exception.InnerExceptions == null) return;

                foreach (var queueEx in x.Exception.InnerExceptions
                    .Select(ex => ex as QueueException ?? new QueueException(ex)))
                {
                    this.exceptions.Add(queueEx);
                }
            });
        }

        public T Receive()
        {
            var result = this.messageReceiver();

            if (result.Message == null)
            {
                this.HandleEmptyMessage(result.Exception);
                if (this.exceptions.Any())
                    throw this.exceptions.First();

                return default(T);
            }

            return this.HandleMessage(result.Message);
        }

        #endregion

        #region Methods (Private)

        private void HandleEmptyMessage(QueueException exception)
        {
            if (exception == null) return;

            this.exceptions.Add(exception);
            if (this.errorHandler != null)
            {
                Task.Factory
                    .StartNew(() => this.errorHandler(exception))
                    .ContinueWith(x =>
                    {
                        if (!x.IsCompleted && !x.IsCanceled && !x.IsFaulted) x.Wait();
                    });
            }
        }

        private T HandleMessage(QueueMessage message)
        {
            var receivedData = default(T);
            if (this.queueInfo.IsPersisted)
            {
                if (this.receivedMessageIds.Contains(message.Id)) return receivedData;
                this.receivedMessageIds.Add(message.Id);
            }

            if (message.Type != typeof(T).FullName) return receivedData;

            if (this.queueInfo.IsPersisted && !this.queueInfo.IsTransactional)
                this.commitHandler(message.Id);

            T data;
            using (var ms = new MemoryStream(message.Body))
            {
                data = (T) formatter.Deserialize(ms);
            }
            receivedData = data;

            if (this.messageHandler == null) return receivedData;

            Task.Factory.StartNew(() => this.messageHandler(data)).ContinueWith(x =>
            {
                if (this.queueInfo.IsPersisted && this.queueInfo.IsTransactional && !x.IsFaulted)
                    this.commitHandler(message.Id);

                if (!x.IsCompleted && !x.IsCanceled && !x.IsFaulted) x.Wait();
            });

            return receivedData;
        }

        #endregion
    }
}