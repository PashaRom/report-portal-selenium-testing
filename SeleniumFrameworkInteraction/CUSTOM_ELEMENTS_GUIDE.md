# Custom Elements Architecture - Usage Guide

## Overview
Реализована архитектура кастомных элементов на базе `IWrapsElement` с поддержкой:
- ✅ Логирования всех действий с элементами
- ✅ Implicit waits (установлен на WebDriver на уровне BaseApplication)
- ✅ Explicit waits (через метод `BaseApplication.CreateWait()`)
- ✅ Fluent wait pattern с polling и exception handling

## Available Custom Elements

### Button
```csharp
public Button CloseBtn => new(By.XPath(".//button[.='Close']"), "Close Button");
// или с готовым элементом:
public Button CloseBtn => new(driver.FindElement(locator), "Close Button");

// Usage:
CloseBtn.Click();        // Автоматически ждёт clickable (displayed + enabled)
CloseBtn.Text;           // Получить текст кнопки
CloseBtn.IsClickable;    // Проверить clickable
CloseBtn.IsDisplayed;    // Проверить видимость
CloseBtn.IsEnabled;      // Проверить enabled
```

### Text (Label, Span, Div)
```csharp
public Text ErrorMessage => new(By.CssSelector(".error-message"), "Error Message");

// Usage:
ErrorMessage.Value;               // Получить текст
ErrorMessage.ContainsText("error"); // Проверить содержание текста
ErrorMessage.InnerHtml;           // Получить inner HTML
ErrorMessage.IsDisplayed;         // Проверить видимость
```

### Link
```csharp
public Link LogoutLink => new(By.XPath("//a[.='Logout']"), "Logout Link");

// Usage:
LogoutLink.Click();      // Автоматически ждёт clickable
LogoutLink.Text;         // Получить текст ссылки
LogoutLink.Href;         // Получить href атрибут
LogoutLink.IsClickable;  // Проверить clickable
```

### Input
```csharp
public Input LoginField => new(By.CssSelector("input[name='login']"), "Login Input");

// Usage:
LoginField.SetValue("username");     // Clear + SendKeys
LoginField.AppendValue(" suffix");   // Append without clear
LoginField.SendKeys(Keys.Enter);     // Send special keys
LoginField.Clear();                  // Clear field
LoginField.Value;                    // Get current value
LoginField.Placeholder;              // Get placeholder attribute
LoginField.IsDisplayed;              // Check visibility
```

### Radio
```csharp
public Radio OptionA => new(By.Id("radioA"), "Option A");

// Usage:
OptionA.Select();        // Select radio with explicit wait
OptionA.IsSelected;      // Check if selected
OptionA.Value;           // Get value attribute
OptionA.LabelText;       // Get associated label text
OptionA.IsClickable;     // Check if clickable
```

## Wait Configuration

### Implicit Wait
Настроен глобально на уровне `BaseApplication` конструктора:
```csharp
// По умолчанию: 10 секунд для Component, 20 секунд для Page
protected BaseApplication(int waitTimeoutSeconds = 10)
{
    // Автоматически configures: 
    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(waitTimeoutSeconds);
}
```

### Explicit Wait (через BaseApplication)
```csharp
// В Page или Component (наследует BaseApplication):
Wait.Until(d => d.FindElement(locator).Displayed);
```

### Fluent Wait (через статический метод)
```csharp
// Создание custom fluent wait с timeout 5 секунд:
var waiter = BaseApplication.CreateWait(5);
var element = waiter.Until(d => 
{
    var el = d.FindElement(locator);
    return el.Displayed && el.Enabled ? el : null;
});

// Параметры:
// - Timeout: параметр метода (по умолчанию 10)
// - PollingInterval: 100ms (проверка каждые 100ms)
// - IgnoredExceptions: NoSuchElementException, StaleElementReferenceException, 
//   ElementNotInteractableException, TimeoutException
```

## Integration with Existing Code

### BasePage пример
```csharp
public class LoginPage : BasePage
{
    // Определить элементы как свойства
    public Input LoginInput => new(By.CssSelector("input[name='login']"), "Login Input");
    public Input PasswordInput => new(By.CssSelector("input[name='password']"), "Password Input");
    public Button LoginButton => new(By.XPath("//button[.='Login']"), "Login Button");
    
    public void Login(string username, string password)
    {
        Logger.LogInformation("Logging in as {Username}", username);
        LoginInput.SetValue(username);
        PasswordInput.SetValue(password);
        LoginButton.Click(); // Автоматически ждёт clickable
    }
}
```

### BaseComponent пример
```csharp
public class AddDashboardDialog : BaseComponent
{
    private static readonly By ModalRoot = By.CssSelector("#modal-root");
    
    public Input NameInput => new(By.CssSelector("input[placeholder='Enter name']"), "Name Input");
    public Button AddButton => new(By.XPath(".//button[.='Add']"), "Add Button");
    public Button CancelButton => new(By.XPath(".//button[.='Cancel']"), "Cancel Button");
    
    protected override IWebElement Root => Driver.FindElement(ModalRoot);
    
    public void FillAndSubmit(string name)
    {
        NameInput.SetValue(name);
        AddButton.Click();
    }
}
```

## Logging Output Example

Все действия автоматически логируются с информацией об элементе:

```
[Button] Close Button: Clicking
[Button] Close Button: Clicked successfully
[Input] Login Input: Setting value: myusername
[Input] Login Input: Value set successfully
[Link] Logout Link: Reading href attribute
[Text] Error Message: Reading text content
[Radio] Option A: Selecting radio button
[Radio] Option A: Radio button selected
```

## Migration from Old Code

### Before (старый подход):
```csharp
var input = FindElement(NameInput);
input.Clear();
input.SendKeys("value");
FindElement(AddBtn).Click();
```

### After (новый подход с кастомными элементами):
```csharp
public Input NameField => new(By.CssSelector("input[name='name']"), "Name Field");
public Button AddBtn => new(By.XPath(".//button[.='Add']"), "Add Button");

NameField.SetValue("value");  // Логирование + explicit wait
AddBtn.Click();               // Логирование + explicit wait + проверка clickable
```

## Architecture Benefits

1. **Type-safe element access** - Дополнительный контроль типов для элементов
2. **Built-in logging** - Все действия логируются автоматически  
3. **Consistent waits** - Одна стратегия waits для всех элементов
4. **Reduced boilerplate** - Не нужно повторять FindElement/Wait в каждом методе
5. **Easy to extend** - Легко добавить новые элементы (Checkbox, Select, Table, etc.)
6. **IWrapsElement compliance** - Соответствует Selenium стандартам

## Extending with New Elements

```csharp
public class Checkbox : WrapperElement
{
    public Checkbox(By locator, string name) : base(locator, name) { }
    public Checkbox(IWebElement element, string name) : base(element, name) { }
    
    public override IWebElement WrappedElement =>
        PreFoundElement ?? DriverManager.Current.FindElement(Locator);
    
    public void Check()
    {
        LogAction("Checking checkbox");
        if (!IsChecked) Element.Click();
    }
    
    public bool IsChecked => Element.Selected;
}
```
