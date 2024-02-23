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
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C0 RID: 192
	public class ChooseRepControler : AbstractItemControler
	{
		// Token: 0x06000E30 RID: 3632 RVA: 0x000A44B8 File Offset: 0x000A26B8
		public override void AddEntryRow(ListSelectedRowCollection lsr, int startIndex)
		{
			Dictionary<long, long> entryIdForChooseRep = ECNOrderServiceHelper.GetEntryIdForChooseRep(base.View.Context, lsr);
			Dictionary<string, DynamicObject> bomHeadObjectBySelectedRows = base.GetBomHeadObjectBySelectedRows(lsr);
			if (ListUtils.IsEmpty<KeyValuePair<long, long>>(entryIdForChooseRep))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("过滤界面子项明细未勾选，选择数据无效!", "015072000033347", 7, new object[0]), "", 0);
				return;
			}
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(treeEntity);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.View.Context, entryIdForChooseRep.Values.Distinct<long>().Cast<object>().ToArray<object>(), base.BomTreeEntity.DynamicObjectType);
			int num = (startIndex >= 0) ? (startIndex + 1) : base.Model.GetEntryRowCount("FTreeEntity");
			List<DynamicObject> list = new List<DynamicObject>();
			DynamicObject[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				DynamicObject selectedBomEntry = array2[i];
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
				DynamicObject entityDataObject2 = base.Model.GetEntityDataObject(treeEntity, num);
				this.FillEntryValue(entityDataObject2, selectedBomEntry);
				if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(selectedBomEntry, "BOMCHILDLOTBASEDQTY", null)) && DataEntityExtend.GetDynamicValue<long>(selectedBomEntry, "MaterialType", 0L) == 1L && DataEntityExtend.GetDynamicValue<long>(entityDataObject2, "ChangeLabel", 0L) == 5L)
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FStairDosage");
					DynamicObjectCollection entityDataObject3 = base.Model.GetEntityDataObject(entity);
					int num2 = 1;
					foreach (DynamicObject sourceBomChildEntry in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(selectedBomEntry, "BOMCHILDLOTBASEDQTY", null))
					{
						base.FillStairDosageEntry(entityDataObject3, sourceBomChildEntry, num2);
						num2++;
					}
				}
				this.FillBomHeadObjectValue(entityDataObject2, bomHeadObjectBySelectedRows, (from f in entryIdForChooseRep
				where f.Value == DataEntityExtend.GetDynamicValue<long>(selectedBomEntry, "Id", 0L)
				select f).FirstOrDefault<KeyValuePair<long, long>>().Key, num);
				num++;
				list.Add(entityDataObject2);
			}
			num--;
			base.SortItem(entityDataObject);
			base.SummaryUpdtBOMVers();
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FTreeEntity", this.Model.GetRowIndex(treeEntity, x));
			});
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x000A47BC File Offset: 0x000A29BC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMATERIALIDCHILD"))
				{
					return;
				}
				e.Cancel = true;
				ListShowParameter listShowParameter = new ListShowParameter
				{
					FormId = "ENG_BOM",
					PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
					IsLookUp = true,
					IsShowApproved = true,
					IsShowUsed = true,
					UseOrgId = DataEntityExtend.GetDynamicValue<long>(base.Model.DataObject, "ChangeOrgId_Id", 0L)
				};
				listShowParameter.ListFilterParameter.Filter = " FMaterialId.FIsECN='1' AND FMATERIALTYPE IN ('1','3') AND FUSEORGID = FCREATEORGID ";
				string filterString = this.GetFilterString();
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(filterString))
				{
					IRegularFilterParameter listFilterParameter = listShowParameter.ListFilterParameter;
					listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? filterString : (" AND " + filterString));
				}
				if (base.UseECREntryBomId && !ListUtils.IsEmpty<long>(base.ECREntryBomId))
				{
					IRegularFilterParameter listFilterParameter2 = listShowParameter.ListFilterParameter;
					listFilterParameter2.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? filterString : string.Format(" AND  FID IN ({0})", string.Join<long>(",", base.ECREntryBomId)));
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
					this.AddEntryRow(collection, e.Row);
				});
			}
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x000A4970 File Offset: 0x000A2B70
		protected string GetFilterString()
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(entity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return string.Empty;
			}
			List<long> list = (from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<long>(x, "BomEntryId", 0L) into x
			where x > 0L
			select x).Distinct<long>().ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return string.Empty;
			}
			return string.Format(" t1.FEntryId NOT IN(SELECT  TB.FENTRYID\r\nFROM    T_ENG_BOMCHILD TA\r\n        INNER JOIN T_ENG_BOMCHILD TB ON TA.FID = TB.FID AND TA.FREPLACEGROUP = TB.FREPLACEGROUP\r\nwhere TA.FENTRYID in ({0}) ) ", string.Join<long>(",", list));
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x000A4A24 File Offset: 0x000A2C24
		public override void DoOperation()
		{
			this.BeforeF7Select(new BeforeF7SelectEventArgs
			{
				FieldKey = "FMATERIALIDCHILD",
				Row = -1
			});
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x000A4A50 File Offset: 0x000A2C50
		protected override void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "RowType", 5);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChangeLabel", 5);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry, "SUPPLYORG_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomEntry, "SUPPLYORG", null));
			this.SetECNGoup(new DynamicObject[]
			{
				targetRow
			});
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x000A4ACD File Offset: 0x000A2CCD
		public override void SetRowFieldControl(DynamicObject dynObj)
		{
		}
	}
}
