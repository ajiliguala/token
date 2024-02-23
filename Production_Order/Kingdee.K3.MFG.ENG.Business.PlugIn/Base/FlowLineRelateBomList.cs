using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001F RID: 31
	[Description("流程生产线与物料关系-列表插件")]
	public class FlowLineRelateBomList : BaseControlList
	{
		// Token: 0x060002A9 RID: 681 RVA: 0x0001F5BC File Offset: 0x0001D7BC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "CheckBOP"))
				{
					if (!(a == "ChildPrdOfferMaterial"))
					{
						return;
					}
					List<string> list = new List<string>();
					ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
					if (selectedRowsInfo.Count<ListSelectedRow>() <= 0)
					{
						this.View.ShowMessage(this.msg, 4);
						return;
					}
					foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
					{
						list.Add(listSelectedRow.PrimaryKeyValue.ToString());
					}
					FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_FlowLineRelateBom");
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
					queryBuilderParemeter.BusinessInfo = formMetaData.BusinessInfo;
					queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						TableName = "TABLE(fn_StrSplit(@PkIds,',',1))",
						TableNameAs = "SP",
						FieldName = "FID",
						ScourceKey = "FID"
					});
					queryBuilderParemeter.SqlParams.Add(new SqlParam("@PkIds", 161, list.Distinct<string>().ToArray<string>()));
					List<string> list2 = new List<string>
					{
						"FID",
						"FUseOrgId",
						"FProductLineId",
						"FMaterialId"
					};
					DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, formMetaData.BusinessInfo.GetSubBusinessInfo(list2).GetDynamicObjectType(), queryBuilderParemeter);
					array = (from o in array
					where !DataEntityExtend.GetDynamicObjectItemValue<bool>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(o, "PRODUCTLINEID", null), "FMainLineType", false)
					select o).ToArray<DynamicObject>();
					int num = 1;
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.FormId = "ENG_ChildPrdOfferMaterial";
					dynamicFormShowParameter.ParentPageId = this.View.PageId;
					dynamicFormShowParameter.OpenStyle.ShowType = 6;
					foreach (DynamicObject value in array)
					{
						dynamicFormShowParameter.CustomComplexParams.Add(num.ToString(), value);
						num++;
					}
					this.View.ShowForm(dynamicFormShowParameter);
				}
				else
				{
					List<string> list3 = new List<string>();
					ListSelectedRowCollection selectedRowsInfo2 = this.ListView.SelectedRowsInfo;
					if (selectedRowsInfo2.Count<ListSelectedRow>() <= 0)
					{
						this.View.ShowMessage(this.msg, 4);
						return;
					}
					foreach (ListSelectedRow listSelectedRow2 in selectedRowsInfo2)
					{
						list3.Add(listSelectedRow2.PrimaryKeyValue.ToString());
					}
					FormMetadata formMetaData2 = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_FlowLineRelateBom");
					QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter();
					queryBuilderParemeter2.BusinessInfo = formMetaData2.BusinessInfo;
					queryBuilderParemeter2.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						TableName = "TABLE(fn_StrSplit(@PkIds,',',1))",
						TableNameAs = "SP",
						FieldName = "FID",
						ScourceKey = "FID"
					});
					queryBuilderParemeter2.SqlParams.Add(new SqlParam("@PkIds", 161, list3.Distinct<string>().ToArray<string>()));
					List<string> list4 = new List<string>
					{
						"FID",
						"FUseOrgId",
						"FProductLineId",
						"FMaterialId"
					};
					DynamicObject[] array3 = BusinessDataServiceHelper.Load(base.Context, formMetaData2.BusinessInfo.GetSubBusinessInfo(list4).GetDynamicObjectType(), queryBuilderParemeter2);
					int num2 = 0;
					DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
					dynamicFormShowParameter2.FormId = "ENG_CheckBOP";
					dynamicFormShowParameter2.ParentPageId = this.View.PageId;
					dynamicFormShowParameter2.OpenStyle.ShowType = 6;
					foreach (DynamicObject value2 in array3)
					{
						dynamicFormShowParameter2.CustomComplexParams.Add(num2.ToString(), value2);
						num2++;
					}
					this.View.ShowForm(dynamicFormShowParameter2);
					return;
				}
			}
		}

		// Token: 0x0400014F RID: 335
		private string msg = ResManager.LoadKDString("请至少选择一条流程生产线与物料关系数据", "015072000018126", 7, new object[0]);
	}
}
