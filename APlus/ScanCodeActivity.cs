
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
using Android.Content.PM;

namespace APlus
{
	[Activity (Label = "ScanCode")]			
	public class ScanCodeActivity : Activity
	{
	    private SeekBar _seekBarGrade;
        private TextView _txtViewGrade;
		private string _userCode;

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

			if (string.IsNullOrEmpty (_userCode)) {
				Finish ();
				return;
			}

            _txtViewGrade = FindViewById<TextView>(Resource.Id.txtViewGrade);
            _seekBarGrade = FindViewById<SeekBar>(Resource.Id.seekBarGrade);

            _seekBarGrade.ProgressChanged += seekBar_ProgressChanged;
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

			if (data == null)
				return;

			string result = data.GetStringExtra("la.droid.qr.result");

			if (string.IsNullOrEmpty (result))
				return;

			_userCode = Decrypt(result);
			Toast.MakeText (this, _userCode, ToastLength.Long).Show ();
		}

		private string Decrypt(string encryptedCode)
		{
			byte[] codeBytes = Convert.FromBase64String (encryptedCode);

			for (int i = 0; i < codeBytes.Length; i++)
			{
				codeBytes[i] = (byte) (codeBytes[i] ^ GetCode());
			}

			return Encoding.UTF8.GetString(codeBytes);
		}

		private int GetCode()
		{
			int code = 0;

			foreach (char @char in "APlus")
				code += @char;

			return code;
		}

		private bool IsPackageInstalled(String packageName, Context context) 
		{
			PackageManager pm = context.PackageManager;

			try {
				pm.GetPackageInfo(packageName, PackageInfoFlags.Activities);
				return true;
			} catch (PackageManager.NameNotFoundException e) {
				return false;
			}
		}
	}
}

