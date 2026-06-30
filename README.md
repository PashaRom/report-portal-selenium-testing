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
│   ├── Base/                      # BaseApplication, BasePage, BaseComponent, BaseTest, BrowserDataSource
│   ├── Clients/                   # IRpApiClient, RpApiClient (HTTP client for ReportPortal)
│   ├── Configuration/             # AppConfiguration, DriverSettings, IAppConfiguration
│   ├── DI/                        # ServiceLocator (IoC composition root)
│   ├── Drivers/                   # WebDriverFactory, DriverManager, IDriverManager
│   ├── Elements/                  # Typed element wrappers (Button, Text, Label, Link, Radio)
│   ├── Enum/                      # BrowserType and other enums
│   ├── Helpers/                   # WaitHelper, ActionHelper, CsvReader, JsonReader
│   ├── Logging/                   # FileLogger, FileLoggerProvider
│   └── Structures/                # Timeouts (shared TimeSpan constants)
|   |__ Utils                      # ScreenshotUtil
├── Business/                      # Application-specific layer
│   ├── Clients/                   # IAuthClient, IDashboardApiClient, IUserApiClient and implementations
│   ├── Components/                # Modal dialogs and reusable UI sections
│   ├── Data/                      # TestDataProvider, WidgetTypesProvider
│   ├── DI/                        # BusinessServiceExtensions (registers Business services)
│   ├── Helpers/                   # DashboardCleanupApiService, UserProvisioningService
│   ├── Model/API/                 # API request/response POCOs (TokenResponse, UserListResponse, CreateUserRq, …)
│   ├── Models/                    # UI-facing POCOs (UserModel, …)
│   ├── Pages/                     # Full-page Page Objects
│   └── Steps/                     # Orchestration classes consumed by tests
└── UITests/                       # NUnit test fixtures
    ├── Tests/
    │   └── Dashboard/             # Dashboard feature tests
    ├── Data/
    │   ├── CSV/                   # User data exported from ReportPortal
    │   └── Templates/             # Widget name locale files (widget_types.en.json, …)
    ├── Hooks/
    │   └── GlobalSetup.cs         # One-time setup / teardown (registers DI + cleans test dashboards)
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
| `Driver` | Current thread's `IWebDriver` via `ServiceLocator.GetService<IDriverManager>().Current` |
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

**`BaseTest`** (NUnit cross-browser base):
- Decorated with `[TestFixtureSource(typeof(BrowserDataSource), nameof(BrowserDataSource.Browsers))]` — NUnit automatically creates one fixture instance per configured browser.
- Constructor accepts `BrowserType browser` — stored and passed to `WebDriverFactory.Create(browser)`.
- `[SetUp] InitDriver()` — creates a new `IWebDriver` for the fixture's browser via `WebDriverFactory` and stores it in `IDriverManager` (thread-local).
- `[TearDown] QuitDriver()` — quits and disposes the driver after each test.
- `ImplicitWait` is set to `TimeSpan.Zero` in `WebDriverFactory.Create()` — all waits are explicit.

---

### Custom Element Wrappers

Located in `Core/Elements/`. Each wrapper encapsulates a locator or a pre-found `IWebElement`, adds automatic logging, and provides safe property access.

| Type | Description |
|------|-------------|
| `Button` | Clickable button; `Click()` waits until displayed + enabled; `ClickWithActions()` uses Selenium Actions for hover-then-click |
| `Text` | Text input field; `SetValue(text)` clears and types (waits up to 5 s for element); `Click()` also waits up to 5 s |
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
- `Element` — every access calls `ScreenshotUtil.TrackElement(el)`, recording the last-touched element for failure screenshots.

---

### WebDriver Management

`WebDriverFactory` creates browser instances based on `DriverSettings`:

| Setting | Values | Notes |
|---------|--------|-------|
| `Browsers` | `["Chrome","Firefox","Edge"]` | List of browsers for cross-browser runs. Default: `["Chrome"]` |
| `Remote` | `true` / `false` | When `true`, `RemoteUri` is required |
| `RemoteUri` | URL string | Selenium Grid / cloud endpoint |
| `Headless` | `true` / `false` | Default: `false` |
| `WindowWidth` | int | Default: `1920` |
| `WindowHeight` | int | Default: `1080` |

`IWebDriverFactory.Create(BrowserType browser)` accepts the target browser explicitly, allowing `BaseTest` to create the correct driver instance for each fixture.

