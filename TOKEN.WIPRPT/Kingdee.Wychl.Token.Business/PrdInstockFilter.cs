using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000016 RID: 22
	[Description("生产入库单联产品过滤插件")]
	public class PrdInstockFilter : AbstractBillPlugIn
	{
		// Token: 0x06000044 RID: 68 RVA: 0x000059B4 File Offset: 0x00003BB4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.FieldKey, "FMaterialId");
			if (flag)
			{
				bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_KING_AUTO")) || this.Model.GetValue("F_KING_AUTO").ToString().Equals("0");
				if (flag2)
				{
					bool flag3 = this.Model.GetValue("FSrcEntryId", e.Row).ToString().Equals("0");
					if (flag3)
					{
						bool flag4 = this.Model.GetValue("FMaterialId", e.Row - 1) != null;
						if (flag4)
						{
							string materialId = (this.Model.GetValue("FMaterialId", e.Row - 1) as DynamicObject)["id"].ToString();
							this.getMaterial(materialId);
							bool flag5 = this.materialList.Count > 0;
							if (flag5)
							{
								e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " and fmaterialid in (" + string.Join(",", this.materialList) + ")";
							}
							else
							{
								base.View.ShowMessage("没有找到对应的联产品", 0);
								e.ListFilterParameter.Filter = "1=2";
							}
						}
						else
						{
							base.View.ShowMessage("没有找到对应的主产品", 0);
							e.ListFilterParameter.Filter = "1=2";
						}
					}
				}
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00005B48 File Offset: 0x00003D48
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BaseDataFieldKey, "FMaterialId");
			if (flag)
			{
				bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_KING_AUTO")) || this.Model.GetValue("F_KING_AUTO").ToString().Equals("0");
				if (flag2)
				{
					bool flag3 = this.Model.GetValue("FSrcEntryId", e.Row).ToString().Equals("0");
					if (flag3)
					{
						bool flag4 = this.Model.GetValue("FMaterialId", e.Row - 1) != null;
						if (flag4)
						{
							string materialId = (this.Model.GetValue("FMaterialId", e.Row - 1) as DynamicObject)["id"].ToString();
							this.getMaterial(materialId);
							bool flag5 = this.materialList.Count > 0;
							if (flag5)
							{
								e.Filter = e.Filter + " and fmaterialid in (" + string.Join(",", this.materialList) + ")";
							}
							else
							{
								base.View.ShowMessage("没有找到对应的联产品", 0);
								e.Filter = "1=2";
							}
						}
						else
						{
							base.View.ShowMessage("没有找到对应的主产品", 0);
							e.Filter = "1=2";
						}
					}
				}
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00005CC8 File Offset: 0x00003EC8
		private void getMaterial(string materialId)
		{
			string text = "/*dialect*/select B.FMATERIALID from T_ENG_BOM A inner join T_ENG_BOMCHILD B ON A.FID=B.FID                           \r\n                              inner join T_ENG_BOMCHILD_A C on C.FENTRYID=B.FENTRYID                            \r\n                              where C.FISSKIP=1  and  A.FDOCUMENTSTATUS='C' and  A.FMATERIALID=" + materialId;
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text2 = dynamicObject["FMATERIALID"].ToString();
					this.materialList.Add(text2);
					this.getMaterial(text2);
				}
			}
		}

		// Token: 0x04000005 RID: 5
		private List<string> materialList = new List<string>();
	}
}
