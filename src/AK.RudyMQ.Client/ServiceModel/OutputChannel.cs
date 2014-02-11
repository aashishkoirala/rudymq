/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.OutputChannel
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
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Implementation of IOutputChannel that serializes and puts WCF messages on a RudyMQ message queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class OutputChannel : ChannelBase, IOutputChannel
    {
        #region The Meat

        private readonly IQueue queue;

        private Action onCloseAction;
        private Action onOpenAction;
        private Action sendAction;

        public OutputChannel(ChannelManagerBase channelManager, IQueue queue,
            EndpointAddress remoteAddress, Uri via) : base(channelManager)
        {
            this.queue = queue;
            this.RemoteAddress = remoteAddress;
            this.Via = via;
        }

        public EndpointAddress RemoteAddress { get; private set; }
        public Uri Via { get; private set; }

        public void Send(Message message)
        {
            var bufferedCopy = message.CreateBufferedCopy(int.MaxValue);
            var messageToStream = bufferedCopy.CreateMessage();

            var binaryMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
            var messageEncoderFactory = binaryMessageEncodingBindingElement.CreateMessageEncoderFactory();
            var messageEncoder = messageEncoderFactory.Encoder;

            byte[] messageData;
            using (var memoryStream = new MemoryStream())
            {
                messageEncoder.WriteMessage(messageToStream, memoryStream);
                messageData = memoryStream.ToArray();
            }

            this.queue.Send(new ServiceOperationMessage {OperationData = messageData});
        }

        #endregion

        #region IOutputChannel BoilerPlate

        protected override void OnAbort() {}

        protected override void OnClose(TimeSpan timeout) {}

        protected override void OnEndClose(IAsyncResult result)
        {
            this.onCloseAction.EndInvoke(result);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.onCloseAction = () => this.OnClose(timeout);
            return this.onCloseAction.BeginInvoke(callback, state);
        }

        protected override void OnOpen(TimeSpan timeout) {}

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.onOpenAction = () => this.OnOpen(timeout);
            return this.onOpenAction.BeginInvoke(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.onOpenAction.EndInvoke(result);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.Send(message);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            this.sendAction = () => this.Send(message);
            return this.sendAction.BeginInvoke(callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.sendAction = () => this.Send(message, timeout);
            return this.sendAction.BeginInvoke(callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.sendAction.EndInvoke(result);
        }

        #endregion
    }
}