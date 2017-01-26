using GitHubImageTagger.Core;
using GitHubImageTagger.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GitHubImageTagger.CLI
{
    public class Program
    {
        static IConfigurationRoot Configuration;
        static HttpClient GitHubApiHttpClient = new HttpClient();
        static ApplicationDbContext ImageTaggerContext = new ApplicationDbContext();

        public static void Main(string[] args)
        {
            // Set GitHub API HttpClient required headers
            GitHubApiHttpClient.BaseAddress = new Uri(@"https://api.github.com/");
            GitHubApiHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            GitHubApiHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("githubimagetagger", "1.0"));

            // Get Configuration
            Console.WriteLine("Loading Configuration");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json");
            Configuration = configBuilder.Build();

            Console.WriteLine("Running Database Migrations");
            ImageTaggerContext.Database.Migrate();

            // Print Menu
            int selection;
            bool parsed;
            do
            {
                Console.Clear();
                Console.WriteLine("ImageTagger CLI\r\n=========================");
                Console.WriteLine("1. Get new images");
                Console.WriteLine("0. Exit");
                Console.Write("Enter your selection: ");
                string input = Console.ReadLine();
                parsed = int.TryParse(input, out selection);

                if (parsed)
                {
                    switch (selection)
                    {
                        case 0:
                            return;
                        case 1:
                            GetNewImages().GetAwaiter().GetResult();
                            break;
                        default:
                            return;
                    }

                    Console.Write("\r\nPress any key to continue...");
                    Console.ReadKey();
                }
            } while (!parsed || selection != 0);

            GitHubApiHttpClient.Dispose();
        }

        // Turns out that this isn't necessary.
        private static async Task GetGitHubAuthorization()
        {
            Console.WriteLine("\r\n-> Get GitHub API authorization\r\nMay not work if you have 2FA enabled on your GitHub account!");
            Console.Write("Enter GitHub username: ");
            string ghUsername = Console.ReadLine();
            Console.Write("Enter GitHub password: ");
            string ghPassword = Console.ReadLine();

            var authByteArray = Encoding.ASCII.GetBytes(ghUsername + ":" + ghPassword);
            var authString = Convert.ToBase64String(authByteArray);
            GitHubApiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

            string jsonAuthRequest = "{ \"client_secret\": \"" + Configuration["GitHubClientSecret"].ToString() + "\", \"scopes\": [ \"public_repo\" ], \"note\": \"ImageTagger CLI\" }";
            StringContent reqContent = new StringContent(jsonAuthRequest);
            //var result = GitHubApiHttpClient.PostAsync("/authorizations", reqContent).Result;
            var result = await GitHubApiHttpClient.PutAsync("/authorizations/clients/" + Configuration["GitHubClientId"].ToString(), reqContent);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                var response = JToken.Parse(content);

                Console.WriteLine("GitHub returned OAuth token: " + response["token"].ToString());
                Console.WriteLine("Put the token into the 'GitHubApiToken' setting in appsettings.json, then run the application again.");
            }

            GitHubApiHttpClient.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task GetNewImages()
        {
            if (Configuration["GitHubClientId"] == null || Configuration["GitHubClientSecret"] == null)
                throw new Exception("GitHubClient* settings are required in appsettings.json to make GitHub API calls");

            ApplicationDbContext _context = new ApplicationDbContext();

            List<Image> toAdd = await GetListFromApi(@"repos/snipe/animated-gifs/contents/");
            _context.Images.AddRange(toAdd);

            _context.SaveChanges();
            Console.WriteLine("Added " + toAdd.Count + " images");

            _context.Dispose();
        }

        public static async Task<List<Image>> GetListFromApi(string requestPath)
        {
            string queryString = $"client_id={Configuration["GitHubClientId"]}&client_secret={Configuration["GitHubClientSecret"]}";
            queryString = (requestPath.Contains('?')) ? '&' + queryString : '?' + queryString;
            Console.WriteLine($"Making HTTP call to {GitHubApiHttpClient.BaseAddress}{requestPath}");
            HttpResponseMessage responseMessage = await GitHubApiHttpClient.GetAsync(requestPath + queryString);

            if (responseMessage.IsSuccessStatusCode)
            {
                string result = await responseMessage.Content.ReadAsStringAsync();
                JToken json = JToken.Parse(result);

                return await AddImages(json, GitHubApiHttpClient);
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
                    Console.WriteLine($"Moving into subdirectory {item["name"].ToString()}");
                    List<Image> dirImages = await GetListFromApi(item["url"].ToString().Substring(22));
                    result.AddRange(dirImages);
                }
            }

            _context.Dispose();
            return result;
        }
    }
}
