/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.ServiceOperationMessage
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

using System;

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// Queue message that represents a WCF operation.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Serializable]
    public class ServiceOperationMessage
    {
        /// <summary>
        /// Serialized WCF message.
        /// </summary>
        public byte[] OperationData { get; set; }
    }
}