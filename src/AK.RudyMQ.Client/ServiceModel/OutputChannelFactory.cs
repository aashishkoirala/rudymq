/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.OutputChannelFactory
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
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Implementation of IChannelFactory of IOutputChannel that creates IOutputChannel's
    /// that send messages to RudyMQ message queues.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class OutputChannelFactory : ChannelFactoryBase<IOutputChannel>
    {
        protected override void OnOpen(TimeSpan timeout) { }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return MootAsyncResultFactory.Create(callback, state, timeout);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.OnOpen(result.Timeout());
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            string hostName, baseAddress, queueName;
            int port;
            UriParser.Parse(via, out hostName, out port, out baseAddress, out queueName);

            var conn = MessageQueue.Connect(hostName, port, baseAddress);
            var queue = conn.Get(queueName);

            return new OutputChannel(this, queue, address, via);
        }
    }
}