# Custom Elements Architecture Documentation

## Overview

Реализована современная архитектура типизированных элементов на базе паттерна `IWrapsElement`, обеспечивающая:

- **Type-safe element handling** — Использование специализированных классов вместо generic IWebElement
- **Automatic logging** — Все действия логируются с именем элемента и типом операции
- **Integrated waits** — Combination of implicit waits (WebDriver) и explicit/fluent waits (WebDriverWait)
- **Reduced boilerplate** — Встроенная обработка стейлеых ссылок, no-such-element exceptions
- **Extensibility** — Легко добавить новые типы элементов

## Architecture Layers

### 1. Base Layer: BaseApplication
`Core/Base/BaseApplication.cs` — Корневой базовый класс для Page Objects и Components.

**Ответственность:**
- Управление доступом к WebDriver
- Конфигурация implicit waits (установка на уровне WebDriver)
- Создание explicit/fluent waits (метод `CreateWait()`)
- Предоставление логгера для наследников

**Параметры конструктора:**
```csharp
protected BaseApplication(int waitTimeoutSeconds = 10)
```
- BasePage переопределяет с `20` секунд (более длинные ожидания для страниц)
- BaseComponent оставляет `10` секунд (быстрее для компонентов)

**Создание wait'ов:**
```csharp
// Implicit wait — автоматически для всех FindElement вызовов
Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

// Explicit wait — через свойство (создается on-demand)
Wait.Until(condition);

// Fluent wait — через статический метод с кастомным timeout
var customWait = BaseApplication.CreateWait(5);
customWait.Until(condition);
```

### 2. Element Wrapper: IWrapperElement + WrapperElement
`Core/Elements/IWrapperElement.cs` — Interface для всех типов элементов  
`Core/Elements/WrapperElement.cs` — Base class с shared логикой

**IWrapperElement наследует IWrapsElement:**
```csharp
public interface IWrapperElement : IWrapsElement  // Selenium standard
{
    string Name { get; }                // Имя элемента для логирования
    By Locator { get; }                 // By locator или placeholder
    IWebElement Element { get; }        // Underlying WebElement
    bool IsDisplayed { get; }           // Проверка видимости
    bool IsEnabled { get; }             // Проверка enabled state
}
```

**WrapperElement features:**
- Двойная инициализация: по By locator или по готовому IWebElement
- Автоматическое логирование через TestLoggerFactory
- Безопасные свойства IsDisplayed/IsEnabled (не выбросят exception)
- Защита от стейлых элементов (try-catch в свойствах)
- Protected методы для логирования (`LogAction()`, `LogWarning()`)

### 3. Concrete Elements

#### Button
```csharp
public class Button : WrapperElement
{
    public void Click()          // Waits for clickable (displayed + enabled)
    public string Text { get; }  // Button text content
    public bool IsClickable { get; }
}
```

#### Text
```csharp
public class Text : WrapperElement
{
    public string Value { get; }              // Text content
    public bool ContainsText(string text)     // Text contains check
    public string InnerHtml { get; }          // Inner HTML attribute
}
```

#### Link
```csharp
public class Link : WrapperElement
{
    public void Click()          // Waits for clickable
    public string Text { get; }  // Link text
    public string Href { get; }  // href attribute
}
```

#### Input
```csharp
public class Input : WrapperElement
{
    public void SetValue(string value)        // Clear + SendKeys
    public void AppendValue(string value)     // Append without clear
    public void SendKeys(string keys)         // Raw SendKeys
    public void Clear()                       // Clear field
    public string Value { get; }              // Current value attribute
    public string Placeholder { get; }        // Placeholder attribute
}
```

#### Radio (NEW!)
```csharp
public class Radio : WrapperElement
{
    public void Select()                      // Select radio with wait
    public bool IsSelected { get; }           // Selection state
    public string Value { get; }              // Value attribute
    public string LabelText { get; }          // Associated label text
    public bool IsClickable { get; }
}
```

## Wait Mechanism Details

### Implicit Wait Strategy
```csharp
// Configured in BaseApplication constructor
protected BaseApplication(int waitTimeoutSeconds = 10)
{
    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(waitTimeoutSeconds);
}

// BasePage overrides with 20 seconds
protected BasePage(int waitTimeoutSeconds = 20) : base(waitTimeoutSeconds)
```

