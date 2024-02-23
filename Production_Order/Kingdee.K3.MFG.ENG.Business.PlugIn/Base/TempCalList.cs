using System;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001A RID: 26
	public class TempCalList : AbstractListPlugIn
	{
		// Token: 0x06000284 RID: 644 RVA: 0x0001DFD4 File Offset: 0x0001C1D4
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBCREATETEMPCALENDAR"))
				{
					return;
				}
				if (this.ListView.SelectedRowsInfo.Count != 1)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请选择一条临时日历记录进行套用", "0151515151774000013705", 7, new object[0]), 0);
					return;
				}
				DynamicObject[] array = BusinessDataServiceHelper.LoadFromCache(base.Context, new object[]
				{
					Convert.ToInt64(this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()[0])
				}, this.ListView.BillBusinessInfo.GetDynamicObjectType());
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "ENG_CreateTempCalendar";
				dynamicFormShowParameter.OpenStyle.ShowType = 4;
				dynamicFormShowParameter.CustomComplexParams.Add("Source", array[0]);
				this.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0001E0BD File Offset: 0x0001C2BD
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (string.IsNullOrWhiteSpace(e.SortString))
			{
				e.SortString = "FDate desc,FEquipmentId";
			}
		}
	}
}
