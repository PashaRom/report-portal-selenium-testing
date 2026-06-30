using Core.Base;
using Core.Elements;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Components;

public class DeleteDashboardDialog : BaseComponent
{
    public DeleteDashboardDialog() : base(
        "Delete Dashboard Dialog",
        By.CssSelector("#modal-root")) { }

    private Button DeleteBtn => new(By.XPath(".//button[.='Delete']"), "Delete Button", Root);

    public void ClickDelete()
    {
        Logger.LogInformation("[{Component}] Clicking Delete via JS (overlay workaround)", Name);
        var element = WaitHelper.DefaultWait(DeleteBtn);
        ActionHelper.JsClick(element, DeleteBtn.Name);
    }
}
