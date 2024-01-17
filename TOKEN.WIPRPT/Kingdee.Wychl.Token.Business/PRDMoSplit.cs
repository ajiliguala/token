using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000017 RID: 23
	[Description("生产订单拆分插件")]
	public class PRDMoSplit : AbstractBillPlugIn
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00005D80 File Offset: 0x00003F80
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbspilt");
			if (flag)
			{
				try
				{
					string a = this.Model.GetValue("FDocumentStatus").ToString();
					string a2 = this.Model.GetValue("FCancelStatus").ToString();
					bool flag2 = a != "A" && a != "D";
					if (flag2)
					{
						base.View.ShowErrMessage("只能拆分创建或者重新审核的单据！", "", 0);
					}
					else
					{
						bool flag3 = a2 == "B";
						if (flag3)
						{
							base.View.ShowErrMessage("该单据已经作废，不允许拆分", "", 0);
						}
						else
						{
							bool flag4 = this.Model.GetEntryRowCount("FTreeEntity") > 1;
							if (flag4)
							{
								base.View.ShowErrMessage("该单据明细的数据行数不允许超过1", "", 0);
							}
							else
							{
								decimal num = Convert.ToDecimal(this.Model.GetValue("F_PIKU_LOTPL", 0));
								bool flag5 = num == 0m || ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_PIKU_LOTPH", 0));
								if (flag5)
								{
									base.View.ShowErrMessage("请录入LOT号和LOT批量", "", 0);
								}
								else
								{
									OperateOption operateOption = OperateOption.Create();
									List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
									list.Add(new KeyValuePair<object, object>(base.View.Model.DataObject["id"], ""));
									IOperationResult operationResult = BusinessDataServiceHelper.SetBillStatus(this.Model.Context, this.Model.BillBusinessInfo, list, null, "Cancel", null);
									bool flag6 = !operationResult.IsSuccess;
									if (flag6)
									{
										base.View.ShowErrMessage("单据作废失败，不允许拆分", "", 0);
									}
									object value = this.Model.GetValue("F_PIKU_LOTPH", 0);
									string str = (value != null) ? value.ToString() : null;
									string text = (this.Model.GetValue("FBillType") as DynamicObject)["number"].ToString();
									string text2 = (this.Model.GetValue("FPrdOrgId") as DynamicObject)["number"].ToString();
									string text3 = string.Empty;
									bool flag7 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FWorkGroupId"));
									if (!flag7)
									{
										text3 = (this.Model.GetValue("FWorkGroupId") as DynamicObject)["number"].ToString();
									}
									string text4 = string.Empty;
									bool flag8 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_MESZCD"));
									if (!flag8)
									{
										text4 = (this.Model.GetValue("F_MESZCD") as DynamicObject)["number"].ToString();
									}
									string text5 = string.Empty;
									bool flag9 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FPlannerID"));
									if (!flag9)
									{
										text5 = (this.Model.GetValue("FPlannerID") as DynamicObject)["number"].ToString();
									}
									string text6 = string.Empty;
									bool flag10 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FOwnerId"));
									if (!flag10)
									{
										text6 = (this.Model.GetValue("FOwnerId") as DynamicObject)["number"].ToString();
									}
									string str2 = this.Model.GetValue("FBillNo").ToString();
									DateTime dateTime = Convert.ToDateTime(this.Model.GetValue("FDate"));
									string text7 = this.Model.GetValue("FOwnerTypeId").ToString();
									string text8 = this.Model.GetValue("F_PIKU_CPJD").ToString();
									object value2 = this.Model.GetValue("F_PIKU_KHDDH");
									string text9 = (value2 != null) ? value2.ToString() : null;
									object value3 = this.Model.GetValue("F_KING_MESZCD");
									string text10 = (value3 != null) ? value3.ToString() : null;
									object value4 = this.Model.GetValue("F_khddsl");
									string text11 = (value4 != null) ? value4.ToString() : null;
									object value5 = this.Model.GetValue("F_STYZ_PRINT");
									string text12 = (value5 != null) ? value5.ToString() : null;
									object value6 = this.Model.GetValue("FPPBOMType");
									string text13 = (value6 != null) ? value6.ToString() : null;
									decimal num2 = Convert.ToDecimal(this.Model.GetValue("FQty", 0));
									object value7 = this.Model.GetValue("FSrcBillType", 0);
									string text14 = (value7 != null) ? value7.ToString() : null;
									string text15 = string.Empty;
									bool flag11 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FSrcBillNo", 0));
									if (!flag11)
									{
										text15 = this.Model.GetValue("FSrcBillNo", 0).ToString();
									}
									object value8 = this.Model.GetValue("FSrcBillEntrySeq", 0);
									string text16 = (value8 != null) ? value8.ToString() : null;
									object value9 = this.Model.GetValue("FReqSrc", 0);
									string text17 = (value9 != null) ? value9.ToString() : null;
									string text18 = string.Empty;
									bool flag12 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FSaleOrderNo", 0));
									if (!flag12)
									{
										text18 = this.Model.GetValue("FSaleOrderNo", 0).ToString();
									}
									object value10 = this.Model.GetValue("FSaleOrderEntrySeq", 0);
									string text19 = (value10 != null) ? value10.ToString() : null;
									object value11 = this.Model.GetValue("F_KHBM", 0);
									string text20 = (value11 != null) ? value11.ToString() : null;
									string text21 = string.Empty;
									bool flag13 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_PIKU_CJSCX", 0));
									if (!flag13)
									{
										text21 = (this.Model.GetValue("F_PIKU_CJSCX", 0) as DynamicObject)["number"].ToString();
									}
									string text22 = string.Empty;
									bool flag14 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FBomId", 0));
									if (!flag14)
									{
										text22 = (this.Model.GetValue("FBomId", 0) as DynamicObject)["number"].ToString();
									}
									string text23 = string.Empty;
									bool flag15 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FWorkShopID", 0));
									if (!flag15)
									{
										text23 = (this.Model.GetValue("FWorkShopID", 0) as DynamicObject)["number"].ToString();
									}
									string text24 = string.Empty;
									bool flag16 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_PIKU_MX", 0));
									if (!flag16)
									{
										text24 = this.Model.GetValue("F_PIKU_MX", 0).ToString();
									}
									string text25 = string.Empty;
									bool flag17 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FRoutingId", 0));
									if (!flag17)
									{
										text25 = (this.Model.GetValue("FRoutingId", 0) as DynamicObject)["number"].ToString();
									}
									string text26 = string.Empty;
									bool flag18 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_PPBOX", 0));
									if (!flag18)
									{
										text26 = this.Model.GetValue("F_PPBOX", 0).ToString();
									}
									string text27 = (this.Model.GetValue("FMaterialId", 0) as DynamicObject)["number"].ToString();
									object value12 = this.Model.GetValue("FProductType", 0);
									string text28 = (value12 != null) ? value12.ToString() : null;
									DateTime dateTime2 = Convert.ToDateTime(this.Model.GetValue("FPlanFinishDate", 0));
									DateTime dateTime3 = Convert.ToDateTime(this.Model.GetValue("FPlanStartDate", 0));
									string text29 = this.Model.GetValue("FCreateType", 0).ToString();
									string text30 = (this.Model.GetValue("FStockInOrgId", 0) as DynamicObject)["number"].ToString();
									string text31 = (this.Model.GetValue("FUnitId", 0) as DynamicObject)["number"].ToString();
									string text32 = this.Model.GetValue("FReqType", 0).ToString();
									object value13 = this.Model.GetValue("F_KING_LOTREMARK", 0);
									string text33 = (value13 != null) ? value13.ToString() : null;
									DynamicObjectCollection dynamicObjectCollection = (base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection)[0]["FTREEENTITY_Link"] as DynamicObjectCollection;
									string text34 = string.Empty;
									string text35 = string.Empty;
									string text36 = string.Empty;
									string text37 = string.Empty;
									bool flag19 = false;
									bool flag20 = dynamicObjectCollection.Count > 0;
									if (flag20)
									{
										DynamicObject dynamicObject = dynamicObjectCollection[0];
										flag19 = true;
										text34 = dynamicObject["Sid"].ToString();
										text35 = dynamicObject["SBillId"].ToString();
										text36 = dynamicObject["RuleId"].ToString();
										text37 = dynamicObject["STableName"].ToString();
									}
									decimal num3 = 0m;
									JArray jarray = new JArray();
									int num4 = 1;
									while (num3 < num2)
									{
										JObject jobject = new JObject();
										JObject jobject2 = new JObject();
										jobject2["FNumber"] = text;
										jobject["FBillType"] = jobject2;
										JObject jobject3 = new JObject();
										jobject3["FNumber"] = text2;
										jobject["FPrdOrgId"] = jobject3;
										jobject["FBillNo"] = str2 + "-" + num4.ToString().PadLeft(3, '0');
										jobject["FDate"] = dateTime;
										jobject["FOwnerTypeId"] = text7;
										jobject["F_PIKU_CPJD"] = text8;
										jobject["F_KING_MESZCD"] = text10;
										jobject["FPPBOMType"] = text13;
										bool flag21 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text3);
										if (!flag21)
										{
											JObject jobject4 = new JObject();
											jobject4["FNumber"] = text3;
											jobject["FWorkGroupId"] = jobject4;
										}
										bool flag22 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4);
										if (!flag22)
										{
											JObject jobject5 = new JObject();
											jobject5["FNumber"] = text4;
											jobject["F_MESZCD"] = jobject5;
										}
										bool flag23 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text5);
										if (!flag23)
										{
											JObject jobject6 = new JObject();
											jobject6["FNumber"] = text5;
											jobject["FPlannerID"] = jobject6;
										}
										bool flag24 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text6);
										if (!flag24)
										{
											JObject jobject7 = new JObject();
											jobject7["FNumber"] = text6;
											jobject["FOwnerId"] = jobject7;
										}
										JArray jarray2 = new JArray();
										JObject jobject8 = new JObject();
										jobject8["F_PIKU_LOTPH"] = str + "-" + num4.ToString().PadLeft(3, '0');
										jobject8["F_PIKU_LOTPL"] = num;
										jobject8["F_KHBM"] = text20;
										bool flag25 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text21);
										if (!flag25)
										{
											JObject jobject9 = new JObject();
											jobject9["FNumber"] = text21;
											jobject8["F_PIKU_CJSCX"] = jobject9;
										}
										jobject8["FProductType"] = text28;
										JObject jobject10 = new JObject();
										jobject10["FNumber"] = text27;
										jobject8["FMaterialId"] = jobject10;
										bool flag26 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text22);
										if (!flag26)
										{
											JObject jobject11 = new JObject();
											jobject11["FNumber"] = text22;
											jobject8["FBomId"] = jobject11;
										}
										bool flag27 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text23);
										if (!flag27)
										{
											JObject jobject12 = new JObject();
											jobject12["FNumber"] = text23;
											jobject8["FWorkShopID"] = jobject12;
										}
										JObject jobject13 = new JObject();
										jobject13["FNumber"] = text31;
										jobject8["FUnitId"] = jobject13;
										decimal num5 = 0m;
										bool flag28 = num3 + num <= num2;
										if (flag28)
										{
											num5 = num;
											jobject8["FQty"] = num5;
											jobject8["FBaseUnitQty "] = num5;
											num3 += num;
										}
										else
										{
											num5 = num2 - num3;
											jobject8["FQty"] = num5;
											jobject8["FBaseUnitQty "] = num5;
											num3 = num2;
										}
										jobject8["FPlanStartDate"] = dateTime3;
										jobject8["FPlanFinishDate"] = dateTime2;
										jobject8["FCreateType"] = text29;
										JObject jobject14 = new JObject();
										jobject14["FNumber"] = text25;
										jobject8["FRoutingId"] = jobject14;
										JObject jobject15 = new JObject();
										jobject15["FNumber"] = text30;
										jobject8["FStockInOrgId"] = jobject15;
										jobject8["FReqType"] = text32;
										jobject8["FSrcBillType"] = text14;
										jobject8["FSrcBillNo"] = text15;
										jobject8["FSrcBillEntrySeq"] = text16;
										jobject8["FReqSrc"] = text17;
										jobject8["FSaleOrderNo"] = text18;
										jobject8["FSaleOrderEntrySeq"] = text19;
										jobject8["F_PIKU_MX"] = text24;
										jobject8["F_PPBOX"] = text26;
										jobject8["F_KING_LOTREMARK"] = text33;
										jobject8["F_PIKU_KHDDH"] = text9;
										jobject8["F_khddsl"] = text11;
										jobject8["F_STYZ_PRINT"] = text12;
										jarray2.Add(jobject8);
										bool flag29 = flag19;
										if (flag29)
										{
											JArray jarray3 = new JArray();
											JObject jobject16 = new JObject();
											jobject16["FTREEENTITY_Link_FRuleId"] = text36;
											jobject16["FTREEENTITY_Link_FSTableName"] = text37;
											jobject16["FTREEENTITY_Link_FSBillId"] = text35;
											jobject16["FTREEENTITY_Link_FSId"] = text34;
											jobject16["FTREEENTITY_Link_FBaseUnitQty"] = num5;
											jarray3.Add(jobject16);
											jobject8["FTREEENTITY_Link"] = jarray3;
										}
										jobject["FTreeEntity"] = jarray2;
										jarray.Add(jobject);
										num4++;
									}
									bool flag30 = jarray.Count > 0;
									if (flag30)
									{
										JObject jobject17 = new JObject();
										jobject17["IsEntryBatchFill"] = false;
										jobject17["Model"] = jarray;
										string text38 = JsonConvert.SerializeObject(jobject17);
										string text39 = "PRD_MO";
										Logger.Info("PRDMoSplit", text38);
										object obj = WebApiServiceCall.BatchSave(ObjectUtils.CreateCopy(base.Context) as Context, text39, text38);
										JObject jobject18 = JObject.Parse(JsonConvert.SerializeObject(obj));
										Logger.Info("PRDMoSplit", JsonConvert.SerializeObject(jobject18));
										object obj2 = jobject18["Result"];
										bool flag31 = ObjectUtils.IsNullOrEmpty(obj2);
										if (flag31)
										{
											base.View.ShowErrMessage("生产订单保存失败，拆分失败", "", 0);
										}
										JObject jobject19 = JObject.Parse(JsonConvert.SerializeObject(obj2));
										object obj3 = jobject19["ResponseStatus"];
										bool flag32 = ObjectUtils.IsNullOrEmpty(obj3);
										if (flag32)
										{
											base.View.ShowErrMessage("生产订单保存失败，拆分失败", "", 0);
										}
										JObject jobject20 = JObject.Parse(JsonConvert.SerializeObject(obj3));
										string text40 = Convert.ToString(jobject20["IsSuccess"]);
										bool flag33 = !StringUtils.EqualsIgnoreCase(text40, "true");
										if (flag33)
										{
											JArray jarray4 = jobject20["Errors"] as JArray;
											bool flag34 = !ObjectUtils.IsNullOrEmpty(jarray4) || jarray4.Count > 0;
											if (flag34)
											{
												string arg = Convert.ToString((jarray4[0] as JObject)["FieldName"]);
												string text41 = Convert.ToString((jarray4[0] as JObject)["Message"]);
												text41 = string.Format("{0}:{1}", arg, text41);
												bool flag35 = text41.Length > 2000;
												if (flag35)
												{
													text41 = text41.Substring(0, 2000);
												}
												base.View.ShowErrMessage(text41, "", 0);
											}
										}
										base.View.ShowMessage("生产订单拆分成功!", 0);
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					base.View.ShowErrMessage(ex.Message, "", 0);
				}
			}
		}
	}
}
