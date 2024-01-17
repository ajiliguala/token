using System;
using System.ComponentModel;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000018 RID: 24
	[Description("生产入库单提交生成不良品拆解单插件")]
	public class prdMoSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x0600004A RID: 74 RVA: 0x00006FA4 File Offset: 0x000051A4
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
			e.FieldKeys.Add("FProductType");
			e.FieldKeys.Add("FInStockType");
			e.FieldKeys.Add("FStockId");
			e.FieldKeys.Add("FWorkShopId1");
			e.FieldKeys.Add("FStockLocId");
			e.FieldKeys.Add("FUnitID");
			e.FieldKeys.Add("FRealQty");
			e.FieldKeys.Add("FApproveDate");
			e.FieldKeys.Add("FBomId");
			e.FieldKeys.Add("F_PAEZ_BILLID");
			e.FieldKeys.Add("F_PAEZ_BZSL");
			e.FieldKeys.Add("F_KING_BLLX");
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00007110 File Offset: 0x00005310
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
							string text7 = dynamicObject2["ProductType"].ToString();
							string text8 = dynamicObject2["InStockType"].ToString();
							bool flag4 = text6.Contains("CFOG");
							if (!flag4)
							{
								bool flag5 = text6.Contains("组装层");
								if (!flag5)
								{
									bool flag6 = text6.Contains("FOG");
									if (!flag6)
									{
										bool flag7 = text7.Equals("2") || (text7.Equals("1") && text8.Equals("2"));
										if (flag7)
										{
											string text9 = string.Empty;
											decimal num2 = Convert.ToDecimal(dynamicObject2["RealQty"]);
											decimal num3 = Convert.ToDecimal(dynamicObject2["F_PAEZ_BZSL"]);
											JObject jobject2 = new JObject();
											jobject2["Fnumber"] = text3;
											string text10 = dynamicObject2["MoBillNo"].ToString();
											DateTime moBillDate = serviceHelper.getMoBillDate(base.Context, text10);
											string entryId = dynamicObject2["MoEntryId"].ToString();
											string text11 = dynamicObject2["MaterialId_id"].ToString();
											JObject jobject3 = new JObject();
											jobject3["FMATERIALID"] = text11;
											DynamicObject dynamicObject4 = dynamicObject2["StockId"] as DynamicObject;
											string text12 = dynamicObject4["name"].ToString();
											string text13 = string.Empty;
											bool flag8 = text12.Contains("来料坏");
											if (flag8)
											{
												text13 = "材损";
											}
											else
											{
												bool flag9 = text12.Contains("生产坏");
												if (flag9)
												{
													text13 = "制损";
												}
												else
												{
													text13 = "其他";
												}
											}
											string text14 = dynamicObject4["id"].ToString();
											JObject jobject4 = new JObject();
											jobject4["FSTOCKID"] = text14;
											int moBillmaterial = serviceHelper.getMoBillmaterial(base.Context, entryId);
											JObject jobject5 = new JObject();
											jobject5["FMATERIALID"] = moBillmaterial;
											string text15 = dynamicObject2["WorkShopId_id"].ToString();
											JObject jobject6 = new JObject();
											jobject6["fdeptid"] = text15;
											string text16 = dynamicObject2["StockLocId_id"].ToString();
											string text17 = "";
											bool flag10 = !text16.Equals("0");
											if (flag10)
											{
												text17 = serviceHelper.getLocName(base.Context, text16);
											}
											string text18 = dynamicObject2["FUnitID_id"].ToString();
											JObject jobject7 = new JObject();
											jobject7["FUnitID"] = text18;
											JObject jobject8 = new JObject();
											string text19 = dynamicObject2["BomId_id"].ToString();
											jobject8["FID"] = text19;
											bool flag11 = num3 > 0m;
											if (flag11)
											{
												decimal num4 = Math.Round(num2 / num3 + 0.49999999999m);
												int num5 = 1;
												while (num5 <= num4)
												{
													JObject jobject9 = new JObject();
													jobject9["FMOBILLNO"] = text10;
													jobject9["FMODATE"] = moBillDate;
													jobject9["FSOURCEENTRYID"] = text4;
													jobject9["FTYPE"] = "1";
													jobject9["F_PAEZ_XS"] = num4;
													jobject9["F_PAEZ_BILLNO"] = text2;
													jobject9["F_PAEZ_DATE"] = dateTime;
													jobject9["F_PAEZ_OrgId"] = jobject2;
													jobject9["F_PAEZ_MATERIAL"] = jobject5;
													jobject9["FMATERIALID"] = jobject3;
													jobject9["FSTOCKID"] = jobject4;
													jobject9["F_PAEZ_ZB"] = text13;
													jobject9["FDEPTID"] = jobject6;
													jobject9["FSTOCKLOCNAME"] = text17;
													jobject9["F_PAEZ_UnitID"] = jobject7;
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
													serviceHelper.getSubByBom(base.Context, ref jarray, text11, text19, num6, "1");
													jobject9["FEntity"] = jarray;
													JObject jobject10 = new JObject();
													jobject10["Model"] = jobject9;
													string text20 = JsonConvert.SerializeObject(jobject10);
													string text21 = "PAEZ_BLPCJ";
													string text22 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
													{
														text21,
														text20
													});
													JObject jobject11 = JObject.Parse(text22);
													object obj = jobject11["Result"];
													JObject jobject12 = JObject.Parse(JsonConvert.SerializeObject(obj));
													object obj2 = jobject12["ResponseStatus"];
													JObject jobject13 = JObject.Parse(JsonConvert.SerializeObject(obj2));
													JArray jarray2 = jobject13["SuccessEntitys"] as JArray;
													text9 = text9 + Convert.ToString((jarray2[0] as JObject)["Id"]) + ",";
													num5++;
												}
											}
											else
											{
												JObject jobject14 = new JObject();
												jobject14["FMOBILLNO"] = text10;
												jobject14["FMODATE"] = moBillDate;
												jobject14["FTYPE"] = "1";
												jobject14["F_PAEZ_XS"] = 1;
												jobject14["FSOURCEENTRYID"] = text4;
												jobject14["F_PAEZ_BILLNO"] = text2;
												jobject14["F_PAEZ_DATE"] = dateTime;
												jobject14["F_PAEZ_OrgId"] = jobject2;
												jobject14["F_PAEZ_MATERIAL"] = jobject5;
												jobject14["FMATERIALID"] = jobject3;
												jobject14["FSTOCKID"] = jobject4;
												jobject14["F_PAEZ_ZB"] = text13;
												jobject14["FDEPTID"] = jobject6;
												jobject14["FSTOCKLOCNAME"] = text17;
												jobject14["F_PAEZ_UnitID"] = jobject7;
												jobject14["F_PAEZ_Qty"] = num2;
												jobject14["F_PAEZ_STATUS"] = "B";
												jobject14["FBOMID"] = jobject8;
												JArray jarray3 = new JArray();
												serviceHelper.getSubByBom(base.Context, ref jarray3, text11, text19, num2, "1");
												jobject14["FEntity"] = jarray3;
												JObject jobject15 = new JObject();
												jobject15["Model"] = jobject14;
												string text23 = JsonConvert.SerializeObject(jobject15);
												string text24 = "PAEZ_BLPCJ";
												string text25 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
												{
													text24,
													text23
												});
												JObject jobject16 = JObject.Parse(text25);
												object obj3 = jobject16["Result"];
												JObject jobject17 = JObject.Parse(JsonConvert.SerializeObject(obj3));
												object obj4 = jobject17["ResponseStatus"];
												JObject jobject18 = JObject.Parse(JsonConvert.SerializeObject(obj4));
												JArray jarray4 = jobject18["SuccessEntitys"] as JArray;
												text9 = text9 + Convert.ToString((jarray4[0] as JObject)["Id"]) + ",";
											}
											dynamicObject2["F_PAEZ_BILLID"] = text9.TrimEnd(new char[]
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

		// Token: 0x0600004C RID: 76 RVA: 0x00007B54 File Offset: 0x00005D54
		private void getSubByBomID(ref JArray Jarray, string bomid, decimal qty)
		{
			string text = "select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                           CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                           from T_ENG_BOMCHILD A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID where A.FID=  " + bomid;
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					JObject jobject = new JObject();
					JObject jobject2 = new JObject();
					string text2 = dynamicObject["FMATERIALID"].ToString();
					jobject2["FMATERIALID"] = text2;
					jobject["FSUBMATERIALID"] = jobject2;
					decimal num = Convert.ToDecimal(dynamicObject["FQTY"]);
					decimal num2 = num * qty;
					jobject["FQTY"] = num;
					jobject["FALLQTY"] = num2;
					Jarray.Add(jobject);
					string text3 = dynamicObject["FBOMID"].ToString();
					bool flag2 = text3.Equals("0");
					if (flag2)
					{
						this.getSubBymaterialID(ref Jarray, text2, num2);
					}
					else
					{
						this.getSubByBomID(ref Jarray, text3, num2);
					}
				}
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00007CB0 File Offset: 0x00005EB0
		private void getSubBymaterialID(ref JArray Jarray, string materialid, decimal qty)
		{
			string text = " select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                            CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                            from T_ENG_BOMCHILD  A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID\r\n\t\t\t\t\t\t\tINNER JOIN (SELECT FID,ROW_NUMBER() OVER (PARTITION BY fmaterialid ORDER BY fnumber DESC) AS sx \r\n                            from T_ENG_BOM where FDOCUMENTSTATUS='C'  and FFORBIDSTATUS='A' and fmaterialid=" + materialid + ") C on C.FID=B.FID and C.sx=1";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					JObject jobject = new JObject();
					JObject jobject2 = new JObject();
					string text2 = dynamicObject["FMATERIALID"].ToString();
					jobject2["FMATERIALID"] = text2;
					jobject["FSUBMATERIALID"] = jobject2;
					decimal num = Convert.ToDecimal(dynamicObject["FQTY"]);
					decimal num2 = num * qty;
					jobject["FQTY"] = num;
					jobject["FALLQTY"] = num2;
					Jarray.Add(jobject);
					string text3 = dynamicObject["FBOMID"].ToString();
					bool flag2 = text3.Equals("0");
					if (flag2)
					{
						this.getSubBymaterialID(ref Jarray, text2, num2);
					}
					else
					{
						this.getSubByBomID(ref Jarray, text3, num2);
					}
				}
			}
		}
	}
}
