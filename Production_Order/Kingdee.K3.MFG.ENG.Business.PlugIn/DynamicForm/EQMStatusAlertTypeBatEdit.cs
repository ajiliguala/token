using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000095 RID: 149
	[Description("设备状态变更_批量修改生产事件（动态表单）处理类")]
	public class EQMStatusAlertTypeBatEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000AC7 RID: 2759 RVA: 0x0007C151 File Offset: 0x0007A351
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.lstSelectDatas = (List<DynamicObject>)e.Paramter.GetCustomParameter("SelectDatas");
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x0007C178 File Offset: 0x0007A378
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.FieldKey.ToUpper() == "FALERTTYPEID" && this.lstSelectDatas.Count > 0)
			{
				long num = Convert.ToInt64(this.lstSelectDatas[0]["OrgId_Id"]);
				string text = Convert.ToString(this.lstSelectDatas[0]["ChangedStatus"]);
				e.ListFilterParameter.Filter = "FUseOrgId= @UserOrgId and FEffectStatus = @ChangeStatus ";
				ListShowParameter listShowParameter = (ListShowParameter)e.DynamicFormShowParameter;
				listShowParameter.SqlParams.Add(new SqlParam("@UserOrgId", 12, num));
				listShowParameter.SqlParams.Add(new SqlParam("@ChangeStatus", 16, text));
			}
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0007C2E0 File Offset: 0x0007A4E0
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpper()) != null)
			{
				if (!(a == "BATCHMODIFY"))
				{
					return;
				}
				long alterTypeId = Convert.ToInt64(this.Model.DataObject["AlertTypeId_Id"]);
				if (alterTypeId == 0L)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("必须指定一个生产事件！", "0151515153499000013585", 7, new object[0]), "", 0);
					return;
				}
				if (this.lstSelectDatas.Count > 0)
				{
					string text = string.Format(ResManager.LoadKDString("已选定{0}条记录，确定要修改！", "0151515153499000013586", 7, new object[0]), this.lstSelectDatas.Count);
					this.View.ShowMessage(text, 1, delegate(MessageBoxResult boxResult)
					{
						if (boxResult == 1)
						{
							List<long> lstIds = (from o in this.lstSelectDatas
							select Convert.ToInt64(o["Id"])).ToList<long>();
							this.UpdateEqmStatusChgLog(lstIds, alterTypeId);
							this.View.ReturnToParentWindow(alterTypeId);
							this.View.Close();
						}
					}, "", 0);
				}
			}
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x0007C3D8 File Offset: 0x0007A5D8
		private void UpdateEqmStatusChgLog(List<long> lstIds, long alertTypeId)
		{
			if (lstIds == null && lstIds.Count == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("update T_ENG_EQMSTATUSCHGLOG set FExpType =@ExpType,FModifyDate= @ModifyTime ");
			lstIds = lstIds.Distinct<long>().ToList<long>();
			string sqlWithCardinality = StringUtils.GetSqlWithCardinality(lstIds.Count, "@FIDS", 1, false);
			string value = string.Format(" where EXISTS ( {0} WHERE b.FID = T_ENG_EQMSTATUSCHGLOG.FID)", sqlWithCardinality);
			stringBuilder.AppendLine(value);
			List<SqlParam> list = new List<SqlParam>();
			list.Add(new SqlParam("@ExpType", 12, alertTypeId));
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
			list.Add(new SqlParam("@ModifyTime", 6, systemDateTime));
			list.Add(new SqlParam("@FIDS", 161, lstIds.ToArray()));
			DBServiceHelper.Execute(base.Context, stringBuilder.ToString(), list);
		}

		// Token: 0x04000511 RID: 1297
		private List<DynamicObject> lstSelectDatas = new List<DynamicObject>();
	}
}
