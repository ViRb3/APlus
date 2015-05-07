using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace APlus
{
	public enum DialogType
	{
		OK,
		YesNo
	}

	public class DialogFragment : Android.App.DialogFragment
	{
		public delegate void OnYes();
		public delegate void OnNo();

		private static string _message, _title;
		private static OnYes _onYes;
		private static OnNo _onNo;

		private bool _cancelable;
		private DialogType _dialogType;
		private AlertDialog.Builder _alertDialog;

		public void InitializeYesNo(string message, string title, OnYes onYes, OnNo onNo, bool cancelable = false)
		{
			_message = message;
			_title = title;
			_onYes = onYes;
			_onNo = onNo;
			_cancelable = cancelable;
			_dialogType = DialogType.YesNo;
		}

		public void InitializeOk(string message, string title, OnYes onYes, bool cancelable = false)
		{
			_message = message;
			_title = title;
			_onYes = onYes;
			_cancelable = cancelable;
			_dialogType = DialogType.OK;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			_alertDialog = new AlertDialog.Builder (Functions.CurrentContext);

			View view = Functions.CurrentContext.LayoutInflater.Inflate(Resource.Layout.ScrollableAlert, null);
			TextView textView = view.FindViewById<TextView> (Resource.Id.textView);
			textView.Text = _message;
			_alertDialog.SetView(view);

			_alertDialog.SetTitle (_title);
			_alertDialog.SetCancelable (_cancelable);

			if (_dialogType == DialogType.OK)
			{
				_alertDialog.SetPositiveButton ("OK", delegate {
					_onYes ();
				});
			} 
			else
			{
				_alertDialog.SetPositiveButton ("Yes", delegate {
					_onYes ();
				});

				_alertDialog.SetNegativeButton ("No", delegate {
					_onNo ();
				});
			}
				
			return _alertDialog.Show();
		}

		public void Show()
		{
			this.Show (Functions.CurrentContext.FragmentManager, string.Empty);
		}
	}
}