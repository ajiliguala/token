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
	// Token: 0x0200000C RID: 12
	public class getSubFile : IScheduleService
	{
		// Token: 0x06000021 RID: 33 RVA: 0x00003810 File Offset: 0x00001A10
		public void Run(Context ctx, Schedule schedule)
		{
			try
			{
				string text = " select * from T_BAS_ATTACHMENT where F_KING_STATUS!='已完成' and isnull(F_KING_QZID,'')!='' ";
				DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
				bool flag = dynamicObjectCollection.Count > 0;
				if (flag)
				{
					serviceHelper.getSubContractStatus(ctx, dynamicObjectCollection);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("getSubFile", ex.Message, ex);
				schedule.Status = 0;
				throw ex;
			}
		}
	}
}
