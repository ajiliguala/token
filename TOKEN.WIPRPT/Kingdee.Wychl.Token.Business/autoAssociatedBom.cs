using System;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000003 RID: 3
	[HotUpdate]
	public class autoAssociatedBom : AbstractBillPlugIn
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002548 File Offset: 0x00000748
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BarItemKey, "getBom");
			if (flag)
			{
				bool flag2 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FMATERIALID"));
				if (!flag2)
				{
					bool flag3 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FSUPPLYORGID"));
					if (!flag3)
					{
						string arg = (this.Model.GetValue("FMATERIALID") as DynamicObject)["id"].ToString();
						DynamicObject dynamicObject = this.Model.GetValue("FSUPPLYORGID") as DynamicObject;
						string text = dynamicObject["number"].ToString();
						bool flag4 = text.Equals("040") || text.Equals("041") || text.Equals("042");
						if (flag4)
						{
							this.Model.SetValue("FBomId", "0");
							base.View.InvokeFieldUpdateService("FBomId", -1);
							base.View.UpdateView("FBomId");
						}
						else
						{
							string arg2 = dynamicObject["id"].ToString();
							string text2 = string.Format("/*dialect*/select bom.FID,bom.FNUMBER from t_bd_material tbm \r\n                                            inner join t_bd_material new on new.fmasterid = tbm.fmasterid and new.fuseorgid = {0}\r\n                                            inner join t_eng_bom bom on bom.fmaterialid = new.fmaterialid\r\n                                            where tbm.fmaterialid = {1} and bom.FFORBIDSTATUS='A' and bom.FDOCUMENTSTATUS='C'\r\n                                            order by bom.fapprovedate desc", arg2, arg);
							DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text2, null, null, CommandType.Text, Array.Empty<SqlParam>());
							bool flag5 = dynamicObjectCollection.Count > 0;
							if (flag5)
							{
								this.Model.SetValue("FBomId", dynamicObjectCollection[0]["FID"].ToString());
								base.View.InvokeFieldUpdateService("FBomId", -1);
								base.View.UpdateView("FBomId");
							}
							else
							{
								this.Model.SetValue("FBomId", "0");
								base.View.InvokeFieldUpdateService("FBomId", -1);
								base.View.UpdateView("FBomId");
							}
						}
					}
				}
			}
		}
	}
}
