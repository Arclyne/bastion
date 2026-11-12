using System.Threading.Tasks;
using Bastion.Client.Screens.Login;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.Login;

public class TestLoginViewModel
{
    private const string AnyPassword = "any-password";
    private const string MalformedIdentifier = "marta@";
    private const string WellFormedNickname = "marta_muros";
    private const string WellFormedEmail = "marta@example.com";
    private const string PaddedNickname = " marta_muros ";

    private readonly FakeAccountClient _accounts;
    private readonly LoginViewModel _viewModel;

    public TestLoginViewModel()
    {
        _accounts = new FakeAccountClient();
        _viewModel = new LoginViewModel(new FakeLanguageService(), _accounts, new FakeNavigator());
        _viewModel.Password = AnyPassword;
    }

    [Fact]
    public async Task LogIn_MalformedIdentifier_MarksIdentifierMalformed()
    {
        _viewModel.Identifier = MalformedIdentifier;

        await _viewModel.LogIn();

        Assert.True(_viewModel.IsIdentifierMalformed);
    }

    [Fact]
    public async Task LogIn_MalformedIdentifier_DoesNotContactServer()
    {
        _viewModel.Identifier = MalformedIdentifier;

        await _viewModel.LogIn();

        Assert.Equal(0, _accounts.LogInCount);
    }

    [Fact]
    public async Task LogIn_EmptyIdentifier_DoesNotMarkIdentifierMalformed()
    {
        _viewModel.Identifier = string.Empty;

        await _viewModel.LogIn();

        Assert.False(_viewModel.IsIdentifierMalformed);
    }

    [Theory]
    [InlineData(WellFormedNickname)]
    [InlineData(WellFormedEmail)]
    [InlineData(PaddedNickname)]
    public async Task LogIn_WellFormedIdentifier_ContactsServer(string identifier)
    {
        _viewModel.Identifier = identifier;

        await _viewModel.LogIn();

        Assert.Equal(1, _accounts.LogInCount);
    }

    [Fact]
    public async Task Identifier_ChangedAfterMalformed_ClearsMalformedMark()
    {
        _viewModel.Identifier = MalformedIdentifier;
        await _viewModel.LogIn();

        _viewModel.Identifier = WellFormedNickname;

        Assert.False(_viewModel.IsIdentifierMalformed);
    }
}
