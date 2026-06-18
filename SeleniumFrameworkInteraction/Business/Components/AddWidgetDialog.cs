using Core.Base;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Business.Components;

public class AddWidgetDialog : BaseComponent
{
    private static readonly By DialogRoot    = By.CssSelector("[class*='widgetWizardModal']");
    private static readonly By NextStepBtn   = By.XPath("//button[contains(.,'Next step')]");
    private static readonly By WidgetNameInput = By.CssSelector("input[placeholder='Enter widget name']");
    private static readonly By AddBtn        = By.XPath(".//button[.='Add']");
    private static readonly By FilterInput      = By.CssSelector("input[placeholder='Search filter by name']");
    private static readonly By LaunchNameInput  = By.CssSelector("input[placeholder='Enter launch name']");
    private static readonly By Level1Input         = By.XPath("//div[contains(@class,'modalField')][.//span[.='Level 1 ']]//input");
    private static readonly By Level1OverviewInput = By.XPath("//div[contains(@class,'modalField')][.//span[.='Level 1 (overview)']]//input");
    private static readonly By AttributeKeyInput   = By.XPath("//div[contains(@class,'modalField')][.//span[.='Attribute key']]//input");
    private static readonly By FirstRadio       = By.CssSelector("[class*='inputRadio__toggler']");

    protected override IWebElement Root => Driver.FindElement(DialogRoot);

    public void Submit(string widgetType, string widgetName)
    {
        Logger.LogInformation("Adding widget type={Type} name={Name}", widgetType, widgetName);

        SelectWidgetType(widgetType);
        ClickNextStep();
        HandleConfigSteps();

        var nameInput = Wait.Until(d => d.FindElements(WidgetNameInput).FirstOrDefault(e => e.Displayed));
        nameInput!.Clear();
        nameInput.SendKeys(widgetName);

        FindElement(AddBtn).Click();
        Wait.Until(d => d.FindElements(DialogRoot).All(e => !e.Displayed));

        Logger.LogInformation("Widget {Name} added successfully", widgetName);
    }

    private void SelectWidgetType(string widgetType)
    {
        // Try multiple selector strategies since widget type cards vary in markup
        var typeEl = Wait.Until(d =>
        {
            var candidates = d.FindElements(
                By.XPath($".//*[normalize-space(text())='{widgetType}']"));
            return candidates.FirstOrDefault(e => e.Displayed);
        });

        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", typeEl);
    }

    private void ClickNextStep()
    {
        var btn = Wait.Until(d =>
        {
            var btns = d.FindElements(NextStepBtn);
            return btns.FirstOrDefault(e => e.Displayed && e.Enabled);
        });
        btn!.Click();
    }

    private void HandleConfigSteps()
    {
        for (var i = 0; i < 8; i++)
        {
            if (Driver.FindElements(WidgetNameInput).Any(e => e.Displayed))
                return;

            var filterInputEls = Driver.FindElements(FilterInput);
            if (filterInputEls.Any(e => e.Displayed))
                TrySelectFirstFilter();

            foreach (var locator in new[] { LaunchNameInput, Level1Input, Level1OverviewInput, AttributeKeyInput })
                TryFillRandomText(locator);

            try
            {
                var stepWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(3));
                var nextBtn = stepWait.Until(d =>
                    d.FindElements(NextStepBtn).FirstOrDefault(e => e.Displayed && e.Enabled));
                nextBtn!.Click();
            }
            catch
            {
                return;
            }
        }
    }

    private void TryFillRandomText(By locator)
    {
        var el = Driver.FindElements(locator).FirstOrDefault(e => e.Displayed);
        if (el == null) return;
        el.Clear();
        el.SendKeys(Guid.NewGuid().ToString("N")[..8]);
    }

    private void TrySelectFirstFilter()
    {
        try
        {
            var radioWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            var radio = radioWait.Until(d =>
                d.FindElements(FirstRadio).FirstOrDefault(e => e.Displayed));

            if (radio != null)
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", radio);
        }
        catch
        {
            Logger.LogWarning("No filter radio buttons found — proceeding without filter selection");
        }
    }
}
