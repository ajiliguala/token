using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000CB RID: 203
	public class NewItemControler : AbstractItemControler
	{
		// Token: 0x06000E6B RID: 3691 RVA: 0x000A6A0C File Offset: 0x000A4C0C
		public override void AddEntryRow(ListSelectedRowCollection selectedRows, int startIndex)
		{
			if (selectedRows == null)
			{
				selectedRows = new ListSelectedRowCollection();
				selectedRows.Add(new ListSelectedRow("0", "0", 0, ""));
			}
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			int num = base.Model.GetEntryRowCount("FTreeEntity");
			DynamicObject dynamicObject = null;
			if (startIndex > -1)
			{
				dynamicObject = base.Model.GetEntityDataObject(treeEntity, startIndex);
				num = startIndex;
				base.Model.DeleteEntryRow("FTreeEntity", num);
			}
			new List<long>();
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(treeEntity);
			DateTime dateTime = DataEntityExtend.GetDynamicValue<DateTime>(base.Model.DataObject, "EffectDate", default(DateTime));
			if (Convert.ToString(base.View.Model.GetValue("FChangeType")) == "1")
			{
				dateTime = MFGBillUtil.GetEffectData(base.View, 0L);
			}
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (ListSelectedRow listSelectedRow in selectedRows)
			{
				DynamicObject dynamicObject2 = DataEntityExtend.CreateNewEntryRow(base.Model, treeEntity, num, ref num);
				if (startIndex > -1)
				{
					DataEntityExtend.CopyPropertyValues(dynamicObject2, dynamicObject, new Func<GetFieldValueCallbackParam, object>(base.GetCHFieldValueCallback), null);
				}
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "RowType", 1);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ChangeLabel", 2);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ECNRowId", SequentialGuid.NewGuid().ToString());
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "EFFECTDATE", dateTime);
				this.SetECNGoup(new DynamicObject[]
				{
					dynamicObject2
				});
				if (StringUtils.EqualsIgnoreCase(listSelectedRow.FormID, "BD_Material"))
				{
					base.Model.SetValue("FMATERIALIDCHILD", listSelectedRow.PrimaryKeyValue, num);
				}
				else if (StringUtils.EqualsIgnoreCase(listSelectedRow.FormID, "ENG_BOM"))
				{
					base.Model.SetValue("FBOMVERSION", listSelectedRow.PrimaryKeyValue, num);
				}
				base.Model.SetValue("FSUPPLYORG", 0, num);
				list.Add(dynamicObject2);
				num++;
			}
			num--;
			base.SortItem(entityDataObject);
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FTreeEntity", this.Model.GetRowIndex(treeEntity, x));
				this.RowControlBuffer.Enqueue(x);
			});
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
			((IDynamicFormViewService)base.View).CustomEvents("FTreeEntity", "RowEdiableEvent", num.ToString());
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x000A6CE8 File Offset: 0x000A4EE8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			if ((key = e.BaseDataField.Key) != null)
			{
				if (key == "FMATERIALIDCHILD")
				{
					e.ListFilterParameter.Filter = string.Format(" {0}{1}{2}", e.ListFilterParameter.Filter, ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : " AND ", base.SetChildMaterilIdFilterString(e.Row));
					return;
				}
				if (key == "FBOMVERSION")
				{
					e.ListFilterParameter.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? " FMaterialId.FIsECN='1' " : " AND FMaterialId.FIsECN='1' ");
					return;
				}
				if (key == "FChildSupplyOrgId")
				{
					e.ListFilterParameter.Filter = base.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
					return;
				}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x000A6DD4 File Offset: 0x000A4FD4
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FBOMVERSION"))
				{
					return;
				}
				e.Filter = StringUtils.JoinFilterString(e.Filter, "FMaterialId.FIsECN='1'", "AND");
			}
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x000A6E1C File Offset: 0x000A501C
		public override void AfterF7Select(AfterF7SelectEventArgs e, int rowIndex)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMATERIALIDCHILD") && !(fieldKey == "FBOMVERSION"))
				{
					return;
				}
				if (e.SelectRows.Count > 1)
				{
					e.Cancel = true;
					this.AddEntryRow(e.SelectRows, rowIndex);
				}
			}
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x000A6E70 File Offset: 0x000A5070
		public override void DoOperation()
		{
			this.AddEntryRow(null, -1);
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x000A6E7A File Offset: 0x000A507A
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string key = e.Key;
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x000A6ECC File Offset: 0x000A50CC
		public override void DataChanged(DataChangedEventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (a == "FBOMVERSION")
				{
					DynamicObject entityDataObject = base.Model.GetEntityDataObject(entity, e.Row);
					base.SummaryUpdtBOMVers();
					base.RowControlBuffer.Enqueue(entityDataObject);
					((IDynamicFormViewService)base.View).CustomEvents("FTreeEntity", "RowEdiableEvent", e.Row.ToString());
					return;
				}
				if (!(a == "FINSERTROW"))
				{
					return;
				}
				if (Convert.ToInt32(e.NewValue) < 0)
				{
					return;
				}
				DynamicObject obj = base.Model.GetEntityDataObject(entity, e.Row);
				if (DataEntityExtend.GetDynamicValue<string>(obj, "MATERIALTYPE", null) == "3")
				{
					return;
				}
				DynamicObjectCollection entityDataObject2 = base.Model.GetEntityDataObject(entity);
				IEnumerable<DynamicObject> enumerable = from f in entityDataObject2
				where DataEntityExtend.GetDynamicValue<string>(f, "ECNGroup", null) == DataEntityExtend.GetDynamicValue<string>(obj, "ECNGroup", null) && DataEntityExtend.GetDynamicValue<string>(f, "MATERIALTYPE", null) == "3"
				select f;
				if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
				{
					foreach (DynamicObject item in enumerable)
					{
						base.Model.SetValue("FInsertRow", e.NewValue, entityDataObject2.IndexOf(item));
					}
				}
			}
		}
	}
}
