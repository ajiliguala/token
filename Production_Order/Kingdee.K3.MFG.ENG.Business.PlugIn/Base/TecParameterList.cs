using System;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000018 RID: 24
	[HotUpdate]
	[Description("工艺参数列表插件")]
	public class TecParameterList : BaseControlList
	{
		// Token: 0x0600026A RID: 618 RVA: 0x0001D064 File Offset: 0x0001B264
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			e.SortString = string.Empty;
			if (e.SelectedEntities.Any((FilterEntity o) => o.Key == "FBillHead"))
			{
				e.AppendQueryOrderby("FSeqNumber");
				e.AppendQueryOrderby("FOperNumber");
			}
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0001D0C4 File Offset: 0x0001B2C4
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.documentStatus = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.View.OpenParameter.GetCustomParameter("DocumentStatus")) ? "" : Convert.ToString(this.View.OpenParameter.GetCustomParameter("DocumentStatus")));
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0001D11C File Offset: 0x0001B31C
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			e.Cancel = true;
			long num = base.Context.IsMultiOrg ? Convert.ToInt64(this.ListView.CurrentSelectedRowInfo.DataRow["FOrgId_Id"]) : base.Context.CurrentOrganizationInfo.ID;
			string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "ENG_TECPARAMETER";
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.MultiSelect = false;
			if (this.documentStatus.Equals("B") || this.documentStatus.Equals("C"))
			{
				billShowParameter.Status = 1;
			}
			else
			{
				billShowParameter.Status = 2;
			}
			billShowParameter.PKey = primaryKeyValue;
			billShowParameter.CustomComplexParams.Add("PrdOrgId", num);
			billShowParameter.CustomComplexParams.Add("documentStatus", this.documentStatus);
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0001D21C File Offset: 0x0001B41C
		private DynamicObject GetEntryCurrentRow()
		{
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntity");
			return this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0001D260 File Offset: 0x0001B460
		private DynamicObject GetCurrentRowData()
		{
			string primaryKeyValue = this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_TECPARAMETER", true) as FormMetadata;
			return BusinessDataServiceHelper.LoadSingle(base.Context, primaryKeyValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0001D2B0 File Offset: 0x0001B4B0
		private DynamicObject GetRoute(string routeNo)
		{
			string text = string.Format("FNUMBER = '{0}' ", routeNo);
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_TECPARAMETER", true) as FormMetadata;
			return BusinessDataServiceHelper.Load(base.Context, formMetadata.BusinessInfo, null, oqlfilter).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x04000138 RID: 312
		private string documentStatus;
	}
}
