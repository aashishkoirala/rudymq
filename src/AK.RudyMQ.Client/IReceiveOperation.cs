/*******************************************************************************************************************************
 * AK.RudyMQ.Client.IReceiveOperation
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
using System.Collections.Generic;

#endregion

namespace AK.RudyMQ.Client
{
    /// <summary>
    /// Lets you stop a queue-receive asynchronous operation and inspect errors if any.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IReceiveOperation : IDisposable
    {
        /// <summary>
        /// Stops this queue-receive asynchronous operation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the list of errors that were captured during the asynchronous operation.
        /// The operation must be stopped first before accessing this.
        /// </summary>
        IEnumerable<QueueException> Exceptions { get; }
    }
}