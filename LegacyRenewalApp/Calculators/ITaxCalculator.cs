namespace LegacyRenewalApp.Calculators
{
    public interface ITaxCalculator
    {
        decimal CalculateTax(string country, decimal taxBase);
    }
}