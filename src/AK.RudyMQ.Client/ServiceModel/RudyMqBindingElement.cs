/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.RudyMqBindingElement
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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

#endregion

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// WCF configuration binding element for RudyMqBinding.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class RudyMqBindingElement : StandardBindingElement
    {
        protected override void OnApplyConfiguration(Binding binding) {}

        protected override Type BindingElementType
        {
            get { return typeof (RudyMqTransportBindingElement); }
        }
    }
}