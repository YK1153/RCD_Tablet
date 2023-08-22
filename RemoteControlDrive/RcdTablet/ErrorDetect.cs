using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using RcdCmn;
using System.Reflection;
using CommWrapper;
using static RcdTablet.clsTabletMsg;
using static RcdTablet.Utils;
using static RcdDao.DEmergencyDao;
using RcdDao;


namespace RcdTablet
{
    public partial class ErrorDetect : Form
    {

        private AppLog LOGGER = AppLog.GetInstance();
        private clsTablet m_parent;

        public ErrorDetect(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
        }

        private void FormErrorDetect_Load(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                SetDoubleBuffer(this);
                List<ErrorStatus> errorStatuses = new DEmergencyDao().GetAllErrorStatus_noSolve(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                lbl_bodyNo_Value.Text = errorStatuses[0].BodyNo;
                lbl_emergency_value.Text = errorStatuses[0].StatusMsg;
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 閉じるボタン押下
        /// </summary>
        private void btnExit_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                Close();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private void btnCarOffAndon_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_parent.andonStop();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        private void PreventRepeatBtn(Control ctrl, int waitTime)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //コントロールのプロパティを変更できるか？
                ctrl.Enabled = false;
                Stopwatch btnWait = new Stopwatch();
                btnWait.Start();
                do
                {

                }
                while (btnWait.ElapsedMilliseconds < waitTime);

                ctrl.Enabled = true;
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnCarResolveSndMsg_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //if (!carStat.Status.In(Res.CarStatus.STOPPED.Code, Res.CarStatus.ERROR.Code))
                //{
                //    LOGGER.Info($"異常解除ボタン押下: 対象ステータスでないため、処理終了。車両ステータス: {m_carStatus.GetMsg(carStat.Status)}");
                //    return;
                //}

                //LOGGER.Info($"車両異常解除ボタン押下: ボデーNo: {carStat.CarNo}, 車両ステータス: {m_carStatus.GetMsg(carStat.Status)}");
                //CarResolveSndMsg msg = new CarResolveSndMsg();
                //m_parent.SendTcp(msg);

                //// 連打防止待ち
                //Task.Run(() =>
                //{
                //    PreventRepeatBtn(btn_carResolve, C_DEFAULT_BTN_WAIT);
                //});

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_carResolve_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_parent.carResolveSndMsg();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
    }
}
