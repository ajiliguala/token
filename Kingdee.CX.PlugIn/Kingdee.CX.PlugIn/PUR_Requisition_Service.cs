using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Toolkit.Helpers;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000010 RID: 16
	[Description("采购申请单推送SRM")]
	[HotUpdate]
	public class PUR_Requisition_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000085 RID: 133 RVA: 0x00005A40 File Offset: 0x00003C40
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FApplicationOrgId");
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("FPurchaseOrgId");
			e.FieldKeys.Add("F_SRMIDENT");
			e.FieldKeys.Add("FDocumentStatus");
			e.FieldKeys.Add("FCloseStatus");
			e.FieldKeys.Add("FCancelStatus");
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00005AD0 File Offset: 0x00003CD0
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			this.OperateResults.Clear();
			List<OperationMessage> list = new List<OperationMessage>();
			bool flag = base.FormOperation.Operation == "PUSHSRM";
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					long num = Helper.ToLong(dynamicObject["Id"]);
					string text = Helper.ToStr(dynamicObject["BillNo"], 0);
					string a = Helper.ToStr(dynamicObject["F_SRMIDENT"], 0);
					string a2 = Helper.ToStr(dynamicObject["DocumentStatus"], 0);
					string a3 = Helper.ToStr(dynamicObject["CloseStatus"], 0);
					string a4 = Helper.ToStr(dynamicObject["CancelStatus"], 0);
					bool flag2 = a2 != "C";
					if (flag2)
					{
						list.Add(new OperationMessage
						{
							BillPKValue = num,
							BillNumber = text,
							Title = "采购申请单【" + text + "】",
							State = false,
							Message = "未审核，不能推送！"
						});
					}
					else
					{
						bool flag3 = a3 == "B";
						if (flag3)
						{
							list.Add(new OperationMessage
							{
								BillPKValue = num,
								BillNumber = text,
								Title = "采购申请单【" + text + "】",
								State = false,
								Message = "已关闭，不能推送！"
							});
						}
						else
						{
							bool flag4 = a4 == "B";
							if (flag4)
							{
								list.Add(new OperationMessage
								{
									BillPKValue = num,
									BillNumber = text,
									Title = "采购申请单【" + text + "】",
									State = false,
									Message = "已作废，不能推送！"
								});
							}
							else
							{
								bool flag5 = a != "1";
								if (flag5)
								{
									string text2 = "";
									DynamicObjectCollection dynamicObjectCollection = dynamicObject["ReqEntry"] as DynamicObjectCollection;
									foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
									{
										DynamicObject dynamicObject3 = dynamicObject2["PurchaseOrgId"] as DynamicObject;
										bool flag6 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace();
										if (flag6)
										{
											text2 = Helper.ToStr(dynamicObject3["Number"], 0);
										}
									}
									bool flag7 = !string.IsNullOrEmpty(text2);
									if (flag7)
									{
										try
										{
											string strSQL = string.Format("SELECT F_STYZ_JKDZ,c.FNUMBER ORGBM,F_STYZ_SRMQYH FROM T_STYZ_SRMZZGX a \r\n                                join T_STYZ_SRMZZGX_Entry b on a.fid=b.fid\r\n                                join T_ORG_ORGANIZATIONS c on c.FORGID=b.F_STYZ_FRZZ\r\n                                where a.FDOCUMENTSTATUS='C' and c.FNUMBER='{0}'", text2);
											DynamicObject dynamicObject4 = DBUtils.ExecuteDynamicObject(base.Context, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
											bool flag8 = !dynamicObject4.IsNullOrEmptyOrWhiteSpace();
											if (flag8)
											{
												string text3 = Helper.ToStr(dynamicObject4["F_STYZ_JKDZ"], 0);
												text3 = Helper.BuildApiUrl(text3, PUR_Requisition_Service.API_METHOD);
												string value = Helper.ToStr(dynamicObject4["F_STYZ_SRMQYH"], 0);
												string text4 = JsonConvert.SerializeObject(new Dictionary<string, object>
												{
													{
														"data",
														new Dictionary<string, object>
														{
															{
																"DOC_NO",
																text
															},
															{
																"COMPANY_CODE",
																value
															}
														}
													}
												});
												string text5 = Utils.HttpPostApi(text3, text4);
												new K3CloudApiLogger(base.Context).Write(true, text, text3, text4, text5, "0", new DateTime?(DateTime.Now), null);
												JObject jobject = (JObject)JsonConvert.DeserializeObject(text5);
												string a5 = jobject["code"].ToString();
												string message = jobject["status"].ToString();
												bool flag9 = a5 == "200";
												if (flag9)
												{
													strSQL = string.Format("update T_PUR_Requisition set F_SRMIDENT = '1' where FBILLNO= '{0}'", text);
													DBServiceHelper.Execute(base.Context, strSQL);
													list.Add(new OperationMessage
													{
														BillPKValue = num,
														BillNumber = text,
														Title = "采购申请单【" + text + "】",
														State = true,
														Message = "推送成功。"
													});
												}
												else
												{
													list.Add(new OperationMessage
													{
														BillPKValue = num,
														BillNumber = text,
														Title = "采购申请单【" + text + "】",
														State = false,
														Message = message
													});
												}
											}
											else
											{
												list.Add(new OperationMessage
												{
													BillPKValue = num,
													BillNumber = text,
													Title = "采购申请单【" + text + "】",
													State = false,
													Message = "金蝶对应SRM组织关系表没有维护组织【" + text2 + "】的SRM企业号数据！"
												});
											}
										}
										catch (Exception ex)
										{
											list.Add(new OperationMessage
											{
												BillPKValue = num,
												BillNumber = text,
												Title = "采购申请单【" + text + "】",
												State = false,
												Message = ex.Message
											});
										}
									}
									else
									{
										list.Add(new OperationMessage
										{
											BillPKValue = num,
											BillNumber = text,
											Title = "采购申请单【" + text + "】",
											State = false,
											Message = "采购组织必填。"
										});
									}
								}
								else
								{
									list.Add(new OperationMessage
									{
										BillPKValue = num,
										BillNumber = text,
										Title = "采购申请单【" + text + "】",
										State = false,
										Message = "已推送SRM，不能重复推送！"
									});
								}
							}
						}
					}
				}
				foreach (OperationMessage operationMessage in list)
				{
					this.OperateResults.Add(new OperateResult
					{
						PKValue = operationMessage.BillPKValue,
						Number = operationMessage.BillNumber,
						Name = operationMessage.Title,
						Message = operationMessage.Message,
						SuccessStatus = operationMessage.State,
						MessageType = (operationMessage.State ? MessageType.Normal : MessageType.FatalError)
					});
				}
			}
			bool flag10 = base.FormOperation.OperationId == FormOperation.Operation_UnAudit;
			if (flag10)
			{
				foreach (DynamicObject dynamicObject5 in e.DataEntitys)
				{
					long num2 = Helper.ToLong(dynamicObject5["Id"]);
					string text6 = Helper.ToStr(dynamicObject5["BillNo"], 0);
					string a6 = Helper.ToStr(dynamicObject5["F_SRMIDENT"], 0);
					bool flag11 = a6 == "1";
					if (flag11)
					{
						string text7 = "";
						DynamicObjectCollection dynamicObjectCollection2 = dynamicObject5["ReqEntry"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject6 in dynamicObjectCollection2)
						{
							DynamicObject dynamicObject7 = dynamicObject6["PurchaseOrgId"] as DynamicObject;
							bool flag12 = !dynamicObject7.IsNullOrEmptyOrWhiteSpace();
							if (flag12)
							{
								text7 = Helper.ToStr(dynamicObject7["Number"], 0);
							}
						}
						bool flag13 = !string.IsNullOrEmpty(text7);
						if (flag13)
						{
							try
							{
								string strSQL2 = string.Format("SELECT F_STYZ_JKDZ,c.FNUMBER ORGBM,F_STYZ_SRMQYH FROM T_STYZ_SRMZZGX a \r\n                                join T_STYZ_SRMZZGX_Entry b on a.fid=b.fid\r\n                                join T_ORG_ORGANIZATIONS c on c.FORGID=b.F_STYZ_FRZZ\r\n                                where a.FDOCUMENTSTATUS='C' and c.FNUMBER='{0}'", text7);
								DynamicObject dynamicObject8 = DBUtils.ExecuteDynamicObject(base.Context, strSQL2, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
								bool flag14 = !dynamicObject8.IsNullOrEmptyOrWhiteSpace();
								if (flag14)
								{
									string text8 = Helper.ToStr(dynamicObject8["F_STYZ_JKDZ"], 0);
									text8 = Helper.BuildApiUrl(text8, PUR_Requisition_Service.API_DELETE_METHOD);
									string value2 = Helper.ToStr(dynamicObject8["F_STYZ_SRMQYH"], 0);
									string text9 = JsonConvert.SerializeObject(new Dictionary<string, object>
									{
										{
											"data",
											new Dictionary<string, object>
											{
												{
													"DOC_NO",
													text6
												},
												{
													"COMPANY_CODE",
													value2
												},
												{
													"BILL_TYPE_CODE",
													"PUR_Requisition"
												}
											}
										}
									});
									string text10 = Utils.HttpPostApi(text8, text9);
									new K3CloudApiLogger(base.Context).Write(true, text6, text8, text9, text10, "0", new DateTime?(DateTime.Now), null);
									JObject jobject2 = (JObject)JsonConvert.DeserializeObject(text10);
									string a7 = jobject2["code"].ToString();
									string message2 = jobject2["status"].ToString();
									bool flag15 = a7 == "200";
									if (flag15)
									{
										strSQL2 = string.Format("update T_PUR_Requisition set F_SRMIDENT = '0' where FBILLNO= '{0}'", text6);
										DBServiceHelper.Execute(base.Context, strSQL2);
										list.Add(new OperationMessage
										{
											BillPKValue = num2,
											BillNumber = text6,
											Title = "采购申请单【" + text6 + "】",
											State = true,
											Message = "删除SRM单据成功。"
										});
									}
									else
									{
										list.Add(new OperationMessage
										{
											BillPKValue = num2,
											BillNumber = text6,
											Title = "采购申请单【" + text6 + "】",
											State = false,
											Message = message2
										});
									}
								}
								else
								{
									list.Add(new OperationMessage
									{
										BillPKValue = num2,
										BillNumber = text6,
										Title = "采购申请单【" + text6 + "】",
										State = false,
										Message = "金蝶对应SRM组织关系表没有维护组织【" + text7 + "】的SRM企业号数据！"
									});
								}
							}
							catch (Exception ex2)
							{
								list.Add(new OperationMessage
								{
									BillPKValue = num2,
									BillNumber = text6,
									Title = "采购申请单【" + text6 + "】",
									State = false,
									Message = ex2.Message
								});
							}
						}
						else
						{
							list.Add(new OperationMessage
							{
								BillPKValue = num2,
								BillNumber = text6,
								Title = "采购申请单【" + text6 + "】",
								State = false,
								Message = "采购组织必填。"
							});
						}
					}
				}
				foreach (OperationMessage operationMessage2 in list)
				{
					this.OperateResults.Add(new OperateResult
					{
						PKValue = operationMessage2.BillPKValue,
						Number = operationMessage2.BillNumber,
						Name = operationMessage2.Title,
						Message = operationMessage2.Message,
						SuccessStatus = operationMessage2.State,
						MessageType = (operationMessage2.State ? MessageType.Normal : MessageType.FatalError)
					});
				}
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00006704 File Offset: 0x00004904
		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			base.OperationResult.OperateResult.Clear();
			foreach (OperateResult item in this.OperateResults)
			{
				base.OperationResult.OperateResult.Add(item);
			}
			base.OperationResult.IsShowMessage = true;
		}

		// Token: 0x0400002F RID: 47
		private static readonly string API_METHOD = "K3DApi/savePurchaseRequisition";

		// Token: 0x04000030 RID: 48
		private static readonly string API_DELETE_METHOD = "K3DApi/deleteBill";

		// Token: 0x04000031 RID: 49
		private readonly List<OperateResult> OperateResults = new List<OperateResult>();
	}
}
