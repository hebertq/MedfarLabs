using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedFarLab.Application.Features.Reporting.DTOs;
using MedfarLabs.Core.Domain.Interfaces;
using MedfarLabs.Core.Domain.Interfaces.Repositories;
using MedfarLabs.Core.Domain.Models.Reporting;
using MedfarLabs.Core.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MedFarLab.Application.Features.Reporting.Queries.GetInvoiceReport
{
    public class GetInvoiceReportQueryHandler : IRequestHandler<GetInvoiceReportQuery, BaseResponse<ReportResponseDTO>>
    {
        private readonly IReportGenerator _reportGenerator;
        private readonly IServiceProvider _serviceProvider;

        public GetInvoiceReportQueryHandler(IReportGenerator reportGenerator, IServiceProvider serviceProvider)
        {
            _reportGenerator = reportGenerator;
            _serviceProvider = serviceProvider;
        }

        public async Task<BaseResponse<ReportResponseDTO>> Handle(GetInvoiceReportQuery request, CancellationToken cancellationToken)
        {
            try
            {
                InvoiceTemplateStyle templateName = request.Payload.PreferredTemplateName ?? InvoiceTemplateStyle.Classic; // Fallback por defecto en PWA
                
                // Si la UI no envió la plantilla local, intentamos buscarla en la base
                if (!request.Payload.PreferredTemplateName.HasValue)
                {
                    try 
                    {
                        // Intentamos resolver IUnitOfWork por si se ejecuta en el servidor (API)
                        var uow = _serviceProvider.GetService<IUnitOfWork>();
                        if (uow != null)
                        {
                            var orgs = await uow.Organizations.GetAllAsync();
                            var activeOrg = orgs.FirstOrDefault(x => x.IsActive);
                            templateName = activeOrg?.DefaultInvoiceTemplate ?? InvoiceTemplateStyle.Classic;
                        }
                    }
                    catch 
                    {
                        // Ignoramos si no hay acceso a DB (ej: PWA local)
                    }
                }

                var reportModel = new InvoiceReportModel 
                { 
                    InvoiceId = request.Payload.InvoiceId, 
                    IsTicketFormat = request.Payload.IsTicketFormat,
                    TemplateName = templateName
                };

                // Si logramos resolver la base de datos (Backend API environment)
                var uowForData = _serviceProvider.GetService<IUnitOfWork>();
                if (uowForData != null)
                {
                    var invoice = await uowForData.Invoices.GetByIdAsync(request.Payload.InvoiceId);
                    if (invoice != null)
                    {
                        reportModel.InvoiceNumber = invoice.InvoiceNumber;
                        reportModel.IssuedDate = invoice.CreatedAt;
                        reportModel.Subtotal = invoice.Subtotal;
                        reportModel.TaxAmount = invoice.TaxAmount;
                        reportModel.TotalAmount = invoice.TotalAmount;

                        var patient = await uowForData.Patients.GetByIdAsync(invoice.PatientId);
                        if (patient != null)
                        {
                            var person = await uowForData.Persons.GetByIdAsync(patient.PersonId);
                            reportModel.PatientName = person != null ? $"{person.FirstName} {person.LastName}" : "Consumidor Final";
                        }
                        
                        var items = await uowForData.InvoiceItems.GetReportItemsByInvoiceIdAsync(invoice.Id);
                        reportModel.Items = items.ToList();
                    }
                }

                var pdfBytes = await _reportGenerator.GenerateReportAsync(
                    request.Payload.IsTicketFormat ? "BillingTicket" : "BillingA4", 
                    reportModel);

                var responseData = new ReportResponseDTO
                {
                    FileName = $"FACTURA_{request.Payload.InvoiceId}.pdf",
                    MimeType = "application/pdf",
                    Base64Data = Convert.ToBase64String(pdfBytes)
                };

                return BaseResponse<ReportResponseDTO>.Success(responseData, "Reporte renderizado localmente en PWA.");
            }
            catch (Exception ex)
            {
                return BaseResponse<ReportResponseDTO>.Failure($"Error al procesar PDF en navegador: {ex.Message}");
            }
        }
    }
}
