using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Reflection;
using RcdCmn;

namespace RcdManagement
{
    public partial class Test1 : Form
    {
        private HttpClient m_client = new HttpClient();
        private AppLog LOGGER = AppLog.GetInstance();
        public Test1()
        {
            InitializeComponent();
        }

        #region ### TESTボタンクリック ###
        private async void btnTest_Click(object sender, EventArgs e)
        {
            // ログテキストボックスクリア
            tboxLog.Clear();
            LOGGER.Info("START");
            
            // 検査情報システムから検査情報取得
            var inspectionList = await getInspection(txtBodyNo.Text);
            
            // 取得した検査情報を表示
            // 製造
            if (inspectionList.manufacture != null)
            {
                tboxLog.AppendText("manufacture\r\n");
                LOGGER.Info("manufacture");
                for (var i = 0; i < inspectionList.manufacture.Count; i++)
                {
                    var ret = GetValues(inspectionList.manufacture[i]);
                    tboxLog.AppendText(ret + "\r\n");
                    LOGGER.Info(ret);
                }
            } else
            {
                tboxLog.AppendText("manufacture null\r\n");
                LOGGER.Info("manufacture null");
            }

            // 検査
            if (inspectionList.inspect != null)
            {
                tboxLog.AppendText("inspect\r\n");
                LOGGER.Info("inspect");
                for (var i = 0; i < inspectionList.inspect.Count; i++)
                {
                    var ret = GetValues(inspectionList.inspect[i]);
                    tboxLog.AppendText(ret + "\r\n");
                    LOGGER.Info(ret);
                }
            }
            else
            {
                tboxLog.AppendText("inspect null\r\n");
                LOGGER.Info("inspect null");
            }

            // 監査
            if (inspectionList.audit != null)
            {
                tboxLog.AppendText("audit\r\n");
                LOGGER.Info("audit");
                for (var i = 0; i < inspectionList.audit.Count; i++)
                {
                    var ret = GetValues(inspectionList.audit[i]);
                    tboxLog.AppendText(ret + "\r\n");
                    LOGGER.Info(ret);
                }
            }
            else
            {
                tboxLog.AppendText("audit null\r\n");
                LOGGER.Info("audit null");
            }
        }
        #endregion

        #region ### 検査情報システムから検査情報取得 ###
        public async Task<InspectionList> getInspection(string p_bodyno)
        {
            InspectionList inspectionList = new InspectionList();
            try
            {
                var w_URL = "";
                // URLテキストに入力がある場合、それを優先して使用する
                if (txtURL.Text == "")
                {
                    if (cbDebug.Checked == false) {
                        // 対象bodyNo未入力時はエラー
                        if (txtBodyNo.Text == "")
                        {
                            tboxLog.AppendText("対象bodyNoが未入力です。\r\n");
                            LOGGER.Info("対象bodyNoが未入力です。");
                            return inspectionList;
                        }
                        // 規定URL生成
                        w_URL = "http://kensa/vehicle/" + p_bodyno + "/bug";
                    }
                    else
                    {
                        // デバッグ環境用URL生成
                        w_URL = "http://localhost/test/111.html";
                    }
                } else
                {
                    // 入力されたURLを使用
                    w_URL = txtURL.Text;
                }
                
                tboxLog.AppendText("GET=" + w_URL + "\r\n");
                LOGGER.Info($"GET={w_URL}");
                
                // 検査情報取得
                var response = await m_client.GetAsync(w_URL);
                tboxLog.AppendText("RESPONSE=" + response + "\r\n");
                LOGGER.Info($"RESPONSE={response}");
                var responseContent = await response.Content.ReadAsStringAsync();
                tboxLog.AppendText("CONTENT=" + responseContent + "\r\n");
                LOGGER.Info($"CONTENT={responseContent}");
                
                // JSONをデシリアライズ
                inspectionList = JsonConvert.DeserializeObject<InspectionList>(responseContent);
            }
            catch (Exception e) {
                tboxLog.AppendText(e.ToString() + "\r\n");
                LOGGER.Error("予期しないエラーが発生しました", e.ToString());
            }
            return inspectionList;
        }
        #endregion