Chrome is configured to suppress password manager prompts, save-password bubbles, and autofill to prevent UI interference during tests.

`IDriverManager` stores the driver in a thread-local slot (`ThreadLocal<IWebDriver?>`), making parallel test execution safe. `IDriverManager.Set(driver)` writes to this slot in `BaseTest.[SetUp]`; `IDriverManager.Current` is resolved via `ServiceLocator` in pages, components, and helpers — no static `DriverContext` class is needed.

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
| `DragAndDropByOffset(source, sourceName, offset, movement)` | Drags the source element and drops it at the given pixel offset in the specified direction (Movement) |
| `Resize(handle, sourceName, offsetX, offsetY)` | Resize element at the given pixel offset from its current size |
| `ScrollToElementTop(element, elementName)` | Scrolls the page until the given element is at the top of the viewport (or as close as possible) |

**JavaScript:**

| Method | Description |
|--------|-------------|
| `JsClick(element, name)` | `arguments[0].click()` via JS — bypasses z-index / overlay interception |
| `JsClearBrowserStorage()` | Clears `localStorage` and `sessionStorage` |
| `JsSetLocalStorageItem(key, value)` | Sets a single `localStorage` item for the current origin |
| `JsFindScrollableContainer()` | Finds the largest scrollable container on the page |
| `JsScrollToTop(container)` | Sets `scrollTop = 0` on a container |
| `JsGetScrollTop(container)` | Returns `scrollTop` value |
| `JsScrollBy(container, pixels)` | Increments `scrollTop` by given pixels |

> **Tip:** Use `JsClick` for buttons inside modal dialogs that have a custom scrollbar overlay (e.g. `position: absolute; inset: 0; overflow: scroll`). Standard clicks and `Actions.Click()` are both blocked by overlay elements — only a JS click fires directly on the target DOM node.

### ScreenshotUtil

`ScreenshotUtil` (`Core/Utils/`) handles failure screenshots with element highlighting.

- **`TrackElement(IWebElement)`** — called automatically by `WrapperElement.Element` on every element access; stores the reference in a `[ThreadStatic]` field (parallel-safe).
- **`TryAttachScreenshot()`** — called by `BaseTest.[TearDown]` on test failure:
  1. Highlights the last-tracked element with a red outline (`outline: 4px solid red`) via JavaScript.
  2. Takes a screenshot.
  3. Restores the original element style.
  4. Attaches the screenshot to the Allure report as `"Screenshot on Failure"`.

The red outline pinpoints exactly which element interaction preceded the failure, making root-cause analysis faster without inspecting logs.

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
| Business | `IUserApiClient` | Singleton |
| Business | `DashboardCleanupApiService` | Singleton |
| Business | `UserProvisioningService` | Singleton |
| Business | `AddDashboardDialog`, `AddWidgetDialog`, `DeleteDashboardDialog`, `SystemAlertDialog` | Transient |
| Business | `LoginPage`, `DashboardListPage`, `DashboardPage` | Transient |
| Business | `AuthSteps`, `DashboardSteps` | Transient |

**Registration flow:**

Core services are registered inside `ServiceLocator.BuildProvider()`. Business services are added via `BusinessServiceExtensions.AddBusinessServices()`, called from `GlobalSetup.OneTimeSetUp` before any `GetService<>()` call:

```csharp
// GlobalSetup.cs
[OneTimeSetUp]
public void OneTimeSetUp()
{
    ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices());
    // ... provisioning + cleanup
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
public class DashboardSteps(DashboardListPage listPage, DashboardPage dashboardPage, SystemAlertDialog systemAlertDialog) { … }

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
| `AddWidgetDialog` | *(dialog root)* | Multi-step widget wizard; handles filter, config, and name steps automatically |
| `SystemAlertDialog` | *(alert root)* | System-level alert that may appear after widget operations; closed automatically by `DashboardSteps.AddWidget` |

**Component rule:** Declare element properties with a relative locator (`.//button[.='Delete']`) and pass `Root` as the third constructor argument. This scopes `FindElement` to the component's DOM subtree.

### Steps

Located in `Business/Steps/`. One class per feature, consumed exclusively by test fixtures. Steps receive pages via constructor injection and expose a high-level API.

| Class | Responsibility |
|-------|---------------|
| `AuthSteps` | `LoginAs(alias)` — resolves user from `TestDataProvider`, logs in via UI; `LoginViaApi(alias)` — injects token into `localStorage` and refreshes (faster, used for test setup) |
| `DashboardSteps` | Full dashboard lifecycle: create (waits up to 20 s for URL redirect), add widget (auto-closes `SystemAlertDialog`), delete, lock/unlock |

