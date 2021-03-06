﻿using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RS2013.RefugeesUnited.Model;
using RS2013.RefugeesUnited.Model.RefugeesUnited;

namespace RS2013.RefugeesUnited.Services.Impl
{
	public class RefugeesUnitedService : IRefugeesUnitedService
	{
		private readonly string _apiServerHost;
		private readonly string _apiServerUsername;
		private readonly string _apiServerPassword;

		public RefugeesUnitedService()
		{
			_apiServerHost = ConfigurationManager.AppSettings["ApiServerHost"];
			_apiServerUsername = ConfigurationManager.AppSettings["ApiServerUsername"];
			_apiServerPassword = ConfigurationManager.AppSettings["ApiServerPassword"];
		}

		public async Task<Profile> Register(Device device, Profile profile) //100%
		{
			var postParameters = new Dictionary<string, string>
				{
					{"username",				profile.username},
					{"password",				profile.password},
					{"primaryEmail",			profile.primaryEmail},
					{"referralId",				profile.referralId},
					{"genderId",				profile.genderId},
					{"givenName",				profile.givenName},
					{"surName",					profile.surName},
					{"birthCountryId",			profile.birthCountryId},
					{"owningMonitorProfileId",	profile.owningMonitorProfileId},
					{"owningPartnerProfileId",	profile.owningPartnerProfileId},
					{"dialCode",				profile.dialCode},
					{"cellPhone",				device.Number},
					{"clientCountryId",			profile.clientCountryId},
					{"lastSighting",			profile.lastSighting},
					{"nickName",				profile.nickName},
					{"otherInformation",		profile.otherInformation},
					{"otherName",				profile.otherName},
					{"tribe",					profile.tribe},
					{"alternateEmail",			profile.alternateEmail},
					{"homeTown",				profile.homeTown},
					{"preferredLanguageId",		profile.preferredLanguageId},
					{"userProfileStateId",		profile.userProfileStateId},
					{"actingUserProfileId",		profile.actingUserProfileId},
					{"assistingUserProfileId",	profile.assistingUserProfileId},
					{"sessionId",				profile.sessionId},
				};

			var y = await Api("profile/", postParameters: postParameters);

			var x = JsonConvert.DeserializeAnonymousType(y, new {profile = new {id = 0}});

			return await GetProfile(x.profile.id.ToString());

			//EG "\n{\"profile\":{\"id\":324865}}"
		}

		public async Task<Profile> GetProfile(string profileId) //75%
		{
			//todo Returns (500) Internal Server Error -> Needs auth?
			var y = await Api("profile/:" + profileId);
			var x = JsonConvert.DeserializeObject<Profile>(y);
			return x;

		}

		public async Task<IEnumerable<SearchResult>> Search(Profile profileToSearch) // 100%
		{
			var parameters = new Dictionary<string, string>();

			if (profileToSearch.surName != null) { parameters.Add("name", profileToSearch.givenName + " " + profileToSearch.surName); }
			else if (profileToSearch.surName == null) { parameters.Add("name", profileToSearch.givenName); }
			if (profileToSearch.genderId != null) { parameters.Add("genderId", profileToSearch.genderId); }
			if (profileToSearch.birthCountryId != null) { parameters.Add("countryOfBirthId", profileToSearch.birthCountryId); }
			if (profileToSearch.lastSighting != null) { parameters.Add("lastSighting", profileToSearch.lastSighting); }
			if (profileToSearch.otherInformation != null) { parameters.Add("otherInformation", profileToSearch.otherInformation); }

			var y = await Api("search/", parameters); //Raw data
			var x = JsonConvert.DeserializeObject<PagedResponse<SearchResult>>(y); //Processed
			return x.results;
		}

		public async Task<IEnumerable<SearchResult>> Search(string nameToSearch)  //100%
		{
			var parameters = new Dictionary<string, string>{{"name", nameToSearch}};

			var y = await Api("search/", parameters); //Raw data
			var x = JsonConvert.DeserializeObject<PagedResponse<SearchResult>>(y); //Processed
			return x.results;
		}

		public async Task<bool> Logout(string username) //99*%
		{
			//todo sort out returns
			var y = await Api("profile/logout/" + username); //raw input

			var a = JsonConvert.DeserializeObject(y);		//*Dont know what data it returns! Cant test as dont have account to logout
			if (a != null)									//Possible hacked workaround? See above^
			{
				return true;
			}
			return false;
		}

		public async Task<Profile> Login(Device device, string username, string password) //99*%
		{
			//todo sort out returns
			var parameters = new[]
				{
					new { Key = "password", Value = password}
				};
			var y = await Api("profile/login/" + username, parameters.ToDictionary(e => e.Key, e => e.Value));
			var x = JsonConvert.DeserializeObject<LoginResponse>(y);

			return null;			//Don't know what data returns when logged is SUCCESSFULL - Would need ProfileID to get the profile.

			//EG "\n{\"authenticated\":false,\"verificationRequired\":false,\"forcePasswordReset\":false}"
		}

		public async Task<bool> UserExists(string username) //100%
		{
			var y = await Api(("profile/exists/:" + username));
			var x = JsonConvert.DeserializeAnonymousType(y, new { exists = false });
			return x.exists;
		}

		public async Task<string> GenerateUsername(string givenName, string surName) //100%
		{
			var parameters =
				new[]
					{
						new { Key = "givenName", Value = givenName },
						new { Key = "surName", Value = surName }
					};

			var y = await Api("usernamegenerator/", parameters.ToDictionary(e => e.Key, e => e.Value));
			var x = JsonConvert.DeserializeAnonymousType(y, new { username = string.Empty });
			return x.username;
		}

		private async Task<string> Api(string apiAction, string data)
		{
			var url = (_apiServerHost + apiAction);

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.PreAuthenticate = true;
			request.Credentials = new NetworkCredential(_apiServerUsername, _apiServerPassword);
			request.ContentType = "application/json";
			request.Method = "GET";

			if (data != null)
			{
				request.Method = "POST";

				using (var sw = new StreamWriter(request.GetRequestStream()))
					await sw.WriteAsync(data);
			}

			var response = (HttpWebResponse) await request.GetResponseAsync();
			var responseStream = response.GetResponseStream();

			if (responseStream == null)
				return null;

			using (var sr = new StreamReader(responseStream))
				return await sr.ReadToEndAsync();
		}

		private async Task<string> Api(string apiAction, IEnumerable<KeyValuePair<string, string>> getParameters = null, IEnumerable<KeyValuePair<string, string>> postParameters = null)
		{
			string postData = null;

			if (postParameters != null)
			{
				postData = postParameters.Aggregate("", (str, i) => (str
					+ HttpUtility.UrlEncode(i.Key) + "="
					+ HttpUtility.UrlEncode(i.Value) + "&"));

				postData = postData.Substring(0, postData.Length - 1);
			}

			return await Api(apiAction, getParameters, postData);
		}

		private async Task<string> Api(string apiAction, IEnumerable<KeyValuePair<string, string>> getParameters, string postData)
		{
			var getData = string.Empty;

			if (getParameters != null)
			{
				getData = getParameters.Aggregate("?", (str, i) => (str
				  + HttpUtility.UrlEncode(i.Key) + "="
				  + HttpUtility.UrlEncode(i.Value) + "&"));

				getData = getData.Substring(0, getData.Length - 1);
			}

			return await Api(apiAction + getData, postData);
		}
	}
}
