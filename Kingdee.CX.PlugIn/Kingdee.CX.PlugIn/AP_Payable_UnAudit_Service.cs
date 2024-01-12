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
	// Token: 0x02000008 RID: 8
	[Description("暂估应付反审核删除SRM单据")]
	[HotUpdate]
	public class AP_Payable_UnAudit_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000063 RID: 99 RVA: 0x0000368C File Offset: 0x0000188C
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FPAYORGID");
			e.FieldKeys.Add("FSetAccountType");
			e.FieldKeys.Add("F_SRMIDENT");
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000036CC File Offset: 0x000018CC
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			this.OperateResults.Clear();
			List<OperationMessage> list = new List<OperationMessage>();
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_UnAudit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					long num = Helper.ToLong(dynamicObject["Id"]);
					string text = Helper.ToStr(dynamicObject["BillNo"], 0);
					string a = Helper.ToStr(dynamicObject["FSetAccountType"], 0);
					bool flag2 = a != "2";
					if (!flag2)
					{
						string a2 = Helper.ToStr(dynamicObject["F_SRMIDENT"], 0);
						bool flag3 = a2 == "1";
						if (flag3)
						{
							DynamicObject dynamicObject2 = dynamicObject["FPAYORGID"] as DynamicObject;
							bool flag4 = !dynamicObject2.IsNullOrEmptyOrWhiteSpace();
							if (flag4)
							{
								try
								{
									string text2 = Helper.ToStr(dynamicObject2["Number"], 0);
									string strSQL = string.Format("SELECT F_STYZ_JKDZ,c.FNUMBER ORGBM,F_STYZ_SRMQYH FROM T_STYZ_SRMZZGX a \r\n                                join T_STYZ_SRMZZGX_Entry b on a.fid=b.fid\r\n                                join T_ORG_ORGANIZATIONS c on c.FORGID=b.F_STYZ_FRZZ\r\n                                where a.FDOCUMENTSTATUS='C' and c.FNUMBER='{0}'", text2);
									DynamicObject dynamicObject3 = DBUtils.ExecuteDynamicObject(base.Context, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
									bool flag5 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace();
									if (flag5)
									{
										string text3 = Helper.ToStr(dynamicObject3["F_STYZ_JKDZ"], 0);
										text3 = Helper.BuildApiUrl(text3, AP_Payable_UnAudit_Service.API_DELETE_METHOD);
										string value = Helper.ToStr(dynamicObject3["F_STYZ_SRMQYH"], 0);
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
													},
													{
														"BILL_TYPE_CODE",
														"AP_Payable"
													}
												}
											}
										});
										string text5 = Utils.HttpPostApi(text3, text4);
										new K3CloudApiLogger(base.Context).Write(true, text, text3, text4, text5, "0", new DateTime?(DateTime.Now), null);
										JObject jobject = (JObject)JsonConvert.DeserializeObject(text5);
										string a3 = jobject["code"].ToString();
										string message = jobject["status"].ToString();
										bool flag6 = a3 == "200";
										if (flag6)
										{
											strSQL = string.Format("update T_AP_PAYABLE set F_SRMIDENT = '0' where FBILLNO= '{0}'", text);
											DBServiceHelper.Execute(base.Context, strSQL);
											list.Add(new OperationMessage
											{
												BillPKValue = num,
												BillNumber = text,
												Title = "暂估应付【" + text + "】",
												State = true,
												Message = "删除SRM单据成功。"
											});
										}
										else
										{
											list.Add(new OperationMessage
											{
												BillPKValue = num,
												BillNumber = text,
												Title = "暂估应付【" + text + "】",
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
											Title = "暂估应付【" + text + "】",
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
										Title = "暂估应付【" + text + "】",
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
									Title = "暂估应付【" + text + "】",
									State = false,
									Message = "付款组织必填。"
								});
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
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00003BD4 File Offset: 0x00001DD4
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

		// Token: 0x04000024 RID: 36
		private static readonly string API_DELETE_METHOD = "K3DApi/deleteBill";

		// Token: 0x04000025 RID: 37
		private readonly List<OperateResult> OperateResults = new List<OperateResult>();
	}
}
