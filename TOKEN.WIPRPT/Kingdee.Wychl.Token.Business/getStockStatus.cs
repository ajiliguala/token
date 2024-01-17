using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200000B RID: 11
	[HotUpdate]
	[Description("超期处理更新库存状态")]
	public class getStockStatus : AbstractOperationServicePlugIn
	{
		// Token: 0x0600001F RID: 31 RVA: 0x00003740 File Offset: 0x00001940
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			bool flag = e.DataEntitys.Length != 0;
			if (flag)
			{
				List<string> list = new List<string>();
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					list.Add("'" + dynamicObject["id"].ToString() + "'");
				}
				string str = " where A.FID in (" + string.Join(",", list) + ")";
				DBServiceHelper.Execute(base.Context, "/*dialect*/update T_KING_INVENTORY set F_PCQE_STATUS=1,F_PCQE_QTY=B.FBASEQTY\r\n                                                        from T_KING_INVENTORY A\r\n                                                        inner join T_STK_INVENTORY B on A.FID=B.FID and B.FBASEQTY!=0" + str);
				DBServiceHelper.Execute(base.Context, "/*dialect*/update T_KING_INVENTORY set F_PCQE_STATUS=2,F_PCQE_QTY=0\r\n                                                        from T_KING_INVENTORY A\r\n                                                        left join T_STK_INVENTORY B on A.FID=B.FID " + str + " and (B.FID =null or B.FBASEQTY=0)");
			}
		}
	}
}
