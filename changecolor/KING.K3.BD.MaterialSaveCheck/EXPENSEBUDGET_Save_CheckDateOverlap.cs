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

namespace KING.K3.BD.MaterialSaveCheck
{
	// Token: 0x02000003 RID: 3
	[HotUpdate]
	[Description("物料保存时进行重复性校验")]
	public class EXPENSEBUDGET_Save_CheckDateOverlap : AbstractOperationServicePlugIn
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000022C0 File Offset: 0x000004C0
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
							StringBuilder stringBuilder2 = stringBuilder;
							string str = "WHERE ml.FMATERIALID <> '";
							object obj = dataEntity["Id"];
							stringBuilder2.AppendLine(str + ((obj != null) ? obj.ToString() : null) + "' ");
							StringBuilder stringBuilder3 = stringBuilder;
							string str2 = "AND ml_l.FNAME = '";
							object obj2 = dataEntity["NAME"];
							stringBuilder3.AppendLine(str2 + ((obj2 != null) ? obj2.ToString() : null) + "' ");
							StringBuilder stringBuilder4 = stringBuilder;
							string str3 = "AND ml_l.FSPECIFICATION = '";
							object obj3 = dataEntity["SPECIFICATION"];
							stringBuilder4.AppendLine(str3 + ((obj3 != null) ? obj3.ToString() : null) + "' ");
							StringBuilder stringBuilder5 = stringBuilder;
							string str4 = "AND ml.FMNEMONICCODE = '";
							object obj4 = dataEntity["MNEMONICCODE"];
							stringBuilder5.AppendLine(str4 + ((obj4 != null) ? obj4.ToString() : null) + "' ");
							StringBuilder stringBuilder6 = stringBuilder;
							string str5 = "AND mlbs.FBASEUNITID = '";
							object obj5 = dynamicObjectCollection[0]["BASEUNITID_ID"];
							stringBuilder6.AppendLine(str5 + ((obj5 != null) ? obj5.ToString() : null) + "' ");
							StringBuilder stringBuilder7 = stringBuilder;
							string str6 = "AND ml.FUSEORGID = '";
							object obj6 = dataEntity["USEORGID_ID"];
							stringBuilder7.AppendLine(str6 + ((obj6 != null) ? obj6.ToString() : null) + "'");
							StringBuilder stringBuilder8 = stringBuilder;
							string str7 = "AND ml.FFORBIDSTATUS = '";
							object obj7 = dataEntity["FORBIDSTATUS"];
							stringBuilder8.AppendLine(str7 + ((obj7 != null) ? obj7.ToString() : null) + "'");
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
								string text = "";
								string str8 = "该物料已存在相同名称、规格型号，重复物料编码：";
								object obj8 = dynamicObjectCollection2[0]["FNUMBER"];
								throw new KDBusinessException(text, str8 + ((obj8 != null) ? obj8.ToString() : null));
							}
						}
					}
				}
			}
		}
	}
}
