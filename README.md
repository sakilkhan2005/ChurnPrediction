# Customer Churn Prediction API

A production-shaped ML.NET service that predicts customer churn risk, with a
documented data-leakage investigation and business-driven threshold tuning —
not just a tutorial clone.

**Stack:** C# · .NET 8 · ML.NET · ASP.NET Core Web API · Docker · xUnit

[Quick Start](#quick-start) · [Dataset](#dataset) · [Key Decisions](#feature-selection--leakage-handling) · [Model Performance](#model-performance--threshold-selection) · [Testing](#testing)

> Built to apply ML.NET in a real, non-trivial way after 15 years in enterprise
> .NET development — includes a genuine data-leakage catch (see below) rather
> than a polished-up tutorial result.

---

## Quick Start

```bash
docker build -t churn-api .
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 -e ASPNETCORE_ENVIRONMENT=Development churn-api
```

Open `http://localhost:8080/swagger` and try the `POST /api/Prediction` endpoint.

Sample response for a high-risk customer profile (month-to-month contract, 2-month tenure, no add-on services):

```json
{
  "willChurn": true,
  "probability": 0.7347772,
  "thresholdUsed": 0.3
}
```

Full setup, retraining, and testing instructions: [HOW_TO_RUN.md](./HOW_TO_RUN.md)

---

## Dataset

**Source:** [IBM Telco Customer Churn (11.1.3+)](https://www.kaggle.com/datasets/alfathterry/telco-customer-churn-11-1-3), MIT License, via Kaggle.

- 7,043 customer records, 50 original columns
- Reduced to 33 columns (32 features + `Churn Label` target) after removing
  identifiers, geography, and data-leakage columns (see below)
- **No missing values** in `Total Charges` in this dataset version — unlike the
  original/classic Telco churn dataset, which has 11 blank rows for zero-tenure
  customers
- **Churn rate: 26.5% Yes / 73.5% No** — a meaningfully imbalanced classification
  problem. This directly informs the choice of evaluation metrics (see
  Model Performance below): raw accuracy would be misleading here, since a model
  that always predicts "No churn" would score ~73.5% accuracy while being useless.

## Feature Selection & Leakage Handling

The raw dataset ships with several columns that had to be excluded before training:

**Identifiers / geography (no predictive value for this exercise):**
`Customer ID`, `Country`, `State`, `City`, `Zip Code`, `Latitude`, `Longitude`,
`Population`, `Quarter`

**Data leakage (only known after, or because, the customer churned):**
- `Customer Status` — directly states "Churned/Stayed/Joined"
- `Churn Score` — a pre-computed churn prediction from IBM's own SPSS Modeler tool;
  using another model's output as an input feature would be leakage
- `Churn Category`, `Churn Reason` — only exist for customers who already churned
- `Satisfaction Score` — initially included, then removed after investigation
  (see Model Performance below); likely collected concurrently with the churn
  decision itself rather than as an independent early-warning signal

**Redundant / descoped for v1:**
- `Under 30` — directly derivable from `Age`
- `Total Revenue` — an aggregate of `Total Charges` + `Total Refunds` +
  `Total Extra Data Charges` + `Total Long Distance Charges`; kept the components
  instead to avoid multicollinearity
- `CLTV` — a legitimate real-world feature, descoped for v1 to keep this project
  focused on churn signal rather than customer-value prediction

**Final feature set (32 columns):** Gender, Age, Senior Citizen, Married,
Dependents, Number of Dependents, Referred a Friend, Number of Referrals,
Tenure in Months, Offer, Phone Service, Avg Monthly Long Distance Charges,
Multiple Lines, Internet Service, Internet Type, Avg Monthly GB Download,
Online Security, Online Backup, Device Protection Plan, Premium Tech Support,
Streaming TV, Streaming Movies, Streaming Music, Unlimited Data, Contract,
Paperless Billing, Payment Method, Monthly Charge, Total Charges, Total Refunds,
Total Extra Data Charges, Total Long Distance Charges

**Target:** `Churn Label` (Yes/No)

## Architecture

```
CSV data
  ↓
ML.NET (MLContext)
  ↓
Train/test split → transforms → binary classification training
  ↓
Evaluation (precision/recall/F1/AUC — not raw accuracy, due to class imbalance)
  ↓
Save model.zip
  ↓
ASP.NET Core Web API
  ↓
POST /api/prediction
  ↓
Prediction + Probability
```

## Model Performance & Threshold Selection

**Algorithm:** SdcaLogisticRegression (ML.NET)

**Initial result:** 99.4% AUC using 33 features including `Satisfaction Score`
— investigated and found to be inflated by data leakage. `Satisfaction Score`
was very likely collected concurrently with the churn decision itself (as part
of the same exit/retention interaction), rather than being an independent
early-warning signal. Removed as a feature.

**Corrected result** (32 features, `Satisfaction Score` removed):
- Accuracy: ~83.4–83.9%
- AUC: ~90.4–90.7%
- F1: ~62.6–62.7% (at default 0.5 threshold)

**Threshold tuning:** Since missing a churner (false negative) is the costlier
mistake for this business — a lost customer with zero retention outreach —
versus a false alarm (false positive) costing only an unnecessary retention
offer, the default 0.5 probability threshold was tuned:

| Threshold | Precision | Recall | F1 |
|---|---|---|---|
| 0.50 (default) | 80.9–84.9% | 49.6–51.2% | 62.6–62.7% |
| 0.30 (chosen) | 68.7–70.1% | 74.8–75.3% | **71.6–72.6%** |
| 0.25 | 64.8–65.2% | 79.9–81.0% | 71.5–72.2% |
| 0.20 | 60.4–60.6% | 83.4–84.2% | 70.2–70.3% |

**Selected operating threshold: 0.30** — consistently the best or near-best F1
score across training runs, moving recall from ~50% to ~75% (catching far more
actual churners) while precision remains reasonable at ~70%. Applied manually
in the API's prediction logic rather than relying on ML.NET's default 0.5 cutoff.

*Note: metrics show minor run-to-run variance due to non-determinism in
ML.NET's SDCA trainer internals, even with a fixed seed (`MLContext(seed: 0)`).
The ranges above reflect that variance across multiple training runs; the
model actually shipped in `model.zip` uses threshold 0.30.*

## Testing

- **Unit tests** (`PredictionServiceTests`) — mock `IModelPredictor` to test the
  0.30 threshold decision logic in isolation, including a boundary test at
  exactly 0.30.
- **Integration test** (`ModelIntegrationTests`) — loads the real `model.zip`
  and verifies a known high-risk customer profile produces a probability above
  the operating threshold, proving the actual shipped model artifact works end
  to end.

Run via **Test Explorer** in Visual Studio, or `dotnet test` from the solution root.

## Tech Stack

C# / .NET 8, ML.NET, ASP.NET Core Web API, xUnit, Docker

## Future Improvements

- Compare `SdcaLogisticRegression` against `LightGbm`/`FastTree`
- Add a `/api/retrain` endpoint (behind auth) for periodic retraining on new data
- Deploy to Azure App Service or Container Apps with CI/CD via GitHub Actions
- Swap CSV source for SQL Server, connecting this project to my existing SQL Server expertise