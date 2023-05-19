// <copyright file="SurrogateForeignKeyAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SurrogateForeignKeyAttribute : Attribute
    {
        public SurrogateForeignKeyAttribute(Type relatedType, string relatedTypeUniqeIndex, string sourceForeignKey)
        {
            RelatedType = relatedType;
            RelatedTypeUniqeIndex = relatedTypeUniqeIndex;
            SourceForeignKey = sourceForeignKey;
        }

        public Type RelatedType { get; set; }

        public string RelatedTypeUniqeIndex { get; set; }

        public string SourceForeignKey { get; set; }
    }
}