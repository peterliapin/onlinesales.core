// <copyright file="SurrogateIdentityAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DataAnnotations;

[AttributeUsage(AttributeTargets.Class)]
public class SurrogateIdentityAttribute : Attribute
{
    public SurrogateIdentityAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }

    public string PropertyName { get; set; }
}

