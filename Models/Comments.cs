using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdventureWorks_POC.Models
{
	public class Comments
	{
        public int CommentId { get; set; }
        public string User { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        public int PhotoId { get; set; }
        public string UserName { get; set; }
    }
}