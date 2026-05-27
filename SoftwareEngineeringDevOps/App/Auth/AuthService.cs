using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.Tokens;
using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.Auth
{
    public sealed class AuthService : AuthenticationStateProvider, IAuthService
    {
        private const string AuthenticationType = "JwtAuthentication";
        private const string TokenStorageKey = "auth-token";
        private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
        private readonly IUsersMediator _usersMediator;
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private IUserInfo? _currentUser;
        private bool _isInitialized;

        public AuthService(IUsersMediator usersMediator, ProtectedLocalStorage protectedLocalStorage)
        {
            _usersMediator = usersMediator;
            _protectedLocalStorage = protectedLocalStorage;
        }

        public bool IsAuthenticated => _currentUser != null;

        public IUserInfo? CurrentUser => _currentUser;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_isInitialized)
            {
                return CreateAuthenticationState(_currentUser);
            }

            try
            {
                var storedToken = await _protectedLocalStorage.GetAsync<string>(TokenStorageKey);
                if (!storedToken.Success || string.IsNullOrWhiteSpace(storedToken.Value))
                {
                    _currentUser = null;
                    _isInitialized = true;
                    return CreateAuthenticationState(_currentUser);
                }

                _currentUser = CreateUserFromToken(storedToken.Value);
                if (_currentUser == null)
                {
                    await _protectedLocalStorage.DeleteAsync(TokenStorageKey);
                }
            }
            catch (InvalidOperationException)
            {
                _currentUser = null;
            }

            _isInitialized = true;
            return CreateAuthenticationState(_currentUser);
        }

        public async Task<IUserInfo?> GetCurrentUserAsync()
        {
            if (!_isInitialized)
            {
                await GetAuthenticationStateAsync();
            }

            return _currentUser;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var user = await _usersMediator.GetUserByUsername(username);
            if (user == null || user.Password != password)
            {
                return false;
            }

            var token = GenerateToken(user);
            _currentUser = CreateUserFromToken(token);
            _isInitialized = true;

            try
            {
                await _protectedLocalStorage.SetAsync(TokenStorageKey, token);
            }
            catch (InvalidOperationException)
            {
                _currentUser = null;
                return false;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(_currentUser)));
            return true;
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            _isInitialized = true;

            try
            {
                await _protectedLocalStorage.DeleteAsync(TokenStorageKey);
            }
            catch (InvalidOperationException)
            {
            }

            NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(_currentUser)));
        }

        private AuthenticationState CreateAuthenticationState(IUserInfo? user)
        {
            if (user == null)
            {
                return new AuthenticationState(Anonymous);
            }

            var identity = new ClaimsIdentity(BuildClaims(user), AuthenticationType);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private string GenerateToken(IUser user)
        {
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey())),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: BuildClaims(user),
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return _tokenHandler.WriteToken(token);
        }

        private IUserInfo? CreateUserFromToken(string token)
        {
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = _tokenHandler.ReadJwtToken(token);
            }
            catch (ArgumentException)
            {
                return null;
            }

            if (jwtToken.ValidTo <= DateTime.UtcNow)
            {
                return null;
            }

            var claims = jwtToken.Claims.ToList();
            var idClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.Ordinal);

            if (!long.TryParse(idClaim, out var id) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return null;
            }

            return new AuthenticatedUserInfo(id, username, firstName, lastName, roles.Contains(nameof(UserRole.Admin)), roles.Contains(nameof(UserRole.Editor)));
        }

        private static IEnumerable<Claim> BuildClaims(IUserInfo user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.Admin)));
            }

            if (user.IsEditor)
            {
                claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.Editor)));
            }

            if (!user.IsAdmin && !user.IsEditor)
            {
                claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.Standard)));
            }

            return claims;
        }

        private static string GetSigningKey()
        {
            return "SoftwareEngineeringDevOps-auth-signing-key-net10";
        }

        private sealed record AuthenticatedUserInfo(
            long Id,
            string Username,
            string FirstName,
            string LastName,
            bool IsAdmin,
            bool IsEditor) : IUserInfo;
    }
}
