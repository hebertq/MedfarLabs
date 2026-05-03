using MedFarLab.Application.Features.Clinical.Commands.RegisterPatient;
using MedFarLab.Application.Features.Clinical.Models;
using MedFarLab.Application.Features.Identity.Commands.RegisterPerson;
using MedFarLab.Application.Features.Identity.Models;
using MedFarLab.Application.Features.Identity.Queries.ConsultarPersona;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Response;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Pwa.State;

namespace MedFarLab.Pwa.Pages.Patient.Registration;

public partial class PatientRegistration : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISender Mediator { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AppState AppState { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery]
    public long? PersonId { get; set; }

    private PatientRegistrationVM Model { get; set; } = new();
    private bool IsSending = false;
    private bool IsLoadingExistingIdentity = false;

    // Catálogos geográficos dinámicos
    private GeoCatalogResponseDTO? GeoCatalog;
    private IEnumerable<GeoCountryDTO> Countries => GeoCatalog?.Countries ?? new List<GeoCountryDTO>();
    
    private IEnumerable<GeoDepartmentDTO> AvailableDepartments => 
        Countries.FirstOrDefault(c => c.Id == Model.BirthCountryId)?.Departments ?? new List<GeoDepartmentDTO>();

    private IEnumerable<GeoMunicipalityDTO> AvailableMunicipalities => 
        AvailableDepartments.FirstOrDefault(d => d.Id == Model.DepartmentId)?.Municipalities ?? new List<GeoMunicipalityDTO>();

    private IEnumerable<GeoDistrictDTO> AvailableDistricts => 
        AvailableMunicipalities.FirstOrDefault(m => m.Id == Model.MunicipalityId)?.Districts ?? new List<GeoDistrictDTO>();

    private GeoCountryDTO? SelectedCountry => Countries.FirstOrDefault(c => c.Id == Model.BirthCountryId);

    private string Tier1Label => SelectedCountry?.Tier1Name ?? "Departamento";
    private string Tier2Label => SelectedCountry?.Tier2Name ?? "Municipio";
    private string Tier3Label => SelectedCountry?.Tier3Name ?? "Distrito";

