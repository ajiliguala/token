using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000082 RID: 130
	[Description("BOP展开_表单插件")]
	public class BopExpandEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060009FB RID: 2555 RVA: 0x0007476C File Offset: 0x0007296C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.lstCacheProductNumbers = new List<string>();
			this.lstCachePrdLineIds = new List<long>();
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x0007478C File Offset: 0x0007298C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.View.ParentFormView != null)
			{
				long formSession = MFGBillUtil.GetFormSession<long>(this.View.ParentFormView, "BopExpandDemandOrgId", true);
				if (formSession > 0L)
				{
					this.Model.SetValue("FDemandOrgId", formSession);
					this.View.UpdateView("FDemandOrgId");
				}
			}
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x000747F0 File Offset: 0x000729F0
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "Refresh"))
				{
					if (!(operation == "Return"))
					{
						return;
					}
					if (e.OperationResult.IsSuccess)
					{
						this.ReturnDataToParent();
						return;
					}
					e.OperationResult.IsShowMessage = true;
				}
				else
				{
					if (e.OperationResult.IsSuccess)
					{
						this.DoRefresh();
						return;
					}
					e.OperationResult.IsShowMessage = true;
					return;
				}
			}
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x00074870 File Offset: 0x00072A70
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FProductNumber")
				{
					this.ShowProductNumberSelForm();
					return;
				}
				if (!(fieldKey == "FParentPrdLineId"))
				{
					return;
				}
				this.SetParentPrdLineFilter(e);
			}
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x000748B8 File Offset: 0x00072AB8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FProductNumber"))
				{
					return;
				}
				if (!this.VerifyProductNumberExistence(Convert.ToString(e.Value)))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00074900 File Offset: 0x00072B00
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FParentMaterialId")
				{
					this.CachePrdLines();
					this.SetDefParentBomId(e);
					this.ClearProductNumber();
					this.CacheProductNumbers();
					this.ClearBopExpandEntrys();
					return;
				}
				if (key == "FParentBomId")
				{
					this.SetDefParentPrdLineId(e);
					this.CacheProductNumbers();
					this.ClearBopExpandEntrys();
					return;
				}
				if (!(key == "FParentPrdLineId"))
				{
					return;
				}
				this.ClearParentPrdLineLocId(e);
				this.ClearBopExpandEntrys();
			}
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x0007498C File Offset: 0x00072B8C
		public override void Dispose()
		{
			base.Dispose();
			if (this.bopExpandOption != null)
			{
				BopExpandServiceHelper.ClearBopExpandResult(base.Context, this.bopExpandOption);
			}
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x000749AD File Offset: 0x00072BAD
		private void DoRefresh()
		{
			this.InitBopExpandOption();
			this.FillBopExpandEntryData();
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x000749BC File Offset: 0x00072BBC
		private void InitBopExpandOption()
		{
			if (this.bopExpandOption != null)
			{
				BopExpandServiceHelper.ClearBopExpandResult(base.Context, this.bopExpandOption);
				this.bopExpandOption.BopExpandId = SequentialGuid.NewGuid().ToString();
				return;
			}
			this.bopExpandOption = new BopExpandOption();
			this.bopExpandOption.BopExpandId = SequentialGuid.NewGuid().ToString();
			this.bopExpandOption.ExpandLevelTo = 1;
			this.bopExpandOption.ExpandVirtualMaterial = true;
			this.bopExpandOption.DeleteVirtualMaterial = false;
			this.bopExpandOption.Mode = 0;
			this.bopExpandOption.Option.SetVariableValue("BopExpandConvert", true);
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x00074AE8 File Offset: 0x00072CE8
		private void FillBopExpandEntryData()
		{
			this.Model.DeleteEntryData("FEntity");
			DynamicObject dynamicObject = this.DoBopExpand();
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "BopExpandResult", null);
			if (dynamicObjectItemValue.Count > 0)
			{
				IEnumerable<DynamicObject> enumerable = from w in dynamicObjectItemValue
				where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0) > 0
				select w;
				string parentPrdLineLocNumber = DataEntityExtend.GetDynamicObjectItemValue<string>(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FParentPrdLineLocId", -1, null, null), "LocationCode", null);
				if (!string.IsNullOrEmpty(parentPrdLineLocNumber))
				{
					enumerable = from w in enumerable
					where string.Compare(DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(w, "ResultProductLocId", null), "LocationCode", null), parentPrdLineLocNumber) <= 0
					select w;
				}
				long ownerId = MFGBillUtil.GetParentFormSession<long>(this.View, "BopExpandOwnerId");
				if (ownerId > 0L)
				{
					enumerable = from w in enumerable
					where ownerId == DataEntityExtend.GetDynamicObjectItemValue<long>(w, "OWNERID_Id", 0L)
					select w;
				}
				enumerable = (from o in enumerable
				orderby DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(o, "ResultProductLocId", null), "LocationCode", null)
				select o).ToList<DynamicObject>();
				if (enumerable.Count<DynamicObject>() > 0)
				{
					this.Model.BeginIniti();
					EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					DataEntityExtend.FromDataSource(entityDataObject, enumerable, true, new Action<CreateEntityRowCallbackParam>(this.CreateNewEntityRowCallback), new Func<GetFieldValueCallbackParam, object>(this.GetFieldValueCallback), null);
					foreach (DynamicObject dynamicObject2 in entityDataObject)
					{
						dynamicObject2["IsSelect"] = true;
					}
					this.Model.EndIniti();
					this.View.UpdateView("FEntity");
					this.View.SetEntityFocusRow("FEntity", 0);
				}
			}
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x00074CE4 File Offset: 0x00072EE4
		private DynamicObject DoBopExpand()
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BopExpandBill", true) as FormMetadata;
			EntryEntity entryEntity = formMetadata.BusinessInfo.GetEntryEntity("FBopSource");
			DynamicObject dynamicObject = new DynamicObject(entryEntity.DynamicObjectType);
			dynamicObject["RowId"] = SequentialGuid.NewGuid().ToString();
			dynamicObject["MaterialId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
			dynamicObject["BomId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FParentBomId", -1, 0L, null);
			dynamicObject["ProductLineId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FParentPrdLineId", -1, 0L, null);
			dynamicObject["UnitId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FParentUnitId", -1, 0L, null);
			dynamicObject["BaseUnitId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FParentBaseUnitId", -1, 0L, null);
			dynamicObject["NeedQty"] = MFGBillUtil.GetValue<decimal>(this.Model, "FBaseParentQty", -1, 0m, null);
			dynamicObject["NeedDate"] = MFGBillUtil.GetValue<DateTime>(this.Model, "FNeedDate", -1, default(DateTime), null);
			dynamicObject["DemandOrgId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FDemandOrgId", -1, 0L, null);
			dynamicObject["SupplyOrgId_Id"] = MFGBillUtil.GetValue<long>(this.Model, "FDemandOrgId", -1, 0L, null);
			return BopExpandServiceHelper.ExpandBopForward(base.Context, new List<DynamicObject>
			{
				dynamicObject
			}, this.bopExpandOption);
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x00074EB3 File Offset: 0x000730B3
		private void CreateNewEntityRowCallback(CreateEntityRowCallbackParam param)
		{
			DataEntityExtend.SetDynamicObjectItemValue(param.TargetDataEntity, "BopExpandEntryId", DataEntityExtend.GetDynamicObjectItemValue<long>(param.SourceDataEntity, "Id", 0L));
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x00074EDC File Offset: 0x000730DC
		private object GetFieldValueCallback(GetFieldValueCallbackParam param)
		{
			if (string.Equals(param.TargetProperty.Name, "BopExpandEntryId"))
			{
				param.IsCancel = true;
				return null;
			}
			FormMetadata formMetadata = null;
			param.Options.TryGetVariableValue<FormMetadata>("FormMetadata", ref formMetadata);
			if (param.SourceProperty != null && param.SourceDataEntity != null)
			{
				return param.SourceProperty.GetValue(param.SourceDataEntity);
			}
			return null;
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00074F60 File Offset: 0x00073160
		private void ReturnDataToParent()
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FEntity");
			List<DynamicObject> list = (from w in this.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请先选择分录，再返回数据！", "015072000017272", 7, new object[0]), "", 0);
				return;
			}
			string value = MFGBillUtil.GetValue<string>(this.Model, "FProductNumber", -1, null, null);
			if (this.VerifyProductNumberMustInput() && string.IsNullOrEmpty(value))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("当产品的库存属性(BOM版本、批号、计划跟踪号)以及辅助属性中，任意一项启用并且影响成本时，生产编号不能为空！", "015072000017273", 7, new object[0]), "", 0);
				return;
			}
			if ((from s in list
			select DataEntityExtend.GetDynamicObjectItemValue<long>(s, "OWNERID_Id", 0L)).Distinct<long>().Count<long>() > 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("返回数据的货主必须相同！", "015072000017274", 7, new object[0]), "", 0);
				return;
			}
			if (this.View.ParentFormView != null && this.View.ParentFormView is IDynamicFormViewService)
			{
				long value2 = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
				long value3 = MFGBillUtil.GetValue<long>(this.Model, "FParentPrdLineId", -1, 0L, null);
				string value4 = MFGBillUtil.GetValue<string>(this.Model, "FTraceNo", -1, string.Empty, null);
				DynamicObject value5 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FParentBomId", -1, null, null);
				DynamicObject value6 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FCompleteReportId", -1, null, null);
				DynamicObject dynamicObject = (value6 != null) ? value6 : value5;
				this.View.ReturnToParentWindow(list);
				OperateOption operateOption = OperateOption.Create();
				operateOption.SetVariableValue("ReturnParentMaterialId", value2);
				operateOption.SetVariableValue("ReturnParentPrdLineId", value3);
				operateOption.SetVariableValue("ReturnTraceNo", value4);
				operateOption.SetVariableValue("ReturnProductNumber", value);
				operateOption.SetVariableValue("ReturnSrcBillType", (value6 != null) ? "R" : "B");
				operateOption.SetVariableValue("ReturnSrcBillNo", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null));
				this.View.ParentFormView.Session["ReturnOperateOption"] = operateOption;
				if (this.View.ParentFormView.Session.ContainsKey("BopExpandOwnerId") && MFGBillUtil.GetParentFormSession<long>(this.View, "BopExpandOwnerId") == 0L)
				{
					this.View.ParentFormView.Session["ReturnOwnerTypeId"] = DataEntityExtend.GetDynamicObjectItemValue<string>(list.FirstOrDefault<DynamicObject>(), "OWNERTYPEID", null);
					this.View.ParentFormView.Session["ReturnOwnerId"] = DataEntityExtend.GetDynamicObjectItemValue<long>(list.FirstOrDefault<DynamicObject>(), "OWNERID_Id", 0L);
					if (value6 != null)
					{
						this.View.ParentFormView.Session["ParentOwnerTypeId"] = value6["ParentOwnerTypeId"];
						this.View.ParentFormView.Session["ParentOwnerId_Id"] = value6["ParentOwnerId_Id"];
					}
				}
				IDynamicFormViewService dynamicFormViewService = this.View.ParentFormView as IDynamicFormViewService;
				dynamicFormViewService.CustomEvents(this.View.PageId, "CustomSelBill", "returnData");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
				this.View.Close();
			}
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00075360 File Offset: 0x00073560
		private bool VerifyProductNumberMustInput()
		{
			long value = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, value, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
			List<long> lstVerifyInvPtyIds = new List<long>
			{
				10003L,
				10004L,
				10006L
			};
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "MaterialInvPty", null);
			bool flag = dynamicObjectItemValue.Any((DynamicObject a) => DataEntityExtend.GetDynamicObjectItemValue<bool>(a, "IsEnable", false) && DataEntityExtend.GetDynamicObjectItemValue<bool>(a, "IsAffectCost", false) && lstVerifyInvPtyIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(a, "InvPtyId_Id", 0L)));
			DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "MaterialAuxPty", null);
			bool flag2 = dynamicObjectItemValue2.Any((DynamicObject a) => DataEntityExtend.GetDynamicObjectItemValue<bool>(a, "IsEnable1", false) && DataEntityExtend.GetDynamicObjectItemValue<bool>(a, "IsAffectCost", false));
			return flag || flag2;
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00075454 File Offset: 0x00073654
		private void SetDefParentBomId(DataChangedEventArgs e)
		{
			if (e.NewValue != null && Convert.ToInt64(e.NewValue) > 0L)
			{
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, Convert.ToInt64(e.NewValue));
				long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(base.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1], 2);
				this.Model.SetValue("FParentBomId", defaultBomKey);
				return;
			}
			this.Model.SetValue("FParentBomId", null);
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x000754D8 File Offset: 0x000736D8
		private void SetDefParentPrdLineId(DataChangedEventArgs e)
		{
			if (Convert.ToInt64(e.NewValue) <= 0L)
			{
				this.Model.SetValue("FParentPrdLineId", null);
				return;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true) as FormMetadata;
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, e.NewValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "BopEntity", null);
			if (dynamicObjectItemValue.Count > 0)
			{
				IEnumerable<long> source = DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicObjectItemValue, "ProductLineId_Id").Distinct<long>();
				long num = source.FirstOrDefault((long f) => this.lstCachePrdLineIds.Contains(f));
				this.Model.SetValue("FParentPrdLineId", num);
				return;
			}
			this.Model.SetValue("FParentPrdLineId", null);
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x000755AA File Offset: 0x000737AA
		private void ClearParentPrdLineLocId(DataChangedEventArgs e)
		{
			if (MFGBillUtil.GetValue<long>(this.Model, "FCompleteReportId", -1, 0L, null) > 0L)
			{
				return;
			}
			this.Model.SetValue("FParentPrdLineLocId", null);
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x000755D6 File Offset: 0x000737D6
		private void ClearBopExpandEntrys()
		{
			this.Model.DeleteEntryData("FEntity");
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x000755E8 File Offset: 0x000737E8
		private void ClearProductNumber()
		{
			if (MFGBillUtil.GetValue<long>(this.Model, "FCompleteReportId", -1, 0L, null) > 0L)
			{
				return;
			}
			this.Model.SetValue("FProductNumber", null);
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x00075624 File Offset: 0x00073824
		private void CacheProductNumbers()
		{
			string text = " 1=1 ";
			long value = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
			if (value > 0L)
			{
				text += string.Format(" AND FMATERIALID = {0} ", value);
			}
			long value2 = MFGBillUtil.GetValue<long>(this.Model, "FParentBomId", -1, 0L, null);
			if (value2 > 0L)
			{
				text += string.Format(" AND FBOMID = {0} ", value2);
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "REM_ProductNumber",
				SelectItems = SelectorItemInfo.CreateItems("FNumber"),
				FilterClauseWihtKey = text
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			this.lstCacheProductNumbers = (from s in dynamicObjectCollection
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "FNumber", null)).Distinct<string>().ToList<string>();
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x0007570B File Offset: 0x0007390B
		private bool VerifyProductNumberExistence(string productNumber)
		{
			return string.IsNullOrEmpty(productNumber) || this.lstCacheProductNumbers.Contains(productNumber);
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x00075778 File Offset: 0x00073978
		private void ShowProductNumberSelForm()
		{
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter();
			listSelBillShowParameter.FormId = "REM_ProductNumber";
			listSelBillShowParameter.MultiSelect = false;
			listSelBillShowParameter.ParentPageId = this.View.PageId;
			listSelBillShowParameter.OpenStyle.ShowType = 6;
			listSelBillShowParameter.IsLookUp = true;
			string text = " 1=1 ";
			long value = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
			if (value > 0L)
			{
				text += string.Format(" AND FMATERIALID = {0} ", value);
			}
			long value2 = MFGBillUtil.GetValue<long>(this.Model, "FParentBomId", -1, 0L, null);
			if (value2 > 0L)
			{
				text += string.Format(" AND FBOMID = {0} ", value2);
			}
			listSelBillShowParameter.ListFilterParameter.Filter = text;
			this.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection source = result.ReturnData as ListSelectedRowCollection;
					this.Model.SetValue("FProductNumber", source.FirstOrDefault<ListSelectedRow>().Number);
				}
			});
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x00075850 File Offset: 0x00073A50
		private void SetParentPrdLineFilter(BeforeF7SelectEventArgs e)
		{
			e.EnableUICache = false;
			if (this.lstCachePrdLineIds.Count == 0)
			{
				e.ListFilterParameter.Filter = "FID = 0";
				return;
			}
			e.ListFilterParameter.Filter = string.Format("FID IN ({0})", string.Join<long>(",", this.lstCachePrdLineIds));
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x000758A8 File Offset: 0x00073AA8
		private void CachePrdLines()
		{
			long value = MFGBillUtil.GetValue<long>(this.Model, "FParentMaterialId", -1, 0L, null);
			this.lstCachePrdLineIds = new List<long>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_RepeatLineRelateBom",
				SelectItems = SelectorItemInfo.CreateItems("FProductLineId"),
				FilterClauseWihtKey = string.Format("FMATERIALID = {0}", value)
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			this.lstCachePrdLineIds.AddRange(DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicObjectCollection, "FProductLineId"));
			queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_FlowLineRelateBom",
				SelectItems = SelectorItemInfo.CreateItems("FProductLineId"),
				FilterClauseWihtKey = string.Format("FMATERIALID = {0}", value)
			};
			dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			this.lstCachePrdLineIds.AddRange(DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicObjectCollection, "FProductLineId"));
		}

		// Token: 0x040004C4 RID: 1220
		private BopExpandOption bopExpandOption;

		// Token: 0x040004C5 RID: 1221
		private List<string> lstCacheProductNumbers;

		// Token: 0x040004C6 RID: 1222
		private List<long> lstCachePrdLineIds;
	}
}
