using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace WebInvestigation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DummyController : ControllerBase
    {
        private string make(string method)
        {
            var ret = new StringBuilder();
            ret.AppendLine($"Method = {method}");
            ret.AppendLine($"Request.Scheme = {Request.Scheme}");
            ret.AppendLine($"Request.Host = {Request.Host}");
            var sr = new StreamReader(Request.BodyReader.AsStream());
            ret.AppendLine($"Request.Body");
            ret.AppendLine(sr.ReadToEnd());
            return ret.ToString();
        }
        [HttpGet]
        public string Get()
        {
            return make("Get");
        }

        [HttpPost]
        public string Post()
        {
            return make("Post");
        }
        [HttpDelete]
        public string Delete()
        {
            return make("Delete");
        }
        [HttpHead]
        public string Head()
        {
            return make("Head");
        }
        [HttpOptions]
        public string Options()
        {
            return make("Options");
        }
        [HttpPatch]
        public string Patch()
        {
            return make("Patch");
        }
        [HttpPut]
        public string Put()
        {
            return make("Put");
        }
    }
}
