using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200009E RID: 158
	[Description("列表模型配置特性检验插件")]
	public class ListPDBFeatureCheckPlugIn : BaseControlList
	{
		// Token: 0x06000B2A RID: 2858 RVA: 0x00080413 File Offset: 0x0007E613
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}
	}
}
