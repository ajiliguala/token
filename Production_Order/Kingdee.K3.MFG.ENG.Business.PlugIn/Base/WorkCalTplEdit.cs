using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000047 RID: 71
	public class WorkCalTplEdit : BaseControlEdit
	{
		// Token: 0x060004E6 RID: 1254 RVA: 0x0003B758 File Offset: 0x00039958
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			base.BeforeDeleteRow(e);
			int num = 0;
			if (int.TryParse(Convert.ToString(this.Model.GetValue("FRuleType", e.Row)), out num) && num == 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("系统内置规则数据不允许删除，删除失败！", "015072000002194", 7, new object[0]), 0, "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0003B8E4 File Offset: 0x00039AE4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBCREATECALENDAR"))
				{
					if (!(a == "TBCREATECALENDAR_WF"))
					{
						return;
					}
					if (base.BillId == 0L || base.BillStatus != 'C')
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("数据未审核，套用失败，请审核后再操作！", "015072000002195", 7, new object[0]), "", 0);
						return;
					}
					if (!CalendarServiceHelper.IsWorkflowTemplate(base.Context, base.BillId))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("请选择日历类型是工作流日历的模板，才能套用生成工作流日历！", "015072000033713", 7, new object[0]), "", 0);
						return;
					}
					CreateCalendarOption createCalendarOption = new CreateCalendarOption();
					createCalendarOption.OperationCode = 1;
					createCalendarOption.CalRuleId = base.BillId;
					createCalendarOption.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
					MFGBillUtil.ShowForm(base.View, "ENG_WorkCalRule_CreateCalendarWf", createCalendarOption, delegate(FormResult result)
					{
					}, 0);
				}
				else
				{
					if (base.BillId == 0L || base.BillStatus != 'C')
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("数据未审核，套用失败，请审核后再操作！", "015072000002195", 7, new object[0]), "", 0);
						return;
					}
					CreateCalendarOption option = new CreateCalendarOption();
					option.OperationCode = 1;
					option.CalRuleId = base.BillId;
					option.WCFormId = Convert.ToString(base.View.OpenParameter.GetCustomParameter("PLMWCFormId"));
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(option.WCFormId) && base.View.ParentFormView != null)
					{
						option.WCFormId = Convert.ToString(base.View.ParentFormView.OpenParameter.GetCustomParameter("PLMWCFormId"));
					}
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(option.WCFormId))
					{
						option.WCFormId = "ENG_WorkCal";
					}
					option.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
					MFGBillUtil.ShowForm(base.View, "ENG_WorkCalRule_CreateCalendar", option, delegate(FormResult result)
					{
						if (result.ReturnData is CreateCalendarOption)
						{
							option = (result.ReturnData as CreateCalendarOption);
							if (option.OperationCode == 1)
							{
								CalendarServiceHelper.CreateCalendar(this.Context, option);
								this.View.ShowMessage(ResManager.LoadKDString("创建操作完成", "015072000001861", 7, new object[0]), 0);
								return;
							}
							if (option.OperationCode == 2)
							{
								CalendarServiceHelper.UpdateCalendar(this.Context, option);
								this.View.ShowMessage(ResManager.LoadKDString("修改操作完成", "015072000001864", 7, new object[0]), 0);
								return;
							}
							if (option.OperationCode == 3)
							{
								CalendarServiceHelper.ExtendCalendar(this.Context, option);
								this.View.ShowMessage(ResManager.LoadKDString("延长操作完成", "015072000001867", 7, new object[0]), 0);
							}
						}
					}, 0);
					return;
				}
			}
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0003BB40 File Offset: 0x00039D40
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FRuleType"))
				{
					if (key == "FDayId")
					{
						e.Cancel = !this.IsDayMonthOk(false, e.Value, e.Row);
						return;
					}
					if (!(key == "FMonthId"))
					{
						return;
					}
					e.Cancel = !this.IsDayMonthOk(true, e.Value, e.Row);
				}
				else
				{
					int num = 0;
					int.TryParse(Convert.ToString(e.Value), out num);
					if (num == 1)
					{
						base.View.ShowMessage(ResManager.LoadKDString("按周类型的规则已内置，请不要再手工新增！", "015072000001876", 7, new object[0]), 0, "", 0);
						base.View.Model.SetValue("FRuleType", 9, e.Row);
						e.Cancel = true;
						return;
					}
				}
			}
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0003BC68 File Offset: 0x00039E68
		private bool IsDayMonthOk(bool isMonth, object newVal, int row)
		{
			if (newVal == null)
			{
				return true;
			}
			int[] array = new int[]
			{
				31,
				29,
				31,
				30,
				31,
				30,
				31,
				31,
				30,
				31,
				30,
				31
			};
			int num = 0;
			int num2 = 0;
			if (isMonth)
			{
				if (int.TryParse(newVal.ToString(), out num2))
				{
					num = MFGBillUtil.GetValue<int>(base.View.Model, "FDayId", row, 0, null);
				}
			}
			else if (int.TryParse(newVal.ToString(), out num))
			{
				num2 = MFGBillUtil.GetValue<int>(base.View.Model, "FMonthId", row, 0, null);
			}
			if (num2 > 0 && num2 <= 12 && num > 0 && num > array[num2 - 1])
			{
				base.View.Model.SetValue(isMonth ? "FMonthId" : "FDayId", null, row);
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("{0} 月份最多有 {1} 天，请重新选择月或日！", "015072000002196", 7, new object[0]), num2, array[num2 - 1]), "", 0);
				return false;
			}
			return true;
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0003BD5C File Offset: 0x00039F5C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.NewValue != null && e.NewValue.Equals(e.OldValue))
			{
				return;
			}
			int num = 0;
			if (e.Row >= 0)
			{
				num = MFGBillUtil.GetValue<int>(this.Model, "FRuleType", e.Row, 0, null);
			}
			TimeSpan t = default(TimeSpan);
			TimeSpan t2 = default(TimeSpan);
			DateTime dateTime = default(DateTime);
			DateTime dateTime2 = default(DateTime);
			DateTime dateTime3 = default(DateTime);
			DateTime dateTime4 = default(DateTime);
			string key;
			switch (key = e.Field.Key)
			{
			case "FPriorityId":
				if (num == 1)
				{
					this.SyncPriorityIdByRuleType(Convert.ToInt64(e.NewValue), (long)num);
				}
				this.CheckPriorityId(Convert.ToInt64(e.NewValue), e.Row, Convert.ToInt64(e.OldValue));
				return;
			case "FIsWorkTime":
			{
				bool value = MFGBillUtil.GetValue<bool>(this.Model, "FIsWorkTime", e.Row, true, null);
				MFGBillUtil.LockField(base.View, "FTeamTimeID", value, e.Row);
				if (!value)
				{
					base.View.Model.SetValue("FTeamTimeID", null, e.Row);
					return;
				}
				break;
			}
			case "FCALTYPE":
			{
				int value2 = MFGBillUtil.GetValue<int>(base.View.Model, "FCALTYPE", -1, 0, null);
				int entryRowCount = this.Model.GetEntryRowCount("FEntity");
				if (value2 == 2)
				{
					for (int i = 0; i < entryRowCount; i++)
					{
						base.View.Model.SetValue("FIsWorkTime", false, i);
						int value3 = MFGBillUtil.GetValue<int>(base.View.Model, "FRuleType", i, 0, null);
						if (value3 != 1)
						{
							base.View.Model.DeleteEntryRow("FEntity", i);
						}
					}
					return;
				}
				for (int j = 0; j < entryRowCount; j++)
				{
					int value4 = MFGBillUtil.GetValue<int>(base.View.Model, "FDateStyle", j, 0, null);
					if (value4 != 2 && value4 != 3)
					{
						base.View.Model.SetValue("FIsWorkTime", true, j);
					}
				}
				return;
			}
			case "FPeriodStart1":
			case "FPeriodEnd1":
			case "FPeriodStart2":
			case "FPeriodEnd2":
			{
				DateTime? dateTime5 = null;
				DateTime? dateTime6 = null;
				if (base.View.Model.GetValue("FPeriodStart1", e.Row) != null && base.View.Model.GetValue("FPeriodEnd1", e.Row) != null)
				{
					dateTime3 = Convert.ToDateTime(base.View.Model.GetValue("FPeriodStart1", e.Row).ToString());
					dateTime4 = Convert.ToDateTime(base.View.Model.GetValue("FPeriodEnd1", e.Row).ToString());
					if (dateTime4 <= dateTime3)
					{
						dateTime5 = new DateTime?(dateTime3);
						base.View.ShowErrMessage(ResManager.LoadKDString("上午结束时间必须大于上午开始时间", "015072000026693", 7, new object[0]), "", 0);
					}
					else
					{
						dateTime5 = new DateTime?(dateTime4);
						t = this.SetToday(dateTime4) - this.SetToday(dateTime3);
					}
				}
				if (base.View.Model.GetValue("FPeriodStart2", e.Row) != null && base.View.Model.GetValue("FPeriodEnd2", e.Row) != null)
				{
					dateTime = Convert.ToDateTime(base.View.Model.GetValue("FPeriodStart2", e.Row).ToString());
					dateTime2 = Convert.ToDateTime(base.View.Model.GetValue("FPeriodEnd2", e.Row).ToString());
					if (dateTime2 <= dateTime)
					{
						dateTime6 = new DateTime?(dateTime2);
						base.View.ShowErrMessage(ResManager.LoadKDString("下午结束时间必须大于下午开始时间", "015072000026694", 7, new object[0]), "", 0);
					}
					else
					{
						dateTime6 = new DateTime?(dateTime);
						t2 = this.SetToday(dateTime2) - this.SetToday(dateTime);
					}
				}
				if (base.View.Model.GetValue("FPeriodStart1", e.Row) != null && base.View.Model.GetValue("FPeriodEnd1", e.Row) == null)
				{
					dateTime5 = new DateTime?(Convert.ToDateTime(base.View.Model.GetValue("FPeriodStart1", e.Row).ToString()));
				}
				if (base.View.Model.GetValue("FPeriodStart1", e.Row) == null && base.View.Model.GetValue("FPeriodEnd1", e.Row) != null)
				{
					dateTime5 = new DateTime?(Convert.ToDateTime(base.View.Model.GetValue("FPeriodEnd1", e.Row).ToString()));
				}
				if (base.View.Model.GetValue("FPeriodStart2", e.Row) != null && base.View.Model.GetValue("FPeriodEnd2", e.Row) == null)
				{
					dateTime6 = new DateTime?(Convert.ToDateTime(base.View.Model.GetValue("FPeriodStart2", e.Row).ToString()));
				}
				if (base.View.Model.GetValue("FPeriodStart2", e.Row) == null && base.View.Model.GetValue("FPeriodEnd2", e.Row) != null)
				{
					dateTime6 = new DateTime?(Convert.ToDateTime(base.View.Model.GetValue("FPeriodEnd2", e.Row).ToString()));
				}
				if (dateTime5 != null && dateTime6 != null && dateTime5 >= dateTime6)
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("下午时间必须大于上午时间", "015072000026695", 7, new object[0]), "", 0);
				}
				base.View.Model.SetValue("FWorkHours", Convert.ToDecimal((t2 + t).TotalHours), e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0003C429 File Offset: 0x0003A629
		private DateTime SetToday(DateTime dt)
		{
			dt = Convert.ToDateTime(dt.ToString("HH:mm:ss"));
			return dt;
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0003C43F File Offset: 0x0003A63F
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.InitWeekRuleData();
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0003C450 File Offset: 0x0003A650
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			string key;
			if ((key = e.Entity.Key) != null)
			{
				if (!(key == "FEntity"))
				{
					return;
				}
				this.SetPriorityId(e.Row);
			}
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0003C490 File Offset: 0x0003A690
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FEntity"))
				{
					return;
				}
				this.SetPriorityId(e.Row);
			}
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0003C4C8 File Offset: 0x0003A6C8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["WorkCalRuleData"];
			int[] selectedRows = base.View.GetControl<EntryGrid>("FEntity").GetSelectedRows();
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "COPYENTRYROW"))
				{
					if (!(a == "INSERTENTRY"))
					{
						goto IL_14F;
					}
					if (selectedRows.Count<int>() <= 0 || selectedRows[0] > 6)
					{
						goto IL_14F;
					}
					e.Cancel = true;
					base.View.ShowMessage(ResManager.LoadKDString("内置规则内不得插入行！", "015072000001891", 7, new object[0]), 0);
				}
				else
				{
					if (dynamicObjectCollection == null || dynamicObjectCollection.Count<DynamicObject>() <= 0 || selectedRows == null || selectedRows.Count<int>() <= 0)
					{
						e.Cancel = true;
						base.View.ShowMessage(ResManager.LoadKDString("表体数据为空，不能进行复制行操作！", "015072000001885", 7, new object[0]), 0);
						return;
					}
					foreach (int index in selectedRows)
					{
						if (1 == DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObjectCollection[index], "RuleType", 0))
						{
							e.Cancel = true;
							base.View.ShowMessage(ResManager.LoadKDString("规则类型为周，不能进行复制行操作！", "015072000001888", 7, new object[0]), 0);
							return;
						}
					}
					goto IL_14F;
				}
				return;
			}
			IL_14F:
			base.BeforeDoOperation(e);
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0003C638 File Offset: 0x0003A838
		private void SetPriorityId(int iRow)
		{
			int value = MFGBillUtil.GetValue<int>(this.Model, "FRuleType", iRow, 0, null);
			if (value != 1)
			{
				DynamicObjectCollection source = (DynamicObjectCollection)this.Model.DataObject["WorkCalRuleData"];
				IEnumerable<int> source2 = from p in source.AsEnumerable<DynamicObject>()
				select new WorkCalRuleDataRowView(p).PriorityId;
				base.View.Model.SetValue("FPriorityId", source2.Max() + 1, iRow);
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0003C6C4 File Offset: 0x0003A8C4
		private void InitWeekRuleData()
		{
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["WorkCalRuleData"] as DynamicObjectCollection;
			if (dynamicObjectCollection != null)
			{
				if (dynamicObjectCollection.Count >= 7)
				{
					return;
				}
				for (int i = dynamicObjectCollection.Count; i < 7; i++)
				{
					this.Model.CreateNewEntryRow("FEntity");
				}
				for (int j = 0; j < 7; j++)
				{
					base.View.Model.SetValue("FPriorityId", 0, j);
					base.View.Model.SetValue("FRuleType", 1, j);
					base.View.Model.SetValue("FWeekId", (j == 0) ? 7 : j, j);
					if (j == 0 || j == 6)
					{
						base.View.Model.SetValue("FDateStyle", 2, j);
						base.View.Model.SetValue("FIsWorkTime", 0, j);
					}
				}
			}
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0003C7C8 File Offset: 0x0003A9C8
		private void SyncPriorityIdByRuleType(long iPriorityId, long iRuleType)
		{
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			this.Model.BeginIniti();
			for (int i = 0; i < entryRowCount; i++)
			{
				int num = Convert.ToInt32(this.Model.GetValue("FRuleType", i));
				if ((long)num == iRuleType)
				{
					base.View.Model.SetValue("FPriorityId", iPriorityId, i);
				}
			}
			this.Model.EndIniti();
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0003C840 File Offset: 0x0003AA40
		private bool CheckPriorityId(long iPriorityId, int iRow, long iOldPriorityId = 999L)
		{
			bool result = true;
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			int value = MFGBillUtil.GetValue<int>(this.Model, "FRuleType", iRow, 0, null);
			int.TryParse(Convert.ToString(this.Model.GetValue("FRuleType", iRow)), out value);
			for (int i = 0; i < entryRowCount; i++)
			{
				int value2 = MFGBillUtil.GetValue<int>(this.Model, "FPriorityId", i, 0, null);
				int value3 = MFGBillUtil.GetValue<int>(this.Model, "FRuleType", i, 0, null);
				if ((long)value2 == iPriorityId && i != iRow && value != 1 && (value != 1 || (value == 1 && value3 != 1)))
				{
					base.View.ShowMessage(string.Format(ResManager.LoadKDString("优先级编号【{0}】已被使用！", "015072000001894", 7, new object[0]), iPriorityId), 0, ResManager.LoadKDString("编号无效！", "015072000001897", 7, new object[0]), 0);
					base.View.Model.SetValue("FPriorityId", iOldPriorityId, iRow);
					result = false;
					break;
				}
			}
			return result;
		}

		// Token: 0x04000220 RID: 544
		private const int DAYCOUNT_WEEK = 7;

		// Token: 0x04000221 RID: 545
		private const string ORMENTITYKEY_RULEDATA = "WorkCalRuleData";

		// Token: 0x04000222 RID: 546
		private const string ORMENTITYKEY_RULETYPE = "RuleType";

		// Token: 0x04000223 RID: 547
		private const string ENTITYKEY_RULEDATA = "FEntity";

		// Token: 0x04000224 RID: 548
		private const string FIELDKEY_RULETYPE = "FRuleType";

		// Token: 0x04000225 RID: 549
		private const string FIELDKEY_PRIORITYID = "FPriorityId";

		// Token: 0x04000226 RID: 550
		private const string FIELDKEY_WEEKID = "FWeekId";

		// Token: 0x04000227 RID: 551
		private const string FIELDKEY_DAYID = "FDayId";

		// Token: 0x04000228 RID: 552
		private const string FIELDKEY_MONTHID = "FMonthId";

		// Token: 0x04000229 RID: 553
		private const string FIELDKEY_TEAMTIMEID = "FTeamTimeID";

		// Token: 0x0400022A RID: 554
		private const string FIELDKEY_DATESTYLE = "FDateStyle";

		// Token: 0x0400022B RID: 555
		private const string FIELDKEY_ISWORKTIME = "FIsWorkTime";

		// Token: 0x0400022C RID: 556
		private const string FIELDKEY_CALTYPE = "FCALTYPE";

		// Token: 0x0400022D RID: 557
		private const string FIELDKEY_WorkHours = "FWorkHours";

		// Token: 0x0400022E RID: 558
		private const string FIELDKEY_AmStart = "FPeriodStart1";

		// Token: 0x0400022F RID: 559
		private const string FIELDKEY_PmStart = "FPeriodStart2";

		// Token: 0x04000230 RID: 560
		private const string FIELDKEY_AmEnd = "FPeriodEnd1";

		// Token: 0x04000231 RID: 561
		private const string FIELDKEY_PmEnd = "FPeriodEnd2";
	}
}
