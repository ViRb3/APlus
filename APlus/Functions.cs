using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Net;

namespace APlus
{
	public static class Functions
	{
		public static Activity CurrentContext;

		public static bool IsOffline ()
		{
			var connectivityManager = (ConnectivityManager)CurrentContext.GetSystemService (Context.ConnectivityService);
			NetworkInfo networkInfo = connectivityManager.ActiveNetworkInfo;

			if (networkInfo == null || !networkInfo.IsConnected)
				return true;
			else
				return false;
		}

		public static bool IsLoggedIn ()
		{
			var data = new NameValueCollection ();
			data.Add ("login", string.Empty);

			if (WebFunctions.Request (data) == "Already logged in!")
				return true;

			return false;	
		}

		public static string GetSha256 (string input)
		{
			SHA256Managed sha256 = new SHA256Managed ();
			byte[] hash = sha256.ComputeHash (Encoding.UTF8.GetBytes (input));

			StringBuilder stringBuilder = new StringBuilder ();

			foreach (byte @byte in hash)
			{
				stringBuilder.Append (@byte.ToString ("x2"));
			}

			return stringBuilder.ToString ();
		}

		public static void DeleteSetting (string settingName)
		{
			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit ();

			preferencesEditor.Clear ();
			preferencesEditor.Commit ();
		}

		public static void DeleteSetting (string settingName, string subSettingName)
		{
			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit ();

			preferencesEditor.Remove (subSettingName);
			preferencesEditor.Commit ();
		}

		public static void SaveSetting (string settingName, Tuple<string, string>[] settings)
		{

			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit ();

			foreach (var setting in settings)
				preferencesEditor.PutString (setting.Item1, setting.Item2);

			preferencesEditor.Commit ();
		}

		public static void SaveSetting (string settingName, string subSettingName, string value)
		{

			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit ();

			preferencesEditor.PutString (subSettingName, value);

			preferencesEditor.Commit ();
		}

		public static string GetSetting (string settingName, string subSettingName)
		{
			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var setting = preferences.GetString (subSettingName, null);

			return setting;
		}

		public static string[] TrimArray (this string[] array)
		{
			List<string> listArray = new List<string> (array);

			while (listArray.Count - 1 >= 0 && string.IsNullOrWhiteSpace(listArray[listArray.Count - 1]))
				listArray.RemoveAt (listArray.Count - 1);

			return listArray.ToArray ();
		}
	}
}