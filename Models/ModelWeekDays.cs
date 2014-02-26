using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplicationIPS2.Web.Areas.Schedule.Models
{
    public class ModelWeekDays
    {
        public string YYYYMM { get; set; }
        /// <summary>
        /// 0: mon 1: tue ..... 6: sunday, '-1': Comment
        /// </summary>
        public int WeekDay { get; set; }
        public string Description { get; set; }
    }
}