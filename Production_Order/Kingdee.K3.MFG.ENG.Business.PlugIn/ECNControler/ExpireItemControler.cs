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
	// Token: 0x020000C9 RID: 201
	public class ExpireItemControler : AbstractItemControler
	{
		// Token: 0x06000E5F RID: 3679 RVA: 0x000A6270 File Offset: 0x000A4470
		public override void AddEntryRow(ListSelectedRowCollection lsr, int startIndex)
		{
			Dictionary<string, DynamicObject> bomHeadObjectBySelectedRows = base.GetBomHeadObjectBySelectedRows(lsr);
			Dictionary<long, long> bomChildReplaceKeyItemIds = ECNOrderServiceHelper.GetBomChildReplaceKeyItemIds(base.View.Context, lsr);
			if (ListUtils.IsEmpty<KeyValuePair<long, long>>(bomChildReplaceKeyItemIds))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("过滤界面子项明细未勾选，选择数据无效!", "015072000033347", 7, new object[0]), "", 0);
				return;
			}
			object[] array = (from x in bomChildReplaceKeyItemIds.Keys
			select x).ToArray<object>();
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(treeEntity);
			DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.View.Context, array, base.BomTreeEntity.DynamicObjectType);
			int num = (startIndex >= 0) ? (startIndex + 1) : base.Model.GetEntryRowCount("FTreeEntity");
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in array2)
			{
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
				long parentRowId = 0L;
				bomChildReplaceKeyItemIds.TryGetValue(OtherExtend.ConvertTo<long>(dynamicObject["Id"], 0L), out parentRowId);
				DynamicObject entityDataObject2 = base.Model.GetEntityDataObject(treeEntity, num);
				DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "SUPPLYORG", null);
				this.FillEntryValue(entityDataObject2, dynamicObject);
				if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null)))
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FStairDosage");
					DynamicObjectCollection entityDataObject3 = base.Model.GetEntityDataObject(entity);
					int num2 = 1;
					foreach (DynamicObject sourceBomChildEntry in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "BOMCHILDLOTBASEDQTY", null))
					{
						base.FillStairDosageEntry(entityDataObject3, sourceBomChildEntry, num2);
						num2++;
					}
				}
				this.FillBomHeadObjectValue(entityDataObject2, bomHeadObjectBySelectedRows, parentRowId, num);
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
			base.View.UpdateView("FStairDosage");
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
			((IDynamicFormViewService)base.View).CustomEvents("FTreeEntity", "RowEdiableEvent", num.ToString());
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x000A6570 File Offset: 0x000A4770
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
				listShowParameter.ListFilterParameter.Filter = " FMaterialId.FIsECN='1' ";
				string selectedBomEntryFilterString = base.GetSelectedBomEntryFilterString();
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(selectedBomEntryFilterString))
				{
					IRegularFilterParameter listFilterParameter = listShowParameter.ListFilterParameter;
					listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? selectedBomEntryFilterString : (" AND " + selectedBomEntryFilterString));
				}
				if (base.UseECREntryBomId && !ListUtils.IsEmpty<long>(base.ECREntryBomId))
				{
					IRegularFilterParameter listFilterParameter2 = listShowParameter.ListFilterParameter;
					listFilterParameter2.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? selectedBomEntryFilterString : string.Format(" AND  FID IN ({0})", string.Join<long>(",", base.ECREntryBomId)));
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

		// Token: 0x06000E61 RID: 3681 RVA: 0x000A6710 File Offset: 0x000A4910
		public override void DoOperation()
		{
			this.BeforeF7Select(new BeforeF7SelectEventArgs
			{
				FieldKey = "FMATERIALIDCHILD",
				Row = -1
			});
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x000A673C File Offset: 0x000A493C
		protected override void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "RowType", 4);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChangeLabel", 4);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "EXPIREDATE", base.Model.GetValue("FEffectDate0"));
			this.SetECNGoup(new DynamicObject[]
			{
				targetRow
			});
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x000A67A0 File Offset: 0x000A49A0
		public override void SetRowFieldControl(DynamicObject dynObj)
		{
		}
	}
}
