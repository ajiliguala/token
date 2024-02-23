using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000020 RID: 32
	[Description("工作中心模板列表插件")]
	public class WorkCenterBaseList : BaseControlList
	{
		// Token: 0x060002AC RID: 684 RVA: 0x0001FA24 File Offset: 0x0001DC24
		public override void FormatCellValue(FormatCellValueArgs args)
		{
			base.FormatCellValue(args);
			string a;
			if ((a = args.Header.Key.ToUpper()) != null)
			{
				if (!(a == "FRESEFFECTDATE") && !(a == "FRESEXPIREDATE"))
				{
					return;
				}
				if (args.Value != null && !string.IsNullOrWhiteSpace(args.Value.ToString()))
				{
					args.FormateValue = Convert.ToDateTime(args.Value).ToShortDateString();
				}
			}
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0001FA9C File Offset: 0x0001DC9C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (a == "GetSendTIME")
				{
					this.GetSendTIME();
					return;
				}
				if (a == "LineLocationBomPara")
				{
					this.LineLocationBomPara();
					return;
				}
				if (!(a == "Buffer"))
				{
					return;
				}
				this.Buffer();
			}
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0001FB08 File Offset: 0x0001DD08
		protected void GetSendTIME()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_GetSendTIME";
			listShowParameter.MultiSelect = false;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.IsLookUp = false;
			listShowParameter.IsIsolationOrg = true;
			listShowParameter.IsShowUsed = false;
			if (this.ListView.CurrentSelectedRowInfo != null)
			{
				string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
				listShowParameter.ListFilterParameter.Filter = string.Format("FProductLineId ={0}", primaryKeyValue);
			}
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0001FBA0 File Offset: 0x0001DDA0
		protected void LineLocationBomPara()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_LineLocationBomPara";
			listShowParameter.MultiSelect = false;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.IsLookUp = false;
			listShowParameter.IsIsolationOrg = true;
			listShowParameter.IsShowUsed = false;
			if (this.ListView.CurrentSelectedRowInfo != null)
			{
				string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
				listShowParameter.ListFilterParameter.Filter = string.Format("FProductLineId ={0}", primaryKeyValue);
			}
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0001FC38 File Offset: 0x0001DE38
		protected void Buffer()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_Buffer";
			listShowParameter.MultiSelect = false;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.IsLookUp = false;
			listShowParameter.IsIsolationOrg = true;
			listShowParameter.IsShowUsed = false;
			if (this.ListView.CurrentSelectedRowInfo != null)
			{
				string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
				listShowParameter.ListFilterParameter.Filter = string.Format("FLineRealationship='A' AND FProductLineId ={0} ", primaryKeyValue);
			}
			this.View.ShowForm(listShowParameter);
		}
	}
}
