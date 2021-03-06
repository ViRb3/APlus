﻿using System;
using System.Collections.Specialized;
using System.Threading;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace APlus
{
	[Activity (Label = "Create account")]			
	public class RegisterActivity : Activity
	{
		Button _btnRegister;
		EditText _txtEmail, _txtPassword, _txtPassword2, _txtFirstName, _txtLastName, _txtClass;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;
			SetContentView (Resource.Layout.Register);

			_btnRegister = FindViewById<Button> (Resource.Id.btnRegister);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);
			_txtPassword2 = FindViewById<EditText> (Resource.Id.txtPassword2);
			_txtFirstName = FindViewById<EditText> (Resource.Id.txtFirstName);
			_txtLastName = FindViewById<EditText> (Resource.Id.txtLastName);
			_txtClass = FindViewById<EditText> (Resource.Id.txtClass);

			_btnRegister.Click +=  btnRegister_OnClick;
		}

		private void btnRegister_OnClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace (_txtEmail.Text) || string.IsNullOrWhiteSpace (_txtPassword.Text) ||
			    string.IsNullOrWhiteSpace (_txtPassword2.Text) || string.IsNullOrWhiteSpace (_txtFirstName.Text) ||
			    string.IsNullOrWhiteSpace (_txtLastName.Text) || string.IsNullOrWhiteSpace (_txtClass.Text))
			{
				ResponseManager.ShowMessage ("Error", "Information is missing!");
				return;
			}

			if (_txtPassword.Text.Length < 6)
			{
				ResponseManager.ShowMessage ("Error", "Password must be at least 6 characters long!");
				return;
			}

			if (_txtPassword.Text != _txtPassword2.Text)
			{
				ResponseManager.ShowMessage ("Error", "Passwords don't match!");
				return;
			}

			ThreadPool.QueueUserWorkItem (o => DoRegister (sender, e));
		}

		private void DoRegister(object sender, EventArgs e)
		{
			if (Functions.IsOffline ())
			{
				ResponseManager.ShowMessage ("Error", "No internet connection!");
				return;
			}

			if (_txtPassword.Text != _txtPassword2.Text)
			{
				ResponseManager.ShowMessage ("Error", "Passwords do not match!");
				return;
			}

			ResponseManager.ShowLoading ("Creating account..."); 

			var data = new NameValueCollection ();
			data.Add ("register", string.Empty);
			data.Add("email", _txtEmail.Text);
			data.Add("password", Functions.GetSha256(_txtPassword.Text));
			data.Add("firstname", _txtFirstName.Text);
			data.Add("lastname", _txtLastName.Text);
			data.Add("class", _txtClass.Text);

			string reply = WebFunctions.Request (data);

			ResponseManager.DismissLoading (); 

			if (reply != "Account created!")
			{
				ResponseManager.ShowMessage ("Error", reply);
				WebFunctions.ClearCookies ();
				return;
			}	

			RunOnUiThread (delegate {
				var dialogFragment = new DialogFragment();

				dialogFragment.InitializeOk(reply, "Success", delegate {
					Intent resultData = new Intent ();
					resultData.PutExtra ("email", _txtEmail.Text);
					resultData.PutExtra ("password", _txtPassword.Text);
					SetResult (Result.Ok, resultData);
					Finish();
				});

				dialogFragment.Show();
			});
		}
	}
}