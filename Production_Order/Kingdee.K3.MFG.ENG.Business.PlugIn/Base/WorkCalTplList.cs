using System;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000048 RID: 72
	public class WorkCalTplList : BaseControlList
	{
		// Token: 0x060004F7 RID: 1271 RVA: 0x0003CA7C File Offset: 0x0003AC7C
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
					if (MFGCommonUtil.IsOnlyQueryUser(base.Context))
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("用户“{0}”为仅查询许可用户，不能执行 “套用模板生成工作流日历” 操作，请联系系统管理员！", "015072000014789", 7, new object[0]), base.Context.UserName), "", 0);
						e.Cancel = true;
						return;
					}
					if (this.ValidateData())
					{
						if (!CalendarServiceHelper.IsWorkflowTemplate(base.Context, base.CurSelBillId))
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("请选择日历类型是工作流日历的模板，才能套用生成工作流日历！", "015072000033713", 7, new object[0]), "", 0);
							return;
						}
						CreateCalendarOption createCalendarOption = new CreateCalendarOption();
						createCalendarOption.OperationCode = 1;
						createCalendarOption.CalRuleId = base.CurSelBillId;
						createCalendarOption.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
						MFGBillUtil.ShowForm(this.View, "ENG_WorkCalRule_CreateCalendarWf", createCalendarOption, delegate(FormResult result)
						{
						}, 0);
					}
				}
				else
				{
					if (MFGCommonUtil.IsOnlyQueryUser(base.Context))
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("用户“{0}”为仅查询许可用户，不能执行 “套用” 操作，请联系系统管理员！", "015072030034927", 7, new object[0]), base.Context.UserName), "", 0);
						e.Cancel = true;
						return;
					}
					if (this.ValidateData())
					{
						CreateCalendarOption option = new CreateCalendarOption();
						option.OperationCode = 1;
						option.CalRuleId = base.CurSelBillId;
						option.WCFormId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("PLMWCFormId"));
						if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(option.WCFormId))
						{
							option.WCFormId = "ENG_WorkCal";
						}
						option.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
						MFGBillUtil.ShowForm(this.View, "ENG_WorkCalRule_CreateCalendar", option, delegate(FormResult result)
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
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0003CCC4 File Offset: 0x0003AEC4
		private bool ValidateData()
		{
			if (base.CurSelBillId == 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有可操作数据，请选中需要操作的数据行！", "015072000002202", 7, new object[0]), "", 0);
				return false;
			}
			if (base.CurSelBillDocStatus != 67)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("数据未审核，套用失败，请审核后再操作！", "015072000002195", 7, new object[0]), "", 0);
				return false;
			}
			if (base.CurSelBillForBidStatus != 65)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("数据被禁用，套用失败，请反禁用后再操作！", "015072000002203", 7, new object[0]), "", 0);
				return false;
			}
			return true;
		}
	}
}
