using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000098 RID: 152
	[Description("变更日志删除（动态表单）处理类")]
	public class EquipmentLogDeleteEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000AF2 RID: 2802 RVA: 0x0007D92B File Offset: 0x0007BB2B
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.EquipmentIds = e.Paramter.GetCustomParameter("EquipmentIds").ToString();
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x0007D94F File Offset: 0x0007BB4F
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.LoadData();
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x0007D960 File Offset: 0x0007BB60
		private void LoadData()
		{
			DateTime dateTime = MFGServiceHelper.GetSysDate(base.Context).AddMonths(-3);
			this.View.Model.SetValue("FDeleteTime", dateTime);
			this.View.UpdateView("FDeleteTime");
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x0007D9B0 File Offset: 0x0007BBB0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			FormMetadata metaData = MetaDataServiceHelper.Load(base.Context, "ENG_EqmStatusChgLogBill", true) as FormMetadata;
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbDelete"))
				{
					return;
				}
				int num = this.DeleteLog(metaData);
				if (num > 0)
				{
					this.SaveLog(metaData);
					this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("成功删除{0}条变更日志", "015072000012088", 7, new object[0]), num), "", 0);
					return;
				}
				this.View.ShowErrMessage(ResManager.LoadKDString("没有要删除的变更日志！", "015072000012089", 7, new object[0]), "", 0);
			}
		}

		// Token: 0x06000AF6 RID: 2806 RVA: 0x0007DA78 File Offset: 0x0007BC78
		private int DeleteLog(FormMetadata metaData)
		{
			int result = 0;
			DateTime dateTime = Convert.ToDateTime(this.Model.GetValue("FDeleteTime").ToString());
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_EqmStatusChgLogBill",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FID",
					"FEQUIPMENTID",
					"FCHANGETIME"
				}),
				FilterClauseWihtKey = " FCHANGETIME < @FDELETETIME "
			};
			ExtJoinTableDescription item = new ExtJoinTableDescription
			{
				FieldName = "FID",
				ScourceKey = "FEQUIPMENTID",
				TableName = "table(fn_StrSplit(@FID,',',1))",
				TableNameAs = "TMP"
			};
			queryBuilderParemeter.ExtJoinTables.Add(item);
			SqlParam item2 = new SqlParam("@FDELETETIME", 6, dateTime);
			queryBuilderParemeter.SqlParams.Add(item2);
			SqlParam item3 = new SqlParam("@FID", 161, (from o in this.EquipmentIds.Split(new char[]
			{
				','
			})
			select Convert.ToInt64(o)).Distinct<long>().ToArray<long>());
			queryBuilderParemeter.SqlParams.Add(item3);
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count > 0)
			{
				result = dynamicObjectCollection.Count;
				List<object> list = (from coll in dynamicObjectCollection
				select coll["FID"]).ToList<object>();
				BusinessDataServiceHelper.Delete(base.Context, metaData.BusinessInfo, list.ToArray(), null, "");
			}
			return result;
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x0007DC2C File Offset: 0x0007BE2C
		private void SaveLog(FormMetadata metaData)
		{
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("IgnoreWarning", true);
			string arg = this.Model.GetValue("FDeleteTime").ToString();
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(this.View.Context);
			string[] array = this.EquipmentIds.Split(new char[]
			{
				','
			});
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (string text in array)
			{
				DynamicObject dynamicObject = new DynamicObject(metaData.BusinessInfo.GetDynamicObjectType());
				dynamicObject["EquipmentId_Id"] = text;
				dynamicObject["ChangerId_Id"] = base.Context.UserId;
				dynamicObject["ChangeTime"] = systemDateTime;
				dynamicObject["ChangeType"] = "A";
				dynamicObject["Description"] = string.Format(ResManager.LoadKDString("删除{0}以前的日志信息", "015072000012090", 7, new object[0]), arg);
				dynamicObject["ModifyDate"] = systemDateTime;
				list.Add(dynamicObject);
			}
			BusinessDataServiceHelper.Save(base.Context, metaData.BusinessInfo, list.ToArray(), operateOption, "Save");
		}

		// Token: 0x04000523 RID: 1315
		private string EquipmentIds = string.Empty;
	}
}
