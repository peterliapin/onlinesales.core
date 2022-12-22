// <copyright file="InnerErrorCodes.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;

namespace OnlineSales.ErrorHandling
{
    public static class InnerErrorCodes
    {
        public static readonly ErrorDescription UnspecifiedError = new ErrorDescription("UnspecifiedError", "The error cannot be specified");

        public class Status400
        {
            public static readonly ErrorDescription ValidationErrors = new ErrorDescription("ValidationErrors", "One or more validation errors occured");

            public static readonly ErrorDescription InvalidScope = new ErrorDescription("InvalidScope", "Scope is invalid");

            public static readonly ErrorDescription InvalidFileName = new ErrorDescription("InvalidFilename", "File Name is invalid");
        }

        public class Status404
        {
            public static readonly ErrorDescription IdNotFound = new ErrorDescription("IdNotFound", "Object is not found", "{0} with Id = {1} is not found");

            public static readonly ErrorDescription FileNotFound = new ErrorDescription("FileNotFound", "File cannot be found", "File with filename {0} is not found");
        }

        public class Status422
        {
            public static readonly ErrorDescription FKIdNotFound = new ErrorDescription("FKIdNotFound", "A related object is not found", "Operation could not be performed because {0} with Id = {1} is not found");

            public static readonly ErrorDescription MIMINotIdentified = new ErrorDescription("MIMINotIdentified", "File MIME is not identified", "MIME of the file {0} not identified.");
        }

        public class Status500
        {
            public static readonly ErrorDescription ExceptionCaught = new ErrorDescription("ExceptionCaught", "An exception was thrown", "An exception of type: {0} was thrown.");
        }
    }
}