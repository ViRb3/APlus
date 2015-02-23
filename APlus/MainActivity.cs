
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace APlus
{
	[Activity (Label = "MainActivity")]			
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			var inflater = MenuInflater;
			inflater.Inflate(Resource.Menu.optionsMenu, menu);        
			return true;
		}
			
		public override bool OnOptionsItemSelected(IMenuItem item) 
		{
			Toast.MakeText(Application.Context, "To be implemented!", ToastLength.Short).Show();
			return base.OnOptionsItemSelected (item);
		}
	}
}

