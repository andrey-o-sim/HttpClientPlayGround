using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class RetryPolicyDelegatingHandler : DelegatingHandler
    {
        private readonly int _maxAmountOfRetries;

        public RetryPolicyDelegatingHandler(int maxAmountOfRetries)
            : base()
        {
            _maxAmountOfRetries = maxAmountOfRetries;
        }

        // This constructor is necessary for cases when we are going to create an object of this handler manually. new HttpClient(new RetryPolicyDelegatingHandler(handler,2))
        public RetryPolicyDelegatingHandler(HttpMessageHandler innerHandler, int maxAmountOfRetries)
           : base(innerHandler)
        {
            _maxAmountOfRetries = maxAmountOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < _maxAmountOfRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }
            return response;
        }
    }
}
