using MedFarLab.Api.IntegrationTests.Common;
using MedfarLabs.Core.Domain.Const;
using System.Net.Http.Json;
using System.Net;
using System.Reflection;

namespace MedFarLab.Api.IntegrationTests.Billing
{
    [Collection("ApiTests")]
    public class BillingSmokeTests : BaseIntegrationTest
    {
        public BillingSmokeTests(CustomWebApplicationFactory factory) : base(factory) { }

        public static IEnumerable<object[]> GetActionCodes()
        {
            var type = typeof(AppAction.Billling);
            if (type == null) return Array.Empty<object[]>();

            var list = new List<object[]>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(int))
                {
                    var value = field.GetRawConstantValue();
                    if (value is int intValue)
                    {
                        list.Add(new object[] { intValue });
                    }
                }
            }
            return list;
        }

        [Theory]
        [MemberData(nameof(GetActionCodes))]
        public async Task AllActions_Post_ShouldReturnSuccessStatusCode(int actionCode)
        {
            var response = await _client.PostAsJsonAsync($"api/Billing/{actionCode}", new { });
            Assert.True(response.IsSuccessStatusCode, $"Ruta POST api/Billing/{actionCode} fallo con status {response.StatusCode}");
        }

        [Theory]
        [MemberData(nameof(GetActionCodes))]
        public async Task AllActions_Get_ShouldReturnSuccessStatusCode(int actionCode)
        {
            var response = await _client.GetAsync($"api/Billing/{actionCode}");
            Assert.True(response.IsSuccessStatusCode, $"Ruta GET api/Billing/{actionCode} fallo con status {response.StatusCode}");
        }
    }
}
