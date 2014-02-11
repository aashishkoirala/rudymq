/*******************************************************************************************************************************
 * AK.RudyMQ.Client.ServiceModel.RudyMqBindingSection
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

using System.ServiceModel.Configuration;

namespace AK.RudyMQ.Client.ServiceModel
{
    /// <summary>
    /// WCF binding configuration section for RudyMqBinding.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class RudyMqBindingSection : StandardBindingCollectionElement<RudyMqBinding, RudyMqBindingElement> {}
}