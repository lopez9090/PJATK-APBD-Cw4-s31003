using System;

namespace LegacyRenewalApp.Calculators
{
    public class FeeCalculator : IFeeCalculator
    {
        public (decimal SupportFee, decimal PaymentFee, string Notes) CalculateFees(
            string planCode, string paymentMethod, bool includePremiumSupport, decimal subtotal)
        {
            decimal supportFee = 0m;
            string notes = string.Empty;

            if (includePremiumSupport)
            {
                if (planCode == "START") supportFee = 250m;
                else if (planCode == "PRO") supportFee = 400m;
                else if (planCode == "ENTERPRISE") supportFee = 700m;
                
                notes += "premium support included; ";
            }

            decimal paymentFee = 0m;
            if (paymentMethod == "CARD")
            {
                paymentFee = (subtotal + supportFee) * 0.02m;
                notes += "card payment fee; ";
            }
            else if (paymentMethod == "BANK_TRANSFER")
            {
                paymentFee = (subtotal + supportFee) * 0.01m;
                notes += "bank transfer fee; ";
            }
            else if (paymentMethod == "PAYPAL")
            {
                paymentFee = (subtotal + supportFee) * 0.035m;
                notes += "paypal fee; ";
            }
            else if (paymentMethod == "INVOICE")
            {
                paymentFee = 0m;
                notes += "invoice payment; ";
            }
            else
            {
                throw new ArgumentException("Unsupported payment method");
            }

            return (supportFee, paymentFee, notes);
        }
    }
}