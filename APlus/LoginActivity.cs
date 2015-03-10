using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Collections.Specialized;
using System.Threading;
using Java.Util.Concurrent;

namespace APlus
{
	[Activity]
	public class LoginActivity : Activity
	{
		Button _btnLogin;
		EditText _txtEmail, _txtPassword;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;

			Functions.DeleteSetting ("signedInCookie");
			Functions.DeleteSetting ("settings", "loggedIn");
			WebFunctions.ClearCookies();

			SetContentView (Resource.Layout.Login);

			_btnLogin = FindViewById<Button> (Resource.Id.btnLogin);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);

			_btnLogin.Click += btnLogin_OnClick;
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;
		}

		void btnLogin_OnClick(object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem (o => Do_btnLogin_OnClick (sender, e));
		}

		void Do_btnLogin_OnClick(object sender, EventArgs e)
		{
			if (Functions.IsOffline()) {
				RunOnUiThread(() => ResponseManager.ShowMessage("Error", "No internet connection!"));
				return;
			}

			ResponseManager.ShowLoading ("Logging in..."); 

			var data = new NameValueCollection();
			data.Add("login", string.Empty);
			data.Add("email", _txtEmail.Text);
			data.Add("password", Functions.GetMd5(_txtPassword.Text));

			string reply = WebFunctions.Request(data);

			if (reply != "Login success!") {
				ResponseManager.DismissLoading (); 
				RunOnUiThread(() => ResponseManager.ShowMessage("Error", reply));
				return;
			}	

			data.Clear ();
			data.Add("getaccounttype", string.Empty);

			reply = WebFunctions.Request(data);

			if (reply != "student" && reply != "teacher") {
				ResponseManager.DismissLoading (); 
				RunOnUiThread(() => ResponseManager.ShowMessage("Error", "Unrecognized account type!"));
				return;
			}		

			Functions.SaveSetting ("settings", "accountType", reply);
			Functions.SaveSetting ("settings", "loggedIn", "true");
			StartActivity (typeof(MainActivity));
			Finish();
		}
	}
}