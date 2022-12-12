// <copyright file="PostsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class PostsTests : SimpleTableTests<Post, TestPostCreateDto, TestPostUpdateDto, TestPostConverter>
{
    public PostsTests()
        : base("/api/posts")
    {
    }    
}

