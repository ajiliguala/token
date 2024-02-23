using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.K3.MFG.ServiceHelper.Common;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000033 RID: 51
	[Description("重复生产线列表插件")]
	public class RepetitiveProductLineList : WorkCenterBaseList
	{
		// Token: 0x060003B5 RID: 949 RVA: 0x0002E5DE File Offset: 0x0002C7DE
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			LicenseVerifierServiceHelper.CheckHasSCXSCFeature(e.Context);
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0002E5EC File Offset: 0x0002C7EC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (a == "RptPrdLineLocation")
				{
					this.RptPrdLineLocation();
					return;
				}
				if (!(a == "RepeatLineRelateBom"))
				{
					return;
				}
				this.RepeatLineRelateBom();
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0002E644 File Offset: 0x0002C844
		private void RptPrdLineLocation()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_RptPrdLineLocation";
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

		// Token: 0x060003B8 RID: 952 RVA: 0x0002E6DC File Offset: 0x0002C8DC
		private void RepeatLineRelateBom()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_RepeatLineRelateBom";
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
