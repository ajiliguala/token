using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomCost;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.MaterialTree;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000078 RID: 120
	[Description("BOM成本查询")]
	public class BomQueryCost : AbstractDynamicFormPlugIn
	{
		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060008BB RID: 2235 RVA: 0x00066899 File Offset: 0x00064A99
		protected string MaterialTree_PageId
		{
			get
			{
				return this._materialTree_PageId;
			}
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x000668A4 File Offset: 0x00064AA4
		public BomQueryCost()
		{
			this._materialTree_PageId = SequentialGuid.NewGuid().ToString();
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060008BD RID: 2237 RVA: 0x000668F1 File Offset: 0x00064AF1
		// (set) Token: 0x060008BE RID: 2238 RVA: 0x000668F9 File Offset: 0x00064AF9
		public object FMaterialId { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x00066902 File Offset: 0x00064B02
		// (set) Token: 0x060008C0 RID: 2240 RVA: 0x0006690A File Offset: 0x00064B0A
		public object FUseOrgId { get; set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x00066913 File Offset: 0x00064B13
		// (set) Token: 0x060008C2 RID: 2242 RVA: 0x0006691B File Offset: 0x00064B1B
		public List<DynamicObject> lstBomHeadData { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060008C3 RID: 2243 RVA: 0x00066924 File Offset: 0x00064B24
		// (set) Token: 0x060008C4 RID: 2244 RVA: 0x0006692C File Offset: 0x00064B2C
		private string costQtyParam { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060008C5 RID: 2245 RVA: 0x00066935 File Offset: 0x00064B35
		// (set) Token: 0x060008C6 RID: 2246 RVA: 0x0006693D File Offset: 0x00064B3D
		public List<DynamicObject> bomDataEntity { get; set; }

		// Token: 0x060008C7 RID: 2247 RVA: 0x00066948 File Offset: 0x00064B48
		public override void OnInitialize(InitializeEventArgs e)
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
			this.memBomExpandOption.Mode = 1;
			SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer");
			splitContainer.HideFirstPanel(true);
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00066A18 File Offset: 0x00064C18
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
			string systemProfile = MFGServiceHelper.GetSystemProfile<string>(base.Context, value, "MFG_EngParameter", "CostQtyParam", null);
			this.costQtyParam = ((!string.IsNullOrWhiteSpace(systemProfile)) ? systemProfile : MFGServiceHelper.GetSystemProfile<string>(base.Context, 0L, "MFG_EngParameter", "CostQtyParam", null));
			Control control = this.View.GetControl("FShouldQty");
			if (control != null)
			{
				control.Visible = false;
			}
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x00066AA4 File Offset: 0x00064CA4
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FREFRESH"))
				{
					return;
				}
				this.FillBomChildData();
				this.GetOrderPrice();
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "CostType", null);
				bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.ParameterData, "PurPriceCheck", false);
				this.CountCost(dynamicValue, dynamicValue2);
			}
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x00066B1C File Offset: 0x00064D1C
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FBOTTOMENTITY"))
				{
					if (!(a == "FTOPENTITY"))
					{
						return;
					}
					bool value = MFGBillUtil.GetValue<bool>(this.Model, "FExpandStyle", -1, false, null);
					if (e.Row < 0)
					{
						return;
					}
					if (value)
					{
						long value2 = MFGBillUtil.GetValue<long>(this.Model, "FBomId", e.Row, 0L, null);
						this.View.Model.SetValue("FBomVersion", value2);
						this.FillBomChildData();
						this.GetOrderPrice();
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "CostType", null);
						FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BomQueryCostParam", true);
						DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, "ENG_BomQueryCostParam", "UserParameter");
						bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "PurPriceCheck", false);
						this.CountCost(dynamicValue, dynamicValue2);
					}
					if (StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryCost"))
					{
						this.FillBomCOBYData();
					}
				}
				else
				{
					long value3 = MFGBillUtil.GetValue<long>(this.Model, "FBomId2", e.Row, 0L, null);
					this.bomDataEntity = this.GetBomCOBYData(value3);
					if (this.bomDataEntity.Count > 0)
					{
						this.View.ShowMessage(ResManager.LoadKDString("该层BOM存在联副产品，成本计算可能不准确", "015072000025784", 7, new object[0]), 0);
						return;
					}
				}
			}
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x00066D28 File Offset: 0x00064F28
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBFILTER"))
				{
					return;
				}
				MFGBillUtil.ShowFilterForm(this.View, this.GetBillName(), null, delegate(FormResult filterResult)
				{
					if (filterResult.ReturnData is FilterParameter)
					{
						this.filterParam = (filterResult.ReturnData as FilterParameter);
						this.FillBomHeadData();
						SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer");
						splitContainer.HideFirstPanel(false);
						BomQueryCommonFuncs.ShowOrHideField(this.View, this.filterParam);
					}
				}, this.GetFilterName(), 0);
				e.Cancel = true;
			}
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x00066DB4 File Offset: 0x00064FB4
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

		// Token: 0x060008CD RID: 2253 RVA: 0x00066F2C File Offset: 0x0006512C
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
				if (!(a == "FTAB_P1"))
				{
					return;
				}
				this.FillBomCOBYData();
			}
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00066FBC File Offset: 0x000651BC
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (this.View.BillBusinessInfo != null)
			{
				TabControl control = this.View.GetControl<TabControl>("FTab");
				if (control != null)
				{
					control.SetFireSelChanged(true);
				}
			}
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x00066FF8 File Offset: 0x000651F8
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FBillMaterialId"))
				{
					if (key == "FBomUseOrgId")
					{
						if (e.NewValue == null || !e.NewValue.Equals(e.OldValue))
						{
							this.Model.DeleteEntryData("FBottomEntity");
							this.View.UpdateView("FBottomEntity");
						}
						this.View.Model.SetValue("FBillMaterialId", null);
						this.View.Model.SetValue("FBomVersion", null);
						string systemProfile = MFGServiceHelper.GetSystemProfile<string>(base.Context, OtherExtend.ConvertTo<long>(e.NewValue, 0L), "MFG_EngParameter", "CostQtyParam", null);
						this.costQtyParam = ((!string.IsNullOrWhiteSpace(systemProfile)) ? systemProfile : MFGServiceHelper.GetSystemProfile<string>(base.Context, 0L, "MFG_EngParameter", "CostQtyParam", "1"));
						return;
					}
					if (!(key == "FExpandStyle"))
					{
						return;
					}
					EntryGrid control = this.View.GetControl<EntryGrid>("FTopEntity");
					if (control != null)
					{
						control.SetFireDoubleClickEvent(Convert.ToBoolean(e.NewValue));
					}
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
					long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], value, 0L);
					this.View.Model.SetValue("FBomVersion", hightVersionBomKey);
					return;
				}
			}
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x000671A8 File Offset: 0x000653A8
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

		// Token: 0x060008D1 RID: 2257 RVA: 0x00067240 File Offset: 0x00065440
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.BaseDataField.Key.Equals("FBomVersion"))
			{
				e.IsShowApproved = false;
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FBillMaterialId", -1, 0L, null);
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

		// Token: 0x060008D2 RID: 2258 RVA: 0x00067319 File Offset: 0x00065519
		protected virtual string GetBillName()
		{
			return "ENG_BomQueryCost";
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x00067320 File Offset: 0x00065520
		protected virtual string GetFilterName()
		{
			return "ENG_BomQueryCost_Filter";
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00067328 File Offset: 0x00065528
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

		// Token: 0x060008D5 RID: 2261 RVA: 0x000673A4 File Offset: 0x000655A4
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

		// Token: 0x060008D6 RID: 2262 RVA: 0x0006741C File Offset: 0x0006561C
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

		// Token: 0x060008D7 RID: 2263 RVA: 0x0006752A File Offset: 0x0006572A
		protected virtual List<DynamicObject> GetBomHeadData()
		{
			return BomQueryServiceHelper.GetBomQueryItems(base.Context, this.filterParam, this.View.BillBusinessInfo.GetForm().Id);
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00067554 File Offset: 0x00065754
		protected void FillBomDataByClick(List<DynamicObject> chooseBom)
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FTopEntity");
			this.Model.DeleteEntryData("FBottomEntity");
			if (this.View.BusinessInfo.GetElement("FCobyEntity") != null)
			{
				this.Model.DeleteEntryData("FCobyEntity");
			}
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTopEntity");
			if (chooseBom != null && chooseBom.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject in chooseBom)
				{
					dynamicObject["SEQ"] = chooseBom.IndexOf(dynamicObject) + 1;
					entityDataObject.Add(dynamicObject);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FTopEntity");
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x00067670 File Offset: 0x00065870
		protected virtual void FillBomChildData()
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = this.BomQueryExpandOption();
			this.Model.DeleteEntryData("FBottomEntity");
			List<DynamicObject> list = this.BuildBomExpandSourceData();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当前物料对应的BOM不存在，请确认！", "015072000003341", 7, new object[0]), ResManager.LoadKDString("BOM不存在！", "015072000002208", 7, new object[0]), 0);
				return;
			}
			List<DynamicObject> list2 = this.GetBomChildData(list, memBomExpandOption_ForPSV);
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				this.View.ShowErrMessage("BOM展开异常，请通过【物料清单正查】查看BOM是否能正确展开！", "", 0);
				return;
			}
			this.bomQueryChildItems = list2;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "CostType", null);
			if (StringUtils.EqualsIgnoreCase(dynamicValue, "1"))
			{
				list2 = (from x in list2
				where DataEntityExtend.GetDynamicValue<string>(x, "MaterialType", null) != "3"
				select x).ToList<DynamicObject>();
			}
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(this.View.Model.DataObject, "BomUseOrgId_Id", 0L);
			JSONObject defCurrencyAndExchangeTypeByBizOrgID = FINServiceHelperForCommon.GetDefCurrencyAndExchangeTypeByBizOrgID(base.Context, dynamicValue2, 0L);
			if (ListUtils.IsEmpty<KeyValuePair<string, object>>(defCurrencyAndExchangeTypeByBizOrgID))
			{
				this.View.ShowErrMessage("物料清单所在使用组织的核算体系未设置本位币！", "", 0);
				return;
			}
			object obj;
			if (!defCurrencyAndExchangeTypeByBizOrgID.TryGetValue("FCyForID", out obj))
			{
				this.View.ShowErrMessage("物料清单所在使用组织的核算体系未设置本位币！", "", 0);
				return;
			}
			foreach (DynamicObject dynamicObject in list2)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "CurrId_Id", obj);
			}
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DBServiceHelper.LoadReferenceObject(this.View.Context, list2.ToArray(), entryEntity.DynamicObjectType, false);
			IEnumerable<DynamicObject> enumerable = list2;
			this.bomPrintChildItems = list2;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			foreach (DynamicObject item in enumerable)
			{
				entityDataObject.Add(item);
			}
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x000678B8 File Offset: 0x00065AB8
		protected virtual MemBomExpandOption_ForPSV BomQueryExpandOption()
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = new MemBomExpandOption_ForPSV();
			memBomExpandOption_ForPSV.ExpandLevelTo = DataEntityExtend.GetDynamicValue<int>(this.Model.DataObject, "ExpandLevel", 0);
			memBomExpandOption_ForPSV.ExpandVirtualMaterial = false;
			memBomExpandOption_ForPSV.DeleteVirtualMaterial = false;
			memBomExpandOption_ForPSV.DeleteSkipRow = false;
			memBomExpandOption_ForPSV.ExpandSkipRow = false;
			memBomExpandOption_ForPSV.IsShowOutSource = false;
			memBomExpandOption_ForPSV.BomExpandId = SequentialGuid.NewGuid().ToString();
			memBomExpandOption_ForPSV.ParentCsdYieldRate = true;
			memBomExpandOption_ForPSV.ChildCsdYieldRate = true;
			memBomExpandOption_ForPSV.Mode = 1;
			memBomExpandOption_ForPSV.ValidDate = new DateTime?(DataEntityExtend.GetDynamicValue<DateTime>(this.Model.DataObject, "ValidDate", default(DateTime)));
			memBomExpandOption_ForPSV.BomExpandCalType = 0;
			memBomExpandOption_ForPSV.IsConvertUnitQty = true;
			memBomExpandOption_ForPSV.isBomCost = true;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "CostType", null);
			if (StringUtils.EqualsIgnoreCase(dynamicValue, "1"))
			{
				memBomExpandOption_ForPSV.CsdSubstitution = false;
				memBomExpandOption_ForPSV.IsExpandSubMtrl = false;
			}
			else
			{
				memBomExpandOption_ForPSV.CsdSubstitution = true;
				memBomExpandOption_ForPSV.IsExpandSubMtrl = true;
			}
			return memBomExpandOption_ForPSV;
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x00067A34 File Offset: 0x00065C34
		private void GetOrderPrice()
		{
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "BomUseOrgId_Id", 0L);
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			PurReferPriceArg purReferPriceArg = new PurReferPriceArg();
			long systemProfile = MFGServiceHelper.GetSystemProfile<long>(base.Context, dynamicValue, "MFG_EngParameter", "DateScope", 0L);
			long dateScope = (systemProfile != 0L) ? systemProfile : MFGServiceHelper.GetSystemProfile<long>(base.Context, 0L, "MFG_EngParameter", "DateScope", 180L);
			MFGServiceHelper.GetSystemProfile<string>(base.Context, dynamicValue, "MFG_EngParameter", "PriceField", "");
			string systemProfile2 = MFGServiceHelper.GetSystemProfile<string>(base.Context, dynamicValue, "MFG_EngParameter", "SourceBill", "");
			string text = (!string.IsNullOrWhiteSpace(systemProfile2)) ? systemProfile2 : MFGServiceHelper.GetSystemProfile<string>(base.Context, 0L, "MFG_EngParameter", "SourceBill", "ORDER");
			purReferPriceArg.DateScope = dateScope;
			purReferPriceArg.DateType = "BillDate";
			purReferPriceArg.PriceType = "LastPrice";
			purReferPriceArg.SourceBill = text;
			purReferPriceArg.ExpirationDate = DataEntityExtend.GetDynamicValue<DateTime>(this.Model.DataObject, "ExpirationDate", default(DateTime));
			bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(this.Model.DataObject, "IsSubProcessFee", false);
			purReferPriceArg.IsSubProcessFee = dynamicValue2;
			List<PurReferPriceDetail> list = new List<PurReferPriceDetail>();
			PurPriceArgsPackage purPriceArgsPackage = new PurPriceArgsPackage();
			purPriceArgsPackage.BusinessType = "InvalidBusinessType";
			purPriceArgsPackage.IsIgnoreBusinessType = true;
			purPriceArgsPackage.BomUseOrgId = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "BomUseOrgId_Id", 0L);
			purPriceArgsPackage.IsSubProcessFee = DataEntityExtend.GetDynamicValue<bool>(this.Model.DataObject, "IsSubProcessFee", false);
			List<PurPriceArgs> list2 = new List<PurPriceArgs>();
			int num = 0;
			DateTime date;
			if (purReferPriceArg.ExpirationDate == DateTime.MinValue)
			{
				date = TimeServiceHelper.GetSystemDateTime(this.View.Context);
			}
			else
			{
				date = purReferPriceArg.ExpirationDate;
			}
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.Model.DataObject, "ExcSupplierMul", null);
			List<string> list3 = new List<string>();
			foreach (DynamicObject dynamicObject in dynamicObjectItemValue)
			{
				list3.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ExcSupplierMul_Id", null));
			}
			if (!ListUtils.IsEmpty<string>(list3))
			{
				purReferPriceArg.ExcSupplierIds = (purPriceArgsPackage.ExcSupplierIds = new HashSet<string>(list3));
			}
			Dictionary<int, long> dictionary = new Dictionary<int, long>();
			foreach (DynamicObject dynamicObject2 in entityDataObject)
			{
				PurReferPriceDetail purReferPriceDetail = new PurReferPriceDetail();
				DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MaterialId", null);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicValue3, "UseOrgId_Id", 0L);
				long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SupplyOrgId_Id", 0L);
				long dynamicValue6 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "AuxPropId_Id", 0L);
				DynamicObject dynamicValue7 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "AuxPropId", null);
				long purchaseOrgId = (dynamicValue5 == 0L) ? dynamicValue4 : dynamicValue5;
				purReferPriceDetail.AuxPropId = dynamicValue6;
				purReferPriceDetail.AuxProp = dynamicValue7;
				purReferPriceDetail.CurrencyId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "CurrId_Id", 0L);
				if (dynamicValue5 != 0L && dynamicValue4 != dynamicValue5)
				{
					long materialID = BomQueryCostServiceHelper.GetMaterialID(base.Context, DataEntityExtend.GetDynamicValue<long>(dynamicValue3, "msterId", 0L), dynamicValue5);
					purReferPriceDetail.MaterialId = materialID;
					dictionary.Add(num, materialID);
				}
				else
				{
					purReferPriceDetail.MaterialId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialId_Id", 0L);
				}
				purReferPriceDetail.PurchaseOrgId = purchaseOrgId;
				purReferPriceDetail.Row = num;
				PurPriceArgs purPriceArgs = new PurPriceArgs();
				purPriceArgs.Date = date;
				if (!this.costQtyParam.Equals("1"))
				{
					purPriceArgs.Qty = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "ActualQty", 0m);
				}
				else
				{
					purPriceArgs.Qty = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "Qty", 0m);
				}
				purPriceArgs.MaterialId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialId_Id", 0L);
				purPriceArgs.MaterialMasterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue3, "msterId", 0L);
				purPriceArgs.PurchaseOrgId = purchaseOrgId;
				purPriceArgs.AuxPropId = dynamicValue6;
				purPriceArgs.AuxProp = dynamicValue7;
				purPriceArgs.LocalCurrId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "CurrId_Id", 0L);
				purPriceArgs.Row = num;
				num++;
				list2.Add(purPriceArgs);
				list.Add(purReferPriceDetail);
			}
			purPriceArgsPackage.PriceArgs = list2;
			purReferPriceArg.PurReferPriceDetails = list;
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary2 = new Dictionary<long, IGrouping<long, DynamicObject>>();
			if (dictionary.Count > 0)
			{
				object[] array = (from x in dictionary
				select x.Value).ToArray<object>();
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BD_MATERIAL");
				DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, array, formMetaData.BusinessInfo.GetDynamicObjectType());
				if (!ListUtils.IsEmpty<DynamicObject>(array2))
				{
					dictionary2 = (from x in array2
					group x by DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
				}
			}
			if (StringUtils.EqualsIgnoreCase(text, "PRICECATEGORY"))
			{
				List<PurPriceResult> purchaseLocCurrPriceList = BomQueryCostServiceHelper.GetPurchaseLocCurrPriceList(base.Context, purPriceArgsPackage);
				if (ListUtils.IsEmpty<PurPriceResult>(purchaseLocCurrPriceList))
				{
					return;
				}
				using (List<PurPriceResult>.Enumerator enumerator2 = purchaseLocCurrPriceList.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						PurPriceResult purPriceResult = enumerator2.Current;
						decimal num2;
						decimal num3;
						if (purPriceResult.UnitId != DataEntityExtend.GetDynamicValue<long>(entityDataObject[purPriceResult.Row], "UnitId_Id", 0L) && purPriceResult.UnitId != 0L)
						{
							UnitConvert unitOrConvertRate = this.GetUnitOrConvertRate(entityDataObject[purPriceResult.Row], (long)purPriceResult.Row, purPriceResult.UnitId);
							num2 = purPriceResult.TaxPrice * unitOrConvertRate.ConvertNumerator / unitOrConvertRate.ConvertDenominator;
							num3 = purPriceResult.Price * unitOrConvertRate.ConvertNumerator / unitOrConvertRate.ConvertDenominator;
							purPriceResult.SubPrice * unitOrConvertRate.ConvertNumerator / unitOrConvertRate.ConvertDenominator;
							purPriceResult.SubTaxPrice * unitOrConvertRate.ConvertNumerator / unitOrConvertRate.ConvertDenominator;
						}
						else
						{
							num2 = purPriceResult.TaxPrice;
							num3 = purPriceResult.Price;
							decimal subPrice = purPriceResult.SubPrice;
							decimal subTaxPrice = purPriceResult.SubTaxPrice;
						}
						this.Model.BeginIniti();
						this.Model.SetValue("FTaxRate", purPriceResult.TaxRate, purPriceResult.Row);
						this.Model.SetValue("FPrice", num3, purPriceResult.Row);
						this.Model.SetValue("FTaxPrice", num2, purPriceResult.Row);
						this.Model.SetValue("FCCurrid", purPriceResult.CurrencyId, purPriceResult.Row);
						this.Model.SetValue("FPriceSource", "采购价目表", purPriceResult.Row);
						this.Model.SetValue("FListSupplierId", purPriceResult.ListSupplierId, purPriceResult.Row);
						this.Model.SetValue("SubPrice", purPriceResult.SubPrice, purPriceResult.Row);
						this.Model.SetValue("SubTaxPrice", purPriceResult.SubTaxPrice, purPriceResult.Row);
						this.Model.SetValue("SubPriceType", "PRICECATEGORY", purPriceResult.Row);
						this.Model.EndIniti();
					}
					goto IL_11A1;
				}
			}
			List<PurReferPriceResult> purLocCurrReferPrice = BomQueryCostServiceHelper.GetPurLocCurrReferPrice(base.Context, purReferPriceArg);
			List<PurPriceResult> purchaseLocCurrPriceList2 = BomQueryCostServiceHelper.GetPurchaseLocCurrPriceList(base.Context, purPriceArgsPackage);
			if (ListUtils.IsEmpty<PurReferPriceResult>(purLocCurrReferPrice) && ListUtils.IsEmpty<PurPriceResult>(purchaseLocCurrPriceList2))
			{
				return;
			}
			Dictionary<int, PurPriceResult> dictionary3 = new Dictionary<int, PurPriceResult>();
			Dictionary<int, PurReferPriceResult> dictionary4 = new Dictionary<int, PurReferPriceResult>();
			List<object> list4 = new List<object>();
			if (!ListUtils.IsEmpty<PurReferPriceResult>(purLocCurrReferPrice))
			{
				dictionary4 = purLocCurrReferPrice.ToDictionary((PurReferPriceResult x) => x.Row);
				IEnumerable<object> collection = (from x in purLocCurrReferPrice
				where x.ListSupplierId > 0L
				select x.ListSupplierId).ToList<object>();
				list4.AddRange(collection);
			}
			if (!ListUtils.IsEmpty<PurPriceResult>(purchaseLocCurrPriceList2))
			{
				dictionary3 = purchaseLocCurrPriceList2.ToDictionary((PurPriceResult x) => x.Row);
				IEnumerable<object> collection2 = (from x in purchaseLocCurrPriceList2
				where x.ListSupplierId > 0L
				select x.ListSupplierId).ToList<object>();
				list4.AddRange(collection2);
			}
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary5 = null;
			if (!ListUtils.IsEmpty<object>(list4))
			{
				DynamicObject[] source = BusinessDataServiceHelper.Load(base.Context, list4.Distinct<object>().ToArray<object>(), (MetaDataServiceHelper.Load(base.Context, "BD_Supplier", true) as FormMetadata).BusinessInfo.GetDynamicObjectType());
				dictionary5 = (from g in source
				group g by OrmUtils.GetPrimaryKeyValue<long>(g, true)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			}
			foreach (DynamicObject dynamicObject3 in entityDataObject)
			{
				int key = entityDataObject.IndexOf(dynamicObject3);
				long dynamicValue8 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "UnitId_Id", 0L);
				PurReferPriceResult purReferPriceResult = null;
				if (dictionary4.TryGetValue(key, out purReferPriceResult))
				{
					if (purReferPriceResult.ReferPrice == 0m)
					{
						if (StringUtils.EqualsIgnoreCase(text, "MTRREFERENCE"))
						{
							continue;
						}
						PurPriceResult purPriceResult2 = null;
						if (dictionary3.TryGetValue(key, out purPriceResult2))
						{
							decimal num4;
							decimal num5;
							if (purPriceResult2.UnitId != dynamicValue8 && purPriceResult2.UnitId != 0L)
							{
								UnitConvert unitOrConvertRate2 = this.GetUnitOrConvertRate(dynamicObject3, (long)purPriceResult2.Row, purPriceResult2.UnitId);
								num4 = purPriceResult2.Price * unitOrConvertRate2.ConvertNumerator / unitOrConvertRate2.ConvertDenominator;
								num5 = purPriceResult2.TaxPrice * unitOrConvertRate2.ConvertNumerator / unitOrConvertRate2.ConvertDenominator;
							}
							else
							{
								num4 = purPriceResult2.Price;
								num5 = purPriceResult2.TaxPrice;
							}
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Price", num4);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxPrice", num5);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "CurrId_Id", purPriceResult2.CurrencyId);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxRate", purPriceResult2.TaxRate);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId_Id", purPriceResult2.ListSupplierId);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPriceType", purPriceResult2.Row);
							if (purPriceResult2.ListSupplierId > 0L && dictionary5 != null)
							{
								IGrouping<long, DynamicObject> source2 = null;
								if (dictionary5.TryGetValue(purPriceResult2.ListSupplierId, out source2))
								{
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId", source2.FirstOrDefault<DynamicObject>());
								}
							}
						}
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PriceSource", "采购价目表");
					}
					else
					{
						if (purReferPriceResult.PriceUnitId != DataEntityExtend.GetDynamicValue<long>(entityDataObject[purReferPriceResult.Row], "UnitId_Id", 0L) && purReferPriceResult.PriceUnitId != 0L)
						{
							UnitConvert unitOrConvertRate3 = this.GetUnitOrConvertRate(entityDataObject[purReferPriceResult.Row], (long)purReferPriceResult.Row, purReferPriceResult.PriceUnitId);
							decimal num6 = purReferPriceResult.ReferPrice * unitOrConvertRate3.ConvertNumerator / unitOrConvertRate3.ConvertDenominator;
							decimal num7 = purReferPriceResult.ReferTaxPrice * unitOrConvertRate3.ConvertNumerator / unitOrConvertRate3.ConvertDenominator;
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Price", num6);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxPrice", num7);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "CurrId_Id", purReferPriceResult.ReferCurrencyId);
						}
						else
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Price", purReferPriceResult.ReferPrice);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxPrice", purReferPriceResult.ReferTaxPrice);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "CurrId_Id", purReferPriceResult.ReferCurrencyId);
						}
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxRate", purReferPriceResult.TaxRate);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PriceSource", purReferPriceResult.PriceSource);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId_Id", purReferPriceResult.ListSupplierId);
						if (purReferPriceResult.ListSupplierId > 0L && dictionary5 != null)
						{
							IGrouping<long, DynamicObject> source3 = null;
							if (dictionary5.TryGetValue(purReferPriceResult.ListSupplierId, out source3))
							{
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId", source3.FirstOrDefault<DynamicObject>());
							}
						}
					}
					if (dynamicValue2 && purReferPriceResult.SubPrice == 0m)
					{
						if (!StringUtils.EqualsIgnoreCase(text, "MTRREFERENCE"))
						{
							PurPriceResult purPriceResult3 = null;
							if (dictionary3.TryGetValue(key, out purPriceResult3) && !(purPriceResult3.SubPrice == 0m))
							{
								decimal num8;
								decimal num9;
								if (purPriceResult3.UnitId != dynamicValue8 && purPriceResult3.UnitId != 0L)
								{
									UnitConvert unitOrConvertRate4 = this.GetUnitOrConvertRate(dynamicObject3, (long)purPriceResult3.Row, purPriceResult3.UnitId);
									num8 = purPriceResult3.SubPrice * unitOrConvertRate4.ConvertNumerator / unitOrConvertRate4.ConvertDenominator;
									num9 = purPriceResult3.SubTaxPrice * unitOrConvertRate4.ConvertNumerator / unitOrConvertRate4.ConvertDenominator;
								}
								else
								{
									num8 = purPriceResult3.SubPrice;
									num9 = purPriceResult3.SubTaxPrice;
								}
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPrice", num8);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubTaxPrice", num9);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPriceType", "PRICECATEGORY");
							}
						}
					}
					else
					{
						if (purReferPriceResult.PriceUnitId != DataEntityExtend.GetDynamicValue<long>(entityDataObject[purReferPriceResult.Row], "UnitId_Id", 0L) && purReferPriceResult.PriceUnitId != 0L)
						{
							UnitConvert unitOrConvertRate5 = this.GetUnitOrConvertRate(entityDataObject[purReferPriceResult.Row], (long)purReferPriceResult.Row, purReferPriceResult.PriceUnitId);
							decimal num10 = purReferPriceResult.SubPrice * unitOrConvertRate5.ConvertNumerator / unitOrConvertRate5.ConvertDenominator;
							decimal num11 = purReferPriceResult.SubTaxPrice * unitOrConvertRate5.ConvertNumerator / unitOrConvertRate5.ConvertDenominator;
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPrice", num10);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubTaxPrice", num11);
						}
						else
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPrice", purReferPriceResult.SubPrice);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubTaxPrice", purReferPriceResult.SubTaxPrice);
						}
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPriceType", purReferPriceResult.SubPriceType);
					}
				}
				else if (!StringUtils.EqualsIgnoreCase(text, "MTRREFERENCE") && !ListUtils.IsEmpty<PurPriceResult>(purchaseLocCurrPriceList2))
				{
					PurPriceResult purPriceResult4 = null;
					if (dictionary3.TryGetValue(key, out purPriceResult4))
					{
						decimal num12;
						decimal num13;
						decimal num14;
						decimal num15;
						if (purPriceResult4.UnitId != dynamicValue8 && purPriceResult4.UnitId != 0L)
						{
							UnitConvert unitOrConvertRate6 = this.GetUnitOrConvertRate(dynamicObject3, (long)purPriceResult4.Row, purPriceResult4.UnitId);
							num12 = purPriceResult4.SubPrice * unitOrConvertRate6.ConvertNumerator / unitOrConvertRate6.ConvertDenominator;
							num13 = purPriceResult4.SubTaxPrice * unitOrConvertRate6.ConvertNumerator / unitOrConvertRate6.ConvertDenominator;
							num14 = purPriceResult4.Price * unitOrConvertRate6.ConvertNumerator / unitOrConvertRate6.ConvertDenominator;
							num15 = purPriceResult4.TaxPrice * unitOrConvertRate6.ConvertNumerator / unitOrConvertRate6.ConvertDenominator;
						}
						else
						{
							num12 = purPriceResult4.SubPrice;
							num13 = purPriceResult4.SubTaxPrice;
							num14 = purPriceResult4.Price;
							num15 = purPriceResult4.TaxPrice;
						}
						if (dynamicValue2)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPrice", num12);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubTaxPrice", num13);
							if (num12 != 0m)
							{
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "SubPriceType", "PRICECATEGORY");
							}
						}
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId_Id", purPriceResult4.ListSupplierId);
						if (purPriceResult4.ListSupplierId > 0L && dictionary5 != null)
						{
							IGrouping<long, DynamicObject> source4 = null;
							if (dictionary5.TryGetValue(purPriceResult4.ListSupplierId, out source4))
							{
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ListSupplierId", source4.FirstOrDefault<DynamicObject>());
							}
						}
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Price", num14);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "TaxPrice", num15);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PriceSource", "采购价目表");
					}
				}
			}
			IL_11A1:
			foreach (DynamicObject dynamicObject4 in entityDataObject)
			{
				int key2 = entityDataObject.IndexOf(dynamicObject4);
				decimal dynamicValue9 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject4, "Price", 0m);
				long key3;
				IGrouping<long, DynamicObject> grouping;
				if (dynamicValue9 <= 0m && dictionary.TryGetValue(key2, out key3) && dictionary2.TryGetValue(key3, out grouping) && !ListUtils.IsEmpty<DynamicObject>(grouping))
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "MaterialId", grouping.FirstOrDefault<DynamicObject>());
				}
			}
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x00068CFC File Offset: 0x00066EFC
		private UnitConvert GetUnitOrConvertRate(DynamicObject detailDataEntitie, long row, long sourceUnitId)
		{
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(detailDataEntitie, "MaterialId", null);
			GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs();
			getUnitConvertRateArgs.PrimaryKey = row;
			getUnitConvertRateArgs.MasterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "msterId", 0L);
			getUnitConvertRateArgs.SourceUnitId = DataEntityExtend.GetDynamicValue<long>(detailDataEntitie, "UnitId_Id", 0L);
			getUnitConvertRateArgs.DestUnitId = sourceUnitId;
			return UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs);
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00068DA4 File Offset: 0x00066FA4
		private void CountCost(string costType, bool purPriceCheck)
		{
			List<string> list = new List<string>();
			List<GetUnitConvertRateArgs> parentParamList = new List<GetUnitConvertRateArgs>();
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			List<string> parentEntryIds = (from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<string>(x, "ParentEntryId", null)).Distinct<string>().ToList<string>();
			Dictionary<long, UnitConvert> unitConvert = this.GetUnitConvert(list, parentParamList, entityDataObject, parentEntryIds);
			List<DynamicObject> list2 = (from x in entityDataObject
			where !parentEntryIds.Contains(DataEntityExtend.GetDynamicValue<string>(x, "EntryId", null))
			select x).ToList<DynamicObject>();
			DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsSubProcessFee", false);
			foreach (DynamicObject dynamicObject in list2)
			{
				int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0);
				if (DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MaterialType", null).Equals("3") && costType.Equals("1"))
				{
					this.Model.BeginIniti();
					this.Model.SetValue("FSingleCost", 0, dynamicValue - 1);
					this.Model.SetValue("FPrice", 0, dynamicValue - 1);
					this.Model.SetValue("FSumTaxCost", 0.0, dynamicValue - 1);
					this.Model.SetValue("FTaxPrice", 0, dynamicValue - 1);
					this.Model.EndIniti();
				}
				else
				{
					decimal dynamicValue2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Price", 0m);
					decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "TaxPrice", 0m);
					DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null);
					DynamicObjectCollection dynamicValue5 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue4, "MaterialStock", null);
					DynamicObjectCollection dynamicValue6 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue4, "MaterialBase", null);
					long dynamicValue7 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UnitId_Id", 0L);
					long dynamicValue8 = DataEntityExtend.GetDynamicValue<long>(dynamicValue6[0], "BaseUnitId_Id", 0L);
					long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(dynamicValue4, "msterID", 0L);
					decimal dynamicValue10 = DataEntityExtend.GetDynamicValue<decimal>(dynamicValue5[0], "RefCost", 0m);
					decimal dynamicValue11;
					if (!this.costQtyParam.Equals("1"))
					{
						dynamicValue11 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Qty", 0m);
					}
					else
					{
						dynamicValue11 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "ActualQty", 0m);
					}
					string item = string.Format("{0}_{1}_{2}", dynamicValue9, dynamicValue7, dynamicValue8);
					long key = (long)list.IndexOf(item);
					UnitConvert unitConvert2;
					if (unitConvert.TryGetValue(key, out unitConvert2))
					{
						decimal value = MFGBillUtil.GetValue<decimal>(this.Model, "FBillAccuFixScrapRate", dynamicValue - 1, 0m, null);
						decimal num = value * unitConvert2.ConvertNumerator / unitConvert2.ConvertDenominator;
						this.Model.SetValue("FBillAccuFixScrapRate", num, dynamicValue - 1);
					}
					decimal num2 = 0m;
					string dynamicValue12 = DataEntityExtend.GetDynamicValue<string>(dynamicValue6[0], "ErpClsID", null);
					string dynamicValue13 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "PriceSource", null);
					DynamicObject dynamicValue14 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "ListSupplierId", null);
					if (!purPriceCheck || dynamicValue12 == "1")
					{
						num2 = dynamicValue10;
						if (!ObjectUtils.IsNullOrEmpty(unitConvert2))
						{
							num2 = dynamicValue10 * unitConvert2.ConvertDenominator / unitConvert2.ConvertNumerator;
						}
						long dynamicValue15 = DataEntityExtend.GetDynamicValue<long>(dynamicValue5[0], "CurrencyId_Id", 0L);
						long num3 = 0L;
						if (DynamicObjectUtils.Contains(dynamicObject, "CurrId") && DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "CurrId", null) != null && DynamicObjectUtils.Contains(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "CurrId", null), "Id"))
						{
							num3 = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "CurrId", null), "Id", 0L);
						}
						if (dynamicValue15 == 0L)
						{
							num2 = 0m;
						}
						if (dynamicValue15 != num3 && dynamicValue15 != 0L && (num3 != 0L & num2 != 0m))
						{
							long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FBomUseOrgId", -1, 0L, null);
							KeyValuePair<decimal, int> currencyRate = BomQueryCostServiceHelper.GetCurrencyRate(base.Context, dynamicValue15, num3, value2);
							decimal key2 = currencyRate.Key;
							int value3 = currencyRate.Value;
							if (key2 > 0m)
							{
								num2 = Math.Round(num2 * key2, value3);
							}
							else
							{
								num2 = 0m;
							}
						}
						if (num2 > 0m)
						{
							this.Model.SetValue("FPriceSource", "物料参考成本", dynamicValue - 1);
							this.Model.SetValue("FListSupplierId", 0, dynamicValue - 1);
						}
					}
					decimal num4 = num2;
					if (dynamicValue2 > 0m)
					{
						num2 = dynamicValue2;
						num4 = dynamicValue3;
						this.Model.SetValue("FPriceSource", dynamicValue13, dynamicValue - 1);
						this.Model.SetValue("FListSupplierId", dynamicValue14, dynamicValue - 1);
					}
					this.Model.SetValue("FSingleCost", num2 * dynamicValue11, dynamicValue - 1);
					this.Model.SetValue("FPrice", num2, dynamicValue - 1);
					this.Model.SetValue("FSumTaxCost", num4 * dynamicValue11, dynamicValue - 1);
					this.Model.SetValue("FTaxPrice", num4, dynamicValue - 1);
					if (num2 <= 0m)
					{
						this.Model.SetValue("FPriceSource", "", dynamicValue - 1);
						this.Model.SetValue("FListSupplierId", 0, dynamicValue - 1);
					}
				}
			}
			(from x in entityDataObject
			where DataEntityExtend.GetDynamicValue<long>(x, "BomLevel", 0L) == 0L
			select x).ToList<DynamicObject>();
			bool dynamicValue16 = DataEntityExtend.GetDynamicValue<bool>(this.Model.DataObject, "IsWgCost", false);
			if (costType != null)
			{
				if (costType == "1")
				{
					this.SummaryCost(entityDataObject, dynamicValue16, purPriceCheck);
					return;
				}
				if (costType == "2")
				{
					this.ReplaceCost(entityDataObject, dynamicValue16, purPriceCheck);
					return;
				}
				if (!(costType == "3"))
				{
					return;
				}
				this.ReplaceOrStandardMin(entityDataObject, dynamicValue16, purPriceCheck);
			}
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00069544 File Offset: 0x00067744
		private void ReplaceOrStandardMin(DynamicObjectCollection detailDataEntities, bool IsWgCost, bool purPriceCheck)
		{
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from x in detailDataEntities
			group x by DataEntityExtend.GetDynamicValue<int>(x, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> x) => x.Key);
			int num = detailDataEntities.Max((DynamicObject m) => DataEntityExtend.GetDynamicValue<int>(m, "BomLevel", 0));
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsSubProcessFee", false);
			for (int i = num; i >= 0; i--)
			{
				IGrouping<int, DynamicObject> source = null;
				dictionary.TryGetValue(i, out source);
				List<DynamicObject> list = source.ToList<DynamicObject>();
				using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject bomEntry = enumerator.Current;
						DataEntityExtend.GetDynamicValue<string>(bomEntry, "EntryId", null);
						DataEntityExtend.GetDynamicValue<string>(bomEntry, "MaterialType", null);
						int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(bomEntry, "Seq", 0);
						DataEntityExtend.GetDynamicValue<int>(bomEntry, "ReplaceGroup", 0);
						decimal num2 = 0m;
						string costQtyParam;
						if ((costQtyParam = this.costQtyParam) == null)
						{
							goto IL_18F;
						}
						if (!(costQtyParam == "2") && !(costQtyParam == "3"))
						{
							if (!(costQtyParam == "1"))
							{
								goto IL_18F;
							}
							num2 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "ActualQty", 0m);
						}
						else
						{
							num2 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(this.Model.DataObject, "Qty", 0m);
						}
						IL_1A8:
						decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "BillAccuScrapRate", 0m);
						decimal dynamicValue4 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "BillAccuFixScrapRate", 0m);
						decimal dynamicValue5 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "BILLACCUNUMERATOR", 0m);
						decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "BillAccuDenominator", 0m);
						decimal num3 = 1m;
						if (this.costQtyParam.Equals("2"))
						{
							num3 = dynamicValue5 / dynamicValue6;
						}
						else if (this.costQtyParam.Equals("3"))
						{
							num3 = dynamicValue5 * ++(dynamicValue3 * Convert.ToDecimal(0.01)) / dynamicValue6 + dynamicValue4;
						}
						if (num3 != 1m)
						{
							num2 *= num3;
						}
						this.Model.SetValue("FShouldQty", num2, detailDataEntities.IndexOf(bomEntry));
						List<DynamicObject> list2 = (from x in detailDataEntities
						where DataEntityExtend.GetDynamicValue<string>(x, "ParentEntryId", null) == DataEntityExtend.GetDynamicValue<string>(bomEntry, "EntryId", null)
						select x).ToList<DynamicObject>();
						if (ListUtils.IsEmpty<DynamicObject>(list2))
						{
							decimal dynamicValue7 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "Price", 0m);
							decimal dynamicValue8 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "TaxPrice", 0m);
							this.Model.SetValue("FSingleCost", dynamicValue7 * num2, detailDataEntities.IndexOf(bomEntry));
							this.Model.SetValue("FSumTaxCost", dynamicValue8 * num2, detailDataEntities.IndexOf(bomEntry));
							if (dynamicValue)
							{
								decimal dynamicValue9 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubPrice", 0m);
								decimal dynamicValue10 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubTaxPrice", 0m);
								this.Model.SetValue("FSubAmount", dynamicValue9 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSubTaxAmount", dynamicValue10 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSingleCost", dynamicValue7 * num2 + dynamicValue9 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSumTaxCost", dynamicValue8 * num2 + dynamicValue10 * num2, detailDataEntities.IndexOf(bomEntry));
								continue;
							}
							continue;
						}
						else
						{
							string dynamicValue11 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(bomEntry, "MaterialId", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
							if (!IsWgCost || !dynamicValue11.Equals("1"))
							{
								decimal dynamicValue12 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "Price", 0m);
								decimal dynamicValue13 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "TaxPrice", 0m);
								decimal dynamicValue14 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubPrice", 0m);
								decimal dynamicValue15 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubTaxPrice", 0m);
								if (purPriceCheck)
								{
									bool flag = false;
									if (dynamicValue12 > 0m && !dynamicValue11.Equals("1"))
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue12 * num2);
										flag = true;
									}
									if (dynamicValue13 > 0m && !dynamicValue11.Equals("1"))
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue13 * num2);
										flag = true;
									}
									if (dynamicValue)
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", dynamicValue14 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", dynamicValue15 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue12 * num2 + dynamicValue14 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue13 * num2 + dynamicValue15 * num2);
									}
									if (flag)
									{
										continue;
									}
								}
								List<IGrouping<int, DynamicObject>> list3 = (from x in list2
								group x by DataEntityExtend.GetDynamicValue<int>(x, "ReplaceGroup", 0)).ToList<IGrouping<int, DynamicObject>>();
								decimal d = 0m;
								decimal num4 = 0m;
								decimal d2 = 0m;
								decimal num5 = 0m;
								foreach (IGrouping<int, DynamicObject> source2 in list3)
								{
									if (source2.Count<DynamicObject>() > 1)
									{
										IGrouping<int, DynamicObject> grouping = (from x in source2
										where DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m) > 0m
										orderby DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m)
										group x by DataEntityExtend.GetDynamicValue<int>(x, "ReplacePriority", 0)).FirstOrDefault<IGrouping<int, DynamicObject>>();
										if (!ListUtils.IsEmpty<DynamicObject>(grouping))
										{
											decimal d3 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "Price", 0m));
											decimal d4 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m));
											d += d3;
											num4 += d4;
											decimal d5 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "TaxPrice", 0m));
											decimal d6 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "SumTaxCost", 0m));
											d2 += d5;
											num5 += d6;
										}
									}
									else
									{
										decimal dynamicValue16 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "Price", 0m);
										decimal dynamicValue17 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "SingleCost", 0m);
										decimal dynamicValue18 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "TaxPrice", 0m);
										decimal dynamicValue19 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "SumTaxCost", 0m);
										string dynamicValue20 = DataEntityExtend.GetDynamicValue<string>(source2.FirstOrDefault<DynamicObject>(), "MaterialType", null);
										if (dynamicValue20.Equals("2"))
										{
											d -= dynamicValue16;
											num4 -= dynamicValue17;
											d2 -= dynamicValue18;
											num5 -= dynamicValue19;
										}
										else
										{
											d += dynamicValue16;
											num4 += dynamicValue17;
											d2 += dynamicValue18;
											num5 += dynamicValue19;
										}
									}
								}
								this.Model.SetValue("FTaxRate", 0, dynamicValue2 - 1);
								this.Model.SetValue("FPrice", num4 / num2, dynamicValue2 - 1);
								this.Model.SetValue("FSingleCost", num4, dynamicValue2 - 1);
								this.Model.SetValue("FTaxPrice", num5 / num2, dynamicValue2 - 1);
								this.Model.SetValue("FSumTaxCost", num5, dynamicValue2 - 1);
								if (dynamicValue)
								{
									decimal dynamicValue21 = DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubPrice", 0m);
									decimal dynamicValue22 = DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubTaxPrice", 0m);
									this.Model.SetValue("FSubAmount", dynamicValue21 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSubTaxAmount", dynamicValue22 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSingleCost", num4 + dynamicValue21 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSumTaxCost", num5 + dynamicValue22 * num2, dynamicValue2 - 1);
								}
								if (num4 > 0m)
								{
									this.Model.SetValue("FPriceSource", "按BOM子项卷算", dynamicValue2 - 1);
								}
								else
								{
									this.Model.SetValue("FPriceSource", "", dynamicValue2 - 1);
								}
								this.Model.SetValue("FListSupplierId", 0, dynamicValue2 - 1);
								continue;
							}
							decimal dynamicValue23 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "Price", 0m);
							this.Model.SetValue("FSingleCost", dynamicValue23 * num2, detailDataEntities.IndexOf(bomEntry));
							if (dynamicValue23 <= 0m)
							{
								this.Model.SetValue("FPriceSource", "", dynamicValue2 - 1);
								this.Model.SetValue("FListSupplierId", 0, dynamicValue2 - 1);
							}
							decimal dynamicValue24 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "TaxPrice", 0m);
							this.Model.SetValue("FSumTaxCost", dynamicValue24 * num2, detailDataEntities.IndexOf(bomEntry));
							if (dynamicValue24 <= 0m)
							{
								this.Model.SetValue("FSumTaxCost", 0.0, dynamicValue2 - 1);
							}
							if (dynamicValue)
							{
								decimal dynamicValue25 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubPrice", 0m);
								decimal dynamicValue26 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "SubTaxPrice", 0m);
								this.Model.SetValue("FSubTaxAmount", dynamicValue26 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSubAmount", dynamicValue25 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSingleCost", dynamicValue23 * num2 + dynamicValue25 * num2, detailDataEntities.IndexOf(bomEntry));
								this.Model.SetValue("FSumTaxCost", dynamicValue24 * num2 + dynamicValue26 * num2, detailDataEntities.IndexOf(bomEntry));
								continue;
							}
							continue;
						}
						IL_18F:
						num2 = DataEntityExtend.GetDynamicValue<decimal>(bomEntry, "Qty", 0m);
						goto IL_1A8;
					}
				}
			}
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0006A3D4 File Offset: 0x000685D4
		private void ReplaceCost(DynamicObjectCollection detailDataEntities, bool IsWgCost, bool purPriceCheck)
		{
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from x in detailDataEntities
			group x by DataEntityExtend.GetDynamicValue<int>(x, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> x) => x.Key);
			int num = detailDataEntities.Max((DynamicObject m) => DataEntityExtend.GetDynamicValue<int>(m, "BomLevel", 0));
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsSubProcessFee", false);
			for (int i = num; i >= 0; i--)
			{
				IGrouping<int, DynamicObject> source = null;
				dictionary.TryGetValue(i, out source);
				List<DynamicObject> list = source.ToList<DynamicObject>();
				using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BomQueryCost.<>c__DisplayClass5a CS$<>8__locals1 = new BomQueryCost.<>c__DisplayClass5a();
						CS$<>8__locals1.bomEntry = enumerator.Current;
						DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.bomEntry, "EntryId", null);
						string materialType = DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.bomEntry, "MaterialType", null);
						int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(CS$<>8__locals1.bomEntry, "Seq", 0);
						int replaceGroup = DataEntityExtend.GetDynamicValue<int>(CS$<>8__locals1.bomEntry, "ReplaceGroup", 0);
						decimal num2 = 0m;
						string costQtyParam;
						if ((costQtyParam = this.costQtyParam) == null)
						{
							goto IL_1AE;
						}
						if (!(costQtyParam == "2") && !(costQtyParam == "3"))
						{
							if (!(costQtyParam == "1"))
							{
								goto IL_1AE;
							}
							num2 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "ActualQty", 0m);
						}
						else
						{
							num2 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(this.Model.DataObject, "Qty", 0m);
						}
						IL_1C7:
						decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "BillAccuScrapRate", 0m);
						decimal dynamicValue4 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "BillAccuFixScrapRate", 0m);
						decimal dynamicValue5 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "BILLACCUNUMERATOR", 0m);
						decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "BillAccuDenominator", 0m);
						decimal num3 = 1m;
						if (this.costQtyParam.Equals("2"))
						{
							num3 = dynamicValue5 / dynamicValue6;
						}
						else if (this.costQtyParam.Equals("3"))
						{
							num3 = dynamicValue5 * ++(dynamicValue3 * Convert.ToDecimal(0.01)) / dynamicValue6 + dynamicValue4;
						}
						if (num3 != 1m)
						{
							num2 *= num3;
						}
						this.Model.SetValue("FShouldQty", num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
						List<DynamicObject> list2 = (from x in detailDataEntities
						where DataEntityExtend.GetDynamicValue<string>(x, "ParentEntryId", null) == DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.bomEntry, "EntryId", null)
						select x).ToList<DynamicObject>();
						if (ListUtils.IsEmpty<DynamicObject>(list2))
						{
							(from x in list
							where DataEntityExtend.GetDynamicValue<int>(x, "ReplaceGroup", 0) == replaceGroup && DataEntityExtend.GetDynamicValue<string>(x, "MaterialType", null).Equals("3") && materialType != "3"
							select x).ToList<DynamicObject>();
							decimal dynamicValue7 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "Price", 0m);
							decimal dynamicValue8 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "TaxPrice", 0m);
							this.Model.SetValue("FSingleCost", dynamicValue7 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
							this.Model.SetValue("FSumTaxCost", dynamicValue8 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
							if (dynamicValue)
							{
								decimal dynamicValue9 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubPrice", 0m);
								decimal dynamicValue10 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubTaxPrice", 0m);
								this.Model.SetValue("FSubAmount", dynamicValue9 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSubTaxAmount", dynamicValue10 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSingleCost", dynamicValue7 * num2 + dynamicValue9 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSumTaxCost", dynamicValue8 * num2 + dynamicValue10 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								continue;
							}
							continue;
						}
						else
						{
							string dynamicValue11 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(CS$<>8__locals1.bomEntry, "MaterialId", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
							if (!IsWgCost || !dynamicValue11.Equals("1"))
							{
								decimal dynamicValue12 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "Price", 0m);
								decimal dynamicValue13 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "TaxPrice", 0m);
								decimal dynamicValue14 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubPrice", 0m);
								decimal dynamicValue15 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubTaxPrice", 0m);
								if (purPriceCheck)
								{
									bool flag = false;
									if (dynamicValue12 > 0m && !dynamicValue11.Equals("1"))
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue12 * num2);
										flag = true;
									}
									if (dynamicValue13 > 0m && !dynamicValue11.Equals("1"))
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue13 * num2);
										flag = true;
									}
									if (dynamicValue)
									{
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", dynamicValue14 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", dynamicValue15 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue12 * num2 + dynamicValue14 * num2);
										DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue13 * num2 + dynamicValue15 * num2);
									}
									if (flag)
									{
										continue;
									}
								}
								List<IGrouping<int, DynamicObject>> list3 = (from x in list2
								group x by DataEntityExtend.GetDynamicValue<int>(x, "ReplaceGroup", 0)).ToList<IGrouping<int, DynamicObject>>();
								decimal d = 0m;
								decimal num4 = 0m;
								decimal d2 = 0m;
								decimal num5 = 0m;
								foreach (IGrouping<int, DynamicObject> source2 in list3)
								{
									if (source2.Count<DynamicObject>() > 1)
									{
										IGrouping<int, DynamicObject> grouping = (from x in source2
										where DataEntityExtend.GetDynamicValue<string>(x, "MaterialType", null).Equals("3") && DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m) > 0m
										orderby DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m)
										group x by DataEntityExtend.GetDynamicValue<int>(x, "ReplacePriority", 0)).FirstOrDefault<IGrouping<int, DynamicObject>>();
										if (!ListUtils.IsEmpty<DynamicObject>(grouping))
										{
											decimal d3 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "Price", 0m));
											decimal d4 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "SingleCost", 0m));
											d += d3;
											num4 += d4;
											decimal d5 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "TaxPrice", 0m));
											decimal d6 = grouping.Sum((DynamicObject x) => DataEntityExtend.GetDynamicValue<decimal>(x, "SumTaxCost", 0m));
											d2 += d5;
											num5 += d6;
										}
									}
									else
									{
										decimal dynamicValue16 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "Price", 0m);
										decimal dynamicValue17 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "SingleCost", 0m);
										decimal dynamicValue18 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "TaxPrice", 0m);
										decimal dynamicValue19 = DataEntityExtend.GetDynamicValue<decimal>(source2.FirstOrDefault<DynamicObject>(), "SumTaxCost", 0m);
										string dynamicValue20 = DataEntityExtend.GetDynamicValue<string>(source2.FirstOrDefault<DynamicObject>(), "MaterialType", null);
										if (dynamicValue20.Equals("2"))
										{
											d -= dynamicValue16;
											num4 -= dynamicValue17;
											d2 -= dynamicValue18;
											num5 -= dynamicValue19;
										}
										else
										{
											d += dynamicValue16;
											num4 += dynamicValue17;
											d2 += dynamicValue18;
											num5 += dynamicValue19;
										}
									}
								}
								this.Model.SetValue("FTaxRate", 0, dynamicValue2 - 1);
								this.Model.SetValue("FPrice", num4 / num2, dynamicValue2 - 1);
								this.Model.SetValue("FSingleCost", num4, dynamicValue2 - 1);
								this.Model.SetValue("FTaxPrice", num5 / num2, dynamicValue2 - 1);
								this.Model.SetValue("FSumTaxCost", num5, dynamicValue2 - 1);
								if (dynamicValue)
								{
									decimal dynamicValue21 = DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubPrice", 0m);
									decimal dynamicValue22 = DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubTaxPrice", 0m);
									this.Model.SetValue("FSubAmount", dynamicValue21 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSubTaxAmount", dynamicValue22 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSingleCost", num4 + dynamicValue21 * num2, dynamicValue2 - 1);
									this.Model.SetValue("FSumTaxCost", num5 + dynamicValue22 * num2, dynamicValue2 - 1);
								}
								if (num4 > 0m)
								{
									this.Model.SetValue("FPriceSource", "按BOM子项卷算", dynamicValue2 - 1);
								}
								else
								{
									this.Model.SetValue("FPriceSource", "", dynamicValue2 - 1);
								}
								this.Model.SetValue("FListSupplierId", 0, dynamicValue2 - 1);
								continue;
							}
							decimal dynamicValue23 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "Price", 0m);
							this.Model.SetValue("FSingleCost", dynamicValue23 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
							if (dynamicValue23 <= 0m)
							{
								this.Model.SetValue("FPriceSource", "", dynamicValue2 - 1);
								this.Model.SetValue("FListSupplierId", 0, dynamicValue2 - 1);
							}
							decimal dynamicValue24 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "TaxPrice", 0m);
							this.Model.SetValue("FSumTaxCost", dynamicValue24 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
							if (dynamicValue24 <= 0m)
							{
								this.Model.SetValue("FSumTaxCost", 0.0, dynamicValue2 - 1);
							}
							if (dynamicValue)
							{
								decimal dynamicValue25 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubPrice", 0m);
								decimal dynamicValue26 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "SubTaxPrice", 0m);
								this.Model.SetValue("FSubAmount", dynamicValue25 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSubTaxAmount", dynamicValue26 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSingleCost", dynamicValue23 * num2 + dynamicValue25 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								this.Model.SetValue("FSumTaxCost", dynamicValue24 * num2 + dynamicValue26 * num2, detailDataEntities.IndexOf(CS$<>8__locals1.bomEntry));
								continue;
							}
							continue;
						}
						IL_1AE:
						num2 = DataEntityExtend.GetDynamicValue<decimal>(CS$<>8__locals1.bomEntry, "Qty", 0m);
						goto IL_1C7;
					}
				}
			}
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0006B1B0 File Offset: 0x000693B0
		private void SummaryCost(DynamicObjectCollection detailDataEntities, bool IsWgCost, bool purPriceCheck)
		{
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from x in detailDataEntities
			group x by DataEntityExtend.GetDynamicValue<int>(x, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> x) => x.Key);
			int num = detailDataEntities.Max((DynamicObject m) => DataEntityExtend.GetDynamicValue<int>(m, "BomLevel", 0));
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = detailDataEntities.GroupBy(delegate(DynamicObject x)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(x, "ParentEntryId", null);
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectItemValue))
				{
					return dynamicObjectItemValue;
				}
				return "root";
			}).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsSubProcessFee", false);
			for (int i = num; i >= 0; i--)
			{
				IGrouping<int, DynamicObject> source = null;
				dictionary.TryGetValue(i, out source);
				List<DynamicObject> list = source.ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject in list)
				{
					DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MaterialType", null);
					int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0);
					decimal num2 = 0m;
					string costQtyParam;
					if ((costQtyParam = this.costQtyParam) == null)
					{
						goto IL_1A1;
					}
					if (!(costQtyParam == "2") && !(costQtyParam == "3"))
					{
						if (!(costQtyParam == "1"))
						{
							goto IL_1A1;
						}
						num2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "ActualQty", 0m);
					}
					else
					{
						num2 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(this.Model.DataObject, "Qty", 0m);
					}
					IL_1B5:
					decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BillAccuScrapRate", 0m);
					decimal dynamicValue4 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BillAccuFixScrapRate", 0m);
					decimal dynamicValue5 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BILLACCUNUMERATOR", 0m);
					decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BillAccuDenominator", 0m);
					decimal num3 = 1m;
					if (this.costQtyParam.Equals("2"))
					{
						num3 = dynamicValue5 / dynamicValue6;
					}
					else if (this.costQtyParam.Equals("3"))
					{
						num3 = dynamicValue5 * ++(dynamicValue3 * Convert.ToDecimal(0.01)) / dynamicValue6 + dynamicValue4;
					}
					if (num3 != 1m)
					{
						num2 *= num3;
					}
					DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "ShouldQty", num2);
					IGrouping<string, DynamicObject> source2 = null;
					string dynamicValue7 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					if (!dictionary2.TryGetValue(dynamicValue7, out source2))
					{
						decimal dynamicValue8 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Price", 0m);
						decimal dynamicValue9 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "TaxPrice", 0m);
						DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue8 * num2);
						DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue9 * num2);
						if (dynamicValue)
						{
							decimal dynamicValue10 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubPrice", 0m);
							decimal dynamicValue11 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubTaxPrice", 0m);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", dynamicValue10 * num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", dynamicValue11 * num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue8 * num2 + dynamicValue10 * num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue9 * num2 + dynamicValue11 * num2);
							continue;
						}
						continue;
					}
					else
					{
						List<DynamicObject> list2 = source2.ToList<DynamicObject>();
						string dynamicValue12 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
						if (!IsWgCost || !dynamicValue12.Equals("1"))
						{
							decimal dynamicValue13 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Price", 0m);
							decimal dynamicValue14 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "TaxPrice", 0m);
							decimal dynamicValue15 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubPrice", 0m);
							decimal dynamicValue16 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubTaxPrice", 0m);
							if (purPriceCheck)
							{
								bool flag = false;
								if (dynamicValue13 > 0m && !dynamicValue12.Equals("1"))
								{
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue13 * num2);
									flag = true;
								}
								if (dynamicValue14 > 0m && !dynamicValue12.Equals("1"))
								{
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue14 * num2);
									flag = true;
								}
								if (dynamicValue)
								{
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", dynamicValue15 * num2);
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", dynamicValue16 * num2);
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue13 * num2 + dynamicValue15 * num2);
									DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue14 * num2 + dynamicValue16 * num2);
								}
								if (flag)
								{
									continue;
								}
							}
							decimal d = 0m;
							decimal num4 = 0m;
							decimal d2 = 0m;
							decimal num5 = 0m;
							foreach (DynamicObject dynamicObject2 in list2)
							{
								decimal dynamicValue17 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "Price", 0m);
								decimal dynamicValue18 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "SingleCost", 0m);
								decimal dynamicValue19 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "TaxPrice", 0m);
								decimal dynamicValue20 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "SumTaxCost", 0m);
								string dynamicValue21 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "MaterialType", null);
								if (dynamicValue21.Equals("2"))
								{
									d -= dynamicValue17;
									num4 -= dynamicValue18;
									d2 -= dynamicValue19;
									num5 -= dynamicValue20;
								}
								else
								{
									d += dynamicValue17;
									num4 += dynamicValue18;
									d2 += dynamicValue19;
									num5 += dynamicValue20;
								}
							}
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "TaxRate", 0);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "Price", num4 / num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", num4);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "TaxPrice", num5 / num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", num5);
							if (dynamicValue)
							{
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubPrice", 0m) * num2);
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubTaxPrice", 0m) * num2);
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", num4 + DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubAmount", 0m));
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", num5 + DataEntityExtend.GetDynamicValue<decimal>(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", 0m));
							}
							if (num4 > 0m)
							{
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "PriceSource", "按BOM子项卷算");
							}
							else
							{
								DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "PriceSource", string.Empty);
							}
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "ListSupplierId_Id", 0);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "ListSupplierId", null);
							continue;
						}
						decimal dynamicValue22 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Price", 0m);
						DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue22 * num2);
						if (dynamicValue)
						{
							decimal dynamicValue23 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubPrice", 0m);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubAmount", dynamicValue23 * num2);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SingleCost", dynamicValue22 * num2 + dynamicValue23 * num2);
						}
						if (dynamicValue22 <= 0m)
						{
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "PriceSource", string.Empty);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "ListSupplierId_Id", 0);
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "ListSupplierId", null);
						}
						decimal dynamicValue24 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "TaxPrice", 0m);
						if (dynamicValue24 <= 0m)
						{
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", 0.0);
						}
						else
						{
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue24 * num2);
						}
						if (!dynamicValue)
						{
							continue;
						}
						decimal dynamicValue25 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "SubTaxPrice", 0m);
						if (dynamicValue25 <= 0m)
						{
							DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", 0.0);
							continue;
						}
						DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SubTaxAmount", dynamicValue25 * num2);
						DataEntityExtend.SetDynamicObjectItemValue(detailDataEntities[dynamicValue2 - 1], "SumTaxCost", dynamicValue24 * num2 + dynamicValue25 * num2);
						continue;
					}
					IL_1A1:
					num2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Qty", 0m);
					goto IL_1B5;
				}
			}
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x0006BDD8 File Offset: 0x00069FD8
		private Dictionary<long, UnitConvert> GetUnitConvert(List<string> ucKeys, List<GetUnitConvertRateArgs> parentParamList, DynamicObjectCollection detailDataEntities, List<string> parentEntryIds)
		{
			(from x in detailDataEntities
			where !parentEntryIds.Contains(DataEntityExtend.GetDynamicValue<string>(x, "EntryId", null))
			select x).ToList<DynamicObject>().ForEach(delegate(DynamicObject x)
			{
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(x, "MaterialId", null);
				DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialBase", null);
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(x, "UnitId_Id", 0L);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicValue2[0], "BaseUnitId_Id", 0L);
				long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "msterID", 0L);
				string item2 = string.Format("{0}_{1}_{2}", dynamicValue5, dynamicValue3, dynamicValue4);
				if (!ucKeys.Contains(item2))
				{
					ucKeys.Add(item2);
				}
			});
			foreach (string text in ucKeys)
			{
				string[] array = text.Split(new string[]
				{
					"_"
				}, StringSplitOptions.RemoveEmptyEntries);
				GetUnitConvertRateArgs item = new GetUnitConvertRateArgs
				{
					PrimaryKey = (long)ucKeys.IndexOf(text),
					MasterId = OtherExtend.ConvertTo<long>(array[0], 0L),
					SourceUnitId = OtherExtend.ConvertTo<long>(array[2], 0L),
					DestUnitId = OtherExtend.ConvertTo<long>(array[1], 0L)
				};
				parentParamList.Add(item);
			}
			return UnitConvertServiceHelper.GetUnitConvertRateList(base.Context, parentParamList);
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0006BF08 File Offset: 0x0006A108
		protected virtual List<DynamicObject> BuildBomExpandSourceData()
		{
			DateTime dynamicValue = DataEntityExtend.GetDynamicValue<DateTime>(this.View.Model.DataObject, "ValidDate", default(DateTime));
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.View.Model.DataObject, "BomVersion", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
			{
				return null;
			}
			List<DynamicObject> list = new List<DynamicObject>();
			List<long> bomIds = (from bomObj in dynamicValue2
			select DataEntityExtend.GetDynamicValue<long>(bomObj, "BomVersion_Id", 0L)).Distinct<long>().ToList<long>();
			DynamicObjectCollection bomInfo = this.GetBomInfo(this.View.Context, bomIds);
			if (ListUtils.IsEmpty<DynamicObject>(bomInfo))
			{
				return null;
			}
			List<string> list2 = new List<string>();
			Dictionary<long, UnitConvert> unitConvert = this.GetUnitConvert(this.View.Context, bomInfo, list2);
			Dictionary<long, DynamicObject> dictionary = bomInfo.ToDictionary((DynamicObject x) => DataEntityExtend.GetDynamicValue<long>(x, "FID", 0L));
			foreach (DynamicObject dynamicObject in dynamicValue2)
			{
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomVersion_Id", 0L);
				DynamicObject dynamicObject2 = null;
				if (dictionary.TryGetValue(dynamicValue3, out dynamicObject2))
				{
					long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FMATERIALID", 0L);
					long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialMasterId", 0L);
					long dynamicValue6 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FUNITID", 0L);
					long dynamicValue7 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FBaseUnitId", 0L);
					long dynamicValue8 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FUseOrgId", 0L);
					long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FParentAuxPropId", 0L);
					BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
					decimal num = DataEntityExtend.GetDynamicValue<decimal>(this.View.Model.DataObject, "Qty", 1m);
					string item = string.Format("{0}_{1}_{2}", dynamicValue5, dynamicValue7, dynamicValue6);
					long key = (long)list2.IndexOf(item);
					UnitConvert unitConvert2 = null;
					if (unitConvert.TryGetValue(key, out unitConvert2))
					{
						num = unitConvert2.ConvertQty(num, "");
					}
					bomForwardSourceDynamicRow.MaterialId_Id = dynamicValue4;
					bomForwardSourceDynamicRow.BomId_Id = dynamicValue3;
					bomForwardSourceDynamicRow.NeedQty = num;
					bomForwardSourceDynamicRow.NeedDate = new DateTime?(dynamicValue);
					bomForwardSourceDynamicRow.UnitId_Id = dynamicValue6;
					bomForwardSourceDynamicRow.BaseUnitId_Id = dynamicValue7;
					bomForwardSourceDynamicRow.SupplyOrgId_Id = dynamicValue8;
					bomForwardSourceDynamicRow.AuxPropId = dynamicValue9;
					bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
					list.Add(bomForwardSourceDynamicRow.DataEntity);
				}
			}
			return list;
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x0006C1BC File Offset: 0x0006A3BC
		private Dictionary<long, UnitConvert> GetUnitConvert(Context ctx, DynamicObjectCollection bomInfos, List<string> ucKeys)
		{
			List<GetUnitConvertRateArgs> list = new List<GetUnitConvertRateArgs>();
			Dictionary<long, DynamicObject> dictionary = bomInfos.ToDictionary((DynamicObject x) => DataEntityExtend.GetDynamicValue<long>(x, "FID", 0L));
			foreach (DynamicObject dynamicObject in bomInfos)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FID", 0L);
				DynamicObject dynamicObject2 = null;
				if (dictionary.TryGetValue(dynamicValue, out dynamicObject2))
				{
					DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FMATERIALID", 0L);
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialMasterId", 0L);
					long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FUNITID", 0L);
					long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FBaseUnitId", 0L);
					DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FUseOrgId", 0L);
					string item = string.Format("{0}_{1}_{2}", dynamicValue2, dynamicValue4, dynamicValue3);
					if (!ucKeys.Contains(item))
					{
						ucKeys.Add(item);
					}
				}
			}
			foreach (string text in ucKeys)
			{
				string[] array = text.Split(new string[]
				{
					"_"
				}, StringSplitOptions.RemoveEmptyEntries);
				GetUnitConvertRateArgs item2 = new GetUnitConvertRateArgs
				{
					PrimaryKey = (long)ucKeys.IndexOf(text),
					MasterId = OtherExtend.ConvertTo<long>(array[0], 0L),
					SourceUnitId = OtherExtend.ConvertTo<long>(array[1], 0L),
					DestUnitId = OtherExtend.ConvertTo<long>(array[2], 0L)
				};
				list.Add(item2);
			}
			return UnitConvertServiceHelper.GetUnitConvertRateList(base.Context, list);
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x0006C394 File Offset: 0x0006A594
		protected virtual DynamicObjectCollection GetBomInfo(Context ctx, List<long> bomIds)
		{
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
			string sqlWithCardinality = StringUtils.GetSqlWithCardinality(bomIds.Distinct<long>().Count<long>(), "@FID", 1, true);
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				TableName = sqlWithCardinality,
				TableNameAs = "ts",
				ScourceKey = "FID"
			});
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@FID", 161, bomIds.Distinct<long>().ToArray<long>())
			};
			return QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter, list);
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x0006C499 File Offset: 0x0006A699
		protected virtual List<DynamicObject> GetBomChildData(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			return BomQueryServiceHelper.GetBomQueryForwardResult(base.Context, lstExpandSource, memBomExpandOption);
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x0006C4A8 File Offset: 0x0006A6A8
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

		// Token: 0x060008E7 RID: 2279 RVA: 0x0006C65C File Offset: 0x0006A85C
		protected virtual List<DynamicObject> GetBomCOBYData(long lBomId)
		{
			return BomQueryServiceHelper.GetBomCobyDataForQuery(base.Context, new List<long>
			{
				lBomId
			}, true);
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0006C684 File Offset: 0x0006A884
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有物料清单正查的{0}权限", "015072000019375", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0006C784 File Offset: 0x0006A984
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			base.BeforeEntityExport(e);
			List<DynamicObject> list = this.bomPrintChildItems;
			this._exportData = new List<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
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
			e.DataSource = new Dictionary<string, List<DynamicObject>>
			{
				{
					"FBottomEntity",
					this._exportData
				}
			};
			e.ExportEntityKeyList = new List<string>
			{
				"FBottomEntity"
			};
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x0006C8F4 File Offset: 0x0006AAF4
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

		// Token: 0x060008EB RID: 2283 RVA: 0x0006C9F8 File Offset: 0x0006ABF8
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

		// Token: 0x060008EC RID: 2284 RVA: 0x0006CA2C File Offset: 0x0006AC2C
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

		// Token: 0x060008ED RID: 2285 RVA: 0x0006CA80 File Offset: 0x0006AC80
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

		// Token: 0x060008EE RID: 2286 RVA: 0x0006CB54 File Offset: 0x0006AD54
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

		// Token: 0x060008EF RID: 2287 RVA: 0x0006CCD0 File Offset: 0x0006AED0
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

		// Token: 0x060008F0 RID: 2288 RVA: 0x0006CDE4 File Offset: 0x0006AFE4
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
					BomQueryCost.<>c__DisplayClass8f CS$<>8__locals1 = new BomQueryCost.<>c__DisplayClass8f();
					CS$<>8__locals1.subDynamicObject = enumerator.Current;
					DynamicObject dynamicObject = OrmUtils.Clone(CS$<>8__locals1.subDynamicObject, CS$<>8__locals1.subDynamicObject.DynamicObjectType, false, true) as DynamicObject;
					int count = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "BomLevel", 0);
					if (DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.subDynamicObject, "MaterialType", null).Equals("3"))
					{
						dynamicObject["BomLevel"] = DataEntityExtend.GetDynamicValue<string>((from z in tempResultDynamicObjects
						where this.countNumber(DataEntityExtend.GetDynamicValue<string>(z, "BomLevel", null)) == count
						select z).Last<DynamicObject>(), "BomLevel", null);
					}
					else
					{
						dynamicObject["BomLevel"] = ((DataEntityExtend.GetDynamicValue<string>(parentNode, "BomLevel", null) == "0") ? "1" : DataEntityExtend.GetDynamicValue<string>(parentNode, "BomLevel", null)) + "." + ((from z in list
						where DataEntityExtend.GetDynamicValue<string>(z, "MaterialType", null) != "3" && DataEntityExtend.GetDynamicValue<string>(z, "BomLevel", null) == DataEntityExtend.GetDynamicValue<string>(CS$<>8__locals1.subDynamicObject, "BomLevel", null)
						select z).ToList<DynamicObject>().IndexOf(CS$<>8__locals1.subDynamicObject) + 1);
					}
					tempResultDynamicObjects.Add(dynamicObject);
					this.AddSubNode(dynamicObject, sourceGroupDynamicObject, tempResultDynamicObjects);
				}
			}
			return tempResultDynamicObjects;
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0006CFC8 File Offset: 0x0006B1C8
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBEXPANDALLROW"))
				{
					return;
				}
				TreeEntryGrid control = this.View.GetControl<TreeEntryGrid>("FBottomEntity");
				Entity entity = this.View.BusinessInfo.GetEntity("FBottomEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from g in entityDataObject
					group g by DataEntityExtend.GetDynamicValue<string>(g, "EntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = (from g in this.bomQueryChildItems
					group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FBottomEntity");
					DynamicObject entityDataObject2 = this.View.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
					if (ListUtils.IsEmpty<KeyValuePair<string, IGrouping<string, DynamicObject>>>(dictionary) || ListUtils.IsEmpty<KeyValuePair<string, IGrouping<string, DynamicObject>>>(dictionary2) || ObjectUtils.IsNullOrEmpty(entityDataObject2))
					{
						e.Cancel = true;
						return;
					}
					string currEntryId = entityDataObject2["EntryId"].ToString();
					this.ExpandedAllRows(entryCurrentRowIndex, currEntryId, this.bomQueryChildItems, dictionary2, dictionary, control, entity, entityDataObject);
					this.View.SetEntityFocusRow("FBottomEntity", entryCurrentRowIndex);
				}
			}
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0006D160 File Offset: 0x0006B360
		private void ExpandedAllRows(int currRowIndex, string currEntryId, List<DynamicObject> bomQueryForwardEntrys, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroupDatas, TreeEntryGrid treeEntryGrid, Entity entry, DynamicObjectCollection bomQueryEntryDatas)
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
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					if (!bomQueryForwardEntryGroupDatas.TryGetValue(dynamicValue, out source))
					{
						this.View.Model.CreateNewEntryRow(entry, -1, dynamicObject);
					}
					int currRowIndex2 = bomQueryEntryDatas.IndexOf(dynamicObject);
					this.ExpandedAllRows(currRowIndex2, dynamicValue, bomQueryForwardEntrys, bomQueryForwardEntryGroups, bomQueryForwardEntryGroupDatas, treeEntryGrid, entry, bomQueryEntryDatas);
				}
			}
		}

		// Token: 0x0400040D RID: 1037
		protected const string EntityKey_FBomChildEntity = "FBottomEntity";

		// Token: 0x0400040E RID: 1038
		protected const string EntityKey_FBomBillHead = "FBillHead";

		// Token: 0x0400040F RID: 1039
		protected const string EntityKey_FCobyEntity = "FCobyEntity";

		// Token: 0x04000410 RID: 1040
		protected const string FieldKey_FExpandLevel = "FExpandLevel";

		// Token: 0x04000411 RID: 1041
		protected const string FieldKey_FValidDate = "FValidDate";

		// Token: 0x04000412 RID: 1042
		protected const string FieldKey_FQty = "FQty";

		// Token: 0x04000413 RID: 1043
		protected const string FieldKey_FBomId = "FBomId";

		// Token: 0x04000414 RID: 1044
		protected const string FieldKey_FMaterialId = "FMaterialId";

		// Token: 0x04000415 RID: 1045
		protected const string FieldKey_FBillBomId = "FBillBomId";

		// Token: 0x04000416 RID: 1046
		protected const string FieldKey_FBillMaterialId = "FBillMaterialId";

		// Token: 0x04000417 RID: 1047
		protected const string FieldKey_FBomUseOrgId = "FBomUseOrgId";

		// Token: 0x04000418 RID: 1048
		protected const string FiledKey_FBomVersion = "FBomVersion";

		// Token: 0x04000419 RID: 1049
		protected const string FieldKey_FBomChildMaterialId = "FMaterialId2";

		// Token: 0x0400041A RID: 1050
		protected const string FieldKey_FBomEntryId = "FBomEntryId";

		// Token: 0x0400041B RID: 1051
		protected const string FormKey_MaterialTree = "MFG_MaterialTree";

		// Token: 0x0400041C RID: 1052
		protected const string ControlKey_SplitContainer = "FSpliteContainer";

		// Token: 0x0400041D RID: 1053
		protected const string EntityKey_FBomHeadEntity = "FTopEntity";

		// Token: 0x0400041E RID: 1054
		protected const string Key_Contain = "FTreePanel";

		// Token: 0x0400041F RID: 1055
		protected FilterParameter filterParam;

		// Token: 0x04000420 RID: 1056
		private string _materialTree_PageId;

		// Token: 0x04000421 RID: 1057
		protected List<DynamicObject> bomQueryChildItems = new List<DynamicObject>();

		// Token: 0x04000422 RID: 1058
		protected List<DynamicObject> bomPrintChildItems = new List<DynamicObject>();

		// Token: 0x04000423 RID: 1059
		protected MemBomExpandOption_ForPSV memBomExpandOption;

		// Token: 0x04000424 RID: 1060
		private List<DynamicObject> _exportData = new List<DynamicObject>();

		// Token: 0x04000425 RID: 1061
		private int curTabindex;
	}
}
