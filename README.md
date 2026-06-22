# ReportPortal Selenium Test Framework

A .NET 8 Selenium UI test framework targeting a [ReportPortal](https://reportportal.io/) instance. Built on top of NUnit and Allure, following the Page Object Model pattern with a layered architecture.

---

## Table of Contents

- [Solution Structure](#solution-structure)
- [Architecture](#architecture)
- [Core Layer](#core-layer)
  - [Base Classes](#base-classes)
  - [Custom Element Wrappers](#custom-element-wrappers)
  - [WebDriver Management](#webdriver-management)
  - [WaitHelper](#waithelper)
  - [Dependency Injection](#dependency-injection)
- [Business Layer](#business-layer)
  - [Pages](#pages)
  - [Components](#components)
  - [Steps](#steps)
  - [Models & Data](#models--data)
- [UITests Layer](#uitests-layer)
  - [Test Fixtures](#test-fixtures)
  - [Test Data](#test-data)
  - [Global Setup & Cleanup](#global-setup--cleanup)
  - [Parallelism](#parallelism)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Allure Reporting](#allure-reporting)
- [Adding New Tests](#adding-new-tests)

---

## Solution Structure

```
SeleniumFrameworkInteraction/
├── Core/                          # Framework infrastructure
│   ├── Base/                      # BaseApplication, BasePage, BaseComponent, BaseTest
│   ├── Clients/                   # IRpApiClient, RpApiClient (HTTP client for ReportPortal)
│   ├── Configuration/             # AppConfiguration, DriverSettings, IAppConfiguration
│   ├── DI/                        # ServiceLocator (IoC composition root)
│   ├── Drivers/                   # WebDriverFactory, DriverManager, DriverContext
│   ├── Elements/                  # Typed element wrappers (Button, Text, Label, Link, Radio)
│   ├── Enum/                      # BrowserType and other enums
│   ├── Helpers/                   # WaitHelper, ActionHelper, CsvReader, JsonReader
│   ├── Logging/                   # FileLogger, FileLoggerProvider
│   └── Structures/                # Timeouts (shared TimeSpan constants)
├── Business/                      # Application-specific layer
│   ├── Clients/                   # IAuthClient, IDashboardApiClient and implementations
│   ├── Components/                # Modal dialogs and reusable UI sections
│   ├── Data/                      # TestDataProvider, WidgetTypesProvider
│   ├── DI/                        # BusinessServiceExtensions (registers Business services)
│   ├── Helpers/                   # DashboardCleanupApiService
│   ├── Model/API/                 # API response POCOs (TokenResponse, DashboardListResponse, …)
│   ├── Models/                    # UI-facing POCOs (UserModel, …)
│   ├── Pages/                     # Full-page Page Objects
│   └── Steps/                     # Orchestration classes consumed by tests
└── UITests/                       # NUnit test fixtures
    ├── Tests/
    │   └── Dashboard/             # Dashboard feature tests
    ├── Data/
    │   ├── CSV/                   # User data exported from ReportPortal
    │   └── Templates/             # Widget name locale files (widget_types.en.json, …)
    ├── GlobalSetup.cs             # One-time setup / teardown (registers DI + cleans test dashboards)
    ├── ParallelConfig.cs          # Assembly-level parallelism attributes
    └── allureConfig.json          # Allure results output path
```

---

## Architecture

The framework uses a strict 3-layer architecture. Tests never interact with pages directly — all orchestration goes through **Steps** classes.

```
UITests (NUnit fixtures)
  └── Business.Steps      ← orchestration; one Steps class per feature
        ├── Business.Pages       ← full-page Page Objects
        └── Business.Components  ← scoped modal dialogs / UI sections
              └── Core.Elements  ← typed element wrappers (Button, Input, …)
```

**Layer flow — example for a dashboard deletion:**

```
DashboardCRUDTests
  → DashboardSteps.DeleteDashboard()
      → DashboardPage.DeleteDashboard()
          → DashboardPage.DeleteBtn.ClickWithActions()     // opens confirm dialog
          → DeleteDashboardDialog.ClickDelete()
              → ActionHelper.JsClick(element)             // JS click bypasses scroll overlay
```

---

## Core Layer

### Base Classes

All Page Objects and Components inherit from `BaseApplication`, which provides:

| Member | Description |
|--------|-------------|
| `Driver` | Current thread's `IWebDriver` via `DriverContext.Current` |
| `Configuration` | `IAppConfiguration` instance from DI |
| `Logger` | `ILogger` scoped to the concrete class name |
| `ExplicitWaitTimeoutSeconds` | Value from `appsettings.json` |

**`BasePage`** (extends `BaseApplication`):
- Constructor accepts `string name` — used in all log messages as `[{Page}]` prefix.
- Adds `NavigateAndWaitForReady(url)` — navigates and waits for `document.readyState == "complete"`.

**`BaseComponent`** (extends `BaseApplication`):
- Constructor accepts `string name` and `By rootLocator`.
- Exposes a `Root` property (`IWebElement`) that is found each call via the provided locator — no stale-element caching.
- All element properties inside a component are scoped to `Root` via the `ISearchContext` constructor overload of element wrappers.

**`BaseTest`** (NUnit `[TestFixture]` base):
- `[SetUp] InitDriver()` — creates a new `IWebDriver` via `WebDriverFactory` and stores it in `DriverContext` (thread-local).
- `[TearDown] QuitDriver()` — quits and disposes the driver after each test.
- `ImplicitWait` is set to `TimeSpan.Zero` in `WebDriverFactory.Create()` — all waits are explicit.

---

### Custom Element Wrappers

Located in `Core/Elements/`. Each wrapper encapsulates a locator or a pre-found `IWebElement`, adds automatic logging, and provides safe property access.

| Type | Description |
|------|-------------|
| `Button` | Clickable button; `Click()` waits until displayed + enabled; `ClickWithActions()` uses Selenium Actions for hover-then-click |
| `Text` | Text input field; `SetValue(text)` clears and types |
| `Label` | Read-only text element; `Value` returns trimmed text |
| `Link` | Anchor element; `Click()` waits until clickable |
| `Radio` | Radio button; `Select()` checks the element |

All wrappers support three constructor overloads:

```csharp
// 1. Locator only (driver-root search)
new Button(By.CssSelector("button[type='submit']"), "Submit Button");

// 2. Locator + ISearchContext (scoped to a component Root)
new Button(By.XPath(".//button[.='Add']"), "Add Button", Root);

// 3. Pre-found element (no re-find on stale)
new Button(existingElement, "Pre-found Button");
```

**Usage pattern (declare as computed property):**

```csharp
// In a Page:
private Button SubmitBtn => new(By.CssSelector("button[type='submit']"), "Submit Button");
private Text   NameField => new(By.XPath("//input[@placeholder='Name']"), "Name Field");

// In a Component (scoped to Root):
private Button AddBtn => new(By.XPath(".//button[.='Add']"), "Add Button", Root);

// Usage:
NameField.SetValue("My Dashboard");
SubmitBtn.Click();
```

Every action is logged automatically:
```
[Button] Submit Button: Clicking on Button element Submit Button
[Button] Submit Button: Clicked on Button element Submit Button successfully
[Text]   Name Field: Setting value "My Dashboard" on Text element Name Field
```

`WrapperElement` base properties:
- `IsDisplayed` — returns `false` on `StaleElementReferenceException` / `NoSuchElementException` instead of throwing.
- `IsEnabled` — same safe guard.

---

### WebDriver Management

`WebDriverFactory` creates browser instances based on `DriverSettings`:

| Setting | Values | Notes |
|---------|--------|-------|
| `Browser` | `Chrome`, `Firefox`, `Edge` | Default: `Chrome` |
| `Remote` | `true` / `false` | When `true`, `RemoteUri` is required |
| `RemoteUri` | URL string | Selenium Grid / cloud endpoint |
| `Headless` | `true` / `false` | Default: `false` |
| `WindowWidth` | int | Default: `1920` |
| `WindowHeight` | int | Default: `1080` |

Chrome is configured to suppress password manager prompts, save-password bubbles, and autofill to prevent UI interference during tests.

`DriverContext` stores the driver in a thread-local slot (`ThreadLocal<IWebDriver?>`), making parallel test execution safe. `IDriverManager.Set(driver)` writes to this slot in `BaseTest.[SetUp]`; `DriverContext.Current` reads from it in pages and components.

`ImplicitWait` is set to `TimeSpan.Zero` once in `WebDriverFactory.Create()` — the framework relies exclusively on explicit waits via `WaitHelper`.

---

### WaitHelper

`WaitHelper` is a static utility wrapping Selenium's `WebDriverWait`. All timeout parameters use `TimeSpan`.

```csharp
// Wait for a custom condition (default 10 s):
WaitHelper.Until(d => d.FindElements(By.CssSelector(".spinner")).Count == 0);

// Wait for an element wrapper to be displayed and enabled (default 5 s):
var webElement = WaitHelper.DefaultWait(myButton);

// With explicit timeout:
WaitHelper.Until(d => d.Url.Contains("/dashboard/"), timeout: Timeouts.Sec15);

// With custom polling interval:
WaitHelper.Until(condition, timeout: Timeouts.Sec10, polling: Timeouts.Ms300);
```

`WaitHelper.DefaultWait` handles:
- **Stale element recovery** — re-finds the element via its `SearchContext` + `Locator` on `StaleElementReferenceException`.
- **Pre-found elements** — when `Locator` is `null`, validates the existing element directly without re-finding.
- **Scoped search** — uses `element.SearchContext` (e.g. component `Root`) instead of driver root when present.

Default ignored exceptions: `NoSuchElementException`, `StaleElementReferenceException`, `ElementNotInteractableException`, `TimeoutException`.

`Timeouts` constants (`Core/Structures/Timeouts.cs`):

```csharp
Timeouts.Ms300  // 300 ms
Timeouts.Ms500  // 500 ms
Timeouts.Sec1   // 1 s
Timeouts.Sec2   // 2 s
Timeouts.Sec3   // 3 s
Timeouts.Sec5   // 5 s
Timeouts.Sec10  // 10 s
Timeouts.Sec20  // 20 s
Timeouts.Sec30  // 30 s
```

---

### ActionHelper

`ActionHelper` (`Core/Helpers/ActionHelper.cs`) is a static utility for interactions that require more than a simple element click.

**Mouse / Actions:**

| Method | Description |
|--------|-------------|
| `MoveToElementAndClick(element, name)` | Moves cursor to element then clicks via `Selenium.Interactions.Actions`. Use when standard click is intercepted or element needs hover |
| `DragAndDrop(source, name, target, name)` | Drags source element and drops on target |
| `DragAndDropByOffset(source, name, offsetX, offsetY)` | Drags source element by pixel offset |

**JavaScript:**

| Method | Description |
|--------|-------------|
| `JsClick(element, name)` | `arguments[0].click()` via JS — bypasses z-index / overlay interception |
| `JsClearBrowserStorage()` | Clears `localStorage` and `sessionStorage` |
| `JsFindScrollableContainer()` | Finds the largest scrollable container on the page |
| `JsScrollToTop(container)` | Sets `scrollTop = 0` on a container |
| `JsGetScrollTop(container)` | Returns `scrollTop` value |
| `JsScrollBy(container, pixels)` | Increments `scrollTop` by given pixels |

> **Tip:** Use `JsClick` for buttons inside modal dialogs that have a custom scrollbar overlay (e.g. `position: absolute; inset: 0; overflow: scroll`). Standard clicks and `Actions.Click()` are both blocked by overlay elements — only a JS click fires directly on the target DOM node.

---

### Dependency Injection

`ServiceLocator` is a thread-safe composition root backed by `Microsoft.Extensions.DependencyInjection`. All framework objects are resolved through the container — no `new` calls in test code.

**Registered services:**

| Layer | Service | Lifetime |
|-------|---------|----------|
| Core | `IAppConfiguration` | Singleton |
| Core | `IWebDriverFactory` | Singleton |
| Core | `IDriverManager` | Singleton |
| Core | `IRpApiClient` | Singleton |
| Business | `IAuthClient` | Singleton |
| Business | `IDashboardApiClient` | Singleton |
| Business | `DashboardCleanupApiService` | Singleton |
| Business | `AddDashboardDialog`, `AddWidgetDialog`, `DeleteDashboardDialog` | Transient |
| Business | `LoginPage`, `DashboardListPage`, `DashboardPage` | Transient |
| Business | `AuthSteps`, `DashboardSteps` | Transient |

**Registration flow:**

Core services are registered inside `ServiceLocator.BuildProvider()`. Business services are added via `BusinessServiceExtensions.AddBusinessServices()`, called from `GlobalSetup.OneTimeSetUp` before any `GetService<>()` call:

```csharp
// GlobalSetup.cs
[OneTimeSetUp]
public void PreCleanupTestDashboards()
{
    ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices());
    // ... rest of setup
}
```

**Resolving services in tests:**

```csharp
[SetUp]
public void InitSteps()
{
    _auth      = ServiceLocator.GetService<AuthSteps>();
    _dashboard = ServiceLocator.GetService<DashboardSteps>();
}
```

Steps, Pages, and Components all use **constructor injection** — dependencies are injected automatically by the container:

```csharp
// Steps receive Pages:
public class AuthSteps(LoginPage loginPage) { … }
public class DashboardSteps(DashboardListPage listPage, DashboardPage dashboardPage) { … }

// Pages receive Components:
public class DashboardListPage(AddDashboardDialog dialog) : BasePage("Dashboard List Page") { … }
public class DashboardPage(AddWidgetDialog addWidget, DeleteDashboardDialog deleteDialog) : BasePage("Dashboard Page") { … }
```

---

## Business Layer

### Pages

Located in `Business/Pages/`. One class per screen/page.

| Class | Responsibility |
|-------|---------------|
| `LoginPage` | Fills credentials and submits the login form |
| `DashboardListPage` | Lists dashboards; opens Add Dashboard dialog; checks dashboard presence |
| `DashboardPage` | Adds / deletes widgets; deletes a dashboard; lock / unlock |

### Components

Located in `Business/Components/`. Each extends `BaseComponent`. The component name and root locator are passed to the base constructor — no need to override `Root`:

```csharp
public class AddDashboardDialog : BaseComponent
{
    public AddDashboardDialog() : base(
        "Add Dashboard Dialog",
        By.CssSelector("#modal-root [class*='modalLayout__modal-window']")) { }

    // All elements are scoped to Root automatically:
    private Button AddBtn => new(By.XPath(".//button[.='Add']"), "Add Button", Root);
}
```

| Class | Root selector | Responsibility |
|-------|--------------|----------------|
| `AddDashboardDialog` | `#modal-root [class*='modalLayout__modal-window']` | Create dashboard form |
| `DeleteDashboardDialog` | `#modal-root` | Confirm dashboard deletion (uses `JsClick` to bypass scroll overlay) |
| `AddWidgetDialog` | *(dialog root)* | Multi-step widget wizard |

**Component rule:** Declare element properties with a relative locator (`.//button[.='Delete']`) and pass `Root` as the third constructor argument. This scopes `FindElement` to the component's DOM subtree.

### Steps

Located in `Business/Steps/`. One class per feature, consumed exclusively by test fixtures. Steps receive pages via constructor injection and expose a high-level API.

| Class | Responsibility |
|-------|---------------|
| `AuthSteps` | `LoginAs(alias)` — resolves user from `TestDataProvider`, logs in |
| `DashboardSteps` | Full dashboard lifecycle: create, add widget, delete, lock/unlock |

Steps hold page / component instances and coordinate multi-page flows. Tests only call Steps methods — never pages directly.

### Models & Data

- **`Business/Models/`** — POCOs like `UserModel` (login, password, display name).
- **`Business/Data/`** — `TestDataProvider` exposes `IEnumerable<object[]>` properties for `[TestCaseSource]`:
  - `DashboardCrudCases` — (login, dashboardName, widgetName) tuples
  - `LoginAliases` — all known user aliases
  - `GetUser(alias)` — resolves a `UserModel` by alias

User data is loaded from a CSV file via `CsvReader` at first access (lazy, cached).

---

## UITests Layer

### Test Fixtures

Located in `UITests/Tests/<Feature>/`. All fixtures inherit `BaseTest`.

```csharp
[TestFixture]
[Category("dashboard_crud")]
[AllureFeature("Dashboard")]
[AllureSuite("CRUD")]
public class DashboardCRUDTests : BaseTest
{
    private AuthSteps _auth = null!;
    private DashboardSteps _dashboard = null!;

    [SetUp]
    public void InitSteps()
    {
        _auth      = ServiceLocator.GetService<AuthSteps>();
        _dashboard = ServiceLocator.GetService<DashboardSteps>();
    }

    [TestCaseSource(typeof(TestDataProvider), nameof(TestDataProvider.DashboardCrudCases))]
    [Description("User creates a named dashboard with a widget, then deletes it")]
    public void CreateDashboardWithWidget_ThenDelete(string login, string dashboardName, string widgetName)
    {
        _auth.LoginAs(login);
        _dashboard.CreateDashboardWithName(dashboardName);
        _dashboard.AddWidget("Launch statistics chart", widgetName);

        Assert.That(_dashboard.IsWidgetVisible(widgetName), Is.True);

        _dashboard.DeleteDashboard();

        Assert.That(_dashboard.IsDashboardInList(dashboardName), Is.False);
    }
}
```

Current test suites:

| Class | Category | Description |
|-------|----------|-------------|
| `DashboardCRUDTests` | `dashboard_crud` | Create dashboard with widget, then delete |
| `DashboardCreateWithWidgetTests` | `dashboard_widget` | Create dashboard and verify widget list |
| `DashboardAllWidgetsTests` | `dashboard_all_widgets` | Add all widget types and verify visibility |
| `DashboardAddDialogTests` | `dashboard_add_dialog` | Add Dashboard dialog validation (name errors, cancel) |
| `DashboardLockTests` | `dashboard_lock` | Lock / Unlock dashboard availability |

### Test Data

**CSV users** (`UITests/Data/CSV/`):
- File name configured via `UsersDataFile` key in `appsettings.json` (default: `RP_USERS_CSV_Report.csv`).
- Columns: `Login`, `Password`, `FullName`, `Role`, etc.

**Widget types** (`UITests/Data/Templates/`):
- JSON file with display names for each widget type in a given locale.
- File selected via `WidgetTypesFile` key (default: `widget_types.en.json`).
- Add `widget_types.<lang>.json` to support another locale.

### Global Setup & Cleanup

`GlobalSetup.cs` runs once before and after the entire test run using NUnit's `[SetUpFixture]`:

1. **Registers Business-layer DI services** via `ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices())` — this is the first action in `OneTimeSetUp`, before any `GetService<>()` call.
2. Connects to ReportPortal REST API using credentials from `appsettings.json`.
3. Deletes all test-owned dashboards for every known user.

Test dashboards are identified by name: prefix `DC_` or suffix ` CRUD Dashboard`. This prevents leftover data from previous failed runs from affecting new ones.

### Parallelism

`ParallelConfig.cs` sets assembly-level attributes:

```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]
```

Change `LevelOfParallelism` to match the number of available browser sessions / grid nodes. Each parallel fixture gets its own `WebDriver` instance via `DriverContext` (thread-local storage), so browser sessions never share state.

---

## Configuration

Configuration is loaded from `appsettings.json` (can be overridden by environment variables).

```json
{
  "BaseUrl": "http://localhost:8080/",
  "ProjectName": "superadmin_personal",
  "UserPassword": "1q2w3e",
  "UsersDataFile": "RP_USERS_CSV_Report.csv",
  "WidgetTypesFile": "widget_types.en.json",
  "ExplicitWaitTimeoutSeconds": 10,
  "DriverSettings": {
    "Browser": "Chrome",
    "Remote": false,
    "RemoteUri": "",
    "Headless": false,
    "WindowWidth": 1920,
    "WindowHeight": 1080
  },
  "LogSettings": {
    "MinimumLevel": "Information"
  }
}
```

| Key | Default | Description |
|-----|---------|-------------|
| `BaseUrl` | `http://localhost:8080/` | ReportPortal instance base URL |
| `ProjectName` | `superadmin_personal` | RP project used in tests |
| `UserPassword` | `1q2w3e` | Password for all test users |
| `UsersDataFile` | `RP_USERS_CSV_Report.csv` | CSV file with test user accounts |
| `WidgetTypesFile` | `widget_types.en.json` | Widget display-name locale template |
| `ExplicitWaitTimeoutSeconds` | `10` | Default timeout for explicit waits |
| `DriverSettings.Browser` | `Chrome` | `Chrome`, `Firefox`, or `Edge` |
| `DriverSettings.Remote` | `false` | Use Selenium Grid when `true` |
| `DriverSettings.RemoteUri` | — | Grid / cloud endpoint URL |
| `DriverSettings.Headless` | `false` | Run browser in headless mode |

All settings can be overridden at runtime via environment variables using the standard .NET `IConfiguration` naming convention (e.g., `DriverSettings__Browser=Firefox`).

---

## Running Tests

All commands run from the `SeleniumFrameworkInteraction/` directory.

### Build

```bash
dotnet build SeleniumFrameworkInteraction.sln
```

---

### Selecting tests to run

```bash
# All tests
dotnet test SeleniumFrameworkInteraction.sln

# By NUnit category
dotnet test SeleniumFrameworkInteraction.sln --filter "TestCategory=dashboard_crud"

# By test fixture name
dotnet test SeleniumFrameworkInteraction.sln --filter "FullyQualifiedName~DashboardCRUDTests"

# By individual test method
dotnet test SeleniumFrameworkInteraction.sln --filter "FullyQualifiedName~CreateDashboardWithWidget_ThenDelete"
```

Available categories:

| Category | Fixture |
|----------|---------|
| `dashboard_crud` | `DashboardCRUDTests` |
| `dashboard_widget` | `DashboardCreateWithWidgetTests` |
| `dashboard_all_widgets` | `DashboardAllWidgetsTests` |
| `dashboard_add_dialog` | `DashboardAddDialogTests` |
| `dashboard_lock` | `DashboardLockTests` |

---

### Choosing a browser

The browser is controlled by the `DriverSettings__Browser` environment variable.  
Supported values: `Chrome` (default), `Firefox`, `Edge`.

**Windows (PowerShell):**
```powershell
$env:DriverSettings__Browser = "Firefox"
dotnet test SeleniumFrameworkInteraction.sln
```

**Linux / macOS:**
```bash
DriverSettings__Browser=Firefox dotnet test SeleniumFrameworkInteraction.sln
DriverSettings__Browser=Edge    dotnet test SeleniumFrameworkInteraction.sln
```

---

### Headless mode

Headless mode is supported for Chrome, Firefox, and Edge.  
Use it in CI environments where no display is available.

**Windows (PowerShell):**
```powershell
$env:DriverSettings__Browser  = "Chrome"
$env:DriverSettings__Headless = "true"
dotnet test SeleniumFrameworkInteraction.sln
```

**Linux / macOS:**
```bash
DriverSettings__Headless=true dotnet test SeleniumFrameworkInteraction.sln
```

---

### Remote execution (Selenium Grid)

Set `DriverSettings__Remote=true` and provide the Grid endpoint via `DriverSettings__RemoteUri`.  
The `Browser` setting still controls which browser capability is requested.

**Windows (PowerShell):**
```powershell
$env:DriverSettings__Remote    = "true"
$env:DriverSettings__RemoteUri = "http://grid-host:4444/wd/hub"
$env:DriverSettings__Browser   = "Chrome"
dotnet test SeleniumFrameworkInteraction.sln
```

**Linux / macOS:**
```bash
DriverSettings__Remote=true \
DriverSettings__RemoteUri=http://grid-host:4444/wd/hub \
DriverSettings__Browser=Chrome \
dotnet test SeleniumFrameworkInteraction.sln
```

> **Tip:** When running remotely, increase `LevelOfParallelism` in `UITests/ParallelConfig.cs`  
> to match the number of available Grid nodes and pass `--workers N` to `dotnet test`.

---

### Parallel execution

Parallelism is configured at assembly level in `UITests/ParallelConfig.cs`:

```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]        // ← change to desired concurrency
```

To run 4 fixtures in parallel, set `LevelOfParallelism(4)` and optionally pass the worker count to `dotnet test`:

```bash
dotnet test SeleniumFrameworkInteraction.sln -- NUnit.NumberOfTestWorkers=4
```

---

### Window size

Default resolution is 1920×1080. Override per run:

**Windows (PowerShell):**
```powershell
$env:DriverSettings__WindowWidth  = "1280"
$env:DriverSettings__WindowHeight = "720"
dotnet test SeleniumFrameworkInteraction.sln
```

---

### Targeting a different ReportPortal instance

```powershell
$env:BaseUrl      = "http://my-rp-instance:8080/"
$env:ProjectName  = "my_project"
$env:UserPassword = "secret"
dotnet test SeleniumFrameworkInteraction.sln
```

---

### Combined example — CI pipeline run

```bash
BaseUrl=http://rp:8080/ \
ProjectName=superadmin_personal \
DriverSettings__Browser=Chrome \
DriverSettings__Headless=true \
DriverSettings__Remote=true \
DriverSettings__RemoteUri=http://grid:4444/wd/hub \
dotnet test SeleniumFrameworkInteraction.sln \
  --filter "TestCategory=dashboard_crud" \
  -- NUnit.NumberOfTestWorkers=4
```

---

## Allure Reporting

Results are written to `allure-results/` (configured in `UITests/allureConfig.json`).

Tests use the following Allure attributes:

| Attribute | Example |
|-----------|---------|
| `[AllureFeature]` | `"Dashboard"` |
| `[AllureSuite]` | `"CRUD"` |
| `[Description]` | Human-readable test description |

Generate and open the report (requires [Allure CLI](https://allurereport.org/docs/install/)):

```bash
allure serve allure-results
```

---

## Adding New Tests

1. **Steps** — create a `<Feature>Steps` class in `Business/Steps/` if none exists; add it to `BusinessServiceExtensions.AddBusinessServices()` as `services.AddTransient<MyFeatureSteps>()`.
2. **Pages** — add a `<Screen>Page` class in `Business/Pages/` extending `BasePage("Page Name")`; inject its components via constructor; register as `services.AddTransient<MyPage>()`.
3. **Components** — add a `<Name>Dialog` / `<Name>Panel` class in `Business/Components/` extending `BaseComponent("Component Name", By.CssSelector("…"))`; register as `services.AddTransient<MyComponent>()`.
4. **Test fixture** — create a class in `UITests/Tests/<Feature>/` extending `BaseTest`:
   - Add `[Category("…")]`, `[AllureFeature("…")]`, `[AllureSuite("…")]`.
   - Use `[TestCaseSource(typeof(TestDataProvider), nameof(…))]` for data-driven tests.
   - Resolve steps in `[SetUp]` via `ServiceLocator.GetService<MyFeatureSteps>()`.
   - Call only Steps methods from test bodies.

**Component checklist:**
- Pass `name` and `By rootLocator` to `base(…)` — no `Root` property override needed.
- Declare element properties as computed `private` properties returning typed wrappers with `Root` as the third argument.
- Use relative locators (`.//…`) for XPath or scoped CSS selectors inside components.
- For buttons inside modals that have custom scrollbar overlays, use `ActionHelper.JsClick(element, name)` instead of `Button.Click()`.
