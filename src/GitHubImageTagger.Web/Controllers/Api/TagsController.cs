using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GitHubImageTagger.Web;
using GitHubImageTagger.Core.Models;
using Microsoft.EntityFrameworkCore;
using GitHubImageTagger.Core;
using System.Text.RegularExpressions;

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
            List<string> splitTerms = new List<string>();
            Regex r = new Regex("([\"'])(?:(?=(\\\\?))\\2.)*?\\1"); // Pattern from http://stackoverflow.com/a/171499/390192
            var m = r.Matches(terms);
            foreach (Match match in m)
            {
                string temp = match.Value.Substring(1, match.Value.Length - 2);
                splitTerms = splitTerms.Append(temp).ToList<string>();
                terms = terms.Replace(match.Value, "");
            }

            char[] delimiters = { ' ', ',', '.' };

            splitTerms.AddRange(terms.ToLower().Trim().Split(delimiters, StringSplitOptions.RemoveEmptyEntries));

            if (splitTerms.Count > 0)
            {
                var results = _context.Tags.Where(t => splitTerms.Any(s => t.Content.Contains(s))).Select(t => t.Image).Distinct().Include(i => i.Tags);

                return results;
            }
            else
            {
                return null;
            }
        }

        [HttpGet("top/{take?}")]
        public object Top(int? take)
        {
            var grouping = _context.Tags.GroupBy(tag => tag.Content)
                                        .Select(group => new { Content = group.Key, Count = group.Count() })
                                        .OrderByDescending(g => g.Count);

            if (take.HasValue)
                return grouping.Take(take.Value);
            else
                return grouping;
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
