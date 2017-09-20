/**
 *  This file is an integral part of the Vulpine Core Library: 
 *  Copyright (c) 2016-2017 Benjamin Jacob Dawson. 
 *
 *      http://www.jakesden.com/corelibrary.html
 *
 *  This file is licensed under the Apache License, Version 2.0 (the "License"); 
 *  you may not use this file except in compliance with the License. You may 
 *  obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.    
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Nural
{
    public sealed class Axon : IComparable<Axon>
    {
        ////stores the index of the axon
        //int index;

        //refrences the input and output by index
        int source;
        int target;

        //determins the weight of the conneciton
        double weight;
        bool enabled;   

        internal Axon(int index, int source, int target, double weight)
        {
            //this.index = index;
            this.source = source;
            this.target = target;
            this.weight = weight;
            this.enabled = true;
        }

        internal Axon(Axon other)
        {
            //index = other.index;
            source = other.source;
            target = other.target;
            weight = other.weight;
            enabled = other.enabled;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Axon-{0:X8} ", source);
            sb.AppendFormat("{0:G6} ", weight);
            sb.Append(enabled ? "Enabled" : "Disabled");

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            //makes sure the object is another axon
            var other = obj as Axon;
            if (other == null) return false;

            //compares the source an the target
            if (source != other.source) return false;
            if (target != other.target) return false;

            return true;
        }

        public override int GetHashCode()
        {
            //generates a hash from the source and target
            return unchecked((source * 907) ^ target);
        }

        public int CompareTo(Axon other)
        {
            //sorts the axons first by the target node
            int test = target.CompareTo(other.target);
            if (test != 0) return test;

            //then compares the axons by their source
            return source.CompareTo(other.source);
        }

        //public int Index
        //{
        //    get { return index; }
        //}

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
            get { return weight; }
            set { weight = value; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }



        
    }
}
