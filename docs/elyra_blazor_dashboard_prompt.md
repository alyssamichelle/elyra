# Elyra — Focused Blazor Dashboard Prompt
*Real-time transaction intelligence*

---

## Overview

Build a responsive, production-quality fintech dashboard using **Telerik UI for Blazor**.

This should be a **single-screen dashboard**, not a multi-page application and not a generic component showcase. The experience should feel like a modern AI-powered SaaS product with a polished dark-mode interface, strong visual hierarchy, and clean responsive behavior.

---

## Core Goal

Create a working demo that highlights only three key experiences:

- KPI cards across the top for high-level payment operations metrics  
- A Smart Grid as the main hero section for transaction monitoring  
- A persistent AI assistant sidebar that explains activity in the grid and supports simple actions  

Do not include Scheduler, Gantt, PivotGrid, extra chart rows, or too many secondary features.

---

## Design Direction

- Dark mode only  
- Modern fintech SaaS aesthetic  
- Deep navy / near-black background  
- Emerald accent #00C896  
- Glassmorphism-style cards with subtle blur and soft borders  
- Clean spacing, polished hover states, and premium visual depth  
- Responsive layout that works well on desktop, tablet, and mobile  

---

## Layout

### Top Bar

Include:

- Elyra logo  
- Subline: *Real-time transaction intelligence*  
- Small “Powered by Telerik Blazor” badge  
- AI search input with placeholder: **Ask Elyra anything…**  
- Notification icon  
- User avatar  

Keep this bar minimal and polished.

---

## Section 1: KPI Cards

Add 4 to 6 KPI cards in a responsive row.

### Suggested Metrics

- Total Volume  
- Successful Payments  
- Failed Transactions  
- Avg. Processing Time  
- Fraud Alerts  
- Net Settlement  

### Each Card Should Include

- Metric label  
- Large value  
- Delta badge  
- Small sparkline or progress accent  

These cards should feel elegant and readable, not crowded.

---

## Section 2: Smart Grid (Hero Component)

Use **Telerik Blazor Grid** as the centerpiece of the page.

### Include

- A natural language filter input above the grid with placeholder: **Filter with AI…**  
- 2 or 3 quick filter chips such as:  
  - Failed today  
  - High value  
  - Fraud flagged  

### Grid Columns

- Transaction ID  
- Merchant  
- Amount  
- Currency  
- Status  
- Payment Rail  
- Risk Score  
- Timestamp  

### Requirements

- Modern dark styling  
- Row hover states  
- Strong visual hierarchy  
- Status badges  
- Risk score as a visual bar or progress indicator  
- Responsive behavior for smaller screens  

The grid should feel like a real operations command center, not a plain table.

---

## Section 3: AI Assistant Sidebar

Add a persistent right-side AI panel on desktop.  
On tablet/mobile, it should collapse or move below the main content.

### Include

- Header: **Elyra AI**  
- A short mock chat conversation based on the visible transactions  
- One example where the user asks why failed payments increased  
- One example where AI identifies a fraud-related pattern  
- 2 quick action buttons:  
  - Summarize current view  
  - Detect anomalies  

The sidebar should feel useful and grounded in the grid content, but it does not need real AI backend functionality.

---

## Data Context

Use realistic UK fintech / payments operations sample data:

- GBP currency  
- Merchant names that feel like real UK businesses  
- Transaction IDs in format `TXN-2024-XXXXXX`  
- Risk scores from 0–100  
- Realistic timestamps relative to today  

---

## Technical Requirements

- Use Telerik UI for Blazor components wherever appropriate  
- Build this as a working demo  
- Keep the code clean and scalable  
- Use reusable sections/components instead of putting everything in one giant file  
- Prioritize responsiveness and maintainability over adding more features  

---

## Avoid

- Multiple screens or tabs  
- Too many charts  
- PivotGrid, Gantt, Scheduler  
- Visual clutter  
- Flat generic admin dashboard styling  
- Fake complexity that hurts responsiveness  

---

## Final Result

The final UI should feel like a focused AI-powered fintech monitoring dashboard with just enough intelligence to be compelling, while still being realistic to build and demo.

---

## Credits

Created collaboratively by Alyssa Nicoll and ChatGPT.