Steps hold page / component instances and coordinate multi-page flows. Tests only call Steps methods — never pages directly.

### API Login (`AuthSteps.LoginViaApi`)

`LoginViaApi` bypasses the UI login form for speed. It:

1. Calls `GET /uat/sso/oauth/token` with the user's credentials to obtain a JWT.
2. Navigates to `BaseUrl + "ui/"` to establish the correct localStorage origin.
3. Waits for the page to be on a real `http(s)://` URL (guards against Grid returning `chrome-error://` or `data:` pages under load).
4. Sets `localStorage["token"]` to `{"type":"Bearer","value":"<JWT>"}`.
5. Sets `localStorage["applicationSettings"]` to `{"shouldRequestOnboarding":false}`.
6. Sets `localStorage["activityTimestamp"]` to the current Unix timestamp in milliseconds.
7. Calls `driver.Navigate().Refresh()` to force React to re-initialise from the token.
8. Waits up to 15 s for the URL to contain `/#` but not `login` (confirming successful authentication).

> **Note:** If a test explicitly logs the user out (calls `AuthSteps.Logout()`), the subsequent login in `[TearDown]` uses `LoginViaApi` again. Only the first login per test uses this fast path; any re-login triggered by a test scenario uses `LoginAs` (UI form).

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

`BaseTest` is parameterised by `BrowserType` via `[TestFixtureSource]`, so each derived class automatically runs once per browser in the configured list. Every fixture must declare a matching constructor:

