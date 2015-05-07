using Android.App;
using Android.OS;


namespace APlus
{
	public class ProgressDialogFragment : Android.App.DialogFragment
	{
		private static string _message, _title;
		private static ProgressDialog _progressDialog;

		private bool _cancelable;


		public void Initialize(string message, string title, bool cancelable = false)
		{
			_message = message;
			_title = title;
			_cancelable = cancelable;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			_progressDialog = new ProgressDialog (Functions.CurrentContext);

			_progressDialog.SetTitle (_title);
			_progressDialog.SetMessage (_message);
			_progressDialog.SetProgressStyle (ProgressDialogStyle.Spinner);
			_progressDialog.SetCancelable (_cancelable);
			_progressDialog.SetCanceledOnTouchOutside (_cancelable);
			_progressDialog.Show ();

			return _progressDialog;
		}

		public override void Dismiss ()
		{
			_progressDialog.Dismiss ();
		}

		public void Show()
		{
			this.Show (Functions.CurrentContext.FragmentManager, string.Empty);
		}
	}
}