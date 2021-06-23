using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class TimeOutDelegatingHandler : DelegatingHandler
    {
        // The default timeout value of the HttpClient
        private readonly TimeSpan timeOut = TimeSpan.FromSeconds(100);

        public TimeOutDelegatingHandler(TimeSpan timeOut) : base()
        {
            this.timeOut = timeOut;
        }

        public TimeOutDelegatingHandler(HttpMessageHandler innerHandler, TimeSpan timeOut) : base(innerHandler)
        {
            this.timeOut = timeOut;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using(var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                linkedCancellationTokenSource.CancelAfter(timeOut);
                try
                {
                    return await base.SendAsync(request, linkedCancellationTokenSource.Token);
                }
                catch(OperationCanceledException ex)
                {
                    if(!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("The request timed out.", ex);
                    }
                    throw;
                }
            }
        }
    }
}
