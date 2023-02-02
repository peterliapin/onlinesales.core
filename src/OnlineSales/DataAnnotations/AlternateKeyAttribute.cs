// <copyright file="AlternateKeyAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DataAnnotations;

[AttributeUsage(AttributeTargets.Property)]
public class AlternateKeyAttribute : Attribute
{
    public AlternateKeyAttribute(string parentUniqueIndex, string sourceParentIdProperty)
    {
        ParentUniqueIndex = parentUniqueIndex;
        SourceParentIdProperty = sourceParentIdProperty;
    }

    public string ParentUniqueIndex { get; set; }

    public string SourceParentIdProperty { get; set; }
}
