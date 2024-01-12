using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Newtonsoft.Json;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000015 RID: 21
	public class UpdateSrmStatus_Service
	{
		// Token: 0x0600009A RID: 154 RVA: 0x00008150 File Offset: 0x00006350
		public static void SendToSrm(Context context, SRMStatus info)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("exectime", info.exectime);
			dictionary.Add("company", info.company);
			dictionary.Add("vendid", info.vendid);
			dictionary.Add("stmtnums", info.stmtnums);
			dictionary.Add("paynums", info.paynums);
			string requestJson = JsonConvert.SerializeObject(dictionary);
			Helper.WriteLog(info.exectime, requestJson);
			string value = Utils.HttpPost(UpdateSrmStatus_Service.url, dictionary);
			SrmResponseInfo srmResponseInfo = JsonConvert.DeserializeObject<SrmResponseInfo>(value);
			bool flag = srmResponseInfo.msg.ToLower() != "success";
			if (flag)
			{
				throw new Exception("修改SRM状态失败：" + srmResponseInfo.msg);
			}
		}

		// Token: 0x04000038 RID: 56
		private static string url = "http://10.10.111.70:8088/cxbiz/stmt/erpUpdStatus";
	}
}
