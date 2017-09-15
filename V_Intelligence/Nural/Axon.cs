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
    public sealed class Axon
    {
        //stores the index of the axon
        int index;

        //stores the index of the input nuron
        int input;

        //determins the weight of the conneciton
        double weight;
        bool enabled;   

        internal Axon(int index, int input, double weight)
        {
            this.index = index;
            this.input = input;
            this.weight = weight;
            this.enabled = true;
        }

        internal Axon(Axon other)
        {
            index = other.index;
            input = other.input;
            weight = other.weight;
            enabled = other.enabled;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Axon-{0:X32} ", index);
            sb.AppendFormat("{0:G6} ", weight);
            sb.Append(enabled ? "Enabled" : "Disabled");

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            //makes sure the object is a keyed item
            var other = obj as Axon;
            if (other == null) return false;

            //compares the inovation numbers
            return (other.index == index);
        }

        public override int GetHashCode()
        {
            //uses the inovation number as a hashcode
            return index;
        }

        public int Index
        {
            get { return index; }
        }

        public int Input
        {
            get { return input; }
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
