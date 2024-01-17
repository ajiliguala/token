using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000013 RID: 19
	[HotUpdate]
	[Description("超期处理界面")]
	public class overdueManage : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000038 RID: 56 RVA: 0x00004B6C File Offset: 0x00002D6C
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BarItemKey, "TBSUBMIT");
			if (flag)
			{
				EntryGrid control = this.View.GetControl<EntryGrid>("FEntity");
				int[] selectedRows = control.GetSelectedRows();
				int num = selectedRows.Length;
				bool flag2 = num == 0;
				if (flag2)
				{
					this.View.ShowMessage("请至少选择一条记录进行操作", 0);
				}
				int num2 = 1;
				List<int> list = new List<int>();
				bool flag3 = false;
				List<string> list2 = new List<string>();
				foreach (int num3 in selectedRows)
				{
					bool flag4 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FUSER", num3));
					if (flag4)
					{
						list.Add(num3);
					}
					else
					{
						bool flag5 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FDEPT", num3));
						if (flag5)
						{
							list.Add(num3);
						}
						else
						{
							bool flag6 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("F_PCQE_Date", num3));
							if (flag6)
							{
								list.Add(num3);
							}
							else
							{
								DynamicObject dynamicObject = this.Model.GetValue("F_ORGID", num3) as DynamicObject;
								string text = dynamicObject["id"].ToString();
								string text2 = (this.Model.GetValue("F_MATERIAL", num3) as DynamicObject)["id"].ToString();
								string text3 = (this.Model.GetValue("F_STOCK", num3) as DynamicObject)["id"].ToString();
								string text4 = (this.Model.GetValue("F_LOT", num3) as DynamicObject)["id"].ToString();
								string text5 = string.Concat(new string[]
								{
									"CQCL",
									dynamicObject["number"].ToString(),
									"-",
									text3,
									"-",
									text2,
									"-",
									text4
								});
								List<string> list3 = list2;
								string format = "('{0}','A','{1}',{2},{3},{4},{5},'{6}','{7}',{8},{9},{10},'1','{11}','{12}')";
								object[] array2 = new object[13];
								array2[0] = this.Model.GetValue("F_ID", num3);
								array2[1] = text5;
								array2[2] = text;
								array2[3] = text3;
								array2[4] = text2;
								array2[5] = text4;
								array2[6] = this.Model.GetValue("F_PRODUCEDATE", num3);
								array2[7] = this.Model.GetValue("F_EXPIRYDATE", num3);
								array2[8] = this.Model.GetValue("F_QTY", num3);
								int num4 = 9;
								DynamicObject dynamicObject2 = this.Model.GetValue("FDEPT", num3) as DynamicObject;
								array2[num4] = ((dynamicObject2 != null) ? dynamicObject2["id"].ToString() : null);
								array2[10] = (this.Model.GetValue("FUSER", num3) as DynamicObject)["id"].ToString();
								array2[11] = Convert.ToDateTime(this.Model.GetValue("F_PCQE_Date", num3));
								array2[12] = DateTime.Now;
								list3.Add(string.Format(format, array2));
								num2++;
								flag3 = true;
							}
						}
					}
				}
				bool flag7 = flag3;
				if (flag7)
				{
					string text6 = "/*dialect*/insert into T_KING_INVENTORY(FID,FDOCUMENTSTATUS,FBILLNO,F_PCQE_ORGID,F_PCQE_STOCKID,F_PCQE_MATERIAL,F_PCQE_LOT,\r\n                                   F_PCQE_PRODUCEDATE,F_PCQE_EXPIRYDATE,F_PCQE_QTY,F_PCQE_DEPT,F_PCQE_USER,F_PCQE_STATUS,F_PCQE_DATE,FCREATEDATE) values " + string.Join(",", list2);
					int num5 = DBServiceHelper.Execute(base.Context, text6);
					string text7 = string.Format("提交{0}行，成功更新{1}行", num, num5);
					bool flag8 = list.Count > 0;
					if (flag8)
					{
						text7 = text7 + "其中部分行处理信息未维护，行号为" + string.Join<int>(",", list);
					}
					bool flag9 = num5 > 0;
					if (flag9)
					{
						this.View.ShowMessage("提交成功:" + text7, 0);
					}
					else
					{
						this.View.ShowMessage("提交失败:" + text7, 0);
					}
				}
				else
				{
					this.View.ShowMessage("提交失败,处理信息未维护", 0);
				}
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00004F74 File Offset: 0x00003174
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			bool flag = e.Key.ToUpper().Equals("FBUTTON");
			if (flag)
			{
				bool flag2 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FORGID"));
				if (flag2)
				{
					this.View.ShowMessage("请选择组织!", 0);
				}
				else
				{
					string text = string.Format("SELECT A.FSTOCKORGID,A.FID,B.FMATERIALID,A.FSTOCKID,A.FBASEQTY,A.FLOT,A.FPRODUCEDATE,A.FEXPIRYDATE\r\n                            FROM T_STK_INVENTORY A\r\n                            inner join T_BD_MATERIAL B on A.FMATERIALID=B.FMASTERID and A.FSTOCKORGID=B.FUSEORGID\r\n                            left JOIN T_KING_INVENTORY C ON A.FID=C.FID\r\n                            where A.FBASEQTY!=0 and A.FEXPIRYDATE<GETDATE() AND C.FID is null\r\n                            and A.FSTOCKORGID={0}", (this.Model.GetValue("FORGID") as DynamicObject)["id"].ToString());
					DynamicObjectCollection dynamicObjectCollection = this.Model.GetValue("FMATERIAL") as DynamicObjectCollection;
					bool flag3 = dynamicObjectCollection.Count > 0;
					if (flag3)
					{
						List<string> list = new List<string>();
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							list.Add(dynamicObject["FMATERIAL_id"].ToString());
						}
						text += string.Format(" and B.FMATERIALID in ({0})", string.Join(",", list));
					}
					DynamicObjectCollection dynamicObjectCollection2 = this.Model.GetValue("FSTOCK") as DynamicObjectCollection;
					bool flag4 = dynamicObjectCollection2.Count > 0;
					if (flag4)
					{
						List<string> list2 = new List<string>();
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
						{
							list2.Add(dynamicObject2["FSTOCK_id"].ToString());
						}
						text += string.Format(" and A.FSTOCKID in ({0})", string.Join(",", list2));
					}
					DynamicObjectCollection dynamicObjectCollection3 = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
					this.View.Model.DeleteEntryData("FEntity");
					bool flag5 = dynamicObjectCollection3.Count > 0;
					if (flag5)
					{
						for (int i = 0; i < dynamicObjectCollection3.Count; i++)
						{
							this.View.Model.CreateNewEntryRow("FEntity");
							this.View.Model.SetItemValueByID("F_ORGID", dynamicObjectCollection3[i]["FSTOCKORGID"], i);
							this.View.Model.SetItemValueByID("F_MATERIAL", dynamicObjectCollection3[i]["FMATERIALID"], i);
							this.View.Model.SetItemValueByID("F_STOCK", dynamicObjectCollection3[i]["FSTOCKID"], i);
							this.View.Model.SetItemValueByID("F_LOT", dynamicObjectCollection3[i]["FLOT"], i);
							this.Model.SetValue("F_QTY", Convert.ToDecimal(dynamicObjectCollection3[i]["FBASEQTY"]), i);
							this.Model.SetValue("F_PRODUCEDATE", Convert.ToDateTime(dynamicObjectCollection3[i]["FPRODUCEDATE"]), i);
							this.Model.SetValue("F_EXPIRYDATE", Convert.ToDateTime(dynamicObjectCollection3[i]["FEXPIRYDATE"]), i);
							this.Model.SetValue("F_ID", dynamicObjectCollection3[i]["FID"], i);
						}
						this.View.UpdateView("FEntity");
					}
				}
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005338 File Offset: 0x00003538
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.FieldKey, "FORGID");
			if (flag)
			{
				List<Organization> userOrg = PermissionServiceHelper.GetUserOrg(this.View.Context);
				bool flag2 = userOrg.Count > 0;
				if (flag2)
				{
					string text = string.Empty;
					foreach (Organization organization in userOrg)
					{
						text = text + organization.Id.ToString() + ",";
					}
					e.ListFilterParameter.Filter = "FORGID in(" + text.TrimEnd(new char[]
					{
						','
					}) + ")";
				}
				else
				{
					this.View.ShowErrMessage("没有组织权限！", "", 0);
					e.ListFilterParameter.Filter = " 1=2";
				}
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00005444 File Offset: 0x00003644
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BaseDataFieldKey, "FORGID");
			if (flag)
			{
				List<Organization> userOrg = PermissionServiceHelper.GetUserOrg(this.View.Context);
				bool flag2 = userOrg.Count > 0;
				if (flag2)
				{
					string text = string.Empty;
					foreach (Organization organization in userOrg)
					{
						text = text + organization.Id.ToString() + ",";
					}
					e.Filter = "FORGID in(" + text.TrimEnd(new char[]
					{
						','
					}) + ")";
				}
				else
				{
					this.View.ShowErrMessage("没有组织权限！", "", 0);
					e.Filter = " 1=2";
				}
			}
		}
	}
}
