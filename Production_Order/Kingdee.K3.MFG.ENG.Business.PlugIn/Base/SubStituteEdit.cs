using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BatchSubStitue;
using Kingdee.K3.Core.MFG.ENG.SynBom;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200003E RID: 62
	public class SubStituteEdit : BaseControlEdit
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600044A RID: 1098 RVA: 0x00035D21 File Offset: 0x00033F21
		// (set) Token: 0x0600044B RID: 1099 RVA: 0x00035D29 File Offset: 0x00033F29
		private object isImportObj { get; set; }

		// Token: 0x0600044C RID: 1100 RVA: 0x00035D34 File Offset: 0x00033F34
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			IDList parentFormSession = MFGBillUtil.GetParentFormSession<IDList>(base.View, "FormInputParam");
			if (e.Paramter is BillOpenParameter)
			{
				(e.Paramter as BillOpenParameter).IdList = parentFormSession;
			}
			base.View.RuleContainer.AddPluginRule("FEntityMainItems", 1, new Action<DynamicObject, object>(this.PriorityChanged), new string[]
			{
				"FMainPriority"
			});
			this.isImportObj = base.View.OpenParameter.GetCustomParameter("ImportView");
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x00035DC8 File Offset: 0x00033FC8
		public override void CreateNewData(BizDataEventArgs e)
		{
			base.CreateNewData(e);
			object obj = null;
			if (base.View.ParentFormView != null)
			{
				base.View.ParentFormView.Session.TryGetValue("NewDataObject", out obj);
			}
			if (obj != null)
			{
				e.BizDataObject = (obj as DynamicObject);
				base.View.ParentFormView.Session["NewDataObject"] = null;
			}
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x00035E32 File Offset: 0x00034032
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.SetMainDefKeyItem();
			this.SetSubsDefKeyItem(0);
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x00035E48 File Offset: 0x00034048
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			string key;
			if ((key = e.Entity.Key) != null)
			{
				if (key == "FEntityMainItems")
				{
					this.SetMainDefKeyItem();
					return;
				}
				if (!(key == "FEntity"))
				{
					return;
				}
				this.SetSubsDefKeyItem(e.Row);
			}
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x00035E9C File Offset: 0x0003409C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (!e.Cancel)
			{
				if (StringUtils.EqualsIgnoreCase(e.Operation.FormOperation.Operation, "BatchReplaceSet"))
				{
					this.ShowBatchSubStitueForm();
				}
				if (StringUtils.EqualsIgnoreCase(e.Operation.FormOperation.Operation, "SynBOM"))
				{
					this.ShowSynBOMForm();
				}
			}
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x00035EFC File Offset: 0x000340FC
		private bool ValidatePermission(string permission)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_BOM",
				SubSystemId = base.View.Model.SubSytemId
			}, permission);
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00035F54 File Offset: 0x00034154
		public void ShowBatchSubStitueForm()
		{
			if (!this.ValidatePermission("f323992d896745fbaab4a2717c79ce2e"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("没有“物料清单”的修改权限！", "015072000018128", 7, new object[0]), "", 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			if ("C" != DataEntityExtend.GetDynamicValue<string>(dataObject, "DocumentStatus", null))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("替代方案未审核不允许进行批量设置！", "015072000018129", 7, new object[0]), "", 0);
				return;
			}
			if ("A" != DataEntityExtend.GetDynamicValue<string>(dataObject, "ForbidStatus", null))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("替代方案已禁用不允许进行批量设置！", "015072030034347", 7, new object[0]), "", 0);
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			DynamicObject dynamicObject = (from w in entityDataObject
			where DataEntityExtend.GetDynamicValue<bool>(w, "IsKeyItem", false)
			select w).FirstOrDefault<DynamicObject>();
			EntryEntity entryEntity2 = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject2 = base.View.Model.GetEntityDataObject(entryEntity2);
			BatchSubStitueOption batchSubStitueOption = new BatchSubStitueOption();
			batchSubStitueOption.ReplacePolicy = DataEntityExtend.GetDynamicValue<string>(dataObject, "ReplacePolicy", null);
			batchSubStitueOption.ReplaceType = DataEntityExtend.GetDynamicValue<string>(dataObject, "ReplaceType", null);
			batchSubStitueOption.SubstitutionId = ((DataEntityExtend.GetDynamicValue<long>(dataObject, "ReplaceNo_Id", 0L) == 0L) ? DataEntityExtend.GetDynamicValue<long>(dataObject, "Id", 0L) : DataEntityExtend.GetDynamicValue<long>(dataObject, "ReplaceNo_Id", 0L));
			batchSubStitueOption.UseOrgId = DataEntityExtend.GetDynamicValue<long>(dataObject, "UseOrgId_Id", 0L);
			batchSubStitueOption.mainMaterialId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialID_Id", 0L);
			batchSubStitueOption.mainMaterialBomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
			batchSubStitueOption.AuxProPid = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropID_Id", 0L);
			batchSubStitueOption.MainReplaceMaterialRows = entityDataObject;
			batchSubStitueOption.ReplaceMaterialRows = entityDataObject2;
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.FormId = "ENG_SubBatchSet";
			dynamicFormShowParameter.CustomComplexParams.Add("BatchSubStitueOption", batchSubStitueOption);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x000361C4 File Offset: 0x000343C4
		public void ShowSynBOMForm()
		{
			if (!this.ValidatePermission("f323992d896745fbaab4a2717c79ce2e"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("没有“物料清单”的修改权限！", "015072000018128", 7, new object[0]), "", 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			if ("C" != DataEntityExtend.GetDynamicValue<string>(dataObject, "DocumentStatus", null))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("替代方案未审核不允许进行同步更新！", "015072000014047", 7, new object[0]), "", 0);
				return;
			}
			if ("A" != DataEntityExtend.GetDynamicValue<string>(dataObject, "ForbidStatus", null))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("替代方案已禁用不允许进行同步更新BOM！", "015072030034348", 7, new object[0]), "", 0);
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			EntryEntity entryEntity2 = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject2 = base.View.Model.GetEntityDataObject(entryEntity2);
			SynBOMOption synBOMOption = new SynBOMOption();
			synBOMOption.ReplacePolicy = DataEntityExtend.GetDynamicValue<string>(dataObject, "ReplacePolicy", null);
			synBOMOption.ReplaceType = DataEntityExtend.GetDynamicValue<string>(dataObject, "ReplaceType", null);
			synBOMOption.SubstitutionId = ((DataEntityExtend.GetDynamicValue<long>(dataObject, "ReplaceNo_Id", 0L) == 0L) ? DataEntityExtend.GetDynamicValue<long>(dataObject, "Id", 0L) : DataEntityExtend.GetDynamicValue<long>(dataObject, "ReplaceNo_Id", 0L));
			synBOMOption.UseOrgId = DataEntityExtend.GetDynamicValue<long>(dataObject, "UseOrgId_Id", 0L);
			synBOMOption.mainMaterialIds = (from x in entityDataObject
			select DataEntityExtend.GetDynamicObjectItemValue<long>(x, "MaterialID_Id", 0L)).ToList<long>();
			synBOMOption.mainEntryIds = (from x in entityDataObject
			select DataEntityExtend.GetDynamicObjectItemValue<long>(x, "Id", 0L)).ToList<long>();
			synBOMOption.MainReplaceMaterialRows = entityDataObject;
			synBOMOption.ReplaceMaterialRows = entityDataObject2;
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.FormId = "ENG_SynBom";
			dynamicFormShowParameter.CustomComplexParams.Add("SynBomOption", synBOMOption);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x0003640C File Offset: 0x0003460C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName == "BatchReplaceEventName")
			{
				IDynamicFormView view = base.View.GetView(e.EventArgs.ToString());
				if (view != null)
				{
					view.Close();
				}
			}
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00036494 File Offset: 0x00034694
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FIsKeyItem")
				{
					this.SetMainItemKey(e.Row);
					return;
				}
				if (key == "FSubIsKeyItem")
				{
					this.SetSubsItemKey(e.Row, e.NewValue);
					return;
				}
				if (key == "FSubMaterialID")
				{
					MFGBillUtil.SetEffectDate(base.View, "FEFFECTDATE", e.Row, 0L);
					return;
				}
				if (key == "FRepType")
				{
					this.DeleteAllEntry();
					return;
				}
				if (!(key == "FNETDEMANDRATE"))
				{
					if (!(key == "FSUBNETDEMANDRATE"))
					{
						return;
					}
				}
				else
				{
					if ((!ObjectUtils.IsNullOrEmpty(this.isImportObj) && StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) || base.Context.ServiceType != null)
					{
						return;
					}
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", -1, null, null);
					decimal d3 = Convert.ToDecimal(e.NewValue);
					DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
					Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from g in dynamicValue
					group g by DataEntityExtend.GetDynamicValue<int>(g, "Priority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
					if (!(value == "3") || dictionary.Keys.Count != 1)
					{
						return;
					}
					using (Dictionary<int, IGrouping<int, DynamicObject>>.Enumerator enumerator = dictionary.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<int, IGrouping<int, DynamicObject>> keyValuePair = enumerator.Current;
							foreach (DynamicObject dynamicObject in keyValuePair.Value)
							{
								bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "SubIsKeyItem", false);
								if (dynamicValue2)
								{
									int num = dynamicValue.IndexOf(dynamicObject);
									base.View.Model.BeginIniti();
									base.View.Model.SetValue("FSUBNETDEMANDRATE", 100m - d3, num);
									base.View.Model.EndIniti();
									base.View.UpdateView("FSUBNETDEMANDRATE", num);
								}
							}
						}
						return;
					}
				}
				if ((ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) && base.Context.ServiceType == null)
				{
					string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", -1, null, null);
					decimal d2 = Convert.ToDecimal(e.NewValue);
					DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
					Dictionary<int, IGrouping<int, DynamicObject>> dictionary2 = (from g in dynamicValue3
					group g by DataEntityExtend.GetDynamicValue<int>(g, "Priority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
					DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "EntityMainItems", null);
					Dictionary<int, IGrouping<int, DynamicObject>> dictionary3 = (from g in dynamicValue4
					group g by DataEntityExtend.GetDynamicValue<int>(g, "MainPriority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
					if (value2 == "3" && dictionary3.Keys.Count == 1 && dictionary2.Keys.Count == 1)
					{
						foreach (KeyValuePair<int, IGrouping<int, DynamicObject>> keyValuePair2 in dictionary3)
						{
							foreach (DynamicObject dynamicObject2 in keyValuePair2.Value)
							{
								bool dynamicValue5 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject2, "IsKeyItem", false);
								if (dynamicValue5)
								{
									int num2 = dynamicValue4.IndexOf(dynamicObject2);
									base.View.Model.BeginIniti();
									base.View.Model.SetValue("FNETDEMANDRATE", 100m - d2, num2);
									base.View.Model.EndIniti();
									base.View.UpdateView("FNETDEMANDRATE", num2);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x0003698C File Offset: 0x00034B8C
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FSubMaterialID") && e.SelectRows.Count > 1)
			{
				e.Cancel = true;
				int num = this.Model.GetEntryCurrentRowIndex("FEntity");
				this.Model.SetValue("FSubMaterialID", e.SelectRows.First<ListSelectedRow>().PrimaryKeyValue, num);
				this.Model.ClearNoDataRow();
				e.SelectRows.Remove(e.SelectRows.First<ListSelectedRow>());
				foreach (ListSelectedRow listSelectedRow in e.SelectRows)
				{
					this.Model.InsertEntryRow("FEntity", ++num);
					this.Model.SetValue("FSubMaterialID", listSelectedRow.PrimaryKeyValue, num);
				}
			}
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00036A80 File Offset: 0x00034C80
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FBomId")
				{
					e.DynamicFormShowParameter.MultiSelect = false;
					e.IsShowApproved = false;
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMaterialID", e.Row, null, null);
					long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMainSupplyOrgId", e.Row, 0L, null);
					e.ListFilterParameter.Filter = this.GetBomIdFilter(e.ListFilterParameter.Filter, value, value2, e.Row);
					return;
				}
				if (fieldKey == "FSubBomId")
				{
					e.DynamicFormShowParameter.MultiSelect = false;
					e.IsShowApproved = false;
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FSubMaterialID", e.Row, null, null);
					long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FSubSupplyOrgId", e.Row, 0L, null);
					e.ListFilterParameter.Filter = this.GetBomIdFilter(e.ListFilterParameter.Filter, value, value2, e.Row);
					return;
				}
				if (!(fieldKey == "FMainSupplyOrgId") && !(fieldKey == "FSubSupplyOrgId"))
				{
					return;
				}
				e.ListFilterParameter.Filter = this.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
			}
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x00036BF0 File Offset: 0x00034DF0
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpper()) != null)
			{
				if (a == "FBomId")
				{
					e.IsShowApproved = false;
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMaterialID", e.Row, null, null);
					long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMainSupplyOrgId", e.Row, 0L, null);
					e.Filter = this.GetBomIdFilter(e.Filter, value, value2, e.Row);
					return;
				}
				if (a == "FSubBomId")
				{
					e.IsShowApproved = false;
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FSubMaterialID", e.Row, null, null);
					long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FSubSupplyOrgId", e.Row, 0L, null);
					e.Filter = this.GetBomIdFilter(e.Filter, value, value2, e.Row);
					return;
				}
				if (!(a == "FMainSupplyOrgId") && !(a == "FSubSupplyOrgId"))
				{
					return;
				}
				e.Filter = this.GetChildSupplyOrgFilter(e.Filter, e.Row);
			}
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x00036D30 File Offset: 0x00034F30
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FReplaceType"))
				{
					if (!(key == "FReplacePolicy"))
					{
						return;
					}
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", -1, null, null);
					string a = Convert.ToString(e.Value);
					if (a != "1" && value == "3")
					{
						e.Cancel = true;
						base.View.ShowErrMessage("替代策略不是混用替代，替代方式不能设置为按比例", "", 0);
					}
				}
				else
				{
					string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FReplacePolicy", -1, null, null);
					string a2 = Convert.ToString(e.Value);
					if (value2 != "1" && a2 == "3")
					{
						e.Cancel = true;
						base.View.ShowErrMessage("替代策略不是混用替代，替代方式不能设置为按比例", "", 0);
						return;
					}
				}
			}
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00036E2B File Offset: 0x0003502B
		private void DeleteAllEntry()
		{
			base.View.Model.DeleteEntryData("FEntity");
			base.View.Model.DeleteEntryData("FEntityMainItems");
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00036E68 File Offset: 0x00035068
		private void SetMainDefKeyItem()
		{
			DynamicObjectCollection source = (DynamicObjectCollection)base.View.Model.DataObject["EntityMainItems"];
			if (!source.Any((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsKeyItem", false)))
			{
				base.View.Model.SetValue("FIsKeyItem", true, 0);
				base.View.UpdateView("FEntityMainItems");
			}
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00036F2C File Offset: 0x0003512C
		private void SetSubsDefKeyItem(int row = 0)
		{
			DynamicObjectCollection subsItems = (DynamicObjectCollection)base.View.Model.DataObject["Entity"];
			if (!subsItems.Any((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "SubIsKeyItem", false)))
			{
				base.View.Model.SetValue("FSubIsKeyItem", true, row);
				base.View.UpdateView("FEntity");
				return;
			}
			IEnumerable<long> source = from x in subsItems
			where subsItems.IndexOf(x) != row
			select OtherExtend.ConvertTo<long>(x["Priority"], 0L);
			if (source.Count<long>() == 0)
			{
				return;
			}
			if (this.Model.GetValue("FSubMaterialID", row) == null)
			{
				this.Model.SetValue("FPriority", source.Max() + 1L, row);
			}
			this.Model.SetValue("FSubIsKeyItem", true, row);
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0003706C File Offset: 0x0003526C
		private void SetMainItemKey(int row)
		{
			int entryRowCount = this.Model.GetEntryRowCount("FEntityMainItems");
			if (entryRowCount > 1)
			{
				if (!MFGBillUtil.GetValue<bool>(this.Model, "FIsKeyItem", row, false, null))
				{
					return;
				}
				for (int i = 0; i < entryRowCount; i++)
				{
					if (i != row)
					{
						base.View.Model.SetValue("FIsKeyItem", false, i);
					}
				}
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x000370F0 File Offset: 0x000352F0
		private void SetSubsItemKey(int row, object newValue)
		{
			if (!(bool)newValue)
			{
				return;
			}
			int priority = MFGBillUtil.GetValue<int>(base.View.Model, "FPriority", row, 0, null);
			Entity entryEntity = base.View.Model.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			IEnumerable<DynamicObject> enumerable = (from w in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "Priority", 0) == priority
			select w).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				int rowIndex = base.View.Model.GetRowIndex(entryEntity, dynamicObject);
				if (rowIndex != row)
				{
					base.View.Model.SetValue("FSubIsKeyItem", false, rowIndex);
				}
			}
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x000371E4 File Offset: 0x000353E4
		private void PriorityChanged(DynamicObject dyObj, dynamic obj)
		{
			Entity entryEntity = base.View.Model.BusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dyObj, "MainPriority", 0);
			DataEntityExtend.GetDynamicObjectItemValue<int>(dyObj, "Seq", 0);
			for (int i = 0; i < entityDataObject.Count; i++)
			{
				if (i != DataEntityExtend.GetDynamicObjectItemValue<int>(dyObj, "Seq", 0) - 1)
				{
					base.View.Model.SetValue("FMainPriority", dynamicObjectItemValue, i);
				}
			}
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00037278 File Offset: 0x00035478
		private string GetChildSupplyOrgFilter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			List<long> list = new List<long>
			{
				102L,
				112L,
				101L,
				104L,
				103L,
				109L
			};
			List<long> list2 = new List<long>();
			List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(base.Context, value, list);
			if (orgByBizRelationOrgs == null || orgByBizRelationOrgs.Count < 1)
			{
				return filter;
			}
			list2.AddRange(orgByBizRelationOrgs);
			list2.Add(value);
			filter += ((filter.Length > 0) ? (" AND " + string.Format("FORGID in ({0})", string.Join<long>(",", list2))) : string.Format("FORGID in ({0})", string.Join<long>(",", list2)));
			return filter;
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00037368 File Offset: 0x00035568
		private string GetBomIdFilter(string filter, DynamicObject Mtrl, long supplyOrgId, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			string text = string.Empty;
			text = " 1=0 ";
			if (Mtrl != null)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(Mtrl, "MsterId", 0L);
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
				object baseDataPkId = BaseDataServiceHelper.GetBaseDataPkId(base.Context, formMetadata.BusinessInfo, "BD_MATERIAL", dynamicObjectItemValue, supplyOrgId);
				if (baseDataPkId != null && baseDataPkId is DynamicObject)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(((DynamicObject)baseDataPkId)[0]));
				}
				else if (baseDataPkId != null && baseDataPkId is long)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(baseDataPkId));
				}
			}
			filter += ((filter.Length > 0) ? (" AND " + text) : text);
			return filter;
		}
	}
}
