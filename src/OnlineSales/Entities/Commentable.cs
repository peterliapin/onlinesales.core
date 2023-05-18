// <copyright file="Commentable.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Entities;
public enum CommentableType
{
    Content = 0,
    Account = 1,
    Order = 2,
    Contact = 3,
}

public interface ICommentable 
{
    public static CommentableType GetCommentableType(Type t)
    {
        return (CommentableType)t.GetMethod("GetCommentableType")!.Invoke(null, null)!;
    }

    public static abstract CommentableType GetCommentableType();
}