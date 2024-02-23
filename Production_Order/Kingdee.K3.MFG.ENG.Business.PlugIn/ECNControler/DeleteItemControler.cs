using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C5 RID: 197
	public class DeleteItemControler : AbstractItemControler
	{
		// Token: 0x06000E49 RID: 3657 RVA: 0x000A58B4 File Offset: 0x000A3AB4
		public override void DoOperation()
		{
			DynamicObjectCollection dataCollection = base.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FTreeEntity"));
			EntryGrid control = base.View.GetControl<EntryGrid>("FTreeEntity");
			HashSet<int> selectIndexs = new HashSet<int>(control.GetSelectedRows());
			HashSet<string> selectGroup = new HashSet<string>(from x in dataCollection
			where selectIndexs.Contains(dataCollection.IndexOf(x))
			select x into r
			select DataEntityExtend.GetDynamicValue<string>(r, "ECNGroup", null));
			List<DynamicObject> list = (from x in dataCollection
			where selectGroup.Contains(DataEntityExtend.GetDynamicValue<string>(x, "ECNGroup", null))
			select x).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject r)
			{
				base.Model.DeleteEntryRow("FTreeEntity", base.Model.GetRowIndex(base.View.BusinessInfo.GetEntity("FTreeEntity"), r));
			});
			base.SummaryUpdtBOMVers();
		}
	}
}
