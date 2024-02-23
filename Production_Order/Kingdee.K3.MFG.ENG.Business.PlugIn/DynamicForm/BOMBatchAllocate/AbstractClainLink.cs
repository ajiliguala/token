using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x0200005F RID: 95
	public abstract class AbstractClainLink
	{
		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600070E RID: 1806 RVA: 0x00053250 File Offset: 0x00051450
		// (set) Token: 0x0600070F RID: 1807 RVA: 0x00053258 File Offset: 0x00051458
		public BusinessInfo BizInfo
		{
			get
			{
				return this.bizInfo;
			}
			set
			{
				this.bizInfo = value;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000710 RID: 1808 RVA: 0x00053261 File Offset: 0x00051461
		// (set) Token: 0x06000711 RID: 1809 RVA: 0x00053269 File Offset: 0x00051469
		public List<DynamicObject> Datas
		{
			get
			{
				return this.datas;
			}
			set
			{
				this.datas = value;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000712 RID: 1810 RVA: 0x00053272 File Offset: 0x00051472
		// (set) Token: 0x06000713 RID: 1811 RVA: 0x0005327A File Offset: 0x0005147A
		public IOperationResult Result
		{
			get
			{
				return this.result;
			}
			set
			{
				this.result = value;
			}
		}

		// Token: 0x06000714 RID: 1812
		public abstract void DoOperation(Context ctx);

		// Token: 0x06000715 RID: 1813 RVA: 0x00053284 File Offset: 0x00051484
		protected void MergeResult(IOperationResult r)
		{
			this.result.IsSuccess = false;
			foreach (OperateResult operateResult in r.OperateResult)
			{
				if (!operateResult.SuccessStatus)
				{
					this.result.OperateResult.Add(new OperateResult
					{
						DataEntityIndex = -1,
						SuccessStatus = false,
						MessageType = 0,
						Message = operateResult.Message
					});
				}
			}
			foreach (ValidationErrorInfo validationErrorInfo in r.ValidationErrors)
			{
				this.result.OperateResult.Add(new OperateResult
				{
					DataEntityIndex = -1,
					SuccessStatus = false,
					MessageType = 0,
					Message = validationErrorInfo.Message
				});
			}
		}

		// Token: 0x04000328 RID: 808
		private BusinessInfo bizInfo;

		// Token: 0x04000329 RID: 809
		private List<DynamicObject> datas;

		// Token: 0x0400032A RID: 810
		private IOperationResult result;
	}
}
