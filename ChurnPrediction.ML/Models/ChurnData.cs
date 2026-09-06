using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace ChurnPrediction.ML.Models
{

    public class ChurnData
    {
        [LoadColumn(0)] public string Gender { get; set; } = string.Empty;
        [LoadColumn(1)] public float Age { get; set; }
        [LoadColumn(2)] public string SeniorCitizen { get; set; } = string.Empty;
        [LoadColumn(3)] public string Married { get; set; } = string.Empty;
        [LoadColumn(4)] public string Dependents { get; set; } = string.Empty;
        [LoadColumn(5)] public float NumberOfDependents { get; set; }
        [LoadColumn(6)] public string ReferredAFriend { get; set; } = string.Empty;
        [LoadColumn(7)] public float NumberOfReferrals { get; set; }
        [LoadColumn(8)] public float TenureInMonths { get; set; }
        [LoadColumn(9)] public string Offer { get; set; } = string.Empty;
        [LoadColumn(10)] public string PhoneService { get; set; } = string.Empty;
        [LoadColumn(11)] public float AvgMonthlyLongDistanceCharges { get; set; }
        [LoadColumn(12)] public string MultipleLines { get; set; } = string.Empty;
        [LoadColumn(13)] public string InternetService { get; set; } = string.Empty;
        [LoadColumn(14)] public string InternetType { get; set; } = string.Empty;
        [LoadColumn(15)] public float AvgMonthlyGBDownload { get; set; }
        [LoadColumn(16)] public string OnlineSecurity { get; set; } = string.Empty;
        [LoadColumn(17)] public string OnlineBackup { get; set; } = string.Empty;
        [LoadColumn(18)] public string DeviceProtectionPlan { get; set; } = string.Empty;
        [LoadColumn(19)] public string PremiumTechSupport { get; set; } = string.Empty;
        [LoadColumn(20)] public string StreamingTV { get; set; } = string.Empty;
        [LoadColumn(21)] public string StreamingMovies { get; set; } = string.Empty;
        [LoadColumn(22)] public string StreamingMusic { get; set; } = string.Empty;
        [LoadColumn(23)] public string UnlimitedData { get; set; } = string.Empty;
        [LoadColumn(24)] public string Contract { get; set; } = string.Empty;
        [LoadColumn(25)] public string PaperlessBilling { get; set; } = string.Empty;
        [LoadColumn(26)] public string PaymentMethod { get; set; } = string.Empty;
        [LoadColumn(27)] public float MonthlyCharge { get; set; }
        [LoadColumn(28)] public float TotalCharges { get; set; }
        [LoadColumn(29)] public float TotalRefunds { get; set; }
        [LoadColumn(30)] public float TotalExtraDataCharges { get; set; }
        [LoadColumn(31)] public float TotalLongDistanceCharges { get; set; }
        [LoadColumn(32)] public float SatisfactionScore { get; set; }

        // Raw label as it appears in the CSV ("Yes"/"No").
        // We convert this to bool in the transform pipeline in Phase 3 —
        // not here, to keep loading and data-prep concerns separate.
        [LoadColumn(33)] public string ChurnLabel { get; set; } = string.Empty;
    }
}