```csharp
[Category("dashboard_crud")]
[AllureFeature("Dashboard")]
[AllureSuite("CRUD")]
public class DashboardCRUDTests : BaseTest
{
    private AuthSteps _auth = null!;
    private DashboardSteps _dashboard = null!;

    public DashboardCRUDTests(BrowserType browser) : base(browser) { }

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

| Class | Responsibility |
|-------|---------------|
| `DashboardCRUDTests` | `dashboard_crud` | Create dashboard with widget, then delete |
| `DashboardCreateWithWidgetTests` | `dashboard_widget` | Create dashboard and verify widget list |
| `DashboardAllWidgetsTests` | `dashboard_all_widgets` | Add all widget types and verify visibility |
| `DashboardAddDialogTests` | `dashboard_add_dialog` | Add Dashboard dialog validation (name errors, cancel) |
| `DashboardLockTests` | `dashboard_lock` | Lock / Unlock dashboard availability |
| `DashboardResizeWidgetTests` | `dashboard_resize` | Create a dashboard with 5 available widgets and resize ones |
| `DashboardMoveWidgetTests` | `dashboard_movement_widget` | Move widget and check new location |

### Test Data

**CSV users** (`UITests/Data/CSV/`):
- File name configured via `UsersDataFile` key in `appsettings.json` (default: `RP_USERS_CSV_Report.csv`).
- Columns: `Login`, `Password`, `FullName`, `Role`, etc.

**Widget types** (`UITests/Data/Templates/`):
- JSON file with display names for each widget type in a given locale.
- File selected via `WidgetTypesFile` key (default: `widget_types.en.json`).
- Add `widget_types.<lang>.json` to support another locale.

### Global Setup & Cleanup

`GlobalSetup.cs` (`UITests/Hooks/`) runs once before and after the entire test run using NUnit's `[SetUpFixture]`. The `[OneTimeSetUp]` sequence is:

1. **Registers Business-layer DI services** via `ServiceLocator.SetAdditionalRegistrations(services => services.AddBusinessServices())` — always the first action.
2. **Provisions missing users** via `UserProvisioningService.EnsureUsersExistAsync()`:
   - Fetches all existing users from ReportPortal (`GET /api/users/all`).
   - For each user in the CSV that is absent: creates the account (`POST /api/users`) and assigns it to the configured project with the role from the CSV (`PUT /api/v1/project/{name}/assign`).
   - Provisioning errors are logged as warnings and do not abort the test run.
3. **Cleans up leftover test dashboards** for every known user via `DashboardCleanupApiService`.

`[OneTimeTearDown]` repeats step 3 to clean up dashboards created during the run.

Test dashboards are identified by name: prefix `DC_` or suffix ` CRUD Dashboard`.

**Screenshot on failure** — `BaseTest.[TearDown]` calls `ScreenshotUtil.TryAttachScreenshot()` when a test fails. The screenshot is attached to the Allure report and the last-accessed element is highlighted with a red outline so the failing interaction is immediately visible.

### User Provisioning

`UserProvisioningService` (`Business/Helpers/`) ensures test users exist in ReportPortal before any test runs. It authenticates as `superadmin` (credentials from the CSV), compares the CSV user list against the live user list, and creates any missing users.

**User API endpoints used:**

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `GET` | `/api/users/all` | Fetch all existing users |
| `POST` | `/api/users` | Create a new user account |
| `PUT` | `/api/v1/project/{name}/assign` | Assign user to project with a role |

**Project role resolution:** the role is parsed from the `ProjectsAndRoles` column in the CSV (e.g. `report_portal - PROJECT_MANAGER`). Falls back to `MEMBER` if the configured project is not listed for that user.

### Parallelism

`ParallelConfig.cs` sets assembly-level attributes:

```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]
```

Change `LevelOfParallelism` to match the number of available browser sessions / grid nodes. Each parallel fixture gets its own `WebDriver` instance via `IDriverManager` (thread-local storage), so browser sessions never share state.

> **Recommended values:** `2` for local runs; scale up to match available Grid nodes for remote runs.

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
    "Browsers": ["Chrome"],
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
| `DriverSettings.Browsers` | `["Chrome"]` | List of browsers — one fixture instance per entry. Default: `["Chrome"]`. If the list is empty or absent, falls back to `Chrome` |
| `DriverSettings.Remote` | `false` | Use Selenium Grid when `true` |
| `DriverSettings.RemoteUri` | — | Grid / cloud endpoint URL |
| `DriverSettings.Headless` | `false` | Run browser in headless mode |

All settings can be overridden at runtime via environment variables using the standard .NET `IConfiguration` naming convention (e.g., `DriverSettings__Headless=true`).

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

### Cross-browser execution

Each entry in `DriverSettings.Browsers` creates a separate fixture instance. NUnit runs all tests in that fixture once per browser.

**Via `appsettings.json`:**
```json
"DriverSettings": {
  "Browsers": ["Chrome", "Firefox"]
}
```

**Via `BROWSERS` environment variable (short form, comma-separated):**

```powershell
# Windows (PowerShell)
$env:BROWSERS = "Chrome,Firefox"
dotnet test SeleniumFrameworkInteraction.sln
```

```bash
# Linux / macOS / CI
BROWSERS=Chrome,Firefox dotnet test SeleniumFrameworkInteraction.sln
```

**Via standard .NET array environment variables:**

```powershell
$env:DriverSettings__Browsers__0 = "Chrome"
$env:DriverSettings__Browsers__1 = "Firefox"
dotnet test SeleniumFrameworkInteraction.sln
```

**Priority** (highest wins): `BROWSERS` env var → `DriverSettings__Browsers__*` env vars → `appsettings.json Browsers` → default `Chrome`.

> **Tip:** When running two browsers in parallel, set `LevelOfParallelism` to at least 2 so Chrome and Firefox fixture instances execute concurrently.

---

### Headless mode

Headless mode is supported for Chrome, Firefox, and Edge.  
Use it in CI environments where no display is available.

**Windows (PowerShell):**
```powershell
$env:BROWSERS                 = "Chrome"
$env:DriverSettings__Headless = "true"
dotnet test SeleniumFrameworkInteraction.sln
```

**Linux / macOS:**
```bash
BROWSERS=Chrome DriverSettings__Headless=true dotnet test SeleniumFrameworkInteraction.sln
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
BROWSERS=Chrome,Firefox \
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
   - Add `public MyTests(BrowserType browser) : base(browser) { }` — required for `[TestFixtureSource]` parameterisation.
   - Add `[Category("…")]`, `[AllureFeature("…")]`, `[AllureSuite("…")]`.
   - Use `[TestCaseSource(typeof(TestDataProvider), nameof(…))]` for data-driven tests.
   - Resolve steps in `[SetUp]` via `ServiceLocator.GetService<MyFeatureSteps>()`.
   - Call only Steps methods from test bodies.

**Component checklist:**
- Pass `name` and `By rootLocator` to `base(…)` — no `Root` property override needed.
- Declare element properties as computed `private` properties returning typed wrappers with `Root` as the third argument.
- Use relative locators (`.//…`) for XPath or scoped CSS selectors inside components.
- For buttons inside modals that have custom scrollbar overlays, use `ActionHelper.JsClick(element, name)` instead of `Button.Click()`.
