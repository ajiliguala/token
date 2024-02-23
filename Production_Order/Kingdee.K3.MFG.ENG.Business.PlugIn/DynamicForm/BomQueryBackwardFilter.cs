using System;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000076 RID: 118
	public class BomQueryBackwardFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x060008B1 RID: 2225 RVA: 0x000665CD File Offset: 0x000647CD
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_BomQueryBackward";
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x000665EC File Offset: 0x000647EC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToString().ToUpper()) != null)
			{
				if (!(a == "FMATERIALIDCHILD"))
				{
					return;
				}
				if (string.IsNullOrWhiteSpace((e.BaseDataField as BaseDataField).Filter))
				{
					this.SetChildMaterilIdFilterString();
				}
			}
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x00066640 File Offset: 0x00064840
		private void SetChildMaterilIdFilterString()
		{
			BaseDataField baseDataFieldByKey = this.View.Model.GetBaseDataFieldByKey("FMATERIALIDCHILD");
			baseDataFieldByKey.Filter = "(FMATERIALID IN (SELECT DISTINCT FMATERIALID FROM T_ENG_BOMCHILD))";
		}
	}
}
