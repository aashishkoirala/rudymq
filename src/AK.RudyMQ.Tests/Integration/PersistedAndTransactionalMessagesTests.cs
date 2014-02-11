/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Integration.PersistedAndTransactionalMessagesTests
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
using AK.RudyMQ.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion

namespace AK.RudyMQ.Tests.Integration
{
    /// <summary>
    /// Persistence-related integration tests for persisted and transactional queues.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class PersistedAndTransactionalMessagesTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void Persisted_Messages_Survive_Service_Restart()
        {
            var sentMessages = new List<TestMessage1>();
            var receivedMessages = new List<TestMessage1>();

            var random = new Random();
            var texts = ((DayOfWeek[])Enum.GetValues(typeof(DayOfWeek))).Select(x => x.ToString()).ToArray();
            var initialTicks = DateTime.Now.AddYears(-20).Ticks;
            var finalTicks = DateTime.Now.Ticks;

            for (var i = 0; i < 20; i++)
            {
                sentMessages.Add(new TestMessage1
                {
                    Id = Guid.NewGuid(),
                    Field1 = texts[random.Next((int) DayOfWeek.Sunday, (int) DayOfWeek.Saturday)],
                    Field2 = random.Next(1, 100),
                    Field3 = new DateTime(initialTicks + random.Next(0, (int) (finalTicks - initialTicks)))
                });
            }

            var hostName = Config.Get("hostName", "localhost");
            var port = Config.Get("port", 8377);
            var baseAddress = Config.Get("baseAddress", "");
            
            var conn = MessageQueue.Connect(hostName, port, baseAddress);
            const string queueName = "PersistedQueue1";

            using (var host = GetQueueHost(hostName, port, baseAddress))
            {
                host.Open();

                TryCreateQueue(conn, queueName, true, false);

                var queue = conn.Get(queueName);

                foreach (var message in sentMessages)
                    queue.Send(message);
            }

            Thread.Sleep(1000);

            using (var host = GetQueueHost(hostName, port, baseAddress))
            {
                host.Open();

                var queue = conn.Get(queueName);

                using (var op = queue.StartReceiving<TestMessage1>(100,
                    receivedMessages.Add, e => { throw e; }))
                {
                    Thread.Sleep(5000);

                    op.Stop();
                    Assert.IsFalse(op.Exceptions.Any());
                }
            }

            Assert.AreEqual(sentMessages.Count, receivedMessages.Count);

            foreach (var receivedMessage in sentMessages
                .Select(x => receivedMessages.SingleOrDefault(y => y.Equals(x))))
            {
                Assert.IsNotNull(receivedMessage);
            }
        }

        [TestMethod]
        public void Transactional_Messages_Put_Back_On_Queue_If_Client_Chokes()
        {
            var sentMessage = new TestMessage1 { Id = Guid.NewGuid() };
            TestMessage1 firstTimeReceivedMessage = null, secondTimeReceivedMessage = null;

            var hostName = Config.Get("hostName", "localhost");
            var port = Config.Get("port", 8377);
            var baseAddress = Config.Get("baseAddress", "");

            var conn = MessageQueue.Connect(hostName, port, baseAddress);

            const string queueName = "TransactionalQueue1";
            const int transitInterval = 10000;

            using (var host = GetQueueHost(hostName, port, baseAddress, transitInterval, transitInterval))
            {
                host.Open();

                TryCreateQueue(conn, queueName, true, true);
                var queue = conn.Get(queueName);

                queue.Send(sentMessage);

                using (var op = queue.StartReceiving<TestMessage1>(100, 
                    m =>
                    {
                        firstTimeReceivedMessage = m;
                        throw new Exception();
                    }))
                {
                    Thread.Sleep(2000);
                    op.Stop();
                }

                Thread.Sleep(2 * transitInterval);

                using (var op = queue.StartReceiving<TestMessage1>(100, m => secondTimeReceivedMessage = m))
                {
                    Thread.Sleep(2000);
                    op.Stop();
                }
            }

            Assert.AreEqual(sentMessage, firstTimeReceivedMessage);
            Assert.AreEqual(sentMessage, secondTimeReceivedMessage);
        }

        private static void TryCreateQueue(IQueueConnection connection, string queueName, bool isPersisted, bool isTransactional)
        {
            try
            {
                connection.Create(queueName, isPersisted, isTransactional);
            }
            catch (QueueException ex)
            {
                if (ex.ErrorInfo.ErrorCode != QueueErrorCode.QueueAlreadyExists) throw;
            }            
        }

        private static QueueHost GetQueueHost(string hostName, int port, string baseAddress, 
            int? transitCleanupInterval = null, int? transitMaximumAge = null)
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(baseDir != null);

            var persistLocation = Config.Get("persistLocation", @"messages\persisted");
            var transitLocation = Config.Get("persistLocation", @"messages\transit");

            if (!Path.IsPathRooted(persistLocation)) persistLocation = Path.Combine(baseDir, persistLocation);
            if (!Path.IsPathRooted(transitLocation)) transitLocation = Path.Combine(baseDir, transitLocation);

            if (!Directory.Exists(persistLocation)) Directory.CreateDirectory(persistLocation);
            if (!Directory.Exists(transitLocation)) Directory.CreateDirectory(transitLocation);

            return new QueueHost(hostName, port, baseAddress,
                                 Config.Get("catalogLocation", "queues.cat"),
                                 persistLocation, transitLocation,
                                 transitCleanupInterval ?? Config.Get("transitCleanupInterval", 600000),
                                 transitMaximumAge ?? Config.Get("transitMaximumAge", 600000));
        }
    }
}
