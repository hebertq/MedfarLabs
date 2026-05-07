using Microsoft.AspNetCore.Http;
using MedfarLabs.Core.Domain.Interfaces.Security;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Security;

namespace MedFarLab.Api.Security
{
    public class HttpUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecurityRepository _securityRepo;
        private readonly IGlobalSecurityCache _cache;
        private readonly MedfarLabs.Core.Domain.Interfaces.Repositories.Identity.IUserRepository _userRepository;
        private HashSet<int>? _userPermissions;

        public HttpUserContext(IHttpContextAccessor httpContextAccessor, ISecurityRepository securityRepo, IGlobalSecurityCache cache, MedfarLabs.Core.Domain.Interfaces.Repositories.Identity.IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _securityRepo = securityRepo;
            _cache = cache;
            _userRepository = userRepository;
        }

        public long UserId 
        { 
            get => GetItem("UserId"); 
            set {} 
        }
        public long OrganizationId 
        { 
            get => GetItem("OrganizationId"); 
            set {} 
        }
        public long BranchId 
        { 
            get => GetItem("BranchId"); 
            set {} 
        }
        public int OrganizationTypeId 
        { 
            get => (int)GetItem("OrganizationTypeId"); 
            set {} 
        }

        private long GetItem(string key) 
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(key, out var val) == true && val is long lVal) 
            {
                return lVal;
            }
            return 0;
        }

        public async Task<bool> HasPermissionAsync(int actionId)
        {
            var user = await _userRepository.GetByIdAsync(UserId);
            if (user != null && user.Username == "masteradmin")
            {
                return true;
            }

            if (_userPermissions == null)
            {
                var userRoleIds = await _securityRepo.GetUserRoleIdsAsync(UserId, OrganizationId, BranchId);
                _userPermissions = _cache.GetPermissionsForRoles(userRoleIds);
            }

            return _userPermissions!.Contains(actionId);
        }
    }
}
