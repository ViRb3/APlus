using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Android.App;
using Android.Content;
using Android.Widget;

namespace APlus
{
	public static class Functions
	{
		private static readonly string _server = "http://microcast.mcserver.ws/aplus/service.php";
        private static BetterWebClient _webClient = new BetterWebClient();
	    private static Cookie _signedInCookie;

	    static Functions()
	    {
			Cookie signedInCookie = new Cookie();

			string cookieName = GetSetting ("signedInCookie", "Name");
			if (!string.IsNullOrEmpty (cookieName))
				signedInCookie.Name = cookieName;

			string cookieDomain = GetSetting ("signedInCookie", "Domain");
			if (!string.IsNullOrEmpty (cookieDomain))
				signedInCookie.Domain = cookieDomain;

			string cookiePath = GetSetting ("signedInCookie", "Path");
			if (!string.IsNullOrEmpty (cookiePath))
				signedInCookie.Path = cookiePath;

			string cookieValue = GetSetting ("signedInCookie", "Value");
			if (!string.IsNullOrEmpty (cookieValue))
				signedInCookie.Value = cookieValue;

			string cookieExpires = GetSetting ("signedInCookie", "Expires");
			if (!string.IsNullOrEmpty (cookieExpires))
				signedInCookie.Expires = Convert.ToDateTime(cookieExpires);

			if (!string.IsNullOrEmpty(signedInCookie.Name) && !string.IsNullOrEmpty(signedInCookie.Domain) &&
				!string.IsNullOrEmpty(signedInCookie.Path) && !string.IsNullOrEmpty(signedInCookie.Value))
	        {
				_signedInCookie = signedInCookie;
				_webClient.CookieJar.Add (_signedInCookie);
	        }
	    }

		public static string Request(NameValueCollection data)
		{
            byte[] response = _webClient.UploadValues(_server, "POST", data);
			string result = _webClient.Encoding.GetString(response);

			if (result != "Login success!")
				return result;

            if (_signedInCookie == null)
		        _signedInCookie = _webClient.CookieJar.GetCookies(new Uri(_server)).Cast<Cookie>().FirstOrDefault(c => c.Name == "signedUser");

            if (_signedInCookie == null)
		        Toast.MakeText(Application.Context, "Unable to load keep signed in cookie", ToastLength.Short);
		    else
		    {
		        List<Tuple<string, string>> settings = new List<Tuple<string, string>>();
                settings.Add(new Tuple<string, string>("Name", _signedInCookie.Name));
                settings.Add(new Tuple<string, string>("Domain", _signedInCookie.Domain));
                settings.Add(new Tuple<string, string>("Path", _signedInCookie.Path));
                settings.Add(new Tuple<string, string>("Value", _signedInCookie.Value));
                settings.Add(new Tuple<string, string>("Expires", _signedInCookie.Expires.ToString()));
                
                SaveSetting("signedInCookie", settings.ToArray());
		    }
            
		    return result;
		}

		public static string GetMd5(string input)
		{
			var md5 = MD5.Create();
			byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

			StringBuilder stringBuilder = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			{
				stringBuilder.Append(data[i].ToString("x2"));
			}
				
			return stringBuilder.ToString();
		}

        public static void SaveSetting(string settingName, Tuple<string, string>[] settings)
        {

            var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
            var preferencesEditor = preferences.Edit();

            foreach (var setting in settings)
                preferencesEditor.PutString(setting.Item1, setting.Item2);

            preferencesEditor.Commit();
        }

        public static string GetSetting(string settingName, string subSettingName)
        {
            var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
			var setting = preferences.GetString(subSettingName, null);

            return setting;
        }
	}

    class BetterWebClient : WebClient
    {
        public CookieContainer CookieJar = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null)
            {
                request.Method = "Post";
                request.CookieContainer = CookieJar;
            }

            return request;
        }
    }
}