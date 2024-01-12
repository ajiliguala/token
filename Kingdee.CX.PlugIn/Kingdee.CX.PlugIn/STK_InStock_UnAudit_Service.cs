﻿using System;
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
	// Token: 0x02000014 RID: 20
	[Description("采购入库单反审核删除SRM单据")]
	[HotUpdate]
	public class STK_InStock_UnAudit_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000095 RID: 149 RVA: 0x00007B98 File Offset: 0x00005D98
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FPurchaseOrgId");
			e.FieldKeys.Add("F_SRMIDENT");
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00007BC8 File Offset: 0x00005DC8
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
					string a = Helper.ToStr(dynamicObject["F_SRMIDENT"], 0);
					bool flag2 = a == "1";
					if (flag2)
					{
						DynamicObject dynamicObject2 = dynamicObject["PurchaseOrgId"] as DynamicObject;
						bool flag3 = !dynamicObject2.IsNullOrEmptyOrWhiteSpace();
						if (flag3)
						{
							try
							{
								string text2 = Helper.ToStr(dynamicObject2["Number"], 0);
								string strSQL = string.Format("SELECT F_STYZ_JKDZ,c.FNUMBER ORGBM,F_STYZ_SRMQYH FROM T_STYZ_SRMZZGX a \r\n                                join T_STYZ_SRMZZGX_Entry b on a.fid=b.fid\r\n                                join T_ORG_ORGANIZATIONS c on c.FORGID=b.F_STYZ_FRZZ\r\n                                where a.FDOCUMENTSTATUS='C' and c.FNUMBER='{0}'", text2);
								DynamicObject dynamicObject3 = DBUtils.ExecuteDynamicObject(base.Context, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
								bool flag4 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace();
								if (flag4)
								{
									string text3 = Helper.ToStr(dynamicObject3["F_STYZ_JKDZ"], 0);
									text3 = Helper.BuildApiUrl(text3, STK_InStock_UnAudit_Service.API_DELETE_METHOD);
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
													"STK_InStock"
												}
											}
										}
									});
									string text5 = Utils.HttpPostApi(text3, text4);
									new K3CloudApiLogger(base.Context).Write(true, text, text3, text4, text5, "0", new DateTime?(DateTime.Now), null);
									JObject jobject = (JObject)JsonConvert.DeserializeObject(text5);
									string a2 = jobject["code"].ToString();
									string message = jobject["status"].ToString();
									bool flag5 = a2 == "200";
									if (flag5)
									{
										strSQL = string.Format("update t_STK_InStock set F_SRMIDENT = '0' where FBILLNO= '{0}'", text);
										DBServiceHelper.Execute(base.Context, strSQL);
										list.Add(new OperationMessage
										{
											BillPKValue = num,
											BillNumber = text,
											Title = "采购入库单【" + text + "】",
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
											Title = "采购入库单【" + text + "】",
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
										Title = "采购入库单【" + text + "】",
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
									Title = "采购入库单【" + text + "】",
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
								Title = "采购入库单【" + text + "】",
								State = false,
								Message = "采购组织必填。"
							});
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

		// Token: 0x06000097 RID: 151 RVA: 0x000080A4 File Offset: 0x000062A4
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

		// Token: 0x04000036 RID: 54
		private static readonly string API_DELETE_METHOD = "K3DApi/deleteBill";

		// Token: 0x04000037 RID: 55
		private readonly List<OperateResult> OperateResults = new List<OperateResult>();
	}
}
