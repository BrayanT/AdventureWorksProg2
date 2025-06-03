using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdventureWorks_POC.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public string ConfirmarClave { get; set; }
    }
}