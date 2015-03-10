using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;

namespace APlus
{
	[Activity (Label = "APlus", MainLauncher = true, Icon = "@drawable/icon")]			
	public class MainActivity : Activity
	{
		private bool _checkedStatus;
		private string _pendingMessage;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;

			string loggedIn = Functions.GetSetting ("settings", "loggedIn");

			if (!object.Equals (loggedIn, "true")) {
				StartActivity (typeof(LoginActivity));
				Finish ();
				return;
			} 
			ThreadPool.QueueUserWorkItem (o => CheckLogin ());

			if (object.Equals(Functions.GetSetting("settings", "accountType"), "teacher"))
				InitializeTeacher ();
			else
				InitializeStudent ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;

			if (!string.IsNullOrWhiteSpace (_pendingMessage)) {
				ResponseManager.ShowMessage ("Result", _pendingMessage);
				_pendingMessage = null;
			}
		}

		private void InitializeTeacher()
		{
			SetContentView (Resource.Layout.MainTeacher);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
			this.Title = "APlus Teacher Panel";

			Button btnGradeIndividual = FindViewById<Button> (Resource.Id.btnGradeIndividual);

			btnGradeIndividual.Click += (object sender, EventArgs e) => {
				if (Functions.IsOffline()) {
					ResponseManager.ShowMessage("Error", "Cannot complete action while offline.");
					return;
				}

				StartActivityForResult(typeof(ScanCodeActivity), 1);
			};
		}

		private void InitializeStudent()
		{
			SetContentView (Resource.Layout.MainStudent);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
			this.Title = "APlus Student Panel";

			if (Functions.IsOffline()) {
				ResponseManager.ShowMessage("Error", "Cannot complete action while offline.");
				return;
			}

			ResponseManager.ShowLoading ("Fetching user data...");

			ThreadPool.QueueUserWorkItem (o => {
				while (!_checkedStatus)
					Thread.Sleep(100);

				FetchStudentData();
			});
		}

		private void FetchStudentData()
		{
			var data = new NameValueCollection();
			data.Add("checkuser", string.Empty);

			string rawReply = WebFunctions.Request (data);

			if (!rawReply.Contains ("Registered")) {
				StartActivity (typeof(LoginActivity));
				Finish ();
			}

			string[] reply = Regex.Split (rawReply, "<br>");

			var gridview = FindViewById<GridView>(Resource.Id.gridView1);
			RunOnUiThread(() => gridview.Adapter = new GradesAdapter(this, reply));

			ResponseManager.DismissLoading ();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (data == null)
				return;

			if (requestCode == 1) {
				if (data.GetStringExtra ("error") == null) {
					string[] array = data.GetStringArrayExtra ("reply");
					_pendingMessage = array[0] + System.Environment.NewLine + array[1];
				}
				else
					_pendingMessage = data.GetStringExtra("error");
			}
		}

		private void CheckLogin()
		{
			try {
				bool loggedIn = Functions.IsLoggedIn();

				if (!loggedIn) {
					StartActivity (typeof(LoginActivity));
					Finish ();
				}
			}
			finally {
				_checkedStatus = true;
			}
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			var inflater = MenuInflater;
			inflater.Inflate(Resource.Menu.optionsMenu, menu);        
			return true;
		}
			
		public override bool OnOptionsItemSelected(IMenuItem item) 
		{
			if (Functions.IsOffline()) {
				ResponseManager.ShowMessage("Error", "Cannot complete action while offline.");
				return base.OnOptionsItemSelected (item);
			}

			ResponseManager.ShowLoading ("Logging out...");
			ThreadPool.QueueUserWorkItem (o => DoLogout ());

			return base.OnOptionsItemSelected (item);
		}

		private static void DoLogout()
		{
			var data = new NameValueCollection();
			data.Add("logout", string.Empty);

			string response = WebFunctions.Request (data);
			if (response == "Logged out successfully") {
				Functions.CurrentContext.StartActivity (typeof(LoginActivity));
				Functions.CurrentContext.Finish ();
			} else {
				ResponseManager.ShowMessage ("Error", response);
				ResponseManager.DismissLoading ();
			}
		}
	}
}