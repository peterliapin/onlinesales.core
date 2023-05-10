// <copyright file="SwaggerExampleAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DataAnnotations;

public class SwaggerExampleAttribute<T> : Attribute
{
    public SwaggerExampleAttribute(T value)
    {
        Value = value;
    }

    public T Value { get; }
}