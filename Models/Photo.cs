using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AdventureWorks_POC.Models
{
	public class Photo
	{
        public int PhotoId { get; set; }
        public string Title { get; set; }
        public byte[] PhotoFile { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Owner { get; set; }
        public string OwnerName { get; set; }
        public string PhotoB64 { get; set; }
    }
}