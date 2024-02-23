using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C4 RID: 196
	public class DeleteCobyControler : AbstractCobyControler
	{
		// Token: 0x06000E45 RID: 3653 RVA: 0x000A5748 File Offset: 0x000A3948
		public override void DoOperation()
		{
			DynamicObjectCollection dataCollection = base.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FCobyEntity"));
			EntryGrid control = base.View.GetControl<EntryGrid>("FCobyEntity");
			HashSet<int> selectIndexs = new HashSet<int>(control.GetSelectedRows());
			HashSet<string> selectGroup = new HashSet<string>(from x in dataCollection
			where selectIndexs.Contains(dataCollection.IndexOf(x))
			select x into r
			select DataEntityExtend.GetDynamicValue<string>(r, "EcnCobyGroup", null));
			List<DynamicObject> list = (from x in dataCollection
			where selectGroup.Contains(DataEntityExtend.GetDynamicValue<string>(x, "EcnCobyGroup", null))
			select x).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject r)
			{
				base.Model.DeleteEntryRow("FCobyEntity", base.Model.GetRowIndex(base.View.BusinessInfo.GetEntity("FCobyEntity"), r));
			});
			base.SortCoby(dataCollection);
			base.SummaryUpdtBOMVers();
		}
	}
}
