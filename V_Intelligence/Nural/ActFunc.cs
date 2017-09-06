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
    public enum ActFunc
    {
        Identity,
        Sine,
        Cosine,
        Gaussian,
        Sigmoid,

        SoftPlus, //rectifier
        Sinc,


        //Non-Continious


        Heavyside,
        Relu, //rectified linear unit
        Abs,


        //multifold

        Max,
        Min,
        Mean,




    }
}
