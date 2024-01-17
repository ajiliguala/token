using System;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000009 RID: 9
	public class FYBYD_Save_CheckFYYS : AbstractOperationServicePlugIn
	{
		// Token: 0x0600001B RID: 27 RVA: 0x00003528 File Offset: 0x00001728
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			int num = Convert.ToInt32(e.DataEntitys[0]["OrgID_Id"]);
			string text = "ER_SystemParameter";
			string text2 = "F_ExpensesPlan";
			string text3 = Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, (long)num, 0L, text, text2, 0L));
			bool flag = text3 == "n" || ObjectUtils.IsNullOrEmptyOrWhiteSpace(text3);
			if (!flag)
			{
				string text4 = string.Empty;
				DynamicObject dynamicObject = e.DataEntitys[0];
				int num2 = Convert.ToInt32(dynamicObject["ID"]);
				string text5 = string.Format("/*dialect*/select org2.FNAME orgname,DEP2.FNAME depname,EXP2.FNAME expname,value.fdiffamount,value.ys_FBILLNO,value.ys_FSEQ,FLOCEXPSUBMITAMOUNT\r\n                            from\r\n                            (\tselect fy.FExpenseOrgId,fy.F_KING_BUDGETDEPT,fy.FExpID,sum(fy.FLOCEXPSUBMITAMOUNT) FLOCEXPSUBMITAMOUNT,\r\n\t                            MIN(ys.F_KING_TOTALAMOUNT) F_KING_TOTALAMOUNT,ys.F_KING_STARTDATE,ys.F_KING_ENDDATE\r\n\t                            ,sum(fy.FLOCEXPSUBMITAMOUNT)-isnull(MIN(ys.F_KING_TOTALAMOUNT),0) fdiffamount\r\n\t                            ,max(ys.FBILLNO) ys_FBILLNO,max(ys.FSEQ) ys_FSEQ\r\n\t                            from (  select t11.FDATE,t11.FExpenseOrgId,t12.F_KING_BUDGETDEPT,t12.FExpID\r\n\t\t\t                            from T_ER_EXPENSEREIMB t11 inner join T_ER_EXPENSEREIMBENTRY t12 on t11.FID=t12.FID and  t11.fid ={0}\r\n                                        group by t11.FDATE,t11.FExpenseOrgId,t12.F_KING_BUDGETDEPT,t12.FExpID \r\n\t                            ) bd  \t\r\n\t                            inner join  \r\n\t                            (   select y1.FBILLNO,y2.FSEQ,y1.F_KING_YEAR,y1.F_KING_BUDGETORGID,y2.F_KING_DEPARTMENT,y2.F_KING_ITEM,y2.F_KING_STARTDATE, y2.F_KING_ENDDATE,y2.F_KING_TOTALAMOUNT\r\n\t\t                            from KING_t_EXPENSEBUDGET y1\r\n\t\t                            inner join  KING_t_EXPENSEBUDGETEntry y2 on y1.FID=y2.FID   and y1.FDOCUMENTSTATUS='c'\r\n\t                            ) ys  \ton   bd.FExpenseOrgId= ys.F_KING_BUDGETORGID and bd.F_KING_BUDGETDEPT=ys.F_KING_DEPARTMENT and bd.FExpID=ys.F_KING_ITEM \r\n\t                            and bd.FDATE between ys.F_KING_STARTDATE and ys.F_KING_ENDDATE \r\n\t\r\n\t                            inner join\r\n\t                            (   select t1.FDATE,t1.FExpenseOrgId,t2.F_KING_BUDGETDEPT,t2.FExpID,t2.FLOCEXPSUBMITAMOUNT\r\n\t\t                            from T_ER_EXPENSEREIMB t1\r\n\t\t                            inner join T_ER_EXPENSEREIMBENTRY t2 on t1.FID=t2.FID and t1.FDOCUMENTSTATUS <> 'Z'   \r\n\t                            ) fy \ton   bd.FExpenseOrgId=fy.FExpenseOrgId and bd.F_KING_BUDGETDEPT=fy.F_KING_BUDGETDEPT and bd.FExpID=fy.FExpID \r\n\t                            and fy.FDATE between ys.F_KING_STARTDATE and ys.F_KING_ENDDATE\r\n\t                            group by fy.FExpenseOrgId,fy.F_KING_BUDGETDEPT,fy.FExpID,ys.F_KING_STARTDATE,ys.F_KING_ENDDATE\r\n\t                            HAVING sum(fy.FLOCEXPSUBMITAMOUNT)-isnull(MIN(ys.F_KING_TOTALAMOUNT),0 ) >0 \r\n                            ) value\r\n                            inner join T_ORG_Organizations org1 on value.FExpenseOrgId=org1.FORGID\r\n                            inner join T_ORG_ORGANIZATIONS_L org2 on org1.FORGID=org2.FORGID and org2.FLocaleId=2052\r\n                            inner join T_BD_DEPARTMENT DEP1 on value.F_KING_BUDGETDEPT=DEP1.FDEPTID\r\n                            inner join T_BD_DEPARTMENT_L DEP2 on DEP1.FDEPTID=DEP2.FDEPTID and DEP2.FLOCALEID=2052\r\n                            inner join T_BD_EXPENSE EXP1 on value.FEXPID=EXP1.FEXPID\r\n                            inner join T_BD_EXPENSE_L EXP2 on EXP1.FEXPID=EXP2.FEXPID and EXP2.FLOCALEID=2052 ", num2);
				using (IDataReader dataReader = DBUtils.ExecuteReader(base.Context, text5))
				{
					while (dataReader.Read())
					{
						text4 += string.Format("{0,-20} {1,-20} {2,-20} 超出预算：{3:N2}    预算单号：{4} 第{5}行\r\n", new object[]
						{
							dataReader["orgname"],
							dataReader["depname"],
							dataReader["expname"],
							dataReader["fdiffamount"],
							dataReader["ys_FBILLNO"],
							dataReader["ys_FSEQ"]
						});
					}
					dataReader.Close();
				}
				bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4);
				if (flag2)
				{
					throw new KDException("", text4);
				}
			}
		}
	}
}
