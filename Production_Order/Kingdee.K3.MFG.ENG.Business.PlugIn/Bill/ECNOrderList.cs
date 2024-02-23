using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;
using Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Bill
{
	// Token: 0x02000053 RID: 83
	public class ECNOrderList : BaseControlList
	{
		// Token: 0x06000641 RID: 1601 RVA: 0x0004AD1C File Offset: 0x00048F1C
		public override void AfterBindData(EventArgs e)
		{
			this.controls.Clear();
			this.controls.Add(8, new ECNQueryControler
			{
				View = this.View,
				Model = this.Model
			});
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0004AD60 File Offset: 0x00048F60
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string operation;
			if ((operation = e.Operation.FormOperation.Operation) != null && !(operation == "SyncPPBom"))
			{
				if (!(operation == "ECNQuery"))
				{
					return;
				}
				this.controls[8].DoOperation();
			}
		}

		// Token: 0x040002C6 RID: 710
		private Dictionary<int, AbstractItemControler> controls = new Dictionary<int, AbstractItemControler>();
	}
}
