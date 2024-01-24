// <copyright file="TestDbContextException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.TestPlugin.Exceptions;
public class TestDbContextException : Exception
{
    public TestDbContextException(string message)
        : base(message)
    {
    }
}