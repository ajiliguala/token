using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200000E RID: 14
	[Description("产品配置单据插件-参数插件")]
	public class BOMConfigParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000209 RID: 521 RVA: 0x00018960 File Offset: 0x00016B60
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.Operation.Operation != "Save")
			{
				return;
			}
			if (this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "ENG_BOMCONFIG")
			{
				int value = MFGBillUtil.GetValue<int>(this.View.Model, "FShowWay", -1, 0, null);
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, "ReflashBomConfigEntityData", value.ToString());
				this.View.SendDynamicFormAction(this.View.ParentFormView);
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00018A20 File Offset: 0x00016C20
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			Field field = this.View.BusinessInfo.GetField("FExtSaveAndSubmit");
			if (field != null && field.DynamicProperty != null)
			{
				bool bEnabled = Convert.ToBoolean(field.DynamicProperty.GetValue(this.Model.DataObject));
				this.LockRelationFields(bEnabled);
			}
			Field field2 = this.View.BusinessInfo.GetField("FExtSubmitAndAudit");
			if (field2 != null && field2.DynamicProperty != null)
			{
				bool flag = Convert.ToBoolean(field2.DynamicProperty.GetValue(this.Model.DataObject));
				this.View.LockField("FExtSubmitAndAudit", flag);
			}
		}

		// Token: 0x0600020B RID: 523 RVA: 0x00018AC8 File Offset: 0x00016CC8
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FExtSaveAndSubmit"))
				{
					return;
				}
				this.LockRelationFields(Convert.ToBoolean(e.NewValue));
			}
		}

		// Token: 0x0600020C RID: 524 RVA: 0x00018B0A File Offset: 0x00016D0A
		private void LockRelationFields(bool bEnabled)
		{
			this.View.LockField("FExtSubmitAndAudit", bEnabled);
			this.Model.SetValue("FExtSubmitAndAudit", bEnabled);
		}
	}
}
