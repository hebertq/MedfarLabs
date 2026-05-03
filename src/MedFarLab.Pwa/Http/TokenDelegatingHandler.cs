using System.Net.Http.Headers;
using MedFarLab.Pwa.State;
using Microsoft.JSInterop;

namespace MedFarLab.Pwa.Http
{
    public class TokenDelegatingHandler : DelegatingHandler
    {
        private readonly AppState _appState;
        private readonly IJSRuntime _jsRuntime;

        public TokenDelegatingHandler(AppState appState, IJSRuntime jsRuntime)
        {
            _appState = appState;
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Intentar recuperar del estado en RAM primero
            var token = _appState.SessionToken;
            var userId = _appState.UserId.ToString();
            var branchId = _appState.BranchId.ToString();

            // Fallback a LocalStorage si hay un F5 (Refresh)
            if (string.IsNullOrEmpty(token))
            {
                try {
                    token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", cancellationToken, "medfarlab_token");
                    var uid = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", cancellationToken, "medfarlab_userId");
                    var bid = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", cancellationToken, "medfarlab_branchId");
                    
                    if (!string.IsNullOrEmpty(token)) {
                        _appState.SessionToken = token;
                        _appState.UserId = long.Parse(uid ?? "0");
                        _appState.BranchId = long.Parse(bid ?? "0");
                        userId = uid;
                        branchId = bid;
                    }
                } catch { } // JSInterop might fail if prerendering or unauthorized
            }

            if (!string.IsNullOrEmpty(token) && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/"))
            {
                // Inyectamos las cabeceras personalizadas según lo esperado por el Middleware del Backend
                request.Headers.Add("X-Auth-Token", token);
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("X-Branch-Id", branchId);
            }

            // Inyectar Idempotency-Key para mutaciones (POST, PUT, DELETE, PATCH)
            if (request.Method == HttpMethod.Post || 
                request.Method == HttpMethod.Put || 
                request.Method == HttpMethod.Delete || 
                request.Method.Method == "PATCH")
            {
                request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
