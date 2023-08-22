using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RcdCmn
{

    public static class CommonProc
    {

        #region　### 角度-ラジアン 変換 ###
        public static double ToRadian(double Degrees)
        {
            return ((Math.PI / 180) * Degrees);
        }

        public static double ToDegree(double Radian)
        {
            return ((180 / Math.PI) * Radian);
        }
        #endregion

        #region ### 点の内包判定 ###
        /// <summary>
        /// 点の内包判定
        /// </summary>
        /// <param name="point">判定する点</param>
        /// <param name="poly">多角形</param>
        /// <returns>True:含む False:含まない</returns>
        /// ルール1.上向きの辺は、開始点を含み終点を含まない。
        /// ルール2.下向きの辺は、開始点を含まず終点を含む。
        /// ルール3.水平線Rと辺が水平でない(がRと重ならない)。
        /// ルール4.水平線Rと辺の交点は厳密に点Pの右側になくてはならない。
        public static bool PointInPolygon(PointF point, PointF[] poly)
        {
            int cnt = 0;
            for (int i = 0; i < poly.Count() - 1; i++)
            {
                // 上向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、終点は含まない。(ルール1)
                if (((poly[i].Y <= point.Y) && (poly[i + 1].Y > point.Y))
                    // 下向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、始点は含まない。(ルール2)
                    || ((poly[i].Y > point.Y) && (poly[i + 1].Y <= point.Y)))
                {
                    // ルール1,ルール2を確認することで、ルール3も確認できている。
                    // 辺は点pよりも右側にある。ただし、重ならない。(ルール4)
                    // 辺が点pと同じ高さになる位置を特定し、その時のxの値と点pのxの値を比較する。
                    float vt = (point.Y - poly[i].Y) / (poly[i + 1].Y - poly[i].Y);
                    if (point.X < (poly[i].X + (vt * (poly[i + 1].X - poly[i].X))))
                    {
                        ++cnt;
                    }
                }
            }

            return cnt % 2 == 1;
        }

        #endregion

        #region ### 円と多角形の衝突判定　###
        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerpoint">円の中心座標</param>
        /// <param name="radius">円の半径</param>
        /// <param name="area">多角形（最初の要素と最後の要素が同じ）</param>
        /// <returns></returns>

        public static bool ElipseInPolygon(PointF centerpoint, double radius, PointF[] area)
        {
            // 円の中心が多角形に内包されているか
            if (CommonProc.PointInPolygon(centerpoint, area))
            {
                return true;
            }
            else
            {
                // 円の中心が多角形エリアの中にない場合
                // 線分ごとに衝突判定
                PointF[] pf = area;
                for (int j = 0; j < pf.Count() - 1; j++)
                {
                    int j2 = j + 1;
                    // 線分の両端が同じ座標の場合はスキップ
                    if (pf[j] == pf[j2]) { continue; }
                    // 衝突判定
                    if (CommonProc.EllipseInLine(pf[j], pf[j2], centerpoint, radius))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region ### 円と線分の衝突判定 ###
        /// <summary>
        /// 円と線分の衝突判定
        /// </summary>
        /// <param name="A">線分始点</param>
        /// <param name="B">線分終点</param>
        /// <param name="P">円の中心点</param>
        /// <param name="Radius">円の半径</param>
        /// <returns></returns>
        public static bool EllipseInLine(PointF A ,PointF B ,PointF P,double Radius)
        {
            // 必要なベクトルの用意
            // 始点から終点
            PointF AB = new PointF(B.X - A.X, B.Y - A.Y);
            // 始点から円の中心
            PointF AP = new PointF(P.X - A.X, P.Y - A.Y);
            
            //単位ベクトル化する
            PointF unitAB = ConvertToNomalizeVector(AB);

            // 射影した線分の長さ
            // 始点と円の中心で外積を行う
            // 円の中心と線分の最小距離
            float distance_projection = AP.X * unitAB.Y - unitAB.X * AP.Y;

            // 最小距離が半径よりも小さい
            if (Math.Abs(distance_projection)< Radius)
            {
                // 終点から円の中心
                PointF BP = new PointF(P.X - B.X, P.Y - B.Y);

                // 始点 => 終点と始点 => 円の中心の内積を計算する
                float dot01 = AP.X * AB.X + AP.Y * AB.Y;
                // 始点 => 終点と終点 => 円の中心の内積を計算する
                float dot02 = BP.X * AB.X + BP.Y * AB.Y;

                // 二つの内積の掛け算結果が0以下なら当たり
                if (dot01 * dot02 <= 0.0f)
                {
                    return true;
                }
                /*
                    上の条件から漏れた場合、円は線分上にはないので、
                    始点 => 円の中心の長さか、終点 => 円の中心の長さが
                    円の半径よりも短かったら当たり
                */
                else if (CalculationVectorLength(A,P) < Radius || CalculationVectorLength(B,P) < Radius)
                {
                    return true;
                }
            }

            return false;
        }

        private  static PointF ConvertToNomalizeVector(PointF vector)
        {
            PointF uvec = new PointF();

            float distance = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (distance > 0.0f)
            {
                uvec.X = vector.X / distance;
                uvec.Y = vector.Y / distance;
            }
            else
            {
                uvec = new PointF(0.0f, 0.0f);
            }

            return uvec;
        }

        /// <summary>
        /// 2点間の距離を求める
        /// </summary>
        /// <param name="vec01"></param>
        /// <returns></returns>
        private static float CalculationVectorLength(PointF A, PointF B)
        {
            return (float)Math.Sqrt(Math.Pow((B.X * A.X), 2) + Math.Pow((B.Y * A.Y), 2));
        }

        #endregion

        #region ### 図形交差判定 ###
        /// <summary>
        /// 2つの図形が重なるか判定
        /// </summary>
        /// <returns></returns>
        public static bool CheckOverlap(List<PointF> sp1, List<PointF> sp2)
        {
            bool result = false;

            //点が含まれる
            for (int i = 0; i < sp2.Count; i++)
            {
                bool include = CommonProc.PointInPolygon(sp2[i], sp1.ToArray());
                if (include)
                {
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                //逆パターン
                for (int i = 0; i < sp1.Count; i++)
                {
                    if (CommonProc.PointInPolygon(sp1[i], sp2.ToArray()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            if (!result)
            {
                //辺の重なり
                for (int i = 0; i < 2; i++)
                {
                    if (CheckIntersected(sp1[i], sp1[i + 1], sp2[i], sp2[i + 1]))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private static bool CheckIntersected(PointF a, PointF b, PointF c, PointF d)
        {
            double ta = (c.X - d.X) * (a.Y - c.Y) + (c.Y - d.Y) * (c.X - a.X);
            double tb = (c.X - d.X) * (b.Y - c.Y) + (c.Y - d.Y) * (c.X - b.X);
            double tc = (a.X - b.X) * (c.Y - a.Y) + (a.Y - b.Y) * (a.X - c.X);
            double td = (a.X - b.X) * (d.Y - a.Y) + (a.Y - b.Y) * (a.X - d.X);

            return tc * td < 0 && ta * tb < 0;
        }
        #endregion

        #region ### 外積 ###
        /// <summary>
        /// 外積を求める(正なら左側、負なら右側)
        /// </summary>
        /// <param name="O">線分始点</param>
        /// <param name="A">線分終点</param>
        /// <param name="B">点</param>
        /// <returns>外積</returns>
        public static double Gaiseki(PointF O, PointF A, PointF B)
        {
            return B.X * (O.Y - A.Y) + O.X * (A.Y - B.Y) + A.X * (B.Y - O.Y);
        }
        #endregion

        #region ### ファイル読み書き ###

        public static void WriteFile<T>(string filepath,T data)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //保存
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (System.IO.FileStream fs = new System.IO.FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                xml.Serialize(fs, data);
            }

            //// バイナリファイルで保存する場合
            //using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            //{
            //    BinaryFormatter bf = new BinaryFormatter();
            //    bf.Serialize(fs, this);
            //}

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public static void ReadFile<T>(string filepath, ref T data)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                if (File.Exists(filepath))
                {
                    System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    using (System.IO.FileStream fs = new System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        data = (T)xml.Deserialize(fs);
                    }

                    //// バイナリで保存する場合
                    //using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    //{
                    //    BinaryFormatter bf = new BinaryFormatter();
                    //    data = (T)bf.Deserialize(fs);
                    //}
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new UserException($"設定ファイルが不正です。{ex.Message}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### 文字列変換 ###
        /// <summary>
        /// stringからcharに変換
        /// </summary>
        public static char[] stringToChar(string ErrorString)
        {
            char[] charFive = new char[5];
            for (int i = 0; i < 5; i++) { charFive[i] = ErrorString[i]; }
            return charFive;
        }

        /// <summary>
        /// charからstringに変換
        /// </summary>
        public static string charToString(char[] Errorchar)
        {
            string stringFive = new string(Errorchar);
            return stringFive;
        }

        #endregion 
    }
}
