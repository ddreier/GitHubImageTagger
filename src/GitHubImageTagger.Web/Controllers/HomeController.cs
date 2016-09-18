using GitHubImageTagger.Core;
using GitHubImageTagger.Core.Models;
using GitHubImageTagger.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubImageTagger.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("/")]
        public IActionResult Index()
        {
            int count = _context.IntFromSql("SELECT COUNT(DISTINCT ImageId) FROM Tags");

            ViewBag.TaggedImageCount = count;

            return View();
        }

        [Route("/queue")]
        public IActionResult Queue()
        {
            List<Image> images = _context.Images.Where(i => i.Tags.Count == 0).OrderBy(i => Guid.NewGuid()).Take(15).ToList();

            ViewBag.Images = images;

            return View();
        }
    }
}
