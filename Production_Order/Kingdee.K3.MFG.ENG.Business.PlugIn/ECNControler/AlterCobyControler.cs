using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000BE RID: 190
	public class AlterCobyControler : AbstractCobyControler
	{
		// Token: 0x06000E1F RID: 3615 RVA: 0x000A371C File Offset: 0x000A191C
		public override void DoOperation()
		{
			this.ShowBomList();
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x000A3774 File Offset: 0x000A1974
		public override void AddEntryRow(ListSelectedRowCollection collection, int startIndex)
		{
			Dictionary<string, DynamicObject> bomHeadObjectBySelectedRows = base.GetBomHeadObjectBySelectedRows(collection);
			object[] array = (from element in collection
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(element.EntryPrimaryKeyValue)
			select element into x
			select x.EntryPrimaryKeyValue).ToArray<object>();
			DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.View.Context, array, base.BomCobyEntity.DynamicObjectType);
			if (ListUtils.IsEmpty<DynamicObject>(array2))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("过滤界面联副产品未勾选，选择数据无效!", "0151515153499030041415", 7, new object[0]), "", 0);
				return;
			}
			Entity cobyEntity = base.View.BusinessInfo.GetEntity("FCobyEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(cobyEntity);
			startIndex = base.Model.GetEntryRowCount("FCobyEntity");
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject sourceBomEntry in array2)
			{
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FCobyEntity", startIndex);
				DynamicObject entityDataObject2 = base.Model.GetEntityDataObject(cobyEntity, startIndex);
				this.FillEntryValue(entityDataObject2, sourceBomEntry, 0);
				this.FillBomHeadObjectValue(entityDataObject2, bomHeadObjectBySelectedRows, -1L, startIndex++);
				list.Add(entityDataObject2);
				((AbstractDynamicFormModel)base.Model).InsertEntryRow("FCobyEntity", startIndex);
				DynamicObject entityDataObject3 = base.Model.GetEntityDataObject(cobyEntity, startIndex);
				this.FillEntryValue(entityDataObject3, sourceBomEntry, 1);
				this.FillBomHeadObjectValue(entityDataObject3, bomHeadObjectBySelectedRows, -1L, startIndex++);
				this.SetECNGoup(new DynamicObject[]
				{
					entityDataObject2,
					entityDataObject3
				});
				list.Add(entityDataObject3);
			}
			list.ForEach(delegate(DynamicObject i)
			{
				this.View.UpdateView("FCobyEntity", this.Model.GetRowIndex(cobyEntity, i));
			});
			base.SortCoby(entityDataObject);
			base.SummaryUpdtBOMVers();
			base.View.RuleContainer.RaiseInitialized("FCobyEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x000A399E File Offset: 0x000A1B9E
		protected void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry, int changeLabel)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyChangeLabel", changeLabel);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyEcnStatus", "0");
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyECNRowType", 13);
		}
	}
}
