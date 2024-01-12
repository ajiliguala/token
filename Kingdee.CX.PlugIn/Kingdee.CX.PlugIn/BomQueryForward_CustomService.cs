using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.ServicesStub;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000002 RID: 2
	[HotUpdate]
	[Description("物料清单正查-自定义接口")]
	public class BomQueryForward_CustomService : AbstractWebApiBusinessService
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public BomQueryForward_CustomService(KDServiceContext context) : base(context)
		{
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
		public string Query(string number)
		{
			Context appContext = base.KDContext.Session.AppContext;
			JSONObject jsonobject = new JSONObject();
			JSONArray jsonarray = new JSONArray();
			try
			{
				bool flag = !string.IsNullOrEmpty(number);
				if (flag)
				{
					string strSQL = "select a.FUNITID,FNUMBER,FNAME from T_BD_UNIT a join T_BD_UNIT_L b on a.FUNITID=b.FUNITID where FDOCUMENTSTATUS='C'";
					DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(appContext, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>());
					Dictionary<long, string> dictionary = new Dictionary<long, string>();
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						long key = Helper.ToLong(dynamicObject["FUNITID"]);
						string value = Helper.ToStr(dynamicObject["FNAME"], 0);
						bool flag2 = !dictionary.ContainsKey(key);
						if (flag2)
						{
							dictionary.Add(key, value);
						}
					}
					strSQL = "select FCUSTMATNO,FMATERIALID from t_Sal_CustMatMappingEntry";
					DynamicObjectCollection dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(appContext, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>());
					Dictionary<long, string> dictionary2 = new Dictionary<long, string>();
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
					{
						long key2 = Helper.ToLong(dynamicObject2["FMATERIALID"]);
						string value2 = Helper.ToStr(dynamicObject2["FCUSTMATNO"], 0);
						bool flag3 = !dictionary2.ContainsKey(key2);
						if (flag3)
						{
							dictionary2.Add(key2, value2);
						}
					}
					strSQL = string.Format("/*dialect*/ select FID,a.FMATERIALID,b.FNUMBER,a.FNUMBER from (\r\n\t\t\t\t\tselect FID,FNUMBER,FMATERIALID,RANK() over(partition by FMATERIALID order by fnumber desc) xh  from T_ENG_BOM \r\n\t\t\t\t\t)a join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID where b.FNUMBER='{0}' and xh=1", number);
					DynamicObject dynamicObject3 = DBUtils.ExecuteDynamicObject(appContext, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
					bool flag4 = !dynamicObject3.IsNullOrEmptyOrWhiteSpace();
					if (flag4)
					{
						string arg = Helper.ToStr(dynamicObject3["FID"], 0);
						long pid = Helper.ToLong(dynamicObject3["FMATERIALID"]);
						strSQL = string.Format("/*dialect*/select a.FID,FROWID,FPARENTROWID,a.FNUMBER,a.FMATERIALID PID,b.FMATERIALID ZXWLID,FMATERIALTYPE ZXLX,FBOMID,b.FUNITID ZXDW,b.FFIXSCRAPQTY GDSH,b.FSCRAPRATE BDSHL,\r\n                        b.FNUMERATOR FZ,b.FDENOMINATOR FM,b.FQTY BZYL, b.FACTUALQTY SJYL,c.FISSKIP TC,c.FISKEYITEM TDZL,FEFFECTDATE, FEXPIREDATE,d.FMEMO BZ,FREPLACEGROUP\r\n                        from T_ENG_BOM a\r\n                        join T_ENG_BOMCHILD b on a.FID=b.FID\r\n                        left join T_ENG_BOMCHILD_A c on c.FENTRYID=b.FENTRYID\r\n                        left join T_ENG_BOMCHILD_L d on d.FENTRYID=b.FENTRYID\r\n                        where a.FID={0}", arg);
						DynamicObjectCollection dynamicObjects = DBUtils.ExecuteDynamicObject(appContext, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>());
						List<BomChild> list = new List<BomChild>();
						this.FillData(dynamicObjects, list, "", pid, 1);
						foreach (BomChild bomChild in list)
						{
							JSONObject jsonobject2 = new JSONObject();
							DynamicObject wlObj = this.GetWlObj(bomChild.FXWLID, appContext);
							bool flag5 = !wlObj.IsNullOrEmptyOrWhiteSpace();
							if (flag5)
							{
								jsonobject2.Add("FXWLBM", Helper.ToStr(wlObj["FNUMBER"], 0));
								jsonobject2.Add("FXWLMC", Helper.ToStr(wlObj["WLMC"], 0));
								bool flag6 = dictionary2.ContainsKey(bomChild.FXWLID);
								if (flag6)
								{
									jsonobject2.Add("KHLH", dictionary2[bomChild.FXWLID]);
								}
								else
								{
									jsonobject2.Add("KHLH", "");
								}
								jsonobject2.Add("ZJM", Helper.ToStr(wlObj["ZJM"], 0));
							}
							jsonobject2.Add("BOMCJ", bomChild.BOMCJ);
							wlObj = this.GetWlObj(bomChild.ZXWLID, appContext);
							bool flag7 = !wlObj.IsNullOrEmptyOrWhiteSpace();
							if (flag7)
							{
								jsonobject2.Add("ZXWLBM", Helper.ToStr(wlObj["FNUMBER"], 0));
								jsonobject2.Add("ZXWLMC", Helper.ToStr(wlObj["WLMC"], 0));
								jsonobject2.Add("ZXWLGGXH", Helper.ToStr(wlObj["GGXH"], 0));
								jsonobject2.Add("ZXWLSX", this.GetWlsxName(Helper.ToStr(wlObj["FERPCLSID"], 0)));
								jsonobject2.Add("FZSX", Helper.ToStr(wlObj["FZSXMC"], 0));
								jsonobject2.Add("ZXWLZJM", Helper.ToStr(wlObj["ZJM"], 0));
								jsonobject2.Add("XXKHWLBM", Helper.ToStr(wlObj["F_KING_CUSTOMML"], 0));
							}
							bool flag8 = bomChild.ZXLX == "1";
							if (flag8)
							{
								jsonobject2.Add("ZXLX", "标准件");
							}
							else
							{
								bool flag9 = bomChild.ZXLX == "2";
								if (flag9)
								{
									jsonobject2.Add("ZXLX", "返还件");
								}
								else
								{
									jsonobject2.Add("ZXLX", "替代件");
								}
							}
							jsonobject2.Add("BOMBB", bomChild.BOMBB);
							bool flag10 = dictionary.ContainsKey(bomChild.ZXDW);
							if (flag10)
							{
								jsonobject2.Add("ZXDW", dictionary[bomChild.ZXDW]);
							}
							jsonobject2.Add("GDSH", bomChild.GDSH);
							jsonobject2.Add("BDSHL", bomChild.BDSHL);
							jsonobject2.Add("FZ", bomChild.FZ);
							jsonobject2.Add("FM", bomChild.FM);
							jsonobject2.Add("BZYL", bomChild.BZYL);
							jsonobject2.Add("SJSL", bomChild.SJSL);
							jsonobject2.Add("SFTC", bomChild.SFTC);
							jsonobject2.Add("TDZL", bomChild.TDZL);
							jsonobject2.Add("FEFFECTDATE", bomChild.FEFFECTDATE);
							jsonobject2.Add("FEXPIREDATE", bomChild.FEXPIREDATE);
							jsonobject2.Add("BZ", bomChild.BZ);
							jsonarray.Add(jsonobject2);
						}
						jsonobject.Add("IsSuccess", true);
						jsonobject.Add("DATA", jsonarray);
						jsonobject.Add("Msg", "");
					}
				}
				else
				{
					jsonobject.Add("IsSuccess", false);
					jsonobject.Add("DATA", jsonarray);
					jsonobject.Add("Msg", "父项物料编码不能为空！");
				}
			}
			catch (Exception ex)
			{
				jsonobject.Add("IsSuccess", false);
				jsonobject.Add("DATA", jsonarray);
				jsonobject.Add("Msg", ex.Message);
			}
			return KDObjectConverter.SerializeObject(jsonobject);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000274C File Offset: 0x0000094C
		private DynamicObject GetWlObj(long wlid, Context ctx)
		{
			string strSQL = string.Format("/*dialect*/ select a.FMATERIALID,FNUMBER,l.FNAME WLMC,FMNEMONICCODE ZJM,l.FSPECIFICATION GGXH,d.FNAME FZSXMC,b.FERPCLSID,F_KING_CUSTOMML from T_BD_MATERIAL a \r\n                            left join T_BD_MATERIAL_L l on l.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialBase b on b.FMATERIALID=a.FMATERIALID\r\n                            left join t_BD_MaterialAuxPty c on a.FMATERIALID=c.FMATERIALID\r\n                            left join T_BD_FLEXAUXPROPERTY_l d on c.FAUXPROPERTYID=d.FID\r\n                            where a.FMATERIALID={0}", wlid);
			return DBUtils.ExecuteDynamicObject(ctx, strSQL, null, null, CommandType.Text, Array.Empty<SqlParam>()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002788 File Offset: 0x00000988
		private string GetWlsxName(string wlsx)
		{
			string result = "";
			bool flag = wlsx == "1";
			if (flag)
			{
				result = "外购";
			}
			else
			{
				bool flag2 = wlsx == "2";
				if (flag2)
				{
					result = "自制";
				}
				else
				{
					bool flag3 = wlsx == "3";
					if (flag3)
					{
						result = "委外";
					}
					else
					{
						bool flag4 = wlsx == "4";
						if (flag4)
						{
							result = "特征";
						}
						else
						{
							bool flag5 = wlsx == "5";
							if (flag5)
							{
								result = "虚拟";
							}
							else
							{
								bool flag6 = wlsx == "6";
								if (flag6)
								{
									result = "服务";
								}
								else
								{
									bool flag7 = wlsx == "7";
									if (flag7)
									{
										result = "一次性";
									}
									else
									{
										bool flag8 = wlsx == "9";
										if (flag8)
										{
											result = "配置";
										}
										else
										{
											bool flag9 = wlsx == "10";
											if (flag9)
											{
												result = "资产";
											}
											else
											{
												bool flag10 = wlsx == "11";
												if (flag10)
												{
													result = "费用";
												}
												else
												{
													bool flag11 = wlsx == "12";
													if (flag11)
													{
														result = "模型";
													}
													else
													{
														bool flag12 = wlsx == "13";
														if (flag12)
														{
															result = "产品系列";
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000028F4 File Offset: 0x00000AF4
		private void FillData(DynamicObjectCollection dynamicObjects, List<BomChild> list, string prowid, long pid, int bomcj)
		{
			foreach (DynamicObject dynamicObject in dynamicObjects)
			{
				string prowid2 = Helper.ToStr(dynamicObject["FROWID"], 0);
				string b = Helper.ToStr(dynamicObject["FPARENTROWID"], 0);
				bool flag = prowid == b;
				if (flag)
				{
					BomChild bomChild = new BomChild();
					long num = Helper.ToLong(dynamicObject["FBOMID"]);
					long num2 = Helper.ToLong(dynamicObject["ZXWLID"]);
					string zxlx = Helper.ToStr(dynamicObject["ZXLX"], 0);
					long zxdw = Helper.ToLong(dynamicObject["ZXDW"]);
					double gdsh = Helper.ToDouble(dynamicObject["GDSH"]);
					double bdshl = Helper.ToDouble(dynamicObject["BDSHL"]);
					double fz = Helper.ToDouble(dynamicObject["FZ"]);
					double fm = Helper.ToDouble(dynamicObject["FM"]);
					double bzyl = Helper.ToDouble(dynamicObject["BZYL"]);
					double sjyl = Helper.ToDouble(dynamicObject["SJYL"]);
					string a = Helper.ToStr(dynamicObject["TC"], 0);
					string a2 = Helper.ToStr(dynamicObject["TDZL"], 0);
					string feffectdate = Helper.ToDateTime(dynamicObject["FEFFECTDATE"]).ToString("yyyy-MM-dd");
					string fexpiredate = Helper.ToDateTime(dynamicObject["FEXPIREDATE"]).ToString("yyyy-MM-dd");
					string bz = Helper.ToStr(dynamicObject["BZ"], 0);
					int num3 = Helper.ToInt(dynamicObject["FREPLACEGROUP"]);
					bomChild.BOMCJ = bomcj;
					bomChild.FXWLID = pid;
					bomChild.ZXWLID = num2;
					bomChild.ZXDW = zxdw;
					bomChild.ZXLX = zxlx;
					bomChild.GDSH = gdsh;
					bomChild.BDSHL = bdshl;
					bomChild.FZ = fz;
					bomChild.FM = fm;
					bomChild.BZYL = bzyl;
					bomChild.SJYL = sjyl;
					bomChild.FEFFECTDATE = feffectdate;
					bomChild.FEXPIREDATE = fexpiredate;
					bomChild.BOMBB = Helper.ToStr(dynamicObject["FNUMBER"], 0);
					bool flag2 = a == "1";
					if (flag2)
					{
						bomChild.SFTC = "是";
					}
					else
					{
						bomChild.SFTC = "否";
					}
					bool flag3 = a2 == "1";
					if (flag3)
					{
						bomChild.TDZL = "是";
					}
					else
					{
						bomChild.TDZL = "否";
					}
					bomChild.BZ = bz;
					list.Add(bomChild);
					this.FillData(dynamicObjects, list, prowid2, num2, bomcj + 1);
				}
			}
		}
	}
}
