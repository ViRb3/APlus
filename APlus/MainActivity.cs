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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			string loggedIn = Functions.GetSetting ("settings", "loggedIn");

			if (!object.Equals (loggedIn, "true")) {
				StartActivity (typeof(LoginActivity));
				Finish ();
				return;
			} 
			else new Thread (CheckLogin).Start ();

			if (object.Equals(Functions.GetSetting("settings", "accountType"), "teacher"))
				InitializeTeacher ();
			else
				InitializeStudent ();
		}

		private void InitializeTeacher()
		{
			SetContentView (Resource.Layout.MainTeacher);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
			this.Title = "APlus Teacher Panel";

			Button btnGradeIndividual = FindViewById<Button> (Resource.Id.btnGradeIndividual);
			btnGradeIndividual.Click += (object sender, EventArgs e) => {
				if (Functions.IsOffline(true)) {
					Toast.MakeText(this, "Cannot complete action while offline.", ToastLength.Long).Show ();
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

			while (!_checkedStatus)
				Thread.Sleep (100);

			var data = new NameValueCollection();
			data.Add("checkuser", string.Empty);

			string rawReply = WebFunctions.Request (data);

			if (!rawReply.Contains ("Registered")) {
				StartActivity (typeof(LoginActivity));
				Finish ();
			}

			string[] reply = Regex.Split (rawReply, "<br>");

			var gridview = FindViewById<GridView>(Resource.Id.gridView1);
			gridview.Adapter = new GradesAdapter(this, reply);

			gridview.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
			{
				Toast.MakeText(this, args.Position.ToString(), ToastLength.Short).Show();
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (data == null)
				return;

			if (requestCode == 1) {
				if (string.IsNullOrEmpty(data.GetStringExtra ("error")))
					Toast.MakeText (this, data.GetStringExtra ("reply"), ToastLength.Long).Show();
				else
					Toast.MakeText (this, data.GetStringExtra ("error"), ToastLength.Long).Show();
			}
		}

		private void CheckLogin()
		{
			Looper.Prepare();

			try {
				bool loggedIn = Functions.IsLoggedIn();
				Functions.DeleteSetting("settings", "offline");
				_checkedStatus = true;

				if (!loggedIn) {
					StartActivity (typeof(LoginActivity));
					Finish ();
				}
			}
			catch (Exception) {
				Functions.SaveSetting ("settings", "offline", "true");
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
			if (Functions.IsOffline(true)) {
				Toast.MakeText (Application.Context, "No internet connection!", ToastLength.Short).Show ();
				return base.OnOptionsItemSelected (item);
			}

			var data = new NameValueCollection();
			data.Add("logout", string.Empty);

			string response = WebFunctions.Request (data);
			if (response == "Logged out successfully") {
				StartActivity (typeof(LoginActivity));
				Finish ();
			} else
				Toast.MakeText (Application.Context, response, ToastLength.Long).Show ();

			return base.OnOptionsItemSelected (item);
		}
	}
}