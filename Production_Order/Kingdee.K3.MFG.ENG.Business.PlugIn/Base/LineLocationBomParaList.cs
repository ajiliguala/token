using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000029 RID: 41
	[Description("生产线工位物料参数-列表插件")]
	public class LineLocationBomParaList : BaseControlList
	{
		// Token: 0x060002FC RID: 764 RVA: 0x00023034 File Offset: 0x00021234
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "GeneratePara"))
				{
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "ENG_GenerateParaFromBop";
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.OpenStyle.ShowType = 6;
				this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.RefreshListView));
			}
		}

		// Token: 0x060002FD RID: 765 RVA: 0x000230B5 File Offset: 0x000212B5
		private void RefreshListView(FormResult fs)
		{
			this.ListView.Refresh();
		}
	}
}
