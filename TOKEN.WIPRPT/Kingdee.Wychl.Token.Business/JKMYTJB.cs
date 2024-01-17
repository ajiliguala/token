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

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200000E RID: 14
	[Description("进口贸易统计表报表插件")]
	public class JKMYTJB : SysReportBaseService
	{
		// Token: 0x06000026 RID: 38 RVA: 0x000038F2 File Offset: 0x00001AF2
		public override void Initialize()
		{
			base.Initialize();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000038FC File Offset: 0x00001AFC
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("FSUPPLYNAME", new LocaleValue("供应商"), 167, true);
			reportHeader.AddChild("FCUSTNAME", new LocaleValue("客户"), 167, true);
			reportHeader.AddChild("FMYFS", new LocaleValue("贸易方式"), 167, true);
			reportHeader.AddChild("FYSFS", new LocaleValue("运输方式"), 167, true);
			reportHeader.AddChild("FCURRENCYID", new LocaleValue("币别"), 167, true);
			reportHeader.AddChild("FGROSSWEIGHT", new LocaleValue("毛重合计"), 106, true);
			reportHeader.AddChild("FNETWEIGHT", new LocaleValue("净重合计"), 106, true);
			reportHeader.AddChild("FVOLUME", new LocaleValue("体积合计"), 106, true);
			reportHeader.AddChild("FCOUNT", new LocaleValue("报关单数量合计"), 56, true);
			reportHeader.AddChild("FTRANSCOST", new LocaleValue("物流费用合计"), 106, true);
			reportHeader.AddChild("FSUMAOUNT", new LocaleValue("金额合计"), 106, true);
			reportHeader.AddChild("FBGAMOUNT", new LocaleValue("报关费合计"), 106, true);
			reportHeader.AddChild("FGS", new LocaleValue("关税合计"), 106, true);
			reportHeader.AddChild("FZZS", new LocaleValue("增值税合计"), 106, true);
			return reportHeader;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003A84 File Offset: 0x00001C84
		public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
		{
			return this.SetSummaryFieldInfo(filter);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003AA0 File Offset: 0x00001CA0
		public List<SummaryField> SetSummaryFieldInfo(IRptParams filter)
		{
			return new List<SummaryField>
			{
				new SummaryField("FGROSSWEIGHT", 1),
				new SummaryField("FNETWEIGHT", 1),
				new SummaryField("FVOLUME", 1),
				new SummaryField("FCOUNT", 1),
				new SummaryField("FTRANSCOST", 1),
				new SummaryField("FSUMAOUNT", 1),
				new SummaryField("FBGAMOUNT", 1),
				new SummaryField("FGS", 1),
				new SummaryField("FZZS", 1)
			};
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003B5C File Offset: 0x00001D5C
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			base.BuilderReportSqlAndTempTable(filter, tableName);
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			string empty = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("/*dialect*/select TName.*,{0} into {1}  from (");
			stringBuilder.AppendLine("select f.FNAME FSUPPLYNAME,g.FNAME FCUSTNAME,c.FNAME FMYFS,d.FNAME FYSFS,e.FNAME FCURRENCYID,\r\n                                sum(F_GROSSWEIGHT) FGROSSWEIGHT,sum(F_NETWEIGHT) FNETWEIGHT,sum(a.F_VOLUME) FVOLUME,\r\n                                count(a.FBILLNO) FCOUNT,sum(F_TRANSCOST) FTRANSCOST,sum(F_BGAMOUNT) FBGAMOUNT,\r\n                                sum(b.GS) FGS,sum(b.ZZS) FZZS,sum(b.FSumAmount) FSUMAOUNT\r\n                                from CX_IMDECLARATION a \r\n                                inner join (select FBillNo,sum(F_AMOUNTS) FSumAmount,\r\n                                sum(F_GSTAX) GS,sum(F_ZZSTAX) ZZS from CX_JKBGDENTRY ck inner join CX_IMDECLARATION ckh on ck.FID=ckh.FID group by FBILLNO) b on a.FBILLNO=b.FBILLNO \r\n                                inner join CX_TRADEMODE_L c on a.F_TRADEMMODE=c.FID and c.FLocaleID=2052\r\n                                inner join CX_TRANSMODE_L d on a.F_TRANSMODE=d.FID and d.FLocaleID=2052 \r\n                                inner join T_BD_CURRENCY_L e on a.F_CURRENY=e.FCURRENCYID and e.FLOCALEID=2052 \r\n                                left join t_BD_Supplier_L f on a.F_SUPPLIER=f.Fsupplierid and e.FLOCALEID=2052 \r\n                                left join t_BD_CUSTOMER_L g on a.F_PIKU_BASE1=g.FCUSTID and e.FLOCALEID=2052  \r\n                                WHERE 1=1");
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
				stringBuilder.Append(string.Format(" and A.F_ORGID in ({0}) ", text));
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
					stringBuilder.Append(string.Format(" and A.F_ORGID in ({0}) ", text2));
				}
			}
			stringBuilder.Append(string.Concat(new string[]
			{
				"AND CONVERT(date,a.F_SBDATE) >= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_START"]).ToString(),
				"') AND CONVERT(date,a.F_SBDATE) <= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_END"]).ToString(),
				"')"
			}));
			stringBuilder.AppendLine("group by c.FNAME,d.FNAME,e.FNAME,f.FNAME,g.FNAME");
			StringBuilder stringBuilder2 = stringBuilder;
			string[] array = new string[5];
			array[0] = "having (f.FNAME like '%";
			int num = 1;
			object obj = customFilter["F_SUPPLYNAME"];
			array[num] = ((obj != null) ? obj.ToString() : null);
			array[2] = "%' and g.FNAME is null) or(g.FNAME like '%";
			int num2 = 3;
			object obj2 = customFilter["F_CUSTNAME"];
			array[num2] = ((obj2 != null) ? obj2.ToString() : null);
			array[4] = "%' and f.FNAME is null)";
			stringBuilder2.AppendLine(string.Concat(array));
			stringBuilder.AppendLine(") TName");
			this.KSQL_SEQ = string.Format(this.KSQL_SEQ, "FSUPPLYNAME,FCUSTNAME");
			DBUtils.Execute(base.Context, string.Format(stringBuilder.ToString(), this.KSQL_SEQ, tableName));
		}
	}
}
