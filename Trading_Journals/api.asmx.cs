using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services;

namespace Trading_Journals
{
    /// <summary>
    /// Summary description for api
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class api : System.Web.Services.WebService
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TJ"].ConnectionString);
        private bool CheckAuthentication()
        {
            try
            {
                var identity = (FormsIdentity)HttpContext.Current.User.Identity;
                var authenticated = HttpContext.Current.User.Identity.IsAuthenticated;
                return authenticated && identity.Ticket.UserData == "Omid@1993";
            }
            catch (Exception e)
            {
                return false;
            }

        }

        [WebMethod]
        public string CheckUserAuthenticated()
        {
            return !CheckAuthentication() ? "false" : GetPanel();
        }

        [WebMethod]
        public string Login(string username, string password)
        {
            if (username != "omidmokhtari93@gmail.com" || password != "Omid@1993") return "false";
            var ticket = new FormsAuthenticationTicket(1, username.Trim(), DateTime.Now,
                DateTime.Now.AddMinutes(60), false, password);
            var cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket));
            HttpContext.Current.Response.Cookies.Add(cookie1);
            return GetPanel();
        }

        [WebMethod]
        public void Logout()
        {
            if (CheckAuthentication())
            {
                var limit = HttpContext.Current.Request.Cookies.Count;
                for (var i = 0; i < limit; i++)
                {
                    var cookieName = HttpContext.Current.Request.Cookies[i]?.Name;
                    var aCookie = new HttpCookie(cookieName) { Expires = DateTime.Now.AddDays(-1) };
                    HttpContext.Current.Response.Cookies.Add(aCookie);
                }
            }

        }

        public string GetPanel()
        {
            con.Open();
            var cmd = new SqlCommand("select panel from Panel", con);
            var rd = cmd.ExecuteReader();
            var panel = "";
            if (rd.Read())
            {
                panel = rd["panel"].ToString();
            }
            con.Close();
            return panel;
        }

        [WebMethod]
        public string GetJournals(int id, string startDate, string endDate)
        {
            if (!CheckAuthentication())
                return new JavaScriptSerializer().Serialize(new { type = "error", message = "خطا در دریافت اطلاعات" });
            var list = new List<TradeData>();
            con.Open();
            var cmd = new SqlCommand("SELECT [Id],[Created],[Modified],[EnterDate],[CloseDate]" +
                                     ",[Symbol],[Volume],[Profit],[TradeReason],[EnterRavani],[CloseRavani]" +
                                     ",[Comment],[FilePath],[Mistakes]FROM [dbo].[Journals] " +
                                     "where (Id = " + id + " or " + id + " = -1) and " +
                                     "((EnterDate > '" + startDate + "' and CloseDate < '" + endDate + "') or " +
                                     " ('" + startDate + "' = '-1' and '" + endDate + "' = '-1'))", con);
            var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new TradeData()
                {
                    Id = rd["Id"].ToString(),
                    EnterDate = rd["EnterDate"].ToString(),
                    CloseDate = rd["CloseDate"].ToString(),
                    Symbol = rd["Symbol"].ToString(),
                    Volume = rd["Volume"].ToString(),
                    Profit = rd["Profit"].ToString(),
                    TradeReason = rd["TradeReason"].ToString(),
                    EnterRavani = rd["EnterRavani"].ToString(),
                    CloseRavani = rd["CloseRavani"].ToString(),
                    Comment = rd["Comment"].ToString(),
                    FilePath = rd["FilePath"].ToString(),
                    Mistakes = rd["Mistakes"].ToString()
                });
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        [WebMethod]
        public string Sabt()
        {
            if (!CheckAuthentication())
                return new JavaScriptSerializer().Serialize(new { message = "خطا در ثبت اطلاعات", type = "error" });
            var data = HttpContext.Current.Request.Form["data"];
            var file = HttpContext.Current.Request.Files["file"];
            var filePath = "";
            if (file.ContentLength > 0)
            {
                var fileName = Guid.NewGuid();
                var fileExtension = Path.GetExtension(file.FileName);
                var fname = Path.Combine(HttpContext.Current.Server.MapPath("files/"), fileName + fileExtension);
                filePath = "files/" + fileName + fileExtension;
                file.SaveAs(fname);
            }
            var obj = new JavaScriptSerializer().Deserialize<TradeData>(data);
            con.Open();
            var cmd = new SqlCommand("INSERT INTO [dbo].[Journals]([Created],[Modified],[EnterDate],[CloseDate],[Symbol],[Volume]," +
                                     "[Profit],[TradeReason],[EnterRavani],[CloseRavani],[Comment],[FilePath],[Mistakes])" +
                                     "VALUES('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' " +
                                     ", '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" +
                                     ",'" + obj.EnterDate + "' , '" + obj.CloseDate + "' , '" + obj.Symbol + "'" +
                                     ", '" + obj.Volume + "','" + obj.Profit + "'" +
                                     ",N'" + obj.TradeReason + "' , N'" + obj.EnterRavani + "',N'" + obj.CloseRavani + "'" +
                                     ", N'" + obj.Comment + "','" + filePath + "' , N'" + obj.Mistakes + "')", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return new JavaScriptSerializer().Serialize(new { message = "با موفقیت ثبت شد", type = "success" });
        }

        [WebMethod]
        public string EditJournal()
        {
            if (!CheckAuthentication())
                return new JavaScriptSerializer().Serialize(new { message = "خطا در ثبت اطلاعات", type = "error" });
            var data = HttpContext.Current.Request.Form["data"];
            var file = HttpContext.Current.Request.Files["file"];
            var obj = new JavaScriptSerializer().Deserialize<TradeData>(data);
            var filePath = "";
            if (file != null)
            {
                con.Open();
                var cmdd = new SqlCommand("select FilePath from Journals where Id = " + obj.Id + " ", con);
                var oldFilePath = cmdd.ExecuteScalar().ToString();
                if (File.Exists(HttpContext.Current.Server.MapPath(oldFilePath)))
                {
                    File.Delete(HttpContext.Current.Server.MapPath(oldFilePath));
                }
                con.Close();
                var fileName = Guid.NewGuid();
                var fileExtension = Path.GetExtension(file.FileName);
                var fname = Path.Combine(HttpContext.Current.Server.MapPath("files/"), fileName + fileExtension);
                filePath = "files/" + fileName + fileExtension;
                file.SaveAs(fname);
            }
            con.Open();
            var cmd = new SqlCommand("update Journals set " +
                                      "Modified = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" +
                                      ",EnterDate = '" + obj.EnterDate + "'" +
                                      ",CloseDate = '" + obj.CloseDate + "'" +
                                      ",Symbol = '" + obj.Symbol + "'" +
                                      ",Volume = '" + obj.Volume + "'" +
                                      ",Profit = '" + obj.Profit + "'" +
                                      ",TradeReason = N'" + obj.TradeReason + "'" +
                                      ",EnterRavani = N'" + obj.EnterRavani + "'" +
                                      ",CloseRavani = N'" + obj.CloseRavani + "'" +
                                      ",Comment = N'" + obj.Comment + "'" +
                                      ",Mistakes =  N'" + obj.Mistakes + "'" +
                                      " " + (filePath == "" ? "" : ",FilePath = '" + filePath + "' ") + " " +
                                      " where Id = " + obj.Id + " ", con);

            cmd.ExecuteNonQuery();
            con.Close();
            return new JavaScriptSerializer().Serialize(new { message = "با موفقیت ویرایش شد", type = "success" });
        }

        [WebMethod]
        public void AddJournalImage()
        {
            if (!CheckAuthentication()) return;
            var file = HttpContext.Current.Request.Files["image"];
            var imageId = HttpContext.Current.Request.Form["image-id"];
            if (file.ContentLength <= 0) return;
            var fileName = Guid.NewGuid();
            var fileExtension = Path.GetExtension(file.FileName);
            var fname = Path.Combine(HttpContext.Current.Server.MapPath("files/"), fileName + fileExtension);
            var filePath = "files/" + fileName + fileExtension;
            file.SaveAs(fname);
            con.Open();
            var cmd = new SqlCommand("update Journals set FilePath = '" + filePath + "' where Id = " + imageId + " ", con);
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}

public class TradeData
{
    public string Id { get; set; }
    public string EnterDate { get; set; }
    public string CloseDate { get; set; }
    public string Symbol { get; set; }
    public string Volume { get; set; }
    public string Profit { get; set; }
    public string TradeReason { get; set; }
    public string EnterRavani { get; set; }
    public string CloseRavani { get; set; }
    public string Comment { get; set; }
    public string Mistakes { get; set; }
    public string FilePath { get; set; }
}
