/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.UriParser
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
    /// Parses different elements of an endpoint address that uses RudyMQ binding.
    /// Expected format is: net.rudymq://{hostname}:{port}/{baseAddress}/{queueName}
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class UriParser
    {
        public const string Scheme = "net.rudymq";

        public static void Parse(Uri uri, out string hostName, out int port, 
            out string baseAddress, out string queueName)
        {
            if (uri.Scheme != Scheme)
                throw new Exception(string.Format("Invalid URI scheme; must be \"{0}\".", Scheme));

            hostName = uri.Host;
            port = uri.Port;

            var localPath = uri.LocalPath.Trim('/');
            if (string.IsNullOrWhiteSpace(localPath))
                throw new Exception("Queue name must be specified.");

            var lastSlashIndex = localPath.LastIndexOf('/');
            if (lastSlashIndex < 0)
            {
                queueName = localPath;
                baseAddress = null;
            }
            else
            {
                queueName = localPath.Substring(lastSlashIndex + 1);
                baseAddress = localPath.Substring(0, lastSlashIndex);
            }

            if (string.IsNullOrWhiteSpace(baseAddress)) baseAddress = null;

            if (string.IsNullOrWhiteSpace(queueName))
                throw new Exception("Queue name must be specified.");
        }
    }
}