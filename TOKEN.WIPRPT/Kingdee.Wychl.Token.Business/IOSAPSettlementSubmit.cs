using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200000D RID: 13
	[Description("应付结算清单——物料更新供应商插件")]
	public class IOSAPSettlementSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00003889 File Offset: 0x00001A89
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003894 File Offset: 0x00001A94
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				string text = string.Format("/*dialect*/update T_IOS_APSettlement set FMAPDETAILID=tbs.FSUPPLIERID\r\n                                from T_IOS_APSettlement tia\r\n                                inner join T_ORG_ORGANIZATIONS too on too.FORGID=tia.FMAPSETTLEORGID\r\n                                inner join T_BD_SUPPLIER tbs on tbs.FUSEORGID=tia.FSETTLEORGID  and tbs.FCORRESPONDORGID=too.FPARENTID\r\n                                where tia.FID={0}", dynamicObject["id"]);
				DBServiceHelper.Execute(base.Context, text);
			}
		}
	}
}
