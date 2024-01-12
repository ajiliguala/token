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
	// Token: 0x02000019 RID: 25
	[Description("账册平衡表耗量")]
	[HotUpdate]
	public class BookBalance : SysReportBaseService
	{
		// Token: 0x060000B7 RID: 183 RVA: 0x00008724 File Offset: 0x00006924
		public override void Initialize()
		{
			base.Initialize();
			this.IsCreateTempTableByPlugin = true;
			base.ReportProperty.DecimalControlFieldList = new List<DecimalControlField>
			{
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlJKqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlJKqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FOEMInstockqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FOEMInstockqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlJZqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlJZqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlFCqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlFCqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlNXqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlNXqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FprdCKqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FprdCKqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdSalOStockqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FPrdSalOStockqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlInvqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FmtrlInvqty_xps",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FprdInventoryqty",
					DecimalControlFieldName = "FPrcision"
				},
				new DecimalControlField
				{
					ByDecimalControlFieldName = "FprdInventoryqty_xps",
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
				}
			};
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00008A6C File Offset: 0x00006C6C
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			ReportHeader reportHeader = new ReportHeader();
			reportHeader.AddChild("Fprdnumber", new LocaleValue("成品料号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fprdxps", new LocaleValue("成品大张对应小片数"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("Fbooknumber", new LocaleValue("加工贸易账册编号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fmtrlnumber", new LocaleValue("料件料号"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fmtrlxps", new LocaleValue("料件大张对应小片数"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("Fmtrlname", new LocaleValue("料件名称"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("Fmodel", new LocaleValue("料件规格"), SqlStorageType.Sqlvarchar, true);
			reportHeader.AddChild("FmtrlJKqty", new LocaleValue("料件进口数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlJKqty_xps", new LocaleValue("料件进口数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FOEMInstockqty", new LocaleValue("料件入库数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FOEMInstockqty_xps", new LocaleValue("料件入库数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlJZqty", new LocaleValue("余料结转数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlJZqty_xps", new LocaleValue("余料结转数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlFCqty", new LocaleValue("料件复出数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlFCqty_xps", new LocaleValue("料件复出数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlNXqty", new LocaleValue("料件内销数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlNXqty_xps", new LocaleValue("料件内销数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FprdCKqty", new LocaleValue("成品出口数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FprdCKqty_xps", new LocaleValue("成品出口数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdSalOStockqty", new LocaleValue("成品出库数量"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FPrdSalOStockqty_xps", new LocaleValue("成品出库数量(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlInvqty", new LocaleValue("料件库存"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FmtrlInvqty_xps", new LocaleValue("料件库存(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FprdInventoryqty", new LocaleValue("成品库存"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FprdInventoryqty_xps", new LocaleValue("成品库存(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FBGCY", new LocaleValue("报关差异"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FBGCY_xps", new LocaleValue("报关差异(小片)"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FRemQty", new LocaleValue("理论结余库存"), SqlStorageType.SqlDecimal, true);
			reportHeader.AddChild("FRemQty_xps", new LocaleValue("理论结余库存(小片)"), SqlStorageType.SqlDecimal, true);
			return reportHeader;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00008D6C File Offset: 0x00006F6C
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			object obj = customFilter["Fbooknumber"];
			object obj2 = customFilter["Fprdnumber"];
			object obj3 = customFilter["Fmtrlnumber"];
			string strSQL = string.Format("/*dialect*/\r\n            create table #TempBookBal(\r\n            Fidentity int identity(1,1),\r\n            FPrcision int not null default 5,\r\n            Fprdnumber nvarchar(50) not null,\r\n            Fbooknumber nvarchar(50) not null,\r\n            Fmtrlnumber nvarchar(50) not null,\r\n            Fmtrlname nvarchar(250),\r\n            Fmodel nvarchar(510),\r\n            Fprdxps decimal(23,10) not null default 0,\r\n            Fmtrlxps decimal(23,10) not null default 0,\r\n            FmtrlJKqty decimal(23,10) not null default 0,\r\n            FOEMInstockqty decimal(23,10) not null default 0,\r\n            FmtrlJZqty decimal(23,10) not null default 0,\r\n            FmtrlFCqty decimal(23,10) not null default 0,\r\n            FmtrlNXqty decimal(23,10) not null default 0,\r\n            FprdCKqty decimal(23,10) not null default 0,\r\n            FPrdSalOStockqty decimal(23,10) not null default 0,\r\n            FmtrlInvqty decimal(23,10) not null default 0,\r\n            FprdInventoryqty decimal(23,10) not null default 0,\r\n            )\r\n            insert into #TempBookBal(Fprdnumber,Fbooknumber,Fmtrlnumber,Fmtrlname,Fmodel,Fprdxps,Fmtrlxps)\r\n            select b.FNUMBER,ah.FNUMBER,c.FNUMBER,cL.FNAME,cL.FSPECIFICATION,b.F_DZDYXPS,c.F_DZDYXPS from \r\n            PIKU_t_Cust_Entry100001 a inner join PIKU_t_Cust100013 ah on a.fid=ah.fid \r\n            inner join T_BD_MATERIAL b on a.F_PRDNO=b.FMATERIALID \r\n            inner join T_BD_MATERIAL c on a.F_ROWMTRLNO=c.FMATERIALID \r\n            inner join T_BD_MATERIAL_L cL on a.F_ROWMTRLNO=cL.FMATERIALID and FLOCALEID=2052 \r\n            where ah.FNUMBER like '%{0}%' and c.FNUMBER like '%{1}%' and b.FNUMBER like '%{2}%'\r\n\r\n\r\n            /*料件进口数量   进口报关单数量*/\r\n            update a set a.FmtrlJKqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER\r\n            ) b on a.Fmtrlnumber=b.Fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*料件入库数量*/\r\n            update a set a.FOEMInstockqty=b.qty\r\n            from #TempBookBal a \r\n            inner join \r\n            (select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(FQTY) qty from T_STK_OEMINSTOCKENTRY oem\r\n            inner join T_BD_MATERIAL m on oem.FMATERIALID=m.FMATERIALID\r\n            inner join T_STK_OEMINSTOCK oemh on oem.FID=oemh.FID \r\n            inner join PIKU_t_Cust100013 zc on oemh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            union all\r\n\r\n            select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(FREALQTY) qty from t_stk_instockentry a \r\n            inner join t_stk_instock b on a.FID=b.FID \r\n            inner join T_BD_MATERIAL m on a.FMATERIALID=m.FMATERIALID \r\n            inner join PIKU_t_Cust100013 zc on b.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fmtrlnumber=b.Fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*余料结转   贸易方式=455805*/\r\n            update a set a.FmtrlJZqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID where F_TRADEMMODE in(455805,3682591) group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fmtrlnumber=b.Fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n\r\n            /*料件复出\t贸易方式=455807*/\r\n            update a set a.FmtrlFCqty=b.qty \r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTY) qty from CX_CKENTRY ck \r\n            inner join T_BD_MATERIAL m on ck.F_MATERIALID=m.FMATERIALID \r\n            inner join CX_EXPORT ckh on ck.FID=ckh.FID \r\n            inner join PIKU_t_Cust100013 zc on ckh.F_JGMYZCNO=zc.FID where F_TRADEMMODE=455807 group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fmtrlnumber=b.Fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n            /*料件内销   贸易方式=(455801 245 来料料件内销)和(4812525 0644 进料料件内销) */\r\n            update a set a.FmtrlNXqty=b.qty\r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER Fmtrlnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTYS) qty from CX_JKBGDENTRY jk \r\n            inner join T_BD_MATERIAL m on jk.F_RTRL=m.FMATERIALID \r\n            inner join CX_IMDECLARATION jkh on jk.FID=jkh.FID \r\n            inner join PIKU_t_Cust100013 zc on jkh.F_JGMYZCBH=zc.FID where F_TRADEMMODE in (455801,4812525) group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fmtrlnumber=b.Fmtrlnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n            /*成品出口数量*/\r\n            update a set a.FprdCKqty=b.qty \r\n            from #TempBookBal a \r\n            inner join\r\n            (select m.FNUMBER FPrdnumber,zc.FNUMBER F_JGMYZCBH,SUM(F_QTY) qty from CX_CKENTRY ck \r\n            inner join T_BD_MATERIAL m on ck.F_MATERIALID=m.FMATERIALID \r\n            inner join CX_EXPORT ckh on ck.FID=ckh.FID \r\n            inner join PIKU_t_Cust100013 zc on ckh.F_JGMYZCNO=zc.FID\r\n\t\t\twhere F_TRADEMMODE not in(4255993,4566933)\r\n\t\t\tgroup by m.FNUMBER,zc.FNUMBER\r\n            ) b on a.Fprdnumber=b.FPrdnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n            /*成品出库数量*/\r\n            update a set a.FPrdSalOStockqty=b.qty\r\n            from #TempBookBal a \r\n            inner join \r\n            (select m.FNUMBER FPrdnumber,zc.FNUMBER F_JGMYZCBH,SUM(os.FRealQty) qty from T_SAL_OUTSTOCKENTRY os\r\n            inner join T_BD_MATERIAL m on os.FMATERIALID=m.FMATERIALID\r\n            inner join T_SAL_OUTSTOCK osh on os.FID=osh.FID \r\n            inner join PIKU_t_Cust100013 zc on osh.F_JGMYZCBH=zc.FID group by m.FNUMBER,zc.FNUMBER \r\n            ) b on a.Fprdnumber=b.FPrdnumber and b.F_JGMYZCBH=a.Fbooknumber\r\n\r\n            update a set a.FmtrlInvqty=b.qty \r\n            from #TempBookBal a inner join \r\n            (select m.FNUMBER fnumber,SUM(inv.FBaseQty) qty from T_STK_INVENTORY inv \r\n            inner join T_BD_MATERIAL m on inv.FMATERIALID=m.FMATERIALID \r\n            where inv.FSTOCKID in (103741,103748,103780,103782,103798,103814,103817,704165,704175,103758,103783,103800,103801,103816,\r\n            409639,409701,673741,704168,878784,880569,103838,409707,539559,539560,704176,774030)\r\n            group by m.FNUMBER) \r\n            b on a.Fmtrlnumber=b.fnumber\r\n\r\n            /*成品数量  三部五部成品仓库*/\r\n            update a set a.FprdInventoryqty=b.qty \r\n            from #TempBookBal a inner join \r\n            (select m.FNUMBER fnumber,SUM(inv.FBaseQty) qty from T_STK_INVENTORY inv \r\n            inner join T_BD_MATERIAL m on inv.FMATERIALID=m.FMATERIALID \r\n            where inv.FSTOCKID in (103785,103823,409688,103760,103762) group by m.FNUMBER) \r\n            b on a.Fprdnumber=b.fnumber\r\n\r\n            select row_number() over(order by Fprdnumber,Fbooknumber) FIDENTITYID,FPrcision,Fprdnumber,Fprdxps,Fbooknumber,Fmtrlnumber,Fmtrlxps,Fmtrlname,Fmodel,\r\n            FmtrlJKqty-FmtrlNXqty FmtrlJKqty,FmtrlJKqty*Fmtrlxps-FmtrlNXqty*Fmtrlxps FmtrlJKqty_xps,\r\n            FOEMInstockqty,FOEMInstockqty*Fmtrlxps FOEMInstockqty_xps,\r\n            FmtrlJZqty,FmtrlJZqty*Fmtrlxps FmtrlJZqty_xps,\r\n            FmtrlFCqty,FmtrlFCqty*Fmtrlxps FmtrlFCqty_xps,\r\n            FmtrlNXqty,FmtrlNXqty*Fmtrlxps FmtrlNXqty_xps,\r\n            FprdCKqty+FmtrlNXqty FprdCKqty,FprdCKqty*Fprdxps+FmtrlNXqty*Fmtrlxps FprdCKqty_xps,\r\n            FPrdSalOStockqty,FPrdSalOStockqty*Fprdxps FPrdSalOStockqty_xps,\r\n            FmtrlInvqty,FmtrlInvqty*Fmtrlxps FmtrlInvqty_xps,\r\n            FprdInventoryqty,FprdInventoryqty*Fprdxps FprdInventoryqty_xps,\r\n            FmtrlJKqty-FmtrlFCqty-FmtrlNXqty-FprdCKqty FBGCY,\r\n            FmtrlJKqty*Fmtrlxps-FmtrlFCqty*Fmtrlxps-FmtrlNXqty*Fmtrlxps-FprdCKqty*Fprdxps FBGCY_xps,\r\n            FOEMInstockqty+FmtrlJZqty-FmtrlFCqty-FmtrlNXqty-FPrdSalOStockqty FRemQty,\r\n            FOEMInstockqty*Fmtrlxps+FmtrlJZqty*Fmtrlxps-FmtrlFCqty*Fmtrlxps-FmtrlNXqty*Fmtrlxps-FPrdSalOStockqty*Fprdxps FRemQty_xps \r\n            into {3} from #TempBookBal \r\n\r\n            if exists(select * from tempdb..sysobjects where id=object_id('tempdb..#TempBookBal'))\r\n            drop table #TempBookBal", new object[]
			{
				obj,
				obj3,
				obj2,
				tableName
			});
			DBUtils.Execute(base.Context, strSQL);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00008DDC File Offset: 0x00006FDC
		public override void CloseReport()
		{
			ITemporaryTableService service = ServiceHelper.GetService<ITemporaryTableService>();
			service.DeleteTemporaryTableName(base.Context);
		}
	}
}
