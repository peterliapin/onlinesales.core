// <copyright file="TestPromotion.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestPromotion : PromotionCreateDto
{
    public TestPromotion(string uid)
    {
        Code = "PromotionCode_" + uid;
        Name = "Name_" + uid;
    }
}