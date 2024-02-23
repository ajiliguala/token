using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200003C RID: 60
	public class ResourceList : BaseControlList
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x000356B4 File Offset: 0x000338B4
		private List<int> lstCurSelId
		{
			get
			{
				return (from p in this.ListView.SelectedRowsInfo
				select Convert.ToInt32(p.PrimaryKeyValue)).ToList<int>();
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x0003582C File Offset: 0x00033A2C
		private Dictionary<int, string> dctCurSelIdNum
		{
			get
			{
				var source = from p in this.ListView.SelectedRowsInfo
				select new
				{
					CurID = Convert.ToInt32(p.PrimaryKeyValue),
					CurNumber = p.Number
				};
				return source.Distinct().ToDictionary(k => k.CurID, k => k.CurNumber);
			}
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x000358AC File Offset: 0x00033AAC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "UNAUDIT"))
				{
					return;
				}
				if (this.firstDoOperation && !this.CanUnAudit(e))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00035904 File Offset: 0x00033B04
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.firstDoOperation = true;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x000359D0 File Offset: 0x00033BD0
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool bCheckResult = false;
			if (this.lstCurSelId.Count <= 0)
			{
				return bCheckResult;
			}
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_WorkCenter");
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetaData.BusinessInfo;
			Entity entity = formMetaData.BusinessInfo.GetField("FResourceID").Entity;
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = entity.TableName,
				TableNameAs = entity.TableAlias,
				FieldName = "FID",
				ScourceKey = "FID"
			});
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "TABLE(fn_StrSplit(@ResourceIds,',',1))",
				TableNameAs = "SP",
				FieldName = "FID",
				ScourceKey = "FResourceID"
			});
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@ResourceIds", 161, this.lstCurSelId.Distinct<int>().ToArray<int>()));
			List<string> list = new List<string>
			{
				"FID",
				"FNumber",
				"FResourceID"
			};
			List<DynamicObject> list2 = BusinessDataServiceHelper.Load(base.Context, formMetaData.BusinessInfo.GetSubBusinessInfo(list).GetDynamicObjectType(), queryBuilderParemeter).ToList<DynamicObject>();
			if (list2 != null && list2.Count > 0)
			{
				bCheckResult = (list2.Count <= 0);
				string text = string.Empty;
				StringBuilder stringBuilder = new StringBuilder();
				using (Dictionary<int, string>.KeyCollection.Enumerator enumerator = this.dctCurSelIdNum.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int iFID = enumerator.Current;
						IEnumerable<DynamicObject> source = from varItem in list2
						where ((DynamicObjectCollection)varItem["WorkCenterCapacity"]).Any((DynamicObject rowItem) => Convert.ToInt32(rowItem["RESOURCEID_Id"]).Equals(iFID))
						select varItem;
						text = string.Join<object>(",", from p in source
						select p["Number"]);
						if (text.Length > 0)
						{
							stringBuilder.AppendFormat(ResManager.LoadKDString("资源[{0}]已被工作中心[{1}]引用,", "015072000001804", 7, new object[0]), this.dctCurSelIdNum[iFID], text);
							stringBuilder.AppendLine();
						}
					}
				}
				stringBuilder.Append(ResManager.LoadKDString("请确认是否反审核?", "015072000001792", 7, new object[0]));
				this.View.ShowMessage(stringBuilder.ToString(), 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						this.View.InvokeFormOperation(e.Operation.FormOperation.Operation.ToString());
						bCheckResult = true;
						return;
					}
					this.firstDoOperation = true;
				}, "", 0);
			}
			else
			{
				bCheckResult = true;
			}
			return bCheckResult;
		}

		// Token: 0x040001D8 RID: 472
		private const string FKey_FResourceID = "FRESOURCEID";

		// Token: 0x040001D9 RID: 473
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x040001DA RID: 474
		private const string FKey_FID = "FID";

		// Token: 0x040001DB RID: 475
		private bool firstDoOperation = true;
	}
}
