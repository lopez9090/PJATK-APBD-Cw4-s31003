namespace LegacyRenewalApp.Discounts
{
    public class LoyaltyDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (customer.YearsWithCompany >= 5) return (baseAmount * 0.07m, "long-term loyalty discount; ");
            if (customer.YearsWithCompany >= 2) return (baseAmount * 0.03m, "basic loyalty discount; ");
            
            return (0m, string.Empty);
        }
    }
}