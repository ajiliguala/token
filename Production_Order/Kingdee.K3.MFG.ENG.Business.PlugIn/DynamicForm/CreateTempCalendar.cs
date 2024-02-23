using System;
using System.Collections.Generic;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000087 RID: 135
	public class CreateTempCalendar : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000A48 RID: 2632 RVA: 0x00077D54 File Offset: 0x00075F54
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBBUTTON_SUBMIT"))
				{
					if (!(a == "TBBUTTON_CANCEL"))
					{
						return;
					}
					this.View.Close();
				}
				else
				{
					DynamicObject dynamicObject = this.View.OpenParameter.GetCustomParameter("Source") as DynamicObject;
					DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["Entity"] as DynamicObjectCollection;
					List<DynamicObject> list = new List<DynamicObject>();
					DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
					{
						long num = Convert.ToInt64(dynamicObject2["EquipmentId_Id"]);
						DateTime d = Convert.ToDateTime(dynamicObject2["Date"]);
						if (num != 0L && d != default(DateTime))
						{
							DynamicObject dynamicObject3 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
							dynamicObject3["CreateDate"] = systemDateTime;
							dynamicObject3["CreatorId_Id"] = base.Context.UserId;
							dynamicObject3["ApproveDate"] = null;
							dynamicObject3["ApproverId_Id"] = null;
							dynamicObject3["ModifyDate"] = null;
							dynamicObject3["ModifierId_Id"] = null;
							dynamicObject3["DocumentStatus"] = "A";
							dynamicObject3["EquipmentId_Id"] = dynamicObject2["EquipmentId_Id"];
							dynamicObject3["EquipmentId"] = null;
							dynamicObject3["Date"] = dynamicObject2["Date"];
							list.Add(dynamicObject3);
						}
					}
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_TempCal", true) as FormMetadata;
					IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, list.ToArray(), null, "");
					if (!operationResult.IsSuccess)
					{
						StringBuilder stringBuilder = new StringBuilder();
						foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
						{
							stringBuilder.AppendLine(validationErrorInfo.Message);
						}
						this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
						return;
					}
					this.View.ShowOperateResult(operationResult.OperateResult, "BOS_BatchTips");
					return;
				}
			}
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x00078020 File Offset: 0x00076220
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FEQUIPMENTID"))
				{
					return;
				}
				string filter = " EXISTS(SELECT 1 FROM (SELECT RS1.FRESID FROM T_ENG_WORKCENTERDATA  DA\r\n                        INNER JOIN T_SFC_DSPRPTPERMENTRY DSP1 on da.fid=DSP1.FWCID\r\n                        INNER JOIN T_SFC_DSPRPTPERM DSP0 ON DSP1.FID = DSP0.FID \r\n                        INNER JOIN T_ENG_RESOURCE RS ON DA.FRESOURCEID=RS.FID \r\n                        INNER JOIN T_ENG_RESOURCEDETAIL RS1 ON RS.FID = RS1.FID \r\n                        AND RS.FDOCUMENTSTATUS = 'C' AND RS.FFORBIDSTATUS = 'A' AND RS1.FRESOURCETYPEID = 'ENG_Equipment' AND RS1.FISFORBIDDEN = '0'\r\n                        WHERE DSP1.FISCHECKED = '1'  AND DSP0.FUSERID = @UserId) AS A WHERE FID= A.FRESID)";
				ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
				listShowParameter.SqlParams.Add(new SqlParam("@UserId", 12, base.Context.UserId));
				listShowParameter.ListFilterParameter.Filter = filter;
			}
		}
	}
}
