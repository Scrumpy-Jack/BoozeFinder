using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using System.Text.RegularExpressions;

namespace BoozeFinder
{
	class GetBooze
	{
		const string baseUrl = "https://www.abc.virginia.gov/webapi";

		static void Main(string[] args)
		{
			var storesWithBooze = new Dictionary<string, string>();
			var goodBooze = GetBooze.scrapeLimitedAvailability();


			foreach (var booze in goodBooze)
			{
				var stores = GetBooze.WebRequest(booze.Value);

				stores.ForEach(store =>
				{
					if (store.quantity > 0)
					{
						var storeFoundText = $"{store.quantity} bottle(s) of {booze.Key} are available at {store.address}";
						var storeId = store.storeId;
						var storeInfo = $"{store.address}, Store ID:[{store.storeId}]";

						storesWithBooze.Add(storeFoundText, storeInfo);
					}

				});

			}

			var sortedStores = storesWithBooze.GroupBy(x => x.Value)
			.ToDictionary(x => x.Key, x => x.Select(i => i.Key).ToList());

			var formatedStores = JsonConvert.SerializeObject(sortedStores);

			Console.WriteLine(formatedStores);
		}

		private static List<Store> WebRequest(string productId)
		{

			try
			{

				// TODO: Take peramater for storeNumber and mileRadius 

				var client = new RestClient("https://www.abc.virginia.gov/webapi/inventory/storeNearby?storeNumber=82&productCode=" + productId + "&mileRadius=30&storeCount=5&buffer=1");
				var request = new RestRequest("", DataFormat.Json);

				var response = client.Get(request);
				var content = response.Content.Trim();

				var product = JsonConvert.DeserializeObject<Root>(content).products[0];

				var stores = new List<Store>();
				stores.Add(product.storeInfo);
				if (product.nearbyStores != null)
				{
					stores.AddRange(product.nearbyStores);

				}

				return stores;

			}
			catch (Exception)
			{

				throw;
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

				var regex = new Regex(@"(?<productName>.*)\W*\|\W*(?<productId>[\d]+)");
				var productMatch = regex.Matches(product.InnerText)[0];

				limitedProducts.Add(productMatch.Groups["productName"].Value, productMatch.Groups["productId"].Value);
			}

			return limitedProducts;

		}


		private class PhoneNumber
		{
			public string AreaCode { get; set; }
			public string Prefix { get; set; }
			public string LineNumber { get; set; }
			public string FormattedPhoneNumber { get; set; }
		}

		private class Product
		{
			public string productId { get; set; }
			public Store storeInfo { get; set; }
			public List<Store> nearbyStores { get; set; }
		}

		private class Root
		{
			public List<Product> products { get; set; }
		}


		private class Store
		{
			public int storeId { get; set; }
			public int quantity { get; set; }
			public double distance { get; set; }
			public double latitude { get; set; }
			public double longitude { get; set; }
			public string address { get; set; }
			public PhoneNumber PhoneNumber { get; set; }
			public string url { get; set; }
			public string address1 { get; set; }
			public string address2 { get; set; }
			public string city { get; set; }
			public string state { get; set; }
			public string zip { get; set; }
			public string hours { get; set; }
			public string shoppingCenter { get; set; }
			public int closeForEcommerce { get; set; }
			public bool counterService { get; set; }
			public bool wholesale { get; set; }

		}

	}
}
