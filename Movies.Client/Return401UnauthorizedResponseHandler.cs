using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class Return401UnauthorizedResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return Task.FromResult(response);
        }
    }
}
