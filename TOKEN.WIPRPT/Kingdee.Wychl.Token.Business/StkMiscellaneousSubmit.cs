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
	// Token: 0x02000024 RID: 36
	[Description("其他入库单提交生成不良品拆解单插件")]
	public class StkMiscellaneousSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000081 RID: 129 RVA: 0x0000DFAC File Offset: 0x0000C1AC
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FBillNo");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("FDocumentStatus");
			e.FieldKeys.Add("FStockOrgId");
			e.FieldKeys.Add("F_ora_Combo");
			e.FieldKeys.Add("F_PAEZ_BILLID");
			e.FieldKeys.Add("F_PAEZ_BZSL");
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("FQty");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("F_PRDMATERIAL");
			e.FieldKeys.Add("FStockId");
			e.FieldKeys.Add("FDEPTID");
			e.FieldKeys.Add("FStockLocId");
			e.FieldKeys.Add("FUnitID");
			e.FieldKeys.Add("FBOMID");
			e.FieldKeys.Add("F_KING_BLLX");
		}

		// Token: 0x06000082 RID: 130 RVA: 0x0000E0E4 File Offset: 0x0000C2E4
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
				DateTime dateTime = Convert.ToDateTime(dynamicObject["FDate"]);
				string text3 = (dynamicObject["FStockOrgId"] as DynamicObject)["number"].ToString();
				bool flag2 = !text3.Equals("071");
				if (!flag2)
				{
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["STK_MISCELLANEOUSENTRY"] as DynamicObjectCollection;
					bool flag3 = !dynamicObject["F_ora_Combo"].ToString().Equals("4");
					if (!flag3)
					{
						bool flag4 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_KING_BLLX"]) || !dynamicObject["F_KING_BLLX"].ToString().Equals("01");
						if (!flag4)
						{
							foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
							{
								string text4 = dynamicObject2["id"].ToString();
								DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
								string text5 = dynamicObject3["name"].ToString();
								string text6 = dynamicObject3["number"].ToString();
								string text7 = string.Empty;
								decimal num2 = Convert.ToDecimal(dynamicObject2["FQty"]);
								decimal num3 = Convert.ToDecimal(dynamicObject2["F_PAEZ_BZSL"]);
								JObject jobject2 = new JObject();
								jobject2["Fnumber"] = text3;
								JObject jobject3 = new JObject();
								jobject3["FMATERIALID"] = dynamicObject2["F_PRDMATERIAL_id"].ToString();
								JObject jobject4 = new JObject();
								string text8 = dynamicObject2["MaterialId_id"].ToString();
								jobject4["FMATERIALID"] = text8;
								DynamicObject dynamicObject4 = dynamicObject2["FSTOCKID"] as DynamicObject;
								string text9 = dynamicObject4["name"].ToString();
								JObject jobject5 = new JObject();
								jobject5["FSTOCKID"] = dynamicObject4["id"].ToString();
								string text10 = dynamicObject2["StockPlaceId_id"].ToString();
								string text11 = "";
								bool flag5 = !text10.Equals("0");
								if (flag5)
								{
									text11 = serviceHelper.getLocName(base.Context, text10);
								}
								JObject jobject6 = new JObject();
								jobject6["FUnitID"] = dynamicObject2["FUnitID_id"].ToString();
								string text12 = string.Empty;
								bool flag6 = text9.Contains("来料坏");
								if (flag6)
								{
									text12 = "材损";
								}
								else
								{
									bool flag7 = text9.Contains("生产坏");
									if (flag7)
									{
										text12 = "制损";
									}
									else
									{
										text12 = "其他";
									}
								}
								JObject jobject7 = new JObject();
								jobject7["fdeptid"] = dynamicObject["DEPTID_id"].ToString();
								JObject jobject8 = new JObject();
								string text13 = dynamicObject2["BOMID_id"].ToString();
								jobject8["FID"] = text13;
								bool flag8 = num3 > 0m;
								if (flag8)
								{
									decimal num4 = Math.Round(num2 / num3 + 0.49999999999m);
									int num5 = 1;
									while (num5 <= num4)
									{
										JObject jobject9 = new JObject();
										jobject9["FTYPE"] = "3";
										jobject9["F_PAEZ_BILLNO"] = text2;
										jobject9["FSOURCEENTRYID"] = text4;
										jobject9["F_PAEZ_DATE"] = dateTime;
										jobject9["F_PAEZ_OrgId"] = jobject2;
										jobject9["F_PAEZ_MATERIAL"] = jobject3;
										jobject9["FMATERIALID"] = jobject4;
										jobject9["FSTOCKID"] = jobject5;
										jobject9["F_PAEZ_ZB"] = text12;
										jobject9["FDEPTID"] = jobject7;
										jobject9["FSTOCKLOCNAME"] = text11;
										jobject9["F_PAEZ_UnitID"] = jobject6;
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
										jobject9["F_PAEZ_Qty"] = num6;
										jobject9["F_PAEZ_STATUS"] = "B";
										jobject9["FBOMID"] = jobject8;
										JArray jarray = new JArray();
										serviceHelper.getSubByBom(base.Context, ref jarray, text8, text13, num6, "1");
										jobject9["FEntity"] = jarray;
										JObject jobject10 = new JObject();
										jobject10["Model"] = jobject9;
										string text14 = JsonConvert.SerializeObject(jobject10);
										string text15 = "PAEZ_BLPCJ";
										string text16 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
										{
											text15,
											text14
										});
										JObject jobject11 = JObject.Parse(text16);
										object obj = jobject11["Result"];
										JObject jobject12 = JObject.Parse(JsonConvert.SerializeObject(obj));
										object obj2 = jobject12["ResponseStatus"];
										JObject jobject13 = JObject.Parse(JsonConvert.SerializeObject(obj2));
										JArray jarray2 = jobject13["SuccessEntitys"] as JArray;
										text7 = text7 + Convert.ToString((jarray2[0] as JObject)["Id"]) + ",";
										num5++;
									}
								}
								else
								{
									JObject jobject14 = new JObject();
									jobject14["FTYPE"] = "3";
									jobject14["F_PAEZ_BILLNO"] = text2;
									jobject14["FSOURCEENTRYID"] = text4;
									jobject14["F_PAEZ_DATE"] = dateTime;
									jobject14["F_PAEZ_OrgId"] = jobject2;
									jobject14["F_PAEZ_MATERIAL"] = jobject3;
									jobject14["FMATERIALID"] = jobject4;
									jobject14["FSTOCKID"] = jobject5;
									jobject14["F_PAEZ_ZB"] = text12;
									jobject14["FDEPTID"] = jobject7;
									jobject14["FSTOCKLOCNAME"] = text11;
									jobject14["F_PAEZ_UnitID"] = jobject6;
									jobject14["F_PAEZ_Qty"] = num2;
									jobject14["F_PAEZ_STATUS"] = "B";
									jobject14["FBOMID"] = jobject8;
									JArray jarray3 = new JArray();
									serviceHelper.getSubByBom(base.Context, ref jarray3, text8, text13, num2, "1");
									jobject14["FEntity"] = jarray3;
									JObject jobject15 = new JObject();
									jobject15["Model"] = jobject14;
									string text17 = JsonConvert.SerializeObject(jobject15);
									string text18 = "PAEZ_BLPCJ";
									string text19 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
									{
										text18,
										text17
									});
									JObject jobject16 = JObject.Parse(text19);
									object obj3 = jobject16["Result"];
									JObject jobject17 = JObject.Parse(JsonConvert.SerializeObject(obj3));
									object obj4 = jobject17["ResponseStatus"];
									JObject jobject18 = JObject.Parse(JsonConvert.SerializeObject(obj4));
									JArray jarray4 = jobject18["SuccessEntitys"] as JArray;
									text7 = text7 + Convert.ToString((jarray4[0] as JObject)["Id"]) + ",";
								}
								dynamicObject2["F_PAEZ_BILLID"] = text7.TrimEnd(new char[]
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
