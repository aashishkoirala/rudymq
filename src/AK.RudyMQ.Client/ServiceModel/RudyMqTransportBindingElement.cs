/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.RudyMqTransportBindingElement
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
using System.ServiceModel.Channels;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// WCF transport binding element that uses RudyMQ for transport.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class RudyMqTransportBindingElement : TransportBindingElement
    {
        public override BindingElement Clone()
        {
            return new RudyMqTransportBindingElement();
        }

        public override string Scheme
        {
            get { return UriParser.Scheme; }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return (IChannelFactory<TChannel>) new OutputChannelFactory();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            return (IChannelListener<TChannel>) new InputChannelListener(
                new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress));
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof (TChannel) == typeof (IOutputChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof (TChannel) == typeof (IInputChannel);
        }
    }
}