using System;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000D0 RID: 208
	public class SimEffectChangeControler : AbstractItemControler
	{
		// Token: 0x06000E95 RID: 3733 RVA: 0x000A953C File Offset: 0x000A773C
		public override void DoOperation()
		{
			IOperationResult operationResult = ECNOrderServiceHelper.SimEffectChange(base.View.Context, base.View.BusinessInfo, base.Model.DataObject);
			if (operationResult.IsSuccess)
			{
				if (ListUtils.IsEmpty<OperateResult>(operationResult.OperateResult))
				{
					base.View.ShowMessage(ResManager.LoadKDString("模拟生效执行成功!", "0151515153499000014785", 7, new object[0]), 0, new Action<MessageBoxResult>(this.OpenSimMaterialList), "", 0);
				}
				else
				{
					operationResult.OperateResult.Add(new OperateResult
					{
						Message = ResManager.LoadKDString("模拟生效执行成功!", "0151515153499000014785", 7, new object[0]),
						DataEntityIndex = -1,
						SuccessStatus = true,
						MessageType = -1
					});
					base.View.ShowOperateResult(operationResult.OperateResult, new Action<FormResult>(this.OperateOpenSimMaterialList), "ENG_ECNBatchTips");
				}
				base.View.Refresh();
				return;
			}
			base.View.ShowOperateResult(operationResult.OperateResult, "ENG_ECNBatchTips");
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x000A9648 File Offset: 0x000A7848
		public void OpenSimMaterialList(MessageBoxResult result)
		{
			if (result == 1)
			{
				string arg = base.Model.GetValue("FBillNo").ToString();
				long value = MFGBillUtil.GetValue<long>(base.Model, "FChangeOrgId", -1, 0L, null);
				ListShowParameter listShowParameter = new ListShowParameter();
				listShowParameter.FormId = "ENG_BBEBOM";
				listShowParameter.ListType = 1;
				listShowParameter.CustomParams.Add("ECNSimEffect", "ECNSimEffect");
				listShowParameter.ListFilterParameter.Filter = string.Format("fcomputeid = '{0}'", arg);
				listShowParameter.UseOrgId = value;
				listShowParameter.OpenStyle.ShowType = 7;
				base.View.ShowForm(listShowParameter, new Action<FormResult>(this.DeleteSimBomData));
			}
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x000A96F8 File Offset: 0x000A78F8
		public void OperateOpenSimMaterialList(FormResult result)
		{
			string arg = base.Model.GetValue("FBillNo").ToString();
			long value = MFGBillUtil.GetValue<long>(base.Model, "FChangeOrgId", -1, 0L, null);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_BBEBOM";
			listShowParameter.ListType = 1;
			listShowParameter.CustomParams.Add("ECNSimEffect", "ECNSimEffect");
			listShowParameter.ListFilterParameter.Filter = string.Format("fcomputeid = '{0}'", arg);
			listShowParameter.UseOrgId = value;
			listShowParameter.OpenStyle.ShowType = 7;
			base.View.ShowForm(listShowParameter, new Action<FormResult>(this.DeleteSimBomData));
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x000A97AC File Offset: 0x000A79AC
		private void DeleteSimBomData(FormResult formResult)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BBEBOM",
				SelectItems = SelectorItemInfo.CreateItems("FID"),
				FilterClauseWihtKey = "fcomputeid =@fcomputeid"
			};
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@fcomputeid", 16, base.Model.GetValue("FBillNo").ToString()));
			DynamicObjectCollection dynamicObjectCollection = MFGServiceHelper.GetDynamicObjectCollection(base.View.Context, queryBuilderParemeter, null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BBEBOM", true);
				object[] array = (from i in dynamicObjectCollection
				select DataEntityExtend.GetDynamicValue<object>(i, "FID", null)).ToArray<object>();
				BusinessDataServiceHelper.Delete(base.View.Context, array, formMetadata.BusinessInfo.GetDynamicObjectType());
			}
		}

		// Token: 0x0400069F RID: 1695
		private Action<MessageBoxOptions> evnetHandler;
	}
}
