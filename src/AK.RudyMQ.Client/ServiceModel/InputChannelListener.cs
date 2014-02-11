/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.InputChannelListener
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
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Implementation of IChannelListener of IInputChannel that acts as a pump of IInputChannels that listen to
    /// RudyMQ queues for messages. See summary for MootAsyncResultFactory for information on the thing I'm doing 
    /// with the Begin/End pairs.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class InputChannelListener : ChannelListenerBase<IInputChannel>
    {
        #region The Meat

        private readonly Uri uri;
        private readonly BlockingCollection<IInputChannel> channelQueue = new BlockingCollection<IInputChannel>();

        public InputChannelListener(Uri uri)
        {
            this.uri = uri;
        }

        public override Uri Uri { get { return this.uri; } }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.ThrowIfDisposed();
            this.AddNewChannel();
        }

        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            if (this.IsDisposed || CommunicationState.Opened != this.State && CommunicationState.Opening != this.State)
                return null;

            this.ThrowIfDisposed();
            return this.channelQueue.Take();
        }

        private void AddNewChannel()
        {
            if (this.IsDisposed || CommunicationState.Opened != this.State && CommunicationState.Opening != this.State)
                return;

            this.ThrowIfDisposed();

            var inputChannel = new InputChannel(this);
            inputChannel.Closed += (sender, args) => this.AddNewChannel();
            this.channelQueue.Add(inputChannel);
        }

        #endregion

        #region IChannelListener Boilerplate

        protected override void OnAbort() {}

        protected override void OnClose(TimeSpan timeout)
        {
            this.channelQueue.Dispose();
        }

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

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            this.ThrowIfDisposed();
            return false;
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {            
            this.ThrowIfDisposed();
            return this.OnWaitForChannel(((IMootAsyncResult) result).Timeout);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.IsDisposed || CommunicationState.Opened != this.State && CommunicationState.Opening != this.State)
                return null;

            this.ThrowIfDisposed();
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            this.ThrowIfDisposed();
            return this.AcceptChannel(result.Timeout());
        }

        #endregion
    }
}