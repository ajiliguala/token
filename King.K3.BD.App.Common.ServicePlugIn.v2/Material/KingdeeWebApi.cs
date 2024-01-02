using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
	// Token: 0x02000003 RID: 3
	[HotUpdate]
	public class KingdeeWebApi
	{
		// Token: 0x06000004 RID: 4 RVA: 0x00002D20 File Offset: 0x00000F20
		public string DraftMaterial(Context context, string FNumber, string FName, string FSpecification, string FMnemonicCode, string FMaterialGroupID, string F_CGY, string FErpClsID, string FBaseUnitId, string FCategoryID, string FLENGTH, string FWIDTH, string FIsKitting, string FExpPeriod, string FormDefId, string YZJSERIALNUMBER, string flowInstId, string formCodeId, string formDefId, string formInstId, string deptInfo)
		{
			string a = FormDefId;
			if (!(a == "无"))
			{
				if (!(a == "JL"))
				{
					if (!(a == "LL"))
					{
						if (!(a == "JK"))
						{
							if (a == "CK")
							{
								FormDefId = "-CK";
							}
						}
						else
						{
							FormDefId = "-JK";
						}
					}
					else
					{
						FormDefId = "-LL";
					}
				}
				else
				{
					FormDefId = "-JL";
				}
			}
			else
			{
				FormDefId = null;
			}
			JObject jobject = new JObject();
			JObject jobject2 = new JObject();
			JObject jobject3 = new JObject();
			JObject jobject4 = new JObject();
			JObject jobject5 = new JObject();
			JObject jobject6 = new JObject();
			JObject jobject7 = new JObject();
			JObject jobject8 = new JObject();
			JObject jobject9 = new JObject();
			JObject jobject10 = new JObject();
			jobject2["Model"] = jobject;
			jobject["FCreateOrgId"] = jobject4;
			jobject["FUseOrgId"] = jobject5;
			jobject["SubHeadEntity5"] = jobject7;
			jobject["SubHeadEntity1"] = jobject6;
			jobject["SubHeadEntity"] = jobject3;
			jobject4["FNumber"] = "999";
			jobject5["FNumber"] = "999";
			jobject["FNumber"] = FNumber;
			jobject["FName"] = FName;
			jobject["FSpecification"] = FSpecification;
			jobject["FMnemonicCode"] = FMnemonicCode;
			jobject["FMaterialGroup"] = jobject10;
			jobject["F_CGY"] = F_CGY;
			jobject["F_KING_YZJSERIALNUMBER"] = YZJSERIALNUMBER;
			jobject["F_KING_flowInstId"] = flowInstId;
			jobject["F_KING_formCodeId"] = formCodeId;
			jobject["F_KING_formDefId"] = formDefId;
			jobject["F_KING_formInstId"] = formInstId;
			jobject["F_KING_APDEPT"] = deptInfo;
			jobject["F_KING_EMPEXPSUFFIX"] = FormDefId;
			jobject3["FErpClsID"] = FErpClsID;
			jobject3["FBaseUnitId"] = jobject8;
			jobject3["FCategoryID"] = jobject9;
			jobject3["FLENGTH"] = FLENGTH;
			jobject3["FWIDTH"] = FWIDTH;
			jobject8["FNumber"] = FBaseUnitId;
			jobject9["FNumber"] = FCategoryID;
			jobject10["FNumber"] = FMaterialGroupID;
			jobject7["FIsKitting"] = FIsKitting;
			jobject6["FIsBatchManage"] = "true";
			jobject6["FExpPeriod"] = FExpPeriod;
			JObject jobject11 = JObject.Parse(JsonConvert.SerializeObject(WebApiServiceCall.Draft(context, "BD_MATERIAL", Convert.ToString(jobject2))));
			return jobject11["Result"]["ResponseStatus"]["SuccessEntitys"][0]["Number"].ToString();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00003088 File Offset: 0x00001288
		public int SelectMaterial(Context context, string F_KING_YZJSERIALNUMBER)
		{
			JObject jobject = new JObject();
			jobject["FormId"] = "BD_MATERIAL";
			jobject["FieldKeys"] = "F_KING_YZJSERIALNUMBER";
			jobject["FilterString"] = "F_KING_YZJSERIALNUMBER = '" + F_KING_YZJSERIALNUMBER + "'";
			return WebApiServiceCall.ExecuteBillQuery(context, Convert.ToString(jobject)).Count;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00003100 File Offset: 0x00001300
		public string SelectUnitNumber(Context context, string FBaseUnitId)
		{
			JObject jobject = new JObject();
			jobject["FormId"] = "BD_UNIT";
			jobject["FieldKeys"] = "FNUMBER";
			jobject["FilterString"] = "FNAME = '" + FBaseUnitId + "' AND FForbidStatus  = 'A'";
			return WebApiServiceCall.ExecuteBillQuery(context, Convert.ToString(jobject))[0][0].ToString();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00003184 File Offset: 0x00001384
		public string SelectYzjUserId(Context context, string FUserName)
		{
			string result = null;
			JObject jobject = new JObject();
			jobject["FormId"] = "SEC_XTUser";
			jobject["FieldKeys"] = "FOpenID";
			jobject["FilterString"] = "FUserName = '" + FUserName + "'";
			List<List<object>> list = WebApiServiceCall.ExecuteBillQuery(context, jobject.ToString());
			bool flag = list != null && list.Count > 0;
			if (flag)
			{
				result = list[0][0].ToString();
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00003224 File Offset: 0x00001424
		public int SelectYwy(Context context, string FName)
		{
			JObject jobject = new JObject();
			jobject["FormId"] = "BD_OPERATOR";
			jobject["FieldKeys"] = "FOPERATORTYPE";
			jobject["FilterString"] = "FNAME = '" + FName + "'AND FOPERATORTYPE = 'WLY'";
			List<List<object>> list = WebApiServiceCall.ExecuteBillQuery(context, jobject.ToString());
			bool flag = list != null && list.Count > 0;
			int result;
			if (flag)
			{
				result = list.Count;
			}
			else
			{
				result = 0;
			}
			return result;
		}
	}
}
