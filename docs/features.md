# Elyra Dashboard Features

## Implementation Notes

- Dark/light mode restoration was requested with this prompt:
  - `I love the added components, however, we seemed to have lost our dark mode/light mode button. I need you to bring that back and ensure using playwright that all components are using a dark theme in dark mode and a light theme in light mode.`
- Implementation summary for that request:
  - Restored the top-bar theme toggle in `Components/Dashboard/TopBar.razor`.
  - Added page-level theme state wiring (`theme-dark` / `theme-light`) in `Pages/Index.razor`.
  - Added dual-mode style coverage in `wwwroot/css/site.css` for dashboard shell + Telerik Prompt/Chat/Grid surfaces.
  - Verified in Playwright that dark mode and light mode both apply to main shell, AI prompt, chat, and grid toolbar.
- User recording for this implementation:
  - [screen.studio/share/cB59H50i?state=uploading](https://screen.studio/share/cB59H50i?state=uploading)

## Ask Elyra Anything (Telerik AI Prompt Experience)

The dashboard now uses the Telerik Blazor `AIPrompt` component as the primary prompt surface, while keeping deterministic, allowlisted transaction-query execution behind it.

### Feature behavior

- Presents a Telerik-native prompt workflow:
  - `TelerikAIPrompt` prompt + output views
  - built-in prompt suggestions
  - speech-to-text entry for prompt capture
  - AI result mirrored into the right-side assistant panel
- Parses plain-English prompts into supported intents.
- Executes interpreted queries against live transaction data.
- Updates Smart Grid to the matching operational subset.
- Keeps parser/executor fallback active so demos remain functional without an external LLM.
- Handles unsupported prompts safely with fallback guidance.
- Expands phrase coverage for higher-variance language such as `show me the worst transactions in the last month`.
- **Current behavior + user test:** Enter `show me the worst transactions in the last month` and click **Generate**; the grid should narrow to the highest-risk transactions from the last month and the sidebar should show the same explanation.

### Supported queries

- `show failed > £3k`
- `why did failures increase`
- `top risky merchants`
- `show me the worst transactions in the last month`

### How to use it

1. Use the Telerik **Ask Elyra anything** prompt console under the top bar.
2. Enter a prompt (or click one of the prompt suggestions) and click **Generate**.
3. Review:
   - the `AIPrompt` output view
   - the **AI Result** card in the assistant sidebar
   - Smart Grid updates for matching rows
   - the assistant chat thread for follow-up prompts
4. For ambiguous/unsupported prompts, follow fallback guidance and suggestions.

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

- `Components/Dashboard/AiPromptConsole.razor` (`TelerikAIPrompt` wrapper with prompt suggestions and speech-to-text)
- `Pages/Index.razor` (orchestrates prompt events from Telerik AI surfaces and maintains assistant conversation)
- `Components/Dashboard/TopBar.razor` (branding + live mode controls)
- `Components/Dashboard/AiAssistantSidebar.razor` (`TelerikChat` sidebar + AI result card + insight actions)
- `Components/Dashboard/SmartGridSection.razor` (Grid smart box AI entry points + export actions)

### Validation method

- Build verification with `dotnet build`.
- Lint verification on changed files.
- Browser runtime checks using automation to run prompt scenarios and verify:
  - grid result changes
  - explanation rendering
  - fallback handling for unsupported prompts

### Validation evidence

1. `show failed > £3k`
   - Grid narrows to failed, high-value transactions.
   - `AIPrompt` output and sidebar AI result record the interpreted result.
2. `why did failures increase`
   - Grid switches to recent failure-focused subset.
   - Output view and sidebar response report rail/decline reason interpretation.
3. `top risky merchants`
   - Grid reflects top risky merchant-related transactions.
   - Chat follow-up prompt path also updates the grid and sidebar summary correctly.
4. `show me the worst transactions in the last month`
   - Grid narrows to 25 highest-risk transactions within the last-month window.
   - Runtime validation confirmed pager count dropped to `1 - 8 of 25 items`.
5. Unsupported query (example: `explain settlements by weather`)
   - No crash.
   - Fallback response appears with guidance/suggestions.

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
- Uses Telerik `Chat` as the main conversational container for follow-up prompts and guided suggestions.
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
