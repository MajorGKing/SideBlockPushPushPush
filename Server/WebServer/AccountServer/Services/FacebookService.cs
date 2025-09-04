using AccountServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AccountServer.Services
{
	public class FacebookService
	{
		HttpClient _httpClient;

		// {app_id}|{app_secret} 
		readonly string _accessToken = "GG|540435154335782|9oGyj8cigWaMXoppGFjE8faw4mI"; // TODO Secret

		public FacebookService()
		{
			_httpClient = new HttpClient() { BaseAddress = new Uri("https://graph.facebook.com/") };
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<FacebookTokenData?> GetUserTokenData(string inputToken)
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"debug_token?input_token={inputToken}&access_token={_accessToken}");

			if (!response.IsSuccessStatusCode)
				return null;

			string resultStr = await response.Content.ReadAsStringAsync();

			FacebookResponseJsonData? result = JsonConvert.DeserializeObject<FacebookResponseJsonData>(resultStr);
			if (result == null)
				return null;

			return result.data;
		}
	}
}
