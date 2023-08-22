using DuoVia.FuzzyStrings;
using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Helper.Enum.Status;

namespace Helper.Model
{
    public class MakerSelection
    {
        #region
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        #endregion
        #region
        public MakerSelection(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }
        #endregion
        #region
        public string GetMFGCode(string maker)
        {
            if (!String.IsNullOrEmpty(maker))
            {
                //maker mapping
                #region maker manipulation
                string MFGCode = "";
                string matchMFECode = "";
                string matchmaker = "";
                string MLLogic = this._Configuration.GetSection("MLLogicMaker")["MLAllow"];
                var lstmaker = (from m in _datacontext.M_MAKERTable
                                where m.IS_ACTIVE == 1
                                select new
                                {
                                    mfgCode = m.MAKER_CODE == null ? "" : m.MAKER_CODE,
                                    makername = m.MAKER_NAME == null ? "" : m.MAKER_NAME,
                                }).Distinct().ToList();
                double percentage = 0;
                bool firstWordMaker = false;
                bool secondWordMaker = false;
                string matchingPercent = this._Configuration.GetSection("MatchingPercentforMaker")["MATCHINGVALUE"];
                // naniwa pump
                //**** 
                string updatedmaker = maker.Replace(".", " ");
                string[] makerSplit = maker.Trim().Split(" ");
                if (MLLogic == "YES")
                {
                    if (lstmaker != null)
                    {
                        if (makerSplit.Count() > 0)
                        {
                            for (int counter = 0; counter < lstmaker.Count; counter++)
                            {
                                var lcs = makerSplit[0].Trim().ToUpper().LongestCommonSubsequence(lstmaker[counter].makername.Trim().ToUpper());
                                if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                {
                                    percentage = lcs.Item2;
                                    MFGCode = lstmaker[counter].mfgCode;
                                    firstWordMaker = true;
                                }
                            }
                        }
                        System.Diagnostics.Trace.WriteLine(makerSplit[0].Count());
                        Console.WriteLine(makerSplit[0].Count());
                        Console.WriteLine((makerSplit[0].Length));
                        System.Diagnostics.Trace.WriteLine("count of maker" + makerSplit.Count());
                        if (makerSplit.Count() > 1)
                        {
                            for (int counter = 0; counter < lstmaker.Count; counter++)
                            {
                                System.Diagnostics.Trace.WriteLine(matchmaker);
                                matchmaker = makerSplit[0] + " " + makerSplit[1];
                                var lcs = matchmaker.Trim().ToUpper().LongestCommonSubsequence(lstmaker[counter].makername.Trim().ToUpper());
                                if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                {
                                    percentage = lcs.Item2;
                                    MFGCode = lstmaker[counter].mfgCode;
                                    secondWordMaker = true;
                                }
                            }
                        }
                    }
                }
                #region
                if (firstWordMaker && secondWordMaker)
                {
                    matchMFECode = MFGCode;
                }
                else if (firstWordMaker && makerSplit.Count() == 1)
                {
                    matchMFECode = MFGCode;
                }
                else if (updatedmaker.ToUpper().Contains("SHINKO"))
                {
                    matchMFECode = "S506";
                }
                else if (updatedmaker.ToUpper().Contains("TAIKO"))
                {
                    matchMFECode = "T403";
                }
                else if (updatedmaker.ToUpper().Contains("FUKUI"))
                {
                    matchMFECode = "F434";
                }
                else if (updatedmaker.ToUpper().Contains("NANIWA"))
                {
                    matchMFECode = "N427";
                }
                else if (updatedmaker.ToUpper().Contains("AKASAKA"))
                {
                    matchMFECode = "A412";
                }
                else if (updatedmaker.ToUpper().Contains("HEISHIN"))
                {
                    matchMFECode = "H422";
                }
                else if (updatedmaker.ToUpper().Contains("NAKAKITA"))
                {
                    matchMFECode = "N417";
                }
                else if (updatedmaker.ToUpper().Contains("YANMAR"))
                {
                    matchMFECode = "Y421";
                }
                else if (updatedmaker.ToUpper().Contains("TEIKOKU"))
                {
                    matchMFECode = "T440";
                }
                else if (updatedmaker.ToUpper().Contains("MIURA"))
                {
                    matchMFECode = "M474";
                }
                else if (updatedmaker.ToUpper().Contains("MAKITA"))
                {
                    matchMFECode = "M407";
                }
                else if (updatedmaker.ToUpper().Contains("VOLCANO"))
                {
                    matchMFECode = "V402";
                }
                else if (updatedmaker.ToUpper().Contains("SASAKURA"))
                {
                    matchMFECode = "S453";
                }
                else if (updatedmaker.ToUpper().Contains("TANABE"))
                {
                    matchMFECode = "T431";
                }
                else if (updatedmaker.ToUpper().Contains("JAPAN ENGINE"))
                {
                    matchMFECode = "J169";
                }
                else if (updatedmaker.ToUpper().Contains("KOBE DIESEL"))
                {
                    matchMFECode = "K465";
                }
                else if (updatedmaker.ToUpper().Contains("MITSUBISHI KAKOKI"))
                {
                    matchMFECode = "M461";
                }
                else if (updatedmaker.ToUpper().Contains("KANAGAWA KIKI"))
                {
                    matchMFECode = "K409";
                }
                else if (updatedmaker.ToUpper().Contains("DAIHATSU"))
                {
                    matchMFECode = "D404";
                }
                else if (updatedmaker.ToUpper().Contains("NIPPON"))
                {
                    matchMFECode = "N440";
                }
                else if (updatedmaker.ToUpper().Contains("THE NIPPON"))
                {
                    matchMFECode = "N440";
                }
                #endregion
                return matchMFECode;
                #endregion unit manipulation
                //end unit mapping
            }
            else
            {
                return maker;
            }
        }
        #endregion

