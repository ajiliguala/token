using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Kingdee.BOS;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Log;
using Kingdee.BOS.NumFormatTran;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.FileServer;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000022 RID: 34
	public class serviceHelper
	{
		// Token: 0x0600006A RID: 106 RVA: 0x0000A9BC File Offset: 0x00008BBC
		public static apiParameter getParameterByDbid(string dbid)
		{
			apiParameter apiParameter = new apiParameter();
			bool flag = dbid == "5fab3ab2d43a29";
			if (flag)
			{
				apiParameter.dbid = "5fab3ab2d43a29";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "216487_T13u0asG0qlVSeSu353NQ9yuRt56RNOP";
				apiParameter.appSecret = "4d2719274c7940f580dd185b57a68537";
			}
			bool flag2 = dbid == "20190918173757209";
			if (flag2)
			{
				apiParameter.dbid = "20190918173757209";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "216488_TZ9JS/us4uA459Uo0e4tz8TERL57TNPI";
				apiParameter.appSecret = "d4118a74927744148818efda12b941c8";
			}
			bool flag3 = dbid == "6256656203ab91";
			if (flag3)
			{
				apiParameter.dbid = "6256656203ab91";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "216488_TZ9JS/us4uA459Uo0e4tz8TERL57TNPI";
				apiParameter.appSecret = "d4118a74927744148818efda12b941c8";
			}
			bool flag4 = dbid == "610916e71f047f";
			if (flag4)
			{
				apiParameter.dbid = "610916e71f047f";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "216924_6c7oXzFO4Mg5RWwsTd7DTYzMzgR8TOlt";
				apiParameter.appSecret = "d4ac4b7aec854a429244ae6a794a12a0";
			}
			bool flag5 = dbid == "6317390df33fc2";
			if (flag5)
			{
				apiParameter.dbid = "6317390df33fc2";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "234079_WddI29ipyJhbQWUFXZ6P6aXG7v4b6sKv";
				apiParameter.appSecret = "625e2880df5845388468eb4e38386548";
			}
			bool flag6 = dbid == "6342610e1fecb9";
			if (flag6)
			{
				apiParameter.dbid = "6342610e1fecb9";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "235310_w46NW6EE1MC+6XUE5eQoR6UMVr0YQClv";
				apiParameter.appSecret = "7bec1fb563024a478f803aa926149dc3";
			}
			bool flag7 = dbid == "63a879849efb12";
			if (flag7)
			{
				apiParameter.dbid = "63a879849efb12";
				apiParameter.apiuser = "administrator";
				apiParameter.appid = "239911_558oxyDEUlG+6bxKR/StRbzuQJx8xCoF";
				apiParameter.appSecret = "5067a773cfe04eb6ac99fe2c45ace31a";
			}
			bool flag8 = dbid == "63b380a2925160";
			if (flag8)
			{
				apiParameter.dbid = "63b380a2925160";
				apiParameter.apiuser = "李凯";
				apiParameter.appid = "242125_Td2BW6iL4nAe681p4+xPydyH2qx9RCkO";
				apiParameter.appSecret = "14a85fb14dbb4ac9a84ae5b74c30ef8f";
			}
			bool flag9 = dbid == "645db677cc0765";
			if (flag9)
			{
				apiParameter.dbid = "645db677cc0765";
				apiParameter.apiuser = "系统接口账号";
				apiParameter.appid = "243306_S+9CS8itUkoX4b1u0c5s1x1LTq06XBNE";
				apiParameter.appSecret = "505e56f710c74b0aaf926ce3ca65ffad";
			}
			return apiParameter;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0000AC08 File Offset: 0x00008E08
		public static string getLocName(Context ctx, string LocId)
		{
			string text = "select vbfl.FNAME from( select FID,FF100001+FF100002+FF100003+FF100004+FF100005 AS FLOCID from T_BAS_FlexValuesDetail\r\n                           where FID='" + LocId + "' ) A left join V_BAS_FLEXVALUESENTRY_L vbfl on vbfl.FENTRYID=A.FLOCID and vbfl.FLOCALEID=2052";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			string result;
			if (flag)
			{
				result = Convert.ToString(dynamicObjectCollection[0]["FNAME"]);
			}
			else
			{
				result = "";
			}
			return result;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x0000AC70 File Offset: 0x00008E70
		public static void getSubByBom(Context ctx, ref JArray Jarray, string materialid, string bomid, decimal qty, string groupid)
		{
			string text = string.Empty;
			bool flag = bomid.Equals("0");
			if (flag)
			{
				text = " select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                            CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                            from T_ENG_BOMCHILD  A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID\r\n\t\t\t\t\t\t\tINNER JOIN (SELECT FID,ROW_NUMBER() OVER (PARTITION BY fmaterialid ORDER BY fnumber DESC) AS sx \r\n                            from T_ENG_BOM where FDOCUMENTSTATUS='C'  and FFORBIDSTATUS='A' and fmaterialid=" + materialid + ") C on C.FID=B.FID and C.sx=1";
			}
			else
			{
				text = "select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                           CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                           from T_ENG_BOMCHILD A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID where A.FID=  " + bomid;
			}
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag2 = dynamicObjectCollection.Count > 0;
			if (flag2)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text2 = dynamicObject["FMATERIALID"].ToString();
					decimal num = Convert.ToDecimal(dynamicObject["FQTY"]);
					decimal num2 = num * qty;
					string text3 = groupid + "." + dynamicObject["FREPLACEGROUP"].ToString();
					JObject jobject = new JObject();
					jobject["FREPLACEGROUP"] = text3;
					jobject["FMATERIALTYPE"] = dynamicObject["FMATERIALTYPE"].ToString();
					JObject jobject2 = new JObject();
					jobject2["FMATERIALID"] = text2;
					jobject["FSUBMATERIALID"] = jobject2;
					JObject jobject3 = new JObject();
					jobject3["FUNITID"] = dynamicObject["FUNITID"].ToString();
					jobject["FUNITID"] = jobject3;
					jobject["FQTY"] = num;
					jobject["FALLQTY"] = num2;
					string bomid2 = dynamicObject["FBOMID"].ToString();
					serviceHelper.getSubByBom(ctx, ref Jarray, text2, bomid2, num2, text3, jobject);
				}
			}
			else
			{
				bool flag3 = groupid.Equals("1");
				if (flag3)
				{
					JObject jobject4 = new JObject();
					jobject4["FREPLACEGROUP"] = groupid;
					JObject jobject5 = new JObject();
					jobject5["FMATERIALID"] = materialid;
					jobject4["FSUBMATERIALID"] = jobject5;
					jobject4["FALLQTY"] = qty;
					Jarray.Add(jobject4);
				}
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x0000AEE8 File Offset: 0x000090E8
		public static void getSubByBom(Context ctx, ref JArray Jarray, string materialid, string bomid, decimal qty, string groupid, JObject obj)
		{
			string text = string.Empty;
			bool flag = bomid.Equals("0");
			if (flag)
			{
				text = " select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                            CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                            from T_ENG_BOMCHILD  A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID\r\n\t\t\t\t\t\t\tINNER JOIN (SELECT FID,ROW_NUMBER() OVER (PARTITION BY fmaterialid ORDER BY fnumber DESC) AS sx \r\n                            from T_ENG_BOM where FDOCUMENTSTATUS='C'  and FFORBIDSTATUS='A' and fmaterialid=" + materialid + ") C on C.FID=B.FID and C.sx=1";
			}
			else
			{
				text = "select A.FMATERIALID,A.FREPLACEGROUP,A.FUNITID,A.FMATERIALTYPE,B.FISSKIP,\r\n                           CASE WHEN A.FDENOMINATOR = 0 or A.FMATERIALTYPE!=1 THEN 0 ELSE A.FNUMERATOR/A.FDENOMINATOR END AS FQTY,A.FBOMID\r\n                           from T_ENG_BOMCHILD A inner join  T_ENG_BOMCHILD_A B ON A.FENTRYID=B.FENTRYID where A.FID=  " + bomid;
			}
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag2 = dynamicObjectCollection.Count > 0;
			if (flag2)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text2 = dynamicObject["FMATERIALID"].ToString();
					decimal num = Convert.ToDecimal(dynamicObject["FQTY"]);
					decimal num2 = num * qty;
					string text3 = groupid + "." + dynamicObject["FREPLACEGROUP"].ToString();
					JObject jobject = new JObject();
					jobject["FREPLACEGROUP"] = text3;
					jobject["FMATERIALTYPE"] = dynamicObject["FMATERIALTYPE"].ToString();
					JObject jobject2 = new JObject();
					jobject2["FMATERIALID"] = text2;
					jobject["FSUBMATERIALID"] = jobject2;
					JObject jobject3 = new JObject();
					jobject3["FUNITID"] = dynamicObject["FUNITID"].ToString();
					jobject["FUNITID"] = jobject3;
					jobject["FQTY"] = num;
					jobject["FALLQTY"] = num2;
					string bomid2 = dynamicObject["FBOMID"].ToString();
					serviceHelper.getSubByBom(ctx, ref Jarray, text2, bomid2, num2, text3, jobject);
				}
			}
			else
			{
				Jarray.Add(obj);
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x0000B0F4 File Offset: 0x000092F4
		public static string CreteItem(Context ctx, string fid)
		{
			string text = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("/*dialect*/select tpp.FBILLTYPEID,tpp.FPURCHASEORGID,tpp.F_BZ,tpp.FBILLNO,tbs.FNUMBER,tbsl.FNAME,tbs.F_KING_SFTBQZXT,tbsa.FADDRESS,tbsa.FSOCIALCRECODE,\r\n                                tbsc.FCONTACT,tbsc.FTEL,tbsc.FFAX,tpp.F_PIKU_QYDD,CONVERT(varchar(100), tpp.FCREATEDATE, 111) as FCREATEDATE,tbcl.FNAME as fcurrencyname,\r\n                                tpf.FBILLALLAMOUNT, case when TPP.F_PIKU_SFHYF='1' then '是' else '否' END AS F_PIKU_SFHYF,tpp.F_PIKU_JHDD,ysfs.FNAME as FYSFS,\r\n                                tbpl.fname as FFKTJ,tpp.F_PIKU_JHYQ,jgtk.FNAME as FJGTK,vb.FNAME as fsellname  \r\n                                from t_PUR_POOrder tpp inner join T_PUR_POORDERFIN tpf on tpf.FID=tpp.FID\r\n                                inner join T_BD_SUPPLIER tbs on tpp.FSUPPLIERID=tbs.FSUPPLIERID\r\n                                inner join T_BD_SUPPLIER_L tbsl on tbsl.FSUPPLIERID=tbs.FSUPPLIERID and tbsl.FLOCALEID=2052\r\n                                inner join t_BD_SupplierBase tbsa on tbsa.FSUPPLIERID=tbs.FSUPPLIERID\r\n                                inner join T_BD_CURRENCY_L tbcl on tbcl.FCURRENCYID=tpf.FSETTLECURRID and tbcl.FLOCALEID=2052\r\n                                left join t_BD_SupplierContact tbsc on tbsc.FContactId=tpp.FPROVIDERCONTACTID\r\n                                left join CX_TRANSMODE_L ysfs on ysfs.FID=tpp.F_TRANSMODE and ysfs.FLocaleID=2052\r\n                                left join T_BD_PAYMENTCONDITION_L tbpl on tbpl.FID=tpf.FPAYCONDITIONID and tbpl.FLOCALEID=2052\r\n                                left join CX_PRICETERMS_L jgtk on jgtk.FID=tpp.F_PRICETERMS and jgtk.FLOCALEID=2052\r\n                                left join V_BD_BUYER_L vb on vb.fid=tpp.FPURCHASERID and vb.FLOCALEID=2052\r\n                                where tpp.FID=" + fid);
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, stringBuilder.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			DynamicObject dynamicObject = dynamicObjectCollection[0];
			string text2 = dynamicObject["FBILLNO"].ToString();
			bool flag = text2.StartsWith("WHCX-2019");
			if (flag)
			{
				text = "WHCX-2019订单不允许签章！";
			}
			else
			{
				string text3 = dynamicObject["FBILLTYPEID"].ToString();
				string text4 = dynamicObject["FPURCHASEORGID"].ToString();
				string text5 = " select A.F_KING_MBID,A.F_KING_SUBID,B.FNAME as swzj,C.FNAME as swfz,A.F_KING_QZID,A.F_KING_ACCOUNT,A.F_KING_NUMBER from KING_QZXGCSENTRY A  left join T_SEC_USER B on A.F_KING_SWZJ=B.FUSERID";
				text5 = string.Concat(new string[]
				{
					text5,
					" left join T_SEC_USER C on C.FUSERID=A.F_KING_SWFZ where A.F_KING_BILLTYPE='",
					text3,
					"' and A.F_KING_ORGID='",
					text4,
					"'"
				});
				DynamicObjectCollection dynamicObjectCollection2 = DBServiceHelper.ExecuteDynamicObject(ctx, text5, null, null, CommandType.Text, Array.Empty<SqlParam>());
				bool flag2 = dynamicObjectCollection2.Count > 0;
				if (flag2)
				{
					string text6 = dynamicObjectCollection2[0]["F_KING_ACCOUNT"].ToString();
					string text7 = dynamicObjectCollection2[0]["F_KING_NUMBER"].ToString();
					JObject jobject = new JObject();
					jobject["code"] = text2;
					jobject["loginName"] = text6;
					jobject["name"] = text2;
					jobject["serviceId"] = "1";
					jobject["ctCode"] = text2;
					jobject["templateId"] = dynamicObjectCollection2[0]["F_KING_MBID"].ToString();
					jobject["remarks"] = dynamicObject["F_BZ"].ToString();
					jobject["formSource"] = "3";
					JArray jarray = new JArray();
					JObject jobject2 = new JObject();
					jobject2["signsort"] = 1;
					jobject2["keyword"] = "PartyAName";
					jobject2["orgNumber"] = text7;
					jarray.Add(jobject2);
					bool flag3 = dynamicObject["F_KING_SFTBQZXT"].ToString().Equals("1");
					if (flag3)
					{
						JObject jobject3 = new JObject();
						jobject3["signsort"] = 2;
						jobject3["keyword"] = "PartyBName";
						bool flag4 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FSOCIALCRECODE"]);
						if (flag4)
						{
							text = "供应商统一社会信用代码没有维护！";
							goto IL_170C;
						}
						string text8 = dynamicObject["FSOCIALCRECODE"].ToString();
						jobject3["orgNumber"] = text8;
						jarray.Add(jobject3);
					}
					jobject["subscriberList"] = jarray;
					JArray jarray2 = new JArray();
					JObject jobject4 = new JObject();
					jobject4["keyword"] = "seller";
					jobject4["value"] = dynamicObject["FNAME"].ToString();
					jarray2.Add(jobject4);
					JObject jobject5 = new JObject();
					jobject5["keyword"] = "table1";
					string text9 = string.Empty;
					JArray jarray3 = new JArray();
					StringBuilder stringBuilder2 = new StringBuilder();
					stringBuilder2.Append("/*dialect*/select tppy.FSEQ,tbm.FNUMBER,tbml.FNAME,CONCAT(tbml.FSPECIFICATION,ISNULL(tbay.FDATAVALUE,'')) FSPECIFICATION,\r\n                                    tbul.FNAME as ftbulname,tppf.FTAXRATE,tppy.FQTY,tppf.FTAXPRICE,tppf.FALLAMOUNT,tools.FNAME as ftoolname,tppd.FDELIVERYDATE\r\n                                    from t_PUR_POOrderentry tppy\r\n                                    inner join T_PUR_POORDERENTRY_F tppf on tppf.FENTRYID=tppy.FENTRYID\r\n                                    inner join T_PUR_POORDERENTRY_D tppd on tppd.FENTRYID=tppy.FENTRYID\r\n                                    inner join T_BD_MATERIAL tbm on tbm.FMATERIALID=tppy.FMATERIALID\r\n                                    inner join T_BD_MATERIAL_L tbml on tbml.FMATERIALID=tbm.FMATERIALID and tbml.FLOCALEID=2052\r\n                                    inner join T_BD_UNIT_L tbul on tbul.FUNITID=tppy.FUNITID and tbul.FLOCALEID=2052\r\n                                    inner join T_ORG_ORGANIZATIONS_L tools on tools.FORGID=tppd.FREQUIREORGID and tools.FLOCALEID=2052\r\n                                    left join T_BD_FLEXSITEMDETAILV tbf on tbf.FID=tppy.FAUXPROPID\r\n\t\t\t\t\t\t\t\t\tleft join T_BAS_ASSISTANTDATAENTRY_L tbay on tbay.FENTRYID=tbf.FF100005 and tbay.FLOCALEID=2052 \r\n                                    where tppy.fid=" + fid);
					DynamicObjectCollection dynamicObjectCollection3 = DBServiceHelper.ExecuteDynamicObject(ctx, stringBuilder2.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
					bool flag5 = dynamicObjectCollection3.Count > 0;
					if (flag5)
					{
						foreach (DynamicObject dynamicObject2 in dynamicObjectCollection3)
						{
							decimal d = Convert.ToDecimal(dynamicObject2["FALLAMOUNT"]);
							text9 = Math.Round(Convert.ToDecimal(dynamicObject2["FTAXRATE"]), 2).ToString() + "%";
							JObject jobject6 = new JObject();
							jobject6["row"] = Convert.ToInt32(dynamicObject2["FSEQ"]);
							JArray jarray4 = new JArray();
							JObject jobject7 = new JObject();
							jobject7["keyword"] = "item";
							jobject7["value"] = dynamicObject2["FSEQ"].ToString();
							jarray4.Add(jobject7);
							JObject jobject8 = new JObject();
							jobject8["keyword"] = "code";
							jobject8["value"] = dynamicObject2["FNUMBER"].ToString();
							jarray4.Add(jobject8);
							JObject jobject9 = new JObject();
							jobject9["keyword"] = "description";
							jobject9["value"] = dynamicObject2["FNAME"].ToString().Replace("\t", "");
							jarray4.Add(jobject9);
							JObject jobject10 = new JObject();
							jobject10["keyword"] = "quality";
							jobject10["value"] = dynamicObject2["FSPECIFICATION"].ToString().Replace("\t", "");
							jarray4.Add(jobject10);
							JObject jobject11 = new JObject();
							jobject11["keyword"] = "unit";
							jobject11["value"] = dynamicObject2["ftbulname"].ToString();
							jarray4.Add(jobject11);
							JObject jobject12 = new JObject();
							jobject12["keyword"] = "qty";
							jobject12["value"] = Math.Round(Convert.ToDecimal(dynamicObject2["FQTY"]), 2).ToString();
							jarray4.Add(jobject12);
							JObject jobject13 = new JObject();
							jobject13["keyword"] = "price";
							jobject13["value"] = Math.Round(Convert.ToDecimal(dynamicObject2["FTAXPRICE"]), 6).ToString();
							jarray4.Add(jobject13);
							JObject jobject14 = new JObject();
							jobject14["keyword"] = "total";
							jobject14["value"] = Math.Round(d, 2).ToString();
							jarray4.Add(jobject14);
							JObject jobject15 = new JObject();
							jobject15["keyword"] = "dep";
							jobject15["value"] = dynamicObject2["ftoolname"].ToString();
							jarray4.Add(jobject15);
							JObject jobject16 = new JObject();
							jobject16["keyword"] = "Arrdate";
							jobject16["value"] = dynamicObject2["FDELIVERYDATE"].ToString();
							jarray4.Add(jobject16);
							jobject6["cells"] = jarray4;
							jarray3.Add(jobject6);
						}
						jobject5["value"] = jarray3;
						jarray2.Add(jobject5);
						bool flag6 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FADDRESS"]);
						if (flag6)
						{
							JObject jobject17 = new JObject();
							jobject17["keyword"] = "address";
							jobject17["value"] = dynamicObject["FADDRESS"].ToString();
							jarray2.Add(jobject17);
						}
						else
						{
							JObject jobject18 = new JObject();
							jobject18["keyword"] = "address";
							jobject18["value"] = "";
							jarray2.Add(jobject18);
						}
						bool flag7 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FCONTACT"]);
						if (flag7)
						{
							JObject jobject19 = new JObject();
							jobject19["keyword"] = "ContactPerson";
							jobject19["value"] = dynamicObject["FCONTACT"].ToString();
							jarray2.Add(jobject19);
						}
						else
						{
							JObject jobject20 = new JObject();
							jobject20["keyword"] = "ContactPerson";
							jobject20["value"] = "";
							jarray2.Add(jobject20);
						}
						bool flag8 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_PIKU_QYDD"]);
						if (flag8)
						{
							JObject jobject21 = new JObject();
							jobject21["keyword"] = "Location";
							jobject21["value"] = dynamicObject["F_PIKU_QYDD"].ToString();
							jarray2.Add(jobject21);
						}
						else
						{
							JObject jobject22 = new JObject();
							jobject22["keyword"] = "Location";
							jobject22["value"] = "";
							jarray2.Add(jobject22);
						}
						JObject jobject23 = new JObject();
						jobject23["keyword"] = "OrderNo";
						jobject23["value"] = dynamicObject["FBILLNO"].ToString();
						jarray2.Add(jobject23);
						JObject jobject24 = new JObject();
						jobject24["keyword"] = "Date";
						jobject24["value"] = dynamicObject["FCREATEDATE"].ToString();
						jarray2.Add(jobject24);
						bool flag9 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FTEL"]);
						if (flag9)
						{
							JObject jobject25 = new JObject();
							jobject25["keyword"] = "Tel";
							jobject25["value"] = dynamicObject["FTEL"].ToString();
							jarray2.Add(jobject25);
						}
						else
						{
							JObject jobject26 = new JObject();
							jobject26["keyword"] = "Tel";
							jobject26["value"] = "";
							jarray2.Add(jobject26);
						}
						bool flag10 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FFAX"]);
						if (flag10)
						{
							JObject jobject27 = new JObject();
							jobject27["keyword"] = "Fax";
							jobject27["value"] = dynamicObject["FFAX"].ToString();
							jarray2.Add(jobject27);
						}
						else
						{
							JObject jobject28 = new JObject();
							jobject28["keyword"] = "Fax";
							jobject28["value"] = "";
							jarray2.Add(jobject28);
						}
						string text10 = Math.Round(Convert.ToDecimal(dynamicObject["FBILLALLAMOUNT"]), 2).ToString();
						JObject jobject29 = new JObject();
						jobject29["keyword"] = "xiaoji";
						jobject29["value"] = text10;
						jarray2.Add(jobject29);
						JObject jobject30 = new JObject();
						jobject30["keyword"] = "yf";
						jobject30["value"] = dynamicObject["F_PIKU_SFHYF"].ToString();
						jarray2.Add(jobject30);
						JObject jobject31 = new JObject();
						jobject31["keyword"] = "shui";
						jobject31["value"] = text9;
						jarray2.Add(jobject31);
						JObject jobject32 = new JObject();
						jobject32["keyword"] = "grandtotal";
						jobject32["value"] = FormatTranslateUtil.Translate(new FormatTranslate
						{
							Resource = text10,
							Type = 0.ToString()
						}) + dynamicObject["fcurrencyname"].ToString();
						jarray2.Add(jobject32);
						bool flag11 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_PIKU_JHDD"]);
						if (flag11)
						{
							string text11 = "select b.FCAPTION from T_META_FORMENUMITEM a inner JOIN T_META_FORMENUMITEM_L b ON a.FENUMID=b.FENUMID AND b.FLOCALEID=2052 \r\n                                                inner join T_META_FORMENUM_L c on c.FID=a.FID and c.FLOCALEID=2052\r\n                                                where c.FNAME='交货地点' and a.FVALUE='" + dynamicObject["F_PIKU_JHDD"].ToString() + "'";
							DynamicObjectCollection dynamicObjectCollection4 = DBServiceHelper.ExecuteDynamicObject(ctx, text11, null, null, CommandType.Text, Array.Empty<SqlParam>());
							bool flag12 = dynamicObjectCollection4.Count > 0;
							if (flag12)
							{
								JObject jobject33 = new JObject();
								jobject33["keyword"] = "delivery";
								jobject33["value"] = dynamicObjectCollection4[0]["FCAPTION"].ToString();
								jarray2.Add(jobject33);
							}
							else
							{
								JObject jobject34 = new JObject();
								jobject34["keyword"] = "delivery";
								jobject34["value"] = "";
								jarray2.Add(jobject34);
							}
						}
						else
						{
							JObject jobject35 = new JObject();
							jobject35["keyword"] = "delivery";
							jobject35["value"] = "";
							jarray2.Add(jobject35);
						}
						bool flag13 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FFKTJ"]);
						if (flag13)
						{
							JObject jobject36 = new JObject();
							jobject36["keyword"] = "Payment";
							jobject36["value"] = dynamicObject["FFKTJ"].ToString();
							jarray2.Add(jobject36);
						}
						else
						{
							JObject jobject37 = new JObject();
							jobject37["keyword"] = "Payment";
							jobject37["value"] = "";
							jarray2.Add(jobject37);
						}
						bool flag14 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FYSFS"]);
						if (flag14)
						{
							JObject jobject38 = new JObject();
							jobject38["keyword"] = "Transportation";
							jobject38["value"] = dynamicObject["FYSFS"].ToString();
							jarray2.Add(jobject38);
						}
						else
						{
							JObject jobject39 = new JObject();
							jobject39["keyword"] = "Transportation";
							jobject39["value"] = "";
							jarray2.Add(jobject39);
						}
						bool flag15 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_PIKU_JHYQ"]);
						if (flag15)
						{
							JObject jobject40 = new JObject();
							jobject40["keyword"] = "Deli";
							jobject40["value"] = dynamicObject["F_PIKU_JHYQ"].ToString();
							jarray2.Add(jobject40);
						}
						else
						{
							JObject jobject41 = new JObject();
							jobject41["keyword"] = "Deli";
							jobject41["value"] = "";
							jarray2.Add(jobject41);
						}
						bool flag16 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FJGTK"]);
						if (flag16)
						{
							JObject jobject42 = new JObject();
							jobject42["keyword"] = "terms";
							jobject42["value"] = dynamicObject["FJGTK"].ToString();
							jarray2.Add(jobject42);
						}
						else
						{
							JObject jobject43 = new JObject();
							jobject43["keyword"] = "terms";
							jobject43["value"] = "";
							jarray2.Add(jobject43);
						}
						bool flag17 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["F_BZ"]);
						if (flag17)
						{
							JObject jobject44 = new JObject();
							jobject44["keyword"] = "remark";
							jobject44["value"] = dynamicObject["F_BZ"].ToString();
							jarray2.Add(jobject44);
						}
						else
						{
							JObject jobject45 = new JObject();
							jobject45["keyword"] = "remark";
							jobject45["value"] = "";
							jarray2.Add(jobject45);
						}
						bool flag18 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["fsellname"]);
						if (flag18)
						{
							JObject jobject46 = new JObject();
							jobject46["keyword"] = "Buyer";
							jobject46["value"] = dynamicObject["fsellname"].ToString();
							jarray2.Add(jobject46);
						}
						else
						{
							JObject jobject47 = new JObject();
							jobject47["keyword"] = "Buyer";
							jobject47["value"] = "";
							jarray2.Add(jobject47);
						}
						bool flag19 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection2[0]["swfz"]);
						if (flag19)
						{
							JObject jobject48 = new JObject();
							jobject48["keyword"] = "Examine";
							jobject48["value"] = dynamicObjectCollection2[0]["swfz"].ToString();
							jarray2.Add(jobject48);
						}
						else
						{
							JObject jobject49 = new JObject();
							jobject49["keyword"] = "Examine";
							jobject49["value"] = "";
							jarray2.Add(jobject49);
						}
						bool flag20 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection2[0]["swzj"]);
						if (flag20)
						{
							JObject jobject50 = new JObject();
							jobject50["keyword"] = "Examination";
							jobject50["value"] = dynamicObjectCollection2[0]["swzj"].ToString();
							jarray2.Add(jobject50);
						}
						else
						{
							JObject jobject51 = new JObject();
							jobject51["keyword"] = "Examination";
							jobject51["value"] = "";
							jarray2.Add(jobject51);
						}
						jobject["contractVariableList"] = jarray2;
						string text12 = "select A.FATTACHMENTNAME,A.FFILEID from T_BAS_ATTACHMENT A where A.FBILLTYPE='PUR_PurchaseOrder' \r\n                                     and A.FEXTNAME = '.pdf' and A.FENTRYINTERID != -1 and A.FINTERID = " + fid;
						DynamicObjectCollection dynamicObjectCollection5 = DBServiceHelper.ExecuteDynamicObject(ctx, text12, null, null, CommandType.Text, Array.Empty<SqlParam>());
						bool flag21 = dynamicObjectCollection2.Count > 0;
						if (flag21)
						{
							JArray jarray5 = new JArray();
							foreach (DynamicObject dynamicObject3 in dynamicObjectCollection5)
							{
								JObject jobject52 = new JObject();
								string userToken = serviceHelper.GetUserToken(ctx);
								string text13 = "1";
								string text14 = dynamicObject3["FFILEID"].ToString();
								string address = string.Format("{0}FileUpLoadServices/Download.aspx?fileId={1}&token={2}&nail={3}", new object[]
								{
									"http://localhost/k3cloud/",
									text14,
									userToken,
									text13
								});
								string text15;
								using (WebClient webClient = new WebClient
								{
									Encoding = Encoding.UTF8
								})
								{
									text15 = Convert.ToBase64String(webClient.DownloadData(address));
								}
								jobject52["fileName"] = dynamicObject3["FATTACHMENTNAME"].ToString();
								jobject52["fileContent"] = text15;
								jobject52["classify"] = "10";
								jarray5.Add(jobject52);
							}
							jobject["attachmentList"] = jarray5;
						}
						string text16 = JsonConvert.SerializeObject(jobject);
						string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
						string text17 = str + "/platform/contract/createContract";
						string text18 = serviceHelper.Post(text17, text16);
						serviceHelper.writeLog(ctx, "签章系统创建合同接口", dynamicObject["FBILLNO"].ToString(), text17, text16, text18, text4);
						JObject jobject53 = JObject.Parse(text18);
						string a = Convert.ToString(jobject53["code"]);
						bool flag22 = a == "0";
						if (flag22)
						{
							JObject jobject54 = JObject.Parse(Convert.ToString(jobject53["data"]));
							string text19 = Convert.ToString(jobject54["id"]);
							JArray jarray6 = jobject54["subscriberList"] as JArray;
							string text20 = string.Empty;
							bool flag23 = !ObjectUtils.IsNullOrEmpty(jarray6) && jarray6.Count > 0;
							if (flag23)
							{
								foreach (JToken jtoken in jarray6)
								{
									JObject jobject55 = (JObject)jtoken;
									string a2 = jobject55["keyword"].ToString();
									bool flag24 = a2 == "PartyAName";
									if (flag24)
									{
										text20 = jobject55["subId"].ToString();
									}
								}
							}
							string text21 = string.Concat(new string[]
							{
								" update t_PUR_POOrder set F_KING_QZSTATUS='已创建',F_KING_QZBILLID='",
								text19,
								"',F_KING_QZSUBID='",
								text20,
								"' where fid=",
								fid
							});
							DBServiceHelper.Execute(ctx, text21);
							text = serviceHelper.doSign(ctx, fid, text19, text20, dynamicObjectCollection2[0]["F_KING_QZID"].ToString(), text6, text4);
						}
						else
						{
							text = "推送签章系统报错，签章系统报错提示为：" + Convert.ToString(jobject53["message"]);
						}
					}
					else
					{
						text = "没有查到明细！";
					}
				}
				else
				{
					text = "接口表没有对应数据，请维护！";
				}
			}
			IL_170C:
			bool flag25 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text);
			if (flag25)
			{
				string text22 = " update t_PUR_POOrder set F_KING_QZSTATUS='失败',F_KING_QZMESSAGE='" + text + "' where fid=" + fid;
			}
			LogObject logObject = new LogObject
			{
				Description = string.Concat(new string[]
				{
					"采购订单内码[",
					fid,
					"]单据编号[",
					text2,
					"]创建签章订单：",
					text
				}),
				OperateName = "签章",
				ObjectTypeId = "PUR_PurchaseOrder",
				SubSystemId = "BOS",
				Environment = 3
			};
			LogServiceHelper.WriteLog(ctx, logObject);
			return text;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000C91C File Offset: 0x0000AB1C
		public static string ListCreteItem(Context ctx, string fid)
		{
			string text = " select A.FDOCUMENTSTATUS,A.F_KING_QZSTATUS,B.FBILLAMOUNT  from t_PUR_POOrder A inner join T_PUR_POORDERFIN B on A.FID=B.FID where A.FID=" + fid;
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
			string text2 = dynamicObjectCollection[0]["FDOCUMENTSTATUS"].ToString();
			bool flag = text2.ToString() == "C";
			string result;
			if (flag)
			{
				bool flag2 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection[0]["F_KING_QZSTATUS"]);
				if (flag2)
				{
					result = serviceHelper.CreteItem(ctx, fid);
				}
				else
				{
					string a = dynamicObjectCollection[0]["F_KING_QZSTATUS"].ToString();
					decimal d = Convert.ToDecimal(dynamicObjectCollection[0]["FBILLAMOUNT"]);
					bool flag3 = a == "失败" || a == "已取消";
					if (flag3)
					{
						bool flag4 = d != 0m;
						if (flag4)
						{
							result = serviceHelper.CreteItem(ctx, fid);
						}
						else
						{
							result = "当前单据金额为0，不允许推送签章系统";
						}
					}
					else
					{
						result = "当前单据状态不允许签章";
					}
				}
			}
			else
			{
				result = "单据状态未审核，不允许签章";
			}
			return result;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x0000CA34 File Offset: 0x0000AC34
		private static string Post(string postadress, string content)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(postadress);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/json";
			string result = "";
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			httpWebRequest.ContentLength = (long)bytes.Length;
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = httpWebResponse.GetResponseStream();
			using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
			{
				result = streamReader.ReadToEnd();
			}
			return result;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0000CB10 File Offset: 0x0000AD10
		private static string get(string postadress, string postValue)
		{
			string result = string.Empty;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(postadress + postValue);
			httpWebRequest.Method = "GET";
			httpWebRequest.ContentType = "application/json";
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = httpWebResponse.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			result = streamReader.ReadToEnd();
			streamReader.Close();
			responseStream.Close();
			return result;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x0000CB88 File Offset: 0x0000AD88
		public static string doSign(Context ctx, string fid)
		{
			string text = " select A.FDOCUMENTSTATUS,B.F_KING_QZID,A.F_KING_QZBILLID,A.F_KING_QZSUBID,A.F_KING_QZJKSTATUS,\r\n                            A.F_KING_QZSTATUS,B.F_KING_ACCOUNT,A.FPURCHASEORGID from  \r\n                            t_PUR_POOrder A inner join KING_QZXGCSENTRY B on A.FPURCHASEORGID=B.F_KING_ORGID and A.FBILLTYPEID=B.F_KING_BILLTYPE \r\n                            where A.fid=" + fid;
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection[0]["FDOCUMENTSTATUS"].ToString() == "C";
			string result;
			if (flag)
			{
				bool flag2 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection[0]["F_KING_QZSTATUS"]);
				if (flag2)
				{
					result = "签章系统状态异常，不允许签章";
				}
				else
				{
					bool flag3 = dynamicObjectCollection[0]["F_KING_QZSTATUS"].ToString() == "已创建";
					if (flag3)
					{
						string account = dynamicObjectCollection[0]["F_KING_ACCOUNT"].ToString();
						string orgId = dynamicObjectCollection[0]["FPURCHASEORGID"].ToString();
						bool flag4 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection[0]["F_KING_QZJKSTATUS"]);
						if (flag4)
						{
							result = serviceHelper.doSign(ctx, fid, dynamicObjectCollection[0]["F_KING_QZBILLID"].ToString(), dynamicObjectCollection[0]["F_KING_QZSUBID"].ToString(), dynamicObjectCollection[0]["F_KING_QZID"].ToString(), account, orgId);
						}
						else
						{
							bool flag5 = dynamicObjectCollection[0]["F_KING_QZJKSTATUS"].ToString() == "失败";
							if (flag5)
							{
								result = serviceHelper.doSign(ctx, fid, dynamicObjectCollection[0]["F_KING_QZBILLID"].ToString(), dynamicObjectCollection[0]["F_KING_QZSUBID"].ToString(), dynamicObjectCollection[0]["F_KING_QZID"].ToString(), account, orgId);
							}
							else
							{
								result = "当前单据状态不允许签章";
							}
						}
					}
					else
					{
						result = "当前单据状态不允许签章";
					}
				}
			}
			else
			{
				result = "单据状态未审核，不允许签章";
			}
			return result;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x0000CD68 File Offset: 0x0000AF68
		public static string doSign(Context ctx, string fid, string billid, string subId, string qzid, string account, string orgId)
		{
			string text = string.Empty;
			JObject jobject = new JObject();
			jobject["version"] = "1.0";
			jobject["loginName"] = account;
			jobject["signMode"] = "SOFT";
			jobject["sealType"] = "1";
			jobject["sealCode"] = qzid;
			JArray jarray = new JArray();
			JObject jobject2 = new JObject();
			jobject2["id"] = billid;
			jobject2["version"] = "0";
			jobject2["subId"] = subId;
			jarray.Add(jobject2);
			jobject["contractList"] = jarray;
			string text2 = JsonConvert.SerializeObject(jobject);
			string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
			string text3 = str + "/platform/contract/sign/doSign";
			string text4 = serviceHelper.Post(text3, text2);
			serviceHelper.writeLog(ctx, "签章系统签章接口", fid, text3, text2, text4, orgId);
			JObject jobject3 = JObject.Parse(text4);
			string a = Convert.ToString(jobject3["code"]);
			bool flag = a == "0";
			if (flag)
			{
				string text5 = " update t_PUR_POOrder set F_KING_QZJKSTATUS='已签章' where fid=" + fid;
				DBServiceHelper.Execute(ctx, text5);
			}
			else
			{
				text = "签章调用接口报错，提示为：" + Convert.ToString(jobject3["message"]);
				string text6 = " update t_PUR_POOrder set F_KING_QZJKSTATUS='失败',F_KING_QZMESSAGE='" + text + "' where fid=" + fid;
				DBServiceHelper.Execute(ctx, text6);
			}
			return text;
		}

		// Token: 0x06000074 RID: 116 RVA: 0x0000CF24 File Offset: 0x0000B124
		public static string cancelContract(Context ctx, string fid, string billid)
		{
			string text = "select A.F_KING_ACCOUNT,tpp.FBILLNO,tpp.FPURCHASEORGID  from t_PUR_POOrder tpp\r\n                        inner join KING_QZXGCSENTRY A  ON A.F_KING_BILLTYPE=TPP.FBILLTYPEID AND A.F_KING_ORGID=tpp.FPURCHASEORGID\r\n                        WHERE TPP.FID=" + fid;
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
			string text2 = string.Empty;
			JObject jobject = new JObject();
			jobject["reason"] = "业务系统反审核撤销";
			jobject["loginName"] = dynamicObjectCollection[0]["F_KING_ACCOUNT"].ToString();
			jobject["contractId"] = billid;
			jobject["version"] = "1.0";
			string text3 = JsonConvert.SerializeObject(jobject);
			string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
			string text4 = str + "/platform/contract/cancelContract";
			string text5 = serviceHelper.Post(text4, text3);
			serviceHelper.writeLog(ctx, "签章系统撤销接口", dynamicObjectCollection[0]["FBILLNO"].ToString(), text4, text3, text5, dynamicObjectCollection[0]["FPURCHASEORGID"].ToString());
			JObject jobject2 = JObject.Parse(text5);
			string a = Convert.ToString(jobject2["code"]);
			bool flag = a == "0";
			if (flag)
			{
				string text6 = " update t_PUR_POOrder set F_KING_QZSTATUS='已取消' where fid=" + fid;
				DBServiceHelper.Execute(ctx, text6);
				string text7 = "update  T_BAS_ATTACHMENT set F_KING_STATUS='已取消',F_KING_QZID='' where F_KING_QZID!='' and FINTERID=" + fid;
				DBServiceHelper.Execute(ctx, text7);
			}
			else
			{
				text2 = Convert.ToString(jobject2["message"]);
				string text8 = " update t_PUR_POOrder set F_KING_QZSTATUS='失败',F_KING_QZMESSAGE='" + text2 + "' where fid=" + fid;
				DBServiceHelper.Execute(ctx, text8);
			}
			return text2;
		}

		// Token: 0x06000075 RID: 117 RVA: 0x0000D0D0 File Offset: 0x0000B2D0
		public static string getContractStatus(Context ctx, DynamicObjectCollection collection)
		{
			string empty = string.Empty;
			foreach (DynamicObject dynamicObject in collection)
			{
				string postValue = dynamicObject["F_KING_QZBILLID"].ToString();
				string text = dynamicObject["fbillno"].ToString();
				string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
				string text2 = serviceHelper.get(str + "/platform/contract/getContractState/", postValue);
				JObject jobject = JObject.Parse(text2);
				string a = Convert.ToString(jobject["code"]);
				bool flag = a == "0";
				if (flag)
				{
					string a2 = Convert.ToString(jobject["data"]);
					bool flag2 = a2 == "3" || a2 == "4";
					if (flag2)
					{
						string text3 = serviceHelper.get(str + "/platform/contract/download/", postValue);
						JObject jobject2 = JObject.Parse(text3);
						string a3 = Convert.ToString(jobject2["code"]);
						bool flag3 = a3 == "0";
						if (flag3)
						{
							string text4 = dynamicObject["fid"].ToString();
							string s = Convert.ToString(jobject2["data"]);
							byte[] array = Convert.FromBase64String(s);
							JObject jobject3 = null;
							int num = array.Length;
							int num2 = 262144;
							string fileId = string.Empty;
							string fileName = "签章回传-" + text4 + ".pdf";
							string userToken = serviceHelper.GetUserToken(ctx);
							bool flag4 = num > num2;
							if (flag4)
							{
								bool flag5 = true;
								bool flag6 = num % num2 == 0;
								int num3;
								if (flag6)
								{
									num3 = num / num2;
									flag5 = false;
								}
								else
								{
									num3 = num / num2 + 1;
								}
								for (int i = 0; i < num3; i++)
								{
									bool flag7 = i == num3 - 1;
									int num4 = num2;
									bool flag8 = flag7 && flag5;
									if (flag8)
									{
										num4 = num % num2;
									}
									byte[] array2 = new byte[num4];
									Array.Copy(array, i * num2, array2, 0, num4);
									Logger.Info("getContractStatus", text + "-" + i.ToString());
									jobject3 = serviceHelper.UploadToWebSite(fileName, fileId, userToken, flag7, array2);
									bool flag9 = jobject3 == null;
									if (!flag9)
									{
										fileId = jobject3["FileId"].ToString();
									}
								}
							}
							else
							{
								Logger.Info("getContractStatus", text);
								jobject3 = serviceHelper.UploadToWebSite(fileName, fileId, userToken, true, array);
								bool flag10 = jobject3 == null;
								if (flag10)
								{
									continue;
								}
							}
							fileId = jobject3["FileId"].ToString();
							bool flag11;
							bool.TryParse(jobject3["Success"].ToString(), out flag11);
							bool flag12 = !flag11;
							if (!flag12)
							{
								string text5 = serviceHelper.SaveBillData(ctx, jobject3, text, text4);
								string text6 = "select * from T_BAS_ATTACHMENT where FBILLTYPE='PUR_PurchaseOrder' and FINTERID=" + text4;
								DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text6, null, null, CommandType.Text, Array.Empty<SqlParam>());
								string text7 = " update t_PUR_POOrder set F_KING_QZSTATUS='已完成',F_PAEZ_ATTACHMENTCOUNT=" + dynamicObjectCollection.Count.ToString() + " where fid=" + dynamicObject["fid"].ToString();
								DBServiceHelper.Execute(ctx, text7);
							}
						}
					}
				}
			}
			return empty;
		}

		// Token: 0x06000076 RID: 118 RVA: 0x0000D45C File Offset: 0x0000B65C
		public static string getSubContractStatus(Context ctx, DynamicObjectCollection collection)
		{
			string empty = string.Empty;
			foreach (DynamicObject dynamicObject in collection)
			{
				string postValue = dynamicObject["F_KING_QZID"].ToString();
				string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
				string text = serviceHelper.get(str + "/platform/contract/getContractState/", postValue);
				JObject jobject = JObject.Parse(text);
				string a = Convert.ToString(jobject["code"]);
				bool flag = a == "0";
				if (flag)
				{
					string a2 = Convert.ToString(jobject["data"]);
					bool flag2 = a2 == "3" || a2 == "4";
					if (flag2)
					{
						string text2 = serviceHelper.get(str + "/platform/contract/download/", postValue);
						JObject jobject2 = JObject.Parse(text2);
						string a3 = Convert.ToString(jobject2["code"]);
						bool flag3 = a3 == "0";
						if (flag3)
						{
							string billno = dynamicObject["FBILLNO"].ToString();
							string text3 = dynamicObject["FINTERID"].ToString();
							string s = Convert.ToString(jobject2["data"]);
							byte[] content = Convert.FromBase64String(s);
							string fileId = string.Empty;
							string fileName = "签章回传-" + dynamicObject["FATTACHMENTNAME"].ToString();
							JObject jobject3 = serviceHelper.UploadToWebSite(fileName, fileId, serviceHelper.GetUserToken(ctx), true, content);
							bool flag4 = jobject3 == null;
							if (!flag4)
							{
								fileId = jobject3["FileId"].ToString();
								bool flag5;
								bool.TryParse(jobject3["Success"].ToString(), out flag5);
								bool flag6 = !flag5;
								if (!flag6)
								{
									string text4 = serviceHelper.SaveBillData(ctx, jobject3, billno, text3);
									string text5 = "select * from T_BAS_ATTACHMENT where FBILLTYPE='PUR_PurchaseOrder' and FINTERID=" + text3;
									DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text5, null, null, CommandType.Text, Array.Empty<SqlParam>());
									string text6 = " update t_PUR_POOrder set F_PAEZ_ATTACHMENTCOUNT=" + dynamicObjectCollection.Count.ToString() + " where fid=" + dynamicObject["fid"].ToString();
									DBServiceHelper.Execute(ctx, text6);
								}
							}
						}
					}
				}
			}
			return empty;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x0000D6E4 File Offset: 0x0000B8E4
		private static string GetUserToken(Context ctx)
		{
			string dbid = ctx.DBId;
			int lcid = 2052;
			apiParameter parameterByDbid = serviceHelper.getParameterByDbid(dbid);
			string text = serviceHelper._client.Login(dbid, parameterByDbid.apiuser, parameterByDbid.appid, parameterByDbid.appSecret, lcid);
			JObject jobject = JsonConvert.DeserializeObject<JObject>(text);
			return jobject["Context"]["UserToken"].ToString();
		}

		// Token: 0x06000078 RID: 120 RVA: 0x0000D750 File Offset: 0x0000B950
		public static JObject UploadToWebSite(string fileName, string fileId, string token, bool last, byte[] content)
		{
			string url = string.Format("{0}FileUpLoadServices/FileService.svc/upload2attachment/?fileName={1}&fileId={2}&token={3}&last={4}", new object[]
			{
				"http://localhost/k3cloud/",
				HttpUtility.HtmlEncode(fileName),
				fileId,
				token,
				last
			});
			string text = serviceHelper._client.UploadData(url, fileName, content);
			JObject jobject = JsonConvert.DeserializeObject<JObject>(text);
			return jobject["Upload2AttachmentResult"] as JObject;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x0000D7BC File Offset: 0x0000B9BC
		public static string SaveBillData(Context ctx, JObject uploadResult, string billno, string id)
		{
			string formId = "BOS_Attachment";
			JObject jobject = new JObject();
			jobject.Add("Creator", "Administrator");
			jobject.Add("NeedUpDateFields", new JArray(""));
			JObject jobject2 = new JObject();
			jobject.Add("Model", jobject2);
			jobject2.Add("FID", 0);
			jobject2.Add("FBILLTYPE", "PUR_PurchaseOrder");
			jobject2.Add("FINTERID", id);
			jobject2.Add("FBILLNO", billno);
			jobject2.Add("FENTRYKEY", "\u00a0");
			jobject2.Add("FENTRYINTERID", -1);
			jobject2.Add("FFILEID", uploadResult["FileId"].ToString());
			jobject2.Add("FFILESTORAGE", FileServerHelper.GetFileStorgaeType(ctx));
			string text = uploadResult["FileName"].ToString();
			jobject2.Add("FATTACHMENTNAME", text);
			jobject2.Add("FEXTNAME", Path.GetExtension(text));
			decimal d = Convert.ToDecimal(uploadResult["FileSize"].ToString());
			jobject2.Add("FATTACHMENTSIZE", Math.Round(d / 1024m, 2));
			jobject2.Add("FBILLSTATUS", "A");
			jobject2.Add("FALIASFILENAME", "");
			jobject2.Add("FIsAllowDownLoad", false);
			JObject jobject3 = new JObject();
			jobject3.Add("FUSERID", 16394);
			JObject jobject4 = jobject3;
			jobject2.Add("FCREATEMEN", jobject4);
			jobject2.Add("FCREATETIME", DateTime.Now);
			jobject2.Add("FMODIFYMEN", jobject4);
			jobject2.Add("FMODIFYTIME", DateTime.Now);
			return serviceHelper._client.Save(formId, jobject.ToString());
		}

		// Token: 0x0600007A RID: 122 RVA: 0x0000D9F8 File Offset: 0x0000BBF8
		public static string CreateSubItem(Context ctx, string code, string mainId, string templateId, string fileId, string filenum, string filename, string password, string account, string orgnumber)
		{
			JObject jobject = new JObject();
			jobject["code"] = filenum;
			jobject["loginName"] = account;
			jobject["mainId"] = mainId;
			jobject["name"] = filename;
			jobject["serviceId"] = "1";
			jobject["ctCode"] = filenum;
			jobject["templateId"] = templateId;
			jobject["remarks"] = "";
			jobject["formSource"] = 3;
			JArray jarray = new JArray();
			JObject jobject2 = new JObject();
			jobject2["signsort"] = 1;
			jobject2["keyword"] = "甲方盖章";
			jobject2["orgNumber"] = orgnumber;
			jarray.Add(jobject2);
			JObject jobject3 = new JObject();
			jobject3["signsort"] = 2;
			jobject3["keyword"] = "乙方盖章";
			jobject3["orgNumber"] = code;
			jarray.Add(jobject3);
			jobject["subscriberList"] = jarray;
			string userToken = serviceHelper.GetUserToken(ctx);
			string text = "1";
			string address = string.Format("{0}FileUpLoadServices/Download.aspx?fileId={1}&token={2}&nail={3}", new object[]
			{
				"http://localhost/k3cloud/",
				fileId,
				userToken,
				text
			});
			string text2;
			using (WebClient webClient = new WebClient
			{
				Encoding = Encoding.UTF8
			})
			{
				text2 = Convert.ToBase64String(webClient.DownloadData(address));
			}
			jobject["fileBase64Str"] = text2;
			string content = JsonConvert.SerializeObject(jobject);
			string str = SystemParameterServiceHelper.GetParamter(ctx, 0L, 0L, "PUR_SystemParameter", "F_KING_URL", 0L).ToString();
			string text3 = serviceHelper.Post(str + "/platform/contract/createSubContract", content);
			JObject jobject4 = JObject.Parse(text3);
			string a = Convert.ToString(jobject4["code"]);
			bool flag = a == "0";
			string result;
			if (flag)
			{
				JObject jobject5 = JObject.Parse(Convert.ToString(jobject4["data"]));
				string str2 = Convert.ToString(jobject5["id"]);
				string text4 = " update T_BAS_ATTACHMENT set F_KING_STATUS='已创建',F_KING_QZID='" + str2 + "' where fid=" + filenum;
				DBServiceHelper.Execute(ctx, text4);
				result = null;
			}
			else
			{
				result = Convert.ToString(jobject4["message"]);
			}
			return result;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000DCBC File Offset: 0x0000BEBC
		public static void writeLog(Context ctx, string type, string billno, string url, string data, string result, string orgId)
		{
			K3CloudApiClient k3CloudApiClient = new K3CloudApiClient("http://localhost/k3cloud/");
			apiParameter parameterByDbid = serviceHelper.getParameterByDbid(ctx.DBId);
			string text = k3CloudApiClient.LoginByAppSecret(ctx.DBId, parameterByDbid.apiuser, parameterByDbid.appid, parameterByDbid.appSecret, 2052);
			JObject jobject = JObject.Parse(text);
			int num = Extensions.Value<int>(jobject["LoginResultType"]);
			bool flag = num == 1 || num == -5;
			if (flag)
			{
				JObject jobject2 = new JObject();
				jobject2["F_PCQE_TYPE"] = type;
				JObject jobject3 = new JObject();
				jobject3["fuserid"] = ctx.UserId;
				jobject2["F_PCQE_UserId"] = jobject3;
				jobject2["F_PCQE_Date"] = DateTime.Now;
				jobject2["F_PCQE_BILLNO"] = billno;
				jobject2["F_PCQE_URL"] = url;
				jobject2["F_DATA"] = "";
				bool flag2 = data.Length >= 9000000;
				if (flag2)
				{
					data = "json过长，不保留";
				}
				jobject2["F_DATA_Tag"] = data;
				jobject2["F_RESULT"] = "";
				bool flag3 = result.Length >= 9000000;
				if (flag3)
				{
					result = "json过长，不保留";
				}
				jobject2["F_RESULT_Tag"] = result;
				JObject jobject4 = new JObject();
				jobject4["FORGID"] = orgId;
				jobject2["F_PCQE_OrgId"] = jobject4;
				JObject jobject5 = new JObject();
				jobject5["Model"] = jobject2;
				string text2 = JsonConvert.SerializeObject(jobject5);
				string text3 = "PCQE_LOG";
				string text4 = k3CloudApiClient.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[]
				{
					text3,
					text2
				});
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000DEC0 File Offset: 0x0000C0C0
		public static int getMoBillmaterial(Context ctx, string entryId)
		{
			string text = "select * from T_PRD_MOENTRY where Fentryid='" + entryId + "'";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			int result;
			if (flag)
			{
				result = Convert.ToInt32(dynamicObjectCollection[0]["FMATERIALID"]);
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x0000DF24 File Offset: 0x0000C124
		public static DateTime getMoBillDate(Context ctx, string billno)
		{
			string text = "select * from T_PRD_MO where fbillno='" + billno + "'";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(ctx, text.ToString(), null, null, CommandType.Text, Array.Empty<SqlParam>());
			bool flag = dynamicObjectCollection.Count > 0;
			DateTime result;
			if (flag)
			{
				result = Convert.ToDateTime(dynamicObjectCollection[0]["FDATE"]);
			}
			else
			{
				result = DateTime.MinValue;
			}
			return result;
		}

		// Token: 0x04000006 RID: 6
		private const string WebSiteUrl = "http://localhost/k3cloud/";

		// Token: 0x04000007 RID: 7
		private static readonly ApiClient _client = new ApiClient("http://localhost/k3cloud/");
	}
}
