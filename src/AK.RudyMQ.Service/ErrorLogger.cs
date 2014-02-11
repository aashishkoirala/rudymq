/*******************************************************************************************************************************
 * AK.RudyMQ.Service.ErrorLogger
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
using System.Diagnostics;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// Logs server errors to the event log.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class ErrorLogger
    {
        private const string SourceName = "RudyMQ";

        /// <summary>
        /// Log the given exception.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        public static void Log(Exception exception)
        {
            if (!EventLog.SourceExists(SourceName))
                EventLog.CreateEventSource(SourceName, "Application");

            EventLog.WriteEntry(SourceName, exception.ToString(), EventLogEntryType.Error);
        }
    }
}