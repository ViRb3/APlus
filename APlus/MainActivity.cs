using System;
using System.Linq;
using System.Collections.Specialized;
using System.Threading;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

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

			if (!object.Equals (loggedIn, "true"))
			{
				StartActivity (typeof(LoginActivity));
				Finish ();
				return;
			} 

			if (!Functions.IsOffline ())
				ThreadPool.QueueUserWorkItem (o => CheckLogin ());

			var accountType = Functions.GetSetting ("settings", "accountType");

			if (object.Equals (accountType, "teacher"))
				InitializeTeacher ();
			else if (object.Equals (accountType, "student"))
				InitializeStudent ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;

			if (!string.IsNullOrWhiteSpace (_pendingMessage))
			{
				ResponseManager.ShowMessage ("Result", _pendingMessage);
				_pendingMessage = null;
			}
		}

		private void InitializeTeacher ()
		{
			SetContentView (Resource.Layout.MainTeacher);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
			this.Title = "APlus Teacher Panel";

			Button btnGradeIndividual = FindViewById<Button> (Resource.Id.btnGradeIndividual);

			btnGradeIndividual.Click += (object sender, EventArgs e) => {
				if (Functions.IsOffline ())
				{
					ResponseManager.ShowMessage ("Error", "Cannot complete action while offline.");
					return;
				}

				StartActivityForResult (typeof(ScanCodeActivity), 1);
			};
		}

		private void InitializeStudent ()
		{
			SetContentView (Resource.Layout.MainStudent);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
			this.Title = "APlus Student Panel";

			if (Functions.IsOffline ())
			{
				ResponseManager.ShowMessage ("Error", "Cannot complete action while offline.");
				return;
			}

			ResponseManager.ShowLoading ("Fetching user data...");

			ThreadPool.QueueUserWorkItem (o => {
				while (!_checkedStatus)
					Thread.Sleep (100);

				FetchStudentData ();
			});
		}

		private void FetchStudentData ()
		{
			var data = new NameValueCollection ();
			data.Add ("checkuser", string.Empty);

			string rawReply = WebFunctions.Request (data);

			if (!rawReply.Contains ("Registered"))
			{
				StartActivity (typeof(LoginActivity));
				Finish ();
			}

			string[] reply = Regex.Split (rawReply, "<br>").TrimArray ();

			Regex userMatch = new Regex (":.*");
			string user = (from a in reply
			               where !string.IsNullOrWhiteSpace (userMatch.Match (a).Value)
			               select userMatch.Match (a).Value.Remove (0, 2)).First ();

			var listView = FindViewById<ListView> (Resource.Id.listView1);

			var adapter = new ArrayAdapter<String> (this, Android.Resource.Layout.SimpleListItem1, reply);
			RunOnUiThread (() => listView.Adapter = adapter);

			ResponseManager.DismissLoading ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			if (data == null)
				return;

			if (requestCode == 1)
			{
				if (data.GetStringExtra ("error") == null)
					_pendingMessage = data.GetStringExtra ("reply");
				else
					_pendingMessage = data.GetStringExtra ("error");
			}
		}

		private void CheckLogin ()
		{
			try
			{
				bool loggedIn = Functions.IsLoggedIn ();

				if (!loggedIn)
				{
					StartActivity (typeof(LoginActivity));
					Finish ();
				}
			} finally
			{
				_checkedStatus = true;
			}
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			var inflater = MenuInflater;
			inflater.Inflate (Resource.Menu.optionsMenu, menu);  

			if (!object.Equals (Functions.GetSetting ("settings", "accountType"), "student"))
				menu.FindItem (Resource.Id.action_refresh).SetVisible (false);

			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.action_settings)
			{
				if (Functions.IsOffline ())
				{
					ResponseManager.ShowMessage ("Error", "Cannot complete action while offline.");
					return base.OnOptionsItemSelected (item);
				}

				ResponseManager.ShowLoading ("Logging out...");
				ThreadPool.QueueUserWorkItem (o => DoLogout ());
			} else if (item.ItemId == Resource.Id.action_refresh)
			{
				if (Functions.IsOffline ())
				{
					ResponseManager.ShowMessage ("Error", "Cannot complete action while offline.");
					return base.OnOptionsItemSelected (item);
				}

				ResponseManager.ShowLoading ("Fetching user data...");
				ThreadPool.QueueUserWorkItem (o => FetchStudentData ());
			}

			return base.OnOptionsItemSelected (item);
		}

		private static void DoLogout ()
		{
			var data = new NameValueCollection ();
			data.Add ("logout", string.Empty);

			string response = WebFunctions.Request (data);
			if (response == "Logged out successfully" || response == "Not logged in!")
			{
				Functions.CurrentContext.StartActivity (typeof(LoginActivity));
				Functions.CurrentContext.Finish ();
			} else
			{
				ResponseManager.ShowMessage ("Error", response);
				ResponseManager.DismissLoading ();
			}
		}
	}
}