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

namespace APlus
{
	[Activity (Label = "APlus", MainLauncher = true, Icon = "@drawable/icon")]			
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			string loggedIn = Functions.GetSetting ("settings", "loggedIn");

			if (!object.Equals (loggedIn, "true")) {
				StartActivity (typeof(LoginActivity));
				Finish ();
			} 
			else new Thread (CheckLogin).Start ();

			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;

			var gridview = FindViewById<GridView>(Resource.Id.gridview);
			gridview.Adapter = new CustomAdapter(this);

			gridview.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
			{
				Toast.MakeText(this, args.Position.ToString(), ToastLength.Short).Show();
			};
		}

		private void CheckLogin()
		{
			Looper.Prepare();

			try {
				bool loggedIn = Functions.IsLoggedIn();
				Functions.DeleteSetting("settings", "offline");

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
				Functions.DeleteSetting ("signedInCookie");
				Functions.DeleteSetting ("settings", "loggedIn");
				WebFunctions.ClearCookies ();
				StartActivity (typeof(LoginActivity));
				Finish ();
			} else
				Toast.MakeText (Application.Context, response, ToastLength.Long).Show ();

			return base.OnOptionsItemSelected (item);
		}
	}
}

