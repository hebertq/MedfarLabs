using System;
using System.Collections.Generic;

namespace MedFarLab.Application.Features.Billing.DTOs
{
    public class InvoiceDto
    {
        public long InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public long PatientId { get; set; } // Identificador Real del Paciente en BD
        public string? PatientName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public string? Status { get; set; }
        public List<InvoiceLineItemDto> Items { get; set; } = new();
    }

    public class InvoiceLineItemDto
    {
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
