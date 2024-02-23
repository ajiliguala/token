using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000039 RID: 57
	[Description("特性日历的构建插件")]
	public class WorkCalCustomEditBuilder : AbstractMFGBillBuilderPlugIn
	{
		// Token: 0x0600042D RID: 1069 RVA: 0x00034FE8 File Offset: 0x000331E8
		public override void CreateMainMenu(BarDataManager bar)
		{
			base.CreateMainMenu(bar);
			foreach (KeyValuePair<string, BarItem> keyValuePair in bar.GetTopLevelBarItems())
			{
				keyValuePair.Value.Visible = 0;
			}
			foreach (BarItem barItem in bar.BarItems)
			{
				barItem.Visible = 0;
			}
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0003508C File Offset: 0x0003328C
		public override void AfterCreateMainMenu(CreateControlEventArgs e)
		{
			base.AfterCreateMainMenu(e);
			e.Control.Put("visible", false);
		}
	}
}
