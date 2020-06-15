using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebInvestigation.Models
{
    public abstract class ResponseBase
    {
        public string Result { get; set; }
        public string HttpStatusCode { get; set; }

        public virtual void Set(Task<HttpResponseMessage> task)
        {
            var res = task.ConfigureAwait(false).GetAwaiter().GetResult();
            HttpStatusCode = $"{(int)res.StatusCode}({res.StatusCode})";
            Result = res.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