    protected override async Task OnInitializedAsync()
    {
        // Init Geographics
        try
        {
            var geoResponse = await Mediator.Send(new MedFarLab.Application.Features.Identity.Queries.GetGeographicCatalog.GetGeographicCatalogQuery(null));
            if (geoResponse.IsSuccess && geoResponse.Data != null)
            {
                GeoCatalog = geoResponse.Data;
            }
        }
        catch(Exception ex)
        {
            Snackbar.Add("Error al cargar esquema geográfico: " + ex.Message, Severity.Warning);
        }

        // Default Country to Nicaragua (1 for now)
        if (Model.BirthCountryId == 0) Model.BirthCountryId = 1;

        if (PersonId.HasValue && PersonId.Value > 0)
        {
            IsLoadingExistingIdentity = true;
            try
            {
                var response = await Mediator.Send(new ConsultarPersonaQuery(PersonId.Value, AppState.OrganizationId));
                if (response.IsSuccess && response.Data != null)
                {
                    var data = response.Data;
                    Model.FirstName = data.FirstName ?? string.Empty;
                    Model.MiddleName = data.MiddleName ?? string.Empty;
                    Model.LastName = data.LastName ?? string.Empty;
                    Model.SecondLastName = data.SecondLastName ?? string.Empty;
                    Model.BirthDate = data.BirthDate;
                    Model.GenderId = data.GenderId;
                    Model.BirthCountryId = data.BirthCountryId;
                    
                    if (data.GeolocationId.HasValue && GeoCatalog != null)
                    {
                        var geoId = data.GeolocationId.Value;
                        // Resolve hierarchy backwards
                        foreach (var country in GeoCatalog.Countries)
                        {
                            foreach (var dept in country.Departments)
                            {
                                if (dept.Id == geoId) { Model.DepartmentId = dept.Id; break; }
                                foreach (var mun in dept.Municipalities)
                                {
                                    if (mun.Id == geoId) { Model.DepartmentId = dept.Id; Model.MunicipalityId = mun.Id; break; }
                                    foreach (var dist in mun.Districts)
                                    {
                                        if (dist.Id == geoId) { Model.DepartmentId = dept.Id; Model.MunicipalityId = mun.Id; Model.DistrictId = dist.Id; break; }
                                    }
                                    if (Model.MunicipalityId > 0) break;
                                }
                                if (Model.DepartmentId > 0) break;
                            }
                        }
                    }
                    Model.Address = data.Address ?? string.Empty;
                    Model.Email = data.Email ?? string.Empty;
                    Model.Phone = data.MobilePhone ?? string.Empty;
                    // DocumentId no está bajando porque BiometricHash es byte[], podrias parchearlo
                    Model.DocumentId = "N/A"; 
                    Snackbar.Add("Se han cargado los datos de la Persona existente.", Severity.Info);
                }
            }
            catch(Exception ex)
            {
                Snackbar.Add("Error al extraer identidad: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsLoadingExistingIdentity = false;
            }
        }
    }

    private void OnMunicipalityChanged(int municipalityId)
    {
        Model.MunicipalityId = municipalityId;
        Model.DistrictId = null;
    }

    private void OnDepartmentChanged(int departmentId)
    {
        Model.DepartmentId = departmentId;
        Model.MunicipalityId = 0; // Reset
        Model.DistrictId = null;
    }

    private void OnCountryChanged(int countryId)
    {
        Model.BirthCountryId = countryId;
        Model.DepartmentId = 0;
        Model.MunicipalityId = 0;
        Model.DistrictId = null;
    }

    private void GoBack()
    {
        NavManager.NavigateTo("/clinical/dashboard");
    }

    private async Task HandleRegistrationAsync()
    {
        IsSending = true;
        StateHasChanged();

        try
        {
            long finalPersonId = 0;

            if (PersonId.HasValue && PersonId.Value > 0)
            {
                // SALTAMOS EL REGISTRO DE IDENTIDAD, USAMOS LA EXISTENTE
                finalPersonId = PersonId.Value;
            }
            else
            {
                var personVm = new PersonVM
                {
                    FirstName = Model.FirstName,
                    MiddleName = Model.MiddleName,
                    LastName = Model.LastName,
                    SecondLastName = Model.SecondLastName,
                    BirthDate = Model.BirthDate,
                    GenderId = Model.GenderId == 0 ? 1 : Model.GenderId,
                    BirthCountryId = Model.BirthCountryId, 
                    GeolocationId = Model.DistrictId ?? (Model.MunicipalityId > 0 ? Model.MunicipalityId : (Model.DepartmentId > 0 ? Model.DepartmentId : null)),
                    Address = Model.Address,
                    Email = Model.Email,
                    MobilePhone = Model.Phone
                };

                var personResult = await Mediator.Send(new RegisterPersonCommand(personVm));

                if (personResult != null && personResult.IsSuccess && personResult.Data != null)
                {
                    finalPersonId = Convert.ToInt64(personResult.Data.ToString()); 
                }
                else
                {
                    Snackbar.Add("Fallo en la comunicación al registrar identidad poblacional.", Severity.Error);
                    return; // Detenemos porque falló el core
                }
            }

            // CREACIÓN DEL EXPEDIENTE (Paso #2)
            var patientVm = new PatientVM
            {
                PersonId = finalPersonId,
                OrganizationId = AppState.OrganizationId,
                InternalCode = $"EXP-{DateTime.Now.Ticks.ToString().Substring(8)}",
                AuditNotes = $"Alergias: {Model.Allergies} | Crónicos: {Model.ChronicConditions}"
            };

            var patientResult = await Mediator.Send(new RegisterPatientCommand(patientVm));

            if (patientResult.IsSuccess)
            {
                Snackbar.Add("El expediente clínico se ha generado exitosamente.", Severity.Success);
                NavManager.NavigateTo($"/patients/record/{patientResult.Data}");
            }
            else
            {
                Snackbar.Add("Fallo al registrar expediente clínico interconectado.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Snackbar.Add($"Error de conexión: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsSending = false;
            StateHasChanged();
        }
    }
}
