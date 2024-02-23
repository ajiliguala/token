using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000012 RID: 18
	[Description("物料阶梯损耗表单插件")]
	public class LadderLossEdit : BaseControlEdit
	{
		// Token: 0x06000218 RID: 536 RVA: 0x00019460 File Offset: 0x00017660
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null && a == "TBUPDATE")
			{
				if (LadderLossUtils.IsUpdatePermission(base.Context))
				{
					if (LadderLossUtils.IsModifyPermisson(base.Context, "ENG_BOM"))
					{
						DynamicObject dataObject = base.View.Model.DataObject;
						if (dataObject["DocumentStatus"].Equals("C"))
						{
							string value = dataObject["MATERIALID_ID"].ToString() ?? "0";
							DynamicObjectCollection lossEntries = dataObject["LADDERLOSSENTRY"] as DynamicObjectCollection;
							string value2 = ResManager.LoadKDString("物料阶梯损耗更新物料清单阶梯用量", "0151515153499030038686", 7, new object[0]);
							DynamicObjectCollection bomData = LadderLossServiceHelper.GetBomData(base.Context, Convert.ToInt64(value), base.Context.CurrentOrganizationInfo.ID);
							DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
							dynamicFormShowParameter.OpenStyle.ShowType = 0;
							dynamicFormShowParameter.FormId = "ENG_BOMLIST";
							dynamicFormShowParameter.CustomComplexParams.Add("title", value2);
							dynamicFormShowParameter.CustomComplexParams.Add("Enable", false);
							dynamicFormShowParameter.CustomComplexParams.Add("data", bomData);
							base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult FormResult)
							{
								Dictionary<string, object> dictionary = FormResult.ReturnData as Dictionary<string, object>;
								bool flag = Convert.ToBoolean(dictionary["isUpdate"]);
								if (flag)
								{
									IOperationResult operationResult = LadderLossUtils.UpdateBom(this.Context, dictionary, dataObject, lossEntries);
									bool flag2 = true;
									StringBuilder stringBuilder = new StringBuilder();
									if (!operationResult.IsSuccess)
									{
										List<ValidationErrorInfo> fatalErrorResults = operationResult.GetFatalErrorResults();
										flag2 = false;
										foreach (ValidationErrorInfo validationErrorInfo in fatalErrorResults)
										{
											stringBuilder.AppendLine(validationErrorInfo.Message);
										}
									}
									if (flag2)
									{
										MFGServiceHelper.ResetBomCache(this.Context);
										FormMetadata formMetadata = MetaDataServiceHelper.Load(this.Context, "ENG_LadderLoss", true) as FormMetadata;
										LadderLossUtils.WriteOperateLog(this.Context, ResManager.LoadKDString("更新物料清单", "0151515153499030038885", 7, new object[0]), ResManager.LoadKDString("更新物料清单的梯用量信息", "0151515153499030038888", 7, new object[0]), formMetadata.BusinessInfo, "ENG_LadderLoss");
										this.View.ShowMessage(ResManager.LoadKDString("更新成功", "0151515153499030038769", 7, new object[0]), 0);
										return;
									}
									this.View.ShowMessage(stringBuilder.ToString(), 4);
									stringBuilder.Clear();
								}
							});
						}
						else
						{
							base.View.ShowMessage(ResManager.LoadKDString("请选择单据状态为【已审核】的物料阶梯损耗！", "0151515153499030038932", 7, new object[0]), 4);
						}
					}
					else
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有【物料清单修改】权限，请联系管理员授权！", "0151515153499000018754", 7, new object[0]), "", 0);
					}
				}
				else
				{
					base.View.ShowMessage(ResManager.LoadKDString("当前用户没有【更新物料清单】权限，请联系管理员授权！", "0151515153499030038837", 7, new object[0]), 4);
				}
			}
			base.BarItemClick(e);
		}
	}
}
