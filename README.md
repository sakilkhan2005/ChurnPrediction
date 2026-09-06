# Customer Churn Prediction API

An end-to-end machine learning project that predicts customer churn using ML.NET,
served via an ASP.NET Core Web API. Built to demonstrate applied ML skills within
the .NET ecosystem — from raw data exploration through to a containerized,
production-shaped API.

## Dataset

**Source:** [IBM Telco Customer Churn (11.1.3+)](https://www.kaggle.com/datasets/alfathterry/telco-customer-churn-11-1-3), MIT License, via Kaggle.

- 7,043 customer records, 50 original columns
- Reduced to 34 columns (33 features + `Churn Label` target) after removing
  identifiers, geography, and data-leakage columns (see below)
- **No missing values** in `Total Charges` in this dataset version — unlike the
  original/classic Telco churn dataset, which has 11 blank rows for zero-tenure
  customers
- **Churn rate: 26.5% Yes / 73.5% No** — a meaningfully imbalanced classification
  problem. This directly informs the choice of evaluation metrics (see
  Model Evaluation below): raw accuracy would be misleading here, since a model
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

**Redundant / descoped for v1:**
- `Under 30` — directly derivable from `Age`
- `Total Revenue` — an aggregate of `Total Charges` + `Total Refunds` +
  `Total Extra Data Charges` + `Total Long Distance Charges`; kept the components
  instead to avoid multicollinearity
- `CLTV` — a legitimate real-world feature, descoped for v1 to keep this project
  focused on churn signal rather than customer-value prediction

**Final feature set (33 columns):** Gender, Age, Senior Citizen, Married,
Dependents, Number of Dependents, Referred a Friend, Number of Referrals,
Tenure in Months, Offer, Phone Service, Avg Monthly Long Distance Charges,
Multiple Lines, Internet Service, Internet Type, Avg Monthly GB Download,
Online Security, Online Backup, Device Protection Plan, Premium Tech Support,
Streaming TV, Streaming Movies, Streaming Music, Unlimited Data, Contract,
Paperless Billing, Payment Method, Monthly Charge, Total Charges, Total Refunds,
Total Extra Data Charges, Total Long Distance Charges, Satisfaction Score

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


## Tech Stack

C# / .NET 8, ML.NET, ASP.NET Core Web API, xUnit, Docker


## Model Performance & Threshold Selection

**Algorithm:** SdcaLogisticRegression (ML.NET)

**Initial result:** 99.4% AUC — investigated and found to be inflated by data
leakage. `Satisfaction Score` was very likely collected concurrently with the
churn decision itself (as part of the same exit/retention interaction), rather
than being an independent early-warning signal. Removed as a feature.

**Corrected result** (33 → 32 features, `Satisfaction Score` removed):
- Accuracy: 83.4%
- AUC: 90.4%
- F1: 62.7% (at default 0.5 threshold)

**Threshold tuning:** Since missing a churner (false negative) is the costlier
mistake for this business — a lost customer with zero retention outreach —
versus a false alarm (false positive) costing only an unnecessary retention
offer, the default 0.5 probability threshold was tuned:

| Threshold | Precision | Recall | F1 |
|---|---|---|---|
| 0.50 (default) | 80.9% | 51.2% | 62.7% |
| 0.30 | 68.7% | 74.8% | 71.6% |
| **0.25 (chosen)** | 65.2% | 81.0% | 72.2% |
| 0.20 | 60.4% | 84.2% | 70.3% |

**Selected operating threshold: 0.25** — best F1 score, and moves recall from
51% to 81% (catching far more actual churners) while precision only drops from
81% to 65%. This threshold will be applied in the API's prediction logic
rather than relying on ML.NET's default 0.5 cutoff.

**Selected operating threshold: 0.30** — best F1 score in the final training
run (72.6%), moving recall from ~50% to ~75% while precision remains reasonable
at ~70%.

*Note: metrics show minor run-to-run variance (e.g., accuracy 83.4–83.9%,
optimal threshold 0.25–0.30) due to non-determinism in ML.NET's SDCA trainer
internals, even with a fixed seed. The final numbers above reflect the model
actually shipped in model.zip.*


## Status

🚧 In progress. Current stage: Phase 8 — Testing.