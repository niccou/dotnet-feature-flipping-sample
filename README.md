# Feature Flipping Sample – .NET 8

A complete **.NET 8** feature-flag management application demonstrating real-world feature-flipping patterns with Clean Architecture, CQRS, EF Core, Blazor Server, and a REST API.

---

## Solution Structure

```
FeatureFlipping.sln
├── src/
│   ├── FeatureFlipping.Domain          # Core domain model (aggregates, value objects, interfaces)
│   ├── FeatureFlipping.Application     # CQRS handlers, DTOs, abstractions (MediatR)
│   ├── FeatureFlipping.Infrastructure  # EF Core + SQLite, in-memory cache, evaluator
│   ├── FeatureFlipping.Api             # ASP.NET Core Minimal API + Swagger
│   └── FeatureFlipping.Blazor          # Blazor Server frontend
└── tests/
    └── FeatureFlipping.Tests           # xUnit + NSubstitute unit tests
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

---

## Quick Start

```bash
# 1 – Start the API (terminal 1)
cd src/FeatureFlipping.Api
dotnet run
# Swagger UI → http://localhost:5000/swagger

# 2 – Start the Blazor frontend (terminal 2)
cd src/FeatureFlipping.Blazor
dotnet run
# Dashboard → http://localhost:5001
```

On first start the API automatically:
- Creates the SQLite database (`featureflags.db`)
- Seeds three demo flags (see below)

---

## Seed Flags

| Key | IsEnabled | UserTargeting | RolloutPercentage | Purpose |
|-----|-----------|---------------|-------------------|---------|
| `dark-mode` | ✅ ON | – | 100% | Always enabled for everyone |
| `new-checkout-flow` | ❌ OFF | `user-42` | 0% | Globally off, but `user-42` can still access it |
| `ai-recommendations` | ✅ ON | – | 30% | Enabled for ~30% of users (deterministic hash) |

---

## Evaluation Rules (priority order)

The evaluator applies rules **top-to-bottom** and returns on the first match:

| Priority | Condition | Result | Reason |
|----------|-----------|--------|--------|
| 1 | Flag not found | `false` | `NotFound` |
| 2 | `userId` is in `UserTargeting` list | `true` | `UserTargeted` (**even if the flag is globally OFF**) |
| 3 | `IsEnabled = false` | `false` | `Disabled` |
| 4 | `RolloutPercentage` is 1–99 and `userId` maps into the bucket | `true` | `RolledOut` |
| 5 | `RolloutPercentage` is 1–99 and `userId` falls outside the bucket | `false` | `Disabled` |
| 6 | All other cases (fully enabled, no targeting, no partial rollout) | `true` | `Enabled` |

> **Key design decision – rule 2:** User targeting overrides the global kill-switch.  
> This lets you grant early-access to specific testers (`user-42`) while the flag stays globally off for everyone else.

---

## Demo: User Targeting (`new-checkout-flow`)

`new-checkout-flow` is **globally disabled** (`IsEnabled = false`) but `user-42` is explicitly in its `UserTargeting` list.

| UserId | Expected result | Reason |
|--------|----------------|--------|
| `user-42` | ✅ ENABLED | `UserTargeted` |
| `user-1` | ❌ DISABLED | `Disabled` |
| `user-99` | ❌ DISABLED | `Disabled` |
| *(empty)* | ❌ DISABLED | `Disabled` |

**How to test:**
1. In the Blazor dashboard, go to **Evaluate Flag**
2. Select `new-checkout-flow`
3. Enter `user-42` → result: **ENABLED / UserTargeted**
4. Enter anything else (e.g. `user-1`) → result: **DISABLED / Disabled**

Or via the API:
```
GET /api/flags/new-checkout-flow/evaluate?userId=user-42
GET /api/flags/new-checkout-flow/evaluate?userId=user-1
```

---

## Demo: Rollout Percentage (`ai-recommendations`)

`ai-recommendations` is enabled with `RolloutPercentage = 30`.  
A user is included if `SHA256(userId + flagKey) mod 100 < 30`.  
The hash is **deterministic** – the same `userId` always yields the same result.

Pre-computed results for demo user IDs:

| UserId | Hash | In rollout? |
|--------|------|-------------|
| `user-2` | 19 | ✅ ENABLED (RolledOut) |
| `user-6` | 19 | ✅ ENABLED (RolledOut) |
| `user-9` | 15 | ✅ ENABLED (RolledOut) |
| `charlie` | 15 | ✅ ENABLED (RolledOut) |
| `dave` | 21 | ✅ ENABLED (RolledOut) |
| `user-1` | 73 | ❌ DISABLED |
| `user-3` | 39 | ❌ DISABLED |
| `user-99` | 52 | ❌ DISABLED |
| `alice` | 59 | ❌ DISABLED |
| `bob` | 36 | ❌ DISABLED |

**How to demo gradual rollout:**
1. In the Blazor dashboard, select `ai-recommendations` in **Evaluate Flag**
2. Try `user-2` → **ENABLED / RolledOut**
3. Try `user-1` → **DISABLED**
4. Try `user-9` → **ENABLED / RolledOut**
5. Increase `RolloutPercentage` from 30 to 70 via the Edit row → more users will now be included

> Notice that `user-2` and `user-6` always get the same result (hash 19) — this is the determinism guarantee: a user always sees a consistent experience.

---

## Demo: Cache Behavior

Every flag evaluation is served from an in-memory cache (30-second TTL).  
The cache is **immediately invalidated** on any write (toggle, update).

| Badge | Meaning |
|-------|---------|
| 🟡 `CACHED` | Result came from cache |
| 🔵 `FRESH` | Cache was empty; result loaded from DB |

**How to observe:**
1. Evaluate any flag → cache badge turns **CACHED**
2. Toggle the flag → cache is invalidated → badge turns **FRESH**
3. Wait 30 s without changes → cache expires → badge turns **FRESH**

---

## API Reference

Base URL: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/flags` | List all flags |
| `GET` | `/api/flags/{key}` | Get a flag by key |
| `POST` | `/api/flags` | Create a flag |
| `PUT` | `/api/flags/{key}` | Update a flag (invalidates cache) |
| `PATCH` | `/api/flags/{key}/toggle` | Toggle IsEnabled (invalidates cache) |
| `GET` | `/api/flags/{key}/evaluate?userId=…` | Evaluate a flag for a user |
| `GET` | `/api/flags/{key}/evaluate/live?userId=…` | SSE stream – evaluation every 2 s |
| `GET` | `/api/flags/{key}/cache-status` | Returns `CACHED` or `FRESH` |

---

## Build & Test

```bash
dotnet build      # → 0 errors, 0 warnings
dotnet test       # → 13/13 tests passing
```

A GitHub Actions CI workflow (`.github/workflows/ci.yml`) runs on every push and pull request:
- Restore → Build (Release) → Test on .NET 8
