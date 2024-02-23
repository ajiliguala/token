using System;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000090 RID: 144
	public class ECNShowResultEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000AB7 RID: 2743 RVA: 0x0007BA60 File Offset: 0x00079C60
		public override void OnInitialize(InitializeEventArgs e)
		{
			DynamicFormOpenParameter paramter = e.Paramter;
			string text = (string)paramter.GetCustomParameter("_ResultSessionKey");
			if (string.IsNullOrEmpty(text) || this.View.ParentFormView == null)
			{
				return;
			}
			object obj = null;
			if (this.View.ParentFormView != null)
			{
				this.View.ParentFormView.Session.TryGetValue(text, out obj);
			}
			if (obj == null || !(obj is OperateResultCollection))
			{
				return;
			}
			this.resultCollection = (obj as OperateResultCollection);
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0007BADC File Offset: 0x00079CDC
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			DynamicObject dataEntity = this.resultCollection.ElementAt(e.Row).DataEntity;
			BillShowParameter billShowParameter = new BillShowParameter
			{
				FormId = "ENG_BOM",
				Status = 0
			};
			billShowParameter.CustomComplexParams.Add("OrderDataObject", dataEntity);
			billShowParameter.DynamicPlugins.Add(new PlugIn
			{
				ClassName = "Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.ECNResultEdit,Kingdee.K3.MFG.ENG.Business.PlugIn",
				OrderId = 99,
				ElementType = 0
			});
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x0400050E RID: 1294
		private OperateResultCollection resultCollection;
	}
}
