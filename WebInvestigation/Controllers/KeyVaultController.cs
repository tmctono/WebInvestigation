// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TonoAspNetCore;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    [RequireHttps]
    public class KeyVaultController : Controller
    {
        static readonly HttpClient HTTP = new HttpClient();

        [HttpGet]
        public IActionResult Index()
        {
            return Index(new KeyVaultModel
            {
                Url = KeyVaultModel.Default.Url,
                ApplicationID = KeyVaultModel.Default.ApplicationID,
                ClientSecret = KeyVaultModel.Default.ClientSecret,
                Key = KeyVaultModel.Default.Key,
                SkipRequest = true,
            });
        }

        [HttpPost]
        public IActionResult Index(KeyVaultModel model)
        {
            Request.Headers.Add("X-WI-ApplicationID", model.ApplicationID);
            Request.Headers.Add("X-WI-ClientSecret", model.ClientSecret);

            // Apply input history from cookie
            var cu = ControllerUtils.From(this);
            cu.PersistInput("Url", model, KeyVaultModel.Default.Url);
            cu.PersistInput("ApplicationID", model, KeyVaultModel.Default.ApplicationID);
            cu.PersistInput("ClientSecret", model, KeyVaultModel.Default.ClientSecret);
            cu.PersistInput("Key", model, KeyVaultModel.Default.Key);

            if (!model.SkipRequest)
            {
                try
                {
                    model.Value = GetSecretAsync(model).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    model.ErrorMessage = ex.Message;
                }
            }
            model.SkipRequest = false;

            return View(model);
        }

        private async Task<string> GetSecretAsync(KeyVaultModel model)
        {
            var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync), HTTP);
            var secret = await client.GetSecretAsync(model.Url, model.Key);
            return secret.Value;
        }

        private async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var clientid = Request.Headers["X-WI-ApplicationID"].ToString();
            var clientsecret = Request.Headers["X-WI-ClientSecret"].ToString();

            var appCredentials = new ClientCredential(clientid, clientsecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, appCredentials);
            return result.AccessToken;
        }
    }
}
