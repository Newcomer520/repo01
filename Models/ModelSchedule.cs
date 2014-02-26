using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplicationIPS2.Web.Areas.Schedule.Models
{
    public class ModelSchedule
    {
        public string CurrentMonth { get; set; }
        public string Id { get; set; }
        public string CellStyle { get; set; }
        public enumAuthority Authority { get; set; }
        public string CurrentUser { get; set; }
        public string CurrentCName { get; set; }
        public string Comment { get; set; }

        public List<ModelEvents> Events { get; set; }
        public List<ModelWeekDays> WeekDays { get; set; }
    }  
}