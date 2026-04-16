# Elyra — Real-Time Transaction Intelligence (Video Script)

## Intro (0:00–0:30)

Today I want to show you something I’ve been building called *Elyra* — a real-time transaction intelligence dashboard.

This isn’t just a UI demo.  
It’s an example of what happens when you combine AI-powered workflows with real, production-ready UI components.

And the cool part?  
This entire application was scaffolded with Telerik UI Generator, then extended with deterministic AI workflows.

---

## What Elyra Is (0:30–1:10)

At a high level, Elyra is built around one simple loop:

You see what’s happening →  
You investigate →  
And then AI helps you understand why.

So instead of just staring at dashboards, you can actually interact with your data.

---

## Telerik UI Generator (1:10–1:40)

To get started, I used the Telerik UI Generator to scaffold the core dashboard experience.

That gave me:
- the layout
- the grid
- the core UI structure

From there, I layered in:
- AI-driven filtering
- deterministic query handling
- grid semantic search and AI smart actions
- and real insight generation

So this isn’t just generated UI — it’s scaffolded and extended into a real application.

---

## Feature 1 — Ask Elyra Anything (1:40–3:00)

The first major feature is “Ask Elyra Anything.”

This is powered by the Telerik AI Prompt component, with speech-to-text enabled.

So I can type something or speak something like:
“show failed transactions above £3k”

And what happens is:

1. The prompt goes through a deterministic query parser  
2. That gets executed against the dataset  
3. And then the grid updates to reflect that result  

So instead of just generating text…

…it’s actually driving application state.

Under the hood, this flows through a parser, executor, and formatter — so the AI experience is grounded, not hallucinated.

---

## Feature 2 — Smart Grid (3:00–3:50)

At the center of everything is the Telerik Blazor Grid.

This is where most of the work happens.

It:
- reflects AI-filtered results
- shows transaction risk visually
- surfaces flagged anomalies directly in the UI after anomaly detection runs
- includes Telerik Grid SmartBox AI assistant + semantic search in the toolbar
- supports CSV and Excel export for operational handoff

The grid becomes more than a table.

It becomes a decision surface.

---

## Feature 3 — AI Sidebar (3:50–5:00)

On the right side, we have the Elyra AI assistant.

This is where we shift from:
“what is happening”
to:
“why is it happening”

You can ask:
“why did failed payments increase today?”

Elyra will:
- run deterministic insight logic on the current dataset
- identify repeatable patterns like failure clusters and rail spikes
- return a grounded explanation tied to visible rows

There are also actions like:
- Summarize current view
- Detect anomalies

These are backed by an insights engine that evaluates:
- repeated failures
- rail spikes
- high-risk clusters

And feeds that back into the UI.

---

## Feature 4 — Live Data Simulation (5:00–6:00)

To make this feel real, I added simulated live updates.

Every few seconds:
- new transactions come in
- KPIs update
- AI context refreshes
- active query context is reapplied
- notable live events stream into the sidebar

So you’re interacting with a living system, not static data.

---

## Why This Matters (6:00–6:40)

What I like about this approach is:

AI isn’t a separate feature.

It’s woven into:
- the prompt
- the grid
- the insights layer

This moves us from:
“AI bolted onto a dashboard”

to:
“a system that actually understands your data”

And because we surface AI run telemetry in the sidebar — including engine, model, latency, and mapped intent — viewers can see exactly what executed on each prompt.

---

## Wrap-Up (6:40–7:10)

So that’s Elyra — built with:
- Telerik UI Generator
- Telerik Blazor components
- Telerik AI Prompt + Grid SmartBox AI experiences
- deterministic AI-driven interaction

If you’re building dashboards today, this is the direction things are heading.

Not just showing data…

…but helping people understand it.

---

## Optional Closing

And the best part is — this is all built on real components you can use today.

No magic. Just the right pieces, connected well.
