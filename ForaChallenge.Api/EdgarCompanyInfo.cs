using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ForaChallenge.Api
{
    public partial class EdgarCompanyInfo
    {
        // Fields
        private readonly Lazy<List<InfoFactUsGaapIncomeLossUnitsUsd>> _relevantIncome;
        private readonly Lazy<IDictionary<string, decimal>> _yearlyIncome;

        // Constructor
        public EdgarCompanyInfo()
        {
            _yearlyIncome = new Lazy<IDictionary<string, decimal>>(CalculateYearlyIncome);
            _relevantIncome = new Lazy<List<InfoFactUsGaapIncomeLossUnitsUsd>>(CalculateRelevantIncome);
        }

        // Properties
        public int Cik { get; set; }
        public string EntityName { get; set; }
        public InfoFact Facts { get; set; }

        // Public Methods
        public decimal GetStandardFundableAmount()
        {
            var incomeData = _relevantIncome.Value;

            if (!incomeData.Any() || !HasIncomeForAllYears() ||
                !HasPositiveIncomeForExpectedYears(Constants.PositiveIncomeYears)) return 0;

            var highestIncome = _yearlyIncome.Value
                .Where(p => Constants.PositiveIncomeYears.Contains(p.Key))
                .MaxBy(p => p.Value);

            return CalculateFundableAmount(highestIncome.Value);
        }

        public decimal GetSpecialFundableAmount()
        {
            var standardFundableAmount = GetStandardFundableAmount();
            if (standardFundableAmount == 0) return 0;

            var specialFundableAmount = standardFundableAmount;

            if (Constants.Vowels.Contains(EntityName[0], StringComparison.CurrentCultureIgnoreCase))
                specialFundableAmount += standardFundableAmount * Constants.VowelModifier;

            var yearlyIncome = _yearlyIncome.Value;
            if (yearlyIncome[Constants.Year2022] < yearlyIncome[Constants.Year2021])
                specialFundableAmount -= standardFundableAmount * Constants.YearComparisonModifier;

            return specialFundableAmount;
        }

        // Private Methods
        private IDictionary<string, decimal> CalculateYearlyIncome()
        {
            return _relevantIncome.Value.ToDictionary(p => p.FrameYear, p => p.Val);
        }

        private List<InfoFactUsGaapIncomeLossUnitsUsd> CalculateRelevantIncome()
        {
            return Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd
                .Where(IsRelevantIncome)
                .ToList() ?? new List<InfoFactUsGaapIncomeLossUnitsUsd>();
        }

        private static bool IsRelevantIncome(InfoFactUsGaapIncomeLossUnitsUsd income)
        {
            return string.Equals(income.Form, Constants.AcceptableForm, StringComparison.CurrentCultureIgnoreCase) &&
                   !string.IsNullOrEmpty(income.Frame) &&
                   MyRegex().IsMatch(income.Frame);
        }

        private bool HasIncomeForAllYears()
        {
            return Constants.AcceptableIncomeYears.Length == _yearlyIncome.Value
                .Select(p => p.Key)
                .Intersect(Constants.AcceptableIncomeYears)
                .Count();
        }

        private bool HasPositiveIncomeForExpectedYears(string[] expectedYears)
        {
            return _yearlyIncome.Value
                .Where(p => p.Value > 0)
                .Select(p => p.Key)
                .Intersect(expectedYears)
                .Count() == expectedYears.Length;
        }

        private static decimal CalculateFundableAmount(decimal income)
        {
            return income >= Constants.IncomeThreshold
                ? income * Constants.OverThresholdModifier
                : income * Constants.UnderThresholdModifier;
        }

        // Nested Types
        public class InfoFact
        {
            [JsonPropertyName("us-gaap")] 
            public InfoFactUsGaap UsGaap { get; set; }
        }

        public class InfoFactUsGaap
        {
            public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; }
        }

        public class InfoFactUsGaapNetIncomeLoss
        {
            public InfoFactUsGaapIncomeLossUnits Units { get; set; }
        }

        public class InfoFactUsGaapIncomeLossUnits
        {
            public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; }
        }

        public class InfoFactUsGaapIncomeLossUnitsUsd
        {
            public string Form { get; set; }
            public string Frame { get; set; }
            public decimal Val { get; set; }

            public string FrameYear => Frame.Length == 6 ? Frame[2..] : string.Empty;
        }

        [GeneratedRegex(Constants.AcceptableFrameFormat, RegexOptions.Multiline)]
        private static partial Regex MyRegex();
    }
}