/*******************************************************************************************************************************
 * AK.RudyMQ.Client.MessageQueue
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

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Lets you connect to a given queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class MessageQueue
    {
        /// <summary>
        /// Connects to the given queue.
        /// </summary>
        /// <param name="hostName">Name/address of machine where queue is listening.</param>
        /// <param name="port">The TCP port that the queue is listening on.</param>
        /// <param name="baseAddress">Base address, if any, that the queue is using.</param>
        /// <returns>A IQueueConnection object that you can use to work with the queue.</returns>
        public static IQueueConnection Connect(string hostName, int port, string baseAddress = null)
        {
            return new QueueConnection(hostName, port, baseAddress);
        }
    }
}