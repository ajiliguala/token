using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200000F RID: 15
	[Description("进料加工核注清单报表插件")]
	public class JLJGHZQD : SysReportBaseService
	{
		// Token: 0x0600002C RID: 44 RVA: 0x00003E19 File Offset: 0x00002019
		public override void Initialize()
		{
			base.Initialize();
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003E24 File Offset: 0x00002024
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("FBGDCDXH", new LocaleValue("报关单草单序号"), 167, true);
			reportHeader.AddChild("FNUMBER", new LocaleValue("物料代码"), 167, true);
			reportHeader.AddChild("FBAXH", new LocaleValue("备案序号"), 167, true);
			reportHeader.AddChild("FLZSBXH", new LocaleValue("流转申报表序号"), 167, true);
			reportHeader.AddChild("F_Currency", new LocaleValue("币制"), 167, true);
			reportHeader.AddChild("FSBSL", new LocaleValue("申报数量"), 167, true);
			reportHeader.AddChild("FFDSL", new LocaleValue("法定数量"), 167, true);
			reportHeader.AddChild("FDDDW", new LocaleValue("法定计量单位"), 167, true);
			reportHeader.AddChild("FDEFDSL", new LocaleValue("第二法定数量"), 167, true);
			reportHeader.AddChild("FDEFDDW", new LocaleValue("第二法定计量单位"), 167, true);
			reportHeader.AddChild("FQYSBDJ", new LocaleValue("企业申报单价"), 167, true);
			reportHeader.AddChild("FQYSBZJ", new LocaleValue("企业申报总价"), 167, true);
			reportHeader.AddChild("FYCG", new LocaleValue("原厂国"), 167, true);
			reportHeader.AddChild("FZLBL", new LocaleValue("重量比例因子"), 167, true);
			reportHeader.AddChild("FDYBL", new LocaleValue("第一比例因子"), 167, true);
			reportHeader.AddChild("FDEBL", new LocaleValue("第二比例因子"), 167, true);
			reportHeader.AddChild("FMZ", new LocaleValue("毛重"), 167, true);
			reportHeader.AddChild("FJZ", new LocaleValue("净重"), 167, true);
			reportHeader.AddChild("FYTDM", new LocaleValue("用途代码"), 167, true);
			reportHeader.AddChild("FZMFS", new LocaleValue("征免方式"), 167, true);
			reportHeader.AddChild("FDHBB", new LocaleValue("单耗版本号"), 167, true);
			reportHeader.AddChild("FBZ", new LocaleValue("备注"), 167, true);
			reportHeader.AddChild("FPZDLX", new LocaleValue("凭证单类型"), 167, true);
			reportHeader.AddChild("FBILLNO", new LocaleValue("凭证单号"), 167, true);
			reportHeader.AddChild("FHH", new LocaleValue("行号"), 167, true);
			reportHeader.AddChild("FZZMDG", new LocaleValue("最终目的国"), 167, true);
			reportHeader.AddChild("FDATE", new LocaleValue("收料日期"), 61, true);
			return reportHeader;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00004130 File Offset: 0x00002330
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			base.BuilderReportSqlAndTempTable(filter, tableName);
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			string empty = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("/*dialect*/select TName.*,{0} into {1}  from (");
			stringBuilder.AppendLine("SELECT A.FSEQ FBGDCDXH,E.FNUMBER,D.FSeq FBAXH,'' as FLZSBXH,\r\n                                CASE BF.FSETTLECURRID WHEN 3 THEN '300' WHEN 4 THEN '116' WHEN 6 THEN '303' WHEN 7 THEN '502' ELSE '' END F_Currency,\r\n                                CAST(CONVERT(DECIMAL(18,0),A.FACTRECEIVEQTY*E.F_DZDYXPS) AS VARCHAR) FSBSL,\r\n                                CASE E.F_FDJLDW2 WHEN '' THEN CAST(CONVERT(DECIMAL(18,2),EB.FNETWEIGHT*A.FACTRECEIVEQTY) AS VARCHAR)  ELSE CAST(CONVERT(DECIMAL(18,0),A.FACTRECEIVEQTY*E.F_DZDYXPS) AS VARCHAR)  END FFDSL,\r\n                                CASE E.F_FDJLDW WHEN '千克' THEN '035' WHEN '个' THEN '007' WHEN '台' THEN '001' ELSE '' END FDDDW,\r\n                                CASE E.F_FDJLDW2 WHEN '' THEN '' ELSE CAST(CONVERT(DECIMAL(18,5),EB.FNETWEIGHT*A.FACTRECEIVEQTY) AS VARCHAR) END as FDEFDSL,\r\n                                CASE E.F_FDJLDW2 WHEN '千克' THEN '035' WHEN '个' THEN '007' WHEN '台' THEN '001' ELSE '' END FDEFDDW,\r\n                                '' FQYSBDJ,CAST(CONVERT(DECIMAL(18,2),FAMOUNT) AS VARCHAR) as FQYSBZJ,\r\n                                CASE B.F_ORGINALCTY WHEN '中国' THEN '142' WHEN '韩国' THEN '133' WHEN '香港' THEN '110' \r\n                                WHEN '台湾' THEN '143' WHEN '日本' THEN '116' WHEN '马来西亚' THEN '122' ELSE B.F_ORGINALCTY END FYCG,\r\n                                '' FZLBL,'' FDYBL,'' FDEBL,'' FMZ,CAST(CONVERT(DECIMAL(18,5),EB.FNETWEIGHT*A.FACTRECEIVEQTY) AS VARCHAR) FJZ,\r\n                                '05' FYTDM,'3' FZMFS,'' FDHBB,'' FBZ,'4'  FPZDLX,B.FBILLNO,A.FSEQ FHH,'142' FZZMDG,B.FDATE \r\n                                FROM T_PUR_ReceiveEntry A \r\n                                INNER JOIN T_PUR_Receive B ON A.FID=B.FID \r\n                                INNER JOIN T_PUR_RECEIVEFIN BF ON B.FID=BF.FID \r\n                                INNER JOIN T_PUR_RECEIVEENTRY_F AF ON A.FENTRYID=AF.FENTRYID \r\n                                INNER JOIN PIKU_t_Cust100013 C ON B.F_JGMYZC=C.FID \r\n                                INNER JOIN CX_BD_ZCLJ D ON C.FID=D.FID \r\n                                INNER JOIN T_BD_MATERIAL E ON A.FMATERIALID=E.FMATERIALID \r\n                                INNER JOIN T_BD_MATERIAL F ON D.F_MATERIALNO=F.FMATERIALID \r\n                                INNER JOIN t_BD_MaterialBase EB ON A.FMATERIALID=EB.FMATERIALID \r\n                                WHERE E.FNUMBER=F.FNUMBER ");
			DynamicObjectCollection dynamicObjectCollection = customFilter["F_ORGID"] as DynamicObjectCollection;
			bool flag = dynamicObjectCollection.Count > 0;
			if (flag)
			{
				string text = string.Empty;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					text = text + dynamicObject["F_ORGID_Id"].ToString() + ",";
				}
				text = text.TrimEnd(new char[]
				{
					','
				});
				stringBuilder.Append(string.Format(" and B.FSTOCKORGID in ({0}) ", text));
			}
			else
			{
				List<Organization> userOrg = PermissionServiceHelper.GetUserOrg(base.Context);
				bool flag2 = userOrg.Count > 0;
				if (flag2)
				{
					string text2 = string.Empty;
					foreach (Organization organization in userOrg)
					{
						text2 = text2 + organization.Id.ToString() + ",";
					}
					text2 = text2.TrimEnd(new char[]
					{
						','
					});
					stringBuilder.Append(string.Format(" and B.FSTOCKORGID in ({0}) ", text2));
				}
			}
			bool flag3 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(customFilter["F_BILLNO"]);
			if (flag3)
			{
				stringBuilder.Append(string.Format(" AND B.FBILLNO LIKE '%{0}%' ", customFilter["F_BILLNO"].ToString()));
			}
			bool flag4 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(customFilter["F_MATERIALNUM"]);
			if (flag4)
			{
				stringBuilder.Append(string.Format(" AND E.FNUMBER LIKE '%{0}%' ", customFilter["F_MATERIALNUM"].ToString()));
			}
			stringBuilder.Append(string.Concat(new string[]
			{
				"AND CONVERT(date,B.FDATE) >= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_START"]).ToString(),
				"') AND CONVERT(date,B.FDATE) <= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_END"]).ToString(),
				"')"
			}));
			stringBuilder.AppendLine(") TName");
			this.KSQL_SEQ = string.Format(this.KSQL_SEQ, "FDATE DESC,FBGDCDXH ASC");
			DBUtils.Execute(base.Context, string.Format(stringBuilder.ToString(), this.KSQL_SEQ, tableName));
		}
	}
}
