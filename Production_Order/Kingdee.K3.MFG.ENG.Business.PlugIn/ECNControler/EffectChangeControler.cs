using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C7 RID: 199
	public class EffectChangeControler : AbstractItemControler
	{
		// Token: 0x06000E55 RID: 3669 RVA: 0x000A5C7E File Offset: 0x000A3E7E
		public override void DoOperation()
		{
			if (base.View is IBillView)
			{
				this.DoOperationOnBill();
				return;
			}
			if (base.View is IListView)
			{
				this.DoOperationOnList();
			}
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x000A5DA4 File Offset: 0x000A3FA4
		private void DoOperationOnBill()
		{
			if (DataEntityExtend.GetDynamicObjectItemValue<bool>(base.Model.ParameterData, "FAsynExec", false))
			{
				IOperationResult result = new OperationResult();
				TaskProxyItem taskProxyItem = new TaskProxyItem();
				List<object> list = new List<object>
				{
					base.View.Context,
					base.View.BusinessInfo,
					base.Model.DataObject,
					result
				};
				taskProxyItem.Parameters = list.ToArray();
				taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.ECNOrderService,Kingdee.K3.MFG.ENG.App.Core";
				taskProxyItem.MethodName = "AsyncEffectChange";
				taskProxyItem.ProgressQueryInterval = 1;
				taskProxyItem.Title = ResManager.LoadKDString("ECN变更生效", "015072000018153", 7, new object[0]);
				FormUtils.ShowLoadingForm(base.View, taskProxyItem, null, true, delegate(IOperationResult op)
				{
					if (result.IsSuccess)
					{
						if (ListUtils.IsEmpty<OperateResult>(result.OperateResult))
						{
							this.View.ShowMessage(ResManager.LoadKDString("生效执行成功!", "015072000018154", 7, new object[0]), 0);
						}
						else
						{
							result.OperateResult.Add(new OperateResult
							{
								Message = ResManager.LoadKDString("生效执行成功!", "015072000018154", 7, new object[0]),
								DataEntityIndex = -1,
								SuccessStatus = true,
								MessageType = -1
							});
							this.View.ShowOperateResult(result.OperateResult, "ENG_ECNBatchTips");
						}
						this.View.Refresh();
						return;
					}
					this.View.ShowOperateResult(result.OperateResult, "ENG_ECNBatchTips");
				});
				return;
			}
			IOperationResult operationResult = ECNOrderServiceHelper.EffectChange(base.View.Context, base.View.BusinessInfo, base.Model.DataObject);
			if (operationResult.IsSuccess)
			{
				if (ListUtils.IsEmpty<OperateResult>(operationResult.OperateResult))
				{
					base.View.ShowMessage(ResManager.LoadKDString("生效执行成功!", "015072000018154", 7, new object[0]), 0);
				}
				else
				{
					operationResult.OperateResult.Add(new OperateResult
					{
						Message = ResManager.LoadKDString("生效执行成功!", "015072000018154", 7, new object[0]),
						DataEntityIndex = -1,
						SuccessStatus = true,
						MessageType = -1
					});
					base.View.ShowOperateResult(operationResult.OperateResult, "ENG_ECNBatchTips");
				}
				base.View.Refresh();
				return;
			}
			base.View.ShowOperateResult(operationResult.OperateResult, "ENG_ECNBatchTips");
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x000A5F7B File Offset: 0x000A417B
		private void DoOperationOnList()
		{
		}
	}
}
