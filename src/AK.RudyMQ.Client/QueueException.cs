/*******************************************************************************************************************************
 * AK.RudyMQ.Client.QueueException
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
    /// Contains information about an error that occurred when working with a message queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class QueueException : Exception
    {
        /// <summary>
        /// Error details about what happened.
        /// </summary>
        public QueueErrorInfo ErrorInfo { get; private set; }

        /// <summary>
        /// Creates a new QueueException based on the given WCF fault exception with QueueErrorInfo.
        /// </summary>
        /// <param name="exception">Fault exception.</param>
        public QueueException(FaultException<QueueErrorInfo> exception) : base(exception.Detail.ErrorMessage, exception)
        {
            this.ErrorInfo = exception.Detail;
        }

        /// <summary>
        /// Creates a new generic QueueException based on the given inner exception.
        /// </summary>
        /// <param name="exception">Inner exception.</param>
        public QueueException(Exception exception) : base(QueueErrorInfo.FromErrorCode(QueueErrorCode.NotSpecified).ErrorMessage, exception)
        {
            this.ErrorInfo = QueueErrorInfo.FromErrorCode(QueueErrorCode.NotSpecified);
        }
    }
}