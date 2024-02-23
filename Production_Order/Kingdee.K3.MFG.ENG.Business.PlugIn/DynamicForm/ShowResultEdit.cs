using System;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B3 RID: 179
	public class ShowResultEdit : AbstractBillPlugIn
	{
		// Token: 0x06000CFF RID: 3327 RVA: 0x00099D7C File Offset: 0x00097F7C
		public override void AfterCreateNewData(EventArgs e)
		{
			DynamicObject dynamicObject = base.View.OpenParameter.GetCustomParameter("OrderDataObject", true) as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			this.Model.DataObject = dynamicObject;
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00099DB5 File Offset: 0x00097FB5
		public override void DataChanged(DataChangedEventArgs e)
		{
			this.IsNeedSave = false;
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00099E30 File Offset: 0x00098030
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "NEW") && !(a == "CLOSE"))
				{
					return;
				}
				if (!this.IsNeedSave)
				{
					return;
				}
				base.View.ShowMessage(ResManager.LoadKDString("内容已修改，是否先保存？", "015072000015033", 7, new object[0]), 4, delegate(MessageBoxResult x)
				{
					if (x == 6)
					{
						this.IsNeedSave = false;
						this.View.InvokeFormOperation("Save");
						return;
					}
					this.IsNeedSave = false;
					this.View.InvokeFormOperation(e.Operation.FormOperation.Operation);
				}, "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x040005EC RID: 1516
		private bool IsNeedSave = true;
	}
}