**Когда срабатывает:**
- При каждом вызове `Driver.FindElement()` или `Driver.FindElements()`
- Применяется ко всем element search операциям без явного Wait.Until()

**Когда НЕ срабатывает:**
- При работе с properties `IsDisplayed`, `IsEnabled` (они используют try-catch)
- При явном использовании `Wait.Until()` или `CreateWait()`

### Explicit Wait (WebDriverWait)
```csharp
// Property в BaseApplication/BasePage/BaseComponent
protected WebDriverWait Wait => CreateWait(WaitTimeoutSeconds);

// Использование:
Wait.Until(d => d.FindElement(locator).Displayed);
```

**Конфигурация:**
- Timeout: как указано (10s для Component, 20s для Page)
- PollingInterval: 100ms (проверяет условие каждые 100 мс)
- IgnoredExceptions: NoSuchElementException, StaleElementReferenceException, ElementNotInteractableException

### Fluent Wait (Custom Waits)
```csharp
// Статический метод для создания custom waits
public static WebDriverWait CreateWait(int timeoutSeconds = 10)
{
    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
    wait.PollingInterval = TimeSpan.FromMilliseconds(100);
    wait.IgnoreExceptionTypes(typeof(NoSuchElementException), ...);
    return wait;
}

// Пример использования в Button.Click()
var waiter = BaseApplication.CreateWait(5);
var element = waiter.Until(d =>
{
    var el = d.FindElement(Locator);
    return el.Displayed && el.Enabled ? el : null;
});
```

## Usage Patterns

### Pattern 1: Simple Property Exposure
```csharp
public class LoginPage : BasePage
{
    public Button LoginBtn => new(By.XPath("//button[.='Login']"), "Login Button");
    
    public void ClickLogin() => LoginBtn.Click();  // Auto-logged, auto-waited
}
```

### Pattern 2: Root-Based Components
```csharp
public class Modal : BaseComponent
{
    private static readonly By ModalRoot = By.Id("modal");
    protected override IWebElement Root => Driver.FindElement(ModalRoot);
    
    public Button OkBtn => new(By.XPath(".//button[.='OK']"), "OK Button");
    // FindElement in OkBtn uses Root.FindElement(locator) from BaseComponent override
}
```

### Pattern 3: Pre-Found Elements
```csharp
var webElement = Driver.FindElement(locator);
var button = new Button(webElement, "My Button");
button.Click();  // Uses pre-found element instead of locator
```

### Pattern 4: Conditional Logic with Elements
```csharp
public class Form : BasePage
{
    public Input NameField => new(By.Id("name"), "Name Field");
    
    public void FillIfVisible(string name)
    {
        if (NameField.IsDisplayed)
        {
            NameField.SetValue(name);  // Logs the action
        }
    }
}
```

### Pattern 5: Radio Button Groups
```csharp
public class Settings : BasePage
{
    public Radio OptionA => new(By.Id("optA"), "Option A");
    public Radio OptionB => new(By.Id("optB"), "Option B");
    
    public void SelectOption(string option)
    {
        (option switch
        {
            "A" => OptionA,
            "B" => OptionB,
            _ => throw new ArgumentException()
        }).Select();  // Logs and waits
    }
}
```

## Logging Output

Все действия автоматически логируются в формате:

```
[ElementType] ElementName: Action description
```

**Примеры:**
```
[Button] Login Button: Clicking
[Button] Login Button: Clicked successfully
[Input] Password Input: Setting value: ****
[Input] Password Input: Value set successfully
[Link] Logout Link: Reading href attribute
[Text] Error Message: Reading text content
[Text] Error Message: Checking text contains 'error': True
[Radio] Option A: Selecting radio button
[Radio] Option A: Radio button selected
[Radio] Option A: Checking selected state: True
```

**Логирование можно контролировать через appsettings.json:**
```json
{
  "LogSettings": {
    "MinLevel": "Information",  // или "Debug" для всех действий
    "EnableFile": true,
    "FilePath": "logs/"
  }
}
```

## Extension Guide

### Добавление нового типа элемента (например Checkbox)

