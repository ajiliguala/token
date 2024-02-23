using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.K3.MFG.ServiceHelper.Common;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000021 RID: 33
	[Description("流程生产线列表插件")]
	public class FlowProductLineList : WorkCenterBaseList
	{
		// Token: 0x060002B2 RID: 690 RVA: 0x0001FCD7 File Offset: 0x0001DED7
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			LicenseVerifierServiceHelper.CheckHasSCXSCFeature(e.Context);
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0001FCE4 File Offset: 0x0001DEE4
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (a == "FlowPrdLineLocation")
				{
					this.FlowPrdLineLocation();
					return;
				}
				if (!(a == "FlowLineRelateBom"))
				{
					return;
				}
				this.FlowLineRelateBom();
			}
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0001FD3C File Offset: 0x0001DF3C
		private void FlowPrdLineLocation()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_FlowPrdLineLocation";
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

		// Token: 0x060002B5 RID: 693 RVA: 0x0001FDD4 File Offset: 0x0001DFD4
		private void FlowLineRelateBom()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_FlowLineRelateBom";
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
	}
}
