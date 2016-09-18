using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubImageTagger.Core.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        public string Content { get; set; }

        public Image Image { get; set; }
    }
}
