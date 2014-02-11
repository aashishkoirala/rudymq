/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Infrastructure.QueueSender
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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace AK.RudyMQ.Tests.Infrastructure
{
    /// <summary>
    /// Queue-sender for end-to-end tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class QueueSender : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IList<Task> tasks = new List<Task>();
        private readonly IList<TestMessage1> message1List = new List<TestMessage1>();
        private readonly IList<TestMessage2> message2List = new List<TestMessage2>();
        private readonly IList<QueueException> exceptions = new List<QueueException>();
        private bool isStopped;

        public QueueSender(IQueueConnection connection, int threadCount, int delay, string queueName)
        {
            for (var i = 0; i < threadCount; i++)
            {
                var token = this.cancellationTokenSource.Token;
                var index = i + 1;
                var task = Task.Factory.StartNew(() =>
                {
                    var queue = connection.Get(queueName);

                    while (true)
                    {
                        if (token.IsCancellationRequested) return;

                        this.SendMessage(queue, index, queueName);

                        if (token.IsCancellationRequested) return;
                        Thread.Sleep(delay);
                    }

                }, token);
                this.tasks.Add(task);
            }
        }

        ~QueueSender()
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

            this.Stop();
        }

        public void Stop()
        {
            if (this.isStopped) return;

            this.cancellationTokenSource.Cancel();
            this.exceptions.Clear();
            foreach (var task in this.tasks)
            {
                try
                {
                    if (!task.IsCanceled && !task.IsCompleted)
                        task.Wait();
                }
                catch (AggregateException ex)
                {
                    foreach (var innerEx in ex.InnerExceptions.Where(x => !(x is TaskCanceledException)))
                    {
                        this.exceptions.Add(innerEx as QueueException ?? new QueueException(innerEx));
                    }
                }
                task.Dispose();
            }
            this.cancellationTokenSource.Dispose();

            this.isStopped = true;
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

        private void SendMessage(IQueue queue, int index, string queueName)
        {
            var random = new Random();
            var texts = ((DayOfWeek[])Enum.GetValues(typeof(DayOfWeek))).Select(x => x.ToString()).ToArray();
            var initialTicks = DateTime.Now.AddYears(-20).Ticks;
            var finalTicks = DateTime.Now.Ticks;

            var msgNum = (random.Next(1, 100) % 2 == 0) ? 1 : 2;
            switch (msgNum)
            {
                case 1:
                    var testMessage1 = new TestMessage1
                    {
                        Id = Guid.NewGuid(),
                        Field1 = texts[random.Next((int)DayOfWeek.Sunday, (int)DayOfWeek.Saturday)],
                        Field2 = random.Next(1, 100),
                        Field3 = new DateTime(initialTicks + random.Next(0, (int)(finalTicks - initialTicks)))
                    };

                    Console.WriteLine("SENDER{0}[{1}]: Sending TestMessage1 - {2}", index, queueName, testMessage1);
                    queue.Send(testMessage1);
                    this.message1List.Add(testMessage1);
                    break;

                case 2:
                    var testMessage2 = new TestMessage2
                    {
                        Id = Guid.NewGuid(),
                        Field4 = texts[random.Next((int)DayOfWeek.Sunday, (int)DayOfWeek.Saturday)],
                        Field5 = random.Next(1, 2) == 1,
                        Field6 = random.Next(5, 150)
                    };

                    Console.WriteLine("SENDER{0}[{1}]: Sending TestMessage2 - {2}", index, queueName, testMessage2);
                    queue.Send(testMessage2);
                    this.message2List.Add(testMessage2);
                    break;
            }
        }
    }
}