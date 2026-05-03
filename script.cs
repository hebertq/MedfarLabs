using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MedfarLabs.Core.Application.Features.Laboratory.Interfaces;
using MedfarLabs.Core.Domain.Interfaces;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using SharedFakers.Fakers.Identity;

namespace Script {
    public class Runner {
        public static async Task Main() {
            try {
                var services = IntegrationTests.IntegrationTestBase.CreateServicesForScript();
                var uow = services.GetRequiredService<IUnitOfWork>();
                var config = new MedfarLabs.Core.Domain.Entities.Laboratory.OrgLabExamConfig { OrganizationId = 1, TemplateId = 1, ServiceId = 1 };
                var id = await uow.OrgLabConfigs.AddAsync(config);
                Console.WriteLine(""ADDED ID: "" + id);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
