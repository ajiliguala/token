using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.VerificationHelper.Verifiers;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200002B RID: 43
	[Description("建模变量插件")]
	public class ModeVariableEditPlugIn : BaseControlEdit
	{
		// Token: 0x06000300 RID: 768 RVA: 0x000230D2 File Offset: 0x000212D2
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x06000301 RID: 769 RVA: 0x000230EC File Offset: 0x000212EC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string text = string.Empty;
			string text2 = "";
			text = Convert.ToString(base.View.Model.GetValue("FVARTYPE"));
			string text3 = e.ListFilterParameter.Filter;
			string a;
			string a2;
			if ((a = e.FieldKey.ToUpper()) != null && a == "FVARSOURCE" && (a2 = text) != null)
			{
				if (!(a2 == "A"))
				{
					if (a2 == "B")
					{
						text2 = " FDataType = 2 ";
					}
				}
				else
				{
					text2 = " FDataType = 3 ";
				}
			}
			if (string.IsNullOrEmpty(text3))
			{
				text3 = text2;
			}
			else
			{
				text3 = text3 + " AND " + text2;
			}
			e.ListFilterParameter.Filter = text3;
		}
	}
}
