using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BarberShop.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MoneyScope.Application.Services;
using BarberShop.Communication.Token;
using BarberShop.Infra.Interfaces;
using BarberShop.Communication.Models;
using BarberShop.Domain;
using BarberShop.Communication.Models.Token;
using BarberShop.Domain.AccessControl;

namespace BarberShop.Application.Services
{
    public class TokenService : BaseService, ITokenService
    {
        private readonly TokenConfigurations _tokenConfig;
        private readonly IConfiguration _config;

        public TokenService(IRepositoryFactory repositoryFactory, TokenConfigurations tokenConfigurations, IConfiguration config) : base(repositoryFactory)
        {
            _tokenConfig = tokenConfigurations;
            _config = config;
        }

        public async Task<ResponseModel<dynamic>?> ValidateRefreshToken(string token)
        {
            var refreshToken = await _repository<RefreshToken>().Get(t => t.Token == token);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.Now)
            {
                return null;
            }

            await _repository<RefreshToken>().Remove(refreshToken);

            var user = await _repository<User>().Get(u => u.Id == refreshToken.UserId);
            if (user == null) return null;

            return await GetToken(user);
        }
        public async Task<string> GenerateRefreshToken(long userId)
        {
            var refreshTokenValidityMins = _tokenConfig.RefreshTokenValidityMins;

            if (refreshTokenValidityMins == 0)
            {
                var configValue = _config["TokenConfigurations:RefreshTokenValidityMins"];
                refreshTokenValidityMins = string.IsNullOrEmpty(configValue) ? 20 : int.Parse(configValue);
            }

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshTokenValidityMins),
                UserId = userId
            };

            await _repository<RefreshToken>().Create(refreshToken);
            return refreshToken.Token;
        }
        public async Task<ResponseModel<dynamic>> GenerateTokenByEmail(string email)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("functionality", "Authorized"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, email));

            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(email, "Login"), claims);

            identity.Claims.ToList();

            DateTime dtCreation = DateTime.Now;
            DateTime dtExpiration;
            dtExpiration = dtCreation +
            TimeSpan.FromSeconds(1800);

            var key = Encoding.ASCII.GetBytes(_config["TokenConfigurations:Key"]!);
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _config["TokenConfigurations:Issuer"],
                Audience = _config["TokenConfigurations:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Subject = identity,
                NotBefore = dtCreation,
                Expires = dtExpiration
            });

            var token = handler.WriteToken(securityToken);

            return await Task.FromResult(FactoryResponse<dynamic>.Success(token));
        }
        public async Task<ResponseModel<dynamic>> GenerateToken(GenerateTokenModel model)
        {
            var claims = new List<Claim>();

            foreach (var m in model.Modules)
            {
                claims.Add(new Claim("functionality", m.Trim()));
            }

            var uniqueName = "";
            uniqueName = model.UserEmail;

            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, uniqueName));
            claims.Add(new Claim("id", model.UserId.ToString()));

            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(uniqueName, "Login"), claims);

            identity.Claims.ToList();

            DateTime dtCreation = DateTime.UtcNow;
            DateTime dtExpiration;
            if (_tokenConfig.TokenValidityMins != 0)
            {
                dtExpiration = dtCreation +
                TimeSpan.FromMinutes(_tokenConfig.TokenValidityMins);
            }
            else
            {
                dtExpiration = dtCreation +
                TimeSpan.FromMinutes(10);
            }

            var key = Encoding.ASCII.GetBytes(_config["TokenConfigurations:Key"]!);
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _config["TokenConfigurations:Issuer"],
                Audience = _config["TokenConfigurations:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Subject = identity,
                NotBefore = dtCreation,
                Expires = dtExpiration,
            });

            var accessToken = handler.WriteToken(securityToken);

            var unvalidatedToken = handler.ReadJwtToken(accessToken);

            return await Task.FromResult(FactoryResponse<dynamic>.Success(new
            {
                model.UserId,
                model.UserName,
                model.UserEmail,
                model.ModulesAssembled,
                model.UserProfileImage,
                dtCreation,
                dtExpiration,
                AccessToken = accessToken,
                RefreshToken = await GenerateRefreshToken(model.UserId)
            }));
        }
        public async Task<ResponseModel<dynamic>> GetToken(User user, int? seconds = null)
        {
            var userProfilesQuery = _relationRepository<ProfileUser>().GetAllWithInclude(x => x.UserId == user.Id, x => x.Include(y => y.Profile).ThenInclude(y => y.ProfilesModules).ThenInclude(y => y.Module));
            var accessProfiles = await userProfilesQuery.ToListAsync();
            if (accessProfiles == null || accessProfiles.Count() == 0) return FactoryResponse<dynamic>.Unauthorized("Nenhum Perfil Atribuido ao usuario");

            var modules = accessProfiles.SelectMany(x => x.Profile.ProfilesModules).ToList();
            var permissionsModules = GetUserModules(modules);
            var modulesAssembledModel = GetModulesModelUserAssembled(modules);
            if (!permissionsModules.Any()) return FactoryResponse<dynamic>.Unauthorized("Nenhum Perfil Atribuido ao usuario");

            var tokenModel = new GenerateTokenModel
            {
                Modules = permissionsModules,
                ModulesAssembled = modulesAssembledModel,
                UserEmail = user.Email,
                UserProfileImage = user.ImageUrl,
                UserId = user.Id,
                UserName = user.Name,
                Seconds = seconds
            };

            var token = await GenerateToken(tokenModel);

            return token;
        }
        private List<string> GetUserModules(List<ProfileModule> profileModules)
        {
            var module = new HashSet<string>();
            foreach (var m in profileModules)
            {
                var moduleName = m.Module.Name.Trim();

                if (m.Register) module.Add($"{moduleName}-C");
                if (m.Edit) module.Add($"{moduleName}-E");
                if (m.Visualize) module.Add($"{moduleName}-V");
                if (m.Inactivate) module.Add($"{moduleName}-I");
                if (m.Exclude) module.Add($"{moduleName}-EX");
            }
            return module.ToList();
        }
        private ModulesModel GetModulesModelUserAssembled(List<ProfileModule> profileModules)
        {
            var ModuleModel = new ModulesModel() { ModuleProfileUser = new List<ModulesModel.ModuleProfileUserModel>() };
            foreach (var m in profileModules)
            {
                var ModuleName = m.Module.Name.Trim();
                var ModuleProfileUser = new ModulesModel.ModuleProfileUserModel();
                ModuleProfileUser.Name = ModuleName;
                ModuleProfileUser.IdModule = m.ModuleId;

                if (m.Register) ModuleProfileUser.Register = true;

                if (m.Edit) ModuleProfileUser.Edit = true;
                if (m.Visualize) ModuleProfileUser.Visualize = true;
                if (m.Inactivate) ModuleProfileUser.Inactivate = true;
                if (m.Exclude) ModuleProfileUser.Exclude = true;

                ModuleModel.ModuleProfileUser.Add(ModuleProfileUser);
            }
            ModuleModel.InfoProfile = new ModulesModel.InfoProfileModel
            {
                IdProfile = profileModules.FirstOrDefault()?.ProfileId,
                Name = profileModules.FirstOrDefault()?.Profile?.Name
            };
            return ModuleModel;
        }
        public string? Getclaim(string claim, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["TokenConfigurations:Key"]!);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["TokenConfigurations:Issuer"],
                    ValidAudience = _config["TokenConfigurations:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                IEnumerable<Claim> claims = jwtToken.Claims;

                var result = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
                return result;
            }
            catch
            {
                return null;
            }
        }
        public bool IsValidToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["TokenConfigurations:Key"]!);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["TokenConfigurations:Issuer"],
                    ValidAudience = _config["TokenConfigurations:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                DateTime validTo = jwtToken.ValidTo;

                if (DateTime.Now > validTo) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<ResponseModel<dynamic>> ExcludeExpiredRefreshTokens()
        {
            var refreshTokens = await _repository<RefreshToken>().GetAll(t => t.ExpiresAt < DateTime.UtcNow);
            foreach (var token in refreshTokens)
            {
                await _repository<RefreshToken>().Remove(token);
            }
            return FactoryResponse<dynamic>.Success("RefreshTokens expirados removidos com sucesso!");
        }
    }
}
