using MedFarLab.Api.IntegrationTests.Common;
using MedfarLabs.Core.Domain.Const;
using System.Net.Http.Json;
using System.Net;
using System.Reflection;

namespace MedFarLab.Api.IntegrationTests.Common
{
    [Collection("ApiTests")]
    public class CommonSmokeTests : BaseIntegrationTest
    {
        public CommonSmokeTests(CustomWebApplicationFactory factory) : base(factory) { }

        public static IEnumerable<object[]> GetActionCodes()
        {
            var type = typeof(AppAction.Common);
            if (type == null) return Array.Empty<object[]>();

            var list = new List<object[]>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(int))
                {
                    var val = field.GetRawConstantValue(); if (val != null) { list.Add(new object[] { (int)val }); }
                }
            }
            return list;
        }

        [Theory]
        [MemberData(nameof(GetActionCodes))]
        public async Task AllActions_Post_ShouldReturnSuccessStatusCode(int actionCode)
        {
            var response = await _client.PostAsJsonAsync($"api/Common/{actionCode}", new { });
            Assert.True(response.IsSuccessStatusCode, $"Ruta POST api/Common/{actionCode} fallo con status {response.StatusCode}");
        }

        [Theory]
        [MemberData(nameof(GetActionCodes))]
        public async Task AllActions_Get_ShouldReturnSuccessStatusCode(int actionCode)
        {
            var response = await _client.GetAsync($"/api/Common/{actionCode}");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Ruta GET api/Common/{actionCode} fallo con status {response.StatusCode}. Body: {body}");
            }
        }
    }
}
