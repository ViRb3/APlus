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
	[Activity (Label = "APlus", MainLauncher = true, Icon = "@drawable/icon")]
	public class LoginActivity : Activity
	{
		Button _btnLogin;
		EditText _txtEmail, _txtPassword;

		protected override void OnCreate (Bundle bundle)
		{
			var data = new NameValueCollection();
			data.Add("login", string.Empty);

			if (Functions.Request (data) == "Already logged in!") {
				StartActivity (typeof(MainActivity));
				Finish ();
			}

			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Login);

			_btnLogin = FindViewById<Button> (Resource.Id.btnLogin);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);

			_btnLogin.Click += btnLogin_OnClick;
		}

		void btnLogin_OnClick(object sender, EventArgs e)
		{
			var data = new NameValueCollection();
			data.Add("login", string.Empty);
			data.Add("email", _txtEmail.Text);
			data.Add("password", Functions.GetMd5(_txtPassword.Text));

			string reply = Functions.Request(data);

			if (reply != "Login success!") {
				Toast.MakeText (this, reply, ToastLength.Short).Show();
				return;
			}			

			StartActivity(typeof(MainActivity));
			Finish();
		}
	}
}