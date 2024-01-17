using System;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000020 RID: 32
	public class salOutStock2stkInStock : AbstractConvertPlugIn
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00009FEC File Offset: 0x000081EC
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] array = e.Result.FindByEntityKey("FBillHead");
			bool flag = array != null && array.Length != 0;
			if (flag)
			{
				foreach (ExtendedDataEntity extendedDataEntity in array)
				{
					DynamicObject dataEntity = extendedDataEntity.DataEntity;
					string arg = dataEntity["F_PCQE_FLORGID_id"].ToString();
					string arg2 = (dataEntity["StockOrgId"] as DynamicObject)["id"].ToString();
					IViewService service = ServiceHelper.GetService<IViewService>();
					string text = string.Format("select A.FSUPPLIERID from T_BD_SUPPLIER A\r\n                                                 inner join T_ORG_ORGANIZATIONS B on A.FCORRESPONDORGID=B.FPARENTID\r\n                                                 where   A.FUSEORGID={0} and B.FORGID={1}", arg2, arg);
					DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
					bool flag2 = dynamicObjectCollection.Count > 0;
					if (flag2)
					{
						string text2 = dynamicObjectCollection[0]["FSUPPLIERID"].ToString();
						dataEntity["SupplierId_id"] = text2;
						dataEntity["SupplyId_id"] = text2;
						dataEntity["SettleId_id"] = text2;
						dataEntity["ChargeId_id"] = text2;
						BaseDataField baseDataField = e.TargetBusinessInfo.GetField("FSupplierId") as BaseDataField;
						DynamicObject[] array3 = service.LoadFromCache(base.Context, new object[]
						{
							text2
						}, baseDataField.RefFormDynamicObjectType);
						baseDataField.RefIDDynamicProperty.SetValue(dataEntity, text2);
						baseDataField.DynamicProperty.SetValue(dataEntity, array3[0]);
						BaseDataField baseDataField2 = e.TargetBusinessInfo.GetField("FSupplyId") as BaseDataField;
						DynamicObject[] array4 = service.LoadFromCache(base.Context, new object[]
						{
							text2
						}, baseDataField2.RefFormDynamicObjectType);
						baseDataField2.RefIDDynamicProperty.SetValue(dataEntity, text2);
						baseDataField2.DynamicProperty.SetValue(dataEntity, array4[0]);
						BaseDataField baseDataField3 = e.TargetBusinessInfo.GetField("FSettleId") as BaseDataField;
						DynamicObject[] array5 = service.LoadFromCache(base.Context, new object[]
						{
							text2
						}, baseDataField3.RefFormDynamicObjectType);
						baseDataField3.RefIDDynamicProperty.SetValue(dataEntity, text2);
						baseDataField3.DynamicProperty.SetValue(dataEntity, array5[0]);
						BaseDataField baseDataField4 = e.TargetBusinessInfo.GetField("FChargeId") as BaseDataField;
						DynamicObject[] array6 = service.LoadFromCache(base.Context, new object[]
						{
							text2
						}, baseDataField4.RefFormDynamicObjectType);
						baseDataField4.RefIDDynamicProperty.SetValue(dataEntity, text2);
						baseDataField4.DynamicProperty.SetValue(dataEntity, array6[0]);
					}
				}
			}
		}
	}
}
