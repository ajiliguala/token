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
	// Token: 0x0200001A RID: 26
	[Description("账册平衡表余量")]
	[HotUpdate]
	public class BookBalanceRem : SysReportBaseService
	{
		// Token: 0x060000BC RID: 188 RVA: 0x00008E08 File Offset: 0x00007008
		public override void Initialize()
		{
			base.Initialize();
			this.IsCreateTempTableByPlugin = true;
			base.ReportProperty.DecimalControlFieldList = new List<DecimalControlField>
			{
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlJKqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FprdCKqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FOEMInstockqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdSalOStockqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FBGCY",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FBGCY_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FRemQty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FRemQty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "Frate",
					DecimalControlFieldName = "FPricision2"
				}
			};
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00008F7C File Offset: 0x0000717C
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("Fbooknumber", new LocaleValue("加工贸易账册编号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("fmtrlnumber", new LocaleValue("料件料号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fmtrlname", new LocaleValue("料件名称"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fmodel", new LocaleValue("料件规格"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FmtrlJKqty_xps", new LocaleValue("料件进口数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FprdCKqty_xps", new LocaleValue("成品出口数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FOEMInstockqty_xps", new LocaleValue("料件入库数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdSalOStockqty_xps", new LocaleValue("成品出库数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FBGCY", new LocaleValue("报关差异"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FBGCY_xps", new LocaleValue("报关差异(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FRemQty", new LocaleValue("理论结余库存"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FRemQty_xps", new LocaleValue("理论结余库存(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("Frate", new LocaleValue("完成率(%)"), SqlStorageType.SqlDecimal, true);
			return reportHeader;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x000090E8 File Offset: 0x000072E8
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			object arg = customFilter["Fbooknumber"];
			object arg2 = customFilter["Fmtrlnumber"];
			string strSQL = string.Format("/*dialect*/\r\n            create table #TempBookBal(\r\n            Fidentity int identity(1,1),\r\n            FPrcision int not null default 5,\r\n            Fprdnumber nvarchar(50) not null,\r\n            Fbooknumber nvarchar(50) not null,\r\n            fmtrlnumber nvarchar(50) not null,\r\n            Fmtrlname nvarchar(250),\r\n            Fmodel nvarchar(510),\r\n            Fprdxps decimal(23,10) not null default 0,\r\n            Fmtrlxps decimal(23,10) not null default 0,\r\n            FmtrlJKqty decimal(23,10) not null default 0,\r\n            FOEMInstockqty decimal(23,10) not null default 0,\r\n            FmtrlJZqty decimal(23,10) not null default 0,\r\n            FmtrlFCqty decimal(23,10) not null default 0,\r\n            FmtrlNXqty decimal(23,10) not null default 0,\r\n            FprdCKqty decimal(23,10) not null default 0,\r\n            FPrdSalOStockqty decimal(23,10) not null default 0,\r\n            FmtrlInvqty decimal(23,10) not null default 0,\r\n            FprdInventoryqty decimal(23,10) not null default 0,\r\n            )\r\n\r\n            insert into #TempBookBal(Fprdnumber,Fbooknumber,Fmtrlnumber,Fmtrlname,Fmodel,Fprdxps,Fmtrlxps)\r\n            select b.FNUMBER,ah.FNUMBER,c.FNUMBER,cL.FNAME,cL.FSPECIFICATION,b.F_DZDYXPS,c.F_DZDYXPS from \r\n            PIKU_t_Cust_Entry100001 a inner join PIKU_t_Cust100013 ah on a.fid=ah.fid \r\n            inner join T_BD_MATERIAL b on a.F_PRDNO=b.FMATERIALID \r\n            inner join T_BD_MATERIAL c on a.F_ROWMTRLNO=c.FMATERIALID \r\n            inner join T_BD_MATERIAL_L cL on a.F_ROWMTRLNO=cL.FMATERIALID and FLOCALEID=2052 \r\n            where ah.FNUMBER like '%{0}%' and c.FNUMBER like '%{1}%' \r\n\r\n            /*料件进口数量   进口报关单数量*/\r\n            update a set a.FmtrlJKqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER\r\n            ) b on a.fmtrlnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*料件入库数量*/\r\n            update a set a.FOEMInstockqty=b.qty\r\n            from #TempBookBal a \r\n            inner join \r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(FQTY) qty from T_STK_OEMINSTOCKENTRY oem\r\n            inner join T_BD_MATERIAL m on oem.FMATERIALID=m.FMATERIALID\r\n            inner join T_STK_OEMINSTOCK oemh on oem.FID=oemh.FID \r\n            inner join PIKU_t_Cust100013 zc on oemh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            union all\r\n\r\n            select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(FREALQTY) qty from t_stk_instockentry a \r\n            inner join t_stk_instock b on a.FID=b.FID \r\n            inner join T_BD_MATERIAL m on a.FMATERIALID=m.FMATERIALID \r\n            inner join PIKU_t_Cust100013 zc on b.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.fmtrlnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*余料结转   贸易方式=455805*/\r\n            update a set a.FmtrlJZqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID where F_TRADEMMODE in(455805,3682591) group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.fmtrlnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*料件复出\t贸易方式=455807*/\r\n            update a set a.FmtrlFCqty=b.qty \r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTY) qty from CX_CKENTRY ck \r\n            inner join T_BD_MATERIAL m on ck.F_MATERIALID=m.FMATERIALID \r\n            inner join CX_EXPORT ckh on ck.FID=ckh.FID \r\n            inner join PIKU_t_Cust100013 zc on ckh.F_JGMYZCNO=zc.FID where F_TRADEMMODE=455807 group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.fmtrlnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n            /*料件内销   贸易方式=(455801 245 来料料件内销)和(4812525 0644 进料料件内销)*/\r\n            update a set a.FmtrlNXqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID where F_TRADEMMODE in (455801,4812525) group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.fmtrlnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n            /*成品出口数量*/\r\n            update a set a.FprdCKqty=b.qty \r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTY) qty from CX_CKENTRY ck \r\n            inner join T_BD_MATERIAL m on ck.F_MATERIALID=m.FMATERIALID \r\n            inner join CX_EXPORT ckh on ck.FID=ckh.FID \r\n            inner join PIKU_t_Cust100013 zc on ckh.F_JGMYZCNO=zc.FID\r\n\t\t\twhere F_TRADEMMODE not in(4255993,4566933)\r\n\t\t\tgroup by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fprdnumber=b.fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n            /*成品出库数量*/\r\n            update a set a.FPrdSalOStockqty=b.qty\r\n            from #TempBookBal a \r\n            inner join \r\n            (select m.FNUMBER FPrdnumber,zc.FNUMBER F_JGMYZCBH,SUM(os.FRealQty) qty from T_SAL_OUTSTOCKENTRY os\r\n            inner join T_BD_MATERIAL m on os.FMATERIALID=m.FMATERIALID\r\n            inner join T_SAL_OUTSTOCK osh on os.FID=osh.FID \r\n            inner join PIKU_t_Cust100013 zc on osh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fprdnumber=b.FPrdnumber and b.F_JGMYZCBH=a.Fbooknumber \r\n\r\n\r\n            select row_number() over(order by Fbooknumber,fmtrlnumber) FIDENTITYID,FPrcision,2 FPricision2,\r\n            Fbooknumber,fmtrlnumber,Fmtrlname,Fmodel,\r\n            avg(FmtrlJKqty*Fmtrlxps)-avg(FmtrlNXqty*Fmtrlxps) FmtrlJKqty_xps,\r\n            sum(FprdCKqty*Fprdxps)+avg(FmtrlNXqty*Fmtrlxps) FprdCKqty_xps,\r\n            avg(FOEMInstockqty*Fmtrlxps) FOEMInstockqty_xps,\r\n            sum(FPrdSalOStockqty*Fprdxps) FPrdSalOStockqty_xps,\r\n            avg(FmtrlJKqty)-avg(FmtrlFCqty)-avg(FmtrlNXqty)-sum(FprdCKqty) FBGCY,\r\n            avg(FmtrlJKqty*Fmtrlxps)-avg(FmtrlFCqty*Fmtrlxps)-avg(FmtrlNXqty*Fmtrlxps)-sum(FprdCKqty*Fprdxps) FBGCY_xps,\r\n            avg(FOEMInstockqty)+avg(FmtrlJZqty)-avg(FmtrlFCqty)-avg(FmtrlNXqty)-sum(FPrdSalOStockqty) FRemQty,\r\n            avg(FOEMInstockqty*Fmtrlxps)+avg(FmtrlJZqty*Fmtrlxps)-avg(FmtrlFCqty*Fmtrlxps)-avg(FmtrlNXqty*Fmtrlxps)-sum(FPrdSalOStockqty*Fprdxps) FRemQty_xps,\r\n            case (avg(FmtrlJKqty*Fmtrlxps)-avg(FmtrlNXqty*Fmtrlxps)) when 0 then 0 \r\n            else (sum(FprdCKqty*Fprdxps)+avg(FmtrlNXqty*Fmtrlxps))/(avg(FmtrlJKqty*Fmtrlxps)-avg(FmtrlNXqty*Fmtrlxps))*100 end Frate \r\n\r\n            into {2} \r\n            from #TempBookBal group by Fbooknumber,fmtrlnumber,FPrcision,Fmtrlname,Fmodel\r\n\r\n            if exists(select * from tempdb..sysobjects where id=object_id('tempdb..#TempBookBal'))\r\n            drop table #TempBookBal", arg, arg2, tableName);
			DBUtils.Execute(base.Context, strSQL);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00009138 File Offset: 0x00007338
		public override void CloseReport()
		{
			ITemporaryTableService service = ServiceHelper.GetService<ITemporaryTableService>();
			service.DeleteTemporaryTableName(base.Context);
		}
	}
}
