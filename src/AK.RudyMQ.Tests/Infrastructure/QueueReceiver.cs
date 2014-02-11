/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Infrastructure.QueueReceiver
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

using AK.RudyMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace AK.RudyMQ.Tests.Infrastructure
{
    /// <summary>
    /// Queue receiver for end-to-end tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class QueueReceiver : IDisposable
    {
        private readonly IList<IReceiveOperation> operations = new List<IReceiveOperation>();
        private readonly IList<TestMessage1> message1List = new List<TestMessage1>();
        private readonly IList<TestMessage2> message2List = new List<TestMessage2>();
        private readonly IList<QueueException> exceptions = new List<QueueException>();
        private bool isStopped;

        public QueueReceiver(IQueueConnection connection, int threadCount, int delay, string queueName)
        {
            for (var i = 0; i < threadCount; i++)
            {
                var queue = connection.Get(queueName);
                var index = i + 1;

                var op1 = queue.StartReceiving<TestMessage1>(delay,
                    m =>
                    {
                        Console.WriteLine("RECEIVER{0}[{1}]: Received TestMessage1- {2}", index, queueName, m);
                        this.message1List.Add(m);
                    },
                    e =>
                    {
                        Console.WriteLine("RECEIVER{0}[{1}]: Error- {2}", index, queueName, e.Message);
                        this.exceptions.Add(e);
                    });

                var op2 = queue.StartReceiving<TestMessage2>(delay,
                    m =>
                    {
                        Console.WriteLine("RECEIVER{0}[{1}]: Received TestMessage2- {2}", index, queueName, m);
                        this.message2List.Add(m);
                    },
                    e =>
                    {
                        Console.WriteLine("RECEIVER{0}[{1}]: Error- {2}", index, queueName, e.Message);
                        this.exceptions.Add(e);
                    });

                this.operations.Add(op1);
                this.operations.Add(op2);
            }
        }

        ~QueueReceiver()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Stop()
        {
            if (this.isStopped) return;

            foreach (var operation in this.operations)
            {
                operation.Stop();
                if (operation.Exceptions != null)
                {
                    foreach (var ex in operation.Exceptions)
                        this.exceptions.Add(ex);
                }
                operation.Dispose();
            }
            this.isStopped = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            this.Stop();
        }

        public IEnumerable<QueueException> Exceptions
        {
            get { return this.exceptions.ToArray(); }
        }

        public IEnumerable<T> GetMessages<T>()
        {
            if (typeof(T) == typeof(TestMessage1)) return this.message1List.Cast<T>().ToArray();
            if (typeof(T) == typeof(TestMessage2)) return this.message2List.Cast<T>().ToArray();
            return Enumerable.Empty<T>();
        }
    }
}
