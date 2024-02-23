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
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C8 RID: 200
	public class ExpireCobyControler : AbstractCobyControler
	{
		// Token: 0x06000E59 RID: 3673 RVA: 0x000A5F85 File Offset: 0x000A4185
		public override void DoOperation()
		{
			this.ShowBomList();
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x000A5FDC File Offset: 0x000A41DC
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

		// Token: 0x06000E5B RID: 3675 RVA: 0x000A61AC File Offset: 0x000A43AC
		protected override void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyChangeLabel", 4);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyEcnStatus", "0");
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyECNRowType", 15);
			DateTime dateTime = Convert.ToDateTime(MFGServiceHelper.GetSysDate(base.View.Context).ToString("D"));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "EXPIREDATECOBY", dateTime);
		}
	}
}
