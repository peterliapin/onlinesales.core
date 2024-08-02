// <copyright file="Commentable.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Entities;

public interface ICommentable
{
    public static string GetCommentableType(Type t)
    {
        return (string)t.GetMethod("GetCommentableType")!.Invoke(null, null)!;
    }

    public static abstract string GetCommentableType();
}