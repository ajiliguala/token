using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000BF RID: 191
	public class AlterItemControler : AbstractItemControler
	{
		// Token: 0x06000E25 RID: 3621 RVA: 0x000A3A70 File Offset: 0x000A1C70
		public override void AddEntryRow(ListSelectedRowCollection lsr, int startIndex)
		{
			Dictionary<string, DynamicObject> bomHeadObjectBySelectedRows = base.GetBomHeadObjectBySelectedRows(lsr);
			object[] array = (from element in lsr
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(element.EntryPrimaryKeyValue)
			select element into x
			select x.EntryPrimaryKeyValue).ToArray<object>();
			DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.View.Context, array, base.BomTreeEntity.DynamicObjectType);
			if (ListUtils.IsEmpty<DynamicObject>(array2))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("过滤界面子项明细未勾选，选择数据无效!", "015072000033347", 7, new object[0]), "", 0);
				return;
			}
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(treeEntity);
			if (startIndex >= 0)
			{
				new List<DynamicObject>
				{
					base.Model.GetEntityDataObject(treeEntity, startIndex),
					base.Model.GetEntityDataObject(treeEntity, startIndex + 1)
				}.ForEach(delegate(DynamicObject r)
				{
					this.Model.DeleteEntryRow("FTreeEntity", this.Model.GetRowIndex(treeEntity, r));
				});
			}
			else
			{
				startIndex = base.Model.GetEntryRowCount("FTreeEntity");
			}
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in array2)
			{
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", startIndex);
				DynamicObject entityDataObject2 = base.Model.GetEntityDataObject(treeEntity, startIndex);
				DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "SUPPLYORG", null);
				this.FillEntryValue(entityDataObject2, dynamicObject, 0);
				if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null)))
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FStairDosage");
					DynamicObjectCollection entityDataObject3 = base.Model.GetEntityDataObject(entity);
					int num = 1;
					foreach (DynamicObject sourceBomChildEntry in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null))
					{
						base.FillStairDosageEntry(entityDataObject3, sourceBomChildEntry, num);
						num++;
					}
				}
				this.FillBomHeadObjectValue(entityDataObject2, bomHeadObjectBySelectedRows, -1L, startIndex++);
				list.Add(entityDataObject2);
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", startIndex);
				DynamicObject entityDataObject4 = base.Model.GetEntityDataObject(treeEntity, startIndex);
				DataEntityExtend.SetDynamicObjectItemValue(entityDataObject4, "SUPPLYORG", null);
				this.FillEntryValue(entityDataObject4, dynamicObject, 1);
				if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null)))
				{
					Entity entity2 = base.View.BusinessInfo.GetEntity("FStairDosage");
					DynamicObjectCollection entityDataObject5 = base.Model.GetEntityDataObject(entity2);
					int num2 = 1;
					foreach (DynamicObject sourceBomChildEntry2 in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null))
					{
						base.FillStairDosageEntry(entityDataObject5, sourceBomChildEntry2, num2);
						num2++;
					}
				}
				this.FillBomHeadObjectValue(entityDataObject4, bomHeadObjectBySelectedRows, -1L, startIndex++);
				if (DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null).Count <= 0 && DataEntityExtend.GetDynamicValue<long>(dynamicObject, "DOSAGETYPE", 0L) == 3L)
				{
					base.CreateStairDosageEntity(DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject4, "MATERIALIDCHILD", null), DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject4, "ParentMaterialId", null), DataEntityExtend.GetDynamicValue<long>(entityDataObject4, "ParentUnitID_Id", 0L), DataEntityExtend.GetDynamicValue<long>(entityDataObject4, "ParentBaseUnitId_Id", 0L), DataEntityExtend.GetDynamicValue<long>(entityDataObject4, "ParentMaterialId_Id", 0L));
				}
				this.SetECNGoup(new DynamicObject[]
				{
					entityDataObject2,
					entityDataObject4
				});
				list.Add(entityDataObject4);
			}
			base.SortItem(entityDataObject);
			base.SummaryUpdtBOMVers();
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FTreeEntity", this.Model.GetRowIndex(treeEntity, x));
				this.RowControlBuffer.Enqueue(x);
			});
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
			startIndex--;
			((IDynamicFormViewService)base.View).CustomEvents("FTreeEntity", "RowEdiableEvent", startIndex.ToString());
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x000A3ED4 File Offset: 0x000A20D4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			if ((key = e.BaseDataField.Key) != null)
			{
				if (!(key == "FMATERIALIDCHILD"))
				{
					if (key == "FChildSupplyOrgId")
					{
						e.ListFilterParameter.Filter = base.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
						return;
					}
				}
				else
				{
					int num = OtherExtend.ConvertTo<int>(base.Model.GetValue("FChangeLabel", e.Row), 0);
					if (num == 1)
					{
						((ListShowParameter)e.DynamicFormShowParameter).MultiSelect = false;
						e.ListFilterParameter.Filter = string.Format(" {0}{1}{2}", e.ListFilterParameter.Filter, ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : " AND ", base.SetChildMaterilIdFilterString(e.Row));
						return;
					}
					e.Cancel = true;
					DynamicObject dynamicObject = base.Model.GetValue("FBOMVERSION", e.Row) as DynamicObject;
					if (dynamicObject != null)
					{
						string.Format("FID = {0}", dynamicObject["Id"]);
						this.ShowBomList(e.Row);
						return;
					}
					return;
				}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x000A4005 File Offset: 0x000A2205
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string key = e.Key;
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x000A4010 File Offset: 0x000A2210
		public override void DataChanged(DataChangedEventArgs e)
		{
			int num = OtherExtend.ConvertTo<int>(base.Model.GetValue("FChangeLabel", e.Row), 0);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FMATERIALIDCHILD"))
				{
					if (!(key == "FIsChangeMtrl"))
					{
						return;
					}
					if (num == 1)
					{
						base.Model.SetValue("FChildSupplyOrgId", 0, e.Row);
						int num2 = OtherExtend.ConvertTo<int>(base.Model.GetValue("FDOSAGETYPE", e.Row), 0);
						if (num2 == 3)
						{
							base.Model.SetValue("FDOSAGETYPE", 2, e.Row);
						}
					}
				}
				else if (num == 1)
				{
					DynamicObject dynamicObject = base.Model.GetValue("FMATERIALIDCHILD") as DynamicObject;
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
					if (dynamicValue != OtherExtend.ConvertTo<long>(e.NewValue, 0L))
					{
						base.Model.SetValue("FIsChangeMtrl", true, e.Row);
						return;
					}
				}
			}
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x000A411F File Offset: 0x000A231F
		public override void DoOperation()
		{
			this.ShowBomList(-1);
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x000A4164 File Offset: 0x000A2364
		private void ShowBomList(int rowIndex)
		{
			ListShowParameter listShowParameter = new ListShowParameter
			{
				FormId = "ENG_BOM",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				IsLookUp = true,
				IsShowApproved = true,
				IsShowUsed = true,
				UseOrgId = DataEntityExtend.GetDynamicValue<long>(base.Model.DataObject, "ChangeOrgId_Id", 0L)
			};
			int num = OtherExtend.ConvertTo<int>(base.Model.DataObject["ChangeType"], 0);
			listShowParameter.ListFilterParameter.Filter = " FMaterialId.FIsECN='1' ";
			switch (num)
			{
			case 0:
			{
				string text = " EXISTS(SELECT 1 FROM (\r\nSELECT FID, FREPLACEGROUP, COUNT(FENTRYID) AS FCOUNT\r\n  FROM T_ENG_BOMchild\r\n where fmaterialtype = '1'\r\n GROUP BY FID, FREPLACEGROUP) TF WHERE TF.FID = T0.FID AND t1.FREPLACEGROUP = TF.FREPLACEGROUP AND TF.FCOUNT = 1 AND T1.FMATERIALTYPE <> '3')";
				IRegularFilterParameter listFilterParameter = listShowParameter.ListFilterParameter;
				listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? text : (" AND " + text));
				break;
			}
			case 2:
			{
				string text = " (FReplacePolicy <> '1' AND FReplacePolicy <> '2' AND FReplacePolicy <> '3') ";
				IRegularFilterParameter listFilterParameter2 = listShowParameter.ListFilterParameter;
				listFilterParameter2.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? text : (" AND " + text));
				break;
			}
			}
			if (rowIndex > -1)
			{
				DynamicObject dynamicObject = base.Model.GetValue("FBOMVERSION", rowIndex) as DynamicObject;
				if (dynamicObject != null)
				{
					string text2 = string.Format("FID = {0}", dynamicObject["Id"]);
					IRegularFilterParameter listFilterParameter3 = listShowParameter.ListFilterParameter;
					listFilterParameter3.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? text2 : (" AND " + text2));
				}
			}
			string selectedBomEntryFilterString = base.GetSelectedBomEntryFilterString();
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(selectedBomEntryFilterString))
			{
				IRegularFilterParameter listFilterParameter4 = listShowParameter.ListFilterParameter;
				listFilterParameter4.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? selectedBomEntryFilterString : (" AND " + selectedBomEntryFilterString));
			}
			if (base.UseECREntryBomId && !ListUtils.IsEmpty<long>(base.ECREntryBomId))
			{
				IRegularFilterParameter listFilterParameter5 = listShowParameter.ListFilterParameter;
				listFilterParameter5.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? selectedBomEntryFilterString : string.Format(" AND  FID IN ({0})", string.Join<long>(",", base.ECREntryBomId)));
			}
			listShowParameter.CustomParams.Add("FromEcnOrder", "true");
			listShowParameter.CustomParams.Add("ECNTreeEntity", "true");
			base.View.ShowForm(listShowParameter, delegate(FormResult x)
			{
				if (x.ReturnData == null)
				{
					return;
				}
				ListSelectedRowCollection collection = x.ReturnData as ListSelectedRowCollection;
				this.AddEntryRow(collection, rowIndex);
			});
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x000A4404 File Offset: 0x000A2604
		protected void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry, int changeLabel)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "RowType", 2);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChangeLabel", changeLabel);
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x000A4430 File Offset: 0x000A2630
		public override void SetRowFieldControl(DynamicObject dynObj)
		{
			if (DataEntityExtend.GetDynamicValue<int>(dynObj, "ChangeLabel", 0) == 0)
			{
				return;
			}
			base.SetRowFieldControl(dynObj);
		}
	}
}
