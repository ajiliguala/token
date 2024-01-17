using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000019 RID: 25
	[Description("生产退料单提交生成不良品拆解单插件")]
	public class PrdReturnMtrlSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x0600004F RID: 79 RVA: 0x00007E1C File Offset: 0x0000601C
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FBillNo");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("FDocumentStatus");
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("FMoBillNo");
			e.FieldKeys.Add("FPrdOrgId");
			e.FieldKeys.Add("FMoEntryId");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("FReturnReason");
			e.FieldKeys.Add("FStockId");
			e.FieldKeys.Add("FWorkShopId1");
			e.FieldKeys.Add("FStockLocId");
			e.FieldKeys.Add("FUnitID");
			e.FieldKeys.Add("FQty");
			e.FieldKeys.Add("FApproveDate");
			e.FieldKeys.Add("FBomId");
			e.FieldKeys.Add("F_PAEZ_BILLID");
			e.FieldKeys.Add("F_PAEZ_BZSL");
			e.FieldKeys.Add("F_KING_BLLX");
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00007F78 File Offset: 0x00006178
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
				string text3 = (dynamicObject["PrdOrgId"] as DynamicObject)["number"].ToString();
				bool flag2 = !text3.Equals("071");
				if (!flag2)
				{
					bool flag3 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_KING_BLLX"]) || !dynamicObject["F_KING_BLLX"].ToString().Equals("01");
					if (!flag3)
					{
						DynamicObjectCollection dynamicObjectCollection = dynamicObject["Entity"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
						{
							string text4 = dynamicObject2["id"].ToString();
							DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
							string text5 = dynamicObject3["name"].ToString();
							string text6 = dynamicObject3["number"].ToString();
							string text7 = (dynamicObject2["ReturnReason"] as DynamicObject)["FNumber"].ToString();
							bool flag4 = text6.Contains("CFOG");
							if (!flag4)
							{
								bool flag5 = text6.Contains("组装层");
								if (!flag5)
								{
									bool flag6 = text6.Contains("FOG");
									if (!flag6)
									{
										bool flag7 = text7.Equals("TLYY05_SYS") || text7.Equals("TLYY03_SYS") || text7.Equals("TLYY04_SYS") || text7.Equals("TLYY02_SYS");
										if (flag7)
										{
											string text8 = string.Empty;
											decimal num2 = Convert.ToDecimal(dynamicObject2["Qty"]);
											decimal num3 = Convert.ToDecimal(dynamicObject2["F_PAEZ_BZSL"]);
											string text9 = dynamicObject2["MoBillNo"].ToString();
											string entryId = dynamicObject2["MoEntryId"].ToString();
											DateTime moBillDate = serviceHelper.getMoBillDate(base.Context, text9);
											JObject jobject2 = new JObject();
											jobject2["Fnumber"] = text3;
											JObject jobject3 = new JObject();
											jobject3["FMATERIALID"] = serviceHelper.getMoBillmaterial(base.Context, entryId);
											JObject jobject4 = new JObject();
											string text10 = dynamicObject2["MaterialId_id"].ToString();
											jobject4["FMATERIALID"] = text10;
											DynamicObject dynamicObject4 = dynamicObject2["StockId"] as DynamicObject;
											string text11 = dynamicObject4["name"].ToString();
											JObject jobject5 = new JObject();
											jobject5["FSTOCKID"] = dynamicObject4["id"].ToString();
											string text12 = dynamicObject2["StockLocId_id"].ToString();
											string text13 = "";
											bool flag8 = !text12.Equals("0");
											if (flag8)
											{
												text13 = serviceHelper.getLocName(base.Context, text12);
											}
											JObject jobject6 = new JObject();
											jobject6["FUnitID"] = dynamicObject2["UnitID_id"].ToString();
											string text14 = string.Empty;
											bool flag9 = text11.Contains("来料坏");
											if (flag9)
											{
												text14 = "材损";
											}
											else
											{
												bool flag10 = text11.Contains("生产坏");
												if (flag10)
												{
													text14 = "制损";
												}
												else
												{
													text14 = "其他";
												}
											}
											JObject jobject7 = new JObject();
											jobject7["fdeptid"] = dynamicObject2["WorkShopId1_id"].ToString();
											JObject jobject8 = new JObject();
											string text15 = dynamicObject2["BomId_id"].ToString();
											jobject8["FID"] = text15;
											bool flag11 = num3 > 0m;
											if (flag11)
											{
												decimal num4 = Math.Round(num2 / num3 + 0.49999999999m);
												int num5 = 1;
												while (num5 <= num4)
												{
													JObject jobject9 = new JObject();
													jobject9["FMOBILLNO"] = text9;
													jobject9["FMODATE"] = moBillDate;
													jobject9["FSOURCEENTRYID"] = text4;
													jobject9["FTYPE"] = "2";
													jobject9["F_PAEZ_BILLNO"] = text2;
													jobject9["F_PAEZ_DATE"] = dateTime;
													jobject9["F_PAEZ_XS"] = num4;
													jobject9["F_PAEZ_OrgId"] = jobject2;
													jobject9["F_PAEZ_MATERIAL"] = jobject3;
													jobject9["FMATERIALID"] = jobject4;
													jobject9["FSTOCKID"] = jobject5;
													jobject9["F_PAEZ_ZB"] = text14;
													jobject9["FDEPTID"] = jobject7;
													jobject9["FSTOCKLOCNAME"] = text13;
													jobject9["F_PAEZ_UnitID"] = jobject6;
													decimal num6 = 0m;
													bool flag12 = num5 == num4;
													if (flag12)
													{
														num6 = num2 - num3 * (num4 - 1m);
													}
													else
													{
														num6 = num3;
													}
													jobject9["F_PAEZ_Qty"] = num6;
													jobject9["F_PAEZ_STATUS"] = "B";
													jobject9["FBOMID"] = jobject8;
													JArray jarray = new JArray();
													serviceHelper.getSubByBom(base.Context, ref jarray, text10, text15, num6, "1");
													jobject9["FEntity"] = jarray;
													JObject jobject10 = new JObject();
													jobject10["Model"] = jobject9;
													string text16 = JsonConvert.SerializeObject(jobject10);
													string text17 = "PAEZ_BLPCJ";
													string text18 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
													{
														text17,
														text16
													});
													JObject jobject11 = JObject.Parse(text18);
													object obj = jobject11["Result"];
													JObject jobject12 = JObject.Parse(JsonConvert.SerializeObject(obj));
													object obj2 = jobject12["ResponseStatus"];
													JObject jobject13 = JObject.Parse(JsonConvert.SerializeObject(obj2));
													JArray jarray2 = jobject13["SuccessEntitys"] as JArray;
													text8 = text8 + Convert.ToString((jarray2[0] as JObject)["Id"]) + ",";
													num5++;
												}
											}
											else
											{
												JObject jobject14 = new JObject();
												jobject14["FMOBILLNO"] = text9;
												jobject14["FMODATE"] = moBillDate;
												jobject14["FTYPE"] = "2";
												jobject14["FSOURCEENTRYID"] = text4;
												jobject14["F_PAEZ_BILLNO"] = text2;
												jobject14["F_PAEZ_DATE"] = dateTime;
												jobject14["F_PAEZ_OrgId"] = jobject2;
												jobject14["F_PAEZ_MATERIAL"] = jobject3;
												jobject14["FMATERIALID"] = jobject4;
												jobject14["FSTOCKID"] = jobject5;
												jobject14["F_PAEZ_ZB"] = text14;
												jobject14["FDEPTID"] = jobject7;
												jobject14["FSTOCKLOCNAME"] = text13;
												jobject14["F_PAEZ_UnitID"] = jobject6;
												jobject14["F_PAEZ_Qty"] = num2;
												jobject14["F_PAEZ_STATUS"] = "B";
												jobject14["FBOMID"] = jobject8;
												JArray jarray3 = new JArray();
												serviceHelper.getSubByBom(base.Context, ref jarray3, text10, text15, num2, "1");
												jobject14["FEntity"] = jarray3;
												JObject jobject15 = new JObject();
												jobject15["Model"] = jobject14;
												string text19 = JsonConvert.SerializeObject(jobject15);
												string text20 = "PAEZ_BLPCJ";
												string text21 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
												{
													text20,
													text19
												});
												JObject jobject16 = JObject.Parse(text21);
												object obj3 = jobject16["Result"];
												JObject jobject17 = JObject.Parse(JsonConvert.SerializeObject(obj3));
												object obj4 = jobject17["ResponseStatus"];
												JObject jobject18 = JObject.Parse(JsonConvert.SerializeObject(obj4));
												JArray jarray4 = jobject18["SuccessEntitys"] as JArray;
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
		}
	}
}
