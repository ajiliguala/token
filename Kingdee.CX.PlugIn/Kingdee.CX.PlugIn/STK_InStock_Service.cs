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
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000012 RID: 18
	[Description("采购入库单推送TPM")]
	[HotUpdate]
	public class STK_InStock_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x0600008F RID: 143 RVA: 0x00006D70 File Offset: 0x00004F70
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("F_YTSTPM");
			e.FieldKeys.Add("FInStockEntry");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("FRealQty");
			e.FieldKeys.Add("FStockOrgId");
			e.FieldKeys.Add("FBillNo");
			e.FieldKeys.Add("FContractlNo");
			e.FieldKeys.Add("FSupplierId");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("F_PCQE_TSQKSM");
			e.FieldKeys.Add("F_SYPGFZR");
			e.FieldKeys.Add("F_PCQE_JSPGFZR");
			e.FieldKeys.Add("F_PCQE_ZBYSZQ");
			e.FieldKeys.Add("F_QQQQ_SFWFSSB");
			e.FieldKeys.Add("F_PCQE_YSLX");
			e.FieldKeys.Add("F_PCQE_GNYSZQ");
			e.FieldKeys.Add("FBillTypeID");
			e.FieldKeys.Add("FPOORDERENTRYID");
			e.FieldKeys.Add("F_QQQQ_TPMSYZZ");
			e.FieldKeys.Add("FBillTypeID");
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00006EDC File Offset: 0x000050DC
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string text = Helper.ToStr(dynamicObject["BillNo"], 0);
					DateTime dateTime = Helper.ToDateTime(dynamicObject["Date"]);
					DynamicObject dynamicObject2 = dynamicObject["StockOrgId"] as DynamicObject;
					bool flag2 = !dynamicObject2.IsNullOrEmptyOrWhiteSpace();
					if (flag2)
					{
						string item = Helper.ToStr(dynamicObject2["Number"], 0);
						string a = Helper.ToBool(dynamicObject["F_YTSTPM"]);
						bool flag3 = a == "true";
						if (flag3)
						{
							throw new Exception("采购入库单（" + text + "）已推送TPM，不能重复推送！");
						}
						DynamicObject dynamicObject3 = dynamicObject["FBillTypeID"] as DynamicObject;
						List<string> list = new List<string>
						{
							"020",
							"021",
							"022",
							"023",
							"025",
							"026",
							"027",
							"070",
							"071",
							"072",
							"073",
							"080"
						};
						bool flag4 = list.Contains(item);
						if (flag4)
						{
							bool flag5 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace();
							if (flag5)
							{
								bool flag6 = Helper.ToStr(dynamicObject3["Number"], 0) == "RKD05_SYS";
								if (flag6)
								{
									List<Dictionary<string, object>> list2 = new List<Dictionary<string, object>>();
									DynamicObject dynamicObject4 = dynamicObject["SupplierId"] as DynamicObject;
									DynamicObjectCollection dynamicObjectCollection = dynamicObject["InStockEntry"] as DynamicObjectCollection;
									foreach (DynamicObject dynamicObject5 in dynamicObjectCollection)
									{
										Dictionary<string, object> dictionary = new Dictionary<string, object>();
										DynamicObject dynamicObject6 = dynamicObject5["MaterialId"] as DynamicObject;
										bool flag7 = !dynamicObject6.IsNullOrEmptyOrWhiteSpace();
										if (flag7)
										{
											dictionary.Add("machine_name", Helper.ToStr(dynamicObject6["Name"], 0));
										}
										dictionary.Add("machine_num", Convert.ToInt32(dynamicObject5["RealQty"]));
										DynamicObject dynamicObject7 = dynamicObject5["F_QQQQ_TPMSYZZ"] as DynamicObject;
										bool flag8 = !dynamicObject7.IsNullOrEmptyOrWhiteSpace();
										if (flag8)
										{
											dictionary.Add("plant_code", Helper.ToStr(dynamicObject7["FNumber"], 0));
											dictionary.Add("plant_name", Helper.ToStr(dynamicObject7["FDataValue"], 0));
										}
										dictionary.Add("estimate_by", Helper.ToStr(dynamicObject5["F_PCQE_JSPGFZR"], 0));
										dictionary.Add("user_by", Helper.ToStr(dynamicObject5["F_SYPGFZR"], 0));
										string a2 = Helper.ToStr(dynamicObject5["F_QQQQ_SFWFSSB"], 0);
										bool flag9 = a2 == "1";
										if (flag9)
										{
											dictionary.Add("is_child", "Y");
										}
										else
										{
											dictionary.Add("is_child", "N");
										}
										string value = "Y";
										dictionary.Add("is_fixedassets", value);
										dictionary.Add("instock_code", text);
										bool flag10 = !dynamicObject4.IsNullOrEmptyOrWhiteSpace();
										if (flag10)
										{
											dictionary.Add("supplier_code", Helper.ToStr(dynamicObject4["Number"], 0));
											dictionary.Add("supplier_name", Helper.ToStr(dynamicObject4["Name"], 0));
										}
										DynamicObject dynamicObject8 = dynamicObject5["F_PCQE_GNYSZQ"] as DynamicObject;
										bool flag11 = !dynamicObject8.IsNullOrEmptyOrWhiteSpace();
										if (flag11)
										{
											dictionary.Add("check_function_days", Helper.ToStr(dynamicObject8["FDataValue"], 0));
										}
										DynamicObject dynamicObject9 = dynamicObject5["F_PCQE_ZBYSZQ"] as DynamicObject;
										bool flag12 = !dynamicObject9.IsNullOrEmptyOrWhiteSpace();
										if (flag12)
										{
											dictionary.Add("signed_days", Helper.ToStr(dynamicObject9["FDataValue"], 0));
										}
										DynamicObject dynamicObject10 = dynamicObject5["F_PCQE_YSLX"] as DynamicObject;
										bool flag13 = !dynamicObject10.IsNullOrEmptyOrWhiteSpace();
										if (flag13)
										{
											dictionary.Add("acceptance_type", Helper.ToStr(dynamicObject10["FDataValue"], 0));
										}
										string value2 = dateTime.ToString("yyyy-MM-dd");
										dictionary.Add("jinchang_date", value2);
										dictionary.Add("remarks", Helper.ToStr(dynamicObject5["F_PCQE_TSQKSM"], 0));
										string strSQL = string.Format("select a.fid,a.FBILLNO,d.FBILLNO CGDDH,d.FDATE CGRQ,FDELIVERYDATE from t_STK_InStock a \r\n                                    join T_STK_INSTOCKENTRY b on a.FID=b.FID\r\n                                    join t_PUR_POOrderEntry c on c.FENTRYID=FPOORDERENTRYID\r\n                                    join t_PUR_POOrder d on c.FID=d.FID\r\n                                    join T_PUR_POORDERENTRY_D e on e.FENTRYID=c.FENTRYID\r\n                                    where FPOORDERENTRYID={0}", Helper.ToLong(dynamicObject5["POORDERENTRYID"]));
										DynamicObject dynamicObject11 = DBUtils.ExecuteDynamicObject(base.Context, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
										bool flag14 = !dynamicObject11.IsNullOrEmptyOrWhiteSpace();
										if (flag14)
										{
											dictionary.Add("purchase_code", Helper.ToStr(dynamicObject11["CGDDH"], 0));
											string value3 = Helper.ToDateTime(dynamicObject11["CGRQ"]).ToString("yyyy-MM-dd");
											dictionary.Add("goumai_date", value3);
											string value4 = Helper.ToDateTime(dynamicObject11["FDELIVERYDATE"]).ToString("yyyy-MM-dd");
											dictionary.Add("expected_delivery_date", value4);
										}
										list2.Add(dictionary);
									}
									string text2 = JsonConvert.SerializeObject(list2.ToArray());
									Helper.WriteLog("STK_InStock-TPM", text2);
									string value5 = Utils.HttpPostApi(this.url, text2);
									ResponseInfo responseInfo = JsonConvert.DeserializeObject<ResponseInfo>(value5);
									bool flag15 = responseInfo.status.ToLower() != "true";
									if (flag15)
									{
										throw new Exception("推送TPM采购入库单失败：" + responseInfo.message);
									}
									string strSQL2 = string.Format("update t_STK_InStock set F_YTSTPM = '1' where FBILLNO= '{0}'", text);
									DBServiceHelper.Execute(base.Context, strSQL2);
								}
							}
						}
					}
				}
			}
			bool flag16 = base.FormOperation.OperationId == FormOperation.Operation_UnAudit;
			if (flag16)
			{
				foreach (DynamicObject dynamicObject12 in e.DataEntitys)
				{
					string text3 = Helper.ToStr(dynamicObject12["BillNo"], 0);
					string a3 = Helper.ToBool(dynamicObject12["F_YTSTPM"]);
					bool flag17 = a3 == "true";
					if (flag17)
					{
						string requestJson = "http://10.11.111.63:8800/api/Tpm/Machine/DeApproval?instock_code=" + text3;
						Helper.WriteLog("STK_InStock-TPM-Delete", requestJson);
						string value6 = Utils.HttpGet(requestJson);
						ResponseInfo responseInfo2 = JsonConvert.DeserializeObject<ResponseInfo>(value6);
						bool flag18 = responseInfo2.status.ToLower() != "true";
						if (flag18)
						{
							throw new Exception("反审删除TPM订单失败：" + responseInfo2.message);
						}
						string strSQL3 = string.Format("update t_STK_InStock set F_YTSTPM = '0' where FBILLNO= '{0}'", text3);
						DBServiceHelper.Execute(base.Context, strSQL3);
					}
				}
			}
		}

		// Token: 0x04000034 RID: 52
		public string url = "http://10.11.111.63:8800/api/Tpm/Machine/ErpAfferentTpm";
	}
}