        #region
        public string GetMFGCodeWithSwitchCase(string maker)
        {
            if (!String.IsNullOrEmpty(maker))
            {
                //maker mapping
                #region maker manipulation
                string MFGCode = "";
                string matchMFECode = "";
                string matchmaker = "";
                string MLLogic = this._Configuration.GetSection("MLLogicMaker")["MLAllow"];
                var lstmaker = (from m in _datacontext.M_MAKERTable
                                where m.IS_ACTIVE == 1
                                select new
                                {
                                    mfgCode = m.MAKER_CODE == null ? "" : m.MAKER_CODE,
                                    makername = m.MAKER_NAME == null ? "" : m.MAKER_NAME,
                                }).Distinct().ToList();
                double percentage = 0;
                bool firstWordMaker = false;
                bool secondWordMaker = false;
                string matchingPercent = this._Configuration.GetSection("MatchingPercentforMaker")["MATCHINGVALUE"];
                // naniwa pump
                //**** 
                string updatedmaker = maker.Replace(".", " ");
                string[] makerSplit = maker.Trim().Split(" ");
                if (MLLogic == "YES")
                {
                    if (lstmaker != null)
                    {
                        if (makerSplit.Count() > 0)
                        {
                            for (int counter = 0; counter < lstmaker.Count; counter++)
                            {
                                var lcs = makerSplit[0].Trim().ToUpper().LongestCommonSubsequence(lstmaker[counter].makername.Trim().ToUpper());
                                if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                {
                                    percentage = lcs.Item2;
                                    MFGCode = lstmaker[counter].mfgCode;
                                    firstWordMaker = true;
                                }
                            }
                        }
                        System.Diagnostics.Trace.WriteLine(makerSplit[0].Count());
                        Console.WriteLine(makerSplit[0].Count());
                        Console.WriteLine((makerSplit[0].Length));
                        System.Diagnostics.Trace.WriteLine("count of maker" + makerSplit.Count());
                        if (makerSplit.Count() > 1)
                        {
                            for (int counter = 0; counter < lstmaker.Count; counter++)
                            {
                                System.Diagnostics.Trace.WriteLine(matchmaker);
                                matchmaker = makerSplit[0] + " " + makerSplit[1];
                                var lcs = matchmaker.Trim().ToUpper().LongestCommonSubsequence(lstmaker[counter].makername.Trim().ToUpper());
                                if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                {
                                    percentage = lcs.Item2;
                                    MFGCode = lstmaker[counter].mfgCode;
                                    secondWordMaker = true;
                                }
                            }
                        }
                    }
                }
                if (firstWordMaker && secondWordMaker)
                {
                    matchMFECode = MFGCode;
                }
                else if (firstWordMaker && makerSplit.Count() == 1)
                {
                    matchMFECode = MFGCode;
                }
                switch (updatedmaker)
                {
                    case var s when updatedmaker.ToUpper().StartsWith("SHINKO"):
                        matchMFECode = "S506";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("TAIKO"):
                        matchMFECode = "T403";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("FUKUI"):
                        matchMFECode = "F434";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("NANIWA"):
                        matchMFECode = "N427";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("AKASAKA"):
                        matchMFECode = "A412";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("HEISHIN"):
                        matchMFECode = "H422";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("NAKAKITA"):
                        matchMFECode = "N417";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("YANMAR"):
                        matchMFECode = "Y421";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("TEIKOKU"):
                        matchMFECode = "T440";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("MIURA"):
                        matchMFECode = "M474";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("MAKITA"):
                        matchMFECode = "M407";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("VOLCANO"):
                        matchMFECode = "V402";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("SASAKURA"):
                        matchMFECode = "S453";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("TANABE"):
                        matchMFECode = "T431";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("JAPAN ENGINE"):
                        matchMFECode = "J169";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("KOBE DIESEL"):
                        matchMFECode = "K465";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("MITSUBISHI KAKOKI"):
                        matchMFECode = "M461";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("KANAGAWA KIKI"):
                        matchMFECode = "K409";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("DAIHATSU"):
                        matchMFECode = "D404";
                        break;
                    case var s when updatedmaker.ToUpper().StartsWith("NIPPON"):
                        matchMFECode = "N440";
                        break;
                    default:
                        string matchedmaker = matchMFECode;
                        matchMFECode = matchedmaker;
                        break;
                }
                return matchMFECode;
                #endregion unit manipulation
                //end unit mapping
            }
            else
            {
                return maker;
            }
        }
        #endregion
    
 }
}
