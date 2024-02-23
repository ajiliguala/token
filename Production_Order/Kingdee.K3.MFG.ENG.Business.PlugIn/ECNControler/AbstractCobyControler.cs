using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000BD RID: 189
	public class AbstractCobyControler : AbstractItemControler
	{
		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x000A32FB File Offset: 0x000A14FB
		// (set) Token: 0x06000E13 RID: 3603 RVA: 0x000A3312 File Offset: 0x000A1512
		protected Entity BomCobyEntity
		{
			get
			{
				return base.BomMeta.BusinessInfo.GetEntity("FEntryBOMCOBY");
			}
			set
			{
				this.bomCobyEntity = value;
			}
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x000A331C File Offset: 0x000A151C
		protected override void SetECNGoup(params DynamicObject[] entryGroup)
		{
			string text = Guid.NewGuid().ToString();
			foreach (DynamicObject dynamicObject in entryGroup)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EcnCobyGroup", text);
			}
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x000A3364 File Offset: 0x000A1564
		protected override void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomBocyEntry)
		{
			DataEntityExtend.CopyPropertyValues(targetRow, sourceBomBocyEntry, new Func<GetFieldValueCallbackParam, object>(base.GetCHFieldValueCallback), null);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CobyBomEntryId", sourceBomBocyEntry["Id"]);
			DBServiceHelper.LoadReferenceObject(base.View.Context, new DynamicObject[]
			{
				targetRow
			}, targetRow.DynamicObjectType, true);
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x000A33C0 File Offset: 0x000A15C0
		protected override void FillBomHeadObjectValue(DynamicObject targetRow, Dictionary<string, DynamicObject> dictBomHeadsGroupByEntryId, long parentRowId, int index)
		{
			DynamicObject dynamicObject;
			if (dictBomHeadsGroupByEntryId.TryGetValue(DataEntityExtend.GetDynamicValue<long>(targetRow, "CobyBomEntryId", 0L).ToString(), out dynamicObject) || dictBomHeadsGroupByEntryId.TryGetValue(parentRowId.ToString(), out dynamicObject))
			{
				base.Model.SetValue("FCobyBomVersion", dynamicObject, index);
			}
		}

		// Token: 0x06000E17 RID: 3607 RVA: 0x000A3428 File Offset: 0x000A1628
		public virtual string GetSelectedBomCobyEntryFilterString()
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FCobyEntity");
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(entity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return "t3.FEntryId != 0";
			}
			List<long> list = (from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<long>(x, "CobyBomEntryId", 0L) into x
			where x > 0L
			select x).Distinct<long>().ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return "t3.FEntryId != 0";
			}
			return string.Format(" t3.FEntryId NOT IN({0}) ", string.Join<long>(",", list));
		}

		// Token: 0x06000E18 RID: 3608 RVA: 0x000A3520 File Offset: 0x000A1720
		protected virtual void ShowBomList()
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
			listShowParameter.ListFilterParameter.Filter = " FMaterialId.FIsECN='1' ";
			listShowParameter.CustomParams.Add("ECNCobyEntity", "true");
			listShowParameter.CustomParams.Add("FromEcnOrder", "true");
			string selectedBomCobyEntryFilterString = this.GetSelectedBomCobyEntryFilterString();
			if (!string.IsNullOrWhiteSpace(selectedBomCobyEntryFilterString))
			{
				listShowParameter.ListFilterParameter.Filter = StringUtils.JoinFilterString(listShowParameter.ListFilterParameter.Filter, selectedBomCobyEntryFilterString, "AND");
			}
			if (base.UseECREntryBomId && !ListUtils.IsEmpty<long>(base.ECREntryBomId))
			{
				listShowParameter.ListFilterParameter.Filter = StringUtils.JoinFilterString(listShowParameter.ListFilterParameter.Filter, string.Format("FId in ({0})", string.Join<long>(",", base.ECREntryBomId)), "AND");
			}
			base.View.ShowForm(listShowParameter, delegate(FormResult x)
			{
				if (base.View == null)
				{
					Logger.Error("MFG_ENG", "工程变更单修改联副产品回调界面为null", null);
					return;
				}
				if (x.ReturnData == null)
				{
					return;
				}
				ListSelectedRowCollection collection = x.ReturnData as ListSelectedRowCollection;
				this.AddEntryRow(collection, -1);
			});
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x000A365C File Offset: 0x000A185C
		protected void SortCoby(DynamicObjectCollection dataCollection)
		{
			IEnumerable<IGrouping<string, DynamicObject>> enumerable = from x in dataCollection
			group x by DataEntityExtend.GetDynamicValue<string>(x, "EcnCobyGroup", null);
			int num = 1;
			foreach (IGrouping<string, DynamicObject> grouping in enumerable)
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					dynamicObject["Seq"] = num++;
				}
			}
		}

		// Token: 0x0400067D RID: 1661
		private Entity bomCobyEntity;
	}
}
