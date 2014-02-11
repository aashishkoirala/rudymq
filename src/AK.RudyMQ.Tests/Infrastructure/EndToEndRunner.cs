/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Infrastructure.EndToEndRunner
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
using AK.RudyMQ.Common;
using AK.RudyMQ.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace AK.RudyMQ.Tests.Infrastructure
{
    /// <summary>
    /// End-to-end test runner.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class EndToEndRunner
    {
        private const int TimeToAllowReceiverToCatchUp = 5000;

        private readonly string hostName;
        private readonly int port;
        private readonly string baseAddress;
        private string catalogLocation;
        private string persistLocation;
        private string transitLocation;
        private readonly int transitCleanupInterval;
        private readonly int transitMaximumAge;
        private readonly string[] queueNames;
        private readonly int senderCount;
        private readonly int senderDelay;
        private readonly int receiverCount;
        private readonly int receiverDelay;
        private readonly bool isPersisted;
        private readonly bool isTransactional;
        private readonly IQueueConnection connection;

        public IEnumerable<QueueException> SendExceptions { get; private set; }
        public IEnumerable<QueueException> ReceiveExceptions { get; private set; }
        public IDictionary<string, IEnumerable<TestMessage1>> SentMessage1List { get; private set; }
        public IDictionary<string, IEnumerable<TestMessage1>> ReceivedMessage1List { get; private set; }
        public IDictionary<string, IEnumerable<TestMessage2>> SentMessage2List { get; private set; }
        public IDictionary<string, IEnumerable<TestMessage2>> ReceivedMessage2List { get; private set; }

        private static readonly object exceptionsLock = new object();

        public EndToEndRunner(string hostName, 
            int port, 
            string baseAddress, 
            string catalogLocation, 
            string persistLocation, 
            string transitLocation, 
            int transitCleanupInterval, 
            int transitMaximumAge, 
            string[] queueNames, 
            int senderCount, 
            int senderDelay, 
            int receiverCount, 
            int receiverDelay,
            bool isPersisted,
            bool isTransactional)
        {
            this.hostName = hostName;
            this.port = port;
            this.baseAddress = baseAddress;
            this.catalogLocation = catalogLocation;
            this.persistLocation = persistLocation;
            this.transitLocation = transitLocation;
            this.transitCleanupInterval = transitCleanupInterval;
            this.transitMaximumAge = transitMaximumAge;
            this.queueNames = queueNames;
            this.senderCount = senderCount;
            this.senderDelay = senderDelay;
            this.receiverCount = receiverCount;
            this.receiverDelay = receiverDelay;
            this.isPersisted = isPersisted;
            this.isTransactional = isTransactional;
            this.connection = MessageQueue.Connect(hostName, port, baseAddress);
            this.ReceiveExceptions = new List<QueueException>();
            this.SendExceptions = new List<QueueException>();
            this.SentMessage1List = new Dictionary<string, IEnumerable<TestMessage1>>();
            this.SentMessage2List = new Dictionary<string, IEnumerable<TestMessage2>>();
            this.ReceivedMessage1List = new Dictionary<string, IEnumerable<TestMessage1>>();
            this.ReceivedMessage2List = new Dictionary<string, IEnumerable<TestMessage2>>();
        }

        public void RunHappyPath(TimeSpan duration)
        {
            this.Initialize();
            using (var manualResetEvent = new ManualResetEvent(false))
            {
                using (this.StartListening())
                {
                    var tasks = this.queueNames.Select(x => Task.Factory.StartNew(() =>
                    {
                        this.TryCreateQueue(x, this.isPersisted, this.isTransactional);
                        using (var receiver = this.StartReceiving(x))
                        {
                            using (var sender = this.StartSending(x))
                            {
                                manualResetEvent.WaitOne();
                                Console.WriteLine("Stopping sending...");
                                sender.Stop();

                                this.SentMessage1List[x] = sender.GetMessages<TestMessage1>();
                                this.SentMessage2List[x] = sender.GetMessages<TestMessage2>();
                                lock (exceptionsLock)
                                {
                                    this.SendExceptions = this.SendExceptions.Union(sender.Exceptions).ToList();
                                }
                            }

                            Console.WriteLine("Stopped sending.");
                            Thread.Sleep(TimeToAllowReceiverToCatchUp);
                            Console.WriteLine("Stopping receiving...");
                            receiver.Stop();

                            this.ReceivedMessage1List[x] = receiver.GetMessages<TestMessage1>();
                            this.ReceivedMessage2List[x] = receiver.GetMessages<TestMessage2>();
                            lock (exceptionsLock)
                            {
                                this.ReceiveExceptions = this.ReceiveExceptions.Union(receiver.Exceptions).ToList();
                            }
                        }
                        Console.WriteLine("Stopped receiving.");
                    })).ToList();

                    Thread.Sleep(duration);
                    manualResetEvent.Set();

                    var tasksToWaitFor = tasks.Where(task => !task.IsCanceled && !task.IsCompleted).ToArray();
                    Task.WaitAll(tasksToWaitFor);

                    Console.WriteLine("Stopping listening...");
                }
                Console.WriteLine("Stopped listening.");
            }
        }

        public void RunPersistedMessagesTest()
        {
            
        }

        private QueueHost StartListening()
        {
            Console.WriteLine("SERVER: Initializing...");
            var queueHost = new QueueHost(this.hostName, this.port, this.baseAddress, this.catalogLocation,
                this.persistLocation, this.transitLocation, this.transitCleanupInterval, this.transitMaximumAge);

            Console.WriteLine("SERVER: Initialized. Opening...");
            queueHost.Open();

            Console.WriteLine("SERVER: Opened, now listening.");
            return queueHost;
        }

        private QueueReceiver StartReceiving(string receiveQueueName)
        {
            Console.WriteLine("RECEIVER: Initializing...");
            var receiver = new QueueReceiver(this.connection, this.receiverCount, this.receiverDelay, receiveQueueName);

            Console.WriteLine("RECEIVER: Initialized. Now receiving.");
            return receiver;
        }

        private QueueSender StartSending(string sendQueueName)
        {
            Console.WriteLine("SENDER: Initializing...");
            var sender = new QueueSender(this.connection, this.senderCount, this.senderDelay, sendQueueName);

            Console.WriteLine("SENDER: Initialized. Now sending.");
            return sender;
        }

        private void Initialize()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(dir != null);

            if (!Path.IsPathRooted(this.catalogLocation)) this.catalogLocation = Path.Combine(dir, this.catalogLocation);
            if (!Path.IsPathRooted(this.persistLocation)) this.persistLocation = Path.Combine(dir, this.persistLocation);
            if (!Path.IsPathRooted(this.transitLocation)) this.transitLocation = Path.Combine(dir, this.transitLocation);

            if (!Directory.Exists(this.persistLocation)) Directory.CreateDirectory(this.persistLocation);
            if (!Directory.Exists(this.transitLocation)) Directory.CreateDirectory(this.transitLocation);
            
            foreach (var file in Directory.GetFiles(this.persistLocation, "*.qm")) File.Delete(file);
            foreach (var file in Directory.GetFiles(this.transitLocation, "*.qm")) File.Delete(file);
        }

        private void TryCreateQueue(string queueToCreate, bool isQueuePersisted, bool isQueueTransactional)
        {
            try
            {
                this.connection.Create(queueToCreate, isQueuePersisted, isQueuePersisted && isQueueTransactional);
            }
            catch (QueueException ex)
            {
                if (ex.ErrorInfo.ErrorCode != QueueErrorCode.QueueAlreadyExists) throw;
            }
        }
    }
}