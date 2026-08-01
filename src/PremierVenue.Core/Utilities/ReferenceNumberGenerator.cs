using System;
namespace PremierVenue.Core.Utilities;

public static class ReferenceNumberGenerator
{
    public static string GenerateBookingReference()
    {
        // Format: PV-YYYYMMDD-XXXX (e.g., PV-20240111-1234)
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(1000, 9999).ToString();
        return $"PV-{datePart}-{randomPart}";
    }

    public static string GeneratePaymentReference()
    {
        // Format: PAY-YYYYMMDD-XXXX (e.g., PAY-20240111-5678)
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(1000, 9999).ToString();
        return $"PAY-{datePart}-{randomPart}";
    }
}