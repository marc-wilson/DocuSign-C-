using DocuSign.eSign.Api;
using DSC = DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.IO;
using MVC = System.Web.Mvc;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace DocuSign.Controllers
{
	public class DocuSignController : ApiController
	{
		public class TestModel
		{
			public string test { get; set; }
		}
        // GET: DocuSign
		[HttpGet()]
		[Route("send")]
        public JsonResult<ViewUrl> Test()
        {
			string username = "";
			string password = "";
			string integratorKey = "";
			// initialize client for desired environment (for production change to www)
			DSC.ApiClient apiClient = new DSC.ApiClient("https://demo.docusign.net/restapi");
			DSC.Configuration.Default.ApiClient = apiClient;

			// configure 'X-DocuSign-Authentication' header
			string authHeader = "{\"Username\":\"" + username + "\", \"Password\":\"" + password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
			DSC.Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

			// we will retrieve this from the login API call
			string accountId = null;

			/////////////////////////////////////////////////////////////////
			// STEP 1: LOGIN API        
			/////////////////////////////////////////////////////////////////

			// login call is available in the authentication api 
			AuthenticationApi authApi = new AuthenticationApi();
			LoginInformation loginInfo = authApi.Login();

			// parse the first account ID that is returned (user might belong to multiple accounts)
			accountId = loginInfo.LoginAccounts[0].AccountId;

			// Update ApiClient with the new base url from login call
			string[] separatingStrings = { "/v2" };
			apiClient = new DSC.ApiClient(loginInfo.LoginAccounts[0].BaseUrl.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)[0]);

			/////////////////////////////////////////////////////////////////
			// STEP 2: CREATE ENVELOPE API        
			/////////////////////////////////////////////////////////////////

			// create a new envelope which we will use to send the signature request
			EnvelopeDefinition envDef = new EnvelopeDefinition();
			envDef.EmailSubject = "[DocuSign C# SDK] - Sample Signature Request";

			// provide a valid template ID from a template in your account
			// envDef.TemplateId = "7decf5f8-b499-4b51-8c3c-7c3a2702eefa";
			Document doc = new Document();
			Signer signer = new Signer();
			Byte[] bytes = File.ReadAllBytes(@"");
			String file = Convert.ToBase64String(bytes);
			doc.DocumentBase64 = file;
			doc.DocumentId = "1";
			doc.Name = "Mutual_NDA.pdf";
			envDef.Documents = new List<Document>();
			envDef.Documents.Add(doc);
			signer.Email = username;
			signer.Name = "Marc";
			signer.RecipientId = "1";
			signer.ClientUserId = "123";
			envDef.Recipients = new Recipients();
			envDef.Recipients.Signers = new List<Signer>();
			envDef.Recipients.Signers.Add(signer);


			// assign recipient to template role by setting name, email, and role name.  Note that the
			// template role name must match the placeholder role name saved in your account template.  
			TemplateRole tRole = new TemplateRole();
			tRole.Email = "";
			tRole.Name = "Marc";
			tRole.RoleName = "Signer";

			// add the roles list with the our single role to the envelope
			List<TemplateRole> rolesList = new List<TemplateRole>() { tRole };
			envDef.TemplateRoles = rolesList;

			// set envelope status to "sent" to immediately send the signature request
			envDef.Status = "sent";

			// |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
			EnvelopesApi envelopesApi = new EnvelopesApi();
			EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
			ViewUrl url = GetViewUrl(envelopesApi, envDef, envelopeSummary);
			return Json(url);
		}

		[HttpGet()]
		[Route("callback")]
		public void ActionHandler()
		{
			var uri = this.Request.RequestUri;

			var t = 1 + 1;
			if (t == 2)
			{

			}
		}

		public ViewUrl GetViewUrl(EnvelopesApi envelopesApi, EnvelopeDefinition envelopeDefinition, EnvelopeSummary envelopeSummary)
		{
			RecipientViewRequest viewOptions = new RecipientViewRequest()
			{
				ReturnUrl = "http://localhost:49899/Finished",
				ClientUserId = "123",
				AuthenticationMethod = "email",
				UserName = envelopeDefinition.Recipients.Signers[0].Name,
				Email = envelopeDefinition.Recipients.Signers[0].Email
			};

			ViewUrl url = envelopesApi.CreateRecipientView("4003313", envelopeSummary.EnvelopeId, viewOptions);
			return url;
		}

		[HttpPost()]
		[Route("pdf")]
		public JsonResult<ViewUrl> GeneratePDF([FromBody]TestModel test)
		{

			PdfDocument pdf = PdfGenerator.GeneratePdf(test.test, PdfSharp.PageSize.A4);
			string base64 = string.Empty;
			ViewUrl ret = new ViewUrl();
			try
			{
				pdf.Save(@"");
				Byte[] bytes = File.ReadAllBytes(@"");
				base64 = Convert.ToBase64String(bytes);
				ViewUrl url = SendToDocuSign(base64);
				return Json(url);
			}
			catch (Exception e)
			{
				
			}

			return null;
			
			
		}

		public ViewUrl SendToDocuSign(string documentBase64)
		{
			string username = "";
			string password = "";
			string integratorKey = "";
			// initialize client for desired environment (for production change to www)
			DSC.ApiClient apiClient = new DSC.ApiClient("https://demo.docusign.net/restapi");
			DSC.Configuration.Default.ApiClient = apiClient;

			// configure 'X-DocuSign-Authentication' header
			string authHeader = "{\"Username\":\"" + username + "\", \"Password\":\"" + password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
			DSC.Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

			// we will retrieve this from the login API call
			string accountId = null;

			/////////////////////////////////////////////////////////////////
			// STEP 1: LOGIN API        
			/////////////////////////////////////////////////////////////////

			// login call is available in the authentication api 
			AuthenticationApi authApi = new AuthenticationApi();
			LoginInformation loginInfo = authApi.Login();

			// parse the first account ID that is returned (user might belong to multiple accounts)
			accountId = loginInfo.LoginAccounts[0].AccountId;

			// Update ApiClient with the new base url from login call
			string[] separatingStrings = { "/v2" };
			apiClient = new DSC.ApiClient(loginInfo.LoginAccounts[0].BaseUrl.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)[0]);

			/////////////////////////////////////////////////////////////////
			// STEP 2: CREATE ENVELOPE API        
			/////////////////////////////////////////////////////////////////

			// create a new envelope which we will use to send the signature request
			EnvelopeDefinition envDef = new EnvelopeDefinition();
			envDef.EmailSubject = "[DocuSign C# SDK] - Sample Signature Request";

			// provide a valid template ID from a template in your account
			// envDef.TemplateId = "7decf5f8-b499-4b51-8c3c-7c3a2702eefa";
			Document doc = new Document();
			Signer signer = new Signer();
			doc.DocumentBase64 = documentBase64;
			doc.DocumentId = "1";
			doc.Name = "Mutual_NDA.pdf";
			envDef.Documents = new List<Document>();
			envDef.Documents.Add(doc);
			signer.Email = username;
			signer.Name = "Marc";
			signer.RecipientId = "1";
			signer.ClientUserId = "123";
			envDef.Recipients = new Recipients();
			envDef.Recipients.Signers = new List<Signer>();
			envDef.Recipients.Signers.Add(signer);


			// assign recipient to template role by setting name, email, and role name.  Note that the
			// template role name must match the placeholder role name saved in your account template.  
			TemplateRole tRole = new TemplateRole();
			tRole.Email = "";
			tRole.Name = "Marc";
			tRole.RoleName = "Signer";

			// add the roles list with the our single role to the envelope
			List<TemplateRole> rolesList = new List<TemplateRole>() { tRole };
			envDef.TemplateRoles = rolesList;

			// set envelope status to "sent" to immediately send the signature request
			envDef.Status = "sent";

			// |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
			EnvelopesApi envelopesApi = new EnvelopesApi();
			EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
			ViewUrl url = GetViewUrl(envelopesApi, envDef, envelopeSummary);
			return url;
		}
    }
}