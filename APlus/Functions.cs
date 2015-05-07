using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Net;

namespace APlus
{
	public static class Functions
	{
		public static Activity CurrentContext;
		static Random _random = new Random();

		public static string[] ShuffleArray(this string[] array)
		{
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();

			foreach (string @string in array)
			{
				list.Add(new KeyValuePair<int, string>(_random.Next(), @string));
			}

			var sorted = from item in list
				orderby item.Key
				select item;

			string[] result = new string[array.Length];

			int index = 0;
			foreach (KeyValuePair<int, string> pair in sorted)
			{
				result[index] = pair.Value;
				index++;
			}

			return result;
		}

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

		public static void SaveSettings (string settingName, string subSettingName, string[] values)
		{

			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var preferencesEditor = preferences.Edit ();

			preferencesEditor.PutInt(subSettingName +"_size", values.Length);

			for(int i = 0; i < values.Length; i++)  
				preferencesEditor.PutString(subSettingName + "_" + i, values[i]);  

			preferencesEditor.Commit ();
		}

		public static string GetSetting (string settingName, string subSettingName)
		{
			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);
			var setting = preferences.GetString (subSettingName, null);

			return setting;
		}

		public static string[] GetSettings (string settingName, string subSettingName)
		{
			var preferences = Application.Context.GetSharedPreferences (settingName, FileCreationMode.Private);

			int size = preferences.GetInt(subSettingName + "_size", 0);  
			String[] settings = new string[size]; 

			for(int i = 0; i < size; i++)  
				settings[i] = preferences.GetString(subSettingName + "_" + i, null);  

			return settings;
		}

		public static string[] TrimArray (this string[] array)
		{
			List<string> listArray = new List<string> (array);

			for (int i = array.Length - 1; i >= 0; i--)
				if (string.IsNullOrWhiteSpace(listArray[i]))
					listArray.RemoveAt (i);

			return listArray.ToArray ();
		}
	}
}