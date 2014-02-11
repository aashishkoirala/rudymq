/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Integration.WcfBindingTests
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
using AK.RudyMQ.Client.ServiceModel;
using AK.RudyMQ.Common;
using AK.RudyMQ.Service;
using AK.RudyMQ.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Tests.Integration
{
    /// <summary>
    /// Integration tests for the WCF binding.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class WcfBindingTests
    {
        [TestMethod]
        public void Wcf_Binding_Works()
        {
            const string hostName = "localhost";
            const int port = 8773;
            const string baseAddress = "MessageQueue";
            const string firstQueue = "FirstWcfQueue";
            const string secondQueue = "SecondWcfQueue";
            const string message1Value = "Message1Value";
            const string message2Value = "Message1Value";

            TestResultBag.Message1Value = TestResultBag.Message2Value = null;

            using (var queueHost = GetQueueHost(hostName, port, baseAddress))
            {
                queueHost.Open();

                var conn = MessageQueue.Connect(hostName, port, baseAddress);
                try
                {
                    conn.Create(firstQueue, true, false);
                    conn.Create(secondQueue, true, false);
                }
                catch (QueueException queueException)
                {
                    if (queueException.ErrorInfo.ErrorCode != QueueErrorCode.QueueAlreadyExists) throw;
                }

                using (var serviceHost = new ServiceHost(typeof (TestService)))
                {
                    var firstQueueAddress = string.Format("net.rudymq://{0}:{1}/{2}/{3}", hostName, port, baseAddress, firstQueue);
                    var secondQueueAddress = string.Format("net.rudymq://{0}:{1}/{2}/{3}", hostName, port, baseAddress, secondQueue);

                    serviceHost.AddServiceEndpoint(typeof (ITestService1), new RudyMqBinding(), firstQueueAddress);
                    serviceHost.AddServiceEndpoint(typeof (ITestService2), new RudyMqBinding(), secondQueueAddress);

                    serviceHost.Open();

                    var c1 = new TestServiceClient1(new RudyMqBinding(), firstQueueAddress);
                    c1.TestMessage1(message1Value);
                    c1.Close();

                    var c2 = new TestServiceClient2(new RudyMqBinding(), secondQueueAddress);
                    c2.TestMessage2(message2Value);
                    c2.Close();

                    serviceHost.Close(TimeSpan.FromMilliseconds(500));
                }
            }

            Assert.AreEqual(TestResultBag.Message1Value, message1Value);
            Assert.AreEqual(TestResultBag.Message2Value, message2Value);
        }

        private static QueueHost GetQueueHost(string hostName, int port, string baseAddress)
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
                                 persistLocation,
                                 transitLocation,
                                 Config.Get("transitCleanupInterval", 600000),
                                 Config.Get("transitMaximumAge", 600000));
        }
    }

    public static class TestResultBag
    {
        public static string Message1Value { get; set; }
        public static string Message2Value { get; set; }
    }

    [ServiceContract]
    public interface ITestService1
    {
        [OperationContract(IsOneWay = true)]
        void TestMessage1(string value);
    }

    [ServiceContract]
    public interface ITestService2
    {
        [OperationContract(IsOneWay = true)]
        void TestMessage2(string value);
    }

    public class TestServiceClient1 : ClientBase<ITestService1>, ITestService1
    {
        public TestServiceClient1(string endpointConfigurationName) : base(endpointConfigurationName) {}
        public TestServiceClient1(Binding binding, string remoteAddress) : base(binding, new EndpointAddress(remoteAddress)) {}

        public void TestMessage1(string value)
        {
            this.Channel.TestMessage1(value);
        }
    }

    public class TestServiceClient2 : ClientBase<ITestService2>, ITestService2
    {
        public TestServiceClient2(string endpointConfigurationName) : base(endpointConfigurationName) {}
        public TestServiceClient2(Binding binding, string remoteAddress) : base(binding, new EndpointAddress(remoteAddress)) { }

        public void TestMessage2(string value)
        {
            this.Channel.TestMessage2(value);
        }
    }

    public class TestService : ITestService1, ITestService2
    {
        public void TestMessage1(string value)
        {
            TestResultBag.Message1Value = value;
        }

        public void TestMessage2(string value)
        {
            TestResultBag.Message2Value = value;
        }
    }
}