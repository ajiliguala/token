using System;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200006C RID: 108
	[Description("BOM批量维护BOM过滤界面插件")]
	public class BomBatchEditBomFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x060007F1 RID: 2033 RVA: 0x0005DF37 File Offset: 0x0005C137
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._UseOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x0005DF5B File Offset: 0x0005C15B
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x0005DF64 File Offset: 0x0005C164
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (!string.IsNullOrEmpty(this.View.OpenParameter.GetCustomParameter("UseOrgId").ToString()))
			{
				this._UseOrgId = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("UseOrgId"));
			}
			if (MFGBillUtil.GetValue<long>(this.View.Model, "FOrgId", -1, 0L, null) != this._UseOrgId)
			{
				this.View.Model.SetValue("FOrgId", this._UseOrgId);
			}
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x0005DFFA File Offset: 0x0005C1FA
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key == "FBtnOK")
			{
				this._IsFromOKBtn = true;
			}
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x0005E01C File Offset: 0x0005C21C
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			e.Cancel = this._IsFromOKBtn;
			this._IsFromOKBtn = false;
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x0005E038 File Offset: 0x0005C238
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBomFrom") && !(fieldKey == "FBomTo") && !(fieldKey == "FSpecBomVer"))
				{
					return;
				}
				e.IsShowApproved = false;
				e.ListFilterParameter.Filter = StringUtils.JoinFilterString(e.ListFilterParameter.Filter, " FDocumentstatus!='Z'", "AND");
			}
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x0005E0AC File Offset: 0x0005C2AC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FBomFrom") && !(baseDataFieldKey == "FBomTo") && !(baseDataFieldKey == "FSpecBomVer"))
				{
					return;
				}
				e.IsShowApproved = false;
				e.Filter = StringUtils.JoinFilterString(e.Filter, " FDocumentstatus!='Z'", "AND");
			}
		}

		// Token: 0x040003A3 RID: 931
		private long _UseOrgId;

		// Token: 0x040003A4 RID: 932
		private bool _IsFromOKBtn;
	}
}
