using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using System.Text;

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
				ShowCodeAlreadyScannedDialog();
				return;
			}

			if (!IsPackageInstalled ("la.droid.qr", this))
			{
				var dialogFragment = new DialogFragment ();
				dialogFragment.InitializeYesNo ("The external application \"QR Droid\" is required but not installed.\r\nWould you like to do that now?", "Error", 
					delegate { GetQRDroid (); }, 
					delegate { Finish ();
				});

				dialogFragment.Show ();
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
			if (!CheckDataOk ())
				return;
			
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
			if (!CheckDataOk ())
				return;
			
			ScannedCode code = new ScannedCode (_editTextSubject.Text, int.Parse (_txtViewGrade.Text), _qrCode);
			ScannedCodesCollection.AddCode(code);

			Intent intent = new Intent (this, typeof(ScanCodeActivity));
			Bundle bundle = new Bundle ();
			bundle.PutString ("subject", _editTextSubject.Text);
			intent.PutExtras (bundle);

			StartActivity(intent);
			Finish ();
		}		

		private bool CheckDataOk()
		{
			if (string.IsNullOrWhiteSpace (_editTextSubject.Text))
			{
				ResponseManager.ShowMessage ("Error", "Subject cannot be empty!");
				return false;
			}

			return true;
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
				ResponseManager.ShowMessage ("Error", "Invalid QR code scanned!");
				return;
			}

			_qrCode = result;

			if (ScannedCodesCollection.CodeExists (result))
			{
				_codeAlreadyUsed = true;
				ShowCodeAlreadyScannedDialog();
				return;
			}

			_scannedCode = true;
		}

		private void ShowCodeAlreadyScannedDialog()
		{
			ScannedCode code = ScannedCodesCollection.GetFullCodeFromCode (_qrCode);

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.AppendLine (string.Format ("Code has already been graded at {0} with {1}!", code.Subject, code.Grade));
			stringBuilder.Append ("Do you want to delete the old grade and save a new one?");

			var dialogFragment = new DialogFragment ();
			dialogFragment.InitializeYesNo (stringBuilder.ToString (), "Question", delegate {
				ScannedCodesCollection.DeleteCode(code);
				_codeAlreadyUsed = false;
			}, delegate {
				Finish();
			});

			dialogFragment.Show ();
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

