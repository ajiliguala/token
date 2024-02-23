using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000045 RID: 69
	[Description("工艺路线_列表插件")]
	public class RouteList : BaseControlList
	{
		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060004BB RID: 1211 RVA: 0x0003A193 File Offset: 0x00038393
		private FormMetadata Meta
		{
			get
			{
				return (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_Route", true);
			}
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0003A1AC File Offset: 0x000383AC
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.IsSyncCostResource)
			{
				this.View.GetMainBarItem("tbRefresh").Visible = false;
				this.View.GetMainBarItem("tbSplitNew").Visible = false;
				this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
				this.View.GetMainBarItem("tbSplitApprove").Visible = false;
				this.View.GetMainBarItem("tbPSplitBussinessOpt").Visible = false;
				this.View.GetMainBarItem("tbParaLists").Visible = false;
				this.View.GetMainBarItem("tbClose").Visible = false;
				this.View.GetMainBarItem("tbDelete").Visible = false;
			}
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0003A27B File Offset: 0x0003847B
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitCustomParameters();
			this.InitFormTitle();
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x0003A2A8 File Offset: 0x000384A8
		public override void AfterGetData()
		{
			base.AfterGetData();
			if (this.IsSyncCostResource)
			{
				this.ListView.BillBusinessInfo.GetForm().ListFormatConditions = (from o in this.ListView.BillBusinessInfo.GetForm().ListFormatConditions
				where !StringUtils.EqualsIgnoreCase("SyncCostResource", o.Key)
				select o).ToList<FormatCondition>();
				this.CurrentViewData = null;
			}
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0003A37C File Offset: 0x0003857C
		public override void OnFormatRowConditions(ListFormatConditionArgs args)
		{
			base.OnFormatRowConditions(args);
			if (this.IsSyncCostResource)
			{
				if (this.CurrentViewData == null)
				{
					this.CurrentViewData = BusinessDataServiceHelper.Load(base.Context, (from o in this.ListView.CurrentPageRowsInfo
					select Convert.ToInt64(o.PrimaryKeyValue)).Distinct<object>().ToArray<object>(), this.Meta.BusinessInfo.GetDynamicObjectType()).ToDictionary((DynamicObject k) => Convert.ToInt64(k["Id"]));
				}
				long key = Convert.ToInt64(((DynamicObjectDataRow)args.DataRow).DynamicObject["FID"]);
				long entryId = Convert.ToInt64(((DynamicObjectDataRow)args.DataRow).DynamicObject["t1_FENTRYID"]);
				long detailId = Convert.ToInt64(((DynamicObjectDataRow)args.DataRow).DynamicObject["t2_FDETAILID"]);
				if (!this.CurrentViewData.ContainsKey(key))
				{
					return;
				}
				DynamicObject dynamicObject = this.CurrentViewData[key];
				DynamicObject dynamicObject2 = ((DynamicObjectCollection)dynamicObject["RouteOperSeq"]).FirstOrDefault((DynamicObject e) => Convert.ToInt64(e["Id"]) == entryId);
				DynamicObject dynamicObject3 = ((DynamicObjectCollection)dynamicObject2["RouteOperDetail"]).FirstOrDefault((DynamicObject e) => detailId == Convert.ToInt64(e["Id"]));
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject3["SubEntityCost"];
				bool flag = dynamicObjectCollection.Count != this.CostResInfo.Count;
				string text = string.Empty;
				if (flag)
				{
					text = "#F2D9B0";
				}
				else
				{
					foreach (DynamicObject dynamicObject4 in dynamicObjectCollection)
					{
						long key2 = Convert.ToInt64(dynamicObject4["ResourceId_Id"]);
						if (!this.CostResInfo.ContainsKey(key2))
						{
							text = "#F2D9B0";
							break;
						}
						decimal d = Convert.ToDecimal(dynamicObject4["UseRate"]);
						int num = Convert.ToInt32(dynamicObject4["ResourceReqNum"]);
						bool flag2 = d != this.CostResInfo[key2].Item1 || num != this.CostResInfo[key2].Item2;
						if (flag2)
						{
							text = "#F2D9B0";
							break;
						}
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					args.FormatConditions.Add(new FormatCondition
					{
						ApplayRow = true,
						BackColor = text,
						Key = "SyncCostResource"
					});
				}
			}
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0003A658 File Offset: 0x00038858
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToString()) != null)
			{
				if (!(a == "Audit"))
				{
					return;
				}
				if (e.OperationResult.OperateResult != null && e.OperationResult.OperateResult.Count != 0)
				{
					List<long> list = (from o in e.OperationResult.OperateResult
					select Convert.ToInt64(o.PKValue)).ToList<long>();
					if (!ObjectUtils.IsNullOrEmpty(list))
					{
						FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_ROUTE_PARAM", true);
						DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, "ENG_ROUTE", "UserParameter");
						if (Convert.ToBoolean(dynamicObject["IsUpdateRoute"]))
						{
							RouteServiceHelper.SetMaterialDefaultRoute(base.Context, list);
						}
					}
				}
			}
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0003A774 File Offset: 0x00038974
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (this.IsContainProduceType)
			{
				e.AppendQueryFilter(string.Format(" FProduceType = '{0}' ", this.ProduceType));
			}
			if (e.SelectedEntities.Any((FilterEntity o) => o.Key == "FEntity"))
			{
				e.AppendQueryOrderby("FSeqNumber");
			}
			if (e.SelectedEntities.Any((FilterEntity o) => o.Key == "FSubEntity"))
			{
				e.AppendQueryOrderby("FOperNumber");
			}
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x0003A84C File Offset: 0x00038A4C
		private void InitCustomParameters()
		{
			Dictionary<string, object> customParameters = this.View.OpenParameter.GetCustomParameters();
			if (customParameters.ContainsKey("ProduceType"))
			{
				this.IsContainProduceType = true;
				this.ProduceType = customParameters["ProduceType"].ToString();
			}
			else
			{
				if (this.View.ParentFormView != null)
				{
					Dictionary<string, object> customParameters2 = this.View.ParentFormView.OpenParameter.GetCustomParameters();
					if (customParameters2.ContainsKey("ProduceType"))
					{
						this.IsContainProduceType = true;
						this.ProduceType = customParameters2["ProduceType"].ToString();
					}
				}
				customParameters.Add("ProduceType", this.ProduceType);
			}
			if (!customParameters.ContainsKey("LayoutId"))
			{
				customParameters.Add("LayoutId", this.GetLayoutId());
			}
			this.IsSyncCostResource = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsSyncCostResource"));
			if (this.IsSyncCostResource)
			{
				DynamicObjectCollection source = (DynamicObjectCollection)this.View.OpenParameter.GetCustomParameter("CostInfo");
				this.CostResInfo = source.ToDictionary((DynamicObject k) => Convert.ToInt64(k["CostResource_Id"]), (DynamicObject v) => new Tuple<decimal, int>(Convert.ToDecimal(v["UseRate"]), Convert.ToInt32(v["ResourceReqNum"])));
			}
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x0003A99C File Offset: 0x00038B9C
		private string GetLayoutId()
		{
			string text = this.View.OpenParameter.LayoutId;
			if (text == null)
			{
				string produceType;
				if ((produceType = this.ProduceType) != null)
				{
					if (produceType == "C")
					{
						text = "";
						goto IL_50;
					}
					if (produceType == "F")
					{
						text = "9c3de02d-5469-44ef-8fd7-9821f371a8cf";
						goto IL_50;
					}
				}
				text = "";
				IL_50:
				this.View.OpenParameter.LayoutId = text;
			}
			return text;
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x0003AA0C File Offset: 0x00038C0C
		private void InitFormTitle()
		{
			if ("C".Equals(this.ProduceType))
			{
				return;
			}
			LocaleValue localeValue = new LocaleValue(ResManager.LoadKDString("柔性工艺路线列表", "015072000011014", 7, new object[0]), base.Context.UserLocale.LCID);
			this.View.SetFormTitle(localeValue);
			this.View.SetInnerTitle(localeValue);
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x0003AA70 File Offset: 0x00038C70
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBCUSBULKEDIT")
				{
					this.ShowBatchEditView(e, 0);
					return;
				}
				if (a == "TBSETDEFAULT")
				{
					this.SetDefaultRoute();
					return;
				}
				if (!(a == "TBBATCHBULKEDIT"))
				{
					return;
				}
				this.ShowBatchEditView(e, 1);
			}
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x0003AAD4 File Offset: 0x00038CD4
		private void ShowBatchEditView(BarItemClickEventArgs e, int flag)
		{
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				e.Cancel = true;
				return;
			}
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			foreach (ListSelectedRow listSelectedRow in this.ListView.SelectedRowsInfo)
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				if (listSelectedRow.FieldValues.TryGetValue("FSubEntity", out empty))
				{
					list.Add(Convert.ToInt64(empty));
				}
				if (listSelectedRow.FieldValues.TryGetValue("FBillHead", out empty2))
				{
					list2.Add(Convert.ToInt64(empty2));
				}
			}
			if (ListUtils.IsEmpty<long>(list))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何工序数据，请先选择工序数据！", "015072000025076", 7, new object[0]), "", 0);
				e.Cancel = true;
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			if (flag == 1)
			{
				this.OperUintlst(list);
				if (this.prdOrgId.Distinct<long>().Count<long>() > 1)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("批量修改工艺路线中加工组织不相同，不允许批量修改", "015072000013464", 7, new object[0]), "", 0);
					return;
				}
				if (this.operUnitIds.Distinct<long>().Count<long>() > 1)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("批量修改工艺路线中工序单位不一样，不允许批量修改", "015072000013465", 7, new object[0]), "", 0);
					return;
				}
				dynamicFormShowParameter.FormId = "ENG_RouteBatchFieldsEdit";
				dynamicFormShowParameter.CustomComplexParams.Add("lstRouteHead", list2);
				dynamicFormShowParameter.CustomComplexParams.Add("FProOrgId", this.prdOrgId[0]);
				dynamicFormShowParameter.CustomComplexParams.Add("operUnitId", this.operUnitIds[0]);
			}
			else if (flag == 0)
			{
				dynamicFormShowParameter.FormId = "ENG_RouteBatchEdit";
			}
			dynamicFormShowParameter.CustomComplexParams.Add("fPKEntryIds", list);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x0003AD58 File Offset: 0x00038F58
		private void SetDefaultRoute()
		{
			if (this.ListView.CurrentSelectedRowInfo == null || this.ListView.SelectedRowsInfo.Count == 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选择数据进行操作！", "015072030045649", 7, new object[0]), "", 0);
				return;
			}
			List<long> list = (from o in this.ListView.SelectedRowsInfo
			select Convert.ToInt64(o.FieldValues["FBillHead"])).ToList<long>();
			IOperationResult operationResult = RouteServiceHelper.SetMaterialDefaultRoute(base.Context, list);
			if (operationResult.IsSuccess)
			{
				this.View.ShowMessage(ResManager.LoadKDString("默认工艺路线设置成功！", "015072000014321", 7, new object[0]), 0);
				return;
			}
			FormUtils.ShowOperationResult(this.View, operationResult, null);
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0003AE24 File Offset: 0x00039024
		private void OperUintlst(List<long> entryIds)
		{
			this.operUnitIds.Clear();
			this.prdOrgId.Clear();
			string text = string.Format("SELECT FPROORGID, FOperUnitID FROM T_ENG_ROUTEOPERDETAIL d \r\n                           INNER JOIN (SELECT /*+ cardinality(b {0})*/ FID FROM table(fn_StrSplit(@entryIds,',',1)) b) T1 ON T1.FID=d.fdetailId", entryIds.Distinct<long>().Count<long>());
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@entryIds", 161, entryIds.Distinct<long>().ToArray<long>())
			};
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, list))
			{
				while (dataReader.Read())
				{
					this.operUnitIds.Add(Convert.ToInt64(dataReader["FOperUnitID"]));
					this.prdOrgId.Add(Convert.ToInt64(dataReader["FPROORGID"]));
				}
			}
		}

		// Token: 0x0400020E RID: 526
		private bool IsContainProduceType;

		// Token: 0x0400020F RID: 527
		private string ProduceType = "C";

		// Token: 0x04000210 RID: 528
		private bool IsSyncCostResource;

		// Token: 0x04000211 RID: 529
		private Dictionary<long, DynamicObject> CurrentViewData;

		// Token: 0x04000212 RID: 530
		private Dictionary<long, Tuple<decimal, int>> CostResInfo = new Dictionary<long, Tuple<decimal, int>>();

		// Token: 0x04000213 RID: 531
		private List<long> operUnitIds = new List<long>();

		// Token: 0x04000214 RID: 532
		private List<long> prdOrgId = new List<long>();
	}
}
