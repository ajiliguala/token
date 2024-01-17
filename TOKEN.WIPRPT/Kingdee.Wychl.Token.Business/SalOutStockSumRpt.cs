using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000021 RID: 33
	[HotUpdate]
	[Description("销售出库汇总")]
	public class SalOutStockSumRpt : SysReportBaseService
	{
		// Token: 0x06000065 RID: 101 RVA: 0x0000A298 File Offset: 0x00008498
		public override void Initialize()
		{
			base.Initialize();
			this.IsCreateTempTableByPlugin = true;
			base.ReportProperty.IsGroupSummary = true;
			base.ReportProperty.DecimalControlFieldList = new List<DecimalControlField>
			{
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FOutStockCost",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FArAmount",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FRATE",
					DecimalControlFieldName = "FPrcision"
				}
			};
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0000A340 File Offset: 0x00008540
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			bool flag = Convert.ToBoolean(customFilter["FSummaryRpt"]);
			bool flag2 = Convert.ToBoolean(customFilter["FIsGpByOrg"]);
			bool flag3 = Convert.ToBoolean(customFilter["FIsGpByCust"]);
			ReportHeader reportHeader = new ReportHeader();
			bool flag4 = flag;
			if (flag4)
			{
				reportHeader.AddChild("FSalOrgNAME", new LocaleValue("销售组织"), 167, true);
			}
			else
			{
				reportHeader.AddChild("FMaterialNumber", new LocaleValue("物料编码"), 167, true);
				reportHeader.AddChild("FMaterialName", new LocaleValue("物料名称"), 167, true);
				reportHeader.AddChild("FMaterialModel", new LocaleValue("规格型号"), 167, true);
				reportHeader.AddChild("FMaterialCATEGORYID", new LocaleValue("存货类别"), 167, true);
				reportHeader.AddChild("FUnit", new LocaleValue("单位"), 167, true);
				bool flag5 = flag2;
				if (flag5)
				{
					reportHeader.AddChild("FSalOrgNAME", new LocaleValue("销售组织"), 167, true);
				}
				bool flag6 = flag3;
				if (flag6)
				{
					reportHeader.AddChild("FCustomerName", new LocaleValue("客户"), 167, true);
				}
			}
			reportHeader.AddChild("FOutStockQty", new LocaleValue("销售出库数量"), 106, true);
			reportHeader.AddChild("FOutStockCost", new LocaleValue("销售出库总成本"), 106, true);
			reportHeader.AddChild("FArQty", new LocaleValue("应收数量"), 106, true);
			reportHeader.AddChild("FArAmount", new LocaleValue("应收金额"), 106, true);
			reportHeader.AddChild("FRate", new LocaleValue("毛利率(%)"), 106, true);
			return reportHeader;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x0000A528 File Offset: 0x00008728
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			base.BuilderReportSqlAndTempTable(filter, tableName);
			IDBService service = ServiceHelper.GetService<IDBService>();
			string[] array = service.CreateTemporaryTableName(base.Context, 3);
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			object obj = customFilter["Fbegindate"];
			object obj2 = customFilter["Fenddate"];
			bool flag = Convert.ToBoolean(customFilter["FSummaryRpt"]);
			bool flag2 = Convert.ToBoolean(customFilter["FIsGpByOrg"]);
			bool flag3 = Convert.ToBoolean(customFilter["FIsGpByCust"]);
			string text = "'" + customFilter["FBillType"].ToString().Replace(",", "','") + "'";
			string text2 = "";
			List<long> isolationOrgList = filter.FilterParameter.IsolationOrgList;
			bool flag4 = isolationOrgList.Count > 0;
			if (flag4)
			{
				text2 = " in (" + string.Join<long>(",", isolationOrgList) + ")";
			}
			string text3 = array[2];
			DBServiceHelper.Execute(base.Context, "/*dialect*/create table " + text3 + "(\r\n                                                    FPrision int default 2,\r\n                                                    FSalOrg int,\r\n                                                    FMaterialNumber nvarchar(255),\r\n                                                    FMaterialName nvarchar(510),\r\n                                                    FMaterialModel nvarchar(510),\r\n                                                    FMaterialCATEGORYID nvarchar(255),\r\n                                                    FSalOrgNAME nvarchar(255),\r\n                                                    FCustomerName nvarchar(255),\r\n                                                    FOutStockQty decimal(23,10) default 0,\r\n                                                    FOutStockCost decimal(23,10) default 0,\r\n                                                    FArQty decimal(23,10) default 0,\r\n                                                    FArAmount decimal(23,10) default 0,\r\n                                                    )");
			DBServiceHelper.Execute(base.Context, string.Format("/*dialect*/insert into {0}(\r\n                                                    FSalOrg,FMaterialNumber,FCustomerName,FOutStockQty,FOutStockCost)\r\n                                                    select FSALEORGID,FMATERIALID,FCUSTOMERID,sum(FREALQTY),sum(FCOSTAMOUNT_LC) from T_SAL_OUTSTOCKENTRY oe \r\n                                                    inner join T_SAL_OUTSTOCK o on oe.FID=o.FID \r\n                                                    inner join T_SAL_OUTSTOCKENTRY_F oef on oe.FENTRYID=oef.FENTRYID\r\n                                                    where o.FDOCUMENTSTATUS='C' and o.FDATE between '{1}' and '{2}' \r\n                                                    and FSALEORGID {3}\r\n                                                    group by FSALEORGID,FMATERIALID,FCUSTOMERID", new object[]
			{
				text3,
				obj,
				obj2,
				text2
			}));
			DBServiceHelper.Execute(base.Context, string.Format("/*dialect*/insert into {0}(\r\n                                                    FSalOrg,FMaterialNumber,FCustomerName,FOutStockQty,FOutStockCost)\r\n                                                    select tss.FSTOCKORGID,tssy.FMATERIALID,tbc.FCUSTID,sum(tssy.FQTY),sum(tssy.FAMOUNT)\r\n                                                    from T_STK_STKTRANSFEROUT tss\r\n                                                    inner join T_STK_STKTRANSFEROUTENTRY tssy on tssy.FID=tss.FID\r\n                                                    inner join T_ORG_ORGANIZATIONS org on org.FORGID=tss.FSTOCKORGID\r\n                                                    inner join T_ORG_ORGANIZATIONS inorg on inorg.FORGID=tss.FSTOCKINORGID\r\n                                                    inner join T_BD_CUSTOMER tbc on tbc.FCORRESPONDORGID=inorg.FPARENTID and tbc.FUSEORGID=tss.FSTOCKORGID\r\n                                                    where tss.FTRANSFERBIZTYPE='OverOrgTransfer'  and tss.FTRANSFERDIRECT='GENERAL'\r\n                                                    and inorg.FPARENTID!=org.FPARENTID and tss.FDOCUMENTSTATUS='C' and tss.FDATE between '{1}' and '{2}' \r\n                                                    and tss.FSTOCKORGID {3}\r\n                                                    group by tss.FSTOCKORGID,tssy.FMATERIALID,tbc.FCUSTID", new object[]
			{
				text3,
				obj,
				obj2,
				text2
			}));
			DBServiceHelper.Execute(base.Context, string.Format("/*dialect*/insert into {0}(\r\n                                                    FSalOrg,FMaterialNumber,FCustomerName,FOutStockQty,FOutStockCost)\r\n                                                    select tss.FSTOCKORGID,tssy.FMATERIALID,tbc.FCUSTID,-sum(tssy.FQTY),-sum(tssy.FAMOUNT)\r\n                                                    from T_STK_STKTRANSFERIN tss\r\n                                                    inner join T_STK_STKTRANSFERINENTRY tssy on tssy.FID=tss.FID\r\n                                                    inner join T_ORG_ORGANIZATIONS org on org.FORGID=tss.FSTOCKOUTORGID\r\n                                                    inner join T_ORG_ORGANIZATIONS inorg on inorg.FORGID=tss.FSTOCKORGID\r\n                                                    inner join T_BD_CUSTOMER tbc on tbc.FCORRESPONDORGID=org.FPARENTID and tbc.FUSEORGID=tss.FSTOCKORGID\r\n                                                    where tss.FTRANSFERBIZTYPE='OverOrgTransfer'  and tss.FTRANSFERDIRECT='RETURN' and tss.FOBJECTTYPEID='STK_TRANSFERIN'\r\n                                                    and inorg.FPARENTID!=org.FPARENTID and tss.FDATE between '{1}' and '{2}' \r\n                                                    and tss.FSTOCKORGID {3}\r\n                                                    group by tss.FSTOCKORGID,tssy.FMATERIALID,tbc.FCUSTID", new object[]
			{
				text3,
				obj,
				obj2,
				text2
			}));
			DBServiceHelper.Execute(base.Context, string.Format("/*dialect*/insert into {0}(\r\n                                                    FSalOrg,FMaterialNumber,FCustomerName,FOutStockQty,FOutStockCost)\r\n                                                    select FSALEORGID,FMATERIALID,FRETCUSTID,-sum(FREALQTY),-sum(FCOSTAMOUNT_LC) from T_SAL_RETURNSTOCKENTRY re \r\n                                                    inner join T_SAL_RETURNSTOCK r on re.FID=r.FID  \r\n                                                    inner join T_SAL_RETURNSTOCKENTRY_F ref on re.FENTRYID=ref.FENTRYID \r\n                                                    where r.FDOCUMENTSTATUS='C' and r.FDATE between '{1}' and '{2}' and FSALEORGID {3}\r\n                                                    group by FSALEORGID,FMATERIALID,FRETCUSTID", new object[]
			{
				text3,
				obj,
				obj2,
				text2
			}));
			DBServiceHelper.Execute(base.Context, string.Format("/*dialect*/insert into {0}(\r\n                                                    FSalOrg,FMaterialNumber,FCustomerName,FArQty,FArAmount)\r\n                                                    select FSALEORGID,FMATERIALID,FCUSTOMERID,sum(FPRICEQTY),sum(FNOTAXAMOUNT) from T_AR_RECEIVABLEENTRY ae \r\n                                                    inner join T_AR_RECEIVABLE a on ae.FID=a.FID \r\n                                                    where a.FDOCUMENTSTATUS='C' and a.FDATE between '{1}' and '{2}' and FBILLTYPEID in({3}) \r\n                                                    and FSALEORGID {4}\r\n                                                    group by FSALEORGID,FMATERIALID,FCUSTOMERID", new object[]
			{
				text3,
				obj,
				obj2,
				text,
				text2
			}));
			string text4 = array[0];
			DBServiceHelper.Execute(base.Context, string.Concat(new string[]
			{
				"/*dialect*/select FPrision,FSalOrg,FMaterialNumber,FMaterialName,FMaterialCATEGORYID,FMaterialModel,FSalOrgNAME,FCustomerName,\r\n                                                    convert(decimal(18,2),sum(FOutStockQty)) FOutStockQty,convert(decimal(18,2),sum(FOutStockCost)) FOutStockCost,\r\n                                                    convert(decimal(18,2),sum(FArQty)) FArQty,convert(decimal(18,2),sum(FArAmount)) FArAmount \r\n                                                    into ",
				text4,
				" from ",
				text3,
				" \r\n                                                    group by FPrision,FSalOrg,FMaterialNumber,FMaterialName,FMaterialModel,FSalOrgNAME,FCustomerName,FMaterialCATEGORYID"
			}));
			DBServiceHelper.Execute(base.Context, "/*dialect*/alter table " + text4 + " add FRate decimal(23,10)");
			DBServiceHelper.Execute(base.Context, "/*dialect*/alter table " + text4 + " add FUnit nvarchar(255)");
			DBServiceHelper.Execute(base.Context, "/*dialect*/update a set a.FSalOrgNAME=ol.FNAME,FCustomerName=cl.FNAME,a.FMaterialNumber=m.FNUMBER,FUnit=ul.FNAME,a.FMaterialCATEGORYID=mcl.FNAME,\r\n                                                    a.FMaterialName=ml.FNAME,FMaterialModel=ml.FSPECIFICATION,Frate=(case FArAmount when 0 then 0 else (FArAmount-FOutStockCost)/FArAmount*100 end) \r\n                                                    from " + text4 + " a \r\n                                                    inner join T_ORG_ORGANIZATIONS_L ol on a.FSalOrg=ol.FORGID and ol.FLOCALEID=2052 \r\n                                                    inner join T_BD_CUSTOMER_L cl on a.FCustomerName=cl.FCUSTID and cl.FLOCALEID=2052 \r\n                                                    inner join T_BD_MATERIAL m on a.FMaterialNumber=m.FMATERIALID \r\n                                                    inner join T_BD_MATERIAL_L ml on a.FMaterialNumber=ml.FMATERIALID and ml.FLOCALEID=2052 \r\n                                                    inner join T_BD_MATERIALBASE mb on a.FMaterialNumber=mb.FMATERIALID \r\n                                                    inner join T_BD_MATERIALCATEGORY_L mcl on mcl.FCATEGORYID=mb.FCATEGORYID and mcl.FLOCALEID=2052 \r\n                                                    inner join  T_BD_UNIT_L ul on mb.FBASEUNITID=ul.FUNITID and ul.FLOCALEID=2052 ");
			DBServiceHelper.Execute(base.Context, "/*dialect*/update a set a.FSalOrgNAME=ol.FNAME,FCustomerName=cl.FNAME,Frate=(case FArAmount when 0 then 0 else (FArAmount-FOutStockCost)/FArAmount*100 end) \r\n                                                    from " + text4 + " a \r\n                                                    inner join T_ORG_ORGANIZATIONS_L ol on a.FSalOrg = ol.FORGID and ol.FLOCALEID = 2052\r\n                                                    inner join T_BD_CUSTOMER_L cl on a.FCustomerName = cl.FCUSTID and cl.FLOCALEID = 2052\r\n                                                    where FMaterialNumber = '0'");
			bool flag5 = flag;
			if (flag5)
			{
				string text5 = array[1];
				DBServiceHelper.Execute(base.Context, string.Concat(new string[]
				{
					"/*dialect*/select FSalOrg,FSalOrgNAME,sum(FOutStockQty) FOutStockQty,sum(FOutStockCost) FOutStockCost,sum(FArQty) FArQty,sum(FArAmount) FArAmount,\r\n                                                         case sum(FArAmount) when 0 then 0 else (sum(FArAmount) - sum(FOutStockCost)) / sum(FArAmount) * 100 end FRate into ",
					text5,
					" from ",
					text4,
					" group by FSalOrg,FSalOrgNAME"
				}));
				DBServiceHelper.Execute(base.Context, string.Concat(new string[]
				{
					"/*dialect*/insert into ",
					text5,
					" select 9999999,'合计',sum(FOutStockQty),sum(FOutStockCost),sum(FArQty),sum(FArAmount),\r\ncase sum(FArAmount) when 0 then 0 else (sum(FArAmount) - sum(FOutStockCost)) / sum(FArAmount) * 100 end from  ",
					text5,
					" "
				}));
				DBServiceHelper.Execute(base.Context, "/*dialect*/select row_number() over(order by FSalOrg) FIDENTITYID,2 FPrcision,* into " + tableName + " from " + text5);
			}
			else
			{
				string text6 = "";
				bool flag6 = flag2;
				if (flag6)
				{
					text6 += "FSalOrgNAME,";
				}
				bool flag7 = flag3;
				if (flag7)
				{
					text6 += "FCustomerName,";
				}
				DBServiceHelper.Execute(base.Context, string.Concat(new string[]
				{
					"/*dialect*/select row_number() over(order by FSalOrg,FMaterialNumber) FIDENTITYID,2 FPrcision,",
					text6,
					"\r\n                                                       FMaterialNumber, FMaterialName,FMaterialCATEGORYID, FMaterialModel, FUnit, sum(FOutStockQty) FOutStockQty, sum(FOutStockCost) FOutStockCost, sum(FArQty) FArQty, sum(FArAmount) FArAmount,\r\n                                                       case sum(FArAmount) when 0 then 0 else (sum(FArAmount) - sum(FOutStockCost)) / sum(FArAmount) * 100 end FRate into ",
					tableName,
					"\r\n                                                       from ",
					text4,
					" \r\n                                                       group by FSalOrg,",
					text6,
					"\r\n                                                       FMaterialNumber,FMaterialName,FMaterialModel,FUnit,FMaterialCATEGORYID"
				}));
			}
			ITemporaryTableService service2 = ServiceHelper.GetService<ITemporaryTableService>();
			service2.DeleteTemporaryTableName(base.Context);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x0000A930 File Offset: 0x00008B30
		public override ReportTitles GetReportTitles(IRptParams filter)
		{
			ReportTitles reportTitles = base.GetReportTitles(filter);
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			object obj = customFilter["Fbegindate"];
			object obj2 = customFilter["Fenddate"];
			bool flag = customFilter != null;
			if (flag)
			{
				bool flag2 = reportTitles == null;
				if (flag2)
				{
					reportTitles = new ReportTitles();
					reportTitles.AddTitle("Fbegindate", obj.ToString());
					reportTitles.AddTitle("Fenddate", obj2.ToString());
				}
			}
			return reportTitles;
		}
	}
}
