namespace ChurnPrediction.Api.DTOs;

public class ChurnRequest
{
    public string Gender { get; set; } = string.Empty;
    public float Age { get; set; }
    public string SeniorCitizen { get; set; } = string.Empty;
    public string Married { get; set; } = string.Empty;
    public string Dependents { get; set; } = string.Empty;
    public float NumberOfDependents { get; set; }
    public string ReferredAFriend { get; set; } = string.Empty;
    public float NumberOfReferrals { get; set; }
    public float TenureInMonths { get; set; }
    public string Offer { get; set; } = string.Empty;
    public string PhoneService { get; set; } = string.Empty;
    public float AvgMonthlyLongDistanceCharges { get; set; }
    public string MultipleLines { get; set; } = string.Empty;
    public string InternetService { get; set; } = string.Empty;
    public string InternetType { get; set; } = string.Empty;
    public float AvgMonthlyGBDownload { get; set; }
    public string OnlineSecurity { get; set; } = string.Empty;
    public string OnlineBackup { get; set; } = string.Empty;
    public string DeviceProtectionPlan { get; set; } = string.Empty;
    public string PremiumTechSupport { get; set; } = string.Empty;
    public string StreamingTV { get; set; } = string.Empty;
    public string StreamingMovies { get; set; } = string.Empty;
    public string StreamingMusic { get; set; } = string.Empty;
    public string UnlimitedData { get; set; } = string.Empty;
    public string Contract { get; set; } = string.Empty;
    public string PaperlessBilling { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public float MonthlyCharge { get; set; }
    public float TotalCharges { get; set; }
    public float TotalRefunds { get; set; }
    public float TotalExtraDataCharges { get; set; }
    public float TotalLongDistanceCharges { get; set; }
}