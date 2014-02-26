using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationIPS2.Service.Schedule;
using MvcApplicationIPS2.Repository.Schedule;
using System.Data;
using MvcApplicationIPS2.Service.SAM;

namespace MvcApplicationIPS2.Web.Areas.Schedule.Controllers
{

    public class DefaultController : Controller
    {
        //
        // GET: /Schedule/Default/

        /// <summary>
        /// 任何人都可以讀，adm則有格外的編輯權限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [MvcApplicationIPS2.Web.Areas.Schedule.Attribute.Schedule(Roles = "ALL")]
        public ActionResult Index(string mainid)
        {            
            if (string.IsNullOrEmpty(mainid))
            {
                mainid = "F0EC4C5F384E2973E043E0C4380A7471";//測試用
            }

            Models.ModelSchedule model = getModel(mainid, DateTime.Now.ToString("yyyyMM"));
            
            //測試用的main id都可進行讀寫
            model.Authority = (mainid == "F0EC4C5F384E2973E043E0C4380A7471") ? Models.enumAuthority.Adm : Models.enumAuthority.Read;

            return View(model);
        }

        [HttpGet]
        [MvcApplicationIPS2.Web.Areas.Schedule.Attribute.Schedule(Roles = "ALL")]
        public ActionResult SysMod(string sysid, string moduleid)
        {
            SchSysModuleRelRepository rep = new SchSysModuleRelService().GetSysModRel(sysid, moduleid);
            Models.ModelSchedule model = getModel(rep.MAIN_ID, DateTime.Now.ToString("yyyyMM"));
            model.Authority = (rep != null && rep.MAIN_ID != null && Session["SCH_" + UserService.Pernr + "_" + rep.MAIN_ID] != null && Convert.ToBoolean(Session["SCH_" + UserService.Pernr + "_" + rep.MAIN_ID]))
                ? Models.enumAuthority.Adm : Models.enumAuthority.Read;
            //model.Authority = Models.enumAuthority.Adm;
            return View("Index", model);
        }
        private Models.ModelSchedule getModel(string mainid, string yyyymm)
        {
            SchMainService mainService = new SchMainService();
            SchStylesRepository style = mainService.GetStyleByMainId(mainid);
            
            Models.ModelSchedule model = new Models.ModelSchedule();
            model.Id = mainid;
            model.CurrentMonth = yyyymm;
            model.CurrentUser = UserService.Pernr;
            model.CurrentCName = UserService.CName;
            model.CellStyle = style.STYLE;
            model.Events = new List<Models.ModelEvents>();
            foreach (DataRow e in mainService.GetEventsByMainIdInTheMonth(mainid, yyyymm).Rows)
            {
                //model.Events.Add(new Models.ModelEvents() { Title = e.TITLE, YYYYMMDD = e.YYYYMMDD, Style = e.STYLE });
                model.Events.Add(new Models.ModelEvents() { Title = e["TITLE"].ToString(), YYYYMMDD = e["YYYYMMDD"].ToString(), HHMM = e["HHMM"].ToString(), Style = e["STYLE"].ToString(), CName = e["CName"].ToString() });
            }

            model.WeekDays = new List<Models.ModelWeekDays>();
            foreach (SchWeekDayRespository wd in mainService.SchWeekDayService.GetWeekDaysByMainId(mainid, yyyymm))
            {
                model.WeekDays.Add(new Models.ModelWeekDays() { WeekDay = Convert.ToInt32(wd.WEEKDAY), Description = wd.DESCRIPTION});
                if (wd.WEEKDAY == -1) {
                    model.Comment = wd.DESCRIPTION;
                }
            }
            //model.Authority = Models.enumAuthority.Adm;
            //model.Authority = Models.enumAuthority.Read;

            //model.WeekDays = mainService.SchWeekDayService.g
            return model;
        }
        [HttpPost]
        [MvcApplicationIPS2.Web.Areas.Schedule.Attribute.Schedule(Roles = "ALL")]
        public ActionResult Index(string mainid, string yyyymm)
        {
            DateTime d1 = DateTime.Now;
            if (string.IsNullOrEmpty(yyyymm))
            {
                return Content("");
            }

            Models.ModelSchedule model = getModel(mainid, yyyymm);
            TimeSpan total = DateTime.Now.Subtract(d1);
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(model));
        }

        [HttpPost]
        [MvcApplicationIPS2.Web.Areas.Schedule.Attribute.Schedule(Roles="ADM")]
        public ActionResult Save(Models.ModelSchedule model, string mainid)
        {
            model.Id = mainid; //for security reason
            string guid = Guid.NewGuid().ToString();
            SchMainService mainService = new SchMainService(guid);
            SchStylesService stylesService = new SchStylesService(guid);
            SchStylesRepository style = stylesService.GetSpecifiedStyleByMainId("td", model.Id);
            SchEventsService eventsService = new SchEventsService(guid);
            SchEventsRepository newEvent;
            SchWeekDayRespository newWeekDay;
            IEnumerable<SchEventsRepository> events;
            MvcApplicationIPS2.Service.SAM.UserService userService = new Service.SAM.UserService();
            
            string pernr = UserService.Pernr;

            try
            {
                if (style != null)
                {
                    style.STYLE = model.CellStyle;
                    stylesService.Update(style);
                }
                else {
                    style = new SchStylesRepository();
                    style.TYPE = "td";
                    style.MAIN_ID = model.Id;
                    style.STYLE = model.CellStyle;
                    style.CREATE_EMPNO = pernr;
                    stylesService.Create(style);

                }

                if (model.WeekDays != null) {
                    //暫時不分月份
                    
                    foreach (SchWeekDayRespository weekday in mainService.SchWeekDayService.GetWeekDaysByMainId(model.Id, model.CurrentMonth))
                    {
                        foreach (Models.ModelWeekDays wd in model.WeekDays)
                        {
                            if (weekday.YYYYMM == wd.YYYYMM && weekday.WEEKDAY == wd.WeekDay)
                                mainService.SchWeekDayService.Delete(weekday);
                        }                        
                    }                        

                    foreach (Models.ModelWeekDays wd in model.WeekDays)
                    {
                        if (string.IsNullOrEmpty(wd.Description) || string.IsNullOrEmpty(wd.YYYYMM))
                            continue;
                        newWeekDay = new SchWeekDayRespository();
                        newWeekDay.DESCRIPTION = wd.Description;
                        newWeekDay.MAIN_ID = model.Id;
                        newWeekDay.WEEKDAY = wd.WeekDay;
                        newWeekDay.YYYYMM = wd.YYYYMM;                        
                        mainService.SchWeekDayService.Create(newWeekDay);
                    }
                }

                //delete 與 create需要分開做 因為不同的event可能是相同的日期 若同在一個loop做 可能會把該次新增的刪除掉
                if (model.Events != null) {
                    foreach (Models.ModelEvents e in model.Events)
                    {
                        events = eventsService.GetEventsByMainIdInDate(model.Id, e.YYYYMMDD);
                        foreach (SchEventsRepository e2 in events)
                        {
                            eventsService.Delete(e2);
                        }
                    }
                    foreach (Models.ModelEvents e in model.Events)
                    {
                        if (string.IsNullOrEmpty(e.YYYYMMDD))
                            continue;
                        if (string.IsNullOrEmpty(e.Title) && string.IsNullOrEmpty(e.Style))
                            continue;
                        newEvent = new SchEventsRepository();
                        newEvent.CREATED_BY = pernr;
                        newEvent.YYYYMMDD = e.YYYYMMDD;
                        newEvent.HHMM = e.HHMM;
                        newEvent.TITLE = e.Title;
                        newEvent.MAIN_ID = model.Id;
                        newEvent.DETAIL = string.Empty;
                        newEvent.STYLE = e.Style;

                        eventsService.Create(newEvent);
                    }
                }                
                

                model.CurrentMonth = string.IsNullOrEmpty(model.CurrentMonth) ? DateTime.Now.ToString("yyyyMM") : model.CurrentMonth;

                eventsService.SaveChange();

                model = getModel(model.Id, model.CurrentMonth);

            }
            catch (Exception ex)
            {
                mainService.RollBack();
                throw ex;
            }
            
            
            model.Authority = Models.enumAuthority.Adm;
            string jsonObj = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            return Content(jsonObj);
        }

    }
}
