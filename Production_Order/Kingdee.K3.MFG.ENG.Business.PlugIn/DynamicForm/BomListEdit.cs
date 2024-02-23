using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000072 RID: 114
	[Description("物料清单列表表单插件")]
	public class BomListEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000828 RID: 2088 RVA: 0x00060ED0 File Offset: 0x0005F0D0
		public override void OnInitialize(InitializeEventArgs e)
		{
			string text = Convert.ToString(this.View.OpenParameter.GetCustomParameter("title") ?? string.Empty);
			LocaleValue formTitle = new LocaleValue(text);
			this.View.SetFormTitle(formTitle);
			base.OnInitialize(e);
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x00060F1C File Offset: 0x0005F11C
		public override void OnLoad(EventArgs e)
		{
			DynamicObjectCollection dynamicObjectCollection = this.View.OpenParameter.GetCustomParameter("data") as DynamicObjectCollection;
			bool flag = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("Enable"));
			Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
			int num = 0;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (flag)
				{
					if (this.IsEnable(Convert.ToInt64(dynamicObject["FID"]), Convert.ToInt64(dynamicObject["ENTRYID"])))
					{
						this.View.Model.CreateNewEntryRow(entity, num);
						this.View.Model.SetValue("FUseOrgId", Convert.ToInt64(dynamicObject["USERORGID"]), num);
						this.View.Model.SetValue("FBOMNumber", dynamicObject["BOMNUMBER"].ToString(), num);
						this.View.Model.SetValue("FDocumentStatus", dynamicObject["DOCUMENTSTATUS"].ToString(), num);
						this.View.Model.SetValue("FMATERIALID", Convert.ToInt64(dynamicObject["MATERIALID"]), num);
						this.View.Model.SetValue("FParentAuxPropId", Convert.ToInt64(dynamicObject["AUXPROPID"]), num);
						this.View.Model.SetValue("FUNITID", Convert.ToInt64(dynamicObject["UNITID"]), num);
						this.View.Model.SetValue("FReplaceGroup", Convert.ToInt32(dynamicObject["REPLACEGROUP"]), num);
						this.View.Model.SetValue("FMATERIALIDCHILD", Convert.ToInt64(dynamicObject["MATERIALIDCHILD"]), num);
						this.View.Model.SetValue("FCHILDUNITID", Convert.ToInt64(dynamicObject["CHILDUNITID"]), num);
						this.View.Model.SetValue("FDOSAGETYPE", dynamicObject["DOSAGETYPE"].ToString(), num);
						this.View.Model.SetValue("FBOMID", dynamicObject["FID"].ToString(), num);
						this.View.Model.SetValue("FEntryId", dynamicObject["ENTRYID"].ToString(), num);
						num++;
					}
				}
				else
				{
					this.View.Model.CreateNewEntryRow(entity, num);
					this.View.Model.SetValue("FUseOrgId", Convert.ToInt64(dynamicObject["USERORGID"]), num);
					this.View.Model.SetValue("FBOMNumber", dynamicObject["BOMNUMBER"].ToString(), num);
					this.View.Model.SetValue("FDocumentStatus", dynamicObject["DOCUMENTSTATUS"].ToString(), num);
					this.View.Model.SetValue("FMATERIALID", Convert.ToInt64(dynamicObject["MATERIALID"]), num);
					this.View.Model.SetValue("FParentAuxPropId", Convert.ToInt64(dynamicObject["AUXPROPID"]), num);
					this.View.Model.SetValue("FUNITID", Convert.ToInt64(dynamicObject["UNITID"]), num);
					this.View.Model.SetValue("FReplaceGroup", Convert.ToInt32(dynamicObject["REPLACEGROUP"]), num);
					this.View.Model.SetValue("FMATERIALIDCHILD", Convert.ToInt64(dynamicObject["MATERIALIDCHILD"]), num);
					this.View.Model.SetValue("FCHILDUNITID", Convert.ToInt64(dynamicObject["CHILDUNITID"]), num);
					this.View.Model.SetValue("FDOSAGETYPE", dynamicObject["DOSAGETYPE"].ToString(), num);
					this.View.Model.SetValue("FBOMID", dynamicObject["FID"].ToString(), num);
					this.View.Model.SetValue("FEntryId", dynamicObject["ENTRYID"].ToString(), num);
					num++;
				}
			}
			this.View.UpdateView("FEntity");
			base.OnLoad(e);
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x00061434 File Offset: 0x0005F634
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FCONFIRM"))
				{
					if (!(a == "FCANCEL"))
					{
						return;
					}
					this.isUpdate = false;
					this.View.Close();
				}
				else
				{
					this.isUpdate = true;
					if (this.GetDataList().Count == 0)
					{
						this.View.ShowMessage(ResManager.LoadKDString("没有勾选任何数据！", "0151515153499030041205", 7, new object[0]), 0);
						return;
					}
					this.View.Close();
					return;
				}
			}
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x000614C8 File Offset: 0x0005F6C8
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			List<LadderLossUtils.DataList> list = new List<LadderLossUtils.DataList>();
			if (this.isUpdate)
			{
				list = this.GetDataList();
			}
			if (list.Count > 0)
			{
				this.isUpdate = true;
			}
			else
			{
				this.isUpdate = false;
			}
			dictionary.Add("isUpdate", this.isUpdate);
			dictionary.Add("datalist", list);
			FormResult formResult = new FormResult(dictionary);
			this.View.ReturnToParentWindow(formResult);
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x0006156C File Offset: 0x0005F76C
		private bool IsEnable(long bomId, long entryId)
		{
			DynamicObject dataObject = LadderLossUtils.GetDataObject(base.Context, bomId, "ENG_BOM");
			DynamicObjectCollection source = dataObject["TreeEntity"] as DynamicObjectCollection;
			DynamicObject dynamicObject = (from p in source
			where Convert.ToInt64(p["id"]) == entryId
			select p).FirstOrDefault<DynamicObject>();
			DynamicObjectCollection dynamicObjectCollection = dynamicObject["BOMCHILDLOTBASEDQTY"] as DynamicObjectCollection;
			bool result = true;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				if (Convert.ToDecimal(dynamicObject2["FIXSCRAPQTYLOT"]) == 0m && Convert.ToDecimal(dynamicObject2["SCRAPRATELOT"]) == 0m)
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x00061678 File Offset: 0x0005F878
		private List<LadderLossUtils.DataList> GetDataList()
		{
			List<LadderLossUtils.DataList> list = new List<LadderLossUtils.DataList>();
			DynamicObject dataObject = this.View.Model.DataObject;
			DynamicObjectCollection dynamicObjectCollection = dataObject["BOMLISTENTRY"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (Convert.ToBoolean(dynamicObject["Opt"]))
				{
					list.Add(new LadderLossUtils.DataList
					{
						idList = Convert.ToInt64(dynamicObject["BOMID"]),
						entryList = Convert.ToInt64(dynamicObject["ENTRYID"]),
						materialidList = Convert.ToInt64(dynamicObject["MATERIALIDCHILD_ID"]),
						unitList = Convert.ToInt64(dynamicObject["CHILDUNITID_ID"])
					});
				}
			}
			return list;
		}

		// Token: 0x040003BA RID: 954
		public bool isUpdate;
	}
}
