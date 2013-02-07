﻿using System.Threading.Tasks;
using System.Web.Mvc;
using RS2013.RefugeesUnited.Model;
using RS2013.RefugeesUnited.Model.RefugeesUnited;
using RS2013.RefugeesUnited.Services;
using RS2013.RefugeesUnited.Model;

namespace RS2013.RefugeesUnited.API.Controllers
{
	public class HomeController : Controller
	{
		private ISessionService SessionService { get; set; }
		private IRefugeesUnitedService RefugeesUnitedService { get; set; }

		public HomeController(ISessionService sessionService, IRefugeesUnitedService refugeesUnitedService)
		{
			SessionService = sessionService;
			RefugeesUnitedService = refugeesUnitedService;
		}

		public async Task<ActionResult> Index()
		{
			//var testUsername = await RefugeesUnitedService.GenerateUsername("Kaelanc", "Fouwelsc");
			//var testUserExists = await RefugeesUnitedService.UserExists("kaelanc.fouwelsc");
			//var testLogin = await RefugeesUnitedService.Login(null, "kaelanc.fouwelsc", "1234");
			//var testLogout = await RefugeesUnitedService.Logout("kaelanc.fouwelsc");
			//var testSearch = await RefugeesUnitedService.Search(testProfile);

			var testProfile = new Profile
			{
				username = "kaelanc.fouwelsc",
				givenName = "kaelanc",
				surName = "fouwelsc",
				password = "1234",
				otherInformation = "I like trains",
				lastSighting = "Test"
			};
			var testDevice = new Device {Number = "+447842073150"};

			//var testRegister = await RefugeesUnitedService.Register(testDevice, testProfile); <-- Problematic

			return Content("");
		}

		[HttpPost]
		public async Task<ActionResult> GlobalUSSD(
			string subscriber, string protocol,
			string sadsSmsMessage = null, string sadsListSegment = null,
			string sadsTextSegment = null, string sadsErrorMessage = null)
		{
			var connector = Request.Headers["X-Connector"]; // wap/sip/http/smpp
			var connectorDescription = Request.Headers["X-Connector-Description"];
			var userAgent = Request.UserAgent;
			var host = Request.Headers["Host"];
			var contentType = Request.ContentType;
			var ussdMessage = Request.Headers["WHOISD-USSD-MESSAGE"]; // USSD user reply
			var ussdAddress = Request.Headers["WHOISD-USSD-ADDRESS"];
			var cookie = Request.Cookies; // To identify session
			var locationCountry = Request.Headers["SADS-Location-Country-Name"];
			var locationCity = Request.Headers["SADS-Location-City-Name"];
			var locationCountryId = Request.Headers["SADS-Location-Country-Id"];
			var locationCityId = Request.Headers["SADS-Location-City-Id"];
			var locationVLR = Request.Headers["SADS-Location-VLR"];
			var locationLatitude = Request.Headers["SADS-Location-Latitude"];
			var locationLongitude = Request.Headers["SADS-Location-Longitude"];
			var locationTimezone = Request.Headers["SADS-Location-Timezone"];

			return Content("");
		}
	}
}
