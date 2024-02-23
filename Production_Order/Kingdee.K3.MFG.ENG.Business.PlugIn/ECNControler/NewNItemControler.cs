using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000CC RID: 204
	public class NewNItemControler : AbstractItemControler
	{
		// Token: 0x06000E73 RID: 3699 RVA: 0x000A70A8 File Offset: 0x000A52A8
		public override void DoOperation()
		{
			base.DoOperation();
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "ENG_ECNADDNITEM";
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.PageId = Guid.NewGuid().ToString();
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (base.View != null)
				{
					object returnData = result.ReturnData;
					if (returnData != null)
					{
						if (StringUtils.EqualsIgnoreCase(returnData.ToString(), "FCANCEL"))
						{
							return;
						}
						int startIndex = 0;
						if (int.TryParse(returnData.ToString(), out startIndex))
						{
							this.AddEntryRow(null, startIndex);
							return;
						}
					}
				}
				else
				{
					Logger.Error("ENG_ECNORDER", "新增N行子项回调后View为null", null);
				}
			});
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x000A7144 File Offset: 0x000A5344
		public override void AddEntryRow(ListSelectedRowCollection collection, int startIndex)
		{
			base.AddEntryRow(collection, startIndex);
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DateTime dateTime = DataEntityExtend.GetDynamicValue<DateTime>(base.Model.DataObject, "EffectDate", default(DateTime));
			if (Convert.ToString(base.View.Model.GetValue("FChangeType")) == "1")
			{
				dateTime = MFGBillUtil.GetEffectData(base.View, 0L);
			}
			List<DynamicObject> list = new List<DynamicObject>();
			int entryRowCount = base.Model.GetEntryRowCount("FTreeEntity");
			base.View.Model.BatchCreateNewEntryRow("FTreeEntity", startIndex);
			DynamicObjectCollection entityDataObject = base.Model.GetEntityDataObject(treeEntity);
			IEnumerable<DynamicObject> enumerable = entityDataObject.Skip(entryRowCount);
			foreach (DynamicObject dynamicObject in enumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "RowType", 1);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ChangeLabel", 2);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ECNRowId", Guid.NewGuid().ToString());
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EFFECTDATE", dateTime);
				this.SetECNGoup(new DynamicObject[]
				{
					dynamicObject
				});
				int rowIndex = base.Model.GetRowIndex(treeEntity, dynamicObject);
				base.Model.SetValue("FSUPPLYORG", 0, rowIndex);
				list.Add(dynamicObject);
			}
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FTreeEntity", this.Model.GetRowIndex(treeEntity, x));
			});
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
		}
	}
}
