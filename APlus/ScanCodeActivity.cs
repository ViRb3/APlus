
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

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ScanCode);

			if (!IsPackageInstalled ("la.droid.qr", this)) {
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetTitle("Error");
				builder.SetMessage("The external application \"QR Droid\" is required but not installed.\r\nWould you like to do that now?");
				builder.SetCancelable(false);
				builder.SetPositiveButton("Yes", delegate { GetQRDroid(); });
				builder.SetNegativeButton ("No", delegate { Finish(); });
				builder.Show();

				return;
			}

			Intent qrDroid = new Intent("la.droid.qr.scan");
			qrDroid.PutExtra("la.droid.qr.complete" , true);
			StartActivityForResult(qrDroid, 0);

            _txtViewGrade = FindViewById<TextView> (Resource.Id.txtViewGrade);
			_editTextSubject = FindViewById<EditText> (Resource.Id.editTextSubject);

			_seekBarGrade = FindViewById<SeekBar> (Resource.Id.seekBarGrade);
            _seekBarGrade.ProgressChanged += seekBar_ProgressChanged;

			_btnGradeCommit = FindViewById<Button>(Resource.Id.btnGradeCommit);
			_btnGradeCommit.Click += DoCommit;

			/*var data = new NameValueCollection ();
			data.Add ("getstudents", string.Empty);

			string reply = WebFunctions.Request (data);
			string[] students = Regex.Split (reply, System.Environment.NewLine);

			if (!reply.Contains (System.Environment.NewLine) || students.Length < 1) {
				Intent resultData = new Intent();
				resultData.PutExtra("error", reply);
				SetResult(Result.Ok, resultData);
				Finish ();
				return;
			}*/

			/*_spinnerStudent = FindViewById<Spinner> (Resource.Id.spinnerStudent);
			_spinnerStudent.Adapter = new ArrayAdapter (this, Resource.Layout.SpinnerItem, students);
			var data = new NameValueCollection();
			data.Add ("getsubjects", string.Empty);
			string reply = WebFunctions.Request (data);

			if (string.IsNullOrWhiteSpace (reply)) {
				Toast.MakeText (Application.Context, "Cannot retrieve subjects!", ToastLength.Long);
				Finish ();
				return;
			}*/
		}

		void DoCommit (object sender, EventArgs e)
		{
			Intent resultData;

			if (string.IsNullOrWhiteSpace (_editTextSubject.Text)) {
				Toast.MakeText (this, "Subject cannot be empty!", ToastLength.Long).Show ();
				return;
			}

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

			reply = WebFunctions.Request (data);

			resultData = new Intent();
			resultData.PutExtra("reply", reply);
			SetResult(Result.Ok, resultData);
			Finish ();
		}

		private void GetQRDroid()
		{
			StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("market://details?id=la.droid.qr")));
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

			if (string.IsNullOrEmpty (result))
				ThrowError ();

			string rawUserCode = Decrypt(result);

			if (string.IsNullOrEmpty (rawUserCode))
				ThrowError ();

			_userCode = Regex.Split (rawUserCode, ":");

			if (_userCode.Length != 3 || _userCode.Any(code => string.IsNullOrWhiteSpace(code)))
				ThrowError ();
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

