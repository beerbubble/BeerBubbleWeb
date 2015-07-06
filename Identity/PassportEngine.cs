using BeerBubbleUtility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BeerBubbleWeb
{
    public class PassportEngine
    {
        private static readonly string CookieName = string.IsNullOrEmpty(ConfigurationManager.AppSettings["CookieName"]) ? "BeerBubble" : ConfigurationManager.AppSettings["CookieName"];
        private const int ExpireTime = 720;
        private const bool Encrypt = true;
        static bool isRemember = false;

        public LoginIdentity CreateIdentity(HttpContext context)
        {
            HttpCookie cookie = context.Request.Cookies[CookieName];
            if (cookie == null) return LoginIdentity.Anonymous;
            else
            {
                // 刷新Cookie过期时间
                if (ExpireTime > 0)
                {
                    long ticks = Convert.ToInt64(cookie.Values["expiry"]);
                    DateTime expiry = new DateTime(ticks);
                    if (expiry <= DateTime.Now) return LoginIdentity.Anonymous;
                    //else
                    //{
                    //    cookie.Values["expiry"] = DateTime.Now.AddMinutes(ExpireTime).Ticks.ToString();
                    //    context.Response.Cookies.Add(cookie);
                    //}
                    else
                    {
                        if (isRemember)
                        {
                            cookie.Values["expiry"] = DateTime.Now.AddDays(ExpireTime).Ticks.ToString();
                            cookie.Expires = DateTime.Now.AddDays(ExpireTime);
                            context.Response.Cookies.Add(cookie);
                        }
                        else
                        {
                            cookie.Values["expiry"] = DateTime.Now.AddMinutes(ExpireTime).Ticks.ToString();
                            cookie.Expires = DateTime.Now.AddHours(ExpireTime);
                            context.Response.Cookies.Add(cookie);
                        }

                    }
                }

                string id = cookie.Values["id"];
                string name = DecodeCookieValue(cookie.Values["name"]);
                bool remember = Convert.ToBoolean(cookie.Values["remember"]);

                if (Encrypt)
                {
                    id = EncryptHelper.AESDecrypt(id);
                    name = EncryptHelper.AESDecrypt(name);
                }

                LoginIdentity identity = new LoginIdentity(Convert.ToInt32(id), name);
                return identity;
            }
        }

        public void SignIn(int userId, string userName, bool remember)
        {

            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException("username");

            HttpCookie cookie = CreateuserCookie(userId.ToString(), userName, remember);
            HttpContext.Current.Response.Cookies.Add(cookie);

        }

        public void SignOut()
        {

            HttpCookie cookie = new HttpCookie(CookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(cookie);

        }

        #region private

        private static HttpCookie CreateuserCookie(string userID, string username, bool remember)
        {
            HttpCookie cookie = new HttpCookie(CookieName);

            //if (remember) cookie.Expires = new DateTime(2038, 1, 1);
            if (remember)
            {
                cookie.Values.Add("expiry", DateTime.Now.AddDays(ExpireTime).Ticks.ToString());
                cookie.Expires = DateTime.Now.AddDays(ExpireTime);
                isRemember = true;


            }

            else if (ExpireTime > 0)
            {
                cookie.Values.Add("expiry", DateTime.Now.AddMinutes(ExpireTime).Ticks.ToString());
                cookie.Expires = DateTime.Now.AddMinutes(ExpireTime);
                isRemember = false;
                //cookie.Values.Add("expiry", DateTime.Now.AddMinutes(ExpireTime).Ticks.ToString());
            }

            cookie.Values.Add("id", EncryptHelper.AESEncrypt(userID));
            cookie.Values.Add("name", EncodeCookieValue(EncryptHelper.AESEncrypt(username)));

            return cookie;
        }

        // UTF-8解码
        private static string DecodeCookieValue(string value)
        {
            return HttpUtility.UrlDecode(value, Encoding.UTF8);
            // HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        // UTF-8编码
        private static string EncodeCookieValue(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        #endregion
    }
}
