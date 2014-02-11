/*******************************************************************************************************************************
 * AK.RudyMQ.Service.TransitCleanupOperation
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace AK.RudyMQ.Service
{
    /// <summary>
    /// Starts the transit cleanup operation and returns an object that can be used to stop it.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class TransitCleanupOperation : IDisposable
    {
        #region Fields

        private const int MinimumCleanupInterval = 1000;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task task;

        #endregion

        #region IDisposable

        ~TransitCleanupOperation()
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
            if (!disposing) return;

            this.cancellationTokenSource.Dispose();
        }

        #endregion

        #region Methods - Public Static

        /// <summary>
        /// Starts the transit cleanup operation.
        /// </summary>
        /// <param name="transitLocation">Folder where messages are temporarily kept  during dequeue until committed.</param>
        /// <param name="cleanupInterval">Period in milliseconds used to scan and  cleanup (i.e. re-queue) uncommitted messages.</param>
        /// <param name="maximumAge">
        /// For uncommitted messages, the maximum number of 
        /// milliseconds they stay that way before being cleaned up (i.e. re-queued).
        /// </param>
        /// <param name="cleanupAction">Action to perform with each uncommitted message to clean it up.</param>
        /// <returns>A TransitCleanupOperation object that can be used to stop the cleanup operation.</returns>
        public static TransitCleanupOperation Start(string transitLocation, int cleanupInterval, int maximumAge, Action<QueueMessage> cleanupAction)
        {
            var operation = new TransitCleanupOperation();
            var token = operation.cancellationTokenSource.Token;
            var formatter = new BinaryFormatter();
            var totalCleanupCount = cleanupInterval/MinimumCleanupInterval;
            totalCleanupCount = totalCleanupCount == 0 ? 1 : totalCleanupCount;

            operation.task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) return;

                    var now = DateTime.Now;
                    var files = Directory.GetFiles(transitLocation, "*.qm")
                        .Select(x => new FileInfo(x))
                        .Where(x => now.Subtract(x.CreationTime).TotalMilliseconds > maximumAge)
                        .Select(x => x.FullName);

                    foreach (var file in files)
                    {
                        QueueMessage message;
                        using (var fs = File.OpenRead(file))
                        {
                            message = (QueueMessage) formatter.Deserialize(fs);
                        }
                        File.Delete(file);
                        cleanupAction(message);
                    }

                    if (token.IsCancellationRequested) return;

                    var cleanupCount = 0;
                    while (cleanupCount < totalCleanupCount)
                    {
                        Thread.Sleep(MinimumCleanupInterval);
                        cleanupCount++;

                        if (token.IsCancellationRequested) return;
                    }
                }
            }, token);

            return operation;
        }

        #endregion

        #region Methods - Public

        /// <summary>
        /// Stops the cleanup operation.
        /// </summary>
        public void Stop()
        {
            this.cancellationTokenSource.Cancel();

            if (this.task == null || task.IsCompleted || task.IsCanceled || task.IsFaulted) return;

            this.task.Wait();
            this.task.Dispose();
            this.task = null;
        }

        #endregion
    }
}