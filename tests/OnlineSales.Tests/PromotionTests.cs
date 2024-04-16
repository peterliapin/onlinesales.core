// <copyright file="PromotionTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;

public class PromotionTests : SimpleTableTests<Promotion, TestPromotion, PromotionUpdateDto, IEntityService<Promotion>>
{
    public PromotionTests()
        : base("/api/promotion")
    {
    }

    protected override PromotionUpdateDto UpdateItem(TestPromotion pcd)
    {
        var from = new PromotionUpdateDto();
        pcd.Name = from.Name = pcd.Name + "Updated";
        return from;
    }
}