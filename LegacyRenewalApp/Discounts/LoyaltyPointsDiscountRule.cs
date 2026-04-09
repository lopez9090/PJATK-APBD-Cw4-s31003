namespace LegacyRenewalApp.Discounts
{
    public class LoyaltyPointsDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                return (pointsToUse, $"loyalty points used: {pointsToUse}; ");
            }
            
            return (0m, string.Empty);
        }
    }
}