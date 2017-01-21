using GitHubImageTagger.Core;
using GitHubImageTagger.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GitHubImageTagger.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {

            GetNewImages();

            Console.ReadLine();

        }

        private static async void GetNewImages()
        {
            ApplicationDbContext _context = new ApplicationDbContext();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(@"https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("githubimagetagger", "1.0"));

            List<Image> toAdd = await GetListFromApi(client, @"repos/snipe/animated-gifs/contents/");
            _context.Images.AddRange(toAdd);

            _context.SaveChanges();
            Console.WriteLine("Added " + toAdd.Count + " images");

            _context.Dispose();
        }

        public static async Task<List<Image>> GetListFromApi(HttpClient client, string requestPath)
        {
            Console.WriteLine($"Making HTTP call to {client.BaseAddress}{requestPath}");
            HttpResponseMessage responseMessage = await client.GetAsync(requestPath);

            if (responseMessage.IsSuccessStatusCode)
            {
                string result = await responseMessage.Content.ReadAsStringAsync();
                JToken json = JToken.Parse(result);

                return await AddImages(json, client);
            }
            else
            {
                string reason = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"Didn't get success for subDir {requestPath}: {reason}");
                return new List<Image>();
            }
        }

        public static async Task<List<Image>> AddImages(JToken json, HttpClient client)
        {
            ApplicationDbContext _context = new ApplicationDbContext();
            List<Image> result = new List<Image>();

            foreach (var item in json)
            {
                if (item["type"].ToString() == "file")
                {
                    string name = item["name"].ToString();

                    if (name.EndsWith("gif") || name.EndsWith("jpg"))
                    {
                        Image img = new Image();
                        img.FileName = name;
                        img.Path = item["path"].ToString();
                        img.Url = item["download_url"].ToString();

                        if (_context.Images.Where(j => j.Path == img.Path).Count() == 0)
                        {
                            result.Add(img);
                        }
                    }
                }
                else if (item["type"].ToString() == "dir")
                {
                    List<Image> dirImages = await GetListFromApi(client, item["url"].ToString().Substring(22));
                    result.AddRange(dirImages);
                }
            }

            _context.Dispose();
            return result;
        }
    }
}
