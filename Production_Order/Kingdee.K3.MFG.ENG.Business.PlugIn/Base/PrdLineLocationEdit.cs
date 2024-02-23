using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200002C RID: 44
	[Description("生产线工位_表单插件")]
	public class PrdLineLocationEdit : BaseControlEdit
	{
		// Token: 0x06000303 RID: 771 RVA: 0x000231AD File Offset: 0x000213AD
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
		}

		// Token: 0x06000304 RID: 772 RVA: 0x000231B8 File Offset: 0x000213B8
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (string.Compare(e.Entity.Key, "FEntity", true) == 0)
			{
				this.Model.GetEntityDataObject(e.Entity);
				this.Model.SetValue("FParentWIPWarehouse", this.Model.GetValue("FWIPWarehouse"), e.Row);
			}
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0002321C File Offset: 0x0002141C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FOnLineLocation") && !(key == "FOffLineLocation") && !(key == "FQualifiedSLocation") && !(key == "FDefaultPickingLocation") && !(key == "FBackflushLocation"))
				{
					return;
				}
				bool flag;
				if (e.NewValue.GetType() == typeof(bool))
				{
					flag = Convert.ToBoolean(e.NewValue);
				}
				else
				{
					if (!(e.NewValue.GetType() == typeof(int)))
					{
						return;
					}
					flag = ((int)e.NewValue == 1);
				}
				if (flag)
				{
					EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					int num = -1;
					foreach (DynamicObject dynamicObject in entityDataObject)
					{
						num++;
						if (num != e.Row)
						{
							base.View.Model.SetValue(e.Field.Key, false, num);
						}
					}
				}
			}
		}

		// Token: 0x06000306 RID: 774 RVA: 0x00023388 File Offset: 0x00021588
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "Save") && !(a == "Submit"))
				{
					return;
				}
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				if (entityDataObject == null || entityDataObject.Count == 0)
				{
					return;
				}
				DynamicObject dynamicObject = (from o in entityDataObject
				orderby DataEntityExtend.GetDynamicObjectItemValue<string>(o, "LocationCode", null) descending
				select o).FirstOrDefault<DynamicObject>();
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "LocationCode", null);
				long num = (long)base.View.Model.GetEntryRowCount("FEntity");
				int num2 = 0;
				while ((long)num2 < num)
				{
					if (string.CompareOrdinal(dynamicObjectItemValue, MFGBillUtil.GetValue<string>(base.View.Model, "FLocationCode", num2, null, null)) == 0)
					{
						base.View.Model.SetValue("FQualifiedSLocation", true, num2);
					}
					else
					{
						base.View.Model.SetValue("FQualifiedSLocation", false, num2);
					}
					num2++;
				}
			}
		}

		// Token: 0x06000307 RID: 775 RVA: 0x000234C4 File Offset: 0x000216C4
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbDeleteEntry"))
				{
					return;
				}
				this.IsQuoted(e);
			}
		}

		// Token: 0x06000308 RID: 776 RVA: 0x000234F8 File Offset: 0x000216F8
		private void IsQuoted(BarItemClickEventArgs e)
		{
			int index = base.View.GetControl<EntryGrid>("FEntity").GetSelectedRows()[0];
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FProductLineId", -1, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			DynamicObject dynamicObject = entityDataObject[index];
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L);
			string arg = DataEntityExtend.GetDynamicObjectItemValue<object>(dynamicObject, "LocationName", null).ToString();
			string text = string.Format("FUSEORGID={0} and FPRODUCTLINEID={1} and FLINELOCATION={2}", value2, value, dynamicObjectItemValue);
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_LineLocationBomPara", null, text, "");
			if (baseBillInfo.Count<DynamicObject>() > 0)
			{
				base.View.ShowErrMessage(string.Format(this.msg1, arg), "", 0);
				e.Cancel = true;
				return;
			}
			string text2 = string.Format("FUSEORGID={0}", value2);
			List<DynamicObject> baseBillInfo2 = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", null, text2, "");
			foreach (DynamicObject dynamicObject2 in baseBillInfo2)
			{
				DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject2, "BopEntity", null);
				foreach (DynamicObject dynamicObject3 in dynamicObjectItemValue2)
				{
					long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject3, "ProductLineId_Id", 0L);
					long dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject3, "PrdLineLocId_Id", 0L);
					if (dynamicObjectItemValue3 == value && dynamicObjectItemValue4 == dynamicObjectItemValue)
					{
						base.View.ShowErrMessage(string.Format(this.msg2, arg), "", 0);
						e.Cancel = true;
						return;
					}
				}
			}
		}

		// Token: 0x0400016A RID: 362
		private string msg1 = ResManager.LoadKDString("生产线工位{0}已被产线工位物料参数引用", "015072000017270", 7, new object[0]);

		// Token: 0x0400016B RID: 363
		private string msg2 = ResManager.LoadKDString("生产线工位{0}已被BOP引用", "015072000017271", 7, new object[0]);
	}
}
