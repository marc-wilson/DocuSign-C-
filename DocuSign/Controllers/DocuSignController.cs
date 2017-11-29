using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
//using System.Web.Mvc;

namespace DocuSign.Controllers
{
    public class DocuSignController : ApiController
    {
        // GET: DocuSign
		[HttpGet()]
		[Route("Test")]
        public JsonResult<EnvelopeSummary> Test()
        {
			string userId = "ba2a2890-7b77-476c-9d75-52266bb879fa"; // use your userId (guid), not email address
			string oauthBasePath = "account-d.docusign.com";
			string integratorKey = "cd7de09e-6e24-4970-b909-624e875b8f68";
			string privateKeyFilename = @"C:\Users\Marc\Source\Repos\DocuSign\DocuSign\PrivateKey.txt";
			int expiresInHours = 1;
			string host = "https://demo.docusign.net/restapi";

			string accountId = string.Empty;

			ApiClient apiClient = new ApiClient(host);
			apiClient.ConfigureJwtAuthorizationFlow(integratorKey, userId, oauthBasePath, privateKeyFilename, expiresInHours);

			/////////////////////////////////////////////////////////////////
			// STEP 1: LOGIN API        
			/////////////////////////////////////////////////////////////////
			AuthenticationApi authApi = new AuthenticationApi(apiClient.Configuration);
			LoginInformation loginInfo = authApi.Login();

			// find the default account for this user
			foreach (LoginAccount loginAcct in loginInfo.LoginAccounts)
			{
				if (loginAcct.IsDefault == "true")
				{
					accountId = loginAcct.AccountId;

					string[] separatingStrings = { "/v2" };

					// Update ApiClient with the new base url from login call
					apiClient = new ApiClient(loginAcct.BaseUrl.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)[0]);
					break;
				}
			}

			/////////////////////////////////////////////////////////////////
			// STEP 2: CREATE ENVELOPE API        
			/////////////////////////////////////////////////////////////////				

			EnvelopeDefinition envDef = new EnvelopeDefinition();
			envDef.EmailSubject = "[DocuSign C# SDK] - Please sign this doc";

			// assign recipient to template role by setting name, email, and role name.  Note that the
			// template role name must match the placeholder role name saved in your account template.  
			TemplateRole tRole = new TemplateRole();
			tRole.Email = "docusignmarc@gmail.com";
			tRole.Name = "Mar";
			tRole.RoleName = "SCP";
			List<TemplateRole> rolesList = new List<TemplateRole>() { tRole };

			// add the role to the envelope and assign valid templateId from your account
			envDef.TemplateRoles = rolesList;
			envDef.TemplateId = "";

			// set envelope status to "sent" to immediately send the signature request
			envDef.Status = "sent";

			// |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
			EnvelopesApi envelopesApi = new EnvelopesApi(apiClient.Configuration);
			EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
			return Json(envelopeSummary);
		}
    }
}