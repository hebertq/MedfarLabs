using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;
using MedFarLab.Application.Features.System.Queries.GetMenus;
using MedFarLab.Application.Features.System.Queries.GetSettingsMenu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace MedFarLab.Pwa.Services
{
    public class MedFarMenuService
    {
        private readonly IMediator _mediator;
        private List<NavigationMenuResponseDTO> _navItems = new();
        private List<NavigationMenuResponseDTO> _settingsItems = new();
        private bool _loaded = false;

        public MedFarMenuService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task LoadAsync(int organizationTypeId, string? userRole = null)
        {
            if (_loaded) return;

            // Carga en paralelo — nav + settings al mismo tiempo
            var navTask = _mediator.Send(new GetNavigationMenusQuery(organizationTypeId));

            var settingsTask = _mediator.Send(new GetSettingsMenuQuery(organizationTypeId, userRole));

            await Task.WhenAll(navTask, settingsTask);

            if (navTask.Result.IsSuccess)
                _navItems = navTask.Result.Data?.Where(m => m.MenuTypeId == 178).ToList() ?? new();

            if (settingsTask.Result.IsSuccess)
                _settingsItems = settingsTask.Result.Data?.Where(m => m.MenuTypeId == 179).ToList() ?? new();

            _loaded = true;
        }

        /// <summary>Ítems para el sidebar lateral — solo tipo NAV.</summary>
        public IEnumerable<NavigationMenuResponseDTO> NavItems => _navItems;

        /// <summary>Ítems agrupados para el dropdown del perfil — tipo SETTINGS.</summary>
        public IEnumerable<IGrouping<int, NavigationMenuResponseDTO>> SettingsGroups =>
            _settingsItems.GroupBy(s => s.SortGroup).OrderBy(g => g.Key);

        /// <summary>Nombres descriptivos de los grupos de ajustes.</summary>
        public string GetGroupLabel(int sortGroup) => sortGroup switch
        {
            0 => "Mi cuenta",
            1 => "Configuración",
            2 => "Sistema",
            _ => string.Empty
        };
    }
}
