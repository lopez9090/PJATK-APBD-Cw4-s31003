namespace LegacyRenewalApp.Discounts
{
    public interface IDiscountRule
    {
        (decimal Amount, string Note) Calculate(
            Customer customer, 
            SubscriptionPlan plan, 
            int seatCount, 
            decimal baseAmount, 
            bool useLoyaltyPoints);
    }
}