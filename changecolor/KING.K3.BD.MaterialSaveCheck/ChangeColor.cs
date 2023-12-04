﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace KING.K3.BD.MaterialSaveCheck.v2
{
	// Token: 0x02000002 RID: 2
	[HotUpdate]
	[Description("物料重复改变颜色")]
	public class ChangeColor : AbstractListPlugIn
	{
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void OnFormatRowConditions(ListFormatConditionArgs args)
        {
            base.OnFormatRowConditions(args);
            FormatCondition formatCondition = new FormatCondition();
            formatCondition.ApplayRow = true;
            bool documentStatusIsNotC = args.DataRow["FDOCUMENTSTATUS"].ToString() != "C";
            //筛选组织为基础资料组织=100229
            bool useOrgIdIsSpecificValue = args.DataRow["FUseOrgId_Id"].ToString() == "100229";

            if (documentStatusIsNotC && useOrgIdIsSpecificValue)
            {
                object materialId = args.DataRow["FMATERIALID"];
                if (materialId != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("SELECT 1 FROM T_BD_MATERIAL ml");
                    stringBuilder.AppendLine("INNER JOIN T_BD_MATERIAL_L AS ml_l ON ml.FMATERIALID = ml_l.FMATERIALID");
                    stringBuilder.AppendLine("INNER JOIN T_BD_MATERIALBASE AS mlbs ON ml.FMATERIALID = mlbs.FMATERIALID");
                    stringBuilder.AppendLine("WHERE ml.FMATERIALID <> @MaterialId");
                    stringBuilder.AppendLine("AND ml_l.FNAME = (SELECT MAX(FNAME) FROM T_BD_MATERIAL_L WHERE FMATERIALID = @MaterialId)");
                    stringBuilder.AppendLine("AND ml_l.FSPECIFICATION = (SELECT MAX(FSPECIFICATION) FROM T_BD_MATERIAL_L WHERE FMATERIALID = @MaterialId)");
                    stringBuilder.AppendLine("AND ml.FMNEMONICCODE = (SELECT MAX(FMNEMONICCODE) FROM T_BD_MATERIAL WHERE FMATERIALID = @MaterialId)");
                    stringBuilder.AppendLine("AND mlbs.FBASEUNITID = (SELECT MAX(FBASEUNITID) FROM T_BD_MATERIALBASE WHERE FMATERIALID = @MaterialId)");
                    stringBuilder.AppendLine("AND ml.FUSEORGID = (SELECT MAX(FUSEORGID) FROM T_BD_MATERIAL WHERE FMATERIALID = @MaterialId)");
                    stringBuilder.AppendLine("AND ml.FFORBIDSTATUS = (SELECT MAX(FFORBIDSTATUS) FROM T_BD_MATERIAL WHERE FMATERIALID = @MaterialId)");

                    // 使用参数化查询
                    List<SqlParam> sqlParams = new List<SqlParam>
            {
                new SqlParam("@MaterialId", KDDbType.String, materialId.ToString())
            };

                    DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(base.Context, stringBuilder.ToString(), null, null, CommandType.Text, sqlParams.ToArray());
                    if (dynamicObjectCollection.Count > 0)
                    {
                        formatCondition.BackColor = "#FF0000";
                    }
                }
            }

            args.FormatConditions.Add(formatCondition);
        }

    }
}
