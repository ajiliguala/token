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

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x02000060 RID: 96
	public class SaveClainLink : AbstractClainLink
	{
		// Token: 0x06000717 RID: 1815 RVA: 0x000533B4 File Offset: 0x000515B4
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
			IOperationResult operationResult = BusinessDataServiceHelper.Save(ctx, base.BizInfo, base.Datas.ToArray(), OperateOption.Create(), "Save");
			if (operationResult.IsSuccess && ListUtils.IsEmpty<ValidationErrorInfo>(operationResult.ValidationErrors))
			{
				if (!operationResult.OperateResult.Any((OperateResult x) => !x.SuccessStatus))
				{
					return;
				}
			}
			List<DynamicObject> list = base.Datas.Except(operationResult.SuccessDataEnity).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject x)
			{
				base.Datas.Remove(x);
			});
			base.MergeResult(operationResult);
		}
	}
}
