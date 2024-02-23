using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.MaterialTree;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using Kingdee.K3.MFG.ServiceHelper.PLN;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000075 RID: 117
	public class BomQueryBackward : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600084C RID: 2124 RVA: 0x000628C0 File Offset: 0x00060AC0
		protected string MaterialTree_PageId
		{
			get
			{
				return string.Format("{0}_MaterialTree", this.View.PageId);
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600084D RID: 2125 RVA: 0x000628D7 File Offset: 0x00060AD7
		// (set) Token: 0x0600084E RID: 2126 RVA: 0x000628E0 File Offset: 0x00060AE0
		public int curRow
		{
			get
			{
				return this._curRow;
			}
			set
			{
				this._curRow = value;
				bool flag = this._curRow + 1 < this.bomParentItems.Count;
				this.View.StyleManager.SetVisible("FBt_More", "ShowMore", flag);
				this.View.StyleManager.SetVisible("FImg", "ShowMore", flag);
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600084F RID: 2127 RVA: 0x00062940 File Offset: 0x00060B40
		// (set) Token: 0x06000850 RID: 2128 RVA: 0x00062948 File Offset: 0x00060B48
		public List<DynamicObject> bomParentItems { get; set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000851 RID: 2129 RVA: 0x00062951 File Offset: 0x00060B51
		// (set) Token: 0x06000852 RID: 2130 RVA: 0x00062959 File Offset: 0x00060B59
		private List<DynamicObject> bomPrintItems { get; set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000853 RID: 2131 RVA: 0x00062962 File Offset: 0x00060B62
		// (set) Token: 0x06000854 RID: 2132 RVA: 0x0006296A File Offset: 0x00060B6A
		public object FMaterialId { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000855 RID: 2133 RVA: 0x00062973 File Offset: 0x00060B73
		// (set) Token: 0x06000856 RID: 2134 RVA: 0x0006297B File Offset: 0x00060B7B
		public object FUseOrgId { get; set; }

		// Token: 0x06000857 RID: 2135 RVA: 0x00062984 File Offset: 0x00060B84
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitializeBomQueryOption();
			DynamicFormOpenParameter paramter = e.Paramter;
			object customParameter = paramter.GetCustomParameter("FMaterialId");
			object customParameter2 = paramter.GetCustomParameter("FUseOrgId");
			if (customParameter != null && customParameter2 != null)
			{
				this.FMaterialId = customParameter;
				this.FUseOrgId = customParameter2;
			}
			SplitContainer control = this.View.GetControl<SplitContainer>("FSpliteContainer1");
			control.HideFirstPanel(true);
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x000629EC File Offset: 0x00060BEC
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (this.FMaterialId != null && this.FUseOrgId != null)
			{
				this.View.Model.SetValue("FBomUseOrgId", OtherExtend.ConvertTo<long>(this.FUseOrgId, 0L));
				string[] array = OtherExtend.ConvertTo<string>(this.FMaterialId, null).Split(new char[]
				{
					';'
				});
				this.View.Model.SetValue("FBillMaterialId", array);
				if (array.Length == 1)
				{
					this.SetAuxComBox(Convert.ToInt64(array[0]));
				}
				this.FillBomDetailData();
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

		// Token: 0x06000859 RID: 2137 RVA: 0x00062AB0 File Offset: 0x00060CB0
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.View.StyleManager.SetVisible("FBt_More", "ShowMore", false);
			this.View.StyleManager.SetVisible("FImg", "ShowMore", false);
			if (base.Context.IsStandardEdition())
			{
				this.Model.SetValue("FIsMultiOrg", false);
				this.View.StyleManager.SetVisible("FIsMultiOrg", "FIsMultiOrg", false);
			}
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x00062B38 File Offset: 0x00060D38
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBREFRESH")
				{
					this.FillBomDetailData();
					e.Cancel = true;
					return;
				}
				if (!(a == "TBFILTER"))
				{
					return;
				}
				this.ShowFilter(false);
				e.Cancel = true;
			}
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00062BA8 File Offset: 0x00060DA8
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
					Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups = (from g in entityDataObject
					group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FBottomEntity");
					DynamicObject entityDataObject2 = this.View.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
					string currEntryId = entityDataObject2["EntryId"].ToString();
					this.ExpandedAllRows(entryCurrentRowIndex, currEntryId, entityDataObject, bomQueryForwardEntryGroups, control);
					this.View.SetEntityFocusRow("FBottomEntity", entryCurrentRowIndex);
				}
			}
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x00062CC8 File Offset: 0x00060EC8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (a == "FRELOADDATA")
				{
					this.FillBomDetailData();
					return;
				}
				if (!(a == "FBT_MORE"))
				{
					return;
				}
				this.ExpandMoreData(this.curRow);
			}
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x00062D1C File Offset: 0x00060F1C
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			bool flag = true;
			if (flag && e.Key.Equals("FTOPENTITY"))
			{
				this.topEntityRowClick();
			}
			if (e.Key.Equals("FBottomEntity".ToUpper()))
			{
				this.bottomEntityRowClick();
			}
			if (e.Key.Equals("FEntityMainItems".ToUpper()))
			{
				this.mainEntityRowClick();
			}
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00062D88 File Offset: 0x00060F88
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.BaseDataField.Key.Equals("FBomVersion"))
			{
				e.IsShowApproved = false;
				DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FBillMaterialId", -1, null, null);
				if (value.Count<DynamicObject>() == 1)
				{
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(value.ElementAt(0), "BillMaterialId_Id", 0L);
					if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = string.Format(" FMATERIALID = {0} ", dynamicValue);
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter += string.Format(" AND FMATERIALID = {0} ", dynamicValue);
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

		// Token: 0x0600085F RID: 2143 RVA: 0x00062E76 File Offset: 0x00061076
		public override void Dispose()
		{
			base.Dispose();
			MFGCommonUtil.DoCommitNetworkCtrl(this.View.Context, this.networkCtrlResults);
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x00062E94 File Offset: 0x00061094
		private void topEntityRowClick()
		{
			int focusRowIndex = this.View.GetControl<EntryGrid>("FTopEntity").GetFocusRowIndex();
			if (focusRowIndex < 0)
			{
				return;
			}
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FMATERIALIDCHILD", focusRowIndex, 0L, null);
			DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, "FAuxPropId", focusRowIndex, null, null);
			this.View.Model.SetValue("FBillMaterialId", value);
			if (value2 == null)
			{
				this.View.Model.SetValue("FAuxCombox", "0");
			}
			else
			{
				string text = value2["Id"].ToString();
				this.View.Model.SetValue("FAuxCombox", text);
			}
			this.FillBomDetailData();
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x00062F84 File Offset: 0x00061184
		private void bottomEntityRowClick()
		{
			int focusRowIndex = this.View.GetControl<EntryGrid>("FBottomEntity").GetFocusRowIndex();
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			if (focusRowIndex < 0 || ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			DynamicObject parentMtrl = this.View.Model.GetEntityDataObject(entryEntity, focusRowIndex);
			DynamicObject dynamicObject = (from dym in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<long>(dym, "EntryId", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(parentMtrl, "ParentEntryId", 0L)
			select dym).ToList<DynamicObject>().FirstOrDefault<DynamicObject>();
			if (dynamicObject == null)
			{
				this.View.Model.SetValue("FSelectMaterialId", 0, -1);
				this.Model.DeleteEntryData("FEntityMainItems");
				this.Model.DeleteEntryData("FEntity");
				return;
			}
			string bomId = DataEntityExtend.GetDynamicObjectItemValue<long>(parentMtrl, "BomId_Id", 0L).ToString();
			this.SetFixedBomSubTab(dynamicObject, bomId);
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x000630E8 File Offset: 0x000612E8
		private void mainEntityRowClick()
		{
			int focusRowIndex = this.View.GetControl<EntryGrid>("FEntityMainItems").GetFocusRowIndex();
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			if (focusRowIndex < 0 || ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			DynamicObject MainMtrl = this.View.Model.GetEntityDataObject(entryEntity, focusRowIndex);
			List<DynamicObject> list = (from dym in this.BomEntryInfos
			where DataEntityExtend.GetDynamicObjectItemValue<long>(dym, "ReplaceGroup", 0L).ToString().Equals(DataEntityExtend.GetDynamicObjectItemValue<long>(MainMtrl, "ReplaceGroup", 0L).ToString()) && DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MaterialType", null).Equals("3")
			select dym).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			this.BindEntitys("FEntity", list);
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x00063280 File Offset: 0x00061480
		private void SetFixedBomSubTab(DynamicObject childMtrl, string bomId)
		{
			this.BomEntryInfos = BomQueryServiceHelper.GetBOMEntityDatas(base.Context, bomId);
			List<int> replaceGroup = (from dym in this.BomEntryInfos
			where DataEntityExtend.GetDynamicObjectItemValue<long>(dym, "MaterialId_Id", 0L).ToString().Equals(DataEntityExtend.GetDynamicObjectItemValue<long>(childMtrl, "MaterialId2_Id", 0L).ToString()) && !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "ReplacePolicy", null))
			select DataEntityExtend.GetDynamicObjectItemValue<int>(dym, "ReplaceGroup", 0)).ToList<int>();
			List<DynamicObject> list = (from dym in this.BomEntryInfos
			where replaceGroup.Contains(DataEntityExtend.GetDynamicObjectItemValue<int>(dym, "ReplaceGroup", 0)) && DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MaterialType", null).Equals("3")
			select dym).ToList<DynamicObject>();
			this.View.Model.SetValue("FSelectMaterialId", DataEntityExtend.GetDynamicObjectItemValue<long>(childMtrl, "MaterialId2_Id", 0L), -1);
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.Model.DeleteEntryData("FEntityMainItems");
				this.Model.DeleteEntryData("FEntity");
				return;
			}
			List<IGrouping<int, DynamicObject>> list2 = (from dym in this.BomEntryInfos
			where replaceGroup.Contains(DataEntityExtend.GetDynamicObjectItemValue<int>(dym, "ReplaceGroup", 0)) && !DataEntityExtend.GetDynamicObjectItemValue<string>(dym, "MaterialType", null).Equals("3")
			group dym by DataEntityExtend.GetDynamicObjectItemValue<int>(dym, "ReplaceGroup", 0) into gDym
			select gDym).ToList<IGrouping<int, DynamicObject>>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			for (int i = 0; i < list2.Count; i++)
			{
				List<DynamicObject> collection = list2.ElementAtOrDefault(i).ToList<DynamicObject>();
				list3.AddRange(collection);
			}
			if (ListUtils.IsEmpty<DynamicObject>(list3))
			{
				return;
			}
			this.BindEntitys("FEntityMainItems", list3);
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0006342C File Offset: 0x0006162C
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			this.curTabindex = e.TabIndex;
			TabControlAppearance tabControlAppearance = this.View.LayoutInfo.GetAppearance(e.Key) as TabControlAppearance;
			if (tabControlAppearance == null)
			{
				return;
			}
			TabPageAppearance tabPageAppearance = tabControlAppearance.TabPages.FirstOrDefault((TabPageAppearance o) => o.PageIndex == e.TabIndex);
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x00063498 File Offset: 0x00061698
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FBillMaterialId"))
				{
					if (key == "FBomUseOrgId")
					{
						this.View.Model.SetValue("FBillMaterialId", null);
						return;
					}
					if (!(key == "FIsAuxpropFilter"))
					{
						return;
					}
					if (Convert.ToBoolean(e.NewValue))
					{
						this.View.GetControl("FAuxCombox").Enabled = false;
						return;
					}
					this.View.GetControl("FAuxCombox").Enabled = true;
					this.View.Model.SetValue("FSAuxpropId", 0);
				}
				else
				{
					if (e.NewValue == null)
					{
						return;
					}
					string[] array;
					if (e.NewValue is Array)
					{
						array = (string[])e.NewValue;
					}
					else
					{
						array = new string[]
						{
							Convert.ToString(e.NewValue)
						};
					}
					if (array.Length > 1)
					{
						this.Model.SetValue("FBomVersion", null);
						this.Model.SetValue("FAuxCombox", null);
						this.View.GetControl("FBomVersion").Enabled = false;
						this.View.GetControl("FAuxCombox").Enabled = false;
						this.Model.SetValue("FMtrlId", null);
						return;
					}
					this.View.GetControl("FBomVersion").Enabled = true;
					this.View.GetControl("FAuxCombox").Enabled = true;
					this.SetAuxComBox(Convert.ToInt64(array[0]));
					if (array[0] == null)
					{
						this.View.Model.SetValue("FBomVersion", null);
						return;
					}
					long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, Convert.ToInt64(array[0]));
					long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
					string value2 = MFGBillUtil.GetValue<string>(this.View.Model, "FAuxCombox", -1, null, null);
					long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], value, string.IsNullOrWhiteSpace(value2) ? 0L : Convert.ToInt64(value2));
					this.View.Model.SetValue("FBomVersion", hightVersionBomKey);
					this.View.Model.SetValue("FMtrlId", Convert.ToInt64(array[0]));
					return;
				}
			}
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x00063704 File Offset: 0x00061904
		private void ExpandMoreData(int row)
		{
			string text = "FBOTTOMENTITY";
			TreeEntryEntity treeEntryEntity = (TreeEntryEntity)this.View.BusinessInfo.GetEntity(text);
			List<DynamicObject> list = this.SetRowType(row + 1, this.bomParentItems);
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsShowHVBom", -1, false, null);
			List<long> list2 = new List<long>();
			list2 = this.GetHighBomVersions(list, value);
			for (int i = 0; i < list.Count; i++)
			{
				this.View.Model.CreateNewEntryRow(treeEntryEntity, -1, list[i]);
				int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex(text);
				long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FBomId", entryCurrentRowIndex, 0L, null);
				if (list2.Contains(value2))
				{
					this.View.Model.SetValue("FHightVersionBom", true, entryCurrentRowIndex);
				}
			}
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(treeEntryEntity);
			this.SetBGColor(entityDataObject, value);
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x000638C8 File Offset: 0x00061AC8
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.Key == "MFG_MaterialTree" && e.EventName == "TreeNodeClick")
			{
				List<DynamicObject> showBom = new List<DynamicObject>();
				string eventArgs = e.EventArgs;
				if (eventArgs.Contains("_"))
				{
					int num = eventArgs.IndexOf("_");
					string orgId = eventArgs.Substring(0, num);
					string groupId = eventArgs.Substring(num + 1, eventArgs.Length - num - 1);
					this.View.Model.SetValue("FBomUseOrgId", orgId);
					showBom = (from p in this.bomQueryChildItems
					where (p["MATERIALIDCHILD"] as DynamicObject)["MaterialGroup_Id"].ToString().Equals(groupId) && p["UseOrgId_Id"].ToString().Equals(orgId)
					select p).ToList<DynamicObject>();
				}
				else if (eventArgs.Contains("m"))
				{
					eventArgs.Substring(1, eventArgs.Length - 1);
				}
				else
				{
					string orgId = eventArgs;
					this.View.Model.SetValue("FBomUseOrgId", orgId);
					showBom = (from p in this.bomQueryChildItems
					where p["UseOrgId_Id"].ToString().Equals(orgId) && (p["MATERIALIDCHILD"] as DynamicObject)["MaterialGroup_Id"].ToString().Equals("0")
					select p).ToList<DynamicObject>();
				}
				this.FillBomHeadByNode(showBom);
				BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
			}
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x00063A5C File Offset: 0x00061C5C
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			base.BeforeEntityExport(e);
			List<DynamicObject> list = this.bomParentItems;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FBottomEntity"));
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FExportBomLevel", -1, false, null);
			if (!value && !ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				List<long> entrysOrder = (from s in entityDataObject
				select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>();
				list = (from o in list
				orderby entrysOrder.IndexOf(DataEntityExtend.GetDynamicValue<long>(o, "Id", 0L))
				select o).ToList<DynamicObject>();
			}
			bool value2 = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsShowHVBom", -1, false, null);
			List<long> list2 = new List<long>();
			list2 = this.GetHighBomVersions(list, value2);
			if (!ListUtils.IsEmpty<long>(list2))
			{
				foreach (DynamicObject dynamicObject in list)
				{
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
					if (list2.Contains(dynamicValue))
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "HightVersionBom", true);
					}
				}
			}
			List<DynamicObject> list3 = new List<DynamicObject>();
			if (!value && list != null && list.Count > 0)
			{
				List<DynamicObject> list4 = (from s in list
				where DataEntityExtend.GetDynamicValue<string>(s, "ParentEntryId", null) == "0"
				select s).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject2 in list4)
				{
					list3.Add(dynamicObject2);
					this.RebuildCollection(list.ToList<DynamicObject>(), dynamicObject2, ref list3);
				}
			}
			e.DataSource = new Dictionary<string, List<DynamicObject>>
			{
				{
					"FBottomEntity",
					value ? list : list3
				}
			};
			e.ExportEntityKeyList = new List<string>
			{
				"FBottomEntity"
			};
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x00063C78 File Offset: 0x00061E78
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有物料清单反查的{0}权限", "015072000019376", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x00063D80 File Offset: 0x00061F80
		private void RebuildCollection(List<DynamicObject> lstDo, DynamicObject currentDo, ref List<DynamicObject> lstExprotData)
		{
			List<DynamicObject> list = (from c in lstDo
			where Convert.ToString(c["ParentEntryId"]) == Convert.ToString(currentDo["EntryId"])
			select c).ToList<DynamicObject>();
			if (list == null || list.Count < 1)
			{
				return;
			}
			foreach (DynamicObject dynamicObject in list)
			{
				DynamicObject dynamicObject2 = (DynamicObject)OrmUtils.Clone(dynamicObject, false, false);
				int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "BomLevel", 0);
				dynamicObject2["BomLevel"] = this.GetBomLevelString(dynamicObjectItemValue);
				lstExprotData.Add(dynamicObject2);
				lstDo.Remove(dynamicObject);
				this.RebuildCollection(lstDo, dynamicObject, ref lstExprotData);
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00063E44 File Offset: 0x00062044
		private string GetBomLevelString(int count)
		{
			string str = string.Empty;
			for (int i = 0; i < count; i++)
			{
				str += ".";
			}
			return str + Convert.ToString(count);
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x00063E80 File Offset: 0x00062080
		protected void InitializeBomQueryOption()
		{
			MFGServiceHelper.GetSysDate(base.Context);
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			this.bomQueryOption = new BomQueryOption(formMetaData);
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x00063EB8 File Offset: 0x000620B8
		protected void UpdateBomQueryOption()
		{
			this.bomQueryOption.ExpandLevelTo = MFGBillUtil.GetValue<int>(this.Model, "FExpandLevel", -1, 0, null);
			if (this.bomQueryOption.ExpandLevelTo == 0)
			{
				this.bomQueryOption.ExpandLevelTo = this.bomQueryOption.BomMaxLevel;
			}
			this.bomQueryOption.ValidDate = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, default(DateTime), null));
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00063F44 File Offset: 0x00062144
		private void FillMtrl()
		{
			this.bomQueryChildItems = BomQueryServiceHelper.GetBomQueryBackwardItems(base.Context, this.filterParam);
			this.bomQueryChildItems.RemoveAll((DynamicObject e) => e["MATERIALIDCHILD"] == null);
			List<object> list = new List<object>();
			foreach (DynamicObject dynamicObject in this.bomQueryChildItems)
			{
				list.Add(dynamicObject["MATERIALIDCHILD_Id"]);
			}
			IDynamicFormView view = this.View.GetView(this.MaterialTree_PageId);
			if (view == null)
			{
				TreeParameters treeParameters = new TreeParameters();
				treeParameters.IsShowOrg = true;
				treeParameters.ShowMtrlLevel = 1;
				this.View.Session["1"] = treeParameters;
				this.View.Session["2"] = list;
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.OpenStyle.ShowType = 3;
				dynamicFormShowParameter.OpenStyle.TagetKey = "FTreePanel";
				dynamicFormShowParameter.FormId = "MFG_MaterialTree";
				dynamicFormShowParameter.PageId = this.MaterialTree_PageId;
				dynamicFormShowParameter.CustomParams.Add("ShowParam", "1");
				dynamicFormShowParameter.CustomParams.Add("ShowObject", "2");
				this.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			TreeParameters treeParameters2 = new TreeParameters();
			treeParameters2.IsShowOrg = true;
			treeParameters2.ShowMtrlLevel = 1;
			this.View.Session["1"] = treeParameters2;
			this.View.Session["2"] = list;
			view.Refresh();
			this.View.SendDynamicFormAction(view);
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00064120 File Offset: 0x00062320
		protected virtual void FillBomHeadData()
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FTopEntity");
			this.Model.DeleteEntryData("FBottomEntity");
			this.FillMtrl();
			this.Model.EndIniti();
			this.View.UpdateView();
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00064174 File Offset: 0x00062374
		protected virtual void FillBomHeadByNode(List<DynamicObject> showBom)
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FTopEntity");
			this.Model.DeleteEntryData("FBottomEntity");
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTopEntity");
			if (showBom != null && showBom.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject in showBom)
				{
					dynamicObject["SEQ"] = showBom.IndexOf(dynamicObject) + 1;
					entityDataObject.Add(dynamicObject);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FTopEntity");
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00064264 File Offset: 0x00062464
		protected virtual void FillBomDetailData()
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			BomQueryOption bomQueryOption = new BomQueryOption(formMetaData);
			bomQueryOption.ExpandLevelTo = MFGBillUtil.GetValue<int>(this.Model, "FExpandLevel", -1, 0, null);
			if (bomQueryOption.ExpandLevelTo == 0)
			{
				bomQueryOption.ExpandLevelTo = bomQueryOption.BomMaxLevel;
			}
			bomQueryOption.ValidDate = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, default(DateTime), null));
			bool value = MFGBillUtil.GetValue<bool>(this.Model, "FIsAuxpropFilter", -1, false, null);
			bomQueryOption.isAuxpropFilter = !value;
			bomQueryOption.Option.SetVariableValue("BomUseOrgId", MFGBillUtil.GetValue<long>(this.Model, "FBomUseOrgId", -1, 0L, null));
			bomQueryOption.Option.SetVariableValue("IsMultiOrg", MFGBillUtil.GetValue<bool>(this.Model, "FIsMultiOrg", -1, false, null));
			bomQueryOption.Option.SetVariableValue("ContainForbiddenBom", MFGBillUtil.GetValue<bool>(this.Model, "FContainForbiddenBom", -1, false, null));
			this.UpdateBomQueryOption();
			this.Model.DeleteEntryData("FBottomEntity");
			List<DynamicObject> list = this.BuildBomExpandSourceData(0);
			if (list == null || list.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当前子项物料为空，请重新选择！", "015072000002204", 7, new object[0]), ResManager.LoadKDString("子项物料为空！", "015072000002205", 7, new object[0]), 0);
				return;
			}
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			taskProxyItem.Parameters = new List<object>
			{
				base.Context,
				list,
				bomQueryOption,
				taskProxyItem.TaskId
			}.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BomQueryService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "GetBomQueryBackwardDatas";
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult result)
			{
				this.BindChildEntitys(result.FuncResult as List<DynamicObject>);
			});
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00064468 File Offset: 0x00062668
		public virtual void BindChildEntitys(List<DynamicObject> bomDatas)
		{
			if (ListUtils.IsEmpty<DynamicObject>(bomDatas))
			{
				return;
			}
			if (!MFGBillUtil.GetValue<bool>(this.View.Model, "FIsBomFilter", -1, false, null))
			{
				this.GetBomResultsByBomId(bomDatas);
				this.FindParentBom(bomDatas);
			}
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsAuxpropFilter", -1, false, null);
			if (value)
			{
				this.GetBomResultsByAuxPropId(bomDatas);
			}
			bool value2 = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsShowCfBom", -1, false, null);
			if (value2)
			{
				this.GetBomResutsByCfgBomId(bomDatas);
			}
			DynamicObject value3 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FFirstParentMtrl", -1, null, null);
			DynamicObject value4 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FFirstParentMtrlTo", -1, null, null);
			if (!ObjectUtils.IsNullOrEmpty(value3) || !ObjectUtils.IsNullOrEmpty(value4))
			{
				bomDatas = this.GetBomResultByFirstParent(bomDatas, value3, value4);
			}
			this.bomParentItems = bomDatas;
			this.bomPrintItems = this.bomParentItems;
			List<DynamicObject> list = this.SetRowType(0, this.bomParentItems);
			bool value5 = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsShowHVBom", -1, false, null);
			List<long> list2 = new List<long>();
			list2 = this.GetHighBomVersions(list, value5);
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (list != null && list.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject in list)
				{
					DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
					if (!ObjectUtils.IsNullOrEmpty(dynamicValue))
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ParentAuxPropId_Id", DataEntityExtend.GetDynamicValue<long>(dynamicValue, "ParentAuxPropId_Id", 0L));
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ParentAuxPropId", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicValue, "ParentAuxPropId", null));
					}
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
					if (list2.Contains(dynamicValue2))
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "HightVersionBom", true);
					}
					entityDataObject.Add(dynamicObject);
				}
			}
			FormOperation operation = this.View.BusinessInfo.GetForm().GetOperation("ExtBomFieldMaps");
			if (!ObjectUtils.IsNullOrEmpty(operation))
			{
				ExtFieldMapOption extFieldMapOption = new ExtFieldMapOption();
				extFieldMapOption.FormId = "ENG_BomQueryBackward";
				extFieldMapOption.EntryKey = "FBottomEntity";
				extFieldMapOption.EntryProp = "BomParent";
				extFieldMapOption.OperationNumber = "ExtBomFieldMaps";
				extFieldMapOption.BillFormId = "ENG_BOM";
				extFieldMapOption.BillInterIdKey = "FBomId";
				extFieldMapOption.BillEntryIdKey = "FBomEntryId";
				extFieldMapOption.Data = this.View.Model.DataObject;
				SupplyDemandStateRptServiceHelper.LoadCustPropertyByEFM(base.Context, extFieldMapOption);
			}
			this.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entryEntity);
			if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject2))
			{
				DynamicObject item = (from f in entityDataObject2
				where DataEntityExtend.GetDynamicValue<int>(f, "BomLevel", 0) == 0
				select f).FirstOrDefault<DynamicObject>();
				this.View.SetEntityFocusRow("FBottomEntity", entityDataObject2.IndexOf(item));
			}
			this.SetBGColor(entityDataObject2, value5);
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x000647B0 File Offset: 0x000629B0
		private void BindEntitys(string EntityKey, List<DynamicObject> lstQueryResult)
		{
			this.Model.DeleteEntryData(EntityKey);
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity(EntityKey);
			if (lstQueryResult != null && lstQueryResult.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject item in lstQueryResult)
				{
					entityDataObject.Add(item);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView(EntityKey);
			this.View.SetEntityFocusRow(EntityKey, 0);
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x0006485C File Offset: 0x00062A5C
		private List<DynamicObject> SetRowType(int rowId, List<DynamicObject> queryResult)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			int count = queryResult.Count;
			int curRow = 0;
			int num = rowId;
			while (num < rowId + 1000 && num < count)
			{
				list.Add(queryResult[num]);
				curRow = num;
				num++;
			}
			this.curRow = curRow;
			return list;
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x000648A5 File Offset: 0x00062AA5
		protected List<DynamicObject> GetSubstituteData(long replaceId)
		{
			return BomQueryCommonFuncs.GetSubStituteDataFromCache(base.Context, replaceId);
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x000648B4 File Offset: 0x00062AB4
		private List<DynamicObject> BuildBomExpandSourceData(int iFocusRow)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.Model.DataObject, "BillMaterialId", null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				foreach (DynamicObject dynamicObject in dynamicValue)
				{
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BillMaterialId_Id", 0L);
					if (dynamicValue2 != 0L)
					{
						BomBackwardSourceDynamicRow bomBackwardSourceDynamicRow = BomBackwardSourceDynamicRow.CreateInstance();
						bomBackwardSourceDynamicRow.MaterialId_Id = dynamicValue2;
						string value = MFGBillUtil.GetValue<string>(this.Model, "FAuxCombox", -1, null, null);
						bomBackwardSourceDynamicRow.AuxPropId = (string.IsNullOrWhiteSpace(value) ? 0L : Convert.ToInt64(value));
						list.Add(bomBackwardSourceDynamicRow.DataEntity);
					}
				}
			}
			return list;
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x00064A24 File Offset: 0x00062C24
		protected virtual void ShowFilter(bool OnInitialize = false)
		{
			MFGBillUtil.ShowFilterForm(this.View, "ENG_BomQueryBackward", null, delegate(FormResult filterResult)
			{
				if (filterResult.ReturnData is FilterParameter)
				{
					this.filterParam = (filterResult.ReturnData as FilterParameter);
					SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer1");
					splitContainer.HideFirstPanel(false);
					this.FillBomHeadData();
					BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
					return;
				}
				if (OnInitialize)
				{
					this.View.Close();
				}
			}, "ENG_BomQueryBackward_Filter", 0);
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x00064A68 File Offset: 0x00062C68
		private void SetAuxComBox(long mtrlId)
		{
			this.View.Model.SetValue("FAuxCombox", null);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FAuxCombox", -1);
			List<EnumItem> auxPropCollection = BomQueryServiceHelper.GetAuxPropCollection(base.Context, mtrlId);
			fieldEditor.SetComboItems(auxPropCollection);
			if (auxPropCollection.Count > 0)
			{
				this.View.Model.SetValue("FAuxCombox", auxPropCollection.First<EnumItem>().Value);
			}
			this.View.UpdateView("FAuxCombox");
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x00064AEC File Offset: 0x00062CEC
		protected virtual T GetEntityFieldValue<T>(Field field, DynamicObject dataEntity)
		{
			if (field.DynamicProperty != null)
			{
				string text = field.PropertyName;
				if ((field is BaseDataField && field.PropertyName != "Id" && field.EntityKey.Equals("FBottomEntity", StringComparison.InvariantCultureIgnoreCase)) || field is RelatedFlexGroupField)
				{
					text = string.Format("{0}_Id", field.PropertyName);
				}
				T t = DataEntityExtend.GetDynamicObjectItemValue<T>(dataEntity, text, default(T));
				string a;
				if ((a = field.Key.ToUpper()) != null && a == "FBOMLEVEL")
				{
					int num = 0;
					if (int.TryParse(Convert.ToString(t), out num))
					{
						t = (T)((object)Convert.ChangeType(-num, typeof(T)));
					}
				}
				return t;
			}
			return default(T);
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x00064BBE File Offset: 0x00062DBE
		private BomChildEntryDataView dynamicObjectToBomChildEntryDataView(DynamicObject obj)
		{
			return new BomChildEntryDataView(obj);
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00064BC8 File Offset: 0x00062DC8
		private void DoStartBomNetworkCtrl(List<DynamicObject> bomDatas)
		{
			if (this.networkCtrlResults == null)
			{
				this.networkCtrlResults = new List<NetworkCtrlResult>();
			}
			Dictionary<object, string> dictionary = new Dictionary<object, string>();
			foreach (DynamicObject dynamicObject in bomDatas)
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

		// Token: 0x0600087C RID: 2172 RVA: 0x00064CA4 File Offset: 0x00062EA4
		private void ExpandedAllRows(int currRowIndex, string currEntryId, DynamicObjectCollection bomQueryForwardEntrys, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, TreeEntryGrid treeEntryGrid)
		{
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
					int currRowIndex2 = bomQueryForwardEntrys.IndexOf(dynamicObject);
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					this.ExpandedAllRows(currRowIndex2, dynamicValue, bomQueryForwardEntrys, bomQueryForwardEntryGroups, treeEntryGrid);
				}
			}
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00064D7C File Offset: 0x00062F7C
		private List<long> GetHighBomVersions(List<DynamicObject> lstQueryResult, bool isShowHVBom)
		{
			List<long> result = new List<long>();
			if (isShowHVBom)
			{
				List<Tuple<long, long, long>> list = new List<Tuple<long, long, long>>();
				foreach (DynamicObject dynamicObject in lstQueryResult)
				{
					DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
					DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId2", null);
					long useOrgId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "UseOrgId_Id", 0L);
					long msterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "MsterId", 0L);
					long auxPropId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "ParentAuxPropId_Id", 0L);
					if (!list.Any((Tuple<long, long, long> p) => p.Item1 == msterId && p.Item2 == useOrgId && p.Item3 == auxPropId))
					{
						list.Add(new Tuple<long, long, long>(msterId, useOrgId, auxPropId));
					}
				}
				List<DynamicObject> list2 = BOMServiceHelper.GetHightVersionBom(base.Context, list).ToList<DynamicObject>();
				if (!ListUtils.IsEmpty<DynamicObject>(list2))
				{
					result = (from s in list2
					select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>();
				}
			}
			return result;
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x00064EC8 File Offset: 0x000630C8
		private void SetBGColor(DynamicObjectCollection entrys, bool isShowHVBom)
		{
			if (isShowHVBom)
			{
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				List<DynamicObject> list2 = (from w in entrys
				where DataEntityExtend.GetDynamicValue<bool>(w, "HightVersionBom", false)
				select w).ToList<DynamicObject>();
				foreach (DynamicObject item in list2)
				{
					int key = entrys.IndexOf(item);
					list.Add(new KeyValuePair<int, string>(key, "#E6B8B7"));
				}
				if (list.Count<KeyValuePair<int, string>>() > 0)
				{
					EntryGrid control = this.View.GetControl<EntryGrid>("FBottomEntity");
					control.SetRowBackcolor(list);
				}
			}
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x00064FC0 File Offset: 0x000631C0
		private void GetBomResutsByCfgBomId(List<DynamicObject> bomResultDatas)
		{
			List<DynamicObject> source = (from w in bomResultDatas
			where Convert.ToInt16(w["BomLevel"]) > 0
			select w).ToList<DynamicObject>();
			List<long> list = new List<long>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult = (from x in bomResultDatas
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in from o in source
			orderby o["BomLevel"]
			select o)
			{
				DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomEntryId", 0L);
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "EntryId", 0L);
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				List<long> list3 = new List<long>();
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "CfgBomId", 0L);
				string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicValue2, "BOMCATEGORY", null);
				if (dynamicValue3 != 0L && dynamicValue4 == "1")
				{
					list2.Add(dynamicObject);
					list3.Add(dynamicValue);
					list.AddRange(list3);
					this.dgFindBom(list3, list, bomExpandResult, list2);
				}
			}
			foreach (DynamicObject item in list2)
			{
				bomResultDatas.Remove(item);
			}
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x000651DC File Offset: 0x000633DC
		private void GetBomResultsByAuxPropId(List<DynamicObject> bomResultDatas)
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, "FSAuxpropId", -1, null, null);
			if (ObjectUtils.IsNullOrEmpty(value))
			{
				return;
			}
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.Model.DataObject, "BillMaterialId", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				return;
			}
			if (dynamicValue.Count > 1)
			{
				return;
			}
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicValue.FirstOrDefault<DynamicObject>(), "BillMaterialId_Id", 0L);
			DynamicObjectCollection auxPtysByMtrId = BOMAuxPtyBulkEditServiceHepler.GetAuxPtysByMtrId(base.Context, dynamicValue2);
			List<DynamicObject> list = (from w in bomResultDatas
			where Convert.ToInt64(w["BomLevel"]) == 1L
			select w).ToList<DynamicObject>();
			List<long> list2 = new List<long>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult = (from x in bomResultDatas
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in list)
			{
				long bomEntryId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomEntryId", 0L);
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "EntryId", 0L);
				DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				DynamicObjectCollection dynamicValue5 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue4, "TreeEntity", null);
				List<long> list4 = new List<long>();
				List<DynamicObject> list5 = (from w in dynamicValue5
				where DataEntityExtend.GetDynamicValue<long>(w, "Id", 0L) == bomEntryId && DataEntityExtend.GetDynamicValue<long>(w, "AuxPropId_Id", 0L) != 0L
				select w).ToList<DynamicObject>();
				if (ListUtils.IsEmpty<DynamicObject>(list5))
				{
					list3.Add(dynamicObject);
					list4.Add(dynamicValue3);
					list2.AddRange(list4);
					this.dgFindBom(list4, list2, bomExpandResult, list3);
				}
				else
				{
					DynamicObject dynamicValue6 = DataEntityExtend.GetDynamicValue<DynamicObject>(list5.FirstOrDefault<DynamicObject>(), "AuxPropId", null);
					bool flag = true;
					foreach (DynamicObject dynamicObject2 in auxPtysByMtrId)
					{
						string dynamicValue7 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "FVALUETYPE", null);
						string dynamicValue8 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "FID", null);
						string text = "F" + dynamicValue8;
						if (dynamicValue7 == "0")
						{
							long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(value, text + "_Id", 0L);
							long dynamicValue10 = DataEntityExtend.GetDynamicValue<long>(dynamicValue6, text + "_Id", 0L);
							if (dynamicValue9 != 0L && dynamicValue9 != dynamicValue10)
							{
								flag = false;
								break;
							}
						}
						else
						{
							string text2 = string.Empty;
							string b = string.Empty;
							if (dynamicValue7 == "1")
							{
								text2 = DataEntityExtend.GetDynamicValue<string>(value, text + "_Id", null);
								b = DataEntityExtend.GetDynamicValue<string>(dynamicValue6, text + "_Id", null);
							}
							else
							{
								text2 = DataEntityExtend.GetDynamicValue<string>(value, text, null);
								b = DataEntityExtend.GetDynamicValue<string>(dynamicValue6, text, null);
							}
							if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2) && text2 != b)
							{
								flag = false;
								break;
							}
						}
					}
					if (!flag)
					{
						list3.Add(dynamicObject);
						list4.Add(dynamicValue3);
						list2.AddRange(list4);
						this.dgFindBom(list4, list2, bomExpandResult, list3);
					}
				}
			}
			foreach (DynamicObject item in list3)
			{
				bomResultDatas.Remove(item);
			}
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x000656F0 File Offset: 0x000638F0
		private void GetBomResultsByBomId(List<DynamicObject> bomResultDatas)
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, "FBomVersion", -1, null, null);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
			{
				return;
			}
			long bomId = DataEntityExtend.GetDynamicValue<long>(value, "Id", 0L);
			DynamicObjectCollection value2 = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FBillMaterialId", -1, null, null);
			DynamicObject dynamicObject = null;
			if (!ListUtils.IsEmpty<DynamicObject>(value2))
			{
				dynamicObject = DataEntityExtend.GetDynamicValue<DynamicObject>(value2.First<DynamicObject>(), "BillMaterialId", null);
			}
			long materialId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MsterId", 0L);
			long value3 = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
			string value4 = MFGBillUtil.GetValue<string>(this.Model, "FAuxCombox", -1, null, null);
			long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, dynamicValue, value3, string.IsNullOrWhiteSpace(value4) ? 0L : Convert.ToInt64(value4));
			List<DynamicObject> list = (from w in bomResultDatas
			where Convert.ToInt64(w["BomLevel"]) == 1L
			select w).ToList<DynamicObject>();
			List<long> list2 = new List<long>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult = (from x in bomResultDatas
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject2 in list)
			{
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "EntryId", 0L);
				DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "BomId", null);
				DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue3, "TreeEntity", null);
				bool value5 = MFGBillUtil.GetValue<bool>(this.Model, "FIsMultiOrg", -1, false, null);
				long masterId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "msterID", 0L);
				List<long> list4 = new List<long>();
				if (hightVersionBomKey == bomId)
				{
					bool flag;
					if (value5)
					{
						flag = dynamicValue4.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(a, "MATERIALIDCHILD", null), "msterID", 0L) == masterId && (DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == 0L || DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId));
					}
					else
					{
						flag = dynamicValue4.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(a, "MATERIALIDCHILD_Id", 0L) == materialId && (DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == 0L || DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId));
					}
					if (!flag)
					{
						list3.Add(dynamicObject2);
						list4.Add(dynamicValue2);
						list2.AddRange(list4);
						this.dgFindBom(list4, list2, bomExpandResult, list3);
					}
				}
				else
				{
					bool flag;
					if (value5)
					{
						flag = dynamicValue4.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(a, "MATERIALIDCHILD", null), "msterID", 0L) == masterId && DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId);
					}
					else
					{
						flag = dynamicValue4.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(a, "MATERIALIDCHILD_Id", 0L) == materialId && DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId);
					}
					if (!flag)
					{
						list3.Add(dynamicObject2);
						list4.Add(dynamicValue2);
						list2.AddRange(list4);
						this.dgFindBom(list4, list2, bomExpandResult, list3);
					}
				}
			}
			foreach (DynamicObject item in list3)
			{
				bomResultDatas.Remove(item);
			}
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x00065B3C File Offset: 0x00063D3C
		private List<DynamicObject> GetBomResultByFirstParent(List<DynamicObject> bomDatas, DynamicObject firstParentMtrl, DynamicObject firstParentMtrlTo)
		{
			List<DynamicObject> list = (from w in bomDatas
			where DataEntityExtend.GetDynamicValue<int>(w, "BomLevel", 0) == 0
			select w).ToList<DynamicObject>();
			List<DynamicObject> list2 = (from w in bomDatas
			where DataEntityExtend.GetDynamicValue<int>(w, "BomLevel", 0) == 1
			select w).ToList<DynamicObject>();
			if (!ObjectUtils.IsNullOrEmpty(firstParentMtrl))
			{
				list2 = (from x in list2
				where MFGToolsUtil.CompareString(DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(x, "MaterialId2", null), "Number", null), DataEntityExtend.GetDynamicValue<string>(firstParentMtrl, "Number", null)) >= 0
				select x).ToList<DynamicObject>();
			}
			if (!ObjectUtils.IsNullOrEmpty(firstParentMtrlTo))
			{
				list2 = (from y in list2
				where MFGToolsUtil.CompareString(DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(y, "MaterialId2", null), "Number", null), DataEntityExtend.GetDynamicValue<string>(firstParentMtrlTo, "Number", null)) <= 0
				select y).ToList<DynamicObject>();
			}
			foreach (DynamicObject dynamicObject in list2)
			{
				string pathEntryId = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "PathEntryId", null);
				List<DynamicObject> collection = (from o in bomDatas
				where OtherExtend.StringNullTrim(DataEntityExtend.GetDynamicValue<string>(o, "PathEntryId", null)).StartsWith(pathEntryId + "\\")
				select o).ToList<DynamicObject>();
				list.Add(dynamicObject);
				list.AddRange(collection);
			}
			return (from o in list
			orderby DataEntityExtend.GetDynamicValue<string>(o, "PathEntryId", null)
			select o).ToList<DynamicObject>();
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00065D30 File Offset: 0x00063F30
		private void FindParentBom(List<DynamicObject> bomResultDatas)
		{
			List<DynamicObject> list = (from w in bomResultDatas
			where Convert.ToInt16(w["BomLevel"]) > 0
			select w).ToList<DynamicObject>();
			List<long> list2 = new List<long>();
			List<Tuple<long, long, long>> list3 = new List<Tuple<long, long, long>>();
			foreach (DynamicObject dynamicObject in list)
			{
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId2", null);
				long useOrgId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "UseOrgId_Id", 0L);
				long msterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "MsterId", 0L);
				long auxPropId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "ParentAuxPropId_Id", 0L);
				if (!list3.Any((Tuple<long, long, long> p) => p.Item1 == msterId && p.Item2 == useOrgId && p.Item3 == auxPropId))
				{
					list3.Add(new Tuple<long, long, long>(msterId, useOrgId, auxPropId));
				}
			}
			List<DynamicObject> source = BOMServiceHelper.GetHightVersionBom(base.Context, list3).ToList<DynamicObject>();
			List<long> list4 = (from s in source
			select Convert.ToInt64(s["Id"])).ToList<long>();
			List<DynamicObject> list5 = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in from o in list
			orderby o["BomLevel"]
			select o)
			{
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "EntryId", 0L);
				if (list2.Contains(dynamicValue3))
				{
					list5.Add(dynamicObject2);
				}
				DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MaterialId2", null);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "BomId_ID", 0L);
				if (!list4.Contains(dynamicValue4) && !list2.Contains(dynamicValue3))
				{
					this.findParentBomByBomId(dynamicValue3, dynamicValue4, list2, bomResultDatas, list5);
				}
			}
			foreach (DynamicObject item in list5)
			{
				bomResultDatas.Remove(item);
			}
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x0006601C File Offset: 0x0006421C
		private void findParentBomByBomId(long rowId, long bomId, List<long> bomRowIds, List<DynamicObject> bomExpandResult, List<DynamicObject> removeBomDatas)
		{
			List<DynamicObject> source = (from w in bomExpandResult
			where DataEntityExtend.GetDynamicValue<long>(w, "ParentEntryId", 0L) == rowId
			select w).ToList<DynamicObject>();
			List<long> list = new List<long>();
			List<DynamicObject> list2 = (from s in source
			select DataEntityExtend.GetDynamicValue<DynamicObject>(s, "BomId", null)).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list2)
			{
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "TreeEntity", null);
				if (!dynamicValue.Any((DynamicObject w) => DataEntityExtend.GetDynamicValue<long>(w, "BOMID_Id", 0L) == bomId))
				{
					long parentybomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
					List<long> collection = (from w in source
					where DataEntityExtend.GetDynamicValue<long>(w, "BomId_Id", 0L) == parentybomId
					select w into s
					select DataEntityExtend.GetDynamicValue<long>(s, "EntryId", 0L)).ToList<long>();
					list.AddRange(collection);
				}
			}
			if (ListUtils.IsEmpty<long>(list))
			{
				return;
			}
			bomRowIds.AddRange(list);
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult2 = (from x in bomExpandResult
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			this.dgFindBom(list, bomRowIds, bomExpandResult2, removeBomDatas);
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x000661D8 File Offset: 0x000643D8
		private void dgFindBom(List<long> rowIds, List<long> bomRowIds, Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult, List<DynamicObject> removeBomDatas)
		{
			foreach (long key in rowIds)
			{
				List<long> list = new List<long>();
				IGrouping<long, DynamicObject> grouping;
				if (bomExpandResult.TryGetValue(key, out grouping))
				{
					removeBomDatas.AddRange(grouping);
					list = (from x in grouping.ToList<DynamicObject>()
					select DataEntityExtend.GetDynamicValue<long>(x, "EntryId", 0L)).ToList<long>();
					list = list.Except(rowIds).ToList<long>();
				}
				bomRowIds.AddRange(list);
				this.dgFindBom(list, bomRowIds, bomExpandResult, removeBomDatas);
			}
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00066288 File Offset: 0x00064488
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
				list = this.SortDynamicObjectSource(this.bomPrintItems);
			}
			e.DataObjects = MFGCommonUtil.ReflushDynamicObjectTypeSource(this.View, businessInfo, e.DataSourceId, e.DynamicObjectType, list);
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0006635C File Offset: 0x0006455C
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
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
					list3 = this.AddSubNode(dynamicObjectItemValue, sourceGroupDynamicObject, list3);
					list.AddRange(list3);
				}
			}
			return list;
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x000664AC File Offset: 0x000646AC
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

		// Token: 0x06000889 RID: 2185 RVA: 0x00066500 File Offset: 0x00064700
		private List<DynamicObject> AddSubNode(string entryId, Dictionary<string, IGrouping<string, DynamicObject>> sourceGroupDynamicObject, List<DynamicObject> tempResultDynamicObjects)
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
				this.AddSubNode(entryId, sourceGroupDynamicObject, tempResultDynamicObjects);
			}
			return tempResultDynamicObjects;
		}

		// Token: 0x040003C4 RID: 964
		protected const string FiledKey_FBomVersion = "FBomVersion";

		// Token: 0x040003C5 RID: 965
		private const string EntityKey_BomHeadEntity = "FTopEntity";

		// Token: 0x040003C6 RID: 966
		private const string EntityKey_BomChildEntity = "FBottomEntity";

		// Token: 0x040003C7 RID: 967
		private const string FieldKey_Auxpty = "FAuxPropId";

		// Token: 0x040003C8 RID: 968
		private const string FieldKey_MaterialId = "FMATERIALIDCHILD";

		// Token: 0x040003C9 RID: 969
		private const string FieldKey_FExpandLevel = "FExpandLevel";

		// Token: 0x040003CA RID: 970
		private const string FieldKey_FValidDate = "FValidDate";

		// Token: 0x040003CB RID: 971
		private const string FieldKey_FREPLACEID = "FREPLACEID";

		// Token: 0x040003CC RID: 972
		private const string FieldKey_FBomEntryId = "FBomEntryId";

		// Token: 0x040003CD RID: 973
		private const string FieldKey_FBomChildMaterialId = "FBomChildMaterialId";

		// Token: 0x040003CE RID: 974
		private const string FieldKey_FBomId = "FBomId";

		// Token: 0x040003CF RID: 975
		private const string FieldKey_FBomUseOrgId = "FBomUseOrgId";

		// Token: 0x040003D0 RID: 976
		private const string FieldKey_FBillMaterialId = "FBillMaterialId";

		// Token: 0x040003D1 RID: 977
		private const string FieldKey_FAuxCombox = "FAuxCombox";

		// Token: 0x040003D2 RID: 978
		private const string FieldKey_FSAuxpropId = "FSAuxpropId";

		// Token: 0x040003D3 RID: 979
		private const string FieldKey_FIsAuxpropFilter = "FIsAuxpropFilter";

		// Token: 0x040003D4 RID: 980
		private const int Const_TreeEntityLoadCount = 1000;

		// Token: 0x040003D5 RID: 981
		protected const string FormKey_MaterialTree = "MFG_MaterialTree";

		// Token: 0x040003D6 RID: 982
		protected const string Key_Contain = "FTreePanel";

		// Token: 0x040003D7 RID: 983
		private const string ORM_EntityBomChild = "BomChild";

		// Token: 0x040003D8 RID: 984
		private const string ORM_FieldAuxpty = "AuxPropId";

		// Token: 0x040003D9 RID: 985
		protected const string ControlKey_tab = "FTABBOTTOM";

		// Token: 0x040003DA RID: 986
		protected BomQueryOption bomQueryOption;

		// Token: 0x040003DB RID: 987
		protected FilterParameter filterParam;

		// Token: 0x040003DC RID: 988
		protected List<DynamicObject> bomQueryChildItems = new List<DynamicObject>();

		// Token: 0x040003DD RID: 989
		protected List<DynamicObject> BomEntryInfos = new List<DynamicObject>();

		// Token: 0x040003DE RID: 990
		private Dictionary<string, decimal> convertRate = new Dictionary<string, decimal>();

		// Token: 0x040003DF RID: 991
		private int _curRow;

		// Token: 0x040003E0 RID: 992
		private List<NetworkCtrlResult> networkCtrlResults;

		// Token: 0x040003E1 RID: 993
		private int curTabindex;
	}
}
