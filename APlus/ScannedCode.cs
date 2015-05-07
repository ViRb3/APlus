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
		private static readonly List<ScannedCode> _scannedCodes = new List<ScannedCode> ();

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
			Load ();
			Functions.SaveSetting ("savedCodes", "code" + (_scannedCodes.Count), SerializeCode (code));
		}

		public static void Load()
		{
			if (_scannedCodes.Count > 0)
				return;
			
			int index = 1;

			while (true)
			{
				string serializedClass = Functions.GetSetting ("savedCodes", "code" + index);

				if (string.IsNullOrEmpty(serializedClass))
					break;

				ScannedCode code = DeserializeCode (serializedClass);
				_scannedCodes.Add (code);

				index++;
			}
		}

		public static void DeleteFirstCode()
		{
			_scannedCodes.RemoveAt (0);
			Functions.DeleteSetting ("savedCodes", "code1");
			FixCodeIndexes (2);
		}

		public static void FixCodeIndexes(int firstIndex)
		{
			while (true)
			{
				string serializedClass = Functions.GetSetting ("savedCodes", "code" + firstIndex);

				if (string.IsNullOrEmpty (serializedClass))
				{
					Functions.DeleteSetting ("savedCodes", "code" + (firstIndex - 1)); // clear old duplicate
					break;
				}

				Functions.SaveSetting ("savedCodes", "code" + (firstIndex - 1), serializedClass);

				firstIndex++;
			}
		}

		public static void DeleteAllCodes()
		{
			Functions.DeleteSetting ("savedCodes");
			_scannedCodes.Clear ();

			ResponseManager.ShowMessage ("Success", "All local grades deleted!");
		}

		public static void DeleteCode(ScannedCode code)
		{
			Load ();

			int index = 1;

			while (true)
			{
				string serializedClass = Functions.GetSetting ("savedCodes", "code" + index);

				if (string.IsNullOrEmpty(serializedClass))
					break;

				ScannedCode deserializedCode = DeserializeCode (serializedClass);

				if (code.Code == deserializedCode.Code)
				{
					Functions.DeleteSetting ("savedCodes", "code" + index);
					_scannedCodes.Remove (code);
					FixCodeIndexes (index + 1);
					break;
				}

				index++;
			}
		}
			
		public static void Sync()
		{
			Load ();

			if (_scannedCodes.Count == 0)
			{
				ResponseManager.ShowMessage ("Note", "No grades to sync!");
				return;
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
					DeleteFirstCode ();
				else if (line.StartsWith ("Code already used"))
					DeleteFirstCode ();
			}

			reply = reply.Replace ("Grade saved!", string.Empty);
			reply = reply.Replace ("Code already used!", string.Empty);
			formattedReply = Regex.Split (reply, Environment.NewLine);
			formattedReply = formattedReply.TrimArray();

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

			if (gradedStudents.Count == 0)
			{
				resultMessage.AppendLine ("All sent codes were already graded!");
			} 
			else
			{
				resultMessage.AppendLine ("Grades successfully synced to server! Results:");
				resultMessage.AppendLine ();

				foreach (string student in Functions.ShuffleArray(gradedStudents.ToArray()))
				{
					resultMessage.AppendLine (student);
				}
			}
				
			Functions.CurrentContext.RunOnUiThread (() => ResponseManager.ShowMessage ("Success", resultMessage.ToString ()));
		}

		public static ScannedCode GetFullCodeFromCode(string code)
		{
			Load ();
			return _scannedCodes.First (sc => sc.Code == code);
		}

		public static int CodesCount()
		{
			Load ();
			return _scannedCodes.Count;
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

