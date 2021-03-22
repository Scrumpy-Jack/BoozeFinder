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

            var goodBooze = GetBooze.scrapeLimitedAvailability();


			foreach (var booze in goodBooze)
			{
				var stores = GetBooze.WebRequest(booze.Value);

				stores.ForEach(store => {
					if(store.quantity > 0)
					{
						Console.WriteLine($"{booze.Key} has been found at store {store.address}");
					}
				
				});

			}


		}

		private static List<Store> WebRequest(string productId)
		{

			try
			{
				var client = new RestClient("https://www.abc.virginia.gov/webapi/inventory/storeNearby?storeNumber=82&productCode=" + productId + "&mileRadius=999&storeCount=5&buffer=1");
				var request = new RestRequest("", DataFormat.Json);

				var response = client.Get(request);
				var content = response.Content.Trim();

			    var product = JsonConvert.DeserializeObject<Root>(content).products[0];

				var stores = new List<Store>();
				stores.Add(product.storeInfo);
				stores.AddRange(product.nearbyStores);

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

				var test = new Regex("fdsfads");
				var match = test.Matches(product.InnerText);

				var split = product.InnerText.Split("|");
				limitedProducts.Add(split[0].Trim(), split[1].Trim().re);
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
