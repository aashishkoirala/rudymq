/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceExecutor
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

using AK.RudyMQ.Common;
using System;
using System.ServiceModel;

#endregion

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Helper class that performs WCF service calls and handles some of the plumbing.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class ServiceExecutor
    {
        public static void Execute<TChannel>(string address, Action<TChannel> action)
        {
            var channelFactory = new ChannelFactory<TChannel>(new NetTcpBinding(), address);
            var channel = channelFactory.CreateChannel();
            try
            {
                action(channel);
            }
            catch (FaultException<QueueErrorInfo> faultException)
            {
                throw new QueueException(faultException);
            }
            catch (Exception exception)
            {
                throw new QueueException(exception);
            }
            finally
            {
                channelFactory.Close();
            }
        }

        public static TResult Execute<TChannel, TResult>(string address, Func<TChannel, TResult> action)
        {
            var channelFactory = new ChannelFactory<TChannel>(new NetTcpBinding(), address);
            var channel = channelFactory.CreateChannel();
            try
            {
                return action(channel);
            }
            catch (FaultException<QueueErrorInfo> faultException)
            {
                throw new QueueException(faultException);
            }
            catch (Exception exception)
            {
                throw new QueueException(exception);
            }
            finally
            {
                channelFactory.Close();
            }
        }
    }
}