using Core.Base;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Components;

public class AddDashboardDialog : BaseComponent
{
    private static readonly By ModalRoot     = By.CssSelector("#modal-root [class*='modalLayout__modal-window']");
    private static readonly By NameInput     = By.CssSelector("input[placeholder='Enter dashboard name']");
    private static readonly By CancelBtn     = By.XPath(".//button[.='Cancel']");
    private static readonly By AddBtn        = By.XPath(".//button[.='Add']");
    private static readonly By NameError     = By.CssSelector("input[class*='error']");
    private static readonly By ShowConfigLink = By.XPath(".//*[text()='Show dashboard configuration']");
    private static readonly By ConfigLabel   = By.XPath(".//*[text()='Configuration']");
    private static readonly By ConfigDesc    = By.XPath(".//*[contains(text(),'Paste from the clipboard')]");

    private static readonly By GlobalNameInput =
        By.CssSelector("#modal-root input[placeholder='Enter dashboard name']");

    protected override IWebElement Root => Driver.FindElement(ModalRoot);

    public bool IsOpen()
    {
        try { return IsElementDisplayed(GlobalNameInput); }
        catch { return false; }
    }

    public bool IsClosed()
    {
        try
        {
            return Wait.Until(d =>
            {
                try
                {
                    var els = d.FindElements(GlobalNameInput);
                    return els.Count == 0 || !els[0].Displayed;
                }
                catch (StaleElementReferenceException) { return true; }
                catch (NoSuchElementException)         { return true; }
            });
        }
        catch { return true; }
    }

    public void FillName(string name)
    {
        var input = FindElement(NameInput);
        input.Clear();
        input.SendKeys(name);
    }

    public void ClickCancel()
    {
        Logger.LogInformation("Clicking Cancel in Add Dashboard dialog");
        FindElement(CancelBtn).Click();
    }

    public void ClickAdd()
    {
        Logger.LogInformation("Clicking Add in Add Dashboard dialog");
        FindElement(AddBtn).Click();
    }

    public bool IsNameFieldInError()
    {
        try { return FindElements(NameError).Any(e => e.Displayed); }
        catch { return false; }
    }

    public bool IsShowConfigLinkVisible()
    {
        try { return IsElementDisplayed(ShowConfigLink); }
        catch { return false; }
    }

    public void ClickShowConfigLink() => FindElement(ShowConfigLink).Click();

    public bool IsConfigurationFieldVisible()
    {
        try { return IsElementDisplayed(ConfigLabel); }
        catch { return false; }
    }

    public bool IsConfigurationDescriptionVisible()
    {
        try { return IsElementDisplayed(ConfigDesc); }
        catch { return false; }
    }
}
