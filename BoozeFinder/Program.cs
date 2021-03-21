using System;
using System.Collections.Generic;
using System.Xml;
using HtmlAgilityPack;
using RestSharp;

namespace BoozeFinder
{
	class GetBooze
	{
		const string baseUrl = "https://www.abc.virginia.gov/webapi";
		static void Main(string[] args)
		{

			var goodBooze = GetBooze.scrapeLimitedAvailability();


			
		}

		private static string WebRequest(string url, DataFormat dataFormant)
		{

			try
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("url", dataFormant);

				var response = client.Get(request);

				var content = response.Content;

				return content;

				//Console.WriteLine(content);

				//var xmlDoc = new XmlDocument();
				//xmlDoc.LoadXml(content);



			}
			catch (Exception)
			{

				throw new Exception("dfafads");
			}
		}

		private static Dictionary<String, String> scrapeLimitedAvailability()
		{
			var limitedProducts = new Dictionary<string, string>();

			var client = new RestClient("https://www.abc.virginia.gov/products/limited-availability");
			var request = new RestRequest("", DataFormat.Xml);

			var response = client.Get(request);
			var content = response.Content;

			var doc = new HtmlDocument();
			doc.LoadHtml(content);

			var limitedProductsList = doc.DocumentNode.SelectNodes("//div/h2[normalize-space()='For In-Store Purchase Only']//ancestor::div[@class = 'bottom']//li");

			foreach (var product in limitedProductsList)
			{
				var split = product.InnerText.Split("|");
				limitedProducts.Add(split[0].Trim(), split[1].Trim());
			}

			return limitedProducts;

		}

	}
}
