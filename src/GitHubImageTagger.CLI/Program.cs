using GitHubImageTagger.Core;
using GitHubImageTagger.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace GitHubImageTagger.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ApplicationDbContext _context = new ApplicationDbContext();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(@"https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("githubimagetagger", "1.0"));

            List<Image> toAdd = GetListFromApi(client, @"repos/snipe/animated-gifs/contents/");
            _context.Images.AddRange(toAdd);

            _context.SaveChanges();
            Console.WriteLine("Added " + toAdd.Count + " images");

            // List file extensions
            //List<string> extensions = new List<string>();
            //foreach (var item in json.Where(j => j["type"].ToString() == "file"))
            //{
            //    string[] split = item["name"].ToString().Split('.');
            //    extensions.Add(split[split.Length - 1]);
            //}
            //foreach (var item in extensions.GroupBy(s => s))
            //{
            //    Console.WriteLine("Extension: " + item.Key);
            //}



            //foreach (var type in json.Where(j => j["type"].ToString() == "dir"))
            //{
            //    Console.WriteLine(type["name"].ToString());
            //}

            Console.ReadLine();
            _context.Dispose();
        }

        public static List<Image> GetListFromApi(HttpClient client, string requestPath)
        {
            var thing = client.GetAsync(requestPath).Result;

            if (thing.IsSuccessStatusCode)
            {
                string result = thing.Content.ReadAsStringAsync().Result;
                JToken json = JToken.Parse(result);

                return AddImages(json, client);
            }
            else
            {
                string reason = thing.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Didn't get success for subDir {requestPath}: {reason}");
                return new List<Image>();
            }
        }

        public static List<Image> AddImages(JToken json, HttpClient client)
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
                    List<Image> dirImages = GetListFromApi(client, item["url"].ToString().Substring(22));
                    result.AddRange(dirImages);
                }

                
            }

            _context.Dispose();
            return result;
        }
    }
}
