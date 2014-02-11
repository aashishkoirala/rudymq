/*******************************************************************************************************************************
 * AK.RudyMQ.Client.IQueue
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

using System;

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Methods to perform operations against a message queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IQueue
    {
        /// <summary>
        /// Sends the given message to the queue. Message should be a simple serializable data structure.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="message">Message object.</param>
        void Send<T>(T message);

        /// <summary>
        /// Purges all messages of the given type from the queue.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        void Purge<T>();

        /// <summary>
        /// Purges all messages from the queue.
        /// </summary>
        void PurgeAll();

        /// <summary>
        /// Starts a thread that polls and receives messages from the queue.
        /// </summary>
        /// <typeparam name="T">Type of message to receive.</typeparam>
        /// <param name="pollInterval">Polling interval in milliseconds.</param>
        /// <param name="messageHandler">Callback that handles a message when received.</param>
        /// <param name="errorHandler">Callback that handles an error when one is thrown.</param>
        /// <returns>A IReceiveOperation object that lets you stop the recieving operation and inspect for errors.</returns>
        IReceiveOperation StartReceiving<T>(int pollInterval, 
            Action<T> messageHandler, Action<QueueException> errorHandler = null);
    }
}