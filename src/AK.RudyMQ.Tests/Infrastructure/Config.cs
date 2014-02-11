/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Infrastructure.Config
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
using System.Configuration;

#endregion

namespace AK.RudyMQ.Tests.Infrastructure
{
    /// <summary>
    /// Configuration reader helper.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class Config
    {
        public static T Get<T>(string key, T defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : (T) Convert.ChangeType(value, typeof (T));
        }
    }
}