using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
	// Token: 0x02000006 RID: 6
	[HotUpdate]
	[Description("物料保存时进行重复性校验")]
	public class SaveCheak : AbstractOperationServicePlugIn
	{
		// Token: 0x06000019 RID: 25 RVA: 0x00003B78 File Offset: 0x00001D78
		public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
		{
			base.BeforeExecuteOperationTransaction(e);
			foreach (ExtendedDataEntity extendedDataEntity in e.SelectedRows)
			{
				int num = 0;
				DynamicObject dataEntity = extendedDataEntity.DataEntity;
				DynamicObjectCollection dynamicObjectCollection = dataEntity["MATERIALBASE"] as DynamicObjectCollection;
				bool flag = dataEntity["USEORGID_ID"].ToString() == "100229";
				if (flag)
				{
					bool flag2 = Convert.ToString(dataEntity["DOCUMENTSTATUS"]) != "C";
					if (flag2)
					{
						bool flag3 = num == 0;
						if (flag3)
						{
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.AppendLine("SELECT FNUMBER FROM T_BD_MATERIAL AS ml");
							stringBuilder.AppendLine("INNER JOIN T_BD_MATERIAL_L AS ml_l ON ml.FMATERIALID = ml_l.FMATERIALID");
							stringBuilder.AppendLine("INNER JOIN T_BD_MATERIALBASE AS mlbs ON ml.FMATERIALID = mlbs.FMATERIALID");
							stringBuilder.AppendLine("WHERE ml.FMATERIALID <> '" + dataEntity["Id"] + "' ");
							stringBuilder.AppendLine("AND ml_l.FNAME = '" + dataEntity["NAME"] + "' ");
							stringBuilder.AppendLine("AND ml_l.FSPECIFICATION = '" + dataEntity["SPECIFICATION"] + "' ");
							stringBuilder.AppendLine("AND ml.FMNEMONICCODE = '" + dataEntity["MNEMONICCODE"] + "' ");
							stringBuilder.AppendLine("AND mlbs.FBASEUNITID = '" + dynamicObjectCollection[0]["BASEUNITID_ID"] + "' ");
							stringBuilder.AppendLine("AND ml.FUSEORGID = '" + dataEntity["USEORGID_ID"] + "'");
							stringBuilder.AppendLine("AND ml.FFORBIDSTATUS = '" + dataEntity["FORBIDSTATUS"] + "'");
							DynamicObjectCollection dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(base.Context, stringBuilder.ToString(), null, null, CommandType.Text, new SqlParam[0]);
							bool flag4 = dynamicObjectCollection2.Count > 0;
							if (flag4)
							{
								num = 1;
							}
							stringBuilder.Clear();
							bool flag5 = num == 1;
							if (flag5)
							{
								throw new KDBusinessException("", "该物料已存在相同名称、规格型号，重复物料编码：" + dynamicObjectCollection2[0]["FNUMBER"]);
							}
						}
					}
				}
			}
		}
	}
}
