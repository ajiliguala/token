using System;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000CA RID: 202
	public class NewCobyControler : AbstractCobyControler
	{
		// Token: 0x06000E66 RID: 3686 RVA: 0x000A67AA File Offset: 0x000A49AA
		public override void DoOperation()
		{
			this.AddEntryRow(null, -1);
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x000A67B4 File Offset: 0x000A49B4
		public override void AddEntryRow(ListSelectedRowCollection selectedRows, int startIndex)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FCobyEntity");
			int entryRowCount = base.Model.GetEntryRowCount("FCobyEntity");
			DynamicObject dynamicObject = DataEntityExtend.CreateNewEntryRow(base.Model, entity, entryRowCount, ref entryRowCount);
			DateTime dynamicValue = DataEntityExtend.GetDynamicValue<DateTime>(base.Model.DataObject, "EffectDate", default(DateTime));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "CobyChangeLabel", 2);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EFFECTDATECOBY", dynamicValue);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "CobyECNRowType", 12);
			base.SortCoby(base.Model.GetEntityDataObject(entity));
			this.SetECNGoup(new DynamicObject[]
			{
				dynamicObject
			});
			base.View.UpdateView("FCobyEntity", entryRowCount);
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(entity);
			base.View.RuleContainer.RaiseInitialized("FCobyEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x000A68BC File Offset: 0x000A4ABC
		public override void DataChanged(DataChangedEventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FCobyEntity");
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FCobyBomVersion"))
				{
					return;
				}
				base.Model.GetEntityDataObject(entity, e.Row);
				base.SummaryUpdtBOMVers();
			}
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x000A6918 File Offset: 0x000A4B18
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FCobyBomVersion"))
				{
					return;
				}
				if (base.UseECREntryBomId && !ListUtils.IsEmpty<long>(base.ECREntryBomId))
				{
					e.ListFilterParameter.Filter = StringUtils.JoinFilterString(e.ListFilterParameter.Filter, string.Format("FId in ({0})", string.Join<long>(",", base.ECREntryBomId)), "AND");
				}
				e.ListFilterParameter.Filter = StringUtils.JoinFilterString(e.ListFilterParameter.Filter, " FMaterialId.FIsECN='1'", "AND");
			}
		}
	}
}
