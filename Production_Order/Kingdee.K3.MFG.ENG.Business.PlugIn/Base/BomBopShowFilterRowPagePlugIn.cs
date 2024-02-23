using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000006 RID: 6
	[Description("Bom页面构造，显示过滤查询行")]
	public class BomBopShowFilterRowPagePlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x0600000D RID: 13 RVA: 0x0000255E File Offset: 0x0000075E
		public BomBopShowFilterRowPagePlugIn()
		{
			this.hashSetEntity.Add("FBopEntity");
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002582 File Offset: 0x00000782
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (this.hashSetEntity.Contains(e.ControlAppearance.Key))
			{
				e.Control.Put("showFilterRow", true);
			}
			base.CreateControl(e);
		}

		// Token: 0x04000003 RID: 3
		private HashSet<string> hashSetEntity = new HashSet<string>();
	}
}