```csharp
public class Checkbox : WrapperElement
{
    public Checkbox(By locator, string name) : base(locator, name) { }
    public Checkbox(IWebElement element, string name) : base(element, name) { }
    
    public override IWebElement WrappedElement =>
        PreFoundElement ?? DriverManager.Current.FindElement(Locator);
    
    /// <summary>Проверить checkbox</summary>
    public void Check()
    {
        LogAction("Checking");
        if (!IsChecked)
        {
            var waiter = BaseApplication.CreateWait(5);
            var element = waiter.Until(d =>
            {
                var el = PreFoundElement ?? d.FindElement(Locator);
                return el.Displayed && el.Enabled ? el : null;
            });
            element?.Click();
        }
        LogAction("Checked");
    }
    
    /// <summary>Открыть checkbox</summary>
    public void Uncheck()
    {
        LogAction("Unchecking");
        if (IsChecked) Element.Click();
        LogAction("Unchecked");
    }
    
    /// <summary>Получить состояние</summary>
    public bool IsChecked => Element.Selected;
}
```

## Migration Strategy

### Step 1: Identify High-Value Pages
Выбрать страницы/компоненты с:
- Частыми действиями с элементами
- Нестабильными тестами (flaky waits)
- Много повторяющегося кода

### Step 2: Create Element Properties
Заменить static By locators на typed properties:

```csharp
// Before
private static readonly By LoginInput = By.CssSelector("input[name='login']");
var input = FindElement(LoginInput);
input.Clear();
input.SendKeys(username);

// After
public Input LoginInput => new(By.CssSelector("input[name='login']"), "Login Input");
LoginInput.SetValue(username);  // Same 3 lines → 1 line with logging
```

### Step 3: Update Methods
Упростить методы используя встроенные waits:

```csharp
// Before
public void Login(string user, string pass)
{
    FindElement(LoginInput).Clear();
    FindElement(LoginInput).SendKeys(user);
    FindElement(PasswordInput).SendKeys(pass);
    var btn = WaitUntilClickable(LoginButton);
    btn.Click();
}

// After
public void Login(string user, string pass)
{
    LoginInput.SetValue(user);
    PasswordInput.SetValue(pass);
    LoginButton.Click();  // Implicit wait + clickable check included
}
```

### Step 4: Verify & Test
1. Запустить существующие тесты — они должны работать как раньше
2. Проверить логи на предмет новых log entries
3. Убедиться, что нет regression'ов

## Best Practices

1. **Именование**: Используй описательные имена для элементов
   ```csharp
   public Button LoginBtn => new(..., "Login Button");  // ✓ Good
   public Button btn => new(..., "btn");               // ✗ Bad
   ```

2. **Группировка**: Группируй связанные элементы
   ```csharp
   public Input SearchField => new(..., "Search Field");
   public Button SearchBtn => new(..., "Search Button");
   ```

3. **Читаемость**: Используй инициализаторы свойств для понятности
   ```csharp
   public Button ConfirmBtn => 
       new(By.XPath("//button[@data-action='confirm']"), "Confirm Button");
   ```

4. **Обработка ошибок**: Полагайся на встроенные wait'ы, а не на try-catch
   ```csharp
   Button.Click();  // Выбросит TimeoutException если не clickable — правильно
   ```

5. **Логирование**: Дополняй встроенное логирование контекстным
   ```csharp
   Logger.LogInformation("Starting login process");
   LoginInput.SetValue(username);  // Auto-logged
   Logger.LogInformation("Credentials entered");
   ```

## Files Created/Modified

**New Files:**
- `Core/Elements/IWrapperElement.cs` — Interface
- `Core/Elements/WrapperElement.cs` — Base class
- `Core/Elements/Button.cs` — Button implementation
- `Core/Elements/Text.cs` — Text implementation
- `Core/Elements/Link.cs` — Link implementation
- `Core/Elements/Input.cs` — Input implementation
- `Core/Elements/Radio.cs` — Radio implementation (NEW!)

**Modified Files:**
- `Core/Base/BaseApplication.cs` — Added wait configuration and CreateWait method

**Examples:**
- `Business/Pages/LoginPageRefactored.cs` — Example page migration
- `Business/Components/ConfigurationPanel.cs` — Example component with Radio buttons

**Documentation:**
- `QUICK_START.md` — Quick reference guide
- `CUSTOM_ELEMENTS_GUIDE.md` — Full usage guide
- `README_ELEMENTS_ARCHITECTURE.md` — This file (architecture details)

---

**Версия:** 1.0  
**Дата:** 2024-06  
**Статус:** Production-ready ✅
