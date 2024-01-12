using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x0200001C RID: 28
	[Description("供应商推送接口到TPM")]
	[HotUpdate]
	public class BD_Supplier_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x060000C6 RID: 198 RVA: 0x00009554 File Offset: 0x00007754
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FNumber");
			e.FieldKeys.Add("FName");
			e.FieldKeys.Add("FShortName");
			e.FieldKeys.Add("FDescription");
			e.FieldKeys.Add("F_YTSTPM");
			e.FieldKeys.Add("FUseOrgId");
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x000095D0 File Offset: 0x000077D0
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string text = Helper.ToStr(dynamicObject["Number"], 0);
					string text2 = Helper.ToStr(dynamicObject["ShortName"], 0);
					string value = Helper.ToStr(dynamicObject["Name"], 0);
					string value2 = Helper.ToStr(dynamicObject["Description"], 0);
					string a = Helper.ToBool(dynamicObject["F_YTSTPM"]);
					DynamicObject dynamicObject2 = dynamicObject["UseOrgId"] as DynamicObject;
					bool flag2 = !dynamicObject2.IsNullOrEmptyOrWhiteSpace();
					if (flag2)
					{
						string a2 = Helper.ToStr(dynamicObject2["Number"], 0);
						bool flag3 = a2 == "999";
						if (flag3)
						{
							bool flag4 = a == "true";
							if (flag4)
							{
								throw new Exception("供应商（" + text + "）已推送TPM，不能重复推送！");
							}
							JObject jobject = new JObject();
							jobject["vendor_code"] = text;
							jobject["vendor_name"] = value;
							jobject["vendor_full_name"] = value;
							jobject["contacter"] = "A";
							jobject["tel_no"] = "A";
							jobject["address"] = "A";
							jobject["remarks"] = value2;
							string text3 = JsonConvert.SerializeObject(jobject);
							Helper.WriteLog("BD_Supplier-TPM", text3);
							string value3 = Utils.HttpPostApi(this.url, text3);
							ResponseInfo responseInfo = JsonConvert.DeserializeObject<ResponseInfo>(value3);
							bool flag5 = responseInfo.status.ToLower() != "true";
							if (flag5)
							{
								throw new Exception("推送TPM供应商失败：" + responseInfo.message);
							}
							string strSQL = string.Format("update t_BD_Supplier set F_YTSTPM = '1' where FNUMBER= '{0}'", text);
							DBServiceHelper.Execute(base.Context, strSQL);
						}
					}
				}
			}
		}

		// Token: 0x04000041 RID: 65
		public string url = "http://10.11.111.63:8800/api/MasterData/vendor/ErpAfferentVendor";
	}
}
