# How to Run & Test — Customer Churn Prediction API

A quick reference for running this project from scratch, in Visual Studio, and via Docker.

---

## 1. Prerequisites

- **.NET 8 SDK** installed (`dotnet --version` should show 8.x)
- **Visual Studio 2022** (Community or higher)
- **Docker Desktop** installed and running (only needed for the Docker section)
- Repo cloned locally, `ChurnPrediction.sln` opened in Visual Studio

---

## 2. Retrain the model (optional — model.zip is already committed)

You only need this if you've changed the training code or want to regenerate
`model.zip` from scratch. If you just want to run the API, skip to Section 3 —
a trained `model.zip` already ships in `ChurnPrediction.Api/MLModels/`.

1. In Solution Explorer, right-click **`ChurnPrediction.ML`** → **Set as Startup Project**.
2. Press **Ctrl+F5** (Start Without Debugging).
3. Wait for the console to finish — it will:
   - Load and split the data
   - Train the model
   - Print evaluation metrics (accuracy, AUC, F1, precision, recall)
   - Print the threshold-tuning table
   - Save a fresh `model.zip` to `ChurnPrediction.Api/MLModels/model.zip`
   - Verify the save/load round-trip
4. Confirm the console ends with:
   ```
   Model save/load round-trip verified successfully.
   ```

---

## 3. Run the API locally (Visual Studio)

1. Right-click **`ChurnPrediction.Api`** → **Set as Startup Project**.
2. Press **Ctrl+F5** (Start Without Debugging).
3. Click **Yes** if prompted to trust the local HTTPS development certificate (first run only).
4. Your browser should open automatically to:
   ```
   https://localhost:<port>/swagger
   ```
   If it doesn't open automatically, check the console output for the exact URL/port.

---

## 4. Test the API via Swagger UI

1. On the Swagger page, expand **`POST /api/Prediction`**.
2. Click **Try it out**.
3. Paste this sample payload (a high-churn-risk profile) into the request body:

```json
{
  "gender": "Male",
  "age": 45,
  "seniorCitizen": "No",
  "married": "No",
  "dependents": "No",
  "numberOfDependents": 0,
  "referredAFriend": "No",
  "numberOfReferrals": 0,
  "tenureInMonths": 2,
  "offer": "None",
  "phoneService": "Yes",
  "avgMonthlyLongDistanceCharges": 10,
  "multipleLines": "No",
  "internetService": "Yes",
  "internetType": "Fiber Optic",
  "avgMonthlyGBDownload": 20,
  "onlineSecurity": "No",
  "onlineBackup": "No",
  "deviceProtectionPlan": "No",
  "premiumTechSupport": "No",
  "streamingTV": "No",
  "streamingMovies": "No",
  "streamingMusic": "No",
  "unlimitedData": "No",
  "contract": "Month-to-Month",
  "paperlessBilling": "Yes",
  "paymentMethod": "Mailed Check",
  "monthlyCharge": 95,
  "totalCharges": 190,
  "totalRefunds": 0,
  "totalExtraDataCharges": 0,
  "totalLongDistanceCharges": 20
}
```

4. Click **Execute**.
5. Expected response (values may vary slightly run-to-run — see README's note on trainer non-determinism):
```json
{
  "willChurn": true,
  "probability": 0.73,
  "thresholdUsed": 0.3
}
```

**Try a low-risk profile too**, to see the other direction — long tenure, two-year contract, several add-on services tends to predict `willChurn: false`:

```json
{
  "gender": "Female",
  "age": 52,
  "seniorCitizen": "No",
  "married": "Yes",
  "dependents": "Yes",
  "numberOfDependents": 2,
  "referredAFriend": "Yes",
  "numberOfReferrals": 3,
  "tenureInMonths": 60,
  "offer": "Offer A",
  "phoneService": "Yes",
  "avgMonthlyLongDistanceCharges": 15,
  "multipleLines": "Yes",
  "internetService": "Yes",
  "internetType": "DSL",
  "avgMonthlyGBDownload": 10,
  "onlineSecurity": "Yes",
  "onlineBackup": "Yes",
  "deviceProtectionPlan": "Yes",
  "premiumTechSupport": "Yes",
  "streamingTV": "No",
  "streamingMovies": "No",
  "streamingMusic": "No",
  "unlimitedData": "Yes",
  "contract": "Two Year",
  "paperlessBilling": "No",
  "paymentMethod": "Bank Withdrawal",
  "monthlyCharge": 60,
  "totalCharges": 3600,
  "totalRefunds": 0,
  "totalExtraDataCharges": 0,
  "totalLongDistanceCharges": 900
}
```

---

## 5. Run the automated tests

1. **Test** menu → **Test Explorer** (or `Ctrl+E, T`).
2. Click **Run All Tests** (▶ icon at the top of the panel).
3. Expect **4 tests, all passing**:
   - `PredictionServiceTests.Predict_WhenProbabilityAboveThreshold_ReturnsWillChurnTrue`
   - `PredictionServiceTests.Predict_WhenProbabilityBelowThreshold_ReturnsWillChurnFalse`
   - `PredictionServiceTests.Predict_WhenProbabilityExactlyAtThreshold_ReturnsWillChurnTrue`
   - `ModelIntegrationTests.LoadedModel_OnHighRiskProfile_PredictsChurn`

If the integration test fails with a "model.zip not found" message, run Section 2 (retrain) first, or confirm `ChurnPrediction.Api/MLModels/model.zip` exists on disk.

---

## 6. Run via Docker

From the **solution root** (same folder as `ChurnPrediction.sln`), open a terminal:

```bash
# Build the image
docker build -t churn-api .

# Run the container
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 -e ASPNETCORE_ENVIRONMENT=Development churn-api
```

Then open:
```
http://localhost:8080/swagger
```

**Note:** use `http`, not `https` — the container isn't configured with a local dev certificate. `ASPNETCORE_ENVIRONMENT=Development` is required for Swagger UI to be enabled (it's intentionally disabled in Production by default).

Test it the same way as Section 4 — same sample payloads, same expected response shape.

To stop the container: press `Ctrl+C` in the terminal, or run `docker ps` to find the container ID and `docker stop <id>`.

---

## 7. Quick troubleshooting

| Symptom | Likely cause |
|---|---|
| Swagger page 404s | `ASPNETCORE_ENVIRONMENT` isn't set to `Development` (common in Docker) |
| `model.zip not found` error | Model hasn't been trained/saved yet — run Section 2 |
| Integration test fails but unit tests pass | `model.zip` missing or stale relative to current schema — retrain |
| API predicts but values look wrong | Double-check JSON field names match `ChurnRequest` exactly (case-sensitive in some clients) |
| Docker build fails at `dotnet restore` | Check you're running the build command from the solution root, not a subfolder |
