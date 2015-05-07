using Android.App;
using Android.Views;
using Android.Widget;

namespace APlus
{
	public static class ResponseManager
	{
		private static ProgressDialog _progressDialog = null;
		private static AlertDialog.Builder _alertDialog = null;

		public static bool ShowLoading (string message)
		{
			Functions.CurrentContext.RunOnUiThread (() => {
				_progressDialog = new ProgressDialog (Functions.CurrentContext);
				_progressDialog.SetProgressStyle (ProgressDialogStyle.Spinner);
				_progressDialog.SetMessage (message);
				_progressDialog.SetCanceledOnTouchOutside (false);
				_progressDialog.Show ();
			});
			
			return true;
		}

		public static bool DismissLoading ()
		{
			if (_progressDialog == null)
				return false;

			_progressDialog.Dismiss ();
			_progressDialog = null;
			return true;
		}

		public static bool ShowMessage (string title, string message)
		{
			Functions.CurrentContext.RunOnUiThread (() => {
				_alertDialog = new AlertDialog.Builder (Functions.CurrentContext);

				View view = Functions.CurrentContext.LayoutInflater.Inflate(Resource.Layout.ScrollableAlert, null);
				TextView textView = view.FindViewById<TextView> (Resource.Id.textView);
				textView.Text = message;
				_alertDialog.SetView(view);

				_alertDialog.SetTitle (title);
				_alertDialog.SetCancelable (false);
				_alertDialog.SetPositiveButton ("OK", delegate {
				});
				_alertDialog.Show ();
			});

			return true;
		}
	}
}