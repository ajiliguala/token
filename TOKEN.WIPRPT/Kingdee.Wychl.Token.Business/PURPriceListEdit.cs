using System;
using System.Data;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200001F RID: 31
	public class PURPriceListEdit : AbstractBillPlugIn
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00009ABC File Offset: 0x00007CBC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			bool flag = e.Field.Key.ToUpper().Equals("FAUXPROPID");
			if (flag)
			{
				this.getHistoryInfo(e.Row);
			}
			bool flag2 = e.Field.Key.ToUpper().Equals("FMATERIALID");
			if (flag2)
			{
				this.getHistoryInfo(e.Row);
			}
			bool flag3 = e.Field.Key.ToUpper().Equals("FSUPPLIERID") || e.Field.Key.ToUpper().Equals("FPRICETYPE");
			if (flag3)
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FPriceListEntry");
				for (int i = 0; i < entryRowCount; i++)
				{
					this.getHistoryInfo(i);
				}
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00009BA4 File Offset: 0x00007DA4
		private void getHistoryInfo(int row)
		{
			bool flag = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FMaterialId", row));
			if (!flag)
			{
				bool flag2 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FSUPPLIERID"));
				if (!flag2)
				{
					bool flag3 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FPriceType"));
					if (!flag3)
					{
						DynamicObject dynamicObject = this.Model.GetValue("FMaterialId", row) as DynamicObject;
						DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialAuxPty"] as DynamicObjectCollection;
						bool flag4 = false;
						bool flag5 = dynamicObjectCollection.Count > 0;
						if (flag5)
						{
							foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
							{
								bool flag6 = (bool)dynamicObject2[1];
								if (flag6)
								{
									flag4 = true;
								}
							}
						}
						string text = "0";
						bool flag7 = flag4;
						if (flag7)
						{
							bool flag8 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FAuxPropId", row));
							if (flag8)
							{
								return;
							}
							DynamicObject dynamicObject3 = this.Model.GetValue("FAuxPropId", row) as DynamicObject;
							StringBuilder stringBuilder = new StringBuilder("select * from T_BD_FLEXSITEMDETAILV where 1=1");
							DynamicObject dynamicObject4 = this.Model.GetValue("FAuxPropId", row) as DynamicObject;
							foreach (DynamicObject dynamicObject5 in dynamicObjectCollection)
							{
								string text2 = "F" + dynamicObject5["AuxPropertyId_Id"].ToString();
								stringBuilder.Append(string.Format(" and {0}='{1}'", "F" + text2, dynamicObject4[text2 + "_id"]));
							}
							DynamicObjectCollection dynamicObjectCollection2 = DBServiceHelper.ExecuteDynamicObject(base.Context, stringBuilder.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
							bool flag9 = dynamicObjectCollection2.Count > 0;
							if (flag9)
							{
								text = dynamicObjectCollection2[0]["fid"].ToString();
							}
						}
						string text3 = this.Model.GetValue("FPriceType").ToString();
						string text4 = (this.Model.GetValue("FSUPPLIERID") as DynamicObject)["id"].ToString();
						string text5 = string.Format("select  top 1 FPRICE,F_FIXLEADTIME,F_EXPPERIOD,F_MINPOQTYNEW,F_INCREASEQTYNEW \r\n                                       from t_PUR_PriceListENTRY A\r\n                                       inner join t_PUR_PriceList B on A.FID=B.FID where B.FDOCUMENTSTATUS='C' and B.FFORBIDSTATUS='A'\r\n                                       and A.FMATERIALID={0} and B.FSUPPLIERID={1} and A.FEFFECTIVEDATE<'{2}' and FAUXPROPID={3}\r\n                                       and B.FPriceType='{4}' order by A.FEFFECTIVEDATE desc", new object[]
						{
							dynamicObject["id"],
							text4,
							DateTime.Now,
							text,
							text3
						});
						DynamicObjectCollection dynamicObjectCollection3 = DBServiceHelper.ExecuteDynamicObject(base.Context, text5, null, null, CommandType.Text, Array.Empty<SqlParam>());
						bool flag10 = dynamicObjectCollection3.Count > 0;
						if (flag10)
						{
							this.Model.SetValue("F_SOURCEPRICE", dynamicObjectCollection3[0]["FPRICE"], row);
							this.Model.SetValue("F_FIXLEADTIME", dynamicObjectCollection3[0]["F_FIXLEADTIME"], row);
							this.Model.SetValue("F_EXPPERIOD", dynamicObjectCollection3[0]["F_EXPPERIOD"], row);
							this.Model.SetValue("F_MINPOQTYNEW", dynamicObjectCollection3[0]["F_MINPOQTYNEW"], row);
							this.Model.SetValue("F_INCREASEQTYNEW", dynamicObjectCollection3[0]["F_INCREASEQTYNEW"], row);
							base.View.InvokeFieldUpdateService("F_SOURCEPRICE", row);
						}
						else
						{
							this.Model.SetValue("F_SOURCEPRICE", 0, row);
							this.Model.SetValue("F_FIXLEADTIME", 0, row);
							this.Model.SetValue("F_EXPPERIOD", 0, row);
							this.Model.SetValue("F_MINPOQTYNEW", 0, row);
							this.Model.SetValue("F_INCREASEQTYNEW", 0, row);
							base.View.InvokeFieldUpdateService("F_SOURCEPRICE", row);
						}
					}
				}
			}
		}
	}
}
