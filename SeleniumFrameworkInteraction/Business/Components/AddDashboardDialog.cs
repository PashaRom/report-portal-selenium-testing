using Core.Base;
using Core.Elements;
using Core.Helpers;
using Core.Structures;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Components;

public class AddDashboardDialog : BaseComponent
{
    public AddDashboardDialog() : base(
        "Add Dashboard Dialog",
        By.CssSelector("#modal-root [class*='modalLayout__modal-window']")) { }

    private Text NameInput        => new(By.CssSelector("input[placeholder='Enter dashboard name']"), "Dashboard Name Input", Root);
    private Text GlobalNameInput  => new(By.CssSelector("input[placeholder='Enter dashboard name']"), "Global Dashboard Name Input", Root);
    private Text NameErrorInput   => new(By.CssSelector("input[class*='error']"), "Name Error Input", Root);
    private Label ConfigurationLabel => new(By.XPath(".//*[text()='Configuration']"), "Configuration Label", Root);
    private Label ConfigurationDesc  => new(By.XPath(".//*[contains(text(),'Paste from the clipboard')]"), "Configuration Description", Root);
    private Button CancelBtn      => new(By.XPath(".//button[.='Cancel']"), "Cancel Button", Root);
    private Button AddBtn         => new(By.XPath(".//button[.='Add']"), "Add Button", Root);
    private Link ShowConfigLink   => new(By.XPath(".//*[text()='Show dashboard configuration']"), "Show Configuration Link", Root);

    public bool IsOpen()
    {
        try
        {
            return WaitHelper.Until(_ => GlobalNameInput.IsDisplayed, timeout: Timeouts.Sec2);
        }
        catch
        {
            return false;
        }
    }

    public bool IsClosed()
    {
        try
        {
            return WaitHelper.Until(_ => !GlobalNameInput.IsDisplayed, timeout: Timeouts.Sec2);
        }
        catch
        {
            return true;
        }
    }

    public void FillName(string name) => NameInput.SetValue(name);

    public void ClickCancel()
    {
        Logger.LogInformation("[{Component}] Clicking Cancel", Name);
        CancelBtn.Click();
    }

    public void ClickAdd()
    {
        Logger.LogInformation("[{Component}] Clicking Add", Name);
        AddBtn.Click();
    }

    public bool IsNameFieldInError() => NameErrorInput.IsDisplayed;

    public bool IsShowConfigLinkVisible() => ShowConfigLink.IsDisplayed;

    public void ClickShowConfigLink() => ShowConfigLink.Click();

    public bool IsConfigurationFieldVisible() => ConfigurationLabel.IsDisplayed;

    public bool IsConfigurationDescriptionVisible() => ConfigurationDesc.IsDisplayed;
}
