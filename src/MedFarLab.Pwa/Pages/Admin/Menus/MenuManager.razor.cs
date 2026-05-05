using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;
using MedfarLabs.Core.Application.Features.System.Dtos;
using MedfarLabs.Core.Application.Features.System.Commands.CreateMenu;
using MedfarLabs.Core.Application.Features.System.Commands.UpdateMenu;
using MedFarLab.Application.Features.System.Queries.GetMenus;

namespace MedFarLab.Pwa.Pages.Admin.Menus;

public partial class MenuManager : ComponentBase
{
    [Inject] protected IMediator Mediator { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool IsSaving = false;
    protected bool ShowDialog = false;
    protected int SelectedOrgTypeId = 124;
    protected List<NavigationMenuResponseDTO> MenusList = new();
    protected NavigationMenuResponseDTO MenuModel = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMenus();
    }

    protected async Task OnOrgTypeChanged(int newType)
    {
        SelectedOrgTypeId = newType;
        await LoadMenus();
    }

    protected async Task LoadMenus()
    {
        IsLoading = true;
        StateHasChanged();
        try
        {
            var response = await Mediator.Send(new GetNavigationMenusQuery(SelectedOrgTypeId));
            if (response != null && response.IsSuccess)
            {
                MenusList = response.Data.ToList();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Error cargando menús dinámicos: " + ex.Message, Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void OpenNewDialog()
    {
        MenuModel = new NavigationMenuResponseDTO { OrganizationTypeId = (MedfarLabs.Core.Domain.Enums.TipoOrganizacion)SelectedOrgTypeId, OrderIndex = MenusList.Count + 1 };
        ShowDialog = true;
    }

    protected void OpenEditDialog(NavigationMenuResponseDTO dto)
    {
        MenuModel = new NavigationMenuResponseDTO 
        { 
            Id = dto.Id, 
            OrganizationTypeId = dto.OrganizationTypeId, 
            Title = dto.Title, 
            Route = dto.Route, 
            Icon = dto.Icon, 
            OrderIndex = dto.OrderIndex 
        };
        ShowDialog = true;
    }

    protected void CloseDialog() => ShowDialog = false;

    protected async Task SaveMenu()
    {
        IsSaving = true;
        try
        {
            if (MenuModel.Id == 0)
            {
                var cmd = new CreateMenuCommand 
                { 
                    OrganizationTypeId = MenuModel.OrganizationTypeId, 
                    Title = MenuModel.Title, 
                    Route = MenuModel.Route, 
                    Icon = MenuModel.Icon, 
                    OrderIndex = MenuModel.OrderIndex, 
                    CurrentUserId = 1 
                };
                var result = await Mediator.Send(cmd);
                if (result.IsSuccess) Snackbar.Add("Menú Agregado.", Severity.Success);
            }
            else
            {
                var cmd = new UpdateMenuCommand 
                { 
                    Id = MenuModel.Id, 
                    OrganizationTypeId = MenuModel.OrganizationTypeId, 
                    Title = MenuModel.Title, 
                    Route = MenuModel.Route, 
                    Icon = MenuModel.Icon, 
                    OrderIndex = MenuModel.OrderIndex, 
                    CurrentUserId = 1,
                    IsActive = true
                };
                var result = await Mediator.Send(cmd);
                if (result.IsSuccess) Snackbar.Add("Cambios Guardados.", Severity.Success);
            }
            
            CloseDialog();
            await LoadMenus();
        }
        catch (Exception ex)
        {
            Snackbar.Add("Error al guardar: " + ex.Message, Severity.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }
}
