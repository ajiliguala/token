using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000004 RID: 4
	public class AutoSubmit : IScheduleService
	{
		// Token: 0x0600000C RID: 12 RVA: 0x0000274C File Offset: 0x0000094C
		public void Run(Context ctx, Schedule schedule)
		{
			try
			{
				string text = string.Format("/*dialect*/ select * from PCQE_CGXXBD where FDOCUMENTSTATUS in('D','A')", Array.Empty<object>());
				DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
				bool flag = dynamicObjectCollection.Count > 0;
				if (flag)
				{
					Organization organization = OrganizationServiceHelper.ReadOrgInfoByOrgId(ctx, 1L);
					List<long> functionIds = new List<long>();
					bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(organization.OrgFunctions);
					if (flag2)
					{
						functionIds = Array.ConvertAll<string, long>(organization.OrgFunctions.Split(new char[]
						{
							','
						}), (string a) => Convert.ToInt64(a)).ToList<long>();
					}
					OrganizationInfo currentOrganizationInfo = new OrganizationInfo
					{
						ID = organization.Id,
						Name = organization.Name,
						FunctionIds = functionIds,
						AcctOrgType = organization.AcctOrgType
					};
					ctx.CurrentOrganizationInfo = currentOrganizationInfo;
					string text2 = string.Empty;
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						text2 = text2 + dynamicObject["fid"].ToString() + ",";
					}
					text2 = text2.TrimEnd(new char[]
					{
						','
					});
					JObject jobject = new JObject();
					jobject["Ids"] = text2;
					object obj = WebApiServiceCall.Submit(ctx, "PCQE_CGXXBD", JsonConvert.SerializeObject(jobject));
				}
			}
			catch (Exception ex)
			{
				Logger.Error("AutoSubmit", ex.Message, ex);
				schedule.Status = 0;
				throw ex;
			}
		}
	}
}
