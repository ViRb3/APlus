using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Collections.Specialized;

namespace APlus
{
	[Activity]
	public class LoginActivity : Activity
	{
		Button _btnLogin;
		EditText _txtEmail, _txtPassword;

		protected override void OnCreate (Bundle bundle)
		{
			Functions.DeleteSetting ("signedInCookie");
			Functions.DeleteSetting ("settings", "loggedIn");
			WebFunctions.ClearCookies();

			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Login);

			_btnLogin = FindViewById<Button> (Resource.Id.btnLogin);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);

			_btnLogin.Click += btnLogin_OnClick;
		}

		void btnLogin_OnClick(object sender, EventArgs e)
		{
			if (Functions.IsOffline(true)) {
				Toast.MakeText (Application.Context, "No internet connection!", ToastLength.Short).Show ();
				return;
			}

			var data = new NameValueCollection();
			data.Add("login", string.Empty);
			data.Add("email", _txtEmail.Text);
			data.Add("password", Functions.GetMd5(_txtPassword.Text));

			string reply = WebFunctions.Request(data);

			if (reply != "Login success!") {
				Toast.MakeText (this, reply, ToastLength.Short).Show();
				return;
			}	
				
			data.Clear ();
			data.Add("getaccounttype", string.Empty);

			reply = WebFunctions.Request(data);

			if (reply != "student" && reply != "teacher") {
				Toast.MakeText (this, "Unrecognized account type!", ToastLength.Short).Show();
				return;
			}		

			Functions.SaveSetting ("settings", "accountType", reply);
			Functions.SaveSetting ("settings", "loggedIn", "true");
			StartActivity (typeof(MainActivity));
			Finish();
		}
	}
}