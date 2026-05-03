using System.Text.Json.Serialization;
using MedFarLab.Application.Features.Identity.Commands.RegisterAppUser;
using MedFarLab.Application.Features.Identity.Commands.RegisterBranch;
using MedFarLab.Application.Features.Identity.Commands.RegisterFacility;
using MedFarLab.Application.Features.Identity.Commands.RegisterOrganization;
using MedFarLab.Application.Features.Identity.Commands.RegisterPerson;
using MedFarLab.Application.Features.Identity.Commands.RegisterUser;
using MedFarLab.Application.Features.Identity.Commands.UpdateOrganizationConfig;
using MedFarLab.Application.Features.Identity.Queries.SearchPersons;
using MedFarLab.Application.Features.Inventory.Commands.RegisterService;
using MedFarLab.Application.Features.Laboratory.Commands.UpdateLabOrderStatus;
using MedFarLab.Application.Features.Laboratory.Commands.RegisterLabOrder;
using MedFarLab.Application.Features.Laboratory.Commands.RegisterLabResult;
using MedFarLab.Application.Features.Patient.Commands;
using MedFarLab.Application.Features.Pharmacy.Commands.RestockMedication;
using MedFarLab.Application.Features.Security.Commands.RegisterRoleGroup;
using MedFarLab.Application.Features.Settings.Commands.UpdateOrganizationTemplates;

namespace MedFarLab.Application.Common.Serialization
{
    [JsonSerializable(typeof(RegisterAppUserCommand))]
    [JsonSerializable(typeof(RegisterBranchCommand))]
    [JsonSerializable(typeof(RegisterFacilityCommand))]
    [JsonSerializable(typeof(RegisterOrganizationCommand))]
    [JsonSerializable(typeof(RegisterPersonCommand))]
    [JsonSerializable(typeof(RegisterUserCommand))]
    [JsonSerializable(typeof(UpdateOrganizationConfigCommand))]
    [JsonSerializable(typeof(SearchPersonsCommand))]
    [JsonSerializable(typeof(RegisterMedicalServiceCommand))]
    [JsonSerializable(typeof(UpdateLabOrderStatusCommand))]
    [JsonSerializable(typeof(RegisterLabOrderCommand))]
    [JsonSerializable(typeof(RegisterLabResultCommand))]
    [JsonSerializable(typeof(CreatePatientCommand))]
    [JsonSerializable(typeof(RestockMedicationCommand))]
    [JsonSerializable(typeof(RegisterRoleGroupCommand))]
    [JsonSerializable(typeof(UpdateOrganizationTemplatesCommand))]
    public partial class PwaJsonContext : JsonSerializerContext
    {
    }
}
