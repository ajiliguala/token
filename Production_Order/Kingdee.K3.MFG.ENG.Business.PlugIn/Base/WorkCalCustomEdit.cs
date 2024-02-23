using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.Calendar;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000040 RID: 64
	[Description("特性日历的插件")]
	public class WorkCalCustomEdit : BaseControlEdit
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000471 RID: 1137 RVA: 0x000374CB File Offset: 0x000356CB
		private string CalUserType
		{
			get
			{
				return MFGBillUtil.GetValue<string>(this.Model, "FCalUserType", -1, string.Empty, null);
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000472 RID: 1138 RVA: 0x000374E4 File Offset: 0x000356E4
		// (set) Token: 0x06000473 RID: 1139 RVA: 0x000374EC File Offset: 0x000356EC
		private protected AbstractBaseDataView CurSelNodeData { protected get; private set; }

		// Token: 0x06000474 RID: 1140 RVA: 0x000374F8 File Offset: 0x000356F8
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
				int num = Convert.ToInt32(sysDate.DayOfWeek);
				DateTime dateTime = sysDate.Date.AddDays((double)(-1 * ((num == 0) ? 6 : (num - 1))));
				int num2 = 0;
				foreach (DynamicObject dynamicObject in entityDataObject)
				{
					if (dateTime.CompareTo(DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "Day", default(DateTime))) <= 0)
					{
						base.View.SetEntityFocusRow("FEntity", num2);
						break;
					}
					num2++;
				}
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000475 RID: 1141 RVA: 0x000375F4 File Offset: 0x000357F4
		// (set) Token: 0x06000476 RID: 1142 RVA: 0x000375FC File Offset: 0x000357FC
		private protected AbstractBaseDataView CurSelOrgNodeData { protected get; private set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000477 RID: 1143 RVA: 0x00037605 File Offset: 0x00035805
		// (set) Token: 0x06000478 RID: 1144 RVA: 0x0003760D File Offset: 0x0003580D
		private protected AbstractBaseDataView CurSelDeptNodeData { protected get; private set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x00037616 File Offset: 0x00035816
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x0003761E File Offset: 0x0003581E
		private protected AbstractBaseDataView CurSelWorkCenterNodeData { protected get; private set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x00037627 File Offset: 0x00035827
		// (set) Token: 0x0600047C RID: 1148 RVA: 0x0003762F File Offset: 0x0003582F
		private protected AbstractBaseDataView CurSelResNodeData { protected get; private set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x00037638 File Offset: 0x00035838
		// (set) Token: 0x0600047E RID: 1150 RVA: 0x00037640 File Offset: 0x00035840
		private protected AbstractBaseDataView CurSelEquNodeData { protected get; private set; }

		// Token: 0x0600047F RID: 1151 RVA: 0x0003764C File Offset: 0x0003584C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE") && !(a == "REMOVEWORKCALMAP"))
				{
					return;
				}
				e.Option.SetVariableValue("NodeDataObject", this.nodeDataObject);
			}
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x000376AC File Offset: 0x000358AC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "REMOVEWORKCALMAP"))
				{
					return;
				}
				if (base.View.ParentFormView != null)
				{
					int num = Convert.ToInt32(this.nodeDataObject.NodeType);
					string text = this.nodeDataObject.Id.ToString() + "_" + num.ToString();
					(base.View.ParentFormView as IDynamicFormViewService).CustomEvents("ENG_WorkCalCustomTree", "AfterDoOperation", text);
					base.View.SendDynamicFormAction(base.View.ParentFormView);
				}
			}
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x00037764 File Offset: 0x00035964
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBGENOUTPUT"))
				{
					return;
				}
				this.DoGenOutput();
			}
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x0003779C File Offset: 0x0003599C
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.nodeDataObject = (MFGBillUtil.GetParentFormSession<object>(base.View, "FormInputParam") as AbstractBaseDataView);
			if (this.nodeDataObject != null)
			{
				this.InitCurSelNodeInfo(this.nodeDataObject);
				this.InitCustomCalInfo();
				this.HiddenOutputColumnAndBtn(this.nodeDataObject);
			}
			this.LockBill();
			base.SetAllMainBarItemVisible(false);
			this.HiddenResColumn();
			this.ChangeDataRowColor(-1);
			base.View.GetControl("FUnitID").Visible = false;
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x00037821 File Offset: 0x00035A21
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			base.SetAllMainBarItemVisible(false);
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00037831 File Offset: 0x00035A31
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x0003796C File Offset: 0x00035B6C
		public override void DataChanged(DataChangedEventArgs e)
		{
			WorkCalCustomEdit.<>c__DisplayClass1 CS$<>8__locals1 = new WorkCalCustomEdit.<>c__DisplayClass1();
			CS$<>8__locals1.e = e;
			CS$<>8__locals1.<>4__this = this;
			base.DataChanged(CS$<>8__locals1.e);
			string key;
			if ((key = CS$<>8__locals1.e.Field.Key) != null)
			{
				if (!(key == "FStdCalId"))
				{
					if (!(key == "FIsWorkTime"))
					{
						return;
					}
					this.SetDefShift((bool)CS$<>8__locals1.e.NewValue, CS$<>8__locals1.e.Row);
					this.ChangeDataRowColor(-1);
				}
				else
				{
					long lStdCalId = 0L;
					if (CS$<>8__locals1.e.OldValue != null && (CS$<>8__locals1.e.NewValue is DynamicObject || long.TryParse(Convert.ToString(CS$<>8__locals1.e.NewValue), out lStdCalId)))
					{
						base.View.ShowMessage(ResManager.LoadKDString("修改标准日历信息将会导致您之前的修改全部丢失，确认继续吗？", "015072000001849", 7, new object[0]), 4, delegate(MessageBoxResult result)
						{
							if (result == 6)
							{
								if (CS$<>8__locals1.e.NewValue is DynamicObject)
								{
									CS$<>8__locals1.<>4__this.FillEntryDataEntity(DataEntityExtend.GetDynamicObjectItemValue<long>(CS$<>8__locals1.e.NewValue as DynamicObject, "Id", 0L));
								}
								if (lStdCalId > 0L)
								{
									CS$<>8__locals1.<>4__this.FillEntryDataEntity(lStdCalId);
									return;
								}
							}
							else
							{
								CS$<>8__locals1.<>4__this.Model.BeginIniti();
								CS$<>8__locals1.<>4__this.View.Model.SetValue(CS$<>8__locals1.e.Field.Key, CS$<>8__locals1.e.OldValue, CS$<>8__locals1.e.Row);
								CS$<>8__locals1.<>4__this.Model.EndIniti();
								CS$<>8__locals1.<>4__this.View.UpdateView(CS$<>8__locals1.e.Field.Key);
							}
						}, "", 0);
						return;
					}
					if (CS$<>8__locals1.e.NewValue is DynamicObject)
					{
						this.FillEntryDataEntity(DataEntityExtend.GetDynamicObjectItemValue<long>(CS$<>8__locals1.e.NewValue as DynamicObject, "Id", 0L));
					}
					if (long.TryParse(Convert.ToString(CS$<>8__locals1.e.NewValue), out lStdCalId))
					{
						this.FillEntryDataEntity(lStdCalId);
						return;
					}
				}
			}
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x00037AE1 File Offset: 0x00035CE1
		public override void BeforeF1Click(F1ClickArgs e)
		{
			if (base.View.ParentFormView != null)
			{
				e.HelpContextId = base.View.ParentFormView.BillBusinessInfo.GetForm().HelpContextId;
			}
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x00037D38 File Offset: 0x00035F38
		private void FillEntryDataEntity(long lStdCalId)
		{
			IEnumerable<DynamicObject> nodeCustomCalendarData = CalendarServiceHelper.GetNodeCustomCalendarData(base.Context, this.CurSelNodeData, lStdCalId);
			IEnumerable<DynamicObject> enumerable = from w in nodeCustomCalendarData
			orderby DataEntityExtend.GetDynamicObjectItemValue<DateTime>(w, "Day", default(DateTime))
			select w;
			DynamicObject productLineInfo = null;
			if (this.nodeDataObject != null && (StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_FlowProductLine") || StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_RepetitiveProductLine")))
			{
				string formId = this.nodeDataObject.FormId;
				long id = this.nodeDataObject.Id;
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FId"));
				list.Add(new SelectorItemInfo("FDailyOutput"));
				list.Add(new SelectorItemInfo("FDailyOutputUnit"));
				string text = string.Format("FID = {0} AND FSCHEDULETYPE = 'B'", id);
				OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
				productLineInfo = BusinessDataServiceHelper.LoadFromCache(base.Context, formId, list, oqlfilter).FirstOrDefault<DynamicObject>();
			}
			DateTime currDate = DateTime.Today;
			this.Model.BeginIniti();
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (entryEntity != null)
			{
				DataEntityExtend.FromDataSource(entityDataObject, enumerable, true, delegate(CreateEntityRowCallbackParam param)
				{
					IDefaultValueCalculator service = this.View.GetService<IDefaultValueCalculator>();
					service.Initialize(this.View.Model);
					service.ApplyDefaultValue(entryEntity, param.TargetDataEntity, param.RowIndex, "Copy");
					if (this.nodeDataObject != null && StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_Resource"))
					{
						ResourceData resourceData = this.nodeDataObject as ResourceData;
						if (resourceData != null && (bool)param.SourceDataEntity["IsWorkTime"])
						{
							DynamicProperty dynamicProperty = null;
							if (param.SourceDataEntity.DynamicObjectType.Properties.TryGetValue("ResQty", ref dynamicProperty))
							{
								param.SourceDataEntity["ResQty"] = resourceData.ResCount;
							}
							else
							{
								service.Model.SetValue("FResQty", resourceData.ResCount, param.RowIndex);
							}
						}
					}
					if (productLineInfo != null && currDate.CompareTo((DateTime)param.SourceDataEntity["Day"]) <= 0 && (bool)param.SourceDataEntity["IsWorkTime"])
					{
						param.TargetDataEntity["UnitID_Id"] = productLineInfo["DailyOutputUnit_Id"];
						param.TargetDataEntity["UnitID"] = productLineInfo["DailyOutputUnit"];
						param.TargetDataEntity["QuotaDailyOutput"] = productLineInfo["DailyOutput"];
						param.TargetDataEntity["ActualDailyOutput"] = productLineInfo["DailyOutput"];
					}
				}, delegate(GetFieldValueCallbackParam param)
				{
					if (param.SourceProperty == null)
					{
						return param.TargetProperty.GetValue(param.TargetEntity);
					}
					return param.SourceProperty.GetValue(param.SourceDataEntity);
				}, null);
			}
			this.Model.EndIniti();
			base.View.UpdateView("FEntity");
			this.ChangeDataRowColor(-1);
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00037F00 File Offset: 0x00036100
		private void InitCurSelNodeInfo(AbstractBaseDataView curSelNodeData)
		{
			this.CurSelOrgNodeData = null;
			this.CurSelDeptNodeData = null;
			this.CurSelWorkCenterNodeData = null;
			this.CurSelResNodeData = null;
			this.CurSelEquNodeData = null;
			this.CurSelNodeData = curSelNodeData;
			switch (this.CurSelNodeData.ParentNodeType)
			{
			case 0:
				this.CurSelOrgNodeData = this.CurSelNodeData;
				return;
			case 1:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData;
				return;
			case 2:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData;
				return;
			case 3:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData.Parent;
				this.CurSelResNodeData = this.CurSelNodeData;
				return;
			case 4:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent.Parent.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelResNodeData = this.CurSelNodeData.Parent;
				this.CurSelEquNodeData = this.CurSelNodeData;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x0003807C File Offset: 0x0003627C
		private void InitCustomCalInfo()
		{
			if (base.View.OpenParameter.Status == null)
			{
				string text;
				switch (this.nodeDataObject.ParentNodeType)
				{
				case 1:
					text = "BD_Department";
					break;
				case 2:
					text = this.nodeDataObject.FormId;
					break;
				case 3:
					text = "ENG_Resource";
					break;
				case 4:
					text = "ENG_Equipment";
					break;
				default:
					text = "ORG_Organizations";
					break;
				}
				base.View.Model.SetValue("FCalUserType", text, -1);
				base.View.Model.SetValue("FCalUserId", this.nodeDataObject.Id, -1);
				base.View.Model.SetValue("FUseOrgId", this.CurSelOrgNodeData.Id, -1);
				base.View.Model.SetValue("FCreateOrgId", base.Context.CurrentOrganizationInfo.ID, -1);
			}
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00038184 File Offset: 0x00036384
		private void HiddenResColumn()
		{
			base.View.GetControl("FResQty").Visible = StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_Resource");
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x000381AC File Offset: 0x000363AC
		private void HiddenOutputColumnAndBtn(AbstractBaseDataView curSelNodeData)
		{
			bool visible = false;
			if (this.nodeDataObject != null && (StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_FlowProductLine") || StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_RepetitiveProductLine")))
			{
				string formId = this.nodeDataObject.FormId;
				long id = this.nodeDataObject.Id;
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FId"));
				list.Add(new SelectorItemInfo("FScheduleType"));
				string text = string.Format("FId = {0}", id);
				OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadFromCache(base.Context, formId, list, oqlfilter).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					visible = dynamicObject["ScheduleType"].ToString().Equals("B");
				}
			}
			base.View.GetControl("FQuotaDailyOutput").Visible = visible;
			base.View.GetControl("FActualDailyOutput").Visible = visible;
			base.View.GetBarItem("FEntity", "tbGenOutput").Visible = visible;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0003831C File Offset: 0x0003651C
		private void SetDefShift(bool isWordDay, int row)
		{
			if (!isWordDay)
			{
				return;
			}
			DateTime curRowDay = MFGBillUtil.GetValue<DateTime>(this.Model, "FDay", row, default(DateTime), null);
			DynamicObjectCollection firstEntityData = base.GetFirstEntityData();
			DynamicObject dynamicObject = (from w in firstEntityData
			where DataEntityExtend.GetDynamicObjectItemValue<DateTime>(w, "Day", default(DateTime)) < curRowDay && DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsWorkTime", false) && DataEntityExtend.GetDynamicObjectItemValue<long>(w, "ShiftId_Id", 0L) > 0L
			select w).LastOrDefault<DynamicObject>();
			if (dynamicObject != null)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ShiftId_Id", 0L);
				this.Model.SetItemValueByID("FShiftId", dynamicObjectItemValue, row);
			}
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0003839C File Offset: 0x0003659C
		private void ChangeDataRowColor(int row = -1)
		{
			int i = row;
			int num = i + 1;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			if (row == -1)
			{
				i = 0;
				num = this.Model.GetEntryRowCount("FEntity");
			}
			while (i < num)
			{
				bool value = MFGBillUtil.GetValue<bool>(this.Model, "FIsWorkTime", i, false, null);
				list.Add(new KeyValuePair<int, string>(i, (!value) ? "#CCCCCC" : ""));
				i++;
			}
			base.View.GetControl<EntryGrid>("FEntity").SetRowBackcolor(list);
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x0003841C File Offset: 0x0003661C
		private void LockBill()
		{
			this.EnabledParentBarItem("btn_view", base.View.OpenParameter.Status != 0);
			bool flag = base.View.OpenParameter.Status == 1;
			this.EnabledParentBarItem("btn_modify", flag);
			this.EnabledParentBarItem("btn_save", !flag);
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x00038479 File Offset: 0x00036679
		private void EnabledParentBarItem(string menuKey, bool isEnabled)
		{
			if (base.View.ParentFormView != null)
			{
				base.View.ParentFormView.GetMainBarItem(menuKey).Enabled = isEnabled;
				base.View.SendDynamicFormAction(base.View.ParentFormView);
			}
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x0003858C File Offset: 0x0003678C
		private void FShiftIdPrivate(int RowIndex)
		{
			long FShiftIDValue = MFGBillUtil.GetValue<long>(base.View.Model, "FSHIFTID", RowIndex, 0L, null);
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSHIFTID", RowIndex, null, null);
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "ENG_SHIFT";
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.MultiSelect = false;
			billShowParameter.Status = 0;
			WorkCalCustomEdit.T_EditShiftParam t_EditShiftParam = default(WorkCalCustomEdit.T_EditShiftParam);
			t_EditShiftParam.ShiftId = FShiftIDValue;
			t_EditShiftParam.SourceCaller = 1;
			t_EditShiftParam.UserOrgId = MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null);
			if (FShiftIDValue == 0L || !DataEntityExtend.GetDynamicObjectItemValue<bool>(value, "IsPrivate", false))
			{
				MFGBillUtil.ShowForm(base.View, "ENG_SHIFT", t_EditShiftParam, delegate(FormResult result)
				{
					if (result.ReturnData is WorkCalCustomEdit.T_EditShiftParam && ((WorkCalCustomEdit.T_EditShiftParam)result.ReturnData).ShiftId > 0L)
					{
						this.View.Model.SetValue("FSHIFTID", ((WorkCalCustomEdit.T_EditShiftParam)result.ReturnData).ShiftId, RowIndex);
					}
				}, 0);
				return;
			}
			billShowParameter.Status = 2;
			billShowParameter.FormId = "ENG_SHIFT";
			if (!FShiftIDValue.ToString().Equals("0"))
			{
				billShowParameter.PKey = FShiftIDValue.ToString();
				base.View.Session["FormInputParam"] = t_EditShiftParam;
			}
			base.View.ShowForm(billShowParameter, delegate(FormResult result)
			{
				if (FShiftIDValue != 0L)
				{
					this.View.Model.SetValue("FSHIFTID", "", RowIndex);
					this.View.Model.SetValue("FSHIFTID", FShiftIDValue, RowIndex);
				}
			});
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0003871C File Offset: 0x0003691C
		private void DoGenOutput()
		{
			DynamicObject dynamicObject = null;
			if (this.nodeDataObject != null && (StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_FlowProductLine") || StringUtils.EqualsIgnoreCase(this.CalUserType, "ENG_RepetitiveProductLine")))
			{
				string formId = this.nodeDataObject.FormId;
				long id = this.nodeDataObject.Id;
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FId"));
				list.Add(new SelectorItemInfo("FDailyOutput"));
				list.Add(new SelectorItemInfo("FDailyOutputUnit"));
				string text = string.Format("FID = {0} AND FSCHEDULETYPE = 'B'", id);
				OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
				dynamicObject = BusinessDataServiceHelper.LoadFromCache(base.Context, formId, list, oqlfilter).FirstOrDefault<DynamicObject>();
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (entityDataObject != null && entityDataObject.Count<DynamicObject>() > 1)
			{
				this.Model.BeginIniti();
				foreach (DynamicObject dynamicObject2 in entityDataObject)
				{
					if (dynamicObject != null)
					{
						dynamicObject2["UnitID_Id"] = dynamicObject["DailyOutputUnit_Id"];
						dynamicObject2["UnitID"] = dynamicObject["DailyOutputUnit"];
						if ((bool)dynamicObject2["IsWorkTime"])
						{
							dynamicObject2["QuotaDailyOutput"] = dynamicObject["DailyOutput"];
							dynamicObject2["ActualDailyOutput"] = dynamicObject["DailyOutput"];
						}
					}
				}
				this.Model.EndIniti();
				base.View.UpdateView("FEntity");
				this.Model.DataChanged = true;
			}
		}

		// Token: 0x040001EE RID: 494
		private AbstractBaseDataView nodeDataObject;

		// Token: 0x02000041 RID: 65
		public struct T_EditShiftParam
		{
			// Token: 0x040001F7 RID: 503
			public long ShiftId;

			// Token: 0x040001F8 RID: 504
			public int SourceCaller;

			// Token: 0x040001F9 RID: 505
			public long UserOrgId;
		}
	}
}
