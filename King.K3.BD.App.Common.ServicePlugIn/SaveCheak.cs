using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
using System.Text;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("物料保存时进行重复性校验")]
    public class SaveCheak : AbstractOperationServicePlugIn
    {

        //本节使用的方法,事务开始前事件
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            //所选择的行,循环,读取单据体 MATERIAL
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {
                int flog = 0;
                DynamicObject ml = extended.DataEntity;


                DynamicObjectCollection mlbs = ml["MATERIALBASE"] as DynamicObjectCollection;

                //如果物料编码重复,报错

                //非基础资料组织物料不参与重复性校验
                if (ml["USEORGID_ID"].ToString() == "100229")
                {
                    //基础资料组织下的已审核物料直接批改不参与重复性校验
                    if (Convert.ToString(ml["DOCUMENTSTATUS"]) != "C")
                    {
                        if (flog == 0)
                        {
                            StringBuilder sql = new StringBuilder();
                            sql.AppendLine("SELECT FNUMBER FROM T_BD_MATERIAL AS ml");
                            sql.AppendLine("INNER JOIN T_BD_MATERIAL_L AS ml_l ON ml.FMATERIALID = ml_l.FMATERIALID");
                            sql.AppendLine("INNER JOIN T_BD_MATERIALBASE AS mlbs ON ml.FMATERIALID = mlbs.FMATERIALID");
                            sql.AppendLine("WHERE ml.FMATERIALID <> '" + ml["Id"] + "' ");//不等于自己
                            sql.AppendLine("AND ml_l.FNAME = '" + ml["NAME"] + "' ");//物料名称相同
                            sql.AppendLine("AND ml_l.FSPECIFICATION = '" + ml["SPECIFICATION"] + "' ");//规格型号相同
                            sql.AppendLine("AND ml.FMNEMONICCODE = '" + ml["MNEMONICCODE"] + "' ");//助记码相同
                            sql.AppendLine("AND mlbs.FBASEUNITID = '" + mlbs[0]["BASEUNITID_ID"] + "' ");//单位相同
                            sql.AppendLine("AND ml.FUSEORGID = '" + ml["USEORGID_ID"] + "'");//使用组织相同
                            sql.AppendLine("AND ml.FFORBIDSTATUS = '" + ml["FORBIDSTATUS"] + "'");//禁用状态相同

                            DynamicObjectCollection iReturn = DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
                            if (iReturn.Count > 0)
                            {
                                flog = 1;
                            }

                            sql.Clear();

                            if (flog == 1)
                            {
                                throw new KDBusinessException("", "该物料已存在相同名称、规格型号，重复物料编码：" + iReturn[0]["FNUMBER"]);
                            }
                        }
                    }
                }
            }
        }
    }
}
