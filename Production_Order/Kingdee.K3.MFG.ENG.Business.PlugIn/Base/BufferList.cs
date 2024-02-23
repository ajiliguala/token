using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000010 RID: 16
	[Description("缓冲区列表插件")]
	[DisplayName("缓冲区")]
	public class BufferList : BaseControlList
	{
		// Token: 0x0600020F RID: 527 RVA: 0x00018B58 File Offset: 0x00016D58
		public override void PrepareFilterParameter(FilterArgs e)
		{
			if (!e.SortString.ToUpper().Contains("FUSEORGID.FNUMBER"))
			{
				e.AppendQueryOrderby("FUSEORGID.FNUMBER");
			}
			if (!e.SortString.ToUpper().Split(new char[]
			{
				','
			}).Contains("FNUMBER"))
			{
				e.AppendQueryOrderby("FNUMBER");
			}
			if (e.SelectedEntities.Any((FilterEntity o) => o.Key == "FEntity"))
			{
				e.AppendQueryOrderby("FLineRealationship");
				e.AppendQueryOrderby("FProductLineId.FNUMBER");
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x00018C00 File Offset: 0x00016E00
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "QueryMaterial"))
				{
					if (!(a == "MainPrdConsumeMaterial"))
					{
						if (!(a == "ChildPrdSupplyMaterial"))
						{
							return;
						}
						List<string> list = new List<string>();
						ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
						if (selectedRowsInfo.Count<ListSelectedRow>() <= 0)
						{
							this.View.ShowMessage(this.msg, 4);
							return;
						}
						foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
						{
							list.Add(listSelectedRow.EntryPrimaryKeyValue.ToString());
						}
						string text = string.Format("select t1.FNUMBER,t3.FNAME,t1.FUSEORGID,t2.FPRODUCTLINEID from T_ENG_Buffer t1 \r\n                                                            left join T_ENG_LineRelationship t2 on t1.FID=t2.FID \r\n                                                            left join T_ENG_Buffer_L t3 on t1.FID=t3.FID \r\n                                                            inner join Table(fn_StrSplit(@EntryId,',',1)) t4 on t4.FId=t2.FEntryID\r\n                                                        where t3.FLocaleID=@FLocaleID and t2.FLINEREALATIONSHIP='A'", new object[0]);
						DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, new SqlParam[]
						{
							new SqlParam("@EntryId", 161, list.ToArray()),
							new SqlParam("@FLocaleID", 11, base.Context.UserLocale.LCID)
						});
						int num = 0;
						DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
						dynamicFormShowParameter.FormId = "ENG_ChildPrdSupplyMaterial";
						dynamicFormShowParameter.ParentPageId = this.View.PageId;
						dynamicFormShowParameter.OpenStyle.ShowType = 6;
						foreach (DynamicObject value in dynamicObjectCollection)
						{
							dynamicFormShowParameter.CustomComplexParams.Add(num.ToString(), value);
							num++;
						}
						this.View.ShowForm(dynamicFormShowParameter);
					}
					else
					{
						List<string> list2 = new List<string>();
						ListSelectedRowCollection selectedRowsInfo2 = this.ListView.SelectedRowsInfo;
						if (selectedRowsInfo2.Count<ListSelectedRow>() <= 0)
						{
							this.View.ShowMessage(this.msg, 4);
							return;
						}
						foreach (ListSelectedRow listSelectedRow2 in selectedRowsInfo2)
						{
							list2.Add(listSelectedRow2.EntryPrimaryKeyValue.ToString());
						}
						string text2 = string.Format("select t1.FNUMBER,t3.FNAME,t1.FUSEORGID,t2.FPRODUCTLINEID from T_ENG_Buffer t1 \r\n                                                            left join T_ENG_LineRelationship t2 on t1.FID=t2.FID \r\n                                                            left join T_ENG_Buffer_L t3 on t1.FID=t3.FID \r\n                                                            inner join Table(fn_StrSplit(@EntryId,',',1)) t4 on t4.FId=t2.FEntryID\r\n                                                        where t2.FLINEREALATIONSHIP='B' and t3.FLocaleID=@FLocaleID", new object[0]);
						DynamicObjectCollection dynamicObjectCollection2 = DBServiceHelper.ExecuteDynamicObject(base.Context, text2, null, null, CommandType.Text, new SqlParam[]
						{
							new SqlParam("@EntryId", 161, list2.ToArray()),
							new SqlParam("@FLocaleID", 11, base.Context.UserLocale.LCID)
						});
						int num2 = 0;
						DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
						dynamicFormShowParameter2.FormId = "ENG_MainPrdConsumeMaterial";
						dynamicFormShowParameter2.ParentPageId = this.View.PageId;
						dynamicFormShowParameter2.OpenStyle.ShowType = 6;
						foreach (DynamicObject value2 in dynamicObjectCollection2)
						{
							dynamicFormShowParameter2.CustomComplexParams.Add(num2.ToString(), value2);
							num2++;
						}
						this.View.ShowForm(dynamicFormShowParameter2);
						return;
					}
				}
				else
				{
					List<string> list3 = new List<string>();
					ListSelectedRowCollection selectedRowsInfo3 = this.ListView.SelectedRowsInfo;
					if (selectedRowsInfo3.Count<ListSelectedRow>() <= 0)
					{
						this.View.ShowMessage(this.msg, 4);
						return;
					}
					foreach (ListSelectedRow listSelectedRow3 in selectedRowsInfo3)
					{
						list3.Add(listSelectedRow3.EntryPrimaryKeyValue.ToString());
					}
					string text3 = string.Format("select t1.FNUMBER,t3.FNAME,t1.FUSEORGID,t2.FPRODUCTLINEID \r\n                                                        from T_ENG_Buffer t1 \r\n                                                             left join T_ENG_LineRelationship t2 on t1.FID=t2.FID \r\n                                                             left join T_ENG_Buffer_L t3 on t1.FID=t3.FID\r\n                                                             inner join Table(fn_StrSplit(@EntryId,',',1)) t4 on t4.FId=t2.FEntryID \r\n                                                        where t3.FLocaleID=@LocaleId;", new object[0]);
					DynamicObjectCollection dynamicObjectCollection3 = DBServiceHelper.ExecuteDynamicObject(base.Context, text3, null, null, CommandType.Text, new SqlParam[]
					{
						new SqlParam("@EntryId", 161, list3.ToArray()),
						new SqlParam("@LocaleId", 11, base.Context.UserLocale.LCID)
					});
					int num3 = 0;
					DynamicFormShowParameter dynamicFormShowParameter3 = new DynamicFormShowParameter();
					dynamicFormShowParameter3.FormId = "ENG_QueryMaterialFromBuffer";
					dynamicFormShowParameter3.ParentPageId = this.View.PageId;
					dynamicFormShowParameter3.OpenStyle.ShowType = 6;
					foreach (DynamicObject value3 in dynamicObjectCollection3)
					{
						dynamicFormShowParameter3.CustomComplexParams.Add(num3.ToString(), value3);
						num3++;
					}
					this.View.ShowForm(dynamicFormShowParameter3);
					return;
				}
			}
		}

		// Token: 0x04000114 RID: 276
		private string msg = ResManager.LoadKDString("请至少选择一条缓冲区数据", "015072000018125", 7, new object[0]);
	}
}
