// <copyright file="RelaxedEnumEquivalencyStep.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;

namespace OnlineSales.Tests.Environment;

internal class RelaxedEnumEquivalencyStep : IEquivalencyStep
{
    public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context, IEquivalencyValidator nestedValidator)
    {
        if (comparands.Subject is string subject && comparands.Expectation?.GetType().IsEnum == true)
        {
            AssertionScope.Current
                .ForCondition(subject == comparands.Expectation.ToString())
                .FailWith(() =>
                {
                    var subjectsUnderlyingValue = ExtractDecimal(comparands.Subject);
                    var expectationsUnderlyingValue = ExtractDecimal(comparands.Expectation);

                    var subjectsName = GetDisplayNameForEnumComparison(comparands.Subject, subjectsUnderlyingValue);
                    var expectationName = GetDisplayNameForEnumComparison(comparands.Expectation, expectationsUnderlyingValue);
                    return new FailReason(
                            $"Expected {{context:string}} to be equivalent to {expectationName}{{reason}}, but found {subjectsName}.");
                });

            return EquivalencyResult.AssertionCompleted;
        }

        if (comparands.Subject?.GetType().IsEnum == true && comparands.Expectation is string expectation)
        {
            AssertionScope.Current
                .ForCondition(comparands.Subject.ToString() == expectation)
                .FailWith(() =>
                {
                    var subjectsUnderlyingValue = ExtractDecimal(comparands.Subject);
                    var expectationsUnderlyingValue = ExtractDecimal(comparands.Expectation);

                    var subjectsName = GetDisplayNameForEnumComparison(comparands.Subject, subjectsUnderlyingValue);
                    var expectationName = GetDisplayNameForEnumComparison(comparands.Expectation, expectationsUnderlyingValue);
                    return new FailReason(
                            $"Expected {{context:enum}} to be equivalent to {expectationName}{{reason}}, but found {subjectsName}.");
                });

            return EquivalencyResult.AssertionCompleted;
        }

        return EquivalencyResult.ContinueWithNext;
    }

    private static string GetDisplayNameForEnumComparison(object o, decimal? v)
    {
        if (o is null)
        {
            return "<null>";
        }

        if (v is null)
        {
            return '\"' + o.ToString() + '\"';
        }

        var typePart = o.GetType().Name;
        var namePart = o.ToString()!.Replace(", ", "|", StringComparison.Ordinal);
        var valuePart = v.Value.ToString(CultureInfo.InvariantCulture);
        return $"{typePart}.{namePart} {{{{value: {valuePart}}}}}";
    }

    private static decimal? ExtractDecimal(object o)
    {
        return o?.GetType().IsEnum == true ? Convert.ToDecimal(o, CultureInfo.InvariantCulture) : null;
    }
}

