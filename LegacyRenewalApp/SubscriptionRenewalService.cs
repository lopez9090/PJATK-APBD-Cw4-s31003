using System;
using System.Collections.Generic;
using LegacyRenewalApp.Discounts;
using LegacyRenewalApp.Calculators;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {

        private readonly IBillingGateway _billingGateway;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IEnumerable<IDiscountRule> _discountRules;
        private readonly IFeeCalculator _feeCalculator;
        private readonly ITaxCalculator _taxCalculator;

        public SubscriptionRenewalService() : this(
            new BillingGatewayAdapter(),
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new List<IDiscountRule>
            {
                new SegmentDiscountRule(),
                new LoyaltyDiscountRule(),
                new SeatCountDiscountRule(),
                new LoyaltyPointsDiscountRule()
            },
            new FeeCalculator(),
            new TaxCalculator())
        {
        }

        public SubscriptionRenewalService(
            IBillingGateway billingGateway,
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IEnumerable<IDiscountRule> discountRules,
            IFeeCalculator feeCalculator,
            ITaxCalculator taxCalculator)
        {
            _billingGateway = billingGateway ?? throw new ArgumentNullException(nameof(billingGateway));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _discountRules = discountRules ?? throw new ArgumentNullException(nameof(discountRules));
            _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
            _taxCalculator = taxCalculator ?? throw new ArgumentNullException(nameof(taxCalculator));
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            ValidateInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive) throw new InvalidOperationException("Inactive customers cannot renew subscriptions");

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            foreach (var rule in _discountRules)
            {
                var result = rule.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
                discountAmount += result.Amount;
                notes += result.Note;
            }

            decimal subtotalAfterDiscount = Math.Max(baseAmount - discountAmount, 300m);
            if (subtotalAfterDiscount == 300m && (baseAmount - discountAmount) < 300m) 
                notes += "minimum discounted subtotal applied; ";

            var fees = _feeCalculator.CalculateFees(normalizedPlanCode, normalizedPaymentMethod, includePremiumSupport, subtotalAfterDiscount);
            
            decimal taxBase = subtotalAfterDiscount + fees.SupportFee + fees.PaymentFee;
            decimal taxAmount = _taxCalculator.CalculateTax(customer.Country, taxBase);
            
            decimal finalAmount = Math.Max(taxBase + taxAmount, 500m);
            if (finalAmount == 500m && (taxBase + taxAmount) < 500m) 
                notes += "minimum invoice amount applied; ";

            var invoice = BuildInvoice(customer, normalizedPlanCode, normalizedPaymentMethod, seatCount, baseAmount, discountAmount, fees.SupportFee, fees.PaymentFee, taxAmount, finalAmount, notes + fees.Notes);
            
            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                _billingGateway.SendEmail(customer.Email, "Subscription renewal invoice", 
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} has been prepared. Final amount: {invoice.FinalAmount:F2}.");
            }

            return invoice;
        }
        private void ValidateInput(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan code is required");
            if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required");
        }

        private RenewalInvoice BuildInvoice(Customer customer, string planCode, string paymentMethod, int seatCount, decimal baseAmount, decimal discountAmount, decimal supportFee, decimal paymentFee, decimal taxAmount, decimal finalAmount, string notes)
        {
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customer.Id}-{planCode}",
                CustomerName = customer.FullName,
                PlanCode = planCode,
                PaymentMethod = paymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