        public string GetValues(object obj)
        {
            var result = "";
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var prop in props)
            {
                var val = prop.GetValue(obj);
                result += prop.Name + "=" + val + ",";
            }
            return result;
        }
        public class Inspection
        {
            [JsonProperty("bodyNo")]
            public string bodyNo { get; set; }
            [JsonProperty("fuguaiNo")]
            public string fuguaiNo { get; set; }
            [JsonProperty("fuguaiCode")]
            public string fuguaiCode { get; set; }
            [JsonProperty("fuguaiName")]
            public string fuguaiName { get; set; }
            [JsonProperty("fuguaiSts")]
            public int fuguaiSts { get; set; }
            [JsonProperty("tenaosiSu")]
            public int tenaosiSu { get; set; }
            [JsonProperty("tegakiKbn")]
            public int tegakiKbn { get; set; }
            [JsonProperty("termSyu")]
            public int termSyu { get; set; }
            [JsonProperty("sinkokuUmu")]
            public int sinkokuUmu { get; set; }
            [JsonProperty("sogaiKbn")]
            public int sogaiKbn { get; set; }
            [JsonProperty("barasiUmu")]
            public int barasiUmu { get; set; }
            [JsonProperty("gamenPage")]
            public string gamenPage { get; set; }
            [JsonProperty("buhinX")]
            public string buhinX { get; set; }
            [JsonProperty("buhinY")]
            public string buhinY { get; set; }
            [JsonProperty("fuguaiMark")]
            public int fuguaiMark { get; set; }
            [JsonProperty("printGrop")]
            public int printGrop { get; set; }
            [JsonProperty("todokeUmu")]
            public int todokeUmu { get; set; }
            [JsonProperty("kobunNo")]
            public int kobunNo { get; set; }
            [JsonProperty("juyoNo")]
            public string juyoNo { get; set; }
            [JsonProperty("sobetuNo")]
            public string sobetuNo { get; set; }
            [JsonProperty("kojoFlg")]
            public string kojoFlg { get; set; }
            [JsonProperty("shopCode")]
            public string shopCode { get; set; }
            [JsonProperty("kumiNo")]
            public string kumiNo { get; set; }
            [JsonProperty("optSyasyu")]
            public string optSyasyu { get; set; }
            [JsonProperty("kobunCall")]
            public string kobunCall { get; set; }
            [JsonProperty("juyoMark")]
            public string juyoMark { get; set; }
            [JsonProperty("sobetuMark")]
            public string sobetuMark { get; set; }
            [JsonProperty("kumiName")]
            public string kumiName { get; set; }
            [JsonProperty("syochiNo")]
            public string syochiNo { get; set; }
            [JsonProperty("syochiCall")]
            public string syochiCall { get; set; }
            [JsonProperty("geninCode")]
            public string geninCode { get; set; }
            [JsonProperty("geninName")]
            public string geninName { get; set; }
            [JsonProperty("iShopCode")]
            public string iShopCode { get; set; }
            [JsonProperty("iShopName")]
            public string iShopName { get; set; }
            [JsonProperty("iTermId")]
            public string iTermId { get; set; }
            [JsonProperty("iZoneKbn")]
            public int iZoneKbn { get; set; }
            [JsonProperty("iQualNo")]
            public int iQualNo { get; set; }
            [JsonProperty("iQualName")]
            public string iQualName { get; set; }
            [JsonProperty("iYymd")]
            public string iYymd { get; set; }
            [JsonProperty("iShift1")]
            public int iShift1 { get; set; }
            [JsonProperty("iShift2")]
            public int iShift2 { get; set; }
            [JsonProperty("iDate")]
            public string iDate { get; set; }
            [JsonProperty("tTermId")]
            public string tTermId { get; set; }
            [JsonProperty("tZoneKbn")]
            public int tZoneKbn { get; set; }
            [JsonProperty("tQualNo")]
            public int tQualNo { get; set; }
            [JsonProperty("tQualName")]
            public string tQualName { get; set; }
            [JsonProperty("tYymd")]
            public string tYymd { get; set; }
            [JsonProperty("tShift1")]
            public int tShift1 { get; set; }
            [JsonProperty("tShift2")]
            public int tShift2 { get; set; }
            [JsonProperty("tDate")]
            public string tDate { get; set; }
            [JsonProperty("kTermId")]
            public string kTermId { get; set; }
            [JsonProperty("kZoneKbn")]
            public int kZoneKbn { get; set; }
            [JsonProperty("kQualNo")]
            public int kQualNo { get; set; }
            [JsonProperty("kQualName")]
            public string kQualName { get; set; }
            [JsonProperty("kYymd")]
            public string kYymd { get; set; }
            [JsonProperty("kShift1")]
            public int kShift1 { get; set; }
            [JsonProperty("kShift2")]
            public int kShift2 { get; set; }
            [JsonProperty("kDate")]
            public string kDate { get; set; }
            [JsonProperty("g1ShopCode")]
            public string g1ShopCode { get; set; }
            [JsonProperty("g1ShopName")]
            public string g1ShopName { get; set; }
            [JsonProperty("g1TermId")]
            public string g1TermId { get; set; }
            [JsonProperty("g1ZoneKbn")]
            public int g1ZoneKbn { get; set; }
            [JsonProperty("g1QualNo")]
            public int g1QualNo { get; set; }
            [JsonProperty("g1QualName")]
            public string g1QualName { get; set; }
            [JsonProperty("g1Yymd")]
            public string g1Yymd { get; set; }
            [JsonProperty("g1Shift1")]
            public int g1Shift1 { get; set; }
            [JsonProperty("g1Shift2")]
            public int g1Shift2 { get; set; }
            [JsonProperty("g1Date")]
            public string g1Date { get; set; }
            [JsonProperty("g2ShopName")]
            public string g2ShopName { get; set; }
            [JsonProperty("g2TermId")]
            public string g2TermId { get; set; }
            [JsonProperty("g2ZoneKbn")]
            public string g2ZoneKbn { get; set; }
            [JsonProperty("g2QualNo")]
            public int g2QualNo { get; set; }
            [JsonProperty("g2QualName")]
            public int g2QualName { get; set; }
            [JsonProperty("g2Yymd")]
            public string g2Yymd { get; set; }
            [JsonProperty("g2Shift1")]
            public string g2Shift1 { get; set; }
            [JsonProperty("g2Shift2")]
            public int g2Shift2 { get; set; }
            [JsonProperty("g2Date")]
            public int g2Date { get; set; }
            [JsonProperty("tenaRirekiSu")]
            public string tenaRirekiSu { get; set; }
            [JsonProperty("delFlg")]
            public int delFlg { get; set; }
            [JsonProperty("reasonNo")]
            public int reasonNo { get; set; }
            [JsonProperty("reason")]
            public string reason { get; set; }
            [JsonProperty("regDeviceId")]
            public string regDeviceId { get; set; }
            [JsonProperty("regDate")]
            public string regDate { get; set; }
            [JsonProperty("updateDeviceId")]
            public string updateDeviceId { get; set; }
            [JsonProperty("updateDate")]
            public string updateDate { get; set; }
        }
        public class InspectionList
        {
            [JsonProperty("manufacture")]
            public List<Inspection> manufacture { get; set; }
            [JsonProperty("inspect")]
            public List<Inspection> inspect { get; set; }
            [JsonProperty("audit")]
            public List<Inspection> audit { get; set; }
        }

        private void Test1_FormClosed(object sender, FormClosedEventArgs e)
        {
            LOGGER.End();
        }
    }
}
