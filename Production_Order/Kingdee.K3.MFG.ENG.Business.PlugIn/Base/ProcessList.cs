using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200003A RID: 58
	public class ProcessList : BaseControlList
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000430 RID: 1072 RVA: 0x000350C0 File Offset: 0x000332C0
		private List<int> lstCurSelId
		{
			get
			{
				return (from p in this.ListView.SelectedRowsInfo
				select Convert.ToInt32(p.PrimaryKeyValue)).Distinct<int>().ToList<int>();
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000431 RID: 1073 RVA: 0x0003523C File Offset: 0x0003343C
		private Dictionary<int, string> dctCurSelIdNum
		{
			get
			{
				var source = (from p in this.ListView.SelectedRowsInfo
				select new
				{
					Id = Convert.ToInt32(p.PrimaryKeyValue),
					Number = p.Number
				}).Distinct();
				return source.ToDictionary(p => p.Id, p => p.Number);
			}
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x000352BC File Offset: 0x000334BC
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

		// Token: 0x06000433 RID: 1075 RVA: 0x00035314 File Offset: 0x00033514
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.firstDoOperation = true;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x000353E0 File Offset: 0x000335E0
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool bCheckResult = false;
			if (this.lstCurSelId.Count <= 0)
			{
				return bCheckResult;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int num in this.lstCurSelId)
			{
				stringBuilder.AppendFormat("{0},", num.ToString());
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			string text = string.Format("{0} IN ({1})", "FPROCESSID", stringBuilder.ToString());
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FPROCESSID")
			};
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Route", list, text, "");
			if (baseBillInfo != null && baseBillInfo.Count > 0)
			{
				bCheckResult = (baseBillInfo.Count <= 0);
				string text2 = string.Empty;
				StringBuilder stringBuilder2 = new StringBuilder();
				using (Dictionary<int, string>.KeyCollection.Enumerator enumerator2 = this.dctCurSelIdNum.Keys.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						int iFID = enumerator2.Current;
						IEnumerable<DynamicObject> source = from varItem in baseBillInfo
						where ((DynamicObjectCollection)varItem["Entity"]).Any((DynamicObject rowItem) => Convert.ToInt32(rowItem["PROCESSID_Id"]).Equals(iFID))
						select varItem;
						text2 = string.Join<object>(",", from p in source
						select p["Number"]);
						if (text2.Length > 0)
						{
							stringBuilder2.AppendFormat(ResManager.LoadKDString("作业[{0}]已被工艺路线[{1}]引用,", "015072000001789", 7, new object[0]), this.dctCurSelIdNum[iFID], text2);
							stringBuilder2.AppendLine();
						}
					}
				}
				stringBuilder2.Append(ResManager.LoadKDString("请确认是否反审核?", "015072000001792", 7, new object[0]));
				this.View.ShowMessage(stringBuilder2.ToString(), 4, delegate(MessageBoxResult result)
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

		// Token: 0x040001CD RID: 461
		private const string FKey_FProcessID = "FPROCESSID";

		// Token: 0x040001CE RID: 462
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x040001CF RID: 463
		private const string FKey_FID = "FID";

		// Token: 0x040001D0 RID: 464
		private bool firstDoOperation = true;

		// Token: 0x0200003B RID: 59
		public class KeyItem
		{
			// Token: 0x0600043B RID: 1083 RVA: 0x00035697 File Offset: 0x00033897
			public static string GetKey(ListSelectedRow item)
			{
				return item.PrimaryKeyValue;
			}

			// Token: 0x040001D6 RID: 470
			public long Id;

			// Token: 0x040001D7 RID: 471
			public string Number;
		}
	}
}
