using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GitHubImageTagger.Web;
using GitHubImageTagger.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GitHubImageTagger.Core;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GitHubImageTagger.Controllers.Api
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private ApplicationDbContext _context;

        public ImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Image> Get()
        {
            return _context.Images;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public Image Get(int id)
        {
            var query = _context.Images.Where(i => i.ImageId == id).Include(i => i.Tags).FirstOrDefault();

            return query;
        }

        [HttpGet("take")]
        public IEnumerable<Image> Take(int count)
        {
            return _context.Images.Take(count);
        }

        [HttpGet("takerandom")]
        public IEnumerable<Image> TakeRandom(int count)
        {
            var res = _context.Images.OrderBy(i => Guid.NewGuid()).Take(count);

            return res;
        }

        [HttpGet("tags/{id}")]
        public IEnumerable<Tag> Tags(int id)
        {
            return _context.Images.Where(i => i.ImageId == id).Select(i => i.Tags).FirstOrDefault();
        }

        [HttpGet("untagged/take/{count}")]
        public IEnumerable<Image> UntaggedTake(int count)
        {
            return _context.Images.Where(i => i.Tags.Count == 0).OrderBy(i => Guid.NewGuid()).Take(count);
        }

        // POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
