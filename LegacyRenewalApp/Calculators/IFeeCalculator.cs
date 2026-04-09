namespace LegacyRenewalApp.Calculators
{
    public interface IFeeCalculator
    {
        (decimal SupportFee, decimal PaymentFee, string Notes) CalculateFees(
            string planCode, 
            string paymentMethod, 
            bool includePremiumSupport, 
            decimal subtotal);
    }
}