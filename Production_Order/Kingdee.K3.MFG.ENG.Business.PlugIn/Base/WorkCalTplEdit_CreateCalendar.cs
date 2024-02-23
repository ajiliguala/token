using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001B RID: 27
	public class WorkCalTplEdit_CreateCalendar : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000287 RID: 647 RVA: 0x0001E0E6 File Offset: 0x0001C2E6
		protected int DefAddYears
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0001E0E9 File Offset: 0x0001C2E9
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.calendarOption = MFGBillUtil.GetParentFormSession<CreateCalendarOption>(this.View, "FormInputParam");
			if (this.calendarOption == null)
			{
				this.calendarOption = new CreateCalendarOption();
			}
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0001E11C File Offset: 0x0001C31C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.View.Model.SetValue("FDateTo", MFGServiceHelper.GetSysDate(base.Context).AddYears(this.DefAddYears));
			this.View.Model.SetValue("FUseOrgId", this.calendarOption.UseOrgId);
			MFGBillUtil.LockField(this.View, "FDateTo", false, -1);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0001E19A File Offset: 0x0001C39A
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetFieldVisible();
			this.SetLoclEntity(1);
			MFGBillUtil.LockField(this.View, "FDateTo", false, -1);
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0001E1E8 File Offset: 0x0001C3E8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FOK"))
				{
					if (!(a == "FCANCEL"))
					{
						return;
					}
					this.View.Close();
				}
				else if (this.VaildatePermission())
				{
					if (!this.ValidataData())
					{
						return;
					}
					List<DynamicObject> list = (from w in (DynamicObjectCollection)this.Model.DataObject["WorkCalData"]
					where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false)
					select DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(w, "WorkCalId", null)).ToList<DynamicObject>();
					IOperationResult operationResult = CalendarServiceHelper.CheckCreateCalendarShift(base.Context, this.FOperateCode, this.calendarOption.CalRuleId, list, MFGBillUtil.GetValue<DynamicObject>(this.View.Model, "FUseOrgId", -1, null, null));
					if (!operationResult.IsSuccess)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("{0}\t\n请检查班制的有效期或执行分配！", "015072000002197", 7, new object[0]), string.Join("\t\n", from w in operationResult.ValidationErrors
						select w.Message)), "", 0);
						return;
					}
					this.calendarOption.OperationCode = this.FOperateCode;
					this.calendarOption.DefAddYears = this.DefAddYears;
					this.calendarOption.DateFrom = MFGBillUtil.GetValue<DateTime>(this.Model, "FDateFrom", -1, MFGServiceHelper.GetSysDate(base.Context), null);
					this.calendarOption.DateTo = MFGBillUtil.GetValue<DateTime>(this.Model, "FDateTo", -1, MFGServiceHelper.GetSysDate(base.Context).AddYears(1), null);
					this.calendarOption.CalIdList = this.WorkCalDataSelected;
					this.calendarOption.UseOrgId = MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null);
					this.View.ReturnToParentWindow(this.calendarOption);
					this.View.Close();
					return;
				}
			}
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0001E414 File Offset: 0x0001C614
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string a;
			if ((a = e.Field.Key.ToUpper()) != null)
			{
				if (!(a == "FOPERATECODE"))
				{
					return;
				}
				int loclEntity = -1;
				int.TryParse(Convert.ToString(e.NewValue), out loclEntity);
				this.SetLoclEntity(loclEntity);
			}
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0001E468 File Offset: 0x0001C668
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToLowerInvariant()) != null)
			{
				if (!(a == "fuseorgid"))
				{
					return;
				}
				string text = this.calendarOption.WCFormId;
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					text = "ENG_WorkCal";
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
				OrgField orgField = formMetadata.BusinessInfo.GetField("FCreateOrgId") as OrgField;
				string createOrgFilter = OrganizationServiceHelper.GetCreateOrgFilter(formMetadata.BusinessInfo, base.Context, orgField, "fce8b1aca2144beeb3c6655eaf78bc34");
				e.ListFilterParameter.Filter = StringUtils.JoinFilterString(e.ListFilterParameter.Filter, createOrgFilter, "AND");
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600028E RID: 654 RVA: 0x0001E51A File Offset: 0x0001C71A
		protected int FOperateCode
		{
			get
			{
				return MFGBillUtil.GetValue<int>(this.Model, "FOperateCode", -1, 1, null);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600028F RID: 655 RVA: 0x0001E52F File Offset: 0x0001C72F
		protected List<long> WorkCalDataSelected
		{
			get
			{
				return this.GetWorkCalDataSelected();
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000290 RID: 656 RVA: 0x0001E538 File Offset: 0x0001C738
		private DateTime FDateTo
		{
			get
			{
				return MFGBillUtil.GetValue<DateTime>(this.Model, "FDateTo", -1, MFGServiceHelper.GetSysDate(base.Context).AddYears(1), null);
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0001E56C File Offset: 0x0001C76C
		protected bool ValidataData()
		{
			if (this.FOperateCode == 3 && KDTimeZone.MinSystemDateTime == MFGBillUtil.GetValue<DateTime>(this.Model, "FDateTo", -1, default(DateTime), null))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("输入延长关联日历的延长终止值", "015072000001900", 7, new object[0]), ResManager.LoadKDString("延长关联的终止日期不能为空", "015072000001903", 7, new object[0]), 0);
				return false;
			}
			if ((this.FOperateCode == 2 || this.FOperateCode == 3) && this.WorkCalDataSelected.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请勾选日历列表中需要修改的工作日历", "015072000001906", 7, new object[0]), ResManager.LoadKDString("待修改工作日历不能为空", "015072000001909", 7, new object[0]), 0);
				return false;
			}
			if (this.FOperateCode == 1)
			{
				if (MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null) == 0L)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("创建日历时请录入一个有日历创建权限的组织", "015072000002198", 7, new object[0]), ResManager.LoadKDString("创建日历时使用组织不能为空", "015072000002199", 7, new object[0]), 0);
					return false;
				}
				if (KDTimeZone.MinSystemDateTime == MFGBillUtil.GetValue<DateTime>(this.Model, "FDateFrom", -1, KDTimeZone.MinSystemDateTime, null))
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("创建日历时请录入开始日期", "015072000002200", 7, new object[0]), ResManager.LoadKDString("创建日历时开始日期不能为空", "015072000002201", 7, new object[0]), 0);
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0001E6FC File Offset: 0x0001C8FC
		private bool VaildatePermission()
		{
			string text = "f323992d896745fbaab4a2717c79ce2e";
			if (this.FOperateCode == 1)
			{
				text = "fce8b1aca2144beeb3c6655eaf78bc34";
			}
			string text2 = this.calendarOption.WCFormId;
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				text2 = "ENG_WorkCal";
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = text2,
				SubSystemId = this.View.Model.SubSytemId
			}, text);
			string a;
			if (!permissionAuthResult.Passed && (a = text) != null)
			{
				if (!(a == "fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					if (a == "f323992d896745fbaab4a2717c79ce2e")
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("需要配置 工作日历 的修改权限", "015072000001918", 7, new object[0]), ResManager.LoadKDString("没有修改权限", "015072000001921", 7, new object[0]), 0);
					}
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("需要配置 工作日历 的新增权限", "015072000001912", 7, new object[0]), ResManager.LoadKDString("没有新增权限", "015072000001915", 7, new object[0]), 0);
				}
			}
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x0001E814 File Offset: 0x0001CA14
		private void SetFieldVisible()
		{
			bool flag = this.calendarOption.WCFormId == "ENG_WorkCal";
			this.View.StyleManager.SetVisible("FWorkCalId", null, flag);
			this.View.StyleManager.SetVisible("FWorkCalName", null, flag);
			this.View.StyleManager.SetVisible("FWCNumber", null, !flag);
			this.View.StyleManager.SetVisible("FWCName", null, !flag);
		}

		// Token: 0x06000294 RID: 660 RVA: 0x0001E89C File Offset: 0x0001CA9C
		private void SetLoclEntity(int iOperCode)
		{
			bool flag = iOperCode != 1;
			this.View.StyleManager.SetEnabled("FWorkCalId", "", flag);
			this.View.StyleManager.SetEnabled("FIsSelect", "", flag);
			if (iOperCode != 1)
			{
				this.BindWorkCalData();
			}
			MFGBillUtil.LockField(this.View, "FDateTo", iOperCode == 3, -1);
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0001E908 File Offset: 0x0001CB08
		protected virtual void BindWorkCalData()
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FEntity");
			List<DynamicObject> list = new List<DynamicObject>();
			if (StringUtils.EqualsIgnoreCase(this.calendarOption.WCFormId, "ENG_WorkCal"))
			{
				list = CalendarServiceHelper.GetLinkWorkCalendars(base.Context, this.calendarOption.CalRuleId, false);
			}
			else
			{
				list = CalendarServiceHelper.GetLinkWorkCalendars(base.Context, this.calendarOption);
			}
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < list.Count; i++)
			{
				if (i >= entryRowCount)
				{
					this.Model.CreateNewEntryRow("FEntity");
				}
				this.Model.SetValue("FIsSelect", true, i);
				this.Model.SetItemValueByID("FWorkCalId", list[i]["Id"], i);
				if (!StringUtils.EqualsIgnoreCase(this.calendarOption.WCFormId, "ENG_WorkCal"))
				{
					this.Model.SetValue("FWCID", list[i]["Id"], i);
					this.Model.SetValue("FWCNumber", list[i]["Number"], i);
					this.Model.SetValue("FWCName", Convert.ToString(list[i]["Name"]), i);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0001EA8C File Offset: 0x0001CC8C
		private List<long> GetWorkCalDataSelected()
		{
			List<long> list = new List<long>();
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				long value = MFGBillUtil.GetValue<long>(this.Model, "FWorkCalId", i, 0L, null);
				if (value == 0L)
				{
					value = MFGBillUtil.GetValue<long>(this.Model, "FWCID", i, 0L, null);
				}
				if (value > 0L && MFGBillUtil.GetValue<bool>(this.Model, "FIsSelect", i, false, null))
				{
					list.Add(value);
				}
			}
			return list;
		}

		// Token: 0x0400013D RID: 317
		protected const string FIELDKEY_DATETO = "FDateTo";

		// Token: 0x0400013E RID: 318
		protected const string FIELDKEY_DATEFROM = "FDateFrom";

		// Token: 0x0400013F RID: 319
		private const string FIELDKEY_OPERATECODE = "FOperateCode";

		// Token: 0x04000140 RID: 320
		protected const string ENTITYKEY_DATA = "FEntity";

		// Token: 0x04000141 RID: 321
		protected const string FIELDKEY_ISSELECT = "FIsSelect";

		// Token: 0x04000142 RID: 322
		protected const string FIELDKEY_WORKCALID = "FWorkCalId";

		// Token: 0x04000143 RID: 323
		protected const string FIELDKEY_USEORGID = "FUseOrgId";

		// Token: 0x04000144 RID: 324
		private const string ORMKEY_ENTITYDATA = "WorkCalData";

		// Token: 0x04000145 RID: 325
		private const string ORMKEY_ISSELECT = "IsSelect";

		// Token: 0x04000146 RID: 326
		private const string ORMKEY_WORKCALID = "WorkCalId";

		// Token: 0x04000147 RID: 327
		protected CreateCalendarOption calendarOption = new CreateCalendarOption();
	}
}
