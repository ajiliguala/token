using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
	// Token: 0x02000005 RID: 5
	[Description("物料审核时，同步审批云之家")]
	[HotUpdate]
	public class ToYunzhijia : AbstractOperationServicePlugIn
	{
		// Token: 0x06000016 RID: 22 RVA: 0x000039CC File Offset: 0x00001BCC
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("F_KING_YZJSERIALNUMBER");
			e.FieldKeys.Add("F_KING_FLOWINSTID");
			e.FieldKeys.Add("F_KING_FORMCODEID");
			e.FieldKeys.Add("F_KING_FORMDEFID");
			e.FieldKeys.Add("F_KING_FORMINSTID");
			e.FieldKeys.Add("FNumber");
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00003A48 File Offset: 0x00001C48
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			KingdeeWebApi kingdeeWebApi = new KingdeeWebApi();
			yunzhijiaInterface yunzhijiaInterface = new yunzhijiaInterface();
			string token = yunzhijiaInterface.GetToken();
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				string text = dynamicObject["F_KING_YZJSERIALNUMBER"].ToString();
				string text2 = dynamicObject["F_KING_FLOWINSTID"].ToString();
				string text3 = dynamicObject["F_KING_FORMCODEID"].ToString();
				string text4 = dynamicObject["F_KING_FORMDEFID"].ToString();
				string text5 = dynamicObject["F_KING_FORMINSTID"].ToString();
				string value = dynamicObject["Number"].ToString();
				bool flag = ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2) || ObjectUtils.IsNullOrEmptyOrWhiteSpace(text3) || ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4) || ObjectUtils.IsNullOrEmptyOrWhiteSpace(text5);
				if (!flag)
				{
					string yzjUserId = kingdeeWebApi.SelectYzjUserId(base.Context, base.Context.UserName).ToString();
					yunzhijiaInterface.Agree(token, text2, text3, text4, text5, value, yzjUserId);
				}
			}
		}
	}
}
