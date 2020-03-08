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
        public string GetJournals(int id)
        {
            if (!CheckAuthentication())
                return new JavaScriptSerializer().Serialize(new { type = "error", message = "خطا در دریافت اطلاعات" });
            var list = new List<TradeData>();
            con.Open();
            var cmd = new SqlCommand("SELECT [Id],[Created],[Modified],[EnterDate],[CloseDate]" +
                                     ",[Symbol],[Volume],[Profit],[TradeReason],[EnterRavani],[CloseRavani]" +
                                     ",[Comment],[FilePath],[Mistakes]FROM [dbo].[Journals] " +
                                     "where Id = " + id + " or " + id + " = -1 ", con);
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
