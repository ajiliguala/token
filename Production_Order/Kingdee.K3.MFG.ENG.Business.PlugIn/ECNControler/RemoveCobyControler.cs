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
	// Token: 0x020000CD RID: 205
	public class RemoveCobyControler : AbstractCobyControler
	{
		// Token: 0x06000E77 RID: 3703 RVA: 0x000A7348 File Offset: 0x000A5548
		public override void DoOperation()
		{
			this.ShowBomList();
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x000A73A0 File Offset: 0x000A55A0
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
				this.FillEntryValue(entityDataObject2, sourceBomEntry);
				this.FillBomHeadObjectValue(entityDataObject2, bomHeadObjectBySelectedRows, -1L, startIndex++);
				this.SetECNGoup(new DynamicObject[]
				{
					entityDataObject2
				});
				list.Add(entityDataObject2);
			}
			base.SortCoby(entityDataObject);
			base.SummaryUpdtBOMVers();
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FCobyEntity", this.Model.GetRowIndex(cobyEntity, x));
			});
			base.View.RuleContainer.RaiseInitialized("FCobyEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x000A756D File Offset: 0x000A576D
		protected override void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyChangeLabel", 3);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyEcnStatus", "0");
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyECNRowType", 14);
		}
	}
}
