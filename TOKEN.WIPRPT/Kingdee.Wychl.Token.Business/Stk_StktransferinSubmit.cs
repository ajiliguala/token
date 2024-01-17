using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000028 RID: 40
	public class Stk_StktransferinSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x0600008D RID: 141 RVA: 0x0000F7B4 File Offset: 0x0000D9B4
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FBillNo");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("FDocumentStatus");
			e.FieldKeys.Add("FStockOrgId");
			e.FieldKeys.Add("F_KING_DBYY");
			e.FieldKeys.Add("F_PAEZ_BILLID");
			e.FieldKeys.Add("F_PAEZ_BZSL");
			e.FieldKeys.Add("FBillEntry");
			e.FieldKeys.Add("FQty");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("FDestStockId");
			e.FieldKeys.Add("FSaleDeptId");
			e.FieldKeys.Add("FDestStockLocId");
			e.FieldKeys.Add("FUnitID");
			e.FieldKeys.Add("FDestBomId");
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000F8CC File Offset: 0x0000DACC
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			K3CloudApiClient k3CloudApiClient = new K3CloudApiClient("http://localhost/k3cloud/");
			string dbid = base.Context.DBId;
			apiParameter parameterByDbid = serviceHelper.getParameterByDbid(dbid);
			string text = k3CloudApiClient.LoginByAppSecret(dbid, parameterByDbid.apiuser, parameterByDbid.appid, parameterByDbid.appSecret, 2052);
			JObject jobject = JObject.Parse(text);
			int num = Extensions.Value<int>(jobject["LoginResultType"]);
			bool flag = num != 1 && num != -5;
			if (flag)
			{
				throw new Exception("api登录失败");
			}
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				string text2 = dynamicObject["BillNo"].ToString();
				DateTime dateTime = Convert.ToDateTime(dynamicObject["Date"]);
				string text3 = (dynamicObject["StockOrgId"] as DynamicObject)["number"].ToString();
				bool flag2 = !text3.Equals("071");
				if (!flag2)
				{
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["TransferDirectEntry"] as DynamicObjectCollection;
					bool flag3 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_KING_DBYY"]);
					if (!flag3)
					{
						string text4 = (dynamicObject["F_KING_DBYY"] as DynamicObject)["fnumber"].ToString();
						bool flag4 = text4.Equals("00004");
						if (flag4)
						{
							foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
							{
								string text5 = dynamicObject2["id"].ToString();
								DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
								string text6 = dynamicObject3["name"].ToString();
								string text7 = dynamicObject3["number"].ToString();
								string text8 = string.Empty;
								decimal num2 = Convert.ToDecimal(dynamicObject2["Qty"]);
								decimal num3 = Convert.ToDecimal(dynamicObject2["F_PAEZ_BZSL"]);
								JObject jobject2 = new JObject();
								jobject2["Fnumber"] = text3;
								JObject jobject3 = new JObject();
								string text9 = dynamicObject2["MaterialId_id"].ToString();
								jobject3["FMATERIALID"] = text9;
								DynamicObject dynamicObject4 = dynamicObject2["DestStockId"] as DynamicObject;
								string text10 = dynamicObject4["name"].ToString();
								JObject jobject4 = new JObject();
								jobject4["FSTOCKID"] = dynamicObject4["id"].ToString();
								string text11 = dynamicObject2["DestStockLocId_id"].ToString();
								string text12 = "";
								bool flag5 = !text11.Equals("0");
								if (flag5)
								{
									text12 = serviceHelper.getLocName(base.Context, text11);
								}
								JObject jobject5 = new JObject();
								jobject5["FUnitID"] = dynamicObject2["UnitID_id"].ToString();
								string text13 = string.Empty;
								bool flag6 = text10.Contains("来料坏");
								if (flag6)
								{
									text13 = "材损";
								}
								else
								{
									bool flag7 = text10.Contains("生产坏");
									if (flag7)
									{
										text13 = "制损";
									}
									else
									{
										text13 = "其他";
									}
								}
								JObject jobject6 = new JObject();
								jobject6["fdeptid"] = dynamicObject["SaleDeptId_id"].ToString();
								JObject jobject7 = new JObject();
								string text14 = dynamicObject2["DestBomId_id"].ToString();
								jobject7["FID"] = text14;
								bool flag8 = num3 > 0m;
								if (flag8)
								{
									decimal num4 = Math.Round(num2 / num3 + 0.49999999999m);
									int num5 = 1;
									while (num5 <= num4)
									{
										JObject jobject8 = new JObject();
										jobject8["FTYPE"] = "5";
										jobject8["F_PAEZ_BILLNO"] = text2;
										jobject8["F_PAEZ_DATE"] = dateTime;
										jobject8["FSOURCEENTRYID"] = text5;
										jobject8["F_PAEZ_OrgId"] = jobject2;
										jobject8["FMATERIALID"] = jobject3;
										jobject8["FSTOCKID"] = jobject4;
										jobject8["F_PAEZ_ZB"] = text13;
										jobject8["FDEPTID"] = jobject6;
										jobject8["FSTOCKLOCNAME"] = text12;
										jobject8["F_PAEZ_UnitID"] = jobject5;
										decimal num6 = 0m;
										bool flag9 = num5 == num4;
										if (flag9)
										{
											num6 = num2 - num3 * (num4 - 1m);
										}
										else
										{
											num6 = num3;
										}
										jobject8["F_PAEZ_Qty"] = num6;
										jobject8["F_PAEZ_STATUS"] = "B";
										jobject8["FBOMID"] = jobject7;
										JArray jarray = new JArray();
										serviceHelper.getSubByBom(base.Context, ref jarray, text9, text14, num6, "1");
										jobject8["FEntity"] = jarray;
										JObject jobject9 = new JObject();
										jobject9["Model"] = jobject8;
										string text15 = JsonConvert.SerializeObject(jobject9);
										string text16 = "PAEZ_BLPCJ";
										string text17 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
										{
											text16,
											text15
										});
										JObject jobject10 = JObject.Parse(text17);
										object obj = jobject10["Result"];
										JObject jobject11 = JObject.Parse(JsonConvert.SerializeObject(obj));
										object obj2 = jobject11["ResponseStatus"];
										JObject jobject12 = JObject.Parse(JsonConvert.SerializeObject(obj2));
										JArray jarray2 = jobject12["SuccessEntitys"] as JArray;
										text8 = text8 + Convert.ToString((jarray2[0] as JObject)["Id"]) + ",";
										num5++;
									}
								}
								else
								{
									JObject jobject13 = new JObject();
									jobject13["FTYPE"] = "5";
									jobject13["F_PAEZ_BILLNO"] = text2;
									jobject13["F_PAEZ_DATE"] = dateTime;
									jobject13["FSOURCEENTRYID"] = text5;
									jobject13["F_PAEZ_OrgId"] = jobject2;
									jobject13["FMATERIALID"] = jobject3;
									jobject13["FSTOCKID"] = jobject4;
									jobject13["F_PAEZ_ZB"] = text13;
									jobject13["FDEPTID"] = jobject6;
									jobject13["FSTOCKLOCNAME"] = text12;
									jobject13["F_PAEZ_UnitID"] = jobject5;
									jobject13["F_PAEZ_Qty"] = num2;
									jobject13["F_PAEZ_STATUS"] = "B";
									jobject13["FBOMID"] = jobject7;
									JArray jarray3 = new JArray();
									serviceHelper.getSubByBom(base.Context, ref jarray3, text9, text14, num2, "1");
									jobject13["FEntity"] = jarray3;
									JObject jobject14 = new JObject();
									jobject14["Model"] = jobject13;
									string text18 = JsonConvert.SerializeObject(jobject14);
									string text19 = "PAEZ_BLPCJ";
									string text20 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
									{
										text19,
										text18
									});
									JObject jobject15 = JObject.Parse(text20);
									object obj3 = jobject15["Result"];
									JObject jobject16 = JObject.Parse(JsonConvert.SerializeObject(obj3));
									object obj4 = jobject16["ResponseStatus"];
									JObject jobject17 = JObject.Parse(JsonConvert.SerializeObject(obj4));
									JArray jarray4 = jobject17["SuccessEntitys"] as JArray;
									text8 = text8 + Convert.ToString((jarray4[0] as JObject)["Id"]) + ",";
								}
								dynamicObject2["F_PAEZ_BILLID"] = text8.TrimEnd(new char[]
								{
									','
								});
							}
						}
					}
				}
			}
		}
	}
}
