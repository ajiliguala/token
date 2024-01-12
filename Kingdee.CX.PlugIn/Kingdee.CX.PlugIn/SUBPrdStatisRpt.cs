using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x0200001B RID: 27
	[Description("委外订单入库领料统计-财务")]
	[HotUpdate]
	public class SUBPrdStatisRpt : SysReportBaseService
	{
		// Token: 0x060000C1 RID: 193 RVA: 0x00009164 File Offset: 0x00007364
		public override void Initialize()
		{
			base.Initialize();
			this.IsCreateTempTableByPlugin = true;
			base.ReportProperty.DecimalControlFieldList = new List<DecimalControlField>
			{
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FInstockQty",
					DecimalControlFieldName = "FPrcision_QTY"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPickMtrlQty",
					DecimalControlFieldName = "FPrcision_QTY"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdCLCost",
					DecimalControlFieldName = "FPrcision_AMT"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdJGCost",
					DecimalControlFieldName = "FPrcision_AMT"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdSUMCost",
					DecimalControlFieldName = "FPrcision_AMT"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FMtrlAmount",
					DecimalControlFieldName = "FPrcision_AMT"
				}
			};
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000926C File Offset: 0x0000746C
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("FORGID", new LocaleValue("组织(全)"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FSUBREQBILLNO", new LocaleValue("委外订单编号(全)"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FSUBREQENTRYSEQ", new LocaleValue("委外订单行号(全)"), SqlStorageType.SqlInt, true);
			reportHeader.AddChild("FORGID_a", new LocaleValue("组织"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FSUBREQBILLNO_a", new LocaleValue("委外订单编号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FSUBREQENTRYSEQ_a", new LocaleValue("委外订单行号"), SqlStorageType.SqlInt, true);
			reportHeader.AddChild("FSUBPRDNUMBER", new LocaleValue("成品物料代码"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FPRDNAME", new LocaleValue("成品物料名称"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FPRDMODEL", new LocaleValue("成品规格型号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FPRDUNIT", new LocaleValue("成品单位"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FInstockQty", new LocaleValue("成品入库数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdCLCost", new LocaleValue("材料成本(本位币)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdJGCost", new LocaleValue("加工费(本位币)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdSUMCost", new LocaleValue("总成本(本位币)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FMTRLNUMBER", new LocaleValue("料件物料代码"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FMTRLNAME", new LocaleValue("料件物料名称"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FMTRLMODEL", new LocaleValue("料件规格型号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FMTRLUNIT", new LocaleValue("料件单位"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FPickMtrlQty", new LocaleValue("料件领料数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FMtrlAmount", new LocaleValue("委外总成本"), SqlStorageType.SqlDecimal, true);
			return reportHeader;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000094A0 File Offset: 0x000076A0
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			object obj = customFilter["Fbegindate"];
			object obj2 = customFilter["Fenddate"];
			List<long> isolationOrgList = filter.FilterParameter.IsolationOrgList;
			bool flag = isolationOrgList.Count > 0;
			if (flag)
			{
				string text = "where s.FSUBORGID in (" + string.Join<long>(",", isolationOrgList) + ")";
			}
			string strSQL = "/*dialect*/ create table #temp\r\n            (\r\n                Fidentity int identity(1,1),\r\n                FORGID nvarchar(50),\r\n                FSUBREQBILLNO nvarchar(50),\r\n                FSUBREQENTRYSEQ int,\r\n                FORGID_a nvarchar(50),\r\n                FSUBREQBILLNO_a nvarchar(50),\r\n                FSUBREQENTRYSEQ_a int,\r\n                FENTRYID int,\r\n                FSUBPRDNUMBER nvarchar(50),\r\n                FPRDNUMBER nvarchar(50),\r\n                FPRDNAME nvarchar(255),\r\n                FPRDMODEL nvarchar(510),\r\n                FPRDUNIT nvarchar(50),\r\n                FPRDINSTOCKQTY decimal(23,10) default 0,\r\n                FPRDRESTOCKQTY decimal(23,10) default 0,\r\n                FPRDIN_CLCOST decimal(23,10) default 0,\r\n                FPRDIN_JGCOST decimal(23,10) default 0,\r\n                FPRDIN_SUMCOST decimal(23,10) default 0,\r\n                FPRDRE_CLCOST decimal(23,10) default 0,\r\n                FPRDRE_JGCOST decimal(23,10) default 0,\r\n                FPRDRE_SUMCOST decimal(23,10) default 0,\r\n                FMTRLNUMBER nvarchar(50),\r\n                FMTRLNAME nvarchar(255),\r\n                FMTRLMODEL nvarchar(510),\r\n                FMTRLUNIT nvarchar(50),\r\n                FMTRLPICKQTY decimal(23,10) default 0,\r\n                FMTRLREQTY decimal(23,10) default 0,\r\n                FMTRLEEDQTY decimal(23,10) default 0,\r\n                FMTRLPICKAMOUNT decimal(23,10) default 0,\r\n                FMTRLREAMOUNT decimal(23,10) default 0,\r\n                FMTRLEEAMOUNT decimal(23,10) default 0\r\n            )\r\n            create table #temp_A\r\n            (\r\n                FSUBREQBILLNO nvarchar(50),\r\n                FSUBREQENTRYSEQ int,\r\n                FENTRYID int,\r\n                FMTRLNUMBER nvarchar(50),\r\n                FMTRLNAME nvarchar(255),\r\n                FMTRLMODEL nvarchar(510),\r\n                FMTRLUNIT nvarchar(50),\r\n                FMTRLPICKQTY decimal(23,10) default 0,\r\n                FMTRLREQTY decimal(23,10) default 0,\r\n                FMTRLEEDQTY decimal(23,10) default 0,\r\n                FMTRLPICKAMOUNT decimal(23,10) default 0,\r\n                FMTRLREAMOUNT decimal(23,10) default 0,\r\n                FMTRLEEAMOUNT decimal(23,10) default 0)\r\n                /*委外领料*/\r\n                insert into #temp_A(FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT,FMTRLPICKQTY,FMTRLPICKAMOUNT)\r\n                select FSUBREQBILLNO,FSUBREQENTRYSEQ,FSUBREQENTRYID,FMATERIALID,FUNITID,FACTUALQTY,FAMOUNT from T_SUB_PICKMTRLDATA pe \r\n                inner join T_SUB_PICKMTRL p on pe.FID=p.FID \r\n                inner join T_SUB_PICKMTRLDATA_A pea on pe.FENTRYID=pea.FENTRYID\r\n                where p.FDATE between '{obj}' and '{obj2}' \r\n                /*委外退料*/\r\n                insert into #temp_A(FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT,FMTRLREQTY,FMTRLREAMOUNT)\r\n                select FSUBREQBILLNO,FSUBREQENTRYSEQ,FSUBREQENTRYID,FMATERIALID,FUNITID,FQTY,FAMOUNT from T_SUB_RETURNMTRLENTRY re\r\n                inner join T_SUB_RETURNMTRL r on r.FID=re.FID \r\n                inner join T_SUB_RETURNMTRLENTRY_A rea on re.FENTRYID=rea.FENTRYID \r\n                where r.FDATE between '{obj}' and '{obj2}'\r\n                /*委外补料*/\r\n                insert into #temp_A(FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT,FMTRLEEDQTY,FMTRLEEAMOUNT)\r\n                select FSUBREQBILLNO,FSUBREQENTRYSEQ,FSUBREQENTRYID,FMATERIALID,FUNITID,FACTUALQTY,FAMOUNT from T_SUB_FEEDMTRLENTRY e\r\n                inner join T_SUB_FEEDMTRL eh on e.FID=eh.FID \r\n                inner join T_SUB_FEEDMTRLENTRY_Q eq on e.FENTRYID=eq.FENTRYID \r\n                where eh.FDATE between '{obj}' and '{obj2}' \r\n                insert into #temp(FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT,\r\n                FMTRLPICKQTY,FMTRLREQTY,FMTRLEEDQTY,FMTRLPICKAMOUNT,FMTRLREAMOUNT,FMTRLEEAMOUNT)\r\n                select s.FBILLNO,se.FSEQ,se.FENTRYID,FMTRLNUMBER,FMTRLUNIT,FMTRLPICKQTY,FMTRLREQTY,FMTRLEEDQTY,FMTRLPICKAMOUNT,FMTRLREAMOUNT,FMTRLEEAMOUNT from T_SUB_REQORDERENTRY se \r\n                inner join T_SUB_REQORDER s on se.FID=s.FID \r\n                left join (\r\n                    select FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT,\r\n                    sum(FMTRLPICKQTY) FMTRLPICKQTY,sum(FMTRLREQTY) FMTRLREQTY,sum(FMTRLEEDQTY) FMTRLEEDQTY,\r\n                    sum(FMTRLPICKAMOUNT) FMTRLPICKAMOUNT,sum(FMTRLREAMOUNT) FMTRLREAMOUNT,sum(FMTRLEEAMOUNT) FMTRLEEAMOUNT \r\n                    from #temp_A group by FSUBREQBILLNO,FSUBREQENTRYSEQ,FENTRYID,FMTRLNUMBER,FMTRLUNIT) b \r\n                    on b.FSUBREQBILLNO=s.FBILLNO and b.FSUBREQENTRYSEQ=se.FSEQ \r\n                    {text} \r\n                order by FSUBREQBILLNO, FSUBREQENTRYSEQ \r\n                /*组织信息*/\r\n                update a set a.FORGID=d.FNAME \r\n                from #temp a join\r\n                (select FBILLNO,FSEQ,FSUBORGID from T_SUB_REQORDERENTRY se \r\n                 inner join T_SUB_REQORDER s on se.FID=s.FID\r\n                 ) b on a.FSUBREQBILLNO=b.FBILLNO and FSUBREQENTRYSEQ=FSEQ \r\n                inner join T_ORG_Organizations_L d on b.FSUBORGID=d.FORGID and d.FLOCALEID=2052 \r\n                /*物料信息*/\r\n                update a set a.FMTRLNUMBER=m.FNUMBER,a.FMTRLNAME=ml.FNAME,a.FMTRLMODEL=ml.FSPECIFICATION,a.FMTRLUNIT=ul.FNAME \r\n                from #temp a \r\n                inner join T_BD_MATERIAL m on a.FMTRLNUMBER=m.FMATERIALID \r\n                inner join T_BD_MATERIAL_L ml on a.FMTRLNUMBER=ml.FMATERIALID and ml.FLOCALEID=2052 \r\n                inner join T_BD_UNIT_L ul on a.FMTRLUNIT=ul.FUNITID and ul.FLOCALEID=2052 \r\n                /*按订单订单号和行号取第一行*/\r\n                update #temp set FSUBREQBILLNO_a=FSUBREQBILLNO,FSUBREQENTRYSEQ_a=FSUBREQENTRYSEQ,FORGID_a=FORGID \r\n                where fidentity in (select fidentity from (select row_number() over(partition by FSUBREQBILLNO,FSUBREQENTRYSEQ order by Fidentity asc) as rk,\r\n                Fidentity from #temp) T where rk=1)\r\n                /*成品物料编码取订单订单，避免空值*/\r\n                update a set a.FSUBPRDNUMBER=b.FMATERIALID,FPRDUNIT=b.FUNITID \r\n                from #temp a join \r\n                (select s.FMATERIALID,sh.FBILLNO,s.FSEQ,s.FUNITID from T_SUB_REQORDERENTRY s inner join T_SUB_REQORDER sh on s.FID=sh.FID) b \r\n                on a.FSUBREQBILLNO_a=b.FBILLNO and a.FSUBREQENTRYSEQ_a=b.FSEQ\r\n                /*采购入库*/\r\n                update a set a.FPRDNUMBER=b.FMATERIALID,a.FPRDINSTOCKQTY=b.FQTY,a.FPRDIN_CLCOST=b.FCLCB,a.FPRDIN_JGCOST=b.FJGF,a.FPRDIN_SUMCOST=b.FZCB\r\n                from #temp a join \r\n                (select s.FBILLNO,se.FSEQ,ie.FMATERIALID,sum(FREALQTY) FQTY,sum(ief.FMATERIALCOSTS_LC) FCLCB,sum(ief.FPROCESSFEE_LC) FJGF,sum(ief.FCOSTAMOUNT_LC) FZCB from T_STK_INSTOCKENTRY ie \r\n                inner join T_STK_INSTOCK i on ie.FID=i.FID \r\n                inner join T_STK_INSTOCKENTRY_F ief on ief.FENTRYID=ie.FENTRYID \r\n                inner join T_PUR_POORDERENTRY_LK poelk on ie.FPOORDERENTRYID=poelk.FENTRYID and poelk.FSTABLENAME='T_SUB_REQORDERENTRY' \r\n                inner join T_SUB_REQORDERENTRY se on poelk.FSID=se.FENTRYID \r\n                inner join T_SUB_REQORDER s on s.FID=se.FID \r\n                where i.FDATE between '{obj}' and '{obj2}' \r\n                group by s.FBILLNO,se.FSEQ,ie.FMATERIALID,ie.FUNITID) b \r\n                on a.FSUBREQBILLNO_a=b.FBILLNO and a.FSUBREQENTRYSEQ_a=b.FSEQ \r\n                /*物料信息*/\r\n                update a set a.FSUBPRDNUMBER=m.FNUMBER,a.FPRDNAME=ml.FNAME,FPRDMODEL=ml.FSPECIFICATION,a.FPRDUNIT=ul.FNAME \r\n                from #temp a \r\n                inner join T_BD_MATERIAL m on a.FSUBPRDNUMBER=m.FMATERIALID \r\n                inner join T_BD_MATERIAL_L ml on a.FSUBPRDNUMBER=ml.FMATERIALID and ml.FLOCALEID=2052 \r\n                inner join T_BD_UNIT_L ul on a.FPRDUNIT=ul.FUNITID and ul.FLOCALEID=2052 \r\n                /*采购退料*/\r\n                update a set a.FPRDNUMBER=b.FMATERIALID,a.FPRDRESTOCKQTY=b.FQTY,a.FPRDRE_CLCOST=b.FCLCB,a.FPRDRE_JGCOST=b.FJGF,a.FPRDRE_SUMCOST=b.FZCB \r\n                from #temp a join \r\n                (select s.FBILLNO,se.FSEQ,pre.FMATERIALID,m.FNUMBER,sum(pre.FRMREALQTY) FQTY,sum(pref.FMATERIALCOSTS_LC) FCLCB,sum(pref.FPROCESSFEE_LC) FJGF,sum(pref.FCOSTAMOUNT_LC) FZCB \r\n                from T_PUR_MRBENTRY pre \r\n                inner join T_PUR_MRB pr on pre.FID=pr.FID \r\n                inner join T_PUR_MRBENTRY_F pref on pref.FENTRYID=pre.FENTRYID \r\n                inner join T_BD_MATERIAL m on pre.FMATERIALID=m.FMATERIALID \r\n                inner join T_PUR_POORDERENTRY_LK poelk on pre.FPOORDERENTRYID=poelk.FENTRYID and poelk.FSTABLENAME='T_SUB_REQORDERENTRY' \r\n                inner join T_SUB_REQORDERENTRY se on poelk.FSID=se.FENTRYID \r\n                inner join T_SUB_REQORDER s on s.FID=se.FID \r\n                where pr.FDATE between '{obj}' and '{obj2}' \r\n                group by s.FBILLNO,se.FSEQ,m.FNUMBER,pre.FMATERIALID) b \r\n                on a.FSUBREQBILLNO_a=b.FBILLNO and a.FSUBREQENTRYSEQ_a=b.FSEQ and a.FSUBPRDNUMBER=b.FNUMBER \r\n                insert into #temp(FORGID,FPRDNUMBER,FPRDINSTOCKQTY,FPRDRESTOCKQTY,\r\n                FPRDIN_CLCOST,FPRDRE_CLCOST,\r\n                FPRDIN_JGCOST,FPRDRE_JGCOST,\r\n                FPRDIN_SUMCOST,FPRDRE_SUMCOST,\r\n                FMTRLPICKQTY,FMTRLREQTY,FMTRLEEDQTY,FMTRLPICKAMOUNT,FMTRLREAMOUNT,FMTRLEEAMOUNT)\r\n                select '总合计','总合计',sum(FPRDINSTOCKQTY),sum(FPRDRESTOCKQTY),\r\n                sum(FPRDIN_CLCOST),sum(FPRDRE_CLCOST),\r\n                sum(FPRDIN_JGCOST),sum(FPRDRE_JGCOST),\r\n                sum(FPRDIN_SUMCOST),sum(FPRDRE_SUMCOST),\r\n                sum(FMTRLPICKQTY),sum(FMTRLREQTY),sum(FMTRLEEDQTY),sum(FMTRLPICKAMOUNT),sum(FMTRLREAMOUNT),sum(FMTRLEEAMOUNT)  \r\n                from #temp where FPRDNUMBER is not null or FMTRLNUMBER is not null\r\n                select row_number() over(order by FORGID) FIDENTITYID,2 FPrcision_AMT,6 FPrcision_QTY,\r\n                FORGID,\r\n                FSUBREQBILLNO,\r\n                FSUBREQENTRYSEQ,\r\n                FORGID_a,\r\n                FSUBREQBILLNO_a,\r\n                FSUBREQENTRYSEQ_a,\r\n                FSUBPRDNUMBER,\r\n                FPRDNAME,\r\n                FPRDMODEL,\r\n                FPRDUNIT,\r\n                FPRDINSTOCKQTY-FPRDRESTOCKQTY FInstockQty,\r\n                FPRDIN_CLCOST-FPRDRE_CLCOST FPrdCLCost,\r\n                FPRDIN_JGCOST-FPRDRE_JGCOST FPrdJGCost,\r\n                FPRDIN_SUMCOST-FPRDRE_SUMCOST FPrdSUMCost,\r\n                FMTRLNUMBER,\r\n                FMTRLNAME,\r\n                FMTRLMODEL,\r\n                FMTRLUNIT,\r\n                FMTRLPICKQTY-FMTRLREQTY+FMTRLEEDQTY FPickMtrlQty, \r\n                FMTRLPICKAMOUNT-FMTRLREAMOUNT+FMTRLEEAMOUNT FMtrlAmount \r\n                into {tableName} \r\n                from #temp \r\n                where FMTRLNUMBER is not null or FPRDNUMBER is not null \r\n                if exists(select * from tempdb..sysobjects where id=object_id('tempdb..#temp'))\r\n                   drop table #temp\r\n                if exists(select * from tempdb..sysobjects where id=object_id('tempdb..#temp_A'))\r\n                   drop table #temp_A";
			DBUtils.Execute(base.Context, strSQL);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00009528 File Offset: 0x00007728
		public override void CloseReport()
		{
			ITemporaryTableService service = ServiceHelper.GetService<ITemporaryTableService>();
			service.DeleteTemporaryTableName(base.Context);
		}
	}
}
