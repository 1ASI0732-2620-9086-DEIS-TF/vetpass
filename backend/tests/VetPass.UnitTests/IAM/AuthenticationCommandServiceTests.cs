using VetPass.API.IAM.Application.Internal.CommandServices;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.Commands;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;

namespace VetPass.UnitTests.IAM;

/// <summary>Password change (US17) and reset by the clinic (US18).</summary>
public class AuthenticationCommandServiceTests
{
    private static readonly Guid Clinic = Guid.NewGuid();
    private static readonly Guid OtherClinic = Guid.NewGuid();
    private static readonly Guid Client = Guid.NewGuid();

    private readonly InMemoryIdentityProvider _identity = new();
    private readonly InMemoryProfiles _profiles = new();
    private readonly AuthenticationCommandService _service;
    private readonly UserProfile _owner;

    public AuthenticationCommandServiceTests()
    {
        _service = new AuthenticationCommandService(_identity, _profiles, new NoClinics(), new NoOpUnitOfWork());
        _owner = new UserProfile(Guid.NewGuid(), "valeria@correo.com", "Valeria", Role.PetOwner, Clinic, Client);
        _profiles.Items.Add(_owner);
        _identity.Passwords[_owner.Id] = "Temporal123";
        _owner.MarkTemporaryPasswordIssued();
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_ReplacesItAndClearsTheMark()
    {
        await _service.ChangePasswordAsync(_owner.Id, "Temporal123", "Perrito2026");

        _identity.Passwords[_owner.Id].ShouldBe("Perrito2026");
        _owner.RequiresPasswordChange.ShouldBeFalse();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_IsRejectedWithoutChanges()
    {
        await Should.ThrowAsync<IncorrectCurrentPasswordException>(() =>
            _service.ChangePasswordAsync(_owner.Id, "NoEsEsta1", "Perrito2026"));

        _identity.Passwords[_owner.Id].ShouldBe("Temporal123");
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_IsRejected() =>
        await Should.ThrowAsync<WeakPasswordException>(() =>
            _service.ChangePasswordAsync(_owner.Id, "Temporal123", "perrito"));

    [Fact]
    public async Task ResetOwnerPassword_IssuesATemporaryPasswordAndMarksTheAccount()
    {
        _owner.MarkPasswordChanged();

        var account = await _service.ResetOwnerPasswordAsync(Client, Clinic);

        account.TemporaryPassword.Length.ShouldBe(12);
        _identity.Passwords[_owner.Id].ShouldBe(account.TemporaryPassword);
        _owner.RequiresPasswordChange.ShouldBeTrue();
    }

    [Fact]
    public async Task ResetOwnerPassword_FromAnotherClinic_IsForbidden() =>
        await Should.ThrowAsync<ForbiddenOperationException>(() =>
            _service.ResetOwnerPasswordAsync(Client, OtherClinic));

    [Fact]
    public async Task ResetOwnerPassword_ClientWithoutAccount_IsNotFound() =>
        await Should.ThrowAsync<ClientWithoutAccountException>(() =>
            _service.ResetOwnerPasswordAsync(Guid.NewGuid(), Clinic));

    [Fact]
    public async Task CreateAccount_IssuesATemporaryPassword()
    {
        var created = await _service.CreateAccountAsync(
            new CreateAccountCommand("jorge@correo.com", "Jorge", Role.PetOwner, Clinic, Guid.NewGuid()));

        created.TemporaryPassword.Length.ShouldBe(12);
        created.Profile.RequiresPasswordChange.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAccount_ExistingEmail_IsRejected() =>
        await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => _service.CreateAccountAsync(
            new CreateAccountCommand("valeria@correo.com", "Valeria", Role.PetOwner, Clinic, Client)));

    private sealed class InMemoryIdentityProvider : IIdentityProvider
    {
        public Dictionary<Guid, string> Passwords { get; } = [];
        private readonly Dictionary<string, Guid> _accounts = new(StringComparer.OrdinalIgnoreCase);

        public Task<AccessToken> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var id = _accounts.TryGetValue(email, out var found) ? found : Passwords.Keys.FirstOrDefault();
            return Passwords.TryGetValue(id, out var stored) && stored == password
                ? Task.FromResult(new AccessToken("token", "refresh", 3600))
                : throw new InvalidCredentialsException();
        }

        public Task<AccessToken> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AccessToken("token", "refresh", 3600));

        public Task<IdentityAccount> CreateAccountAsync(string email, string password, Role role, Guid? clinicId,
            Guid? clientId, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            _accounts[email] = id;
            Passwords[id] = password;
            return Task.FromResult(new IdentityAccount(id, email));
        }

        public Task SetPasswordAsync(Guid accountId, string password, CancellationToken cancellationToken = default)
        {
            Passwords[accountId] = password;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryProfiles : IUserProfileRepository
    {
        public List<UserProfile> Items { get; } = [];

        public Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

        public Task<UserProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(p => p.Email == email.Trim().ToLowerInvariant()));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(p => p.Email == email.Trim().ToLowerInvariant()));

        public Task<IReadOnlyList<Guid>> ListClientIdsWithAccountAsync(Guid clinicId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Guid>>(Items.Where(p => p.ClientId != null).Select(p => p.ClientId!.Value).ToList());

        public Task<UserProfile?> FindByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(p => p.ClientId == clientId));

        public Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
        {
            Items.Add(profile);
            return Task.CompletedTask;
        }
    }

    private sealed class NoClinics : IClinicRepository
    {
        public Task<Clinic?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Clinic?>(null);

        public Task<Clinic?> FindFirstAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Clinic?>(null);

        public Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
