# Project Conventions

## Directory Structure

### Core/Enum
All enums must be placed in `Core/Enum/` with namespace `Core.Enum`.

### Core/Helpers
Reusable infrastructure utilities (e.g., `CsvReader`) belong in `Core/Helpers/` with namespace `Core.Helpers`.

### Core/Logging
All logging classes must be placed in `Core/Logging/` with namespace `Core.Logging`.

### Core/Base
Base classes for Page Object pattern (`BasePage`, `BaseComponent`) are in `Core/Base/` with namespace `Core.Base`.
Concrete page classes go in `Business/Pages/`, components in `Business/Components/`.

### Business Layer
- `Business/Pages/` — Page Object classes (namespace `Business.Pages`). One class per page/screen.
- `Business/Components/` — Reusable UI components and modal dialogs (namespace `Business.Components`).
- `Business/Steps/` — Orchestration step classes consumed by tests (namespace `Business.Steps`). Steps combine page/component calls into higher-level test actions.
- `Business/Models/` — Plain data model POCOs (namespace `Business.Models`).
- `Business/Data/` — Static test data providers (namespace `Business.Data`).

### UITests Layer
- `UITests/Tests/<Feature>/` — NUnit test fixtures grouped by feature (namespace `UITests.Tests.<Feature>`).
- `UITests/Base/BaseTest.cs` — Base fixture that initialises and tears down the WebDriver per test.
- `UITests/ParallelConfig.cs` — Assembly-level parallelism attributes (`[Parallelizable(ParallelScope.All)]`, `[LevelOfParallelism(4)]`).
- `UITests/Data/CSV/` — CSV files with test data (e.g., users exported from ReportPortal).
- `UITests/Data/Templates/` — Language-specific JSON templates for UI element names (e.g., `widget_types.en.json`). Config key `WidgetTypesFile` selects the active file; create `widget_types.<lang>.json` to add a new locale.
