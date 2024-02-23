using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000005 RID: 5
	[Description("基本活动编辑界面插件")]
	public class BaseActivityEdit : BaseControlEdit
	{
		// Token: 0x06000009 RID: 9 RVA: 0x000023BE File Offset: 0x000005BE
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			base.View.Model.SetValue("FIsSystemSet", false);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000023E4 File Offset: 0x000005E4
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FActCategory"))
				{
					return;
				}
				this.setPhaseValue();
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000241C File Offset: 0x0000061C
		private void setPhaseValue()
		{
			DynamicObject dynamicObject = base.View.Model.DataObject["ActCategory"] as DynamicObject;
			if (dynamicObject != null && !dynamicObject["Number"].Equals(""))
			{
				bool flag = dynamicObject["Number"].ToString().Equals("01") || BaseActivityEditServiceHelper.IsPhase(base.Context, dynamicObject["Number"].ToString());
				if (flag)
				{
					base.View.GetControl("FPhase").Enabled = true;
					base.View.GetControl("FPhase").SetCustomPropertyValue("mustInput", true);
					base.View.UpdateView("FPhase");
					return;
				}
				base.View.Model.DataObject["Phase"] = "";
				base.View.GetControl("FPhase").SetCustomPropertyValue("mustInput", false);
				base.View.GetControl("FPhase").Enabled = false;
				base.View.UpdateView("FPhase");
			}
		}
	}
}
