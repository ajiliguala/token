using System;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Bill
{
	// Token: 0x02000054 RID: 84
	public class TestEntityEdit : BaseControlEdit
	{
		// Token: 0x06000644 RID: 1604 RVA: 0x0004ADC0 File Offset: 0x00048FC0
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			int num = Convert.ToInt32(this.Model.GetValue("FInteger1"));
			((AbstractDynamicFormModel)this.Model).BatchInsertEntryRow("FEntity", 0, num);
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0004ADFC File Offset: 0x00048FFC
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.Model.SetValue("FText1", Guid.NewGuid().ToString(), e.Row);
			this.Model.SetValue("FInteger", new Random().Next(), e.Row);
			this.Model.SetValue("FDecimal", new Random().Next(), e.Row);
		}
	}
}
