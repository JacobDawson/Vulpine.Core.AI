/**
 *  This file is an integral part of the Vulpine Core Library
 *  Copyright (c) 2016-2018 Benjamin Jacob Dawson
 *
 *      http://www.jakesden.com/corelibrary.html
 *
 *  The Vulpine Core Library is free software; you can redistribute it 
 *  and/or modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  The Vulpine Core Library is distributed in the hope that it will 
 *  be useful, but WITHOUT ANY WARRANTY; without even the implied 
 *  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *  See the GNU Lesser General Public License for more details.
 *
 *      https://www.gnu.org/licenses/lgpl-2.1.html
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Nural
{
    public sealed class Axon : IComparable<Axon>
    {
        //refrences the input and output by index
        int source;
        int target;

        //determins the weight of the conneciton
        double weight;
        bool enabled;

        internal Axon(int source, int target, double weight)
        {
            this.source = source;
            this.target = target;
            this.weight = weight;
            this.enabled = true;
        }

        internal Axon(Axon other)
        {
            source = other.source;
            target = other.target;
            weight = other.weight;
            enabled = other.enabled;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int hash = GetHashCode();

            sb.AppendFormat("Axon-{0:X8} ", hash);
            sb.AppendFormat("{0:G6} ", weight);
            sb.Append(enabled ? "Enabled" : "Disabled");

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            //makes sure the object is another axon
            var other = obj as Axon;
            if (other == null) return false;

            //compares the source and the target
            if (source != other.source) return false;
            if (target != other.target) return false;

            return true;
        }

        public override int GetHashCode()
        {
            //generates a hash from the source and target
            return unchecked((source * 32707) ^ target);
        }

        public int CompareTo(Axon other)
        {
            //sorts the axons first by the target node
            int test = target.CompareTo(other.target);
            if (test != 0) return test;

            //then compares the axons by their source
            return source.CompareTo(other.source);
        }

        public int Index
        {
            get { return GetHashCode(); }
        }

        public int Source
        {
            get { return source; }
        }

        public int Target
        {
            get { return target; }
        }

        public double Weight
        {
            get 
            {
                //returns the weight, or zero if disabled
                return enabled ? weight : 0.0; 
            }
            set 
            { 
                //simply sets the wieght
                weight = value; 
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        ///// <summary>
        ///// Makes a genetic copy of another axon that shares the same ID.
        ///// </summary>
        ///// <param name="other">Axon to copy</param>
        //internal void CopyGenome(Axon other)
        //{
        //    if (index == other.index)
        //    {
        //        source = other.source;
        //        weight = other.weight;
        //        Enabled = other.enabled;
        //    }
        //}
        
    }
}
