using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Android.App;
using Android.Content;
using System.Collections.Specialized;
using Android.Widget;
using System.Diagnostics;
using Android.Net;
using System.Collections.Generic;

namespace APlus
{
	public static class Functions
	{
		public static Activity CurrentContext;

		public static bool IsOffline() 
		{
			var connectivityManager = (ConnectivityManager)CurrentContext.GetSystemService(Context.ConnectivityService);
			NetworkInfo networkInfo = connectivityManager.ActiveNetworkInfo;

			if (networkInfo == null || !networkInfo.IsConnected)
				return true;
			else
				return false;
		}

		public static bool IsLoggedIn()
		{
			var data = new NameValueCollection();
			data.Add("login", string.Empty);

			if (WebFunctions.Request (data) == "Already logged in!")
				return true;

			return false;	
		}

		public static string GetMd5(string input)
		{
			var md5 = MD5.Create();
			byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

			StringBuilder stringBuilder = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			{
				stringBuilder.Append(data[i].ToString("x2"));
			}
				
			return stringBuilder.ToString();
		}
			
		public static void DeleteSetting(string settingName)
		{
			var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit();

			preferencesEditor.Clear ();
			preferencesEditor.Commit ();
		}

		public static void DeleteSetting(string settingName, string subSettingName)
		{
			var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit();

			preferencesEditor.Remove (subSettingName);
			preferencesEditor.Commit ();
		}

        public static void SaveSetting(string settingName, Tuple<string, string>[] settings)
        {

            var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
            var preferencesEditor = preferences.Edit();

            foreach (var setting in settings)
                preferencesEditor.PutString(setting.Item1, setting.Item2);

            preferencesEditor.Commit();
        }

		public static void SaveSetting(string settingName, string subSettingName, string value)
		{

			var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit();

			preferencesEditor.PutString(subSettingName, value);

			preferencesEditor.Commit();
		}

        public static string GetSetting(string settingName, string subSettingName)
        {
            var preferences = Application.Context.GetSharedPreferences(settingName, FileCreationMode.Private);
			var setting = preferences.GetString(subSettingName, null);

            return setting;
        }

		public static string[] TrimArray(this string[] array)
		{
			List<string> listArray = new List<string> (array);

			while(string.IsNullOrWhiteSpace(listArray[listArray.Count - 1]))
				listArray.RemoveAt(listArray.Count - 1);

			return listArray.ToArray();
		}
	} 
}