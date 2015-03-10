using System;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;
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
		private string[] _userCode;

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
				_userCode = bundle.GetStringArray ("userCode");
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
			bundle.PutStringArray("userCode", _userCode);
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
			data.Add("getstudentemail", string.Empty);
			data.Add("firstname", _userCode[0]);
			data.Add("lastname", _userCode[1]);
			data.Add("class", _userCode[2]);

			string reply = WebFunctions.Request (data);

			if (!reply.Contains ("@")) {
				resultData = new Intent();
				resultData.PutExtra("error", reply);
				SetResult(Result.Ok, resultData);
				Finish ();
				return;
			}

			data.Clear ();
			data.Add ("newgrade", string.Empty);
			data.Add ("subject", _editTextSubject.Text);
			data.Add ("grade", _txtViewGrade.Text);
			data.Add ("student", reply);
			data.Add ("code", _qrCode);

			string reply2 = WebFunctions.Request (data);

			resultData = new Intent();
			resultData.PutExtra("reply", new[] {reply2, string.Format("{0} {1} {2}", _userCode[0], _userCode[1], _userCode[2])});
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
			_qrCode = result;

			if (string.IsNullOrEmpty (result)) {
				ThrowError ();
				return;
			}

			try {
				string rawUserCode = Decrypt(result);

				if (string.IsNullOrEmpty (rawUserCode)) {
					ThrowError ();
					return;
				}

				_userCode = Regex.Split (rawUserCode, ":");

				if (_userCode.Length != 3 || _userCode.Any(code => string.IsNullOrWhiteSpace(code))) {
					ThrowError ();
					return;
				}
			}
			catch (Exception) {
				ThrowError ();
				return;
			}

			_scannedCode = true;
		}

		private void ThrowError()
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle("Error");
			builder.SetMessage("Invalid QR code scanned!");
			builder.SetCancelable(false);
			builder.SetNeutralButton ("OK", delegate { Finish(); });
			builder.Show();
		}

		private string Decrypt(string data)
		{
			byte[] dataBytes = Convert.FromBase64String(data);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(dataBytes, 0, dataBytes.Length);

				using (AesManaged aes = new AesManaged())
				{
					aes.Key = GetCode();

					byte[] iv = new byte[16];
					memoryStream.Position = 0;
					memoryStream.Read(iv, 0, 16);
					aes.IV = iv;

					using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read)) 
					using (StreamReader streamReader = new StreamReader(cryptoStream))
						return streamReader.ReadToEnd();        
				}
			}
		}

		private byte[] GetCode()
		{
			int code = 0;

			foreach (char @char in "APlus")
				code += @char;

			return MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(code.ToString()));
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

