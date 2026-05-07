using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;

namespace MedFarLab.Pwa.Pages.Laboratory
{
    public partial class Samples : ComponentBase
    {
        [Inject] private ISender Mediator { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.Services.MedFarSnackbarService Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;

        public string SearchString { get; set; } = string.Empty;

        public class SampleRecord
        {
            public long Id { get; set; }
            public string Barcode { get; set; } = string.Empty;
            public string PatientName { get; set; } = string.Empty;
            public string SampleType { get; set; } = string.Empty;
            public string TestName { get; set; } = string.Empty;
            public string Status { get; set; } = "Pendiente";
        }

        public List<SampleRecord> MockSamples { get; set; } = new();
        protected bool IsLoading { get; set; } = true;

        private bool FilterFunc(SampleRecord sample, string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return true;
            return sample.Barcode.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                   sample.PatientName.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                   sample.TestName.Contains(term, System.StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadSamples();
        }

        private async Task LoadSamples()
        {
            IsLoading = true;
            try
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Queries.GetLabSamples.GetLabSamplesQuery());
                if (response != null)
                {
                    MockSamples = response.Select(x => new SampleRecord
                    {
                        Id = x.Id,
                        Barcode = x.Barcode,
                        PatientName = x.PatientName,
                        SampleType = x.SampleType,
                        TestName = x.TestName,
                        Status = x.Status
                    }).ToList();
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.ShowError("Error al cargar muestras desde el servidor.", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ReceiveSample(SampleRecord sample)
        {
            try
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Commands.ReceiveSample.ReceiveSampleCommand(sample.Id));
                
                if (response != null && response.IsSuccess)
                {
                    Snackbar.ShowSuccess(response.Message);
                    sample.Status = "Recolectada";
                    StateHasChanged();
                }
                else
                {
                    Snackbar.ShowError(response?.Message ?? "Error procesando solicitud");
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.ShowError("Fallo en la comunicación con el API.", ex.Message);
            }
        }

        protected async Task RejectSample(SampleRecord sample)
        {
            try
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Commands.RejectSample.RejectSampleCommand(sample.Id, "Rechazo manual desde UI"));
                
                if (response != null && response.IsSuccess)
                {
                    Snackbar.ShowWarning(response.Message);
                    sample.Status = "Rechazada";
                    StateHasChanged();
                }
                else
                {
                    Snackbar.ShowError(response?.Message ?? "Error procesando solicitud");
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.ShowError("Fallo en la comunicación con el API.", ex.Message);
            }
        }

        protected async Task OpenReceiveSampleDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<ReceiveSampleDialog>("Recibir Muestra", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is ReceiveSampleDialog.ReceiveSampleResult data)
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Commands.RegisterSample.RegisterSampleCommand(
                    data.PatientId,
                    data.Barcode,
                    data.SampleType,
                    data.Notes
                ));

                if (response != null && response.IsSuccess)
                {
                    Snackbar.ShowSuccess($"Muestra para {data.PatientName} registrada exitosamente");
                    await LoadSamples(); // Refrescar la grilla desde la BD
                }
                else
                {
                    Snackbar.ShowError("Error al registrar la muestra en la base de datos");
                }
            }
        }
    }
}
