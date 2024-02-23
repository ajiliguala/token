using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x02000061 RID: 97
	public class SubmitClainLink : AbstractClainLink
	{
		// Token: 0x0600071B RID: 1819 RVA: 0x000534D8 File Offset: 0x000516D8
		public override void DoOperation(Context ctx)
		{
			if (!base.Result.IsSuccess)
			{
				return;
			}
			if (ListUtils.IsEmpty<DynamicObject>(base.Datas))
			{
				return;
			}
			object[] array = (from x in base.Datas
			select x["Id"]).ToArray<object>();
			IOperationResult operationResult = BusinessDataServiceHelper.Submit(ctx, base.BizInfo, array, "Submit", OperateOption.Create());
			if (operationResult.IsSuccess && ListUtils.IsEmpty<ValidationErrorInfo>(operationResult.ValidationErrors))
			{
				if (!operationResult.OperateResult.Any((OperateResult x) => !x.SuccessStatus))
				{
					return;
				}
			}
			SubmitClainLink.<>c__DisplayClass9 CS$<>8__locals1 = new SubmitClainLink.<>c__DisplayClass9();
			SubmitClainLink.<>c__DisplayClass9 CS$<>8__locals2 = CS$<>8__locals1;
			List<long> successIds;
			if (operationResult.SuccessDataEnity != null)
			{
				successIds = (from x in operationResult.SuccessDataEnity
				select DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).ToList<long>();
			}
			else
			{
				successIds = new List<long>();
			}
			CS$<>8__locals2.successIds = successIds;
			List<DynamicObject> list = (from x in base.Datas
			where !CS$<>8__locals1.successIds.Contains(DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L))
			select x).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject x)
			{
				base.Datas.Remove(x);
			});
			base.MergeResult(operationResult);
		}
	}
}
