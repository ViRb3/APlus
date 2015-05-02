using System;
using System.Collections.Specialized;
using System.Threading;

using Android.App;
using Android.Widget;
using Android.OS;

namespace APlus
{
	[Activity]
	public class LoginActivity : Activity
	{
		Button _btnLogin;
		EditText _txtEmail, _txtPassword;
		TextView _lblRegister;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;

			Functions.DeleteSetting ("signedInCookie");
			Functions.DeleteSetting ("settings", "loggedIn");
			WebFunctions.ClearCookies ();

			SetContentView (Resource.Layout.Login);

			_btnLogin = FindViewById<Button> (Resource.Id.btnLogin);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);
			_lblRegister = FindViewById<TextView> (Resource.Id.lblRegister);

			_btnLogin.Click += btnLogin_OnClick;
			_lblRegister.Click += btnRegister_OnClick;
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;
		}

		private void btnLogin_OnClick (object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem (o => DoLogin (sender, e));
		}

		private void DoLogin (object sender, EventArgs e)
		{
			if (Functions.IsOffline ())
			{
				RunOnUiThread (() => ResponseManager.ShowMessage ("Error", "No internet connection!"));
				return;
			}

			ResponseManager.ShowLoading ("Logging in..."); 

			var data = new NameValueCollection ();
			data.Add ("login", string.Empty);
			data.Add ("email", _txtEmail.Text);
			data.Add ("password", Functions.GetSha256 (_txtPassword.Text));

			string reply = WebFunctions.Request (data);

			if (reply != "Login success!")
			{
				ResponseManager.DismissLoading (); 
				RunOnUiThread (() => ResponseManager.ShowMessage ("Error", reply));
				WebFunctions.ClearCookies ();
				return;
			}	

			data.Clear ();
			data.Add ("getaccounttype", string.Empty);

			reply = WebFunctions.Request (data);

			if (reply != "student" && reply != "teacher")
			{
				ResponseManager.DismissLoading (); 
				RunOnUiThread (() => ResponseManager.ShowMessage ("Error", "Unrecognized account type!"));
				WebFunctions.ClearCookies ();
				return;
			}		

			Functions.SaveSetting ("settings", "accountType", reply);
			Functions.SaveSetting ("settings", "loggedIn", "true");
			StartActivity (typeof(MainActivity));
			Finish ();
		}

		private void btnRegister_OnClick (object sender, EventArgs e)
		{
			StartActivityForResult (typeof(RegisterActivity), 1);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Android.Content.Intent data)
		{
			if (requestCode != 1 || resultCode != Result.Ok || data == null)
				return;

			_txtEmail.Text = data.GetStringExtra ("email");
			_txtPassword.Text = data.GetStringExtra ("password");
		}
	}
}