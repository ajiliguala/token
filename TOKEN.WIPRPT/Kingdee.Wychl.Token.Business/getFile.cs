using System;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200000A RID: 10
	public class getFile : IScheduleService
	{
		// Token: 0x0600001D RID: 29 RVA: 0x000036BC File Offset: 0x000018BC
		public void Run(Context ctx, Schedule schedule)
		{
			try
			{
				string text = string.Format("/*dialect*/ select tpp.F_KING_QZBILLID,tpp.fbillno,tpp.fid \r\n                            from  t_PUR_POOrder tpp\r\n                            inner join T_BD_SUPPLIER tbs on tbs.FSUPPLIERID=tpp.FSUPPLIERID\r\n                            where F_KING_QZSTATUS!='已完成' and isnull(F_KING_QZBILLID,'')!=''\r\n                            and tbs.F_KING_SFTBQZXT='1'", Array.Empty<object>());
				DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
				bool flag = dynamicObjectCollection.Count > 0;
				if (flag)
				{
					serviceHelper.getContractStatus(ctx, dynamicObjectCollection);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("getFile", ex.Message, ex);
				schedule.Status = 0;
				throw ex;
			}
		}
	}
}
