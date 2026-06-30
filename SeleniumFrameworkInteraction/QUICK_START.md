# Custom Elements Architecture - Quick Start

## What's New

Внедрена полная архитектура кастомных типизированных элементов с автоматическим логированием, явными и неявными ожиданиями.

## File Structure

```
Core/
├── Base/
│   ├── BaseApplication.cs      ← Управляет waits (implicit + explicit/fluent)
│   ├── BasePage.cs             ← Наследует BaseApplication (20s timeout)
│   └── BaseComponent.cs        ← Наследует BaseApplication (10s timeout)
│
└── Elements/                   ← NEW
    ├── IWrapperElement.cs      ← Interface для всех элементов
    ├── WrapperElement.cs       ← Base class с логированием
    ├── Button.cs               ← Clickable button
    ├── Text.cs                 ← Text/label элемент
    ├── Link.cs                 ← Hyperlink
    ├── Input.cs                ← Text input field
    └── Radio.cs                ← Radio button (NEW!)
```

## Usage Example

### Before (Old Way)
```csharp
var input = FindElement(NameInput);
input.Clear();
input.SendKeys("John");
FindElement(AddBtn).Click();
Wait.Until(d => d.FindElements(SuccessMsg).Any(e => e.Displayed));
```

### After (New Way)
```csharp
public Input NameField => new(By.CssSelector("input[name='name']"), "Name Field");
public Button AddBtn => new(By.XPath(".//button[.='Add']"), "Add Button");
public Text SuccessMsg => new(By.ClassName("success"), "Success Message");

NameField.SetValue("John");  // ✓ Логирование + explicit wait
AddBtn.Click();              // ✓ Логирование + explicit wait + проверка clickable
Wait.Until(d => SuccessMsg.IsDisplayed);
```

## Wait Strategy

### 🔹 Implicit Wait
- **Where**: Configured in `BaseApplication` constructor
- **When**: Applied automatically to all element searches
- **Timeout**: 10s (Component), 20s (Page) — configurable per class

### 🔹 Explicit Wait
- **Where**: Property `Wait` in BasePage/BaseComponent
- **When**: Used via `Wait.Until()`
- **Timeout**: Same as implicit (10s or 20s)

### 🔹 Fluent Wait
- **Where**: Static method `BaseApplication.CreateWait(timeoutSeconds)`
- **When**: Custom conditions with polling
- **Config**: 100ms polling, ignores transient exceptions

```csharp
var wait = BaseApplication.CreateWait(5);
var element = wait.Until(d => 
{
    var el = d.FindElement(locator);
    return el.Displayed && el.Enabled ? el : null;
});
```

## Custom Elements at a Glance

| Element | Method | Properties | Use Case |
|---------|--------|-----------|----------|
| **Button** | `Click()` | `Text`, `IsClickable` | Clickable buttons, links, menu items |
| **Text** | `Value`, `ContainsText()` | `IsDisplayed` | Labels, error messages, display text |
| **Link** | `Click()` | `Text`, `Href` | Hyperlinks with navigation |
| **Input** | `SetValue()`, `Clear()` | `Value`, `Placeholder` | Text fields, search boxes, forms |
| **Radio** | `Select()` | `IsSelected`, `Value` | Radio button groups, option selection |

## Logging Output

All actions are automatically logged with context:

```
[Button] Login Button: Clicking
[Button] Login Button: Clicked successfully
[Input] Password Field: Setting value: ****
[Input] Password Field: Value set successfully
[Link] Forgot Password: Reading href attribute
[Text] Error Message: Reading text content
[Radio] Option A: Selecting radio button
[Radio] Option A: Radio button selected
[Radio] Option A: Checking selected state: True
```

## Migration Path

1. **Identify elements** in your Page/Component (By locators)
2. **Create typed properties** using custom elements
3. **Replace FindElement calls** with typed element properties
4. **Simplify action methods** — waits are now built-in

**Example for AddDashboardDialog:**

```csharp
// Before: Generic By locators, manual FindElement
private static readonly By NameInput = By.CssSelector("input[placeholder='Enter dashboard name']");
private static readonly By AddBtn = By.XPath(".//button[.='Add']");

// After: Typed properties with automatic waits
public Input NameInput => new(By.CssSelector("input[placeholder='Enter dashboard name']"), "Name Input");
public Button AddBtn => new(By.XPath(".//button[.='Add']"), "Add Button");

// Usage becomes simpler:
public void Submit(string name)
{
    NameInput.SetValue(name);    // Automatic wait + logging
    AddBtn.Click();              // Automatic wait + logging
}
```

## Key Benefits

✅ **Type-safety** — Compiler checks element access  
✅ **Logging** — All actions logged automatically  
✅ **Consistent waits** — Same strategy everywhere  
✅ **Less boilerplate** — No repeated FindElement/Wait code  
✅ **Extensible** — Easy to add new element types (Checkbox, Select, etc.)  
✅ **IWrapsElement compliant** — Follows Selenium patterns  

## Examples

- **LoginPageRefactored.cs** — Page refactored with Input, Button, Link
- **ConfigurationPanel.cs** — Component with Radio buttons and Text
- **CUSTOM_ELEMENTS_GUIDE.md** — Full documentation with patterns

## Next Steps

1. Review [CUSTOM_ELEMENTS_GUIDE.md](CUSTOM_ELEMENTS_GUIDE.md) for detailed patterns
2. Look at examples: LoginPageRefactored.cs, ConfigurationPanel.cs
3. Extend with new elements as needed (Checkbox, Select, Table, etc.)
4. Gradually migrate existing pages/components to use custom elements
