using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace APlus
{
	public static class ScannedCodesCollection
	{
		private static List<ScannedCode> _scannedCodes = new List<ScannedCode> ();

		public static void AddCode(ScannedCode code)
		{
			_scannedCodes.Add (code);
			Save (code);
		}

		public static bool CodeExists(string code)
		{
			Load ();

			foreach (var existingCode in _scannedCodes)
				if (existingCode.Code == code)
					return true;

			return false;
		}

		public static string SerializeCode(ScannedCode code)
		{
			IFormatter formatter = new BinaryFormatter ();

			using (MemoryStream memoryStream = new MemoryStream ())
			{
				formatter.Serialize(memoryStream, code);
				return Convert.ToBase64String(memoryStream.ToArray ());
			}
		}

		public static ScannedCode DeserializeCode(string code)
		{
			IFormatter formatter = new BinaryFormatter ();
			byte[] serializedClass = Convert.FromBase64String(code);

			using (MemoryStream memoryStream = new MemoryStream (serializedClass))
			{
				return (ScannedCode) formatter.Deserialize(memoryStream);
			}
		}
			
		public static void Save(ScannedCode code)
		{
			Functions.SaveSetting ("settings", "savedCode" + (CountSavedCodes () + 1), SerializeCode (code));
		}

		public static void Load()
		{
			_scannedCodes.Clear ();
			int index = 1;

			while (true)
			{
				string serializedClass = Functions.GetSetting ("settings", "savedCode" + index);

				if (string.IsNullOrEmpty(serializedClass))
					break;

				ScannedCode code = DeserializeCode (serializedClass);
				_scannedCodes.Add (code);

				index++;
			}
		}

		public static int CountSavedCodes()
		{
			int index = 1;

			while (true)
			{
				string serializedClass = Functions.GetSetting ("settings", "savedCode" + index);

				if (string.IsNullOrEmpty (serializedClass))
					if (index == 1)
						return 0;
					else
						break;

				index++;
			}

			return index;
		}

		public static void DeleteCode(ScannedCode code)
		{
			int index = _scannedCodes.IndexOf (code);
			_scannedCodes.RemoveAt (index);
			Functions.DeleteSetting ("settings", "savedCode" + (index + 1));
		}

		public static void DeleteAllCodes()
		{
			Load ();

			for(int i = 0; i < _scannedCodes.Count; i++)
				DeleteCode (_scannedCodes[i]);

			ResponseManager.ShowMessage ("Success", "All local grades deleted!");
		}
			
		public static void Sync()
		{
			if (_scannedCodes.Count == 0)
			{
				Load ();

				if (_scannedCodes.Count == 0)
				{
					ResponseManager.ShowMessage ("Note", "No grades to sync!");
					return;
				}
			}
			StringBuilder stringBuilder = new StringBuilder ();

			foreach (var code in _scannedCodes)
			{
				stringBuilder.AppendFormat ("{0}-|-{1}-|-{2}-||-", code.Subject, code.Grade, code.Code);
			}

			var data = new NameValueCollection ();
			data.Add ("newgrades", string.Empty);
			data.Add ("data", stringBuilder.ToString());

			string reply = WebFunctions.Request (data);
			string[] formattedReply = Regex.Split (reply, Environment.NewLine);

			foreach (string line in formattedReply)
			{
				if (line.StartsWith ("Grade saved"))
					DeleteCode (_scannedCodes [0]);
				else if (line.StartsWith ("Code already used"))
					DeleteCode (_scannedCodes [0]);
			}

			reply = reply.Replace ("Grade saved!", string.Empty);
			reply = reply.Replace ("Code already used!", string.Empty);
			formattedReply = Regex.Split (reply, Environment.NewLine);
			formattedReply = Functions.TrimArray (formattedReply);

			List<string> gradedStudents = new List<string> ();
			StringBuilder resultMessage = new StringBuilder ();

			foreach (string line in formattedReply)
			{

				if (line.StartsWith ("Graded student"))
					gradedStudents.Add (line.Replace ("Graded student: ", string.Empty));
				else
				{
					foreach (string line2 in formattedReply)
					{
						resultMessage.AppendLine (line2);
					}

					Functions.CurrentContext.RunOnUiThread (() => ResponseManager.ShowMessage ("Success", resultMessage.ToString ()));
					return;
				}
			}
				
			resultMessage.AppendLine ("Grades successfully synced to server!");
			resultMessage.AppendLine ("Graded students:");
			resultMessage.AppendLine ();

			foreach (string student in Functions.ShuffleArray(gradedStudents.ToArray()))
			{
				resultMessage.AppendLine (student);
			}

			Functions.CurrentContext.RunOnUiThread (() => ResponseManager.ShowMessage ("Success", resultMessage.ToString ()));
		}

		public static ScannedCode GetFullCodeFromCode(string code)
		{
			Load ();
			return _scannedCodes.First (sc => sc.Code == code);
		}
	}

	[Serializable]
	public class ScannedCode
	{
		private readonly string _subject;

		public string Subject
		{
			get
			{
				return _subject;
			}
		}

		private readonly string _code;

		public string Code
		{
			get
			{
				return _code;
			}
		}
			
		private readonly int _grade;

		public int Grade
		{
			get
			{
				return _grade;
			}
		}

		public ScannedCode (string subject, int grade, string code)
		{
			_subject = subject;
			_grade = grade;
			_code = code;
		}
	}
}

