using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GitHubImageTagger.Core.Models
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public virtual List<Tag> Tags { get; set; }
    }
}
