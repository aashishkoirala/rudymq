/*******************************************************************************************************************************
 * AK.RudyMQ.Common.AddressBuilder
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

namespace AK.RudyMQ.Common
{
    /// <summary>
    /// Generates WCF addresses for the queue.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class AddressBuilder
    {
        /// <summary>
        /// Queue host name for TCP.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// TCP port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Base address, if any.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Generates a WCF TCP endpoint address for communication with the queue.
        /// </summary>
        /// <returns>WCF TCP endpoint address.</returns>
        public string BuildAddress()
        {
            var port = this.Port == 0 ? 8777 : this.Port;
            var baseAddress = string.IsNullOrWhiteSpace(this.BaseAddress)
                                  ? "RudyMQ/MessageQueueService"
                                  : this.BaseAddress;
            var hostName = string.IsNullOrWhiteSpace(this.HostName) ? "localhost" : this.HostName;

            return string.Format("net.tcp://{0}:{1}/{2}/", hostName, port, baseAddress);
        }
    }
}