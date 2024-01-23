// <copyright file="SmscException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class SmscException : Exception
    {
        public SmscException()
        {
        }

        public SmscException(string? message)
            : base(message)
        {
        }

        public SmscException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}