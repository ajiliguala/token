using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.PreInsertData;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.JSON;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Serialization;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.MaterialTree;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000050 RID: 80
	public class BomQueryForward : AbstractDynamicFormPlugIn
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600059D RID: 1437 RVA: 0x00043796 File Offset: 0x00041996
		protected string MaterialTree_PageId
		{
			get
			{
				return this._materialTree_PageId;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x0004379E File Offset: 0x0004199E
		// (set) Token: 0x0600059F RID: 1439 RVA: 0x000437A8 File Offset: 0x000419A8
		public int curRow
		{
			get
			{
				return this._curRow;
			}
			set
			{
				this._curRow = value;
				bool flag = this._curRow + 1 < this.bomQueryChildItems.Count;
				ControlAppearance controlAppearance = this.View.LayoutInfo.GetControlAppearance("FBt_More");
				if (controlAppearance != null)
				{
					this.View.StyleManager.SetVisible(controlAppearance, "ShowMore", flag);
				}
				ControlAppearance controlAppearance2 = this.View.LayoutInfo.GetControlAppearance("FExpandAllRows");
				if (controlAppearance2 != null)
				{
					this.View.StyleManager.SetVisible(controlAppearance2, "ExpandAllRows", !flag);
				}
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060005A0 RID: 1440 RVA: 0x00043835 File Offset: 0x00041A35
		// (set) Token: 0x060005A1 RID: 1441 RVA: 0x0004383D File Offset: 0x00041A3D
		public object FMaterialId { get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x00043846 File Offset: 0x00041A46
		// (set) Token: 0x060005A3 RID: 1443 RVA: 0x0004384E File Offset: 0x00041A4E
		public object FUseOrgId { get; set; }

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x00043857 File Offset: 0x00041A57
		// (set) Token: 0x060005A5 RID: 1445 RVA: 0x0004385F File Offset: 0x00041A5F
		public List<DynamicObject> lstBomHeadData { get; set; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060005A6 RID: 1446 RVA: 0x00043868 File Offset: 0x00041A68
		// (set) Token: 0x060005A7 RID: 1447 RVA: 0x00043870 File Offset: 0x00041A70
		public List<DynamicObject> bomDataEntity { get; set; }

		// Token: 0x060005A8 RID: 1448 RVA: 0x0004387C File Offset: 0x00041A7C
		public BomQueryForward()
		{
			this._materialTree_PageId = SequentialGuid.NewGuid().ToString();
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x000438D4 File Offset: 0x00041AD4
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			DynamicFormOpenParameter paramter = e.Paramter;
			object customParameter = paramter.GetCustomParameter("FMaterialId");
			object customParameter2 = paramter.GetCustomParameter("FUseOrgId");
			this.currentFormId = this.View.BillBusinessInfo.GetForm().Id;
			if (customParameter != null)
			{
				this.FMaterialId = customParameter;
				this.FUseOrgId = customParameter2;
			}
			this.InitializeBomQueryOption();
			SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer1");
			splitContainer.HideFirstPanel(true);
			bool defaultcheme = this.GetDefaultcheme(this.GetBillName(), this.GetFilterName(), out this.filterParam);
			if (defaultcheme)
			{
				this.FillBomHeadDataBySC();
				splitContainer.HideFirstPanel(false);
				BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
			}
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x000439B4 File Offset: 0x00041BB4
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			bool value = MFGBillUtil.GetValue<bool>(this.Model, "FExpandStyle", -1, false, null);
			if (e.Row < 0)
			{
				return;
			}
			if (value && e.Key.ToUpperInvariant().Equals("FTopEntity".ToUpperInvariant()))
			{
				long bomId = MFGBillUtil.GetValue<long>(this.Model, "FBomId", e.Row, 0L, null);
				DynamicObject dynamicObject = (from o in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.View.Model.DataObject, "BomParent", null)
				where DataEntityExtend.GetDynamicObjectItemValue<long>(o, "BomId_Id", 0L) == bomId
				select o).FirstOrDefault<DynamicObject>();
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "BomId", null), "ParentAuxPropId_Id", 0L);
				this.View.Model.SetValue("FBomVersion", bomId);
				this.View.Model.BeginIniti();
				this.View.Model.SetValue("FBillMtrlAuxId", dynamicObjectItemValue);
				this.View.Model.EndIniti();
				this.View.UpdateView("FBillMtrlAuxId");
				this.FillBomChildData();
			}
			if (StringUtils.EqualsIgnoreCase(e.Key, "FBottomEntity") && StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryForward2"))
			{
				this.FillBomCOBYData();
			}
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00043B25 File Offset: 0x00041D25
		public override void Dispose()
		{
			base.Dispose();
			MFGCommonUtil.DoCommitNetworkCtrl(this.View.Context, this.networkCtrlResults);
			BomExpandServiceHelper.ClearBomExpandResult(base.Context, this.memBomExpandOption);
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x00043B54 File Offset: 0x00041D54
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (this.FMaterialId != null && this.FUseOrgId != null)
			{
				this.View.Model.SetValue("FBomUseOrgId", this.FUseOrgId);
				this.View.Model.SetValue("FBillMaterialId", this.FMaterialId);
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, Convert.ToInt64(this.FMaterialId));
				long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], Convert.ToInt64(this.FUseOrgId));
				if (hightVersionBomKey == 0L)
				{
					return;
				}
				this.View.Model.SetValue("FBomVersion", hightVersionBomKey);
				this.FillBomChildData();
			}
			if (this.View.BillBusinessInfo != null)
			{
				TabControl control = this.View.GetControl<TabControl>("FTabBottom");
				if (control != null)
				{
					control.SetFireSelChanged(true);
				}
			}
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x00043C34 File Offset: 0x00041E34
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			ControlAppearance controlAppearance = this.View.LayoutInfo.GetControlAppearance("FBt_More");
			if (controlAppearance != null)
			{
				this.View.StyleManager.SetVisible(controlAppearance, "ShowMore", false);
			}
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x00043CD8 File Offset: 0x00041ED8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBREFRESH")
				{
					this.FillBomChildData();
					int num = this.curTabindex;
					if (num == 1)
					{
						this.FillBomCOBYData();
					}
					e.Cancel = true;
					return;
				}
				if (a == "TBFILTER")
				{
					MFGBillUtil.ShowFilterForm(this.View, this.GetBillName(), null, delegate(FormResult filterResult)
					{
						if (filterResult.ReturnData is FilterParameter)
						{
							this.filterParam = (filterResult.ReturnData as FilterParameter);
							this.FillBomHeadData();
							SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer1");
							splitContainer.HideFirstPanel(false);
							BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
						}
					}, this.GetFilterName(), 0);
					e.Cancel = true;
					return;
				}
				if (!(a == "TBSCHEDULEDELETE"))
				{
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "MFG_PLNScheduleParam";
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.CustomParams.Add("MFG_ScheduleParams_Name", ResManager.LoadKDString("BOM展开历史数据清理", "015072000025078", 7, new object[0]));
				dynamicFormShowParameter.CustomParams.Add("MFG_ScheduleParams_Description", string.Empty);
				dynamicFormShowParameter.CustomParams.Add("MFG_ScheduleParams_PluginClass", "Kingdee.K3.MFG.ENG.App.Core.BacksageSchedule.BomExpResultDelSchedule,Kingdee.K3.MFG.ENG.App.Core");
				this.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x00043E24 File Offset: 0x00042024
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBEXPANDALLROWS"))
				{
					return;
				}
				TreeEntryGrid control = this.View.GetControl<TreeEntryGrid>("FBottomEntity");
				Entity entity = this.View.BusinessInfo.GetEntity("FBottomEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroupDatas = (from g in entityDataObject
					group g by DataEntityExtend.GetDynamicValue<string>(g, "EntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups = (from g in this.bomQueryChildItems
					group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FBottomEntity");
					DynamicObject entityDataObject2 = this.View.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
					string currEntryId = entityDataObject2["EntryId"].ToString();
					int num = 0;
					int userParam = MFGBillUtil.GetUserParam<int>(this.View, "ExpandLineQty", 2000);
					this.ExpandedAllRows(entryCurrentRowIndex, currEntryId, this.bomQueryChildItems, bomQueryForwardEntryGroups, bomQueryForwardEntryGroupDatas, control, entity, entityDataObject, userParam, ref num);
					this.View.SetEntityFocusRow("FBottomEntity", entryCurrentRowIndex);
					this.expandAllLimitSetColor(bomQueryForwardEntryGroups, this.View.Model.GetEntityDataObject(entity), control);
				}
			}
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x00043FD0 File Offset: 0x000421D0
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FRELOADDATA"))
				{
					if (!(a == "FREFRESH"))
					{
						return;
					}
					this.FillBomChildData();
				}
				else
				{
					this.FillBomChildData();
					int num = this.curTabindex;
					if (num != 1)
					{
						return;
					}
					this.FillBomCOBYData();
					return;
				}
			}
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x00044030 File Offset: 0x00042230
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FExpandStyle"))
				{
					if (!(key == "FBillMaterialId"))
					{
						if (key == "FBomUseOrgId")
						{
							this.View.Model.SetValue("FBillMaterialId", null);
							this.View.Model.SetValue("FBomVersion", null);
							FilterSchemeUtil.SetFilterDirty(this.View);
							return;
						}
						if (!(key == "FBillMtrlAuxId"))
						{
							return;
						}
						this.GetHightVersionBom();
					}
					else
					{
						if (e.NewValue == null)
						{
							this.View.Model.SetValue("FBomVersion", null);
							return;
						}
						long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, Convert.ToInt64(e.NewValue));
						long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
						long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], value);
						this.View.Model.SetValue("FBomVersion", hightVersionBomKey);
						return;
					}
				}
				else
				{
					EntryGrid control = this.View.GetControl<EntryGrid>("FTopEntity");
					if (control != null)
					{
						control.SetFireDoubleClickEvent(Convert.ToBoolean(e.NewValue));
						return;
					}
				}
			}
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00044174 File Offset: 0x00042374
		private void GetHightVersionBom()
		{
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(this.View.Model.DataObject, "BillMtrlAuxId_Id", 0L);
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBillMaterialId", -1, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
			long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, value);
			if (dynamicObjectItemValue <= 0L)
			{
				long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], value2);
				this.View.Model.SetValue("FBomVersion", hightVersionBomKey);
				this.View.UpdateView("FBomVersion");
				return;
			}
			List<Tuple<long, long, long>> list = new List<Tuple<long, long, long>>
			{
				new Tuple<long, long, long>(materialMasterAndUserOrgId[0], value2, dynamicObjectItemValue)
			};
			List<DynamicObject> list2 = BOMServiceHelper.GetHightVersionBom(base.Context, list).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				this.View.ShowMessage(ResManager.LoadKDString("该辅助属性下无对应BOM版本！", "015072000014052", 7, new object[0]), 0);
				return;
			}
			DynamicObject dynamicObject = list2.FirstOrDefault<DynamicObject>();
			this.View.Model.SetValue("FBomVersion", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L));
			this.View.UpdateView("FBomVersion");
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x000442BF File Offset: 0x000424BF
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FBillMtrlAuxId"))
			{
				this.GetHightVersionBom();
			}
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x000442F0 File Offset: 0x000424F0
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			if (e.BaseDataField.Key.Equals("FBomVersion"))
			{
				e.IsShowApproved = false;
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBillMaterialId", -1, 0L, null);
				if (value != 0L)
				{
					if (string.IsNullOrWhiteSpace(e.Filter))
					{
						e.Filter = string.Format(" FMATERIALID = {0} ", value);
						return;
					}
					e.Filter += string.Format(" AND FMATERIALID = {0} ", value);
				}
			}
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00044388 File Offset: 0x00042588
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.BaseDataField.Key.Equals("FBomVersion"))
			{
				e.IsShowApproved = false;
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBillMaterialId", -1, 0L, null);
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(this.View.Model.DataObject, "BillMtrlAuxId_Id", 0L);
				long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
				List<long> list = new List<long>();
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, value);
				if (value != 0L)
				{
					if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = string.Format(" FMATERIALID = {0} ", value);
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter += string.Format(" AND FMATERIALID = {0} ", value);
					}
					if (dynamicObjectItemValue > 0L)
					{
						list = BOMServiceHelper.GetApprovedBomIdByOrgId(this.View.Context, materialMasterAndUserOrgId[0], value2, dynamicObjectItemValue);
					}
					if (!ListUtils.IsEmpty<long>(list))
					{
						if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", list));
						}
						else
						{
							IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
							listFilterParameter2.Filter += string.Format(" AND FID IN ({0}) ", string.Join<long>(",", list));
						}
					}
				}
			}
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FBOMUSEORGID"))
				{
					return;
				}
				bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
				if (flag)
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00044558 File Offset: 0x00042758
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			this.curTabindex = e.TabIndex;
			TabControlAppearance tabControlAppearance = this.View.LayoutInfo.GetAppearance(e.Key) as TabControlAppearance;
			if (tabControlAppearance == null)
			{
				return;
			}
			TabPageAppearance tabPageAppearance = tabControlAppearance.TabPages.FirstOrDefault((TabPageAppearance o) => o.PageIndex == e.TabIndex);
			if (tabPageAppearance == null)
			{
				return;
			}
			string a;
			if ((a = tabPageAppearance.Key.ToUpper()) != null)
			{
				if (!(a == "FTABCOBY"))
				{
					return;
				}
				this.FillBomCOBYData();
			}
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00044614 File Offset: 0x00042814
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			base.BeforeEntityExport(e);
			List<DynamicObject> list = this.bomPrintChildItems;
			this._exportData = new List<DynamicObject>();
			if (list != null && list.Count > 0)
			{
				this.FormatDecimalScale(list);
				Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups = (from g in list
				group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
				Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from g in list
				group g by DataEntityExtend.GetDynamicValue<int>(g, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
				IGrouping<int, DynamicObject> grouping;
				if (dictionary.TryGetValue(0, out grouping))
				{
					bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FisShowNumber", -1, false, null);
					foreach (DynamicObject dynamicObject in grouping)
					{
						this._exportData.Add(dynamicObject);
						this.RebuildCollection(bomQueryForwardEntryGroups, dynamicObject, value);
					}
				}
			}
			List<DynamicObject> list2 = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in list)
			{
				if (DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "BomId_Id", 0L) != 0L)
				{
					list2.Add(dynamicObject2);
				}
			}
			List<DynamicObject> allBomCOBYDate = this.GetAllBomCOBYDate(list2);
			e.DataSource = new Dictionary<string, List<DynamicObject>>
			{
				{
					"FBottomEntity",
					this._exportData
				},
				{
					"FCobyEntity",
					allBomCOBYDate
				}
			};
			e.ExportEntityKeyList = new List<string>
			{
				"FBottomEntity",
				"FCobyEntity"
			};
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x00044810 File Offset: 0x00042A10
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有物料清单正查的{0}权限", "015072000019375", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x000448E4 File Offset: 0x00042AE4
		private void RebuildCollection(Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, DynamicObject currentDo, bool isShowNumber)
		{
			string key = Convert.ToString(currentDo["EntryId"]);
			IGrouping<string, DynamicObject> source;
			if (!bomQueryForwardEntryGroups.TryGetValue(key, out source))
			{
				return;
			}
			List<DynamicObject> list = source.ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				DynamicObject dynamicObject2 = (DynamicObject)OrmUtils.Clone(dynamicObject, false, false);
				if (isShowNumber)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplaceGroup", null);
					dynamicObject2["BomLevel"] = ((DataEntityExtend.GetDynamicValue<string>(currentDo, "BomLevel", null) == "0") ? "1" : DataEntityExtend.GetDynamicValue<string>(currentDo, "BomLevel", null)) + "." + dynamicValue;
				}
				else
				{
					dynamicObject2["BomLevel"] = this.GetTreeFormat(dynamicObject2);
				}
				this._exportData.Add(dynamicObject2);
				this.RebuildCollection(bomQueryForwardEntryGroups, dynamicObject2, isShowNumber);
			}
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x000449E8 File Offset: 0x00042BE8
		private int countNumber(string p)
		{
			int num = 0;
			foreach (char c in p)
			{
				if (c == '.')
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00044A1C File Offset: 0x00042C1C
		private void FormatDecimalScale(List<DynamicObject> dyCollectionLst)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FBottomEntity");
			List<Field> list = new List<Field>();
			foreach (Field field in entity.Fields)
			{
				if (field.GetType() == typeof(DecimalField))
				{
					list.Add(field);
				}
				if (field.GetType() == typeof(BasePropertyField))
				{
					BasePropertyField basePropertyField = (BasePropertyField)field;
					Field sourceField = basePropertyField.SourceField;
					if (sourceField != null && sourceField.GetType() == typeof(DecimalField))
					{
						list.Add(field);
					}
				}
			}
			if (!ListUtils.IsEmpty<Field>(list))
			{
				foreach (DynamicObject dynamicObject in dyCollectionLst)
				{
					foreach (Field field2 in list)
					{
						if (field2.GetType() == typeof(DecimalField))
						{
							int fieldScale = ((DecimalField)field2).FieldScale;
							string decimalFormatString = FieldFormatterUtil.GetDecimalFormatString(base.Context, (decimal)dynamicObject[field2.PropertyName], fieldScale);
							dynamicObject[field2.PropertyName] = decimalFormatString;
						}
						if (field2.GetType() == typeof(BasePropertyField))
						{
							BasePropertyField basePropertyField2 = (BasePropertyField)field2;
							BaseDataField baseDataField = (BaseDataField)basePropertyField2.ControlField;
							DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, baseDataField.PropertyName, null);
							if (!ObjectUtils.IsNullOrEmpty(dynamicValue))
							{
								Field sourceField2 = basePropertyField2.SourceField;
								int fieldScale2 = ((DecimalField)sourceField2).FieldScale;
								if (sourceField2.Entity is HeadEntity)
								{
									decimal dynamicValue2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicValue, sourceField2.PropertyName, 0m);
									string decimalFormatString2 = FieldFormatterUtil.GetDecimalFormatString(base.Context, dynamicValue2, fieldScale2);
									dynamicValue[sourceField2.PropertyName] = decimalFormatString2;
								}
								if (sourceField2.Entity is SubHeadEntity)
								{
									DynamicObject dynamicObject2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, sourceField2.Entity.EntryName, null).FirstOrDefault<DynamicObject>();
									decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, sourceField2.PropertyName, 0m);
									string decimalFormatString3 = FieldFormatterUtil.GetDecimalFormatString(base.Context, dynamicValue3, fieldScale2);
									dynamicObject2[sourceField2.PropertyName] = decimalFormatString3;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x00044CFC File Offset: 0x00042EFC
		protected void FillMaterialTree(List<object> showMaterials)
		{
			TreeParameters treeParameters = new TreeParameters();
			treeParameters.IsShowOrg = true;
			treeParameters.ShowMtrlLevel = 2;
			this.View.Session["1"] = treeParameters;
			this.View.Session["2"] = showMaterials;
			IDynamicFormView view = this.View.GetView(this.MaterialTree_PageId);
			if (view == null)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.OpenStyle.ShowType = 3;
				dynamicFormShowParameter.OpenStyle.TagetKey = "FTreePanel";
				dynamicFormShowParameter.FormId = "MFG_MaterialTree";
				dynamicFormShowParameter.PageId = this.MaterialTree_PageId;
				dynamicFormShowParameter.CustomParams.Add("ShowParam", "1");
				dynamicFormShowParameter.CustomParams.Add("ShowObject", "2");
				this.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			view.OpenParameter.SetCustomParameter("ShowParam", "1");
			view.OpenParameter.SetCustomParameter("ShowObject", "2");
			view.Refresh();
			this.View.SendDynamicFormAction(view);
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x00044E30 File Offset: 0x00043030
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			string a;
			if ((a = e.EventName.ToUpperInvariant()) != null)
			{
				if (!(a == "TREENODECLICK"))
				{
					return;
				}
				if (e.Key == "MFG_MaterialTree" && e.EventName == "TreeNodeClick")
				{
					List<DynamicObject> list = new List<DynamicObject>();
					string eventArgs = e.EventArgs;
					if (eventArgs.Contains("_"))
					{
						int num = eventArgs.IndexOf("_");
						eventArgs.Substring(0, num);
						eventArgs.Substring(num + 1, eventArgs.Length - num - 1);
					}
					else if (eventArgs.Contains("m"))
					{
						string mtrlId = eventArgs.Substring(1, eventArgs.Length - 1);
						list = (from p in this.lstBomHeadData
						where p["MaterialId_Id"].ToString().Equals(mtrlId)
						select p).ToList<DynamicObject>();
						long num2 = Convert.ToInt64(list.FirstOrDefault<DynamicObject>()["UseOrgId_Id"]);
						this.View.Model.SetValue("FBomUseOrgId", num2);
						this.View.Model.SetValue("FBillMaterialId", mtrlId);
					}
					this.FillBomDataByClick(list);
					BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
				}
			}
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00044F88 File Offset: 0x00043188
		private void BomForwardSearch(string searchContent, bool isSearchNext)
		{
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			TreeEntryGrid control = this.View.GetControl<TreeEntryGrid>("FBottomEntity");
			int focusRowIndex = control.GetFocusRowIndex();
			int num;
			if (isSearchNext)
			{
				if (focusRowIndex == entityDataObject.Count - 1)
				{
					this.View.ShowMessage(ResManager.LoadKDString("当前行已是末行", "015072000012052", 7, new object[0]), 0);
					return;
				}
				num = this.FindNextRowIndex(entityDataObject, searchContent, focusRowIndex + 1);
			}
			else
			{
				if (focusRowIndex == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("当前行已是首行", "015072000012053", 7, new object[0]), 0);
					return;
				}
				num = this.FindPrevRowIndex(entityDataObject, searchContent, focusRowIndex - 1);
			}
			if (num == -1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("未找到匹配结果", "015072000012054", 7, new object[0]), 0);
				return;
			}
			control.SetFocusRowIndex(num);
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x0004507C File Offset: 0x0004327C
		private int FindNextRowIndex(DynamicObjectCollection entityDatas, string searchContent, int start)
		{
			int result = -1;
			for (int i = start; i < entityDatas.Count; i++)
			{
				if (this.IsMatchedSearchContent(entityDatas[i], searchContent))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x000450B4 File Offset: 0x000432B4
		private int FindPrevRowIndex(DynamicObjectCollection entityDatas, string searchContent, int start)
		{
			int result = -1;
			for (int i = start; i >= 0; i--)
			{
				if (this.IsMatchedSearchContent(entityDatas[i], searchContent))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x000450E4 File Offset: 0x000432E4
		private bool IsMatchedSearchContent(DynamicObject row, string searchContent)
		{
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(row, "MaterialId", null);
			if (dynamicValue != null)
			{
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicValue, "Number", null);
				string text = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue, "Name", null).ToString();
				string text2 = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue, "Specification", null).ToString();
				return dynamicValue2.Contains(searchContent) || text.Contains(searchContent) || text2.Contains(searchContent);
			}
			return false;
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x000451AC File Offset: 0x000433AC
		public override void RowExpanding(EntityRowClickEventArgs e)
		{
			if (!this.IsOpenAttach)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity(e.Key);
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entity, e.Row);
			DynamicObjectCollection entityDataObject2 = this.View.Model.GetEntityDataObject(entity);
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from g in entityDataObject2
			group g by DataEntityExtend.GetDynamicValue<string>(g, "EntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			string curEntryId = entityDataObject["EntryId"].ToString();
			int num = Convert.ToInt32(entityDataObject["BomLevel"]);
			IEnumerable<DynamicObject> enumerable = this.FindChildRows(curEntryId);
			if (ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				return;
			}
			if (num == 0)
			{
				DynamicObject dynamicObject = entityDataObject2.SingleOrDefault((DynamicObject f) => DataEntityExtend.GetDynamicValue<string>(f, "ParentEntryId", null) == curEntryId && DataEntityExtend.GetDynamicValue<long>(f, "Id", 0L) == 0L && DataEntityExtend.GetDynamicValue<int>(f, "RowType", 0) == 64);
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject))
				{
					int num2 = entityDataObject2.IndexOf(dynamicObject);
					this.View.Model.DeleteEntryRow(e.Key, num2);
				}
			}
			foreach (DynamicObject dynamicObject2 in enumerable)
			{
				IGrouping<string, DynamicObject> grouping = null;
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "EntryId", null);
				if (!dictionary.TryGetValue(dynamicValue, out grouping))
				{
					this.View.Model.CreateNewEntryRow(entity, -1, dynamicObject2);
				}
			}
			this.SetColorByAddRows(entity, enumerable);
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x00045358 File Offset: 0x00043558
		private void SetColorByAddRows(Entity expandedTree, IEnumerable<DynamicObject> addChildRows)
		{
			if (ListUtils.IsEmpty<DynamicObject>(addChildRows))
			{
				return;
			}
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject[expandedTree.EntryName] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return;
			}
			foreach (DynamicObject dynamicObject in addChildRows)
			{
				int key = dynamicObjectCollection.IndexOf(dynamicObject);
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ErpClsID", null);
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
				if (("2".Equals(dynamicValue) || "3".Equals(dynamicValue) || "5".Equals(dynamicValue)) && dynamicValue2 == 0L)
				{
					list.Add(new KeyValuePair<int, string>(key, "#E6B8B7"));
				}
			}
			if (MFGBillUtil.GetValue<bool>(this.Model, "FNoneBomFlag", -1, false, null) && !ListUtils.IsEmpty<KeyValuePair<int, string>>(list))
			{
				EntryGrid control = this.View.GetControl<EntryGrid>(expandedTree.Key);
				control.SetRowBackcolor(list);
			}
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00045478 File Offset: 0x00043678
		private void InsertNewEntryRow(DynamicObject rowItem, Entity entry)
		{
			this.View.Model.CreateNewEntryRow(entry, -1, rowItem);
			if (StringUtils.EqualsIgnoreCase(this.currentFormId, "ENG_BomQueryForward2") && ENUM_ROWTYPEUtils.IsExpandRow(Convert.ToInt32(rowItem["RowType"])))
			{
				DynamicObject dynamicObject = rowItem.DynamicObjectType.CreateInstance() as DynamicObject;
				dynamicObject["RowType"] = 64;
				dynamicObject["EntryId"] = SequentialGuid.NewNativeGuid().ToString("N");
				dynamicObject["ParentEntryId"] = rowItem["EntryId"];
				this.View.Model.CreateNewEntryRow(entry, -1, dynamicObject);
			}
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00045554 File Offset: 0x00043754
		private List<DynamicObject> FindChildRows(string parentEntryId)
		{
			return (from o in this.bomQueryChildItems
			where o["ParentEntryId"].ToString().Equals(parentEntryId)
			select o).ToList<DynamicObject>();
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x0004558C File Offset: 0x0004378C
		protected void FillMtrlTree()
		{
			this.lstBomHeadData = this.GetBomHeadData();
			List<object> list = new List<object>();
			foreach (DynamicObject dynamicObject in this.lstBomHeadData)
			{
				list.Add(dynamicObject["MaterialId_Id"]);
			}
			this.FillMaterialTree(list);
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00045604 File Offset: 0x00043804
		protected void FillBomDataByClick(List<DynamicObject> chooseBom)
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, this.View.BillBusinessInfo.GetForm().Id);
			EntryEntity entryEntity = formMetaData.BusinessInfo.GetEntryEntity("FTopEntity");
			new DynamicObjectCollection(entryEntity.DynamicObjectType, null);
			DBServiceHelper.LoadReferenceObject(this.View.Context, chooseBom.ToArray(), entryEntity.DynamicObjectType, false);
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FTopEntity");
			this.Model.DeleteEntryData("FBottomEntity");
			if (this.View.BusinessInfo.GetElement("FCobyEntity") != null)
			{
				this.Model.DeleteEntryData("FCobyEntity");
			}
			Entity entryEntity2 = this.Model.BillBusinessInfo.GetEntryEntity("FTopEntity");
			if (chooseBom != null && chooseBom.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity2);
				foreach (DynamicObject dynamicObject in chooseBom)
				{
					dynamicObject["SEQ"] = chooseBom.IndexOf(dynamicObject) + 1;
					entityDataObject.Add(dynamicObject);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FTopEntity");
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00045768 File Offset: 0x00043968
		protected void FillBomHeadData()
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FTopEntity");
			this.Model.DeleteEntryData("FBottomEntity");
			if (this.View.BusinessInfo.GetElement("FCobyEntity") != null)
			{
				this.Model.DeleteEntryData("FCobyEntity");
			}
			this.FillMtrlTree();
			this.Model.EndIniti();
			this.View.UpdateView();
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000457E3 File Offset: 0x000439E3
		protected void FillBomHeadDataBySC()
		{
			this.FillMtrlTree();
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00045970 File Offset: 0x00043B70
		protected virtual void FillBomChildData()
		{
			this.UpdateBomQueryOption();
			this.Model.DeleteEntryData("FBottomEntity");
			if (StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryForward2"))
			{
				this.Model.DeleteEntryData("FCobyEntity");
				this.View.Model.SetValue("FSelectMaterialId", 0, -1);
			}
			int iForceRow = 0;
			List<DynamicObject> lstExpandSource = this.BuildBomExpandSourceData(iForceRow);
			if (ListUtils.IsEmpty<DynamicObject>(lstExpandSource))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当前物料对应的BOM不存在，请确认！", "015072000003341", 7, new object[0]), ResManager.LoadKDString("BOM不存在！", "015072000002208", 7, new object[0]), 0);
				return;
			}
			ViewUtils.ShowProcessForm(this.View, delegate(FormResult t)
			{
			}, true, ResManager.LoadKDString("正在查询数据，请稍候...", "015072000039150", 7, new object[0]));
			MainWorker.QuequeTask(delegate()
			{
				try
				{
					CultureInfoUtils.SetCurrentLanguage(this.Context);
					this.View.Session["ProcessRateValue"] = 10;
					this.bomQueryChildItems = this.GetBomChildData(lstExpandSource, this.memBomExpandOption);
					this.View.Session["ProcessRateValue"] = 60;
					bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FShowSerialNumber", -1, false, null);
					if (value)
					{
						this.GroupBomEntryWithParentEntryID(this.bomQueryChildItems);
					}
					this.bomPrintChildItems = this.bomQueryChildItems;
					if (StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryForward2"))
					{
						this.IsShowSubstituteMaterials();
					}
					this.BindChildEntitys();
					this.View.Session["ProcessRateValue"] = 100;
				}
				finally
				{
					this.View.Session["ProcessRateValue"] = 100;
				}
			}, delegate(AsynResult t)
			{
				this.RefreshEntityView();
			});
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00045A9F File Offset: 0x00043C9F
		protected virtual void RefreshEntityView()
		{
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00045ABC File Offset: 0x00043CBC
		protected void IsShowSubstituteMaterials()
		{
			if (ListUtils.IsEmpty<DynamicObject>(this.bomQueryChildItems))
			{
				return;
			}
			if (this.View.Model.DataObject["IsShowSubMtrl"].ToString().Equals("False"))
			{
				this.bomQueryChildItems = (from dym in this.bomQueryChildItems
				where !DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MaterialType", null).Equals("3")
				select dym).ToList<DynamicObject>();
				this.bomPrintChildItems = this.bomQueryChildItems;
			}
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00045C10 File Offset: 0x00043E10
		private List<DynamicObject> SortBindDatas(List<DynamicObject> ChildItems)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			List<int> list2 = (from dym in ChildItems
			where DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MATERIALTYPE", null).ToString().Equals("3")
			select DataEntityExtend.GetDynamicObjectItemValue<int>(dym, "ReplaceGroup", 0)).Distinct<int>().ToList<int>();
			if (ListUtils.IsEmpty<int>(list2))
			{
				return ChildItems;
			}
			IEnumerable<IGrouping<int, DynamicObject>> enumerable = from O in ChildItems
			group O by DataEntityExtend.GetDynamicObjectItemValue<int>(O, "ReplaceGroup", 0);
			foreach (IGrouping<int, DynamicObject> grouping in enumerable)
			{
				if (!list2.Contains(grouping.Key))
				{
					using (IEnumerator<DynamicObject> enumerator2 = grouping.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							DynamicObject item = enumerator2.Current;
							list.Add(item);
						}
						continue;
					}
				}
				DynamicObject dynamicObject = (from dym in grouping
				where !DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MATERIALTYPE", null).ToString().Equals("3") && StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "IsSubsKeyItem", null).ToString(), "True")
				select dym).ToList<DynamicObject>().FirstOrDefault<DynamicObject>();
				list.Add(dynamicObject);
				int num = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0);
				List<DynamicObject> list3 = (from dym in grouping
				where DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MATERIALTYPE", null).ToString().Equals("3")
				select dym).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject2 in list3)
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Seq", ++num);
				}
				list.AddRange(list3);
				List<DynamicObject> list4 = (from dym in grouping
				where !DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MATERIALTYPE", null).ToString().Equals("3") && StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "IsSubsKeyItem", null).ToString(), "false")
				select dym).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject3 in list4)
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Seq", ++num);
				}
				list.AddRange(list4);
			}
			return list;
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00045EC8 File Offset: 0x000440C8
		protected void BindChildEntitys()
		{
			IEnumerable<DynamicObject> enumerable = this.bomQueryChildItems;
			if (!StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryIntegration"))
			{
				if (this.bomQueryChildItems.Count > 1000)
				{
					this.IsOpenAttach = true;
					enumerable = from o in this.bomQueryChildItems
					where o["BomLevel"].ToString().Equals("0")
					select o;
				}
				else
				{
					this.IsOpenAttach = false;
				}
			}
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			this.Model.BeginIniti();
			if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				int num = 0;
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				foreach (DynamicObject dynamicObject in enumerable)
				{
					entityDataObject.Add(dynamicObject);
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ErpClsID", null);
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
					if (("2".Equals(dynamicValue) || "3".Equals(dynamicValue) || "5".Equals(dynamicValue)) && dynamicValue2 == 0L)
					{
						list.Add(new KeyValuePair<int, string>(num, "#E6B8B7"));
					}
					num++;
					if (StringUtils.EqualsIgnoreCase(this.currentFormId, "ENG_BomQueryForward2") && ENUM_ROWTYPEUtils.IsExpandRow(Convert.ToInt32(dynamicObject["RowType"])) && this.IsOpenAttach)
					{
						DynamicObject dynamicObject2 = new DynamicObject(entityDataObject.DynamicCollectionItemPropertyType);
						dynamicObject2["RowType"] = 64;
						dynamicObject2["EntryId"] = SequentialGuid.NewNativeGuid().ToString("N");
						dynamicObject2["ParentEntryId"] = dynamicObject["EntryId"];
						dynamicObject2["Seq"] = 2;
						entityDataObject.Add(dynamicObject2);
						num++;
					}
				}
				if (MFGBillUtil.GetValue<bool>(this.Model, "FNoneBomFlag", -1, false, null) && !ListUtils.IsEmpty<KeyValuePair<int, string>>(list))
				{
					EntryGrid control = this.View.GetControl<EntryGrid>("FBottomEntity");
					control.SetRowBackcolor(list);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
			this.View.SetEntityFocusRow("FBottomEntity", 0);
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00046154 File Offset: 0x00044354
		private List<DynamicObject> GetAllBomCOBYDate(List<DynamicObject> mtrls)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(mtrls))
			{
				return list;
			}
			foreach (DynamicObject dynamicObject in mtrls)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
				decimal dynamicValue2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Qty", 0m);
				List<DynamicObject> bomCOBYData = this.GetBomCOBYData(dynamicValue);
				if (!ListUtils.IsEmpty<DynamicObject>(bomCOBYData))
				{
					foreach (DynamicObject dynamicObject2 in bomCOBYData)
					{
						dynamicObject2["SEQ"] = bomCOBYData.IndexOf(dynamicObject2) + 1;
						dynamicObject2["Qty"] = Convert.ToDecimal(dynamicObject2["Qty"]) * dynamicValue2;
						list.Add(dynamicObject2);
					}
				}
			}
			return list;
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x0004626C File Offset: 0x0004446C
		protected void FillBomCOBYData()
		{
			this.Model.DeleteEntryData("FCobyEntity");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FBottomEntity");
			if (entryCurrentRowIndex < 0)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
			this.View.Model.SetValue("FSelectMaterialId", DataEntityExtend.GetDynamicObjectItemValue<long>(entityDataObject, "MaterialId_Id", 0L), -1);
			this.Model.BeginIniti();
			long value = MFGBillUtil.GetValue<long>(this.Model, "FBomId2", entryCurrentRowIndex, 0L, null);
			decimal value2 = MFGBillUtil.GetValue<decimal>(this.Model, "FQty2", entryCurrentRowIndex, 1m, null);
			this.bomDataEntity = this.GetBomCOBYData(value);
			EntryEntity entryEntity2 = this.Model.BillBusinessInfo.GetEntryEntity("FCobyEntity");
			if (this.bomDataEntity != null && entryEntity2 != null)
			{
				DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entryEntity2);
				foreach (DynamicObject dynamicObject in this.bomDataEntity)
				{
					dynamicObject["SEQ"] = this.bomDataEntity.IndexOf(dynamicObject) + 1;
					dynamicObject["Qty"] = Convert.ToDecimal(dynamicObject["Qty"]) * value2;
					entityDataObject2.Add(dynamicObject);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FCobyEntity");
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00046420 File Offset: 0x00044620
		protected virtual void InitializeBomQueryOption()
		{
			MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			this.memBomExpandOption = new MemBomExpandOption_ForPSV();
			this.memBomExpandOption.ExpandLevelTo = 0;
			this.memBomExpandOption.ExpandVirtualMaterial = false;
			this.memBomExpandOption.DeleteVirtualMaterial = false;
			this.memBomExpandOption.DeleteSkipRow = false;
			this.memBomExpandOption.ExpandSkipRow = false;
			this.memBomExpandOption.IsShowOutSource = false;
			this.memBomExpandOption.BomExpandId = SequentialGuid.NewGuid().ToString();
			this.memBomExpandOption.ParentCsdYieldRate = true;
			this.memBomExpandOption.ChildCsdYieldRate = true;
			this.memBomExpandOption.CsdSubstitution = true;
			this.memBomExpandOption.Option.SetVariableValue("requireDataPermission", true);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x000464F0 File Offset: 0x000446F0
		protected virtual void UpdateBomQueryOption()
		{
			this.memBomExpandOption.ExpandLevelTo = MFGBillUtil.GetValue<int>(this.Model, "FExpandLevel", -1, 0, null);
			DateTime? dateTime = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, MFGServiceHelper.GetSysDate(base.Context), null));
			if (dateTime != null && dateTime >= new DateTime(1900, 1, 1))
			{
				this.memBomExpandOption.ValidDate = new DateTime?(dateTime.Value);
			}
			this.memBomExpandOption.BomExpandCalType = 0;
			this.memBomExpandOption.CsdSubstitution = !(this.View.Model.DataObject["IsShowSubMtrl"].ToString() == "False");
			this.memBomExpandOption.IsExpandSubMtrl = !(this.View.Model.DataObject["IsExpandSubMtrl"].ToString() == "False");
			this.memBomExpandOption.IsHideOutSourceBOM = !(this.View.Model.DataObject["IsHideOutSourceBOM"].ToString() == "False");
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x0004666C File Offset: 0x0004486C
		protected virtual List<DynamicObject> BuildBomExpandSourceData(int iForceRow)
		{
			DateTime value = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FValidDate", -1, default(DateTime), null);
			DynamicObjectCollection value2 = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FBomVersion", -1, null, null);
			if (ListUtils.IsEmpty<DynamicObject>(value2))
			{
				return null;
			}
			List<DynamicObject> list = new List<DynamicObject>();
			List<long> source = (from bomObj in value2
			select OtherExtend.ConvertTo<long>(bomObj["BomVersion_Id"], 0L)).ToList<long>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FID",
					"FMaterialId.FMASTERID as MaterialMasterId",
					"FMATERIALID",
					"FUseOrgId",
					"FUNITID",
					"FBaseUnitId",
					"FParentAuxPropId"
				})
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				TableName = "table(fn_StrSplit(@ids,',',1))",
				TableNameAs = "ts",
				ScourceKey = "FID"
			});
			List<SqlParam> list2 = new List<SqlParam>
			{
				new SqlParam("@ids", 161, source.Distinct<long>().ToArray<long>())
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list2);
			Dictionary<long, DynamicObject> dictionary = dynamicObjectCollection.ToDictionary((DynamicObject x) => OtherExtend.ConvertTo<long>(x["FID"], 0L));
			List<GetUnitConvertRateArgs> list3 = new List<GetUnitConvertRateArgs>();
			foreach (DynamicObject dynamicObject in value2)
			{
				long key = Convert.ToInt64(dynamicObject["BomVersion_Id"]);
				DynamicObject dynamicObject2 = null;
				if (dictionary.TryGetValue(key, out dynamicObject2))
				{
					long materialId = OtherExtend.ConvertTo<long>(dynamicObject2["FMATERIALID"], 0L);
					long masterId = OtherExtend.ConvertTo<long>(dynamicObject2["MaterialMasterId"], 0L);
					long sourceUnitId = Convert.ToInt64(dynamicObject2["FUNITID"]);
					long destUnitId = Convert.ToInt64(dynamicObject2["FBaseUnitId"]);
					Convert.ToInt64(dynamicObject2["FUseOrgId"]);
					list3.Add(new GetUnitConvertRateArgs
					{
						PrimaryKey = (long)value2.IndexOf(dynamicObject),
						MaterialId = materialId,
						MasterId = masterId,
						SourceUnitId = sourceUnitId,
						DestUnitId = destUnitId
					});
				}
			}
			Dictionary<long, UnitConvert> unitConvertRateList = UnitConvertServiceHelper.GetUnitConvertRateList(base.Context, list3);
			foreach (DynamicObject dynamicObject3 in value2)
			{
				long num = Convert.ToInt64(dynamicObject3["BomVersion_Id"]);
				DynamicObject dynamicObject4 = null;
				if (dictionary.TryGetValue(num, out dynamicObject4))
				{
					long materialId_Id = OtherExtend.ConvertTo<long>(dynamicObject4["FMATERIALID"], 0L);
					long unitId_Id = Convert.ToInt64(dynamicObject4["FUNITID"]);
					long baseUnitId_Id = Convert.ToInt64(dynamicObject4["FBaseUnitId"]);
					long supplyOrgId_Id = Convert.ToInt64(dynamicObject4["FUseOrgId"]);
					long auxPropId = Convert.ToInt64(dynamicObject4["FParentAuxPropId"]);
					BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
					decimal num2 = MFGBillUtil.GetValue<decimal>(this.Model, "FQty", -1, 1m, null);
					UnitConvert unitConvert = null;
					if (unitConvertRateList.TryGetValue((long)value2.IndexOf(dynamicObject3), out unitConvert))
					{
						num2 = unitConvert.ConvertQty(num2, "");
					}
					bomForwardSourceDynamicRow.MaterialId_Id = materialId_Id;
					bomForwardSourceDynamicRow.BomId_Id = num;
					bomForwardSourceDynamicRow.NeedQty = num2;
					bomForwardSourceDynamicRow.NeedDate = new DateTime?(value);
					bomForwardSourceDynamicRow.UnitId_Id = unitId_Id;
					bomForwardSourceDynamicRow.BaseUnitId_Id = baseUnitId_Id;
					bomForwardSourceDynamicRow.SupplyOrgId_Id = supplyOrgId_Id;
					bomForwardSourceDynamicRow.AuxPropId = auxPropId;
					bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
					list.Add(bomForwardSourceDynamicRow.DataEntity);
				}
			}
			return list;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x00046AB4 File Offset: 0x00044CB4
		protected virtual List<DynamicObject> GetBomHeadData()
		{
			return BomQueryServiceHelper.GetBomQueryItemsForUnForbid(base.Context, this.filterParam, this.View.BillBusinessInfo.GetForm().Id);
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00046ADC File Offset: 0x00044CDC
		protected virtual List<DynamicObject> GetBomChildData(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			memBomExpandOption.IsConvertUnitQty = true;
			List<DynamicObject> bomQueryForwardResult = BomQueryServiceHelper.GetBomQueryForwardResult(base.Context, lstExpandSource, memBomExpandOption);
			this.SetTopMaterialId(bomQueryForwardResult);
			return bomQueryForwardResult;
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00046B08 File Offset: 0x00044D08
		private void SetIsExistCoby(List<DynamicObject> queryChildData)
		{
			if (ListUtils.IsEmpty<DynamicObject>(queryChildData))
			{
				return;
			}
			foreach (DynamicObject dynamicObject in queryChildData)
			{
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				if (!ObjectUtils.IsNullOrEmpty(dynamicValue))
				{
					DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "EntryBOMCOBY", null);
					if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "IsExistCoby", true);
					}
				}
			}
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x00046B94 File Offset: 0x00044D94
		protected virtual List<DynamicObject> GetBomCOBYData(long lBomId)
		{
			return BomQueryServiceHelper.GetBomCobyDataForQuery(base.Context, new List<long>
			{
				lBomId
			}, false);
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00046BBB File Offset: 0x00044DBB
		protected virtual string GetBillName()
		{
			return "ENG_BomQueryForward2";
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x00046BC2 File Offset: 0x00044DC2
		protected virtual string GetFilterName()
		{
			return "ENG_BomQueryForward_Filter";
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00046BCC File Offset: 0x00044DCC
		private void ShowBomSearchView()
		{
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FBottomEntity");
			if (this.View.Model.GetEntityDataObject(entryEntity) == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有可进行查找的分录行，请先录入子项物料！", "015072000012051", 7, new object[0]), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_BOMSearchRow";
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00046C50 File Offset: 0x00044E50
		private void DoStartBomNetworkCtrl()
		{
			if (this.networkCtrlResults == null)
			{
				this.networkCtrlResults = new List<NetworkCtrlResult>();
			}
			Dictionary<object, string> dictionary = new Dictionary<object, string>();
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FBillBomId", -1, null, null);
			if (value != null)
			{
				dictionary.Add(DataEntityExtend.GetDynamicObjectItemValue<object>(value, "Id", null), DataEntityExtend.GetDynamicObjectItemValue<string>(value, "Number", null));
			}
			foreach (DynamicObject dynamicObject in this.bomPrintChildItems)
			{
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "BomId", null);
				if (dynamicObjectItemValue != null)
				{
					object dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<object>(dynamicObjectItemValue, "Id", null);
					if (!dictionary.ContainsKey(dynamicObjectItemValue2))
					{
						dictionary.Add(dynamicObjectItemValue2, DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue, "Number", null));
					}
				}
			}
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			List<NetworkCtrlResult> list = MFGCommonUtil.DoStartNetworkCtrl(this.View.Context, formMetaData.BusinessInfo, dictionary);
			if (!ListUtils.IsEmpty<NetworkCtrlResult>(list))
			{
				this.networkCtrlResults.AddRange(list);
			}
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x00046D6C File Offset: 0x00044F6C
		private void ExpandedAllRows(int currRowIndex, string currEntryId, List<DynamicObject> bomQueryForwardEntrys, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroupDatas, TreeEntryGrid treeEntryGrid, Entity entry, DynamicObjectCollection bomQueryEntryDatas, int maxLineLimit, ref int maxCount)
		{
			maxCount++;
			if (maxCount > maxLineLimit)
			{
				return;
			}
			IGrouping<string, DynamicObject> source = null;
			if (!bomQueryForwardEntryGroups.TryGetValue(currEntryId, out source))
			{
				return;
			}
			IEnumerable<DynamicObject> enumerable = source.ToList<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				treeEntryGrid.ExpandedRow(currRowIndex);
				foreach (DynamicObject dynamicObject in enumerable)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					if (!bomQueryForwardEntryGroupDatas.TryGetValue(dynamicValue, out source))
					{
						this.View.Model.CreateNewEntryRow(entry, -1, dynamicObject);
					}
					int currRowIndex2 = bomQueryEntryDatas.IndexOf(dynamicObject);
					this.ExpandedAllRows(currRowIndex2, dynamicValue, bomQueryForwardEntrys, bomQueryForwardEntryGroups, bomQueryForwardEntryGroupDatas, treeEntryGrid, entry, bomQueryEntryDatas, maxLineLimit, ref maxCount);
				}
			}
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x00046E3C File Offset: 0x0004503C
		private void expandAllLimitSetColor(Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, DynamicObjectCollection bomQueryEntryDatas, TreeEntryGrid treeEntryGrid)
		{
			if (ListUtils.IsEmpty<DynamicObject>(bomQueryEntryDatas))
			{
				return;
			}
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			foreach (DynamicObject dynamicObject in bomQueryEntryDatas)
			{
				IGrouping<string, DynamicObject> grouping = null;
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
				if (!bomQueryForwardEntryGroups.TryGetValue(dynamicValue, out grouping))
				{
					int key = bomQueryEntryDatas.IndexOf(dynamicObject);
					string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ErpClsID", null);
					if ("2".Equals(dynamicValue2) || "3".Equals(dynamicValue2) || "5".Equals(dynamicValue2))
					{
						list.Add(new KeyValuePair<int, string>(key, "#E6B8B7"));
					}
				}
			}
			if (!ListUtils.IsEmpty<KeyValuePair<int, string>>(list))
			{
				treeEntryGrid.AftSetRowBackcolor(list);
			}
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x00046F10 File Offset: 0x00045110
		protected bool GetDefaultcheme(string FormId, string FormFilterId, out FilterParameter FilterParam)
		{
			FilterParam = new FilterParameter();
			string nextEntrySchemeId = UserParamterServiceHelper.GetNextEntrySchemeId(base.Context, FormId);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(nextEntrySchemeId))
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BOS_FilterScheme");
				FormMetadata formMetaData2 = MetaDataServiceHelper.GetFormMetaData(base.Context, FormFilterId);
				DynamicObject dynamicObject = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					nextEntrySchemeId
				}, formMetaData.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
				if (!ObjectUtils.IsNullOrEmpty(dynamicObject))
				{
					if (StringUtils.EqualsIgnoreCase(Convert.ToString(dynamicObject["FSchemeName"]), "Default Scheme"))
					{
						return false;
					}
					FilterScheme filterScheme = new FilterScheme(dynamicObject);
					SchemeEntity schemeEntity = (SchemeEntity)new DcxmlSerializer(new PreInsertDataDcxmlBinder()).DeserializeFromString(filterScheme.Scheme, null);
					DcxmlBinder dcxmlBinder = new DynamicObjectDcxmlBinder(formMetaData2.BusinessInfo);
					dcxmlBinder.OnlyDbProperty = false;
					CultureInfo culture = new CultureInfo(base.Context.UserLocale.LCID);
					dcxmlBinder.Culture = culture;
					DcxmlSerializer dcxmlSerializer = new DcxmlSerializer(dcxmlBinder);
					DynamicObject dynamicObject2 = (DynamicObject)dcxmlSerializer.DeserializeFromString(schemeEntity.CustomFilterSetting, null);
					if (!ObjectUtils.IsNullOrEmpty(dynamicObject2))
					{
						FilterParam.CustomFilter = dynamicObject2;
						JSONArray jsonarray = new JSONArray(schemeEntity.ColumnSetting);
						for (int i = 0; i < jsonarray.Count; i++)
						{
							Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonarray[i];
							if (1 == DictionaryUtils.GetInt(dictionary, "V"))
							{
								ColumnField columnField = new ColumnField();
								columnField.Key = DictionaryUtils.GetString(dictionary, "F");
								columnField.FieldName = DictionaryUtils.GetString(dictionary, "F");
								FilterParam.ColumnInfo.Add(columnField);
							}
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x000470D4 File Offset: 0x000452D4
		private void GroupBomEntryWithParentEntryID(IEnumerable<DynamicObject> lstQueryResult)
		{
			Dictionary<string, IGrouping<string, DynamicObject>> dicBomExpandResults = (from g in lstQueryResult
			group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			int num = 0;
			foreach (DynamicObject dynamicObject in lstQueryResult)
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SerialNumber", null);
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue))
				{
					List<int> serialNumbers = new List<int>
					{
						num
					};
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SerialNumber", num);
					string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					this.QueryChildBomEntry(dicBomExpandResults, dynamicValue2, serialNumbers);
				}
			}
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x000471B4 File Offset: 0x000453B4
		private void QueryChildBomEntry(Dictionary<string, IGrouping<string, DynamicObject>> dicBomExpandResults, string rowId, List<int> serialNumbers)
		{
			IGrouping<string, DynamicObject> grouping;
			if (dicBomExpandResults.TryGetValue(rowId, out grouping))
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					serialNumbers.Add(serialNumbers.Max() + 1);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SerialNumber", serialNumbers.Max());
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					this.QueryChildBomEntry(dicBomExpandResults, dynamicValue, serialNumbers);
				}
			}
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x00047270 File Offset: 0x00045470
		private void SetTopMaterialId(List<DynamicObject> bomData)
		{
			if (ListUtils.IsEmpty<DynamicObject>(bomData))
			{
				return;
			}
			Dictionary<string, List<DynamicObject>> dictionary = (from m in bomData
			group m by DataEntityExtend.GetDynamicValue<string>(m, "TopEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> m) => m.Key, (IGrouping<string, DynamicObject> n) => n.ToList<DynamicObject>());
			if (ListUtils.IsEmpty<KeyValuePair<string, List<DynamicObject>>>(dictionary))
			{
				return;
			}
			foreach (KeyValuePair<string, List<DynamicObject>> keyValuePair in dictionary)
			{
				DynamicObject dynamicObject = keyValuePair.Value.First((DynamicObject m) => ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(m, "ParentEntryId", null)));
				if (!ObjectUtils.IsNullOrEmpty(dynamicObject))
				{
					DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null);
					foreach (DynamicObject dynamicObject2 in keyValuePair.Value)
					{
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "ParentEntryId", null)))
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "TopMaterialId", dynamicValue);
						}
					}
				}
			}
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x000473D4 File Offset: 0x000455D4
		public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
		{
			base.OnPrepareNotePrintData(e);
			BusinessInfo businessInfo = this.View.BusinessInfo;
			List<DynamicObject> list = null;
			if (e.DataSourceId.Equals("FBillHead", StringComparison.OrdinalIgnoreCase))
			{
				list = new DynamicObject[]
				{
					this.View.Model.DataObject
				}.ToList<DynamicObject>();
			}
			if (e.DataSourceId.Equals("FBottomEntity", StringComparison.OrdinalIgnoreCase))
			{
				list = this.SortDynamicObjectSource(this.bomPrintChildItems);
			}
			e.DataObjects = MFGCommonUtil.ReflushDynamicObjectTypeSource(this.View, businessInfo, e.DataSourceId, e.DynamicObjectType, list);
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x000474A8 File Offset: 0x000456A8
		private List<DynamicObject> SortDynamicObjectSource(List<DynamicObject> sourceDynamicObject)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from o in sourceDynamicObject
			group o by DataEntityExtend.GetDynamicObjectItemValue<int>(o, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> o) => o.Key);
			Dictionary<string, IGrouping<string, DynamicObject>> sourceGroupDynamicObject = (from o in sourceDynamicObject
			where DataEntityExtend.GetDynamicObjectItemValue<int>(o, "BomLevel", 0) > 0
			group o by DataEntityExtend.GetDynamicObjectItemValue<string>(o, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> o) => o.Key);
			IGrouping<int, DynamicObject> source;
			dictionary.TryGetValue(0, out source);
			List<DynamicObject> list2 = source.ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list2)
			{
				list.Add(dynamicObject);
				IGrouping<int, DynamicObject> grouping;
				if (dictionary.TryGetValue(1, out grouping))
				{
					List<DynamicObject> list3 = new List<DynamicObject>();
					bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FisShowNumber", -1, false, null);
					if (value)
					{
						list3 = this.AddSubNode(dynamicObject, sourceGroupDynamicObject, list3);
					}
					else
					{
						string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
						list3 = this.AddSubNodeOld(dynamicObjectItemValue, sourceGroupDynamicObject, list3);
					}
					list.AddRange(list3);
				}
			}
			return list;
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00047624 File Offset: 0x00045824
		private string GetTreeFormat(DynamicObject dynamicObject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			long num = Convert.ToInt64(dynamicObject["BomLevel"].ToString());
			int num2 = 0;
			while ((long)num2 < num)
			{
				stringBuilder.Append('.');
				num2++;
			}
			stringBuilder.Append(num.ToString());
			return stringBuilder.ToString();
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00047678 File Offset: 0x00045878
		private List<DynamicObject> AddSubNodeOld(string entryId, Dictionary<string, IGrouping<string, DynamicObject>> sourceGroupDynamicObject, List<DynamicObject> tempResultDynamicObjects)
		{
			IGrouping<string, DynamicObject> grouping;
			sourceGroupDynamicObject.TryGetValue(entryId, out grouping);
			if (grouping == null)
			{
				return tempResultDynamicObjects;
			}
			List<DynamicObject> list = grouping.ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				DynamicObject dynamicObject2 = OrmUtils.Clone(dynamicObject, dynamicObject.DynamicObjectType, false, true) as DynamicObject;
				dynamicObject2["BomLevel"] = this.GetTreeFormat(dynamicObject2);
				tempResultDynamicObjects.Add(dynamicObject2);
				entryId = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
				this.AddSubNodeOld(entryId, sourceGroupDynamicObject, tempResultDynamicObjects);
			}
			return tempResultDynamicObjects;
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x00047784 File Offset: 0x00045984
		private List<DynamicObject> AddSubNode(DynamicObject parentNode, Dictionary<string, IGrouping<string, DynamicObject>> sourceGroupDynamicObject, List<DynamicObject> tempResultDynamicObjects)
		{
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(parentNode, "EntryId", null);
			IGrouping<string, DynamicObject> grouping;
			sourceGroupDynamicObject.TryGetValue(dynamicObjectItemValue, out grouping);
			if (grouping == null)
			{
				return tempResultDynamicObjects;
			}
			List<DynamicObject> list = grouping.ToList<DynamicObject>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BomQueryForward.<>c__DisplayClass61 CS$<>8__locals1 = new BomQueryForward.<>c__DisplayClass61();
					CS$<>8__locals1.subDynamicObject = enumerator.Current;
					DynamicObject dynamicObject = OrmUtils.Clone(CS$<>8__locals1.subDynamicObject, CS$<>8__locals1.subDynamicObject.DynamicObjectType, false, true) as DynamicObject;
					int count = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "BomLevel", 0);
					if (DataEntityExtend.GetDynamicValue<int>(CS$<>8__locals1.subDynamicObject, "MaterialType", 0) == 3)
					{
						dynamicObject["BomLevel"] = DataEntityExtend.GetDynamicValue<string>((from z in tempResultDynamicObjects
						where this.countNumber(DataEntityExtend.GetDynamicValue<string>(z, "BomLevel", null)) == count
						select z).Last<DynamicObject>(), "BomLevel", null);
					}
					else
					{
						dynamicObject["BomLevel"] = ((DataEntityExtend.GetDynamicValue<string>(parentNode, "BomLevel", null) == "0") ? "1" : DataEntityExtend.GetDynamicValue<string>(parentNode, "BomLevel", null)) + "." + ((from z in list
						where DataEntityExtend.GetDynamicValue<int>(z, "MaterialType", 0) != 3 && DataEntityExtend.GetDynamicValue<string>(z, "BomLevel", null) == DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.subDynamicObject, "BomLevel", null)
						select z).ToList<DynamicObject>().IndexOf(CS$<>8__locals1.subDynamicObject) + 1);
					}
					tempResultDynamicObjects.Add(dynamicObject);
					this.AddSubNode(dynamicObject, sourceGroupDynamicObject, tempResultDynamicObjects);
				}
			}
			return tempResultDynamicObjects;
		}

		// Token: 0x0400026B RID: 619
		protected const string EntityKey_FBomHeadEntity = "FTopEntity";

		// Token: 0x0400026C RID: 620
		protected const string EntityKey_FBomChildEntity = "FBottomEntity";

		// Token: 0x0400026D RID: 621
		protected const string EntityKey_FBomBillHead = "FBillHead";

		// Token: 0x0400026E RID: 622
		protected const string EntityKey_FCobyEntity = "FCobyEntity";

		// Token: 0x0400026F RID: 623
		protected const string FieldKey_FExpandLevel = "FExpandLevel";

		// Token: 0x04000270 RID: 624
		protected const string FieldKey_FValidDate = "FValidDate";

		// Token: 0x04000271 RID: 625
		protected const string FieldKey_FQty = "FQty";

		// Token: 0x04000272 RID: 626
		protected const string FieldKey_FBomId = "FBomId";

		// Token: 0x04000273 RID: 627
		protected const string FieldKey_FMaterialId = "FMaterialId";

		// Token: 0x04000274 RID: 628
		protected const string ControlKey_SplitContainer = "FSpliteContainer1";

		// Token: 0x04000275 RID: 629
		protected const string FieldKey_FBillBomId = "FBillBomId";

		// Token: 0x04000276 RID: 630
		protected const string FieldKey_FBillMaterialId = "FBillMaterialId";

		// Token: 0x04000277 RID: 631
		protected const string FieldKey_FBillMtrlAuxId = "FBillMtrlAuxId";

		// Token: 0x04000278 RID: 632
		protected const string FieldKey_FBomUseOrgId = "FBomUseOrgId";

		// Token: 0x04000279 RID: 633
		protected const string FiledKey_FBomVersion = "FBomVersion";

		// Token: 0x0400027A RID: 634
		protected const string FieldKey_FBomChildMaterialId = "FMaterialId2";

		// Token: 0x0400027B RID: 635
		protected const string FieldKey_FBomEntryId = "FBomEntryId";

		// Token: 0x0400027C RID: 636
		protected const string ControlKey_tab = "FTABBOTTOM";

		// Token: 0x0400027D RID: 637
		protected const string FormKey_MaterialTree = "MFG_MaterialTree";

		// Token: 0x0400027E RID: 638
		protected const string Key_Contain = "FTreePanel";

		// Token: 0x0400027F RID: 639
		private bool IsOpenAttach;

		// Token: 0x04000280 RID: 640
		protected string currentFormId = "ENG_BomQueryForward2";

		// Token: 0x04000281 RID: 641
		private string _materialTree_PageId;

		// Token: 0x04000282 RID: 642
		protected List<DynamicObject> bomQueryChildItems = new List<DynamicObject>();

		// Token: 0x04000283 RID: 643
		protected List<DynamicObject> bomPrintChildItems = new List<DynamicObject>();

		// Token: 0x04000284 RID: 644
		protected MemBomExpandOption_ForPSV memBomExpandOption;

		// Token: 0x04000285 RID: 645
		protected FilterParameter filterParam;

		// Token: 0x04000286 RID: 646
		private int curTabindex;

		// Token: 0x04000287 RID: 647
		private int _curRow;

		// Token: 0x04000288 RID: 648
		private List<NetworkCtrlResult> networkCtrlResults;

		// Token: 0x04000289 RID: 649
		private List<DynamicObject> _exportData = new List<DynamicObject>();
	}
}
