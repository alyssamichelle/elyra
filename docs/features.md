# Elyra Dashboard Features

## Ask Elyra Anything (Top Bar AI Query)

The top-bar input now supports real, structured AI-like actions against the live transaction dataset without requiring an external LLM.

### Feature behavior

- Parses plain-English prompts into supported intents.
- Executes the interpreted query against the existing grid transaction data.
- Updates the Smart Grid view with the matching operational subset.
- Displays an **AI Result** explanation (1–3 sentences) in the AI sidebar.
- Handles unsupported prompts safely with fallback guidance and suggested examples.
- **Current behavior + user test:** Enter `show failed > £3k` and click **Ask**; the grid should narrow to failed high-value rows and the sidebar should show an AI explanation matching that filtered result.

### Supported queries

- `show failed > £3k`
- `why did failures increase`
- `top risky merchants`

### How to use it

1. In the top bar, type a prompt into **Ask Elyra anything…**.
2. Click **Ask**.
3. Review:
   - the updated grid rows (filtered/re-ranked based on intent)
   - the **AI Result** card in the right sidebar for an explanation
4. If your prompt is unsupported, use one of the suggested examples shown in the fallback response.

### Implementation details (modular design)

Feature is implemented with separated, testable modules:

- `Services/AiQuery/AiQueryParser.cs`
  - Converts prompt text into intent + parameters.
- `Services/AiQuery/AiQueryExecutor.cs`
  - Applies intent logic to `TransactionRecord` data and returns result rows.
- `Services/AiQuery/AiQueryResponseFormatter.cs`
  - Generates human-readable AI summaries and fallback suggestions.

Supporting models:

- `Models/AiQueryIntent.cs`
- `Models/AiQueryInterpretation.cs`
- `Models/AiQueryExecutionResult.cs`
- `Models/AiQueryResponse.cs`

UI wiring:

- `Components/Dashboard/TopBar.razor` (captures prompt and triggers Ask action)
- `Pages/Index.razor` (orchestrates parse/execute/format and updates visible grid data)
- `Components/Dashboard/AiAssistantSidebar.razor` (renders AI Result panel)

### Validation method

- Build verification with `dotnet build`.
- Lint verification on changed files.
- Browser runtime checks using automation to run prompt scenarios and verify:
  - grid result changes
  - explanation rendering
  - fallback handling for unsupported prompts

### Validation evidence

1. `show failed > £3k`
   - Grid narrowed to failed, high-value transactions.
   - AI Result explanation correctly described findings.
2. `why did failures increase`
   - Grid switched to recent failure-focused subset.
   - AI Result provided concise likely cause summary.
3. `top risky merchants`
   - Grid reflected top risky merchant-related transactions.
   - AI Result listed merchants and explained sorting focus.
4. Unsupported query (example: `explain settlements by weather`)
   - No crash.
   - Fallback message appeared with example prompts.

Console checks during validation reported no active runtime errors for this feature flow.

## Feature 2: Make AI Sidebar Actions Functional

The right-side AI action buttons now execute real analytics logic against the **current filtered grid view**.

### Feature behavior

- **Summarize current view** computes:
  - total transaction count
  - successful/failed/pending counts
  - failure rate
  - average amount
  - top merchant by volume and top merchant by count
  - high-risk transaction count (`risk >= 70`)
- **Detect anomalies** runs rule-based checks:
  - repeated failures by merchant in a recent time window
  - payment rail concentration spike
  - high-risk failed cluster above threshold (`failed`, `risk >= 75`, `amount >= £3k`)
- Adds timestamped insight cards in the AI sidebar for each button run.
- Flags anomaly-linked transactions directly in the grid with a `Flagged` badge.
- **Current behavior + user test:** Apply a filter (for example `failed`), click **Summarize current view**, then click **Detect anomalies**; summary metrics should reflect the filtered subset and anomaly runs should add insights plus `Flagged` badges on related rows.

### How to use it

1. Optionally narrow the grid using:
   - top-bar Ask query
   - `Filter with AI…` input
   - quick filter chips
2. Click **Summarize current view** to generate an operational summary insight.
3. Click **Detect anomalies** to run anomaly rules on the same filtered rows.
4. Re-run after changing filters to compare insights between views.

### Implementation details (modular design)

- `Services/AiInsights/AiInsightsEngine.cs`
  - `SummarizeCurrentView(...)` returns data-derived summary message.
  - `DetectAnomalies(...)` returns anomaly insight + flagged transaction IDs.
- `Models/AiInsightMessage.cs`
  - Timestamped insight payload rendered in sidebar.
