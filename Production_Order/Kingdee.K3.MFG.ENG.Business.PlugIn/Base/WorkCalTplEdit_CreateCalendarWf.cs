using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001C RID: 28
	public class WorkCalTplEdit_CreateCalendarWf : WorkCalTplEdit_CreateCalendar
	{
		// Token: 0x0600029B RID: 667 RVA: 0x0001EB30 File Offset: 0x0001CD30
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpper()) == null || !(a == "FOK"))
			{
				base.ButtonClick(e);
				return;
			}
			if (!base.ValidataData())
			{
				return;
			}
			this.calendarOption.OperationCode = base.FOperateCode;
			this.calendarOption.DefAddYears = base.DefAddYears;
			this.calendarOption.DateFrom = MFGBillUtil.GetValue<DateTime>(this.Model, "FDateFrom", -1, MFGServiceHelper.GetSysDate(base.Context), null);
			this.calendarOption.DateTo = MFGBillUtil.GetValue<DateTime>(this.Model, "FDateTo", -1, MFGServiceHelper.GetSysDate(base.Context).AddYears(1), null);
			this.calendarOption.CalIdList = base.WorkCalDataSelected;
			this.calendarOption.UseOrgId = MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null);
			IOperationResult operationResult;
			if (this.calendarOption.OperationCode == 1)
			{
				operationResult = CalendarServiceHelper.ApplyTemplateCreate(base.Context, this.calendarOption.CalRuleId, this.calendarOption.DateFrom, this.calendarOption.DateTo);
			}
			else if (this.calendarOption.OperationCode == 3)
			{
				operationResult = CalendarServiceHelper.ApplyTemplateExtend(base.Context, this.calendarOption.CalRuleId, this.calendarOption.CalIdList, this.calendarOption.DateTo);
			}
			else
			{
				operationResult = CalendarServiceHelper.ApplyTemplateUpdate(base.Context, this.calendarOption.CalRuleId, this.calendarOption.CalIdList);
			}
			if (operationResult != null && operationResult.OperateResult != null && operationResult.OperateResult.Count > 0)
			{
				if (operationResult.OperateResult[0].SuccessStatus)
				{
					this.View.ShowMessage(operationResult.OperateResult[0].Message, 0, delegate(MessageBoxResult re)
					{
						this.View.Close();
					}, "", 0);
					return;
				}
				this.View.ShowErrMessage(operationResult.OperateResult[0].Message, "", 0);
			}
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0001ED3C File Offset: 0x0001CF3C
		protected override void BindWorkCalData()
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FEntity");
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID")
			};
			OQLFilter oqlfilter = new OQLFilter();
			oqlfilter.Add(new OQLFilterHeadEntityItem
			{
				FilterString = string.Format("FCalendarTemplateId={0} AND FIsShared = '1' AND FForbidStatus <> 'B' AND FDocumentStatus ='C' ", this.calendarOption.CalRuleId)
			});
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, "WF_Calendar", list, oqlfilter);
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < array.Length; i++)
			{
				if (i >= entryRowCount)
				{
					this.Model.CreateNewEntryRow("FEntity");
				}
				this.Model.SetValue("FIsSelect", true, i);
				this.Model.SetItemValueByID("FWorkCalId", array[i]["Id"], i);
			}
			this.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}
	}
}
