using System;
using System.Net;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace APlus
{
	public static class Functions
	{
		static readonly string _server = "http://microcast.mcserver.ws/aplus/service.php";

		public static string Request(NameValueCollection data)
		{
			using (var wb = new WebClient())
			{
				byte[] response = wb.UploadValues(_server, "POST", data);
				string result = wb.Encoding.GetString(response);

				return result;
			}
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
	}
}