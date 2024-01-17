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
	// Token: 0x02000008 RID: 8
	[Description("出口贸易统计表报表插件")]
	public class CKMYTJB : SysReportBaseService
	{
		// Token: 0x06000015 RID: 21 RVA: 0x00003079 File Offset: 0x00001279
		public override void Initialize()
		{
			base.Initialize();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00003084 File Offset: 0x00001284
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("FORGNAME", new LocaleValue("组织"), 167, true);
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
			return reportHeader;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000031D8 File Offset: 0x000013D8
		public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
		{
			return this.SetSummaryFieldInfo(filter);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000031F4 File Offset: 0x000013F4
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
				new SummaryField("FBGAMOUNT", 1)
			};
		}

		// Token: 0x06000019 RID: 25 RVA: 0x0000328C File Offset: 0x0000148C
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			base.BuilderReportSqlAndTempTable(filter, tableName);
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			string empty = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("/*dialect*/select TName.*,{0} into {1}  from (");
			stringBuilder.AppendLine("select g.FNAME FORGNAME,f.fname FCUSTNAME,c.FNAME FMYFS,d.FNAME FYSFS,e.FNAME FCURRENCYID,\r\n                                sum(F_GROSSWEIGHT) FGROSSWEIGHT,sum(F_NETWEIGHT) FNETWEIGHT,sum(a.F_VOLUME) FVOLUME,\r\n                                count(a.FBILLNO) FCOUNT,sum(a.F_TRANSCOST) FTRANSCOST,sum(b.FSumAmount) FSUMAOUNT,sum(F_BGAMOUNT) FBGAMOUNT \r\n                                from CX_EXPORT a \r\n                                inner join (select FBillNo,sum(F_AMOUNT) FSumAmount from CX_CKENTRY ck inner join CX_EXPORT ckh on ck.FID=ckh.FID \r\n                                group by FBILLNO) b on a.FBILLNO=b.FBILLNO \r\n                                inner join CX_TRADEMODE_L c on a.F_TRADEMMODE=c.FID and c.FLocaleID=2052\r\n                                inner join CX_TRANSMODE_L d on a.F_TRANSMODE=d.FID and d.FLocaleID=2052 \r\n                                inner join T_BD_CURRENCY_L e on a.F_FCURRENCY1=e.FCURRENCYID and e.FLOCALEID=2052 \r\n                                inner join T_BD_CUSTOMER_L f on a.F_PIKU_BASE1=f.FCUSTID and f.FLOCALEID=2052 \r\n                                inner join T_ORG_ORGANIZATIONS_L g on a.F_ORGID=g.FORGID and g.FLOCALEID=2052 \r\n                                WHERE 1=1");
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
			bool flag3 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(customFilter["F_CUSTNAME"]);
			if (flag3)
			{
				stringBuilder.Append(string.Format(" AND F.FNAME LIKE '%{0}%' ", customFilter["F_CUSTNAME"].ToString()));
			}
			stringBuilder.Append(string.Concat(new string[]
			{
				"AND CONVERT(date,a.F_SBDATE) >= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_START"]).ToString(),
				"') AND CONVERT(date,a.F_SBDATE) <= CONVERT(date,'",
				Convert.ToDateTime(customFilter["F_END"]).ToString(),
				"')"
			}));
			stringBuilder.AppendLine("group by g.FNAME,f.FNAME,c.FNAME,d.FNAME,e.FNAME");
			stringBuilder.AppendLine(") TName");
			this.KSQL_SEQ = string.Format(this.KSQL_SEQ, "FORGNAME,FCUSTNAME");
			DBUtils.Execute(base.Context, string.Format(stringBuilder.ToString(), this.KSQL_SEQ, tableName));
		}
	}
}
