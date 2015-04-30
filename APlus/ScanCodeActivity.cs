using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using System.Collections.Specialized;
using System.Threading;

namespace APlus
{
	[Activity (Label = "ScanCode")]			
	public class ScanCodeActivity : Activity
	{
	    private SeekBar _seekBarGrade;
        private TextView _txtViewGrade;
		private Button _btnGradeCommit;
		private EditText _editTextSubject;

		private string _qrCode;
		private bool _scannedCode;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;
			SetContentView (Resource.Layout.ScanCode);

			if (bundle != null)
				_scannedCode = bundle.GetBoolean ("scannedCode");

			if (!IsPackageInstalled ("la.droid.qr", this)) {
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetTitle("Error");
				builder.SetMessage("The external application \"QR Droid\" is required but not installed.\r\nWould you like to do that now?");
				builder.SetCancelable(false);
				builder.SetPositiveButton("Yes", delegate { GetQRDroid(); });
				builder.SetNegativeButton("No", delegate { Finish(); });
				builder.Show();

				return;
			}

			if (!_scannedCode) {
				Intent qrDroid = new Intent("la.droid.qr.scan");
				qrDroid.PutExtra("la.droid.qr.complete" , true);
				StartActivityForResult(qrDroid, 0);
			}

            _txtViewGrade = FindViewById<TextView> (Resource.Id.txtViewGrade);
			_editTextSubject = FindViewById<EditText> (Resource.Id.editTextSubject);

			_seekBarGrade = FindViewById<SeekBar> (Resource.Id.seekBarGrade);
            _seekBarGrade.ProgressChanged += seekBar_ProgressChanged;

			if (bundle != null) {
				_seekBarGrade.Progress = bundle.GetInt ("grade") - 2;
				_editTextSubject.Text = bundle.GetString ("subject");
				_qrCode = bundle.GetString ("qrCode");
			}

			_btnGradeCommit = FindViewById<Button>(Resource.Id.btnGradeCommit);
			_btnGradeCommit.Click += Commit;
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;
		}

		protected override void OnSaveInstanceState(Bundle bundle) 
		{
			bundle.PutBoolean("scannedCode", _scannedCode);
			bundle.PutInt("grade", int.Parse(_txtViewGrade.Text));
			bundle.PutString("subject", _editTextSubject.Text);
			bundle.PutString("qrCode", _qrCode);
		}

		private void Commit (object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace (_editTextSubject.Text)) {
				ResponseManager.ShowMessage ("Error", "Subject cannot be empty!");
				return;
			}

			if (Functions.IsOffline()) {
				ResponseManager.ShowMessage ("Error", "Cannot complete action while offline.");
				return;
			}

			ResponseManager.ShowLoading ("Saving grade...");
			ThreadPool.QueueUserWorkItem (o => DoCommit ());
		}

		private void DoCommit()
		{
			Intent resultData;

			var data = new NameValueCollection();
			data.Add ("newgrade", string.Empty);
			data.Add ("subject", _editTextSubject.Text);
			data.Add ("grade", _txtViewGrade.Text);
			data.Add ("code", _qrCode);

			string reply = WebFunctions.Request (data);

			if (string.IsNullOrWhiteSpace(reply) || reply == "Error") {
				ThrowError ();
				return;
			}

			resultData = new Intent();
			resultData.PutExtra("reply", reply);
			SetResult(Result.Ok, resultData);
			Finish ();
		}

		private void GetQRDroid()
		{
			Intent playStore = new Intent (Intent.ActionView, Android.Net.Uri.Parse ("market://details?id=la.droid.qr"));
			playStore.AddFlags(ActivityFlags.NewTask);
			StartActivity(playStore);
			Finish ();
		}

        private void seekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            _txtViewGrade.Text = (e.Progress + 2).ToString();
        }

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (data == null) {
				Finish ();
				return;
			}

			string result = data.GetStringExtra("la.droid.qr.result");

			if (string.IsNullOrWhiteSpace (result)) {
				ThrowError ();
				return;
			}

			_scannedCode = true;
			_qrCode = result;
		}

		private void ThrowError(string message = "Invalid QR code scanned!")
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle("Error");
			builder.SetMessage(message);
			builder.SetCancelable(false);
			builder.SetNeutralButton ("OK", delegate { Finish(); });
			builder.Show();
		}

		private bool IsPackageInstalled(String packageName, Context context) 
		{
			PackageManager pm = context.PackageManager;

			try {
				pm.GetPackageInfo(packageName, PackageInfoFlags.Activities);
				return true;
			} catch (PackageManager.NameNotFoundException) {
				return false;
			}
		}
	}
}

