using Allure.NUnit.Attributes;
using Core.Enum;
using UITests.Hooks;

namespace UITests.Tests.Dashboard;

/// <summary>
/// Feature: Dashboard Add New Dashboard Dialog
/// Background: login as "default", open the Add New Dashboard dialog
/// </summary>
[Category("dashboard_add_dialog")]
[AllureFeature("Dashboard")]
[AllureSuite("Add Dashboard Dialog")]
public class DashboardAddDialogTests : DashboardTestBase
{
    public DashboardAddDialogTests(BrowserType browser) : base(browser) { }

    [SetUp]
    public void OpenDialog()
    {
        _auth.LoginViaApi("default");
        _dashboard.OpenAddDialog();
        Assume.That(_dashboard.IsAddDialogOpen(), Is.True,
            "Precondition: Add New Dashboard dialog must be open");
    }

    [Test]
    [Description("Cancel closes the Add New Dashboard dialog")]
    public void Cancel_ClosesDialog()
    {
        _dashboard.Dialog.ClickCancel();

        Assert.That(_dashboard.IsAddDialogClosed(), Is.True,
            "Dialog should be closed after clicking Cancel");
    }

    [Test]
    [Description("Clicking Add without a name keeps the dialog open with a validation error")]
    public void AddWithoutName_ShowsValidationError_AndKeepsDialogOpen()
    {
        _dashboard.Dialog.ClickAdd();

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.Dialog.IsNameFieldInError(), Is.True,
                "Name field should be highlighted with an error");
            Assert.That(_dashboard.IsAddDialogOpen(), Is.True,
                "Dialog should remain open after failed submission");
        });

        _dashboard.Dialog.ClickCancel();
        Assert.That(_dashboard.IsAddDialogClosed(), Is.True);
    }

    [Test]
    [Description("Expanding Show dashboard configuration reveals the Configuration field")]
    public void ShowDashboardConfiguration_RevealsConfigurationField()
    {
        Assert.That(_dashboard.Dialog.IsShowConfigLinkVisible(), Is.True,
            "Show dashboard configuration link should be visible");

        _dashboard.Dialog.ClickShowConfigLink();

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.Dialog.IsConfigurationFieldVisible(), Is.True,
                "Configuration field should be visible after expanding");
            Assert.That(_dashboard.Dialog.IsConfigurationDescriptionVisible(), Is.True,
                "Configuration description should contain import instructions");
        });
    }
}
