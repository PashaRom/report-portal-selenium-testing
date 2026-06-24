using Core.Base;
using Core.Elements;
using Core.Helpers;
using Core.Structures;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Components;

public class AddWidgetDialog : BaseComponent
{
    public AddWidgetDialog() : base(
        "Add Widget Dialog",
        By.CssSelector("[class*='widgetWizardModal']")) { }

    private Text DialogRoot      => new(By.CssSelector("[class*='widgetWizardModal']"), "Widget Wizard Modal");
    private Button AddBtn        => new(By.XPath(".//button[.='Add']"), "Add Button", Root);
    private Button NextStepBtn   => new(By.XPath(".//button[contains(.,'Next step')]"), "Next Step Button", Root);
    private Text WidgetNameInput => new(By.CssSelector("input[placeholder='Enter widget name']"), "Widget Name Input", Root);
    private Text FilterInput     => new(By.CssSelector("input[placeholder='Search filter by name']"), "Filter Search Input", Root);
    private Text LaunchNameInput => new(By.CssSelector("input[placeholder='Enter launch name']"), "Launch Name Input", Root);
    private Text Level1Input     => new(By.XPath(".//div[contains(@class,'modalField')][.//span[.='Level 1 ']]//input"), "Level 1 Input", Root);
    private Text Level1OverviewInput => new(By.XPath(".//div[contains(@class,'modalField')][.//span[.='Level 1 (overview)']]//input"), "Level 1 Overview Input", Root);
    private Text AttributeKeyInput   => new(By.XPath(".//div[contains(@class,'modalField')][.//span[.='Attribute key']]//input"), "Attribute Key Input", Root);
    private Radio FirstFilterRadio   => new(By.CssSelector("[class*='inputRadio__toggler']"), "First Filter Radio", Root);

    public void WaitUntilVisible() =>
        WaitHelper.Until(_ => DialogRoot.IsDisplayed, timeout: Timeouts.Sec1);

    public void Submit(string widgetType, string widgetName)
    {
        Logger.LogInformation("[{Component}] Adding widget type={Type} name={WidgetName}", Name, widgetType, widgetName);

        SelectWidgetType(widgetType);
        ClickNextStep();
        HandleConfigSteps();

        WaitHelper.Until(_ => WidgetNameInput.IsDisplayed, timeout: Timeouts.Sec10);
        WidgetNameInput.SetValue(widgetName);
        AddBtn.Click();
        WaitHelper.Until(_ => !DialogRoot.IsDisplayed, timeout: Timeouts.Sec5);

        Logger.LogInformation("[{Component}] Widget {WidgetName} added successfully", Name, widgetName);
    }

    private void SelectWidgetType(string widgetType)
    {
        var card = new Text(By.XPath($"//*[normalize-space(text())='{widgetType}']"), widgetType);
        var el = WaitHelper.DefaultWait(card);
        ActionHelper.JsClick(el, widgetType);
    }

    private void ClickNextStep() => NextStepBtn.Click();

    private void HandleConfigSteps()
    {
        for (var i = 0; i < 15; i++)
        {
            if (WidgetNameInput.IsDisplayed)
            {
                Logger.LogDebug("[{Component}] Widget name input is visible at iteration {Iteration}, config steps complete", Name, i);
                return;
            }

            if (FilterInput.IsDisplayed)
            {
                TrySelectFirstFilter();
            }

            TryFillIfVisible(LaunchNameInput);
            TryFillIfVisible(Level1Input);
            TryFillIfVisible(Level1OverviewInput);
            TryFillIfVisible(AttributeKeyInput);

            try
            {
                NextStepBtn.Click();
            }
            catch (Exception ex)
            {
                // Next Step button is gone — we may already be on the widget name step (still transitioning).
                // Break and let the explicit wait in Submit() handle it.
                Logger.LogWarning(ex, "[{Component}] Next step button click failed at iteration {Iteration} — stopping config steps", Name, i);
                break;
            }
        }
    }

    private void TryFillIfVisible(Text input)
    {
        if (!input.IsDisplayed)
        {
            Logger.LogDebug("Input '{Name}' is not visible, skipping fill", input.Name);
            return;
        }
        input.SetValue(Guid.NewGuid().ToString("N")[..8]);
        WaitHelper.Until(_ => !string.IsNullOrEmpty(input.Value), timeout: Timeouts.Sec2);
        Root.Click();
    }

    private void TrySelectFirstFilter()
    {
        try
        {
            // Radio buttons load asynchronously — give up to 10 seconds (Firefox/Edge remote may be slow)
            WaitHelper.Until(_ => FirstFilterRadio.IsDisplayed, timeout: Timeouts.Sec10);

            // Use JS click to reliably trigger React event handlers in all browsers (headless Firefox/Edge
            // may not propagate a standard WebDriver click through the custom radio label to React).
            var el = WaitHelper.DefaultWait(FirstFilterRadio);
            ActionHelper.JsClick(el, FirstFilterRadio.Name);
        }
        catch
        {
            Logger.LogWarning("[{Component}] No filter radio buttons found — proceeding without filter selection", Name);
        }
    }
}
