/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.MootAsyncResultFactory
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
using System.Threading;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Creates "moot" IAsyncResult instances. For some reason, the BeginInvoke/EndInvoke type operations in the
    /// WCF channel and channel listeners are not working like I would expect them to. Therefore, what I'm doing in these
    /// situations is- returning a "moot" IAsyncResult from the Begin method and then just calling the synchronous
    /// version of the operation from the End method. I tried a number of different approaches before giving up and
    /// resorting to this. The one extra thing that this allows me to do is pass in a timeout value from the Begin method
    /// to be used by the End method to pass in to the synchronous version of the method.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class MootAsyncResultFactory
    {
        /// <summary>
        /// Creates a new moot IAsyncResult as mentioned in the summary for MootAsyncResultFactory.
        /// </summary>
        /// <param name="callback">Callback from the Begin method.</param>
        /// <param name="state">State from the Begin method.</param>
        /// <param name="timeout">Timeout from the Begin method.</param>
        /// <returns>IAsyncResult.</returns>
        public static IAsyncResult Create(AsyncCallback callback, object state, TimeSpan? timeout = null)
        {
            Action action = () => { };
            var asyncResult = action.BeginInvoke(callback, state);

            return timeout.HasValue ? new MootAsyncResult(asyncResult, timeout.Value) : new MootAsyncResult(asyncResult);
        }

        /// <summary>
        /// Extracts the timeout from the given IAsyncResult. If it is a IMootAsyncResult, the stored
        /// value will be extracted. Otherwise, a default value of TimeSpan.MaxValue is used.
        /// </summary>
        /// <param name="result">IAsyncResult.</param>
        /// <returns>TimeSpan.</returns>
        public static TimeSpan Timeout(this IAsyncResult result)
        {
            var asyncResult = result as IMootAsyncResult;
            return asyncResult != null ? asyncResult.Timeout : TimeSpan.MaxValue;
        }

        /// <summary>
        /// Implementation of IMootAsyncResult that wraps an existing IAsyncResult,
        /// as explained in the summary for MootAsyncResultFactory.
        /// </summary>
        /// <author>Aashish Koirala</author>
        private class MootAsyncResult : IMootAsyncResult
        {
            private readonly IAsyncResult innerAsyncResult;

            public MootAsyncResult(IAsyncResult innerAsyncResult) : this(innerAsyncResult, TimeSpan.MaxValue) {}

            public MootAsyncResult(IAsyncResult innerAsyncResult, TimeSpan timeout)
            {
                this.innerAsyncResult = innerAsyncResult;
                this.Timeout = timeout;
            }

            public bool IsCompleted { get { return this.innerAsyncResult.IsCompleted; } }
            public WaitHandle AsyncWaitHandle { get { return this.innerAsyncResult.AsyncWaitHandle; } }
            public object AsyncState { get { return this.innerAsyncResult.AsyncState; } }
            public bool CompletedSynchronously { get { return this.innerAsyncResult.CompletedSynchronously; } }

            // ReSharper disable MemberHidesStaticFromOuterClass
            public TimeSpan Timeout { get; private set; }
            // ReSharper restore MemberHidesStaticFromOuterClass
        }
    }

    /// <summary>
    /// Extends IAsyncResult with a Timeout property, as explained in the summary for MootAsyncResultFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal interface IMootAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Timeout as a TimeSpan.
        /// </summary>
        TimeSpan Timeout { get; }
    }
}