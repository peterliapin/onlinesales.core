// <copyright file="PluginDbContextNotFoundException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class PluginDbContextNotFoundException : PluginDbContextException
{
    private static readonly string HardcodedMessage = "Plugin database context is not registered in service provider";

    public PluginDbContextNotFoundException()
        : base()
    {
    }

    public PluginDbContextNotFoundException(Type? unregisteredDbContext)
        : base(HardcodedMessage, unregisteredDbContext)
    {
    }

    public PluginDbContextNotFoundException(Type? unregisteredDbContext, Exception? innerException)
        : base(HardcodedMessage, unregisteredDbContext, innerException)
    {
    }
}