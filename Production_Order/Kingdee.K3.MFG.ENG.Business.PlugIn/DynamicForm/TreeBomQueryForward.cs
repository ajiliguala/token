using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B6 RID: 182
	[Description("BOM正查(树形)插件")]
	public class TreeBomQueryForward : BomQueryForward
	{
		// Token: 0x06000D36 RID: 3382 RVA: 0x0009BB0C File Offset: 0x00099D0C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("PrintPreview") || e.Operation.FormOperation.IsEqualOperation("PrintExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BusinessInfo.GetForm().Id, "8dfa91ae26774d7ea46b29e29ecb3044"))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有物料清单正查的打印权限", "015072000014790", 7, new object[0]), "", 0);
				e.Cancel = true;
			}
		}
	}
}