- `Models/AiAnomalyResult.cs`
  - Carries both anomaly message and row-flag set.

UI wiring:

- `Pages/Index.razor`
  - Maintains `CurrentGridView`, insight history, and flagged rows.
  - Handles action callbacks and updates state.
- `Components/Dashboard/SmartGridSection.razor`
  - Emits current filtered rows via `OnCurrentViewChanged`.
  - Renders flag indicators in transaction ID cells.
- `Components/Dashboard/AiAssistantSidebar.razor`
  - Triggers summarize/anomaly callbacks.
  - Renders insight stream with timestamps and severity styling.

### Validation method

- Build verification with `dotnet build`.
- Lint verification on changed files.
- Browser runtime checks using automation to:
  - run summary/anomaly actions
  - change filters between runs
  - verify insight changes and row flagging

### Validation evidence

Validated on a running app instance:

1. Baseline summary (no extra filter)
   - Insight generated with full-view metrics:
   - `Current view contains 12 transactions: 6 successful, 4 failed, 2 pending (failure rate 33.3%).`
2. Changed filter (`Filter with AI…` = `failed`) and re-ran summary
   - Insight changed correctly for filtered subset:
   - `Current view contains 4 transactions: 0 successful, 4 failed, 0 pending (failure rate 100.0%).`
3. Ran anomaly detection on filtered view
   - Insight generated:
   - `High-risk failed cluster found: 2 failed payments above £3,000 with risk >= 75.`
   - Grid row flagging confirmed (`FLAGGED` count: `2`).

This confirms both sidebar actions are visible, data-driven, and responsive to filter changes.

## Feature 3: Add Richer Demo Data + Simulated Live Updates

The dashboard now supports realistic large-scale exploration with a live-like stream mode.

### Feature behavior

- Seeds **520 realistic UK fintech transactions** at startup for richer demo coverage.
- Preserves existing grid columns while enriching each transaction with:
  - `DeclineReason`
  - `RetryCount`
  - `IssuerResponseCode`
  - `CustomerSegment`
  - `Country`
  - `Channel`
- Adds `ProcessingTimeSeconds` to support operational KPI recalculation.
- AI summary/anomaly logic now uses richer fields (decline reason, retry behavior, issuer code, channel, customer segment).
- Adds a top-bar **Simulated Live** toggle:
  - injects a new transaction every 4 seconds when enabled
  - re-applies active Ask-query intent to keep grid context coherent
  - recalculates KPI cards/deltas continuously
  - pushes timestamped live feed events into AI insights stream
- **Current behavior + user test:** Turn on **Simulated Live** for at least 8-12 seconds and confirm item count increases, KPI values update, and new live insight entries appear while existing filters and Ask-query context remain applied.

### How to use it

1. Open the dashboard and inspect baseline dataset (500+ rows with paging/filter/sort).
2. Enable **Simulated Live** from the top bar.
3. Observe:
   - grid total count incrementing as rows are injected
   - KPI values/deltas recalculating
   - AI insights receiving live feed/event entries
4. Use Ask/filter/chips while live mode is running to verify behavior remains responsive and contextual.

### Implementation details (modular design)

- `Services/DemoData/TransactionDataFactory.cs`
  - `GenerateSeedData(...)` creates 520 deterministic, varied records.
  - `CreateLiveTransaction(...)` generates a realistic incremental transaction.
- `Models/TransactionRecord.cs`
  - extended with richer operational fields and processing time.
- `Pages/Index.razor`
  - live-mode loop via `PeriodicTimer` + cancellation token.
  - transaction injection, KPI rebuild, active-query re-application.
  - timestamped live insight insertion and cleanup.
- `Services/AiInsights/AiInsightsEngine.cs`
  - enriched summaries/anomalies using decline and issuer/channel signals.
- `Components/Dashboard/TopBar.razor`
  - Simulated Live toggle wiring.

### Validation method

- Build verification with `dotnet build`.
- Runtime validation via browser automation:
  - verify initial data volume
  - enable live mode
  - verify count increment, KPI value change, and live insight generation

### Validation evidence

Observed during live-mode validation run:

- Pager count increased:
  - before: `1 - 8 of 520 items`
  - after: `1 - 8 of 521 items`
- KPI changed while live mode ran:
  - before: `£438,782`
  - after: `£433,508`
- AI insights updated with live entries:
  - `INSIGHTS_COUNT: 1`

This confirms live mode updates grid + KPIs + AI sidebar and supports realistic large dataset exploration.
