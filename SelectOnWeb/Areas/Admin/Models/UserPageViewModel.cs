using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SelectOnWeb.Areas.Admin.Models
{
    public class UserPageViewModel
    {
        [Range(1,30)]
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public IEnumerable<SelectOnWeb.Models.tb_student> data { get; set; }
    }
}