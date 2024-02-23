using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200000C RID: 12
	[Description("物料清单用户参数插件")]
	public class BOMParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000201 RID: 513 RVA: 0x00018664 File Offset: 0x00016864
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdLockField(this.View, "FUpdateRange");
			}
			Field field = this.View.BusinessInfo.GetField("FMATERIALIDCHILD");
			if (field != null && field.DynamicProperty != null)
			{
				bool bEnabled = Convert.ToBoolean(field.DynamicProperty.GetValue(this.Model.DataObject));
				this.LockRelationFields(bEnabled);
			}
		}

		// Token: 0x06000202 RID: 514 RVA: 0x000186DC File Offset: 0x000168DC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FMATERIALIDCHILD"))
				{
					return;
				}
				this.LockRelationFields(Convert.ToBoolean(e.NewValue));
			}
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00018720 File Offset: 0x00016920
		private void LockRelationFields(bool bEnabled)
		{
			this.View.LockField("FBOMID", bEnabled);
			this.View.LockField("FAuxPropId", bEnabled);
			this.View.LockField("FSUPPLYORG", bEnabled);
			this.View.LockField("FOPERID", bEnabled);
			this.View.LockField("FPROCESSID", bEnabled);
			this.View.LockField("FPOSITIONNO", bEnabled);
			this.View.LockField("FCHILDUNITID", bEnabled);
			this.View.LockField("FNUMERATOR", bEnabled);
			this.View.LockField("FDENOMINATOR", bEnabled);
			this.View.LockField("FFIXSCRAPQTY", bEnabled);
			this.View.LockField("FSCRAPRATE", bEnabled);
			if (!bEnabled)
			{
				this.Model.SetValue("FBOMID", bEnabled);
				this.Model.SetValue("FAuxPropId", bEnabled);
				this.Model.SetValue("FSUPPLYORG", bEnabled);
				this.Model.SetValue("FOPERID", bEnabled);
				this.Model.SetValue("FPROCESSID", bEnabled);
				this.Model.SetValue("FPOSITIONNO", bEnabled);
				this.Model.SetValue("FCHILDUNITID", bEnabled);
				this.Model.SetValue("FNUMERATOR", bEnabled);
				this.Model.SetValue("FDENOMINATOR", bEnabled);
				this.Model.SetValue("FFIXSCRAPQTY", bEnabled);
				this.Model.SetValue("FSCRAPRATE", bEnabled);
			}
		}
	}
}
