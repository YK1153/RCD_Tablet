using AtrptShare;
using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RcdOperation.Control.ControlMain;
using static RcdOperationSystemConst.OperationConst;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RcdOperation.Control
{
    public partial class ControlMain 
    {
        private bool MainC = true;
        private bool SubC = false;

        private CamInfo GetCamRcvInfo(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamRcvInfo;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamRcvInfo;

                default:
                    return new CamInfo();
            }
        }

        private List<CamInfo> GetCamRcvInfoHistory(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamRcvInfoHistory;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamRcvInfoHistory;

                default:
                    return new List<CamInfo>();
            }
        }

        private PointF GetCamLeftBackPoint(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return new PointF(m_camClPana.CamRcvInfo.xPosition, m_camClPana.CamRcvInfo.yPosition);

                case OperationConst.CamType.AVP:
                    PointF rearCenter = new PointF((float)(m_camClAvp.CamRcvInfo.xPosition), (float)(m_camClAvp.CamRcvInfo.yPosition));
                    double carYawrate = CommonProc.ToRadian(m_camClAvp.CamRcvInfo.MoveVector);
                    double LLCY = m_carSpec.RearOverhang * Math.Cos(carYawrate);
                    double LLSY = m_carSpec.RearOverhang * Math.Sin(carYawrate);
                    double TCY = (m_carSpec.WidthDistance / 2) * Math.Cos(carYawrate);
                    double TSY = (m_carSpec.WidthDistance / 2) * Math.Sin(carYawrate);
                    PointF leftbackpoint = new PointF();
                    leftbackpoint.X = (float)(rearCenter.X - LLCY - TSY);
                    leftbackpoint.Y = (float)(rearCenter.Y - LLSY + TCY);

                    return leftbackpoint;

                default:
                    return new PointF();
            }
        }

        private bool GetCamCommStatus(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CommStatus;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CommStatus;

                case null:
                    return true;

                default:
                    return false;
            }
        }

        private int GetCamHeakthStatus(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.HealthStatus;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.HealthStatus;

                case null:
                    return OperationConst.C_NORMAL;

                default:
                    return OperationConst.C_ABNORMAL;
            }
        }

        private List<CommCam.HealthInfo> GetCamHeakthStatusInfo(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.HealthStatusInfo;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.HealthStatusInfo;

                default:
                    return null;
            }
        }


        private int GetPauseStatus(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.PauseStatus;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.PauseStatus;

                case null:
                    return OperationConst.C_NORMAL;

                default:
                    return OperationConst.C_ABNORMAL;
            }
        }

        private int GetCamStartRes(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamStartRes;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamStartRes;

                case null:
                    return OperationConst.C_NORMAL;

                default:
                    return OperationConst.C_ABNORMAL;
            }
        }

        private int GetCamStarted(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamSarted;

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamSarted;

                case null:
                    return OperationConst.C_NORMAL;

                default:
                    return OperationConst.C_ABNORMAL;
            }
        }





        private void ExeCamResetVal(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    m_camClPana.CamResetVal();
                    break;

                case OperationConst.CamType.AVP:
                    m_camClAvp.CamResetVal();
                    break;

            }
        }

        private bool ExeCamPreControlStart(bool Type, string bodyno)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamPreControlStart(bodyno);

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamPreControlStart(bodyno);

                case null:
                    return true;

                default:
                    return false;

            }
        }

        private bool ExeCamControlStart(bool Type, string bodyno, DateTime now)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    return m_camClPana.CamControlStart(bodyno,now);

                case OperationConst.CamType.AVP:
                    return m_camClAvp.CamControlStart(bodyno, now);

                case null:
                    return true;

                default:
                    return false;

            }
        }

        private void ExeCamEnd(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    m_camClPana.CamEnd();
                    break;

                case OperationConst.CamType.AVP:
                    m_camClAvp.CamEnd();
                    break;

                case null:
                    return;
            }
        }

        public void ExeReGetVector(bool Type)
        {
            OperationConst.CamType? camType = Type ? MainCamType : SubCamType;

            switch (camType)
            {
                case OperationConst.CamType.Pana:
                    m_camClPana.ReGetVector();
                    break;

                case OperationConst.CamType.AVP:
                    m_camClAvp.ReGetVector();
                    break;

            }
        }

    }
}
