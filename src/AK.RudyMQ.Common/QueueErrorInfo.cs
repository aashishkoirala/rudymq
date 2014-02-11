/*******************************************************************************************************************************
 * AK.RudyMQ.Common.QueueErrorInfo
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
    /// Contains details about queue errors.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [DataContract(Namespace = "http://rudymq.aashishkoirala.com/")]
    public class QueueErrorInfo
    {
        /// <summary>
        /// Error code.
        /// </summary>
        [DataMember]
        public QueueErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Queue message instance, if relevant.
        /// </summary>
        [DataMember]
        public QueueMessage QueueMessage { get; set; }

        /// <summary>
        /// Queue info instance, if relevant.
        /// </summary>
        [DataMember]
        public QueueInfo QueueInfo { get; set; }

        /// <summary>
        /// Creates a new QueueErrorInfo from the given error code.
        /// </summary>
        /// <param name="errorCode">QueueErrorCode value.</param>
        /// <returns>QueueErrorInfo object.</returns>
        public static QueueErrorInfo FromErrorCode(QueueErrorCode errorCode)
        {
            var message = "An unexpected error occurred.";

            switch(errorCode)
            {
                case QueueErrorCode.ServerNotInitialized:
                    message = "Server not initialized.";
                    break;
                case QueueErrorCode.ServerInitializationFailed:
                    message = "Server initialization failed.";
                    break;
                case QueueErrorCode.ServerHostInitializationFailed:
                    message = "Server host initialization failed.";
                    break;
                case QueueErrorCode.CannotGetQueueInfo:
                    message = "Cannot get queue info.";
                    break;
                case QueueErrorCode.CannotCreateQueue:
                    message = "Cannot create queue.";
                    break;
                case QueueErrorCode.QueueAlreadyExists:
                    message = "A queue with this name already exists.";
                    break;
                case QueueErrorCode.CannotRemoveQueue:
                    message = "Cannot remove queue.";
                    break;
                case QueueErrorCode.QueueDoesNotExist:
                    message = "A queue with this name does not exist.";
                    break;
                case QueueErrorCode.CannotPurgeQueue:
                    message = "Cannot purge queue.";
                    break;
                case QueueErrorCode.CannotEnqueue:
                    message = "Cannot put message on queue.";
                    break;
                case QueueErrorCode.CannotDequeue:
                    message = "Cannot take message from queue.";
                    break;
                case QueueErrorCode.TransactionalQueueMustBePersisted:
                    message = "To create a transactional queue, it must also be persisted.";
                    break;
            }

            return new QueueErrorInfo {ErrorCode = errorCode, ErrorMessage = message};
        }
    }
}