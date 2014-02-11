/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.InputChannel
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

using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Implementation of IInputChannel that listens to a RudyMQ queue for messages.
    /// See summary for MootAsyncResultFactory for information on the thing I'm doing with the Begin/End pairs.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class InputChannel : ChannelBase, IInputChannel
    {
        #region The Meat

        private IQueue queue;

        public InputChannel(ChannelManagerBase channelManager) : base(channelManager)
        {
            var listener = channelManager as InputChannelListener;
            if (listener == null) throw new ArgumentException("Invalid channel manager.", "channelManager");

            this.LocalAddress = new EndpointAddress(listener.Uri);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.ThrowIfDisposed();

            string hostName, baseAddress, queueName;
            int port;
            UriParser.Parse(this.LocalAddress.Uri, out hostName, out port, out baseAddress, out queueName);

            var conn = MessageQueue.Connect(hostName, port, baseAddress);
            this.queue = conn.Get(queueName);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            this.ThrowIfDisposed();

            message = null;
            if (this.State == CommunicationState.Closed || this.State == CommunicationState.Closing)
                return true;

            var serviceOperationMessage = ((Queue)this.queue).Receive<ServiceOperationMessage>();
            if (serviceOperationMessage == null) return false;

            var binaryMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
            var messageEncoderFactory = binaryMessageEncodingBindingElement.CreateMessageEncoderFactory();
            var messageEncoder = messageEncoderFactory.Encoder;

            using (var memoryStream = new MemoryStream(serviceOperationMessage.OperationData))
            {
                var streamedMessage = messageEncoder.ReadMessage(memoryStream, int.MaxValue);
                var bufferedCopy = streamedMessage.CreateBufferedCopy(int.MaxValue);
                message = bufferedCopy.CreateMessage();
            }

            if (!message.Headers.Any(x => x.Name == "To" && x.Namespace == "http://www.w3.org/2005/08/addressing"))
                message.Headers.Add(MessageHeader.CreateHeader("To", "http://www.w3.org/2005/08/addressing",
                                                               this.LocalAddress.Uri));

            return true;
        }

        #endregion

        #region IInputChannel Boilerplate

        public EndpointAddress LocalAddress { get; private set; }

        protected override void OnAbort() {}

        protected override void OnClose(TimeSpan timeout) {}

        protected override void OnEndClose(IAsyncResult result)
        {
            this.OnClose(result.Timeout());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.ThrowIfDisposed();
            this.OnOpen(result.Timeout());
        }

        public Message Receive()
        {
            this.ThrowIfDisposed();
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            this.ThrowIfDisposed();

            var result = false;
            Message message = null;

            while (!result) result = this.TryReceive(timeout, out message);

            return message;
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        public Message EndReceive(IAsyncResult result)
        {
            this.ThrowIfDisposed();
            return this.Receive(result.Timeout());
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();

            if (this.State == CommunicationState.Closed || this.State == CommunicationState.Closing)
                return null;

            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            this.ThrowIfDisposed();
            return this.TryReceive(result.Timeout(), out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            this.ThrowIfDisposed();
            return true;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            this.ThrowIfDisposed();
            return this.WaitForMessage(((IMootAsyncResult) result).Timeout);
        }

        #endregion
    }
}