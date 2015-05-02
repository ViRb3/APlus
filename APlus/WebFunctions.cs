using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Android.App;
using Android.Widget;
using Android.Content;

namespace APlus
{
	public static class WebFunctions
	{
		private static readonly string _server = "http://microcast.mcserver.ws/aplus/service.php";
		private static BetterWebClient _webClient = new BetterWebClient ();
		private static Cookie _signedInCookie;
		public static Context Activity;

		static WebFunctions ()
		{
			Cookie signedInCookie = new Cookie ();

			string cookieName = Functions.GetSetting ("signedInCookie", "Name");
			if (!string.IsNullOrEmpty (cookieName))
				signedInCookie.Name = cookieName;

			string cookieDomain = Functions.GetSetting ("signedInCookie", "Domain");
			if (!string.IsNullOrEmpty (cookieDomain))
				signedInCookie.Domain = cookieDomain;

			string cookiePath = Functions.GetSetting ("signedInCookie", "Path");
			if (!string.IsNullOrEmpty (cookiePath))
				signedInCookie.Path = cookiePath;

			string cookieValue = Functions.GetSetting ("signedInCookie", "Value");
			if (!string.IsNullOrEmpty (cookieValue))
				signedInCookie.Value = cookieValue;

			string cookieExpires = Functions.GetSetting ("signedInCookie", "Expires");
			if (!string.IsNullOrEmpty (cookieExpires))
				signedInCookie.Expires = Convert.ToDateTime (cookieExpires);

			if (!string.IsNullOrEmpty (signedInCookie.Name) && !string.IsNullOrEmpty (signedInCookie.Domain) &&
			    !string.IsNullOrEmpty (signedInCookie.Path) && !string.IsNullOrEmpty (signedInCookie.Value))
			{
				_signedInCookie = signedInCookie;
				_webClient.CookieJar.Add (_signedInCookie);
			}
		}

		public static string Request (NameValueCollection data)
		{
			byte[] response;

			try
			{
				response = _webClient.UploadValues (_server, "POST", data);
			} catch (WebException e)
			{
				return e.Message;
			}

			string result = _webClient.Encoding.GetString (response);

			if (result != "Login success!")
				return result;

			HandleLogin ();
			return result;
		}

		private static void HandleLogin ()
		{
			if (_signedInCookie == null)
				_signedInCookie = _webClient.CookieJar.GetCookies (new Uri (_server)).Cast<Cookie> ().FirstOrDefault (c => c.Name == "signedUser");

			if (_signedInCookie == null)
				Functions.CurrentContext.RunOnUiThread (() => Toast.MakeText (Application.Context, "Unable to load keep signed in cookie", ToastLength.Short));
			else
			{
				List<Tuple<string, string>> settings = new List<Tuple<string, string>> ();
				settings.Add (new Tuple<string, string> ("Name", _signedInCookie.Name));
				settings.Add (new Tuple<string, string> ("Domain", _signedInCookie.Domain));
				settings.Add (new Tuple<string, string> ("Path", _signedInCookie.Path));
				settings.Add (new Tuple<string, string> ("Value", _signedInCookie.Value));
				settings.Add (new Tuple<string, string> ("Expires", _signedInCookie.Expires.ToString ()));

				Functions.SaveSetting ("signedInCookie", settings.ToArray ());
			}
		}

		public static void ClearCookies ()
		{
			_signedInCookie = null;
			_webClient.CookieJar = new CookieContainer ();
		}
	}

	class BetterWebClient : WebClient
	{
		public BetterWebClient ()
		{
			this.Encoding = Encoding.UTF8;
		}

		public CookieContainer CookieJar = new CookieContainer ();

		protected override WebRequest GetWebRequest (Uri address)
		{
			HttpWebRequest request = base.GetWebRequest (address) as HttpWebRequest;

			if (request != null)
			{
				request.Method = "Post";
				request.CookieContainer = CookieJar;
			}

			return request;
		}
	}
}

