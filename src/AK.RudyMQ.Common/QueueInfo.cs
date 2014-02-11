/*******************************************************************************************************************************
 * AK.RudyMQ.Common.QueueInfo
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
using System.Runtime.Serialization;

#endregion

namespace AK.RudyMQ.Common
{
    /// <summary>
    /// Information/metadata about a queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Serializable]
    [DataContract(Namespace = "http://rudymq.aashishkoirala.com/")]
    public class QueueInfo
    {
        /// <summary>
        /// Name of the queue.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Whether messages to this queue are persisted to disk until they're received.
        /// </summary>
        [DataMember]
        public bool IsPersisted { get; set; }

        /// <summary>
        /// Whether messages to this queue are put back if the client operation that handles the messages fails.
        /// The queue must also be persisted for it to be transactional.
        /// </summary>
        [DataMember]
        public bool IsTransactional { get; set; }
    }
}