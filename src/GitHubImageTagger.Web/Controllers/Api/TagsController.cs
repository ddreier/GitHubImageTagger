using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GitHubImageTagger.Web;
using GitHubImageTagger.Core.Models;
using Microsoft.EntityFrameworkCore;
using GitHubImageTagger.Core;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GitHubImageTagger.Controllers.Api
{
    [Route("api/[controller]")]
    public class TagsController : Controller
    {
        private ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Tag> Get()
        {
            return _context.Tags;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public Tag Get(int id)
        {
            return _context.Tags.Where(t => t.TagId == id).FirstOrDefault();
        }

        [HttpGet("search")]
        public IEnumerable<Image> Search(string terms)
        {
            char[] delimiters = { ' ', ',', '.' };

            string[] splitTerms = terms.ToLower().Trim().Split(delimiters);

            var results = _context.Tags.Where(t => splitTerms.Any(s => t.Content.Contains(s))).Select(t => t.Image).Distinct().Include(i => i.Tags);

            return results;
        }

        // POST api/values
        [HttpPost]
        public void Post(string tags, int imageId)
        {
            string[] tagArr = tags.Split(',');
            Image img = _context.Images.Where(i => i.ImageId == imageId).FirstOrDefault();
            
            if (img != null)
            {
                if (img.Tags == null)
                    img.Tags = new List<Tag>();
                foreach (string tag in tagArr)
                {
                    img.Tags.Add(new Tag { Content = tag.ToLower().Trim(), Image = img });
                }

                _context.Images.Update(img);
                _context.SaveChanges();
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            if (value == null)
            {
                return BadRequest("Replacement tag content must be provided");
            }

            var tagToUpdateQuery = _context.Tags.Where(t => t.TagId == id);

            if (tagToUpdateQuery.Any())
            {
                Tag tagToUpdate = tagToUpdateQuery.First();
                tagToUpdate.Content = value;
                _context.Tags.Update(tagToUpdate);
                _context.SaveChanges();

                return Ok();
            }
            else
                return NotFound();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tagToDeleteQuery = _context.Tags.Where(t => t.TagId == id);

            if (tagToDeleteQuery.Any())
            {
                _context.Tags.Remove(tagToDeleteQuery.First());
                _context.SaveChanges();

                return Ok();
            }
            else
                return NotFound();
        }
    }
}
