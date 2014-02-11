/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Integration.PersistedHappyPathTests
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

using AK.RudyMQ.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

#endregion

namespace AK.RudyMQ.Tests.Integration
{
    /// <summary>
    /// Happy-path integration tests for persisted queues.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class PersistedHappyPathTests
    {
        private static EndToEndRunner runner;
        private static bool isRun;
        private static readonly object isRunLock = new object();
        
        [TestInitialize]
        public void EnsureRun()
        {
            lock (isRunLock)
            {
                if (isRun) return;

                runner = new EndToEndRunner(Config.Get("hostName", "localhost"),
                                            Config.Get("port", 8377),
                                            Config.Get("baseAddress", ""),
                                            Config.Get("catalogLocation", "queues.cat"),
                                            Config.Get("persistLocation", @"messages\persisted"),
                                            Config.Get("transitLocation", @"messages\transit"),
                                            Config.Get("transitCleanupInterval", 600000),
                                            Config.Get("transitMaximumAge", 600000),
                                            new[] { "P_Queue1", "P_Queue2", "P_Queue3" },
                                            10, 500, 5, 100, true, false);

                runner.RunHappyPath(TimeSpan.FromMinutes(1));

                isRun = true;
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Persisted_Happy_Path_Has_No_Receive_Exceptions()
        {
            Assert.IsFalse(runner.ReceiveExceptions.Any());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Persisted_Happy_Path_Has_No_Send_Exceptions()
        {
            Assert.IsFalse(runner.SendExceptions.Any());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Persisted_Happy_Path_Send_And_Receive_Counts_Match()
        {
            foreach (var queue in runner.SentMessage1List.Keys)
            {
                var count1Equal = runner.SentMessage1List[queue].Count() == runner.ReceivedMessage1List[queue].Count();
                var count2Equal = runner.SentMessage2List[queue].Count() == runner.ReceivedMessage2List[queue].Count();

                Assert.IsTrue(count1Equal && count2Equal);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Persisted_Happy_Path_All_Messages_Are_Accounted_For()
        {
            foreach (var eachQueue in runner.SentMessage1List.Keys)
            {
                var queue = eachQueue;
                foreach (var receivedMessage in runner.SentMessage1List[queue]
                    .Select(sentMessage => runner.ReceivedMessage1List[queue].SingleOrDefault(x => x.Equals(sentMessage))))
                {
                    Assert.IsNotNull(receivedMessage);
                }

                foreach (var receivedMessage in runner.SentMessage2List[queue]
                    .Select(sentMessage => runner.ReceivedMessage2List[queue].SingleOrDefault(x => x.Equals(sentMessage))))
                {
                    Assert.IsNotNull(receivedMessage);
                }
            }
        }
    }
}
