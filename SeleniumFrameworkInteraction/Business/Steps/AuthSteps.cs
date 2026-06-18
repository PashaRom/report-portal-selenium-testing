using Business.Data;
using Business.Pages;

namespace Business.Steps;

public class AuthSteps
{
    private readonly LoginPage _loginPage = new();

    public void LoginAs(string loginAlias)
    {
        var user = TestDataProvider.GetUser(loginAlias);
        _loginPage.Login(user.Login, user.Password);
    }

    public void Logout() => _loginPage.Logout();
}
