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
	// Token: 0x0200001A RID: 26
	[Description("采购价目表审核更新物料以及生成价格变动单据插件")]
	public class PriceAudit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000052 RID: 82 RVA: 0x000089AC File Offset: 0x00006BAC
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FNumber");
			e.FieldKeys.Add("FPriceListEntry");
			e.FieldKeys.Add("F_ExpPeriod");
			e.FieldKeys.Add("F_MinPOQty");
			e.FieldKeys.Add("F_IncreaseQty");
			e.FieldKeys.Add("F_FixLeadTime");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("FCreateOrgId");
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00008A4C File Offset: 0x00006C4C
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["PriceListEntry"] as DynamicObjectCollection;
				DynamicObject dynamicObject2 = dynamicObject["CreateOrgId"] as DynamicObject;
				long num = Convert.ToInt64(dynamicObject2["id"]);
				bool flag = Convert.ToBoolean(SystemParameterServiceHelper.GetParamter(base.Context, num, 0L, "PUR_SystemParameter", "F_KING_ISUPDATE", 0L).ToString());
				bool flag2 = flag;
				if (flag2)
				{
					bool flag3 = false;
					JArray jarray = new JArray();
					JArray jarray2 = new JArray();
					foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
					{
						DynamicObject dynamicObject4 = dynamicObject3["MaterialId"] as DynamicObject;
						string arg = dynamicObject4["msterid"].ToString();
						DynamicObject dynamicObject5 = (dynamicObject4["MaterialPlan"] as DynamicObjectCollection)[0];
						int num2 = Convert.ToInt32((dynamicObject4["MaterialStock"] as DynamicObjectCollection)[0]["ExpPeriod"]);
						int num3 = Convert.ToInt32(dynamicObject5["MinPOQty"]);
						int num4 = Convert.ToInt32(dynamicObject5["IncreaseQty"]);
						int num5 = Convert.ToInt32(dynamicObject5["FixLeadTime"]);
						int num6 = Convert.ToInt32(dynamicObject3["F_ExpPeriod"]);
						int num7 = Convert.ToInt32(dynamicObject3["F_MinPOQty"]);
						int num8 = Convert.ToInt32(dynamicObject3["F_IncreaseQty"]);
						int num9 = Convert.ToInt32(dynamicObject3["F_FixLeadTime"]);
						bool flag4 = num2 != num6 || num5 != num9 || num4 != num8 || num3 != num7;
						if (flag4)
						{
							string text = string.Format("/*dialect*/select tbm.FMATERIALID from T_BD_MATERIAL tbm\r\n                            inner join T_ORG_ORGANIZATIONS too on too.FORGID=tbm.FUSEORGID\r\n                            where tbm.FMASTERID={0} and too.FPARENTID={1}", arg, num);
							DynamicObjectCollection dynamicObjectCollection2 = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
							bool flag5 = dynamicObjectCollection2.Count > 0;
							if (flag5)
							{
								flag3 = true;
								foreach (DynamicObject dynamicObject6 in dynamicObjectCollection2)
								{
									JObject jobject = new JObject();
									jobject["FMATERIALID"] = dynamicObject6["FMATERIALID"].ToString();
									JObject jobject2 = new JObject();
									jobject2["FExpPeriod"] = num6;
									jobject["SubHeadEntity1"] = jobject2;
									JObject jobject3 = new JObject();
									jobject3["FFixLeadTime"] = num9;
									jobject3["FMinPOQty"] = num7;
									jobject3["FIncreaseQty"] = num8;
									jobject["SubHeadEntity4"] = jobject3;
									jarray.Add(jobject);
								}
								JObject jobject4 = new JObject();
								JObject jobject5 = new JObject();
								jobject5["FNUMBER"] = dynamicObject4["number"].ToString();
								jobject4["F_MATERIAL"] = jobject5;
								jobject4["F_materialExpPeriod"] = num2;
								jobject4["F_materialFixLeadTime"] = num5;
								jobject4["F_materialIncreaseQty"] = num4;
								jobject4["F_materialMinPOQty"] = num3;
								jobject4["F_priceExpPeriod"] = num6;
								jobject4["F_priceFixLeadTime"] = num9;
								jobject4["F_priceIncreaseQty"] = num8;
								jobject4["F_priceMinPOQty"] = num7;
								jarray2.Add(jobject4);
							}
						}
					}
					bool flag6 = flag3;
					if (flag6)
					{
						K3CloudApiClient k3CloudApiClient = new K3CloudApiClient("http://localhost/k3cloud/");
						string dbid = base.Context.DBId;
						apiParameter parameterByDbid = serviceHelper.getParameterByDbid(dbid);
						string text2 = k3CloudApiClient.LoginByAppSecret(dbid, parameterByDbid.apiuser, parameterByDbid.appid, parameterByDbid.appSecret, 2052);
						JObject jobject6 = JObject.Parse(text2);
						int num10 = Extensions.Value<int>(jobject6["LoginResultType"]);
						bool flag7 = num10 != 1 && num10 != -5;
						if (flag7)
						{
							throw new Exception("api登录失败");
						}
						JObject jobject7 = new JObject();
						jobject7["Model"] = jarray;
						JArray jarray3 = new JArray();
						jarray3.Add("FMATERIALID");
						jarray3.Add("FExpPeriod");
						jarray3.Add("SubHeadEntity1");
						jarray3.Add("FFixLeadTime");
						jarray3.Add("FMinPOQty");
						jarray3.Add("FIncreaseQty");
						jarray3.Add("SubHeadEntity4");
						jobject7["NeedUpDateFields"] = jarray3;
						string text3 = JsonConvert.SerializeObject(jobject7);
						string text4 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.BatchSave", new object[]
						{
							"BD_MATERIAL",
							text3
						});
						bool flag8 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4);
						if (flag8)
						{
							throw new Exception("修改物料失败");
						}
						JObject jobject8 = JObject.Parse(text4);
						object obj = jobject8["Result"];
						bool flag9 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj);
						if (flag9)
						{
							throw new Exception("修改物料失败");
						}
						JObject jobject9 = JObject.Parse(JsonConvert.SerializeObject(obj));
						object obj2 = jobject9["ResponseStatus"];
						bool flag10 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj2);
						if (flag10)
						{
							throw new Exception("修改物料失败");
						}
						JObject jobject10 = JObject.Parse(JsonConvert.SerializeObject(obj2));
						string text5 = Convert.ToString(jobject10["IsSuccess"]);
						bool flag11 = !text5.ToLowerInvariant().Equals("true");
						if (flag11)
						{
							JArray jarray4 = jobject10["Errors"] as JArray;
							string text6 = string.Empty;
							bool flag12 = !ObjectUtils.IsNullOrEmpty(jarray4) || jarray4.Count > 0;
							if (flag12)
							{
								for (int j = 0; j < jarray4.Count; j++)
								{
									text6 += string.Format("{0}:{1}", Convert.ToString((jarray4[j] as JObject)["FieldName"]), Convert.ToString((jarray4[j] as JObject)["Message"]));
								}
							}
							bool flag13 = text6.Length > 2000;
							if (flag13)
							{
								text6 = text6.Substring(0, 2000);
							}
							throw new Exception("修改物料失败,具体提示为：" + text6);
						}
						JObject jobject11 = new JObject();
						JObject jobject12 = new JObject();
						jobject12["FNUMBER"] = dynamicObject2["number"].ToString();
						jobject11["F_ORGID"] = jobject12;
						jobject11["F_BILLNO"] = dynamicObject["Number"].ToString();
						jobject11["FEntity"] = jarray2;
						JObject jobject13 = new JObject();
						jobject13["Model"] = jobject11;
						string text7 = JsonConvert.SerializeObject(jobject13);
						string text8 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
						{
							"PCQE_CGXXBD",
							text7
						});
					}
				}
			}
		}
	}
}
