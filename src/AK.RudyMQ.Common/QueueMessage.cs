/*******************************************************************************************************************************
 * AK.RudyMQ.Common.QueueMessage
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
    /// Queue message.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Serializable]
    [DataContract(Namespace = "http://rudymq.aashishkoirala.com/")]
    public class QueueMessage
    {
        /// <summary>
        /// Unique identifier; assigned by server on first enqueue.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Name of queue to which this message belongs or is headed to.
        /// </summary>
        [DataMember]
        public string QueueName { get; set; }

        /// <summary>
        /// Type of message, is the full-type name of the serialized data
        /// in Body.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Serialized data that is the body of the message.
        /// </summary>
        [DataMember]
        public byte[] Body { get; set; }
    }
}