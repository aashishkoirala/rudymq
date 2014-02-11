/*******************************************************************************************************************************
 * AK.RudyMQ.Service.Locked
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

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// Provides thread-protected access to any object.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class Locked<T> : IDisposable
    {
        private readonly T value;
        private readonly ReaderWriterLockSlim slim = new ReaderWriterLockSlim();

        /// <summary>
        /// Wraps the given value in a thread-protected container.
        /// </summary>
        /// <param name="value">Internal value to protect.</param>
        public Locked(T value)
        {
            this.value = value;
        }

        #region IDisposable

        ~Locked()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) this.slim.Dispose();
        }

        #endregion

        /// <summary>
        /// Executes the given action on the protected value within a read-lock.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        public void ExecuteWithinReadLock(Action<T> action)
        {
            this.slim.EnterReadLock();
            try
            {
                action(this.value);
            }
            finally
            {
                this.slim.ExitReadLock();
            }
        }

        /// <summary>
        /// Executes the given action on the protected value within a write-lock.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        public void ExecuteWithinWriteLock(Action<T> action)
        {
            this.slim.EnterWriteLock();
            try
            {
                action(this.value);
            }
            finally
            {
                this.slim.ExitWriteLock();
            }
        }
    }
}