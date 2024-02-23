using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.SFC.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200004F RID: 79
	[Description("工艺路线_表单插件")]
	[HotUpdate]
	public class RouteEdit : BaseControlEdit
	{
		// Token: 0x0600055C RID: 1372 RVA: 0x0004084A File Offset: 0x0003EA4A
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitCustomParameters();
			this.InitFormTitle();
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x00040860 File Offset: 0x0003EA60
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			if (0.Equals(base.View.OpenParameter.CreateFrom))
			{
				this.Model.SetValue("FProduceType", this.ProduceType);
				this.Model.SetValue("FSeqNumber", '0', 0);
				this.Model.SetValue("FSeqName", ResManager.LoadKDString("主序列", "015072000013679", 7, new object[0]), 0);
				this.Model.SetValue("FSeqType", 'M', 0);
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x00040914 File Offset: 0x0003EB14
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (string.Equals(e.Entity.Key, "FSubEntity", StringComparison.OrdinalIgnoreCase))
			{
				IEnumerable<DynamicObject> entityDataObject = this.Model.GetEntityDataObject(e.Entity);
				if (e.Row == entityDataObject.Count<DynamicObject>() - 1)
				{
					int value = (from w in entityDataObject
					select DataEntityExtend.GetDynamicObjectItemValue<int>(w, "OperNumber", 0)).Max();
					int num = (int)Math.Ceiling(++value / 10m) * 10;
					this.Model.SetValue("FOperNumber", (num > 9999) ? 9999 : num, e.Row);
				}
				this.InsertBaseActivities(e.Row);
			}
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x000409EC File Offset: 0x0003EBEC
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			Dictionary<string, object> customParameters = base.View.OpenParameter.GetCustomParameters();
			if (customParameters.ContainsKey("materialId") && !string.IsNullOrWhiteSpace(customParameters["materialId"].ToString()))
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					customParameters["materialId"]
				}, formMetadata.BusinessInfo.GetDynamicObjectType());
				this.Model.SetItemValueByID("FMATERIALID", Convert.ToInt64(array[0]["Id"]), 0);
			}
			this.UpdateActRelationDefault();
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdHideField(base.View, "FProxyEnableLocationNo");
			}
			bool userParam = MFGBillUtil.GetUserParam<bool>(base.View, "IsShowCostResource", false);
			this.IsNeedHandleCostRes = false;
			if (base.View.OpenParameter.GetCustomParameters().ContainsKey("ProduceType"))
			{
				this.IsNeedHandleCostRes = ("F".Equals(Convert.ToString(base.View.Model.GetValue("FProduceType"))) && userParam);
			}
			else
			{
				this.IsNeedHandleCostRes = ("F".Equals(this.ProduceType) && userParam);
			}
			base.View.GetControl("FTabCostRes").Visible = this.IsNeedHandleCostRes;
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00040B68 File Offset: 0x0003ED68
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			EntryGrid control = base.View.GetControl<EntryGrid>("FSubEntity");
			control.SetSort("FOperNumber", 1);
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00040B9C File Offset: 0x0003ED9C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (key == "FSeqNumber")
				{
					this.SeqNumberBeforeUpdateValue(e);
					return;
				}
				if (key == "FSeqType")
				{
					this.SeqTypeBeforeUpdateValue(e);
					return;
				}
				if (!(key == "FAllocateWorkTime"))
				{
					if (!(key == "FBaseBatch"))
					{
						return;
					}
					decimal d = Convert.ToDecimal(e.Value);
					if (d <= 0m)
					{
						e.Cancel = true;
					}
				}
				else
				{
					decimal d2 = Convert.ToDecimal(e.Value);
					if (d2 <= 0m)
					{
						e.Cancel = true;
						return;
					}
					decimal costResTempQty = this.GetCostResTempQty();
					if (costResTempQty <= 0m)
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("请先维护活动数量！", "015072030036340", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
					decimal num = d2 * 100m / costResTempQty;
					if (num > 100m)
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("资源使用比例无法超过100%，请重新输入操作时长！", "015072030039749", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
					base.View.Model.SetValue("FUseRate", num, e.Row);
					base.View.UpdateView("FUseRate", e.Row);
					return;
				}
			}
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x00040D1C File Offset: 0x0003EF1C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			switch (key = e.Field.Key)
			{
			case "FProcessType":
				this.ProcessTypeDataChanged(e);
				break;
			case "FMATERIALID":
				this.MaterialIdDataChanged(e);
				break;
			case "FWorkCenterId":
				this.WorkCenterIdDataChanged(e);
				break;
			case "FPurchaseOrgId":
				if (this.OperationType.Equals("CopySubEntryRow"))
				{
					this.OperationType = "";
				}
				else
				{
					this.PurchaseOrgIdDataChanged(e);
				}
				break;
			case "FSupplier":
				this.SupplierDataChanged(e);
				break;
			case "FProOrgId":
				this.ProOrgChanged(e);
				break;
			case "FOptCtrlCodeId":
				if (this.OperationType.Equals("CopySubEntryRow"))
				{
					this.OperationType = "";
				}
				else
				{
					this.SetPurchaseOrgDefault(e.Row);
				}
				break;
			case "FProcessId":
				if (this.IsNeedHandleCostRes)
				{
					if (0L == Convert.ToInt64(e.NewValue))
					{
						base.View.Model.DeleteEntryData("FSubEntityCost");
						base.View.UpdateView("FSubEntityCost");
					}
					else
					{
						this.HandleCostResInfo(true);
					}
				}
				break;
			case "FActivity1Qty":
			case "FActivity2Qty":
			case "FActivity3Qty":
			case "FBaseBatch":
				if (this.IsNeedHandleCostRes)
				{
					this.HandleCostResInfo(false);
				}
				break;
			case "FUseRate":
			{
				DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntityCost");
				base.View.Model.SetValue("FAllocateWorkTime", this.GetCostResTempQty() * Convert.ToDecimal(entryCurrentRow["UseRate"]) / 100m, e.Row);
				base.View.UpdateView("FAllocateWorkTime", e.Row);
				break;
			}
			case "FIsProcessRecordStation":
			case "FIsQualityInspectStation":
				this.HandleDefaultSchemaEntry(e);
				break;
			}
			if (string.Compare(e.Field.Entity.Key, "FSubEntity", true) == 0)
			{
				this.Model.SetValue("FIsDirty", true, e.Row);
			}
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0004101C File Offset: 0x0003F21C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (a == "Save")
				{
					this.SetOperModifierAndDate(e);
					return;
				}
				if (!(a == "CopySubEntryRow"))
				{
					return;
				}
				this.OperationType = "CopySubEntryRow";
			}
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x00041077 File Offset: 0x0003F277
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ConfrimWorkTimeCollect())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x00041090 File Offset: 0x0003F290
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			this.InitFormTitle();
			bool flag = false;
			string a;
			if ((a = e.Operation.Operation.ToString()) != null)
			{
				if (!(a == "Save"))
				{
					if (a == "Audit")
					{
						bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.Model.ParameterData, "IsUpdateRoute", false);
						if (e.OperationResult.IsSuccess && dynamicValue)
						{
							string text = Convert.ToString(this.Model.DataObject["Id"]);
							RouteServiceHelper.SetMaterialDefaultRoute(base.Context, text);
						}
					}
				}
				else if (e.OperationResult.IsSuccess)
				{
					flag = true;
				}
			}
			if (flag)
			{
				base.View.Refresh();
			}
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0004114A File Offset: 0x0003F34A
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			base.BeforeDeleteRow(e);
			if (string.Compare(e.EntityKey, "FEntity", true) == 0)
			{
				this.VerifyAllowDeleteEntry(e);
			}
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x00041170 File Offset: 0x0003F370
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToString()) != null)
			{
				if (!(a == "tbInsertSubEntryRow"))
				{
					return;
				}
				EntryGrid control = base.View.GetControl<EntryGrid>("FSubEntity");
				control.SetSort("", 1);
			}
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x000411C0 File Offset: 0x0003F3C0
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			if (string.Compare(e.EntityKey, "FEntity", true) == 0)
			{
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, e.Row);
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(entityDataObject, "SeqType", null);
				DynamicObject entityDataObject2 = this.Model.GetEntityDataObject(entryEntity, e.NewRow);
				if (dynamicObjectItemValue.Equals("M"))
				{
					Field field = base.View.BusinessInfo.GetField("FSeqAlignment");
					Field field2 = base.View.BusinessInfo.GetField("FSeqRefer");
					this.Model.SetValue(field, entityDataObject2, "S");
					this.Model.SetValue(field2, entityDataObject2, "0");
				}
				Field field3 = base.View.BusinessInfo.GetField("FSeqNumber");
				this.Model.SetValue(field3, entityDataObject2, string.Empty);
			}
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x000412C0 File Offset: 0x0003F4C0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FMaterialGroupId":
				this.MaterialGroupIdBeforeF7Select(e);
				return;
			case "FActivity1RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity1Id");
				return;
			case "FActivity2RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity2Id");
				return;
			case "FActivity3RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity3Id");
				return;
			case "FActivity4RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity4Id");
				return;
			case "FActivity5RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity5Id");
				return;
			case "FActivity6RepFormulaId":
				this.ActivityRepFormulaIdF7Select(e, "FActivity6Id");
				return;
			case "FWorkCenterId":
				this.WorkCenterFilter(e);
				return;
			case "FBomId":
				this.BOMFilter(e);
				return;
			case "FProcessCheckSchemaId":
			case "FInspectCheckSchemaId":
				this.CheckSchemaFilter(e);
				return;
			case "FProcessCheckSchemaEntryId":
			case "FInspectCheckSchemaEntryId":
				this.CheckSchemaEntryIdFilter(e);
				break;

				return;
			}
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x00041460 File Offset: 0x0003F660
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBSETDEFAULT"))
				{
					return;
				}
				this.SetDefaultRoute();
			}
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00041498 File Offset: 0x0003F698
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbTecParameter" || barItemKey == "tbTecParameterLst")
				{
					this.ShowTecParameterForm(e.BarItemKey);
					return;
				}
				if (!(barItemKey == "tbDeleteSubEntry"))
				{
					return;
				}
				this.DeleteTecParameter();
			}
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x000414F4 File Offset: 0x0003F6F4
		private void SeqNumberBeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Value != null && !string.IsNullOrEmpty(e.Value.ToString()) && !Regex.IsMatch(e.Value.ToString(), "^[A-Za-z0-9]+$"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("序列号只能包含字母或数字！", "015072000012571", 7, new object[0]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x00041560 File Offset: 0x0003F760
		private void SeqTypeBeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Value.ToString().Equals("M"))
			{
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				List<string> dynamicObjectColumnValues = DataEntityExtend.GetDynamicObjectColumnValues<string>(entityDataObject, "SeqType");
				if (dynamicObjectColumnValues.Contains("M"))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("工序序列有且只能含有一个主干序列！", "015072000012572", 7, new object[0]), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x000415F0 File Offset: 0x0003F7F0
		private void ProcessTypeDataChanged(DataChangedEventArgs e)
		{
			if (!e.NewValue.ToString().Equals("M"))
			{
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "BD_UNIT",
					FilterClauseWihtKey = "FUNITGROUPID = 10086 AND FISBASEUNIT = '1'",
					SelectItems = SelectorItemInfo.CreateItems("FUNITID")
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
				{
					this.Model.SetValue("FUnitID", dynamicObjectCollection[0]["FUNITID"]);
				}
			}
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x00041680 File Offset: 0x0003F880
		private void MaterialIdDataChanged(DataChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FMATERIALID", -1, null, null);
				DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(value, "MaterialProduce", null);
				DynamicObject dynamicObject = null;
				if (dynamicObjectItemValue != null && dynamicObjectItemValue.Count > 0)
				{
					dynamicObject = dynamicObjectItemValue.FirstOrDefault<DynamicObject>();
				}
				long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ProduceUnitId_Id", 0L);
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject2 in entityDataObject)
				{
					DynamicObjectCollection dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject2, "RouteOperDetail", null);
					foreach (DynamicObject dynamicObject3 in dynamicObjectItemValue3)
					{
						Field field = base.View.BusinessInfo.GetField("FOperUnitID");
						this.Model.SetValue(field, dynamicObject3, dynamicObjectItemValue2);
					}
				}
			}
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x000417B0 File Offset: 0x0003F9B0
		private void WorkCenterIdDataChanged(DataChangedEventArgs e)
		{
			this.CleanActivities(e.Row);
			if (e.NewValue != null)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_WorkCenter", true) as FormMetadata;
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, e.NewValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "WorkCenterBaseActivity", null);
				DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "WorkCenterScheduling", null);
				foreach (DynamicObject dynamicObject2 in dynamicObjectItemValue)
				{
					long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "BaseActivityID_Id", 0L);
					decimal dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject2, "DefaultValue", 0m);
					long dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "TimeUnit_Id", 0L);
					long dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ActFormula_Id", 0L);
					long dynamicObjectItemValue7 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ActRepFormula_Id", 0L);
					int num = dynamicObjectItemValue.IndexOf(dynamicObject2);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Id", dynamicObjectItemValue3, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Qty", dynamicObjectItemValue4, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "UnitID", dynamicObjectItemValue5, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "FormulaId", dynamicObjectItemValue6, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "RepFormulaId", dynamicObjectItemValue7, e.Row);
				}
				if (dynamicObjectItemValue2.Count > 0)
				{
					DynamicObject dynamicObject3 = dynamicObjectItemValue2[0];
					decimal dynamicObjectItemValue8 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "StdQueueTime", 0m);
					decimal dynamicObjectItemValue9 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "MinQueueTime", 0m);
					decimal dynamicObjectItemValue10 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "StdWaitTime", 0m);
					decimal dynamicObjectItemValue11 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "MinWaitTime", 0m);
					decimal dynamicObjectItemValue12 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "StdCarryTime", 0m);
					decimal dynamicObjectItemValue13 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject3, "MinCarryTime", 0m);
					this.Model.SetValue("FStdQueueTime", dynamicObjectItemValue8, e.Row);
					this.Model.SetValue("FMinQueueTime", dynamicObjectItemValue9, e.Row);
					this.Model.SetValue("FStdWaitTime", dynamicObjectItemValue10, e.Row);
					this.Model.SetValue("FMinWaitTime", dynamicObjectItemValue11, e.Row);
					this.Model.SetValue("FStdMoveTime", dynamicObjectItemValue12, e.Row);
					this.Model.SetValue("FMinMoveTime", dynamicObjectItemValue13, e.Row);
				}
			}
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x00041AF8 File Offset: 0x0003FCF8
		private void SupplierDataChanged(DataChangedEventArgs e)
		{
			if (e.NewValue == null || string.IsNullOrEmpty(e.NewValue.ToString()) || "0".Equals(e.NewValue.ToString()))
			{
				this.Model.SetValue("FTaxRate", 0, e.Row);
				return;
			}
			if (MFGBillUtil.GetValue<decimal>(this.Model, "FTaxRate", e.Row, 0m, null) == 0m)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_Supplier", true) as FormMetadata;
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, e.NewValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				DynamicObject dynamicObject2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "SupplierFinance", null).FirstOrDefault<DynamicObject>();
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject2, "TaxRateId", null);
				if (dynamicObjectItemValue != null)
				{
					decimal dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObjectItemValue, "TaxRate", 0m);
					this.Model.SetValue("FTaxRate", dynamicObjectItemValue2, e.Row);
					decimal value = MFGBillUtil.GetValue<decimal>(this.Model, "FOutSrcTaxPrice", e.Row, 0m, null);
					decimal num = value / ++(dynamicObjectItemValue2 / 100m);
					this.Model.SetValue("FOutSrcPrice", num, e.Row);
					decimal value2 = MFGBillUtil.GetValue<decimal>(this.Model, "FScrapTaxPrice", e.Row, 0m, null);
					decimal num2 = value2 / ++(value2 / 100m);
					this.Model.SetValue("FScrapPrice", num2, e.Row);
					decimal value3 = MFGBillUtil.GetValue<decimal>(this.Model, "FMatScrapTaxPrice", e.Row, 0m, null);
					decimal num3 = value3 / ++(value3 / 100m);
					this.Model.SetValue("FMatScrapPrice", num3, e.Row);
				}
			}
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x00041D14 File Offset: 0x0003FF14
		private void PurchaseOrgIdDataChanged(DataChangedEventArgs e)
		{
			this.Model.SetValue("FSupplier", 0, e.Row);
			object value = this.Model.GetValue("FPurchaseOrgId", e.Row);
			object value2 = this.Model.GetValue("FOptCtrlCodeId", e.Row);
			if (value != null && value2 != null && ((DynamicObject)value2)["ProcessingMode"].ToString() == "20")
			{
				long orgId = Convert.ToInt64(((DynamicObject)value)["Id"]);
				this.CommonDefaultCurrency(orgId, e.Row);
			}
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x00041DD8 File Offset: 0x0003FFD8
		private void MaterialGroupIdBeforeF7Select(BeforeF7SelectEventArgs e)
		{
			e.Cancel = true;
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "SFC_MaterialGroupSelect";
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData != null)
				{
					this.Model.SetItemValueByID("FMaterialGroupId", result.ReturnData, -1);
				}
			});
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x00041E28 File Offset: 0x00040028
		private void VerifyAllowDeleteEntry(BeforeDeleteRowEventArgs e)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, e.Row);
			if ("M".Equals(DataEntityExtend.GetDynamicObjectItemValue<string>(entityDataObject, "SeqType", null)))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("不允许删除主干序列！", "015072000012573", 7, new object[0]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x00041EAC File Offset: 0x000400AC
		private void SetOperModifierAndDate(BeforeDoOperationEventArgs e)
		{
			long userId = base.Context.UserId;
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "RouteOperDetail", null);
				foreach (DynamicObject dynamicObject2 in dynamicObjectItemValue)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject2, "IsDirty", false))
					{
						Field field = base.View.BusinessInfo.GetField("FOperModifierId");
						Field field2 = base.View.BusinessInfo.GetField("FOperModifyDate");
						this.Model.SetValue(field, dynamicObject2, userId);
						this.Model.SetValue(field2, dynamicObject2, systemDateTime);
					}
				}
			}
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x00041FE8 File Offset: 0x000401E8
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			DynamicObject dataObject = e.DataObject;
			DynamicObject srcDataObject = e.SrcDataObject;
			DataEntityExtend.SetDynamicObjectItemValue(dataObject, "ProcessType", DataEntityExtend.GetDynamicObjectItemValue<char>(srcDataObject, "ProcessType", '\0'));
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x00042028 File Offset: 0x00040228
		private void InitCustomParameters()
		{
			Dictionary<string, object> customParameters = base.View.OpenParameter.GetCustomParameters();
			if (customParameters.ContainsKey("ProduceType"))
			{
				this.ProduceType = customParameters["ProduceType"].ToString();
			}
			else if (base.View.ParentFormView != null)
			{
				Dictionary<string, object> customParameters2 = base.View.ParentFormView.OpenParameter.GetCustomParameters();
				if (customParameters2.ContainsKey("ProduceType"))
				{
					this.ProduceType = customParameters2["ProduceType"].ToString();
				}
				customParameters.Add("ProduceType", this.ProduceType);
			}
			if (!customParameters.ContainsKey("LayoutId"))
			{
				customParameters.Add("LayoutId", this.GetLayoutId());
			}
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x000420E0 File Offset: 0x000402E0
		private void InsertBaseActivities(int rowIndex)
		{
			if ("C".Equals(this.ProduceType))
			{
				return;
			}
			this.CleanActivities(rowIndex);
			if (this.BaseActivities == null)
			{
				this.BaseActivities = this.LoadActivities();
			}
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FProOrgId", rowIndex, null, null);
			this.CacheUseOrgID = ((value == null) ? 0L : OtherExtend.ConvertTo<long>(value["ID"], 0L));
			int num = 0;
			foreach (DynamicObject dynamicObject in this.BaseActivities)
			{
				long num2 = Convert.ToInt64(dynamicObject["Id"]);
				decimal num3 = Convert.ToDecimal(dynamicObject["DefaultValue"]);
				long num4 = Convert.ToInt64(dynamicObject["FUnitID_Id"]);
				this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Id", num2, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Qty", num3, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "UnitID", num4, rowIndex);
				this.SetActRelationDefault(rowIndex, num + 1, dynamicObject, this.CacheUseOrgID);
				num++;
			}
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x00042244 File Offset: 0x00040444
		private void UpdateActRelationDefault()
		{
			if ("C".Equals(this.ProduceType) || this.CacheUseOrgID != 0L)
			{
				return;
			}
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FUseOrgId", -1, null, null);
			this.CacheUseOrgID = ((value == null) ? 0L : OtherExtend.ConvertTo<long>(value["ID"], 0L));
			for (int i = 1; i < 4; i++)
			{
				this.SetActRelationDefault(0, i, this.Model.GetValue("FActivity" + Convert.ToString(i) + "Id", 0) as DynamicObject, this.CacheUseOrgID);
			}
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x000422E8 File Offset: 0x000404E8
		private void SetActRelationDefault(int rowIndex, int seq, DynamicObject baseActivity, long useOrgID)
		{
			if (baseActivity == null)
			{
				return;
			}
			Tuple<long, string> formulaByActivityPhase = this.GetFormulaByActivityPhase(baseActivity);
			this.SetActFormulaDefault(rowIndex, seq, useOrgID, formulaByActivityPhase.Item1);
			this.SetActRepFormulaDefault(rowIndex, seq, useOrgID, formulaByActivityPhase.Item2);
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x00042324 File Offset: 0x00040524
		private void SetActFormulaDefault(int rowIndex, int seq, long useOrgID, long formulaId)
		{
			string filter = string.Format(" FUSEORGID={0} AND FMASTERID={1} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND FFORMULAUSE = '7' ", useOrgID, formulaId);
			List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
			if (formulaInfo.Count > 0)
			{
				this.Model.SetValue("FActivity" + Convert.ToString(seq) + "FormulaId", formulaInfo[0]["ID"], rowIndex);
			}
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0004238C File Offset: 0x0004058C
		private void SetActRepFormulaDefault(int rowIndex, int seq, long useOrgID, string formulaUse)
		{
			string filter = string.Format(" FUSEORGID={0} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND FFORMULAUSE = '{1}' AND FISDEFAULT='1' ", useOrgID, formulaUse);
			List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
			if (formulaInfo.Count > 0)
			{
				this.Model.SetValue("FActivity" + Convert.ToString(seq) + "RepFormulaId", formulaInfo[0]["ID"], rowIndex);
			}
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x000423F0 File Offset: 0x000405F0
		private void SetPurchaseOrgDefault(int rowIndex)
		{
			object value = this.Model.GetValue("FProOrgId", rowIndex);
			object value2 = this.Model.GetValue("FOptCtrlCodeId", rowIndex);
			if (value2 != null && ((DynamicObject)value2)["ProcessingMode"].ToString() == "20" && value != null)
			{
				long num = Convert.ToInt64(((DynamicObject)value)["Id"]);
				long[] orgIdsByFuncId = OrganizationServiceHelper.GetOrgIdsByFuncId(base.Context, 102.ToString("D"));
				if (orgIdsByFuncId.Contains(num))
				{
					this.Model.SetValue("FPurchaseOrgId", num, rowIndex);
					this.CommonDefaultCurrency(num, rowIndex);
				}
			}
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x000424A4 File Offset: 0x000406A4
		private void CommonDefaultCurrency(long orgId, int rowIndex)
		{
			long num = 0L;
			long[] orgIdsByFuncId = OrganizationServiceHelper.GetOrgIdsByFuncId(base.Context, 107.ToString("D"));
			if (orgIdsByFuncId.Contains(orgId))
			{
				num = MFGServiceHelper.GetDefCurrencyAndExchangeTypeByBizOrgID(base.Context, orgId, 0L);
			}
			else
			{
				object[] orgByBizRelationship = OrganizationServiceHelper.GetOrgByBizRelationship(base.Context, orgId, Convert.ToInt64(105), true, true);
				if (orgByBizRelationship.Count<object>() == 1 && orgIdsByFuncId.Contains(Convert.ToInt64(orgByBizRelationship[0])))
				{
					num = MFGServiceHelper.GetDefCurrencyAndExchangeTypeByBizOrgID(base.Context, Convert.ToInt64(orgByBizRelationship[0]), 0L);
				}
			}
			this.Model.SetValue("FOutSrcCurrency", num, rowIndex);
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x00042548 File Offset: 0x00040748
		private DynamicObject[] LoadActivities()
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BaseActivity", true) as FormMetadata;
			return BusinessDataServiceHelper.Load(base.Context, new object[]
			{
				40380L,
				40381L,
				40382L
			}, formMetadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x000425B4 File Offset: 0x000407B4
		private List<DynamicObject> GetFormulaInfo(string filter)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FISDEFAULT"),
				new SelectorItemInfo("FFORMULAUSE")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_FORMULA", list, filter, "");
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x00042620 File Offset: 0x00040820
		private void ActivityRepFormulaIdF7Select(BeforeF7SelectEventArgs e, string activityIdFieldKey)
		{
			string text = string.Empty;
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, activityIdFieldKey, e.Row, null, null);
			text = StringUtils.JoinFilterString(text, string.Format(" FFORMULAUSE = '{0}' ", this.GetFormulaByActivityPhase(value).Item2), "AND");
			e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x00042688 File Offset: 0x00040888
		private Tuple<long, string> GetFormulaByActivityPhase(DynamicObject baseActivity)
		{
			long item = 0L;
			string item2 = string.Empty;
			if (!ObjectUtils.IsNullOrEmpty(baseActivity))
			{
				string text = Convert.ToString(baseActivity["Phase"]);
				string a;
				if ((a = text) != null)
				{
					if (!(a == "10"))
					{
						if (!(a == "20"))
						{
							if (a == "30")
							{
								item = 40388L;
								item2 = "10";
							}
						}
						else
						{
							item = 40387L;
							item2 = "9";
						}
					}
					else
					{
						item = 40386L;
						item2 = "8";
					}
				}
			}
			return new Tuple<long, string>(item, item2);
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x00042718 File Offset: 0x00040918
		private string GetLayoutId()
		{
			string text = base.View.OpenParameter.LayoutId;
			if (text == null)
			{
				string produceType;
				if ((produceType = this.ProduceType) != null)
				{
					if (produceType == "C")
					{
						text = "";
						goto IL_50;
					}
					if (produceType == "F")
					{
						text = "9c3de02d-5469-44ef-8fd7-9821f371a8cf";
						goto IL_50;
					}
				}
				text = "";
				IL_50:
				base.View.OpenParameter.LayoutId = text;
			}
			return text;
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x00042788 File Offset: 0x00040988
		private void InitFormTitle()
		{
			if ("C".Equals(this.ProduceType))
			{
				return;
			}
			OperationStatus status = base.View.OpenParameter.Status;
			LocaleValue localeValue;
			if (status == null)
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性工艺路线 - 新增", "015072000011011", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			else if (status == 2)
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性工艺路线 - 修改", "015072000011012", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			else
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性工艺路线 - 查看", "015072000011013", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			base.View.SetFormTitle(localeValue);
			base.View.SetInnerTitle(localeValue);
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x00042862 File Offset: 0x00040A62
		private void ProOrgChanged(DataChangedEventArgs e)
		{
			this.InsertBaseActivities(e.Row);
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x00042870 File Offset: 0x00040A70
		private void CleanActivities(int rowIndex)
		{
			for (int i = 0; i < 6; i++)
			{
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "Id", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "Qty", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "UnitID", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "FormulaId", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "RepFormulaId", null, rowIndex);
			}
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x00042943 File Offset: 0x00040B43
		private void WorkCenterFilter(BeforeF7SelectEventArgs e)
		{
			if (this.ProduceType.Equals("C"))
			{
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, " FWORKCENTERTYPE = 'A' ");
			}
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x00042978 File Offset: 0x00040B78
		private void BOMFilter(BeforeF7SelectEventArgs e)
		{
			if (Convert.ToInt64(this.Model.DataObject["MATERIALID_Id"]) != 0L)
			{
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, string.Format(" FMATERIALID = '{0}'", this.Model.DataObject["MATERIALID_Id"]));
				if (Convert.ToInt64(this.Model.DataObject["AuxPropId_Id"]) != 0L)
				{
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, string.Format(" FParentAuxPropId = '{0}'", this.Model.DataObject["AuxPropId_Id"]));
				}
			}
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x00042A44 File Offset: 0x00040C44
		private void SetDefaultRoute()
		{
			string text = Convert.ToString(this.Model.DataObject["Id"]);
			IOperationResult operationResult = RouteServiceHelper.SetMaterialDefaultRoute(base.Context, text);
			if (operationResult.IsSuccess)
			{
				base.View.ShowMessage(ResManager.LoadKDString("默认工艺路线设置成功！", "015072000014321", 7, new object[0]), 0);
				return;
			}
			if (operationResult.ValidationErrors.Count > 0)
			{
				string text2 = string.Join(",", from o in operationResult.ValidationErrors
				select o.Message);
				base.View.ShowErrMessage(text2, "", 0);
			}
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x00042AF8 File Offset: 0x00040CF8
		private DynamicObject GetEntryCurrentRow(string entryEntityKey)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(entryEntityKey);
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(entryEntityKey);
			return this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x00042B34 File Offset: 0x00040D34
		private void CreatNewTecParameterBill(DynamicObject currentRow)
		{
			if (currentRow == null)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选中分录行", "015165000011032", 7, new object[0]), 0);
				return;
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "ENG_TECPARAMETER";
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.MultiSelect = false;
			billShowParameter.Status = 0;
			billShowParameter.CustomComplexParams.Add("currentRow", currentRow);
			billShowParameter.CustomComplexParams.Add("PrdOrgId", DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, "UseOrgId_Id", 0L));
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x00042BE0 File Offset: 0x00040DE0
		private void ShowTecParameterBill(DynamicObject currentRow)
		{
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(currentRow, "Id", 0L);
			if (dynamicObjectItemValue == 0L)
			{
				return;
			}
			string text = string.Format("SELECT M.FID FROM T_ENG_TECPARAMETER  M where M.FROUTEENTRYID = {0}", dynamicObjectItemValue);
			string pkey = "";
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text))
			{
				while (dataReader.Read())
				{
					pkey = Convert.ToString(dataReader["FID"]);
				}
			}
			string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "DocumentStatus", null);
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "ENG_TECPARAMETER";
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.MultiSelect = false;
			if (dynamicObjectItemValue2.Equals("B") || dynamicObjectItemValue2.Equals("C"))
			{
				billShowParameter.Status = 1;
			}
			else
			{
				billShowParameter.Status = 2;
			}
			billShowParameter.PKey = pkey;
			billShowParameter.CustomComplexParams.Add("currentRow", currentRow);
			billShowParameter.CustomComplexParams.Add("PrdOrgId", DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, "UseOrgId_Id", 0L));
			billShowParameter.CustomComplexParams.Add("DocumentStatus", dynamicObjectItemValue2);
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x00042D38 File Offset: 0x00040F38
		private void ShowTecParameterBillList(DynamicObject currentRow)
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_TECPARAMETER";
			listShowParameter.ParentPageId = base.View.PageId;
			listShowParameter.MultiSelect = true;
			listShowParameter.ListFilterParameter.Filter = string.Format(" FROUTENO = '{0}' ", DataEntityExtend.GetDynamicObjectItemValue<string>((currentRow.Parent as DynamicObject).Parent as DynamicObject, "Number", null));
			listShowParameter.CustomComplexParams.Add("CurrentRow", currentRow);
			listShowParameter.CustomComplexParams.Add("DocumentStatus", DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "DocumentStatus", null));
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x00042DE8 File Offset: 0x00040FE8
		private long GetTecParameterBillCount(DynamicObject currentRow)
		{
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(currentRow, "Id", 0L);
			string text = string.Format("SELECT COUNT(1) FCOUNT FROM T_ENG_TECPARAMETER WHERE FROUTEENTRYID = {0}", dynamicObjectItemValue);
			long result = 0L;
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text))
			{
				while (dataReader.Read())
				{
					result = Convert.ToInt64(dataReader["FCOUNT"]);
				}
			}
			return result;
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00042E5C File Offset: 0x0004105C
		private void ShowTecParameterForm(string barItem)
		{
			DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntity");
			if (DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "DocumentStatus", null).Equals("Z") || DataEntityExtend.GetDynamicObjectItemValue<long>(entryCurrentRow, "Id", 0L) == 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请先保存工艺路线！", "015072000013575", 7, new object[0]), 0);
				return;
			}
			long tecParameterBillCount = this.GetTecParameterBillCount(entryCurrentRow);
			if (barItem != null)
			{
				if (!(barItem == "tbTecParameter"))
				{
					if (!(barItem == "tbTecParameterLst"))
					{
						return;
					}
					this.ShowTecParameterBillList(entryCurrentRow);
				}
				else
				{
					if (tecParameterBillCount > 0L)
					{
						this.ShowTecParameterBill(entryCurrentRow);
						return;
					}
					if (DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "DocumentStatus", null).Equals("B") || DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "DocumentStatus", null).Equals("C"))
					{
						base.View.ShowMessage(ResManager.LoadKDString("请先反审核！", "015072000013576", 7, new object[0]), 0);
						return;
					}
					this.CreatNewTecParameterBill(entryCurrentRow);
					return;
				}
			}
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x00042F7C File Offset: 0x0004117C
		private void CheckSchemaEntryIdFilter(BeforeF7SelectEventArgs e)
		{
			if (Convert.ToString(this.Model.DataObject["ProcessType"]) == "M")
			{
				string text = string.Format(" FID IN (select ST.FID from V_SFC_PRCCTRLCKSCMF8 ST\r\n\tINNER JOIN T_SFC_PrcCtrlCkScm scm on scm.fid=ST.fschemaid\r\n\tINNER JOIN  T_SFC_PrcCtrlCkScmMatEntry try on try.fid=scm.fid where FMATERIALID = {0})", Convert.ToInt64(this.Model.DataObject["MATERIALID_Id"]));
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x00042FF8 File Offset: 0x000411F8
		private void DeleteTecParameter()
		{
			DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntity");
			if (entryCurrentRow == null)
			{
				return;
			}
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>((entryCurrentRow.Parent as DynamicObject).Parent as DynamicObject, "Number", null);
			string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(entryCurrentRow.Parent as DynamicObject, "SeqNumber", null);
			long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(entryCurrentRow, "OperNumber", 0L);
			this.DeleteTecParameter(dynamicObjectItemValue, dynamicObjectItemValue2, dynamicObjectItemValue3);
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x00043064 File Offset: 0x00041264
		private void DeleteTecParameter(string routeNo, string seqNumber, long operNumber)
		{
			string text = string.Format("SELECT M.FID FROM T_ENG_TECPARAMETER  M\r\n                                                    WHERE M.FROUTENO = @RouteNo \r\n                                                      AND M.FSEQNUMBER = @seqNumber\r\n                                                      AND M.FOPERNUMBER = @operNumber", new object[0]);
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@RouteNo", 0, routeNo),
				new SqlParam("@seqNumber", 0, seqNumber),
				new SqlParam("@operNumber", 11, operNumber)
			};
			List<object> list2 = new List<object>();
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, list))
			{
				while (dataReader.Read())
				{
					list2.Add(dataReader["FID"]);
				}
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_TECPARAMETER", true);
			BusinessDataServiceHelper.Delete(base.Context, list2.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0004314C File Offset: 0x0004134C
		private void CheckSchemaFilter(BeforeF7SelectEventArgs e)
		{
			if (Convert.ToString(this.Model.DataObject["ProcessType"]) == "M")
			{
				string text = string.Format(" FID IN (SELECT FID FROM T_SFC_PrcCtrlCkScmMatEntry WHERE FMATERIALID = {0})", Convert.ToInt64(this.Model.DataObject["MATERIALID_Id"]));
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x000431C8 File Offset: 0x000413C8
		private void HandleDefaultSchemaEntry(DataChangedEventArgs e)
		{
			if (e.NewValue == null)
			{
				return;
			}
			DynamicObject dynamicObject = null;
			string text = "FProcessCheckSchemaId";
			string text2 = "FProcessCheckSchemaEntryId";
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FIsProcessRecordStation"))
				{
					if (key == "FIsQualityInspectStation")
					{
						text = "FInspectCheckSchemaId";
						text2 = "FInspectCheckSchemaEntryId";
					}
				}
				else
				{
					text = "FProcessCheckSchemaId";
					text2 = "FProcessCheckSchemaEntryId";
				}
			}
			if (Convert.ToBoolean(e.NewValue))
			{
				if (MFGBillUtil.GetValue<DynamicObject>(this.Model, text2, e.Row, null, null) == null)
				{
					DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntity");
					CheckSchemaParam checkSchemaParam = new CheckSchemaParam
					{
						CheckType = (("FInspectCheckSchemaId" == text) ? "1" : "2"),
						IsFlexPrd = !("C" == this.ProduceType),
						ProcessId = Convert.ToInt64(entryCurrentRow["ProcessId_Id"]),
						MaterialId = Convert.ToInt64(this.Model.DataObject["MATERIALID_Id"]),
						ProcessType = Convert.ToString(this.Model.DataObject["ProcessType"])
					};
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, text, -1, null, null);
					if (value != null)
					{
						checkSchemaParam.SchemaId = Convert.ToInt64(value["Id"]);
					}
					dynamicObject = SFCCheckSchemaEntity.Instance.GetDefaultSchemaEntryF8Obj(base.Context, checkSchemaParam);
					this.Model.SetValue(text2, dynamicObject, e.Row);
					return;
				}
			}
			else
			{
				this.Model.SetValue(text2, dynamicObject, e.Row);
			}
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x00043374 File Offset: 0x00041574
		private void HandleCostResInfo(bool isUpdateByProcess = true)
		{
			if (this.IsNeedHandleCostRes)
			{
				if (isUpdateByProcess)
				{
					DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntity");
					DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)((DynamicObject)entryCurrentRow["ProcessId"])["CostResourceEntity"];
					if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
					{
						DynamicObjectCollection dynamicObjectCollection2 = (DynamicObjectCollection)entryCurrentRow["SubEntityCost"];
						int num = 0;
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							decimal d = Convert.ToDecimal(dynamicObject["UseRate"]);
							base.View.Model.CreateNewEntryRow("FSubEntityCost");
							DynamicObject dynamicObject2 = dynamicObjectCollection2[num];
							dynamicObject2["Seq"] = num + 1;
							dynamicObject2["ResourceId_Id"] = dynamicObject["CostResource_Id"];
							dynamicObject2["UseRate"] = dynamicObject["UseRate"];
							dynamicObject2["ResourceReqNum"] = dynamicObject["ResourceReqNum"];
							dynamicObject2["AllocateWorkTime"] = this.GetCostResTempQty() * d / 100m;
							num++;
						}
						DBServiceHelper.LoadReferenceObject(base.Context, dynamicObjectCollection2.ToArray<DynamicObject>(), dynamicObjectCollection2.DynamicCollectionItemPropertyType, true);
						base.View.UpdateView("FSubEntityCost");
						return;
					}
				}
				else
				{
					decimal costResTempQty = this.GetCostResTempQty();
					DynamicObjectCollection dynamicObjectCollection3 = (DynamicObjectCollection)this.GetEntryCurrentRow("FSubEntity")["SubEntityCost"];
					if (costResTempQty > 0m && dynamicObjectCollection3.Count > 0)
					{
						foreach (DynamicObject dynamicObject3 in dynamicObjectCollection3)
						{
							decimal d2 = Convert.ToDecimal(dynamicObject3["UseRate"]);
							dynamicObject3["AllocateWorkTime"] = costResTempQty * d2 / 100m;
						}
						base.View.UpdateView("FSubEntityCost");
					}
				}
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x000435C4 File Offset: 0x000417C4
		private decimal GetCostResTempQty()
		{
			DynamicObject entryCurrentRow = this.GetEntryCurrentRow("FSubEntity");
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(entryCurrentRow))
			{
				return 0m;
			}
			long unitId = Convert.ToInt64(entryCurrentRow["Activity1UnitID_Id"]);
			long unitId2 = Convert.ToInt64(entryCurrentRow["Activity2UnitID_Id"]);
			long unitId3 = Convert.ToInt64(entryCurrentRow["Activity3UnitID_Id"]);
			decimal d = this.HandleActivityQtyConvert(Convert.ToDecimal(entryCurrentRow["Activity1Qty"]), unitId);
			decimal d2 = this.HandleActivityQtyConvert(Convert.ToDecimal(entryCurrentRow["Activity2Qty"]), unitId2);
			decimal d3 = this.HandleActivityQtyConvert(Convert.ToDecimal(entryCurrentRow["Activity3Qty"]), unitId3);
			decimal d4 = Convert.ToDecimal(entryCurrentRow["BaseBatch"]);
			return d + (d2 + d3) / d4;
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00043697 File Offset: 0x00041897
		private decimal HandleActivityQtyConvert(decimal qty, long unitId)
		{
			if (0L == unitId || 80506L == unitId)
			{
				return qty;
			}
			if (80505L == unitId)
			{
				return Math.Round(qty / 60m, 0);
			}
			return qty * 60m;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x000436E8 File Offset: 0x000418E8
		private bool ConfrimWorkTimeCollect()
		{
			bool result = true;
			if (!this.ProduceType.Equals("C"))
			{
				string text = Convert.ToString(this.Model.GetValue("FWorkTimeColect"));
				if (text.Equals("B"))
				{
					base.View.ShowMessage(ResManager.LoadKDString("实际工时大于标准工时，是否保存?", "015072030038685", 7, new object[0]), 4, delegate(MessageBoxResult yesRes)
					{
						if (1 != yesRes)
						{
							result = false;
						}
					}, "", 3);
				}
			}
			return result;
		}

		// Token: 0x0400025E RID: 606
		private const long PrepareActivityId = 40380L;

		// Token: 0x0400025F RID: 607
		private const long ProcessActivityId = 40381L;

		// Token: 0x04000260 RID: 608
		private const long RemoveActivityId = 40382L;

		// Token: 0x04000261 RID: 609
		private const long PrepareFormulaId = 40386L;

		// Token: 0x04000262 RID: 610
		private const long ProcessFormulaId = 40387L;

		// Token: 0x04000263 RID: 611
		private const long RemoveFormulaId = 40388L;

		// Token: 0x04000264 RID: 612
		private string OperationType = "";

		// Token: 0x04000265 RID: 613
		private string ProduceType = "C";

		// Token: 0x04000266 RID: 614
		private DynamicObject[] BaseActivities;

		// Token: 0x04000267 RID: 615
		private long CacheUseOrgID;

		// Token: 0x04000268 RID: 616
		private bool IsNeedHandleCostRes;
	}
}
