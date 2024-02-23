using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A0 RID: 160
	[Description("物料信息修改过滤界面")]
	public class MaterialEditFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000B3E RID: 2878 RVA: 0x00081D05 File Offset: 0x0007FF05
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.useOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
			this.materialIdChilds = (e.Paramter.GetCustomParameter("MaterialIds") as List<string>);
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x00081D44 File Offset: 0x0007FF44
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00081D50 File Offset: 0x0007FF50
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (MFGBillUtil.GetValue<long>(this.View.Model, "FOrgId", -1, 0L, null) != this.useOrgId)
			{
				this.View.Model.SetValue("FOrgId", this.useOrgId);
			}
			this.View.Model.SetValue("FMaterialIdChild", this.materialIdChilds.Distinct<string>().ToArray<string>());
			this.SetShow("FEntity");
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x00081DD8 File Offset: 0x0007FFD8
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.Key.Equals("FIsSync"))
			{
				Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				DynamicObject entityDataObject2 = this.Model.GetEntityDataObject(entryEntity, e.Row);
				bool isSync = OtherExtend.ConvertTo<bool>(e.NewValue, false);
				string text = entityDataObject2["FieldKey"].ToString();
				string a;
				if ((a = text) != null)
				{
					if (a == "FISSUETYPE")
					{
						this.SetIsSyncValue(entityDataObject, isSync, "FBACKFLUSHTYPE", e.Row);
						return;
					}
					if (a == "FBACKFLUSHTYPE")
					{
						this.SetIsSyncValue(entityDataObject, isSync, "FISSUETYPE", e.Row);
						return;
					}
					if (a == "FSTOCKID")
					{
						this.SetIsSyncValue(entityDataObject, isSync, "FSTOCKLOCID", e.Row);
						return;
					}
					if (a == "FSTOCKLOCID")
					{
						this.SetIsSyncValue(entityDataObject, isSync, "FSTOCKID", e.Row);
						return;
					}
					if (a == "FTIMEUNIT")
					{
						this.SetIsSyncValue(entityDataObject, isSync, "FOFFSETTIME", e.Row);
						return;
					}
					if (!(a == "FOFFSETTIME"))
					{
						return;
					}
					this.SetIsSyncValue(entityDataObject, isSync, "FTIMEUNIT", e.Row);
				}
			}
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x00081F2C File Offset: 0x0008012C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FOrgId"))
				{
					if (!(fieldKey == "FBomId"))
					{
						return;
					}
					e.IsShowApproved = false;
				}
				else
				{
					List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
					{
						Id = "ENG_BOM"
					}, "f323992d896745fbaab4a2717c79ce2e");
					if (!ListUtils.IsEmpty<long>(permissionOrg))
					{
						e.ListFilterParameter.Filter = this.SqlAppendAnd(e.ListFilterParameter.Filter, string.Format("FORGID IN ({0})", string.Join<long>(",", permissionOrg)));
						return;
					}
				}
			}
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x00081FCC File Offset: 0x000801CC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FOrgId"))
				{
					if (!(baseDataFieldKey == "FBomId"))
					{
						return;
					}
					e.IsShowApproved = false;
				}
				else
				{
					List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
					{
						Id = "ENG_BOM"
					}, "f323992d896745fbaab4a2717c79ce2e");
					if (!ListUtils.IsEmpty<long>(permissionOrg))
					{
						e.Filter = this.SqlAppendAnd(e.Filter, string.Format("FORGID IN ({0})", string.Join<long>(",", permissionOrg)));
						return;
					}
				}
			}
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x0008205F File Offset: 0x0008025F
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key == "FBtnOK")
			{
				this.isFromOKBtn = true;
			}
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x00082081 File Offset: 0x00080281
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			e.Cancel = this.isFromOKBtn;
			this.isFromOKBtn = false;
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x000820A0 File Offset: 0x000802A0
		private void SetShow(string entityKey)
		{
			int num = 0;
			this.View.Model.DeleteEntryData("FEntity");
			foreach (Field field in this.changeFiled)
			{
				this.View.Model.CreateNewEntryRow(entityKey);
				this.View.Model.SetValue("FFieldKey", field.Key, num);
				this.View.Model.SetValue("FFieldProp", field.PropertyName, num);
				this.View.Model.SetValue("FFieldName", field.Name[base.Context.LogLocale.LCID], num);
				this.View.Model.SetValue("FIsSync", true, num);
				num++;
			}
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x000821C8 File Offset: 0x000803C8
		private void SetIsSyncValue(DynamicObjectCollection entityDatas, bool isSync, string fieldKey, int index)
		{
			DynamicObject dynamicObject = (from I in entityDatas
			where DataEntityExtend.GetDynamicObjectItemValue<string>(I, "FieldKey", null) == fieldKey
			select I).FirstOrDefault<DynamicObject>();
			int num = entityDatas.IndexOf(dynamicObject);
			if (dynamicObject == null)
			{
				return;
			}
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsSync", false);
			if (dynamicValue != isSync)
			{
				this.Model.SetValue("FIsSync", isSync, num);
			}
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x0008222E File Offset: 0x0008042E
		private string SqlAppendAnd(string sql, string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				return sql;
			}
			return sql + (string.IsNullOrWhiteSpace(sql) ? "" : " AND ") + filter;
		}

		// Token: 0x0400054D RID: 1357
		private long useOrgId;

		// Token: 0x0400054E RID: 1358
		private bool isFromOKBtn;

		// Token: 0x0400054F RID: 1359
		private List<string> materialIdChilds;

		// Token: 0x04000550 RID: 1360
		public List<Field> changeFiled = new List<Field>
		{
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("变动损耗率", "0151515153499000013262", 7, new object[0])),
				Key = "FSCRAPRATE",
				PropertyName = "SCRAPRATE",
				EntityKey = "FTreeEntity",
				ElementType = 2
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("固定损耗", "015072000014374", 7, new object[0])),
				Key = "FFIXSCRAPQTY",
				PropertyName = "FIXSCRAPQTY",
				EntityKey = "FTreeEntity",
				ElementType = 22
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("发料方式", "015072000014378", 7, new object[0])),
				Key = "FISSUETYPE",
				PropertyName = "ISSUETYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("倒冲时机", "015072000014379", 7, new object[0])),
				Key = "FBACKFLUSHTYPE",
				PropertyName = "BACKFLUSHTYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("发料仓库", "0151515153499000013263", 7, new object[0])),
				Key = "FSTOCKID",
				PropertyName = "STOCKID",
				EntityKey = "FTreeEntity",
				ElementType = 13
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("发料仓位", "0151515153499000013264", 7, new object[0])),
				Key = "FSTOCKLOCID",
				PropertyName = "STOCKLOCID",
				EntityKey = "FTreeEntity",
				ElementType = 13
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("超发控制方式", "015072000014383", 7, new object[0])),
				Key = "FOVERCONTROLMODE",
				PropertyName = "OVERCONTROLMODE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("领料考虑最小发料批量", "015072000025077", 7, new object[0])),
				Key = "FISMINISSUEQTY",
				PropertyName = "ISMINISSUEQTY",
				EntityKey = "FTreeEntity",
				ElementType = 8
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("时间单位", "015072000014386", 7, new object[0])),
				Key = "FTIMEUNIT",
				PropertyName = "TIMEUNIT",
				EntityKey = "FTreeEntity",
				ElementType = 9
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("偏置时间", "015072000014385", 7, new object[0])),
				Key = "FOFFSETTIME",
				PropertyName = "OFFSETTIME",
				EntityKey = "FTreeEntity",
				ElementType = 3
			},
			new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("是否关键件", "015072000014387", 7, new object[0])),
				Key = "FISKEYCOMPONENT",
				PropertyName = "ISKEYCOMPONENT",
				EntityKey = "FTreeEntity",
				ElementType = 8
			}
		};
	}
}
