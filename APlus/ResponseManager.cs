using Android.App;

namespace APlus
{
	public static class ResponseManager
	{
		private static ProgressDialogFragment _progressDialog = null;

		public static bool ShowLoading (string message)
		{
			Functions.CurrentContext.RunOnUiThread (() => {
				Functions.CurrentContext.RequestedOrientation = Android.Content.PM.ScreenOrientation.Locked;
				_progressDialog = new ProgressDialogFragment();
				_progressDialog.Initialize(message, string.Empty);
				_progressDialog.Show();
			});
			
			return true;
		}

		public static bool DismissLoading ()
		{
			if (_progressDialog == null)
				return false;

			_progressDialog.Dismiss ();
			_progressDialog = null;
			Functions.CurrentContext.RequestedOrientation = Android.Content.PM.ScreenOrientation.User;
			return true;
		}

		public static bool ShowMessage (string title, string message)
		{
			Functions.CurrentContext.RunOnUiThread (() => {
				var dialogFragment = new DialogFragment();
				dialogFragment.InitializeOk(message, title, delegate { }, true);
				dialogFragment.Show();
			});

			return true;
		}
	}
}