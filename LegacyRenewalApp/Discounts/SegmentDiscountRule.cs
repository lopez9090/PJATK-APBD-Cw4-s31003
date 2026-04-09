namespace LegacyRenewalApp.Discounts

{
    public class SegmentDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (customer.Segment == "Silver") return (baseAmount * 0.05m, "silver discount; ");
            if (customer.Segment == "Gold") return (baseAmount * 0.10m, "gold discount; ");
            if (customer.Segment == "Platinum") return (baseAmount * 0.15m, "platinum discount; ");
            if (customer.Segment == "Education" && plan.IsEducationEligible) return (baseAmount * 0.20m, "education discount; ");
            
            return (0m, string.Empty);
        }
    }
}
