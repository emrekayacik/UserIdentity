using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace UserIdentity.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}