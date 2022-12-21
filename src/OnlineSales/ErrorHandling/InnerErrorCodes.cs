// <copyright file="InnerErrorCodes.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;

namespace OnlineSales.ErrorHandling
{
    public static class InnerErrorCodes
    {
        public static readonly string UnspecifiedError = "The error is not specified";

        public class Status400
        {
            public static readonly (string, string) ValidationErrors = ("ValidationErrors", "One or more validation errors occured");

            public static readonly (string, string) InvalidScope = ("InvalidScope", "Scope is invalid");

            public static readonly (string, string) InvalidFileName = ("File Name is invalid", "Scope is invalid");
        }

        public class Status404
        {
            public static readonly (string, string) IdNotFound = ("IdNotFound", "{0} with Id = {1} is not found");

            public static readonly (string, string) FileNotFound = ("FileNotFound", "File with filename {0} is not found");
        }

        public class Status422
        {
            public static readonly (string, string) FKIdNotFound = ("FKIdNotFound", "Operation could not be performed because {0} with Id = {1} is not found");

            public static readonly (string, string) MIMINotIdentified = ("MIMINotIdentified", "MIME of the file {0} not identified.");            
        }
    }
}