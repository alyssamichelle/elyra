# Elyra Dashboard Features (Source-Backed)

This document reflects the current implementation in the `elyra` codebase and includes direct source references plus Playwright evidence captures.

## Theme + Dashboard Shell

### What is implemented

- Page-level theme class binding is wired at the root container: `theme-dark` / `theme-light`.
  - `Pages/Index.razor:L10-L16`
- Theme state is maintained and updated through a callback.
  - `Pages/Index.razor:L47`, `Pages/Index.razor:L123-L127`
- Top bar exposes a theme toggle callback and live toggle callback.
  - `Components/Dashboard/TopBar.razor:L12-L24`, `Components/Dashboard/TopBar.razor:L37-L55`
- Theme-specific styles cover shell + Telerik prompt/chat/grid surfaces.
  - `wwwroot/css/site.css:L27-L32`, `wwwroot/css/site.css:L767-L812`, `wwwroot/css/site.css:L968-L1017`

### Playwright evidence

- Dark-mode shell and Telerik surfaces:
  - `docs/elyra-dashboard-verified.png`

## Feature 1: Ask Elyra Anything (Telerik AI Prompt + Deterministic Query Engine)

### What is implemented

- Prompt console uses Telerik `TelerikAIPrompt` with prompt/output views and speech-to-text.
  - `Components/Dashboard/AiPromptConsole.razor:L11-L22`
- Prompt handling routes through a deterministic parser/executor/formatter flow:
  - Parse: `Services/AiQuery/AiQueryParser.cs:L12-L76`
  - Execute: `Services/AiQuery/AiQueryExecutor.cs:L7-L20`
  - Format response/fallback: `Services/AiQuery/AiQueryResponseFormatter.cs:L16-L27`
- Prompt results update grid scope and sidebar AI result.
  - `Pages/Index.razor:L329-L359`
  - `Components/Dashboard/AiAssistantSidebar.razor:L7-L23`
- Grid integrates Telerik smart box AI prompt + semantic search.
  - `Components/Dashboard/SmartGridSection.razor:L31-L43`, `Components/Dashboard/SmartGridSection.razor:L235-L239`

### Supported intents in source

- Failed-above-amount: `Services/AiQuery/AiQueryParser.cs:L42-L55`
- Why failures increased: `Services/AiQuery/AiQueryParser.cs:L31-L40`
- Top risky merchants: `Services/AiQuery/AiQueryParser.cs:L22-L29`
- Worst transactions with optional time window parsing (`last month`, `last week`, etc.):
  - `Services/AiQuery/AiQueryParser.cs:L57-L69`
  - `Services/AiQuery/AiQueryParser.cs:L107-L130`

### Playwright evidence

- Prompt + AI result + grid narrowing (`show failed > £3k`):
  - `docs/elyra-after-pager-fix.png`
- Alternate prompt capture (fallback path visible for unsupported wording variant):
  - `docs/suggestion-contrast-verified.png`

## Feature 2: Functional AI Sidebar Actions

### What is implemented

- Sidebar exposes action buttons:
  - `Summarize current view`
  - `Detect anomalies`
  - `Components/Dashboard/AiAssistantSidebar.razor:L51-L61`
- Summary action computes counts, failure rate, average amount, top merchants, top decline reason, and dominant segment.
  - `Services/AiInsights/AiInsightsEngine.cs:L8-L65`
- Anomaly action evaluates repeated failures, rail spikes, and high-risk failed clusters; returns flagged row IDs.
  - `Services/AiInsights/AiInsightsEngine.cs:L67-L164`
- Page-level state updates insight stream and flagged rows.
  - `Pages/Index.razor:L129-L144`
- Grid renders `Flagged` badge on matching transaction IDs.
  - `Components/Dashboard/SmartGridSection.razor:L59-L71`

### Playwright evidence

- Sidebar actions executed with insights and flagged badges rendered in-grid:
  - `docs/elyra-dashboard-verified.png`

## Feature 3: Rich Demo Data + Simulated Live Updates

### What is implemented

- Seed data generation initializes 520 records:
  - `Pages/Index.razor:L67-L74`
  - `Services/DemoData/TransactionDataFactory.cs:L23-L38`
- Transaction model includes enriched operational fields:
  - `Models/TransactionRecord.cs:L19-L33`
- Live mode uses a periodic 4-second loop and injects new transactions.
  - `Pages/Index.razor:L173-L205`
- Active query context is reapplied after live inserts; KPI cards are rebuilt.
  - `Pages/Index.razor:L193-L204`
- Live feed and notable live-event insights are pushed into sidebar stream.
  - `Pages/Index.razor:L207-L231`
- Top bar exposes `Simulated Live` control.
  - `Components/Dashboard/TopBar.razor:L19-L24`

### Playwright evidence

- Simulated live running with live feed entries visible:
  - `docs/feature3-live-mode-validation.png`

## Validation Notes

- `features.md` claims were reconciled against current source files listed above.
- Screenshot evidence was captured against a running app instance at `http://localhost:5083`.
- Where runtime wording differs by prompt phrasing, behavior is documented as implemented by parser rules in source.
