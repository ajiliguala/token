using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
	// Token: 0x02000004 RID: 4
	[HotUpdate]
	public class yunzhijiaInterface
	{
		// Token: 0x0600000A RID: 10 RVA: 0x000032C0 File Offset: 0x000014C0
		public static long ConvertDateTimeToInt(DateTime time)
		{
			DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
			return (time.Ticks - dateTime.Ticks) / 10000L;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00003308 File Offset: 0x00001508
		private static string Post(string url, string content)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/json";
			string result = "";
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			httpWebRequest.ContentLength = (long)bytes.Length;
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = httpWebResponse.GetResponseStream();
			using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
			{
				result = streamReader.ReadToEnd();
			}
			return result;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000033E4 File Offset: 0x000015E4
		public string GetToken()
		{
			JObject jobject = new JObject();
			jobject["appId"] = "SP1663384";
			jobject["eid"] = "1663384";
			jobject["secret"] = "JE2S9DAvYzHDtr8FD6oHei4L07n8Qc";
			jobject["timestamp"] = yunzhijiaInterface.ConvertDateTimeToInt(DateTime.Now).ToString();
			jobject["scope"] = "team";
			string text = yunzhijiaInterface.Post("https://yunzhijia.com/gateway/oauth2/token/getAccessToken", Convert.ToString(jobject));
			JObject jobject2 = JObject.Parse(text);
			return Convert.ToString(jobject2["data"]["accessToken"]);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000034B0 File Offset: 0x000016B0
		public JObject ViewFormIns(string token, string formInstId, string formCodeId)
		{
			JObject result3;
			using (HttpClient httpClient = new HttpClient())
			{
				Uri requestUri = new Uri("https://yunzhijia.com/gateway/workflow/form/thirdpart/viewFormInst?accessToken=" + token);
				FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{
						"formInstId",
						formInstId
					},
					{
						"formCodeId",
						formCodeId
					}
				});
				HttpResponseMessage result = httpClient.PostAsync(requestUri, content).Result;
				string result2 = result.Content.ReadAsStringAsync().Result;
				JObject jobject = JObject.Parse(result2);
				result3 = jobject;
			}
			return result3;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00003548 File Offset: 0x00001748
		public JObject FindFlows(string token, string Name)
		{
			JObject jobject = new JObject();
			JArray jarray = new JArray();
			JArray jarray2 = new JArray();
			JArray jarray3 = new JArray();
			jarray.Add(Name);
			jarray3.Add("RUNNING");
			jarray2.Add("1d87e21742bd4bf5a190aa330c34f100");
			jobject["approvers"] = jarray;
			jobject["formCodeIds"] = jarray2;
			jobject["pageSize"] = "100";
			jobject["status"] = jarray3;
			string text = yunzhijiaInterface.Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/findFlows?accessToken=" + token, Convert.ToString(jobject));
			return JObject.Parse(text);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00003604 File Offset: 0x00001804
		public JObject GetTemplates(string token)
		{
			JObject result3;
			using (HttpClient httpClient = new HttpClient())
			{
				Uri requestUri = new Uri("https://yunzhijia.com/gateway/workflow/form/thirdpart/getTemplates?accessToken=" + token);
				FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>());
				HttpResponseMessage result = httpClient.PostAsync(requestUri, content).Result;
				string result2 = result.Content.ReadAsStringAsync().Result;
				JObject jobject = JObject.Parse(result2);
				result3 = jobject;
			}
			return result3;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00003684 File Offset: 0x00001884
		public string ModifyInst(string token, string formCodeId, string formDefId, string formInstId, string widgetValue)
		{
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			jobject["creator"] = "5d91b7cfe4b01b007c703bb7";
			jobject["formCodeId"] = formCodeId;
			jobject["formDefId"] = formDefId;
			jobject["formInstId"] = formInstId;
			jobject2["Te_7"] = widgetValue;
			jobject["widgetValue"] = jobject2;
			return yunzhijiaInterface.Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/modifyInst?accessToken=" + token, Convert.ToString(jobject));
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00003728 File Offset: 0x00001928
		public string GetFlowDetailById(string token)
		{
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			jobject["flowInstId"] = "5fd58808e6df1500014149db";
			jobject["activityCodeId"] = "3716f5fc19434ead8c2e62a8a4c0271f";
			jobject["activityType"] = "5fd587ee02c1040001a71df5";
			jobject["predictFlag"] = "5fd58808e6df1500014149db";
			return yunzhijiaInterface.Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/getFlowDetailById?accessToken=" + token, Convert.ToString(jobject));
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000037B8 File Offset: 0x000019B8
		public string Agree(string token, string flowInstId, string formCodeId, string formDefId, string formInstId, string value, string yzjUserId)
		{
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			JObject jobject3 = new JObject();
			JObject jobject4 = new JObject();
			jobject["approver"] = yzjUserId;
			jobject2["flowInstId"] = flowInstId;
			jobject2["opinion"] = "同意";
			jobject["flow"] = jobject2;
			jobject3["formCodeId"] = formCodeId;
			jobject3["formDefId"] = formDefId;
			jobject3["formInstId"] = formInstId;
			jobject4["Te_7"] = value;
			jobject3["widgetValue"] = jobject4;
			jobject["form"] = jobject3;
			return yunzhijiaInterface.Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/agree?accessToken=" + token, Convert.ToString(jobject));
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000038AC File Offset: 0x00001AAC
		public string RetrunBack(string token, string flowInstId, string formCodeId, string formDefId, string formInstId, string value)
		{
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			JObject jobject3 = new JObject();
			JObject jobject4 = new JObject();
			jobject["approver"] = "5d91b7cfe4b01b007c703bb7";
			jobject2["flowInstId"] = flowInstId;
			jobject2["opinion"] = "不同意";
			jobject2["returnStartActivity"] = true;
			jobject["flow"] = jobject2;
			jobject4["Te_7"] = value;
			jobject3["widgetValue"] = jobject4;
			jobject["form"] = jobject3;
			return yunzhijiaInterface.Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/return?accessToken=" + token, Convert.ToString(jobject));
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000397C File Offset: 0x00001B7C
		public string GetByGroupId(string token)
		{
			JObject value = new JObject();
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			JObject jobject3 = new JObject();
			return yunzhijiaInterface.Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/getByGroupId?accessToken=" + token, Convert.ToString(value));
		}
	}
}
