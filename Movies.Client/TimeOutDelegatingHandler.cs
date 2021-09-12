using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class TimeOutDelegatingHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(100);

        public TimeOutDelegatingHandler(TimeSpan timeOut)
        : base()
        {
            _timeout = timeOut;
        }

        public TimeOutDelegatingHandler(HttpMessageHandler innerHandler, TimeSpan timeOut)
            : base(innerHandler)
        {
            _timeout = timeOut;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCancellationTokenSource.CancelAfter(_timeout);
            try
            {
                return await base.SendAsync(request, linkedCancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("The request timed out.", ex);
                }
                throw;
            }
        }
    }
}
