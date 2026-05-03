using MedfarLabs.Core.Domain.Common.Responses.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("tenant/{organizationId}")]
        public async Task<IActionResult> GetTenantDashboard(long organizationId)
        {
            // Simular carga analítica desde la base de datos usando Bogus de momento 
            // para devolver el WOW Effect al frontend. Eventualmente esto llamará al Dapper Service.
            // Retardo artificial para emular agregación B2B
            await Task.Delay(500);

            var random = new Random();
            var response = new TenantDashboardResponseDTO
            {
                TotalRevenueThisMonth = random.Next(15000, 85000),
                TotalAppointmentsToday = random.Next(5, 40),
                PatientsWaiting = random.Next(0, 15),
                DoctorsOnline = random.Next(1, 10),
                LatestPatients = new List<PatientShortInfo>
                {
                    new() { Name = "Carlos Mendoza", Status = "En Consulta", Time = "10:30 AM" },
                    new() { Name = "María Silva", Status = "Sala de Espera", Time = "10:45 AM" },
                    new() { Name = "Jorge Romero", Status = "Atendido", Time = "09:15 AM" }
                },
                RevenueTrend = new List<decimal> { 1200, 3500, 2800, 6000, 5000, 8500, 9200 }
            };

            return Ok(new BaseResponse<TenantDashboardResponseDTO> { Data = response, IsSuccess = true, Message = "OK" });
        }
        [HttpGet("master")]
        public async Task<IActionResult> GetMasterAdminDashboard()
        {
            await Task.Delay(500); // Emular procesamiento B2B pesado

            var response = new MasterDashboardResponseDTO
            {
                PendingInvoicesCount = 8,
                PendingSubscriptionsCount = 3,
                PendingOnboardings = 5,
                ActiveOrganizationsCount = 142,
                MonthlyRecurringRevenue = 45290.50m
            };

            return Ok(new BaseResponse<MasterDashboardResponseDTO> { Data = response, IsSuccess = true, Message = "OK" });
        }
    }

    public class TenantDashboardResponseDTO
    {
        public decimal TotalRevenueThisMonth { get; set; }
        public int TotalAppointmentsToday { get; set; }
        public int PatientsWaiting { get; set; }
        public int DoctorsOnline { get; set; }
        public List<PatientShortInfo> LatestPatients { get; set; } = new();
        public List<decimal> RevenueTrend { get; set; } = new();
    }

    public class PatientShortInfo
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public string Time { get; set; } = "";
    }

    public class MasterDashboardResponseDTO
    {
        public int PendingInvoicesCount { get; set; }
        public int PendingSubscriptionsCount { get; set; }
        public int PendingOnboardings { get; set; }
        public int ActiveOrganizationsCount { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
    }
}
