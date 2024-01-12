using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Newtonsoft.Json;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x0200000C RID: 12
	[Description("采购退料单更新SRM送货单据状态")]
	[HotUpdate]
	public class PUR_MRB_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00004418 File Offset: 0x00002618
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FPURMRBENTRY");
			e.FieldKeys.Add("FSRMNO");
			e.FieldKeys.Add("FORDERNO");
			e.FieldKeys.Add("FPOORDERENTRYID");
			e.FieldKeys.Add("FRMREALQTY");
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00004484 File Offset: 0x00002684
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string text = Helper.ToStr(dynamicObject["BillNo"], 0);
					string value = Helper.ToStr(dynamicObject["FSRMNO"], 0);
					bool flag2 = !string.IsNullOrEmpty(value);
					if (flag2)
					{
						DynamicObjectCollection dynamicObjectCollection = dynamicObject["PUR_MRBENTRY"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
						{
							double num = Helper.ToDouble(dynamicObject2["RMREALQTY"]);
							string arg = Helper.ToStr(dynamicObject2["ORDERNO"], 0);
							int num2 = Helper.ToInt(dynamicObject2["POORDERENTRYID"]);
							string strSQL = string.Format("select FID from t_PUR_POOrder where FBILLNO='{0}'", arg);
							DynamicObject dynamicObject3 = DBUtils.ExecuteDynamicObject(base.Context, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
							bool flag3 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace() && num > 0.0 && num2 > 0;
							if (flag3)
							{
								string value2 = Helper.ToStr(dynamicObject3["FID"], 0);
								Dictionary<string, string> dictionary = new Dictionary<string, string>();
								dictionary.Add("packslip", value);
								dictionary.Add("fid", value2);
								dictionary.Add("fentryid", num2.ToString());
								dictionary.Add("qty", num.ToString());
								string requestJson = JsonConvert.SerializeObject(dictionary);
								Helper.WriteLog("erpUpdRcvStatusByMR", requestJson);
								string value3 = Utils.HttpPost(this.url, dictionary);
								SrmResponseInfo srmResponseInfo = JsonConvert.DeserializeObject<SrmResponseInfo>(value3);
								bool flag4 = srmResponseInfo.msg.ToLower() != "success";
								if (flag4)
								{
									throw new Exception("采购退料单审核更新SRM相关送货单据状态失败：" + srmResponseInfo.msg);
								}
							}
						}
					}
				}
			}
			bool flag5 = base.FormOperation.OperationId == FormOperation.Operation_UnAudit;
			if (flag5)
			{
				foreach (DynamicObject dynamicObject4 in e.DataEntitys)
				{
					string text2 = Helper.ToStr(dynamicObject4["BillNo"], 0);
					string value4 = Helper.ToStr(dynamicObject4["FSRMNO"], 0);
					bool flag6 = !string.IsNullOrEmpty(value4);
					if (flag6)
					{
						DynamicObjectCollection dynamicObjectCollection2 = dynamicObject4["PUR_MRBENTRY"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject5 in dynamicObjectCollection2)
						{
							double num3 = 0.0 - Helper.ToDouble(dynamicObject5["RMREALQTY"]);
							string arg2 = Helper.ToStr(dynamicObject5["ORDERNO"], 0);
							int num4 = Helper.ToInt(dynamicObject5["POORDERENTRYID"]);
							string strSQL2 = string.Format("select FID from t_PUR_POOrder where FBILLNO='{0}'", arg2);
							DynamicObject dynamicObject6 = DBUtils.ExecuteDynamicObject(base.Context, strSQL2, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
							bool flag7 = !dynamicObject6.IsNullOrEmptyOrWhiteSpace() && num3 > 0.0 && num4 > 0;
							if (flag7)
							{
								string value5 = Helper.ToStr(dynamicObject6["FID"], 0);
								Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
								dictionary2.Add("packslip", value4);
								dictionary2.Add("fid", value5);
								dictionary2.Add("fentryid", num4.ToString());
								dictionary2.Add("qty", num3.ToString());
								string requestJson2 = JsonConvert.SerializeObject(dictionary2);
								Helper.WriteLog("erpUpdRcvStatusByMR", requestJson2);
								string value6 = Utils.HttpPost(this.url, dictionary2);
								SrmResponseInfo srmResponseInfo2 = JsonConvert.DeserializeObject<SrmResponseInfo>(value6);
								bool flag8 = srmResponseInfo2.msg.ToLower() != "success";
								if (flag8)
								{
									throw new Exception("采购退料单反审核更新SRM相关送货单据状态失败：" + srmResponseInfo2.msg);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x04000028 RID: 40
		public string url = "http://10.10.111.70:8088/cxbiz/rcv/erpUpdRcvStatusByMR";
	}
}
