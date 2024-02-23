using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.DI;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001D RID: 29
	[Description("状态变更日志 - 列表插件")]
	public class EqmStatusChgLogList : BaseControlList
	{
		// Token: 0x0600029F RID: 671 RVA: 0x0001EE74 File Offset: 0x0001D074
		public override void PrepareFilterParameter(FilterArgs e)
		{
			long num = 0L;
			FilterRow filterRow = (from o in this.ListView.Model.FilterParameter.FilterRows
			where o.FilterField.Key == "FOrgId"
			select o).FirstOrDefault<FilterRow>();
			if (filterRow != null)
			{
				if (this.orgMetaData == null)
				{
					this.orgMetaData = (MetaDataServiceHelper.Load(base.Context, "ORG_Organizations", true) as FormMetadata);
				}
				string filterClauseWihtKey = " FNumber= @number or FName=@name ";
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
				queryBuilderParemeter.FilterClauseWihtKey = filterClauseWihtKey;
				queryBuilderParemeter.FormId = "ORG_Organizations";
				List<SqlParam> list = new List<SqlParam>();
				string text = Convert.ToString(filterRow.Value);
				list.Add(new SqlParam("@number", 16, text));
				list.Add(new SqlParam("@name", 16, text));
				queryBuilderParemeter.SqlParams.AddRange(list);
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadFromCache(base.Context, this.orgMetaData.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					num = Convert.ToInt64(dynamicObject["Id"]);
				}
			}
			if (num == 0L)
			{
				num = base.Context.CurrentOrganizationInfo.ID;
			}
			List<long> permissionOrg = this.GetPermissionOrg();
			if (!permissionOrg.Contains(num) && num > 0L)
			{
				num = 0L;
				this.View.ShowErrMessage(ResManager.LoadKDString("没有当前组织的业务权限，请重新选择!", "0151515153499000013567", 7, new object[0]), "", 0);
			}
			if (num > 0L)
			{
				List<long> list2 = DICommonServericeHelper.GetEquipmentByUserAuth(base.Context, base.Context.UserId, num).ToList<long>();
				if (list2 != null && list2.Count > 0)
				{
					e.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						TableName = "TABLE(fn_StrSplit(@spEqmIds,',',1))",
						TableNameAs = "SPE",
						FieldName = "FID",
						ScourceKey = "FEquipmentId"
					});
					e.SqlParams.Add(new SqlParam("@spEqmIds", 161, list2.Distinct<long>().ToArray<long>()));
				}
				else
				{
					num = 0L;
					this.View.ShowErrMessage(ResManager.LoadKDString("没有设置设备监控权限，请设置！", "0151515153499000013568", 7, new object[0]), "", 0);
				}
			}
			if (string.IsNullOrWhiteSpace(e.FilterString))
			{
				e.FilterString = " FOrgId= @OrgId";
			}
			else
			{
				e.FilterString = string.Format("{0} AND FOrgId= @OrgId ", e.FilterString);
			}
			e.SqlParams.Add(new SqlParam("@OrgId", 12, num));
			e.AppendQueryOrderby("FChangeTime desc");
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0001F108 File Offset: 0x0001D308
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpper()) != null)
			{
				if (!(a == "BATCHMODIFYPRDEVENT"))
				{
					return;
				}
				bool flag = this.ValidateSelectDatas();
				if (flag)
				{
					this.ShowBatchModify();
				}
			}
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0001F150 File Offset: 0x0001D350
		private bool ValidateSelectDatas()
		{
			bool result = true;
			string text = null;
			if (this.ListView.SelectedRowsInfo.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("至少选中一行数据！", "0151515153499000013569", 7, new object[0]), "", 0);
				result = false;
			}
			else
			{
				new StringBuilder();
				string text2 = null;
				if (this.eqmStatusMetadata == null)
				{
					this.eqmStatusMetadata = (MetaDataServiceHelper.Load(base.Context, "ENG_EqmStatusChgLogBill", true) as FormMetadata);
				}
				this.lstSelectDatas.Clear();
				foreach (ListSelectedRow listSelectedRow in this.ListView.SelectedRowsInfo)
				{
					long num = Convert.ToInt64(listSelectedRow.PrimaryKeyValue);
					DynamicObject dynamicObject = BusinessDataServiceHelper.LoadFromCache(base.Context, new object[]
					{
						num
					}, this.eqmStatusMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
					DynamicObject dynamicObject2 = dynamicObject["ExpType"] as DynamicObject;
					if (dynamicObject2 != null)
					{
						long num2 = Convert.ToInt64(dynamicObject2["msterID"]);
						if (num2 == 10001L || num2 == 10002L)
						{
							text = ResManager.LoadKDString("不能批改开工或完工待机事件！", "0151515153499000013570", 7, new object[0]);
							result = false;
							break;
						}
					}
					string text3 = Convert.ToString(dynamicObject["ChangedStatus"]);
					if (string.IsNullOrWhiteSpace(text2))
					{
						text2 = text3;
					}
					else if (text2 != text3)
					{
						text = ResManager.LoadKDString("选中行的变更后状态必须相同才能批改！", "0151515153499000013571", 7, new object[0]);
						result = false;
						break;
					}
					this.lstSelectDatas.Add(dynamicObject);
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					this.View.ShowErrMessage(text, "", 0);
				}
			}
			return result;
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0001F490 File Offset: 0x0001D690
		private void ShowBatchModify()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "ENG_EQMStatusAlertTypeBatEdit";
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.CustomComplexParams.Add("SelectDatas", this.lstSelectDatas);
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null)
				{
					IOperationResult operationResult = new OperationResult();
					foreach (DynamicObject dynamicObject in this.lstSelectDatas)
					{
						OperateResult operateResult = new OperateResult();
						operateResult.SuccessStatus = true;
						string value = string.Format(ResManager.LoadKDString("工序计划：{0}-{1}-{2}", "0151515153499000013572", 7, new object[0]), Convert.ToString(dynamicObject["OpPlanBillNo"]), Convert.ToString(dynamicObject["OpSeq"]), Convert.ToString(dynamicObject["OpNo"]));
						operateResult.Name = string.Format(ResManager.LoadKDString("变更时间：{0}", "0151515153499000013573", 7, new object[0]), Convert.ToDateTime(dynamicObject["ChangeTime"]).ToString());
						operateResult.Message = Convert.ToString(value);
						operateResult.PKValue = Convert.ToInt64(dynamicObject["Id"]);
						operateResult.MessageType = -1;
						operationResult.OperateResult.Add(operateResult);
					}
					FormUtils.ShowOperationResult(this.View, operationResult, null);
					this.ListView.Refresh();
				}
			});
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0001F4FC File Offset: 0x0001D6FC
		private List<long> GetPermissionOrg()
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = "ENG_EqmStatusChgLogBill",
				PermissionControl = this.View.BillBusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = this.View.ParentFormView.Model.SubSytemId
			};
			return MFGServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x0400014B RID: 331
		private List<DynamicObject> lstSelectDatas = new List<DynamicObject>();

		// Token: 0x0400014C RID: 332
		private FormMetadata orgMetaData;

		// Token: 0x0400014D RID: 333
		private FormMetadata eqmStatusMetadata;
	}
}
