/*******************************************************************************************************************************
 * AK.RudyMQ.Common.QueueErrorCode
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

using System.Runtime.Serialization;

namespace AK.RudyMQ.Common
{
    /// <summary>
    /// Queue operation error codes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [DataContract]
    public enum QueueErrorCode
    {
        [EnumMember] NotSpecified = 0,
        [EnumMember] ServerNotInitialized = 1,
        [EnumMember] ServerInitializationFailed = 2,
        [EnumMember] ServerHostInitializationFailed = 3,
        [EnumMember] CannotGetQueueInfo = 4,
        [EnumMember] CannotCreateQueue = 5,
        [EnumMember] QueueAlreadyExists = 6,
        [EnumMember] CannotRemoveQueue = 7,
        [EnumMember] QueueDoesNotExist = 8,
        [EnumMember] CannotPurgeQueue = 9,
        [EnumMember] CannotEnqueue = 10,
        [EnumMember] CannotDequeue = 11,
        [EnumMember] TransactionalQueueMustBePersisted = 12
    }
}