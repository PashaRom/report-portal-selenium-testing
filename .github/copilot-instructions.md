# Copilot Instructions

## Project Overview

This is a **.NET 8 Selenium UI test framework** targeting a [ReportPortal](https://reportportal.io/) instance. The solution lives entirely under `SeleniumFrameworkInteraction/` and consists of three projects:

- **Core** — framework infrastructure (driver management, base classes, custom elements, config, logging)
- **Business** — page objects, components, steps, models, data, and API clients
- **UITests** — NUnit test fixtures and test data

## Build & Test Commands

All commands run from `SeleniumFrameworkInteraction/`.

```bash
# Build
dotnet build SeleniumFrameworkInteraction.sln

# Run all tests
dotnet test SeleniumFrameworkInteraction.sln

# Run tests in a specific category (NUnit category filter)
dotnet test SeleniumFrameworkInteraction.sln --filter "TestCategory=dashboard_crud"

# Run a single test class
dotnet test SeleniumFrameworkInteraction.sln --filter "FullyQualifiedName~DashboardCRUDTests"

# Run a single test method
dotnet test SeleniumFrameworkInteraction.sln --filter "FullyQualifiedName~CreateDashboardWithWidget_ThenDelete"
```

## Architecture

### Layer flow

```
UITests (test fixtures)
  └── Business.Steps (orchestration, one Steps class per feature)
        ├── Business.Pages (full-page Page Objects)
        └── Business.Components (scoped UI components / modal dialogs)
              └── Core.Elements (typed element wrappers: Button, Input, Text, Link, Radio)
```

Tests **never** talk to pages directly — they go through `Steps` classes.

### Base class hierarchy

- `BaseApplication` — provides `Driver`, `Wait` (explicit), implicit wait config, and abstract `FindElement`/`FindElements`/`IsElementDisplayed`
  - `BasePage` (`waitTimeoutSeconds = 20`) — scopes finds to `Driver`; adds `NavigateTo`, `WaitUntilClickable`
  - `BaseComponent` (`waitTimeoutSeconds = 10`) — scopes all finds to an abstract `Root` property (the component's root `IWebElement`)
- `BaseTest` — NUnit `[TestFixture]` that creates/quits `WebDriver` per test via `[SetUp]`/`[TearDown]`

### Custom element wrappers (`Core/Elements/`)

Prefer typed element properties over raw `FindElement` calls in pages and components:

```csharp
// Declare as computed property in a Page/Component:
public Input NameField => new(By.CssSelector("input[placeholder='…']"), "Name Field");
public Button AddBtn   => new(By.XPath(".//button[.='Add']"), "Add Button");

// Use:
NameField.SetValue("John");  // built-in wait + logging
AddBtn.Click();              // built-in wait + logging
```

Available types: `Button`, `Input`, `Text`, `Link`, `Radio`. All log every action automatically.

### Configuration

Configuration is loaded from `appsettings.json` (linked from `../../../report-portal-testing/global/appsettings.json` — a sibling repo). Key settings:

| Key | Default | Purpose |
|-----|---------|---------|
| `BaseUrl` | `http://localhost:8080/` | ReportPortal instance URL |
| `ProjectName` | `superadmin_personal` | RP project used in tests |
| `UserPassword` | `1q2w3e` | Password for all test users |
| `UsersDataFile` | `RP_USERS_CSV_Report.csv` | CSV with test users |
| `WidgetTypesFile` | `widget_types.en.json` | Widget name locale template |
| `DriverSettings.Browser` | `Chrome` | `Chrome`, `Firefox`, `Edge`, `Remote` |
| `DriverSettings.Headless` | `false` | Headless mode |
| `DriverSettings.RemoteUri` | — | Required when `Browser = Remote` |

### Test data

- Users are loaded from a CSV file (`UITests/Data/CSV/`) via `CsvReader` into `TestDataProvider`.
- `TestDataProvider` exposes `IEnumerable<object[]>` properties for use with `[TestCaseSource]`.
- Widget display names come from a JSON template in `UITests/Data/Templates/` (locale-switchable via `WidgetTypesFile` config key).

### Parallelism & setup

- `ParallelConfig.cs` sets `[Parallelizable(ParallelScope.Fixtures)]` with `LevelOfParallelism(1)` at assembly level (per-fixture parallelism, one at a time by default).
- `GlobalSetup.cs` (`[SetUpFixture]`) runs before/after all tests to clean up leftover test dashboards via the ReportPortal REST API.

## Key Conventions

### Directory & namespace mapping

| Location | Namespace |
|----------|-----------|
| `Core/Enum/` | `Core.Enum` |
| `Core/Helpers/` | `Core.Helpers` |
| `Core/Logging/` | `Core.Logging` |
| `Core/Base/` | `Core.Base` |
| `Business/Pages/` | `Business.Pages` — one class per page |
| `Business/Components/` | `Business.Components` — modals and reusable UI sections |
| `Business/Steps/` | `Business.Steps` — orchestration consumed by tests |
| `Business/Models/` | `Business.Models` — plain POCOs |
| `Business/Data/` | `Business.Data` — static test data providers |
| `UITests/Tests/<Feature>/` | `UITests.Tests.<Feature>` |

### Adding new tests

1. Create a `Steps` class in `Business/Steps/` if the feature doesn't have one.
2. Create Page/Component classes in `Business/Pages/` or `Business/Components/`.
3. Create a test fixture in `UITests/Tests/<Feature>/` inheriting `BaseTest`.
4. Decorate with `[Category("…")]`, `[AllureFeature("…")]`, `[AllureSuite("…")]`.
5. Use `[TestCaseSource(typeof(TestDataProvider), nameof(…))]` for data-driven tests.

### Components

- Every `BaseComponent` subclass **must** override the abstract `Root` property to return the component's container element.
- Components scope `FindElement`/`FindElements` to `Root` automatically — no need to prefix locators.

### Test dashboard cleanup

Test dashboards are identified by name prefix `DC_` or suffix ` CRUD Dashboard`. `GlobalSetup` deletes them via the RP API before and after the run.

### Allure reporting

Results go to `allure-results/` (configured in `UITests/allureConfig.json`). Tests use `[AllureFeature]`, `[AllureSuite]`, and `[Description]` attributes.
