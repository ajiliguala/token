using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200001D RID: 29
	public class purOrderList : AbstractListPlugIn
	{
		// Token: 0x0600005B RID: 91 RVA: 0x00009450 File Offset: 0x00007650
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string text = e.Operation.Operation ?? "";
			bool flag = StringUtils.EqualsIgnoreCase(text, "tbcreateitem");
			if (flag)
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				string[] primaryKeyValues = selectedRowsInfo.GetPrimaryKeyValues();
				bool flag2 = primaryKeyValues.Length < 1;
				if (flag2)
				{
					this.View.ShowErrMessage("请选择至少一条分录", "", 0);
				}
				else
				{
					List<string> list = new List<string>();
					for (int i = 0; i < primaryKeyValues.Length; i++)
					{
						bool flag3 = !list.Contains(primaryKeyValues[i]);
						if (flag3)
						{
							list.Add(primaryKeyValues[i]);
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < list.Count; j++)
					{
						stringBuilder.Append(serviceHelper.ListCreteItem(base.Context, list[j]));
					}
					bool flag4 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder);
					if (flag4)
					{
						this.View.ShowMessage(stringBuilder.ToString(), 0);
					}
					else
					{
						this.View.ShowMessage("推送签章系统成功", 0);
					}
				}
			}
			else
			{
				bool flag5 = StringUtils.EqualsIgnoreCase(text, "tbdosign");
				if (flag5)
				{
					ListSelectedRowCollection selectedRowsInfo2 = this.ListView.SelectedRowsInfo;
					string[] primaryKeyValues2 = selectedRowsInfo2.GetPrimaryKeyValues();
					bool flag6 = primaryKeyValues2.Length < 1;
					if (flag6)
					{
						this.View.ShowErrMessage("请选择至少一条分录", "", 0);
					}
					else
					{
						List<string> list2 = new List<string>();
						for (int k = 0; k < primaryKeyValues2.Length; k++)
						{
							bool flag7 = !list2.Contains(primaryKeyValues2[k]);
							if (flag7)
							{
								list2.Add(primaryKeyValues2[k]);
							}
						}
						StringBuilder stringBuilder2 = new StringBuilder();
						for (int l = 0; l < list2.Count; l++)
						{
							stringBuilder2.Append(serviceHelper.doSign(base.Context, list2[l]));
						}
						bool flag8 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder2);
						if (flag8)
						{
							this.View.ShowMessage(stringBuilder2.ToString(), 0);
						}
						else
						{
							this.View.ShowMessage("签章成功", 0);
						}
					}
				}
				else
				{
					bool flag9 = StringUtils.EqualsIgnoreCase(text, "tbCreateSubItem");
					if (flag9)
					{
						ListSelectedRowCollection selectedRowsInfo3 = this.ListView.SelectedRowsInfo;
						string[] primaryKeyValues3 = selectedRowsInfo3.GetPrimaryKeyValues();
						bool flag10 = primaryKeyValues3.Length < 1;
						if (flag10)
						{
							this.View.ShowErrMessage("请选择至少一条分录", "", 0);
						}
						else
						{
							string text2 = string.Empty;
							for (int m = 0; m < primaryKeyValues3.Length; m++)
							{
								text2 = text2 + primaryKeyValues3[m] + ",";
							}
							string text3 = "select A.FID,A.FATTACHMENTNAME,A.FFILEID,B.F_KING_QZBILLID,C.F_KING_SUBID,D.FSOCIALCRECODE,B.FBILLNO,C.F_KING_ACCOUNT,C.F_KING_NUMBER from T_BAS_ATTACHMENT A\r\n                                inner join t_PUR_POOrder B on A.FINTERID=b.FID  and A.FBILLTYPE='PUR_PurchaseOrder' and A.FEXTNAME='.pdf' and A.FENTRYINTERID!=-1\r\n                                inner join KING_QZXGCSENTRY C  on C.F_KING_ORGID=B.FPURCHASEORGID and C.F_KING_BILLTYPE=B.FBILLTYPEID\r\n                                inner join t_BD_SupplierBase D on D.FSUPPLIERID=B.FSUPPLIERID\r\n                                where B.F_KING_QZSTATUS in ('已创建','已完成') and A.F_KING_QZID='' and B.FID in (" + text2.TrimEnd(new char[]
							{
								','
							}) + ")";
							DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text3.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
							bool flag11 = dynamicObjectCollection.Count > 0;
							if (flag11)
							{
								bool flag12 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "PUR_SystemParameter", "F_KING_PASSWORD", 0L));
								if (flag12)
								{
									this.View.ShowMessage("请联系管理员维护密码！", 0);
								}
								else
								{
									string password = SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "PUR_SystemParameter", "F_KING_PASSWORD", 0L).ToString();
									List<string> list3 = new List<string>();
									StringBuilder stringBuilder3 = new StringBuilder();
									foreach (DynamicObject dynamicObject in dynamicObjectCollection)
									{
										bool flag13 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FSOCIALCRECODE"]);
										if (flag13)
										{
											bool flag14 = !list3.Contains(dynamicObject["FBILLNO"].ToString());
											if (flag14)
											{
												list3.Add(dynamicObject["FBILLNO"].ToString());
											}
										}
										else
										{
											stringBuilder3.Append(serviceHelper.CreateSubItem(base.Context, dynamicObject["FSOCIALCRECODE"].ToString(), dynamicObject["F_KING_QZBILLID"].ToString(), dynamicObject["F_KING_SUBID"].ToString(), dynamicObject["FFILEID"].ToString(), dynamicObject["FBILLNO"].ToString() + dynamicObject["FID"].ToString(), dynamicObject["FATTACHMENTNAME"].ToString(), password, dynamicObject["F_KING_ACCOUNT"].ToString(), dynamicObject["F_KING_NUMBER"].ToString()));
										}
									}
									bool flag15 = list3.Count > 0;
									if (flag15)
									{
										string text4 = string.Empty;
										for (int n = 0; n < list3.Count; n++)
										{
											text4 = text4 + list3[n] + "、";
										}
										text4 = text4.TrimEnd(new char[]
										{
											'、'
										});
										stringBuilder3.Append("存在部分单据的社会信用代码没有维护，具体单号如下：" + text4);
									}
									bool flag16 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder3);
									if (flag16)
									{
										this.View.ShowMessage(stringBuilder3.ToString(), 0);
									}
									else
									{
										this.View.ShowMessage("附属合同推送成功", 0);
									}
								}
							}
							else
							{
								this.View.ShowMessage("选择的数据没有需要推送附属合同，可能的原因如下：1.没有需要推送的附属合同；2.模板没有维护相关信息", 0);
							}
						}
					}
				}
			}
		}
	}
}
