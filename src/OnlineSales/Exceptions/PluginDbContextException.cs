// <copyright file="PluginDbContextException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class PluginDbContextException : Exception
{
    public PluginDbContextException()
        : base()
    {
    }

    public PluginDbContextException(string? message, Type? unregisteredDbContext)
        : base(message)
    {
        UnregisteredDbContext = unregisteredDbContext;
    }

    public PluginDbContextException(string? message, Type? unregisteredDbContext, Exception? innerException)
        : base(message, innerException)
    {
        UnregisteredDbContext = unregisteredDbContext;
    }

    public Type? UnregisteredDbContext { get; private set; }
}