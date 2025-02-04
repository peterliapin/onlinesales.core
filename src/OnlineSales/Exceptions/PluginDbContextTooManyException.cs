﻿// <copyright file="PluginDbContextTooManyException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class PluginDbContextTooManyException : PluginDbContextException
{
    private static readonly string HardcodedMessage = "Plugin database context is registered in service provider more than once";

    public PluginDbContextTooManyException()
        : base()
    {
    }

    public PluginDbContextTooManyException(Type? unregisteredDbContext)
        : base(HardcodedMessage, unregisteredDbContext)
    {
    }

    public PluginDbContextTooManyException(Type? unregisteredDbContext, Exception? innerException)
        : base(HardcodedMessage, unregisteredDbContext, innerException)
    {
    }
}