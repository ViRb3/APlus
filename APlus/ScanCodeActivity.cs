using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;

namespace APlus
{
	[Activity (Label = "ScanCode")]			
	public class ScanCodeActivity : Activity
	{
		private SeekBar _seekBarGrade;
		private TextView _txtViewGrade;
		private EditText _editTextSubject;

		private string _qrCode;
		private bool _scannedCode;
		private bool _codeAlreadyUsed;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Functions.CurrentContext = this;
			SetContentView (Resource.Layout.ScanCode);

			_txtViewGrade = FindViewById<TextView> (Resource.Id.txtViewGrade);
			_editTextSubject = FindViewById<EditText> (Resource.Id.editTextSubject);

			_seekBarGrade = FindViewById<SeekBar> (Resource.Id.seekBarGrade);
			_seekBarGrade.ProgressChanged += seekBar_ProgressChanged;

			_editTextSubject.Text = this.Intent.GetStringExtra ("subject");

			if (bundle != null)
			{
				_qrCode = bundle.GetString ("qrCode");
				_scannedCode = bundle.GetBoolean ("scannedCode");
				_codeAlreadyUsed = bundle.GetBoolean ("codeAlreadyUsed");
			}

			if (_codeAlreadyUsed)
			{
				ScannedCode code = ScannedCodesCollection.GetFullCodeFromCode (_qrCode);
				ThrowError (string.Format("Code has already been graded at {0} with {1}!", code.Subject, code.Grade));
				return;
			}

			if (!IsPackageInstalled ("la.droid.qr", this))
			{
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle ("Error");
				builder.SetMessage ("The external application \"QR Droid\" is required but not installed.\r\nWould you like to do that now?");
				builder.SetCancelable (false);
				builder.SetPositiveButton ("Yes", delegate {
					GetQRDroid ();
				});
				builder.SetNegativeButton ("No", delegate {
					Finish ();
				});
				builder.Show ();

				return;
			}

			if (!_scannedCode)
			{
				Intent qrDroid = new Intent ("la.droid.qr.scan");
				qrDroid.PutExtra ("la.droid.qr.complete", true);
				StartActivityForResult (qrDroid, 1);
			}

			if (bundle != null)
			{
				_seekBarGrade.Progress = bundle.GetInt ("grade") - 2;
				_editTextSubject.Text = bundle.GetString ("subject");
				_qrCode = bundle.GetString ("qrCode");
			}

			Button btnFinish = FindViewById<Button> (Resource.Id.btnFinish);
			btnFinish.Click += Finish;

			Button btnNext = FindViewById<Button> (Resource.Id.btnNext);
			btnNext.Click += Next;
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			Functions.CurrentContext = this;
		}

		protected override void OnSaveInstanceState (Bundle bundle)
		{
			bundle.PutBoolean ("codeAlreadyUsed", _codeAlreadyUsed);
			bundle.PutBoolean ("scannedCode", _scannedCode);
			bundle.PutInt ("grade", int.Parse (_txtViewGrade.Text));
			bundle.PutString ("subject", _editTextSubject.Text);
			bundle.PutString ("qrCode", _qrCode);
		}

		private void Finish (object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace (_editTextSubject.Text))
			{
				ResponseManager.ShowMessage ("Error", "Subject cannot be empty!");
				return;
			}
				
			ScannedCode code = new ScannedCode (_editTextSubject.Text, int.Parse (_txtViewGrade.Text), _qrCode);
			ScannedCodesCollection.AddCode (code);
			Finish ();
		}

		private void Next (object sender, EventArgs e)
		{
			ScannedCode code = new ScannedCode (_editTextSubject.Text, int.Parse (_txtViewGrade.Text), _qrCode);
			ScannedCodesCollection.AddCode(code);

			Intent intent = new Intent (this, typeof(ScanCodeActivity));
			Bundle bundle = new Bundle ();
			bundle.PutString ("subject", _editTextSubject.Text);
			intent.PutExtras (bundle);

			StartActivity(intent);
			Finish ();
		}			

		private void GetQRDroid ()
		{
			Intent playStore = new Intent (Intent.ActionView, Android.Net.Uri.Parse ("market://details?id=la.droid.qr"));
			playStore.AddFlags (ActivityFlags.NewTask);
			StartActivity (playStore);
			Finish ();
		}

		private void seekBar_ProgressChanged (object sender, SeekBar.ProgressChangedEventArgs e)
		{
			_txtViewGrade.Text = (e.Progress + 2).ToString ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode != 1)
				return;
			
			if (data == null)
			{
				Finish ();
				return;
			}

			string result = data.GetStringExtra ("la.droid.qr.result");

			if (string.IsNullOrWhiteSpace (result))
			{
				ThrowError ();
				return;
			}

			_qrCode = result;

			if (ScannedCodesCollection.CodeExists (result))
			{
				_codeAlreadyUsed = true;
				ScannedCode code = ScannedCodesCollection.GetFullCodeFromCode (_qrCode);
				ThrowError (string.Format("Code has already been graded at {0} with {1}!", code.Subject, code.Grade));
				return;
			}

			_scannedCode = true;
		}

		private void ThrowError (string message = "Invalid QR code scanned!")
		{
			AlertDialog.Builder builder = new AlertDialog.Builder (this);
			builder.SetTitle ("Error");
			builder.SetMessage (message);
			builder.SetCancelable (false);
			builder.SetNeutralButton ("OK", delegate {
				Finish ();
			});

			builder.Show ();
		}

		private bool IsPackageInstalled (String packageName, Context context)
		{
			PackageManager packageManager = context.PackageManager;

			try
			{
				packageManager.GetPackageInfo (packageName, PackageInfoFlags.Activities);
				return true;
			} catch (PackageManager.NameNotFoundException)
			{
				return false;
			}
		}
	}
}

