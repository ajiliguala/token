using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000011 RID: 17
	public class MyPatchPrintAction : AbstractListPlugIn
	{
		// Token: 0x06000034 RID: 52 RVA: 0x00004A18 File Offset: 0x00002C18
		public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
		{
			bool flag = e.DataSourceId.Equals("FBillHead", StringComparison.OrdinalIgnoreCase);
			if (flag)
			{
				DynamicObject dataObject = this.Model.DataObject;
			}
		}
	}
}
