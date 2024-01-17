using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000002 RID: 2
	public class ApiClient
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public ApiClient(string webSiteUrl)
		{
			this._cookieContainer = new CookieContainer();
			this._webSiteUrl = webSiteUrl;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000206C File Offset: 0x0000026C
		private HttpWebRequest CreateHttpRequest(string url)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.CookieContainer = this._cookieContainer;
			httpWebRequest.Method = "POST";
			return httpWebRequest;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020A4 File Offset: 0x000002A4
		public string UploadData(string url, string filename, byte[] data)
		{
			HttpWebRequest httpWebRequest = this.CreateHttpRequest(url);
			httpWebRequest.ContentType = "application/octet-stream";
			httpWebRequest.ContentLength = (long)data.Length;
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(data, 0, data.Length);
				requestStream.Flush();
			}
			string result;
			using (Stream responseStream = httpWebRequest.GetResponse().GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					result = streamReader.ReadToEnd();
				}
			}
			return result;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002158 File Offset: 0x00000358
		public byte[] DownloadData(string url)
		{
			List<byte> list = new List<byte>();
			byte[] array = new byte[1048576];
			HttpWebRequest httpWebRequest = this.CreateHttpRequest(url);
			httpWebRequest.Method = "GET";
			using (Stream responseStream = httpWebRequest.GetResponse().GetResponseStream())
			{
				int num;
				while ((num = responseStream.Read(array, 0, array.Length)) > 0)
				{
					byte[] array2 = new byte[num];
					Array.Copy(array, array2, num);
					list.AddRange(array2);
				}
			}
			return list.ToArray();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021FC File Offset: 0x000003FC
		public string Login(string dbId, string username, string appid, string appSecret, int lcid)
		{
			object[] parameters = new object[]
			{
				dbId,
				username,
				appid,
				appSecret,
				lcid
			};
			return this.Execute("Kingdee.BOS.WebApi.ServicesStub.AuthService.LoginByAppSecret", parameters);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000223C File Offset: 0x0000043C
		public string Save(string formId, string modelData)
		{
			object[] parameters = new object[]
			{
				formId,
				modelData
			};
			return this.Execute("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", parameters);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000226C File Offset: 0x0000046C
		private string Execute(string servicename, object[] parameters = null)
		{
			JObject jobject = new JObject();
			jobject.Add("format", 1);
			jobject.Add("useragent", "ApiClient");
			jobject.Add("rid", Guid.NewGuid().ToString().GetHashCode().ToString());
			jobject.Add("parameters", JsonConvert.SerializeObject(parameters));
			jobject.Add("timestamp", DateTime.Now);
			jobject.Add("v", "1.0");
			string s = jobject.ToString();
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			string url = this._webSiteUrl + servicename + ".common.kdsvc";
			HttpWebRequest httpWebRequest = this.CreateHttpRequest(url);
			httpWebRequest.ContentType = "application/json";
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Flush();
			}
			string result;
			using (Stream responseStream = httpWebRequest.GetResponse().GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					string responseText = streamReader.ReadToEnd();
					result = this.ValidateResult(responseText);
				}
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000023FC File Offset: 0x000005FC
		private string Encode(object data)
		{
			string s = "KingdeeK";
			string s2 = "KingdeeK";
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			byte[] bytes2 = Encoding.ASCII.GetBytes(s2);
			byte[] inArray = null;
			int length = 0;
			using (DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider())
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					ICryptoTransform transform = descryptoServiceProvider.CreateEncryptor(bytes, bytes2);
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
					{
						using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
						{
							streamWriter.Write(data);
							streamWriter.Flush();
							cryptoStream.FlushFinalBlock();
							streamWriter.Flush();
							inArray = memoryStream.GetBuffer();
							length = (int)memoryStream.Length;
						}
					}
				}
			}
			return Convert.ToBase64String(inArray, 0, length);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000251C File Offset: 0x0000071C
		private string ValidateResult(string responseText)
		{
			bool flag = responseText.StartsWith("response_error:");
			if (flag)
			{
				throw new Exception(responseText);
			}
			return responseText;
		}

		// Token: 0x04000001 RID: 1
		private readonly CookieContainer _cookieContainer;

		// Token: 0x04000002 RID: 2
		private string _webSiteUrl;
	}
}
