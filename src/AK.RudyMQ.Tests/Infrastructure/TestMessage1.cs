/*******************************************************************************************************************************
 * AK.RudyMQ.Tests.Infrastructure.TestMessage1
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

namespace AK.RudyMQ.Tests.Infrastructure
{
    /// <summary>
    /// Test message for integration tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Serializable]
    public class TestMessage1
    {
        public Guid Id { get; set; }
        public string Field1 { get; set; }
        public int Field2 { get; set; }
        public DateTime Field3 { get; set; }

        public override string ToString()
        {
            return string.Format("Field1: {0}, Field2: {1}, Field3: {2}", this.Field1, this.Field2, this.Field3);
        }

        public override bool Equals(object obj)
        {
            var msg = obj as TestMessage1;
            if (msg == null) return false;

            return this.Id == msg.Id &&
                   this.Field1 == msg.Field1 &&
                   this.Field2 == msg.Field2 &&
                   this.Field3 == msg.Field3;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
