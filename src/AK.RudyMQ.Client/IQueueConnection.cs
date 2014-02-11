/*******************************************************************************************************************************
 * AK.RudyMQ.Client.IQueueConnection
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
    /// Represents a connection to a message queue- lets you create or remove queues and also get reference
    /// to queues to operate on them.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IQueueConnection
    {
        /// <summary>
        /// Creates a new queue. Throws if one already exists by that name.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <param name="isPersisted">Whether messages to this queue are persisted to disk until they're received.</param>
        /// <param name="isTransactional">
        /// Whether messages to this queue are put back if the client operation that 
        /// handles the messages fails. The queue must also be persisted for it to be transactional.
        /// </param>
        void Create(string queueName, bool isPersisted, bool isTransactional);

        /// <summary>
        /// Removes the given queue. Throws if it does not exist.
        /// </summary>
        /// <param name="queueName">Name of queue to remove.</param>
        void Remove(string queueName);

        /// <summary>
        /// Gets a reference to the given queue. Throws if it does not exist.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <returns>An IQueue object that you can use to send and receive messages to and from the queue.</returns>
        IQueue Get(string queueName);
    }
}