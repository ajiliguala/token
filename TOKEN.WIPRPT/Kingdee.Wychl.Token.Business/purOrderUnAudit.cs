using System;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200001E RID: 30
	public class purOrderUnAudit : AbstractOperationServicePlugIn
	{
		// Token: 0x0600005D RID: 93 RVA: 0x00009A35 File Offset: 0x00007C35
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("F_KING_QZBILLID");
			e.FieldKeys.Add("FDocumentStatus");
			e.FieldKeys.Add("F_KING_QZSTATUS");
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00009A74 File Offset: 0x00007C74
		public override void OnAddValidators(AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);
			purOrderUnAudit.TestValidator testValidator = new purOrderUnAudit.TestValidator();
			testValidator.AlwaysValidate = true;
			testValidator.EntityKey = "FBillHead";
			e.Validators.Add(testValidator);
		}

		// Token: 0x0200002D RID: 45
		private class TestValidator : AbstractValidator
		{
			// Token: 0x0600009A RID: 154 RVA: 0x00010DF8 File Offset: 0x0000EFF8
			public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
			{
				foreach (ExtendedDataEntity extendedDataEntity in dataEntities)
				{
					string text = string.Empty;
					string a = extendedDataEntity["DocumentStatus"].ToString();
					bool flag = a == "C";
					if (flag)
					{
						string fid = extendedDataEntity["id"].ToString();
						string text2 = extendedDataEntity["F_KING_QZBILLID"].ToString();
						bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2);
						if (flag2)
						{
							string text3 = extendedDataEntity["F_KING_QZSTATUS"].ToString();
							bool flag3 = text3.Equals("已完成");
							if (flag3)
							{
								text = "签章已经完成，不允许反审";
							}
							else
							{
								text = serviceHelper.cancelContract(base.Context, fid, text2);
							}
						}
					}
					bool flag4 = text.Length > 0;
					if (flag4)
					{
						validateContext.AddError(extendedDataEntity.DataEntity, new ValidationErrorInfo("", extendedDataEntity.DataEntity["Id"].ToString(), extendedDataEntity.DataEntityIndex, 0, "001", "单据编号" + extendedDataEntity.BillNo + text, "反审核" + extendedDataEntity.BillNo, 2));
					}
				}
			}
		}
	}
}
