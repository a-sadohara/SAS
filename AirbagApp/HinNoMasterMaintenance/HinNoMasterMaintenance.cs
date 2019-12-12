using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HinNoMasterMaintenance
{
    public partial class HinNoMasterMaintenance : Form
    {
        private const int m_CON_REALITY_SIZE_W = 640;
        private const int m_CON_REALITY_SIZE_H = 480;
        private double m_dblSizeRateW = 100.00;
        private double m_dblSizeRateH = 100.00;
        private double m_dblSizeRate = 100.00;
        private int m_intColumn_cnt = 5;

        public HinNoMasterMaintenance()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SelectErrorReason frmSelectErrorReason = new SelectErrorReason();
            frmSelectErrorReason.ShowDialog(this);
        }

        private void HinNoMasterMaintenance_Load(object sender, EventArgs e)
        {
            string strSQL = "";

            //dgvUser.Rows.Clear();

            //usrctlDataGridWpf = new DataGridWpf_UserCtrl(this, elementHost1, DataGridWpf_UserCtrl.COLUM_TYPE.USER);

            //try
            //{
            //    if (bolModeNonDBCon == true)
            //        throw new Exception("DB非接続モードです");

            //    // PostgreSQLへ接続
            //    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
            //    {
            //        NpgsqlCon.Open();

            //        // SQL抽出
            //        NpgsqlCommand NpgsqlCom = null;
            //        NpgsqlDataAdapter NpgsqlDtAd = null;
            //        dtData = new DataTable();
            //        strSQL += "SELECT * FROM SAGYOSYA ";
            //        if (!string.IsNullOrEmpty(strKanaSta) && !string.IsNullOrEmpty(strKanaEnd))
            //        {
            //            strSQL += "WHERE SUBSTRING(USERYOMIGANA,1,1) BETWEEN '" + strKanaSta + "' AND '" + strKanaEnd + "'";
            //        }

            //        strSQL += "ORDER BY USERNO ASC ";
            //        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
            //        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
            //        NpgsqlDtAd.Fill(dtData);

            //        //TODO:
            //        // データグリッドに反映
            //        //usrctlDataGridWpf = new DataGridWpf_UserCtrl(this, elementHost1, DataGridWpf_UserCtrl.COLUM_TYPE.USER, dtData);
            //        //elementHost1.Child = usrctlDataGridWpf;
            //        // データグリッドビューに反映
            //        foreach (DataRow row in dtData.Rows)
            //        {
            //            this.dgvUser.Rows.Add(row.ItemArray);
            //        }
            //    }

            //    // 描画崩れ防止
            //    if (this.dgvUser.Rows.Count == 0)
            //    {
            //        this.dgvUser.Rows.Add(new object[] { null, null });
            //        dgvUser.Rows.RemoveAt(0);
            //    }
            //    elementHost1.Child = usrctlDataGridWpf;

            //}
            //catch (Exception e)
            //{
            //    string strErrMsg = "";
            //    strErrMsg = e.Message;

            // 後々この処理は消す
            //foreach (string line in File.ReadLines("品名.tsv", Encoding.Default))
            //{
            //    // 改行コードを変換
            //    string strLine = line.Replace("\\rn", Environment.NewLine);

            //    string[] csv = strLine.Split('\t');
            //    string[] data = new string[csv.Length];
            //    Array.Copy(csv, 0, data, 0, data.Length);
            //    this.dgvUser.Rows.Add(data);
            //}
            //}

            //txtHinNoCount.Text = dgvUser.RowCount.ToString();

            // 列数
            m_intColumn_cnt = 4;

            // 列数でコントロールを非表示にする
            if (m_intColumn_cnt < 2)
            {
                txtColumnThreshold01.Visible = false;
                txtLineThresholdB1.Visible = false;
                txtLineThresholdB2.Visible = false;
                lblBDash.Visible = false;
            }
            if (m_intColumn_cnt < 3)
            {
                txtColumnThreshold02.Visible = false;
                txtLineThresholdC1.Visible = false;
                txtLineThresholdC2.Visible = false;
                lblCDash.Visible = false;
            }
            if (m_intColumn_cnt < 4)
            {
                txtColumnThreshold03.Visible = false;
                lblDDash.Visible = false;
            }
            if (m_intColumn_cnt < 5)
            {
                txtColumnThreshold04.Visible = false;
                txtLineThresholdE1.Visible = false;
                txtLineThresholdE2.Visible = false;
                lblEDash.Visible = false;
            }

            // マスタ画像イメージ反映
            picMasterImage.Image = System.Drawing.Image.FromFile(".\\Image\\MasterImage.png");

            // 表示領域とマスタ画像実寸との比率を算出
            // ※実寸：640x480
            picMasterImage.Location = new Point(0, 0);
            picMasterImage.Size = new Size(m_CON_REALITY_SIZE_W, m_CON_REALITY_SIZE_H);
            m_dblSizeRateW = (double)pnlMasterImage.ClientSize.Width / (double)picMasterImage.ClientSize.Width;
            m_dblSizeRateH = (double)pnlMasterImage.ClientSize.Height / (double)picMasterImage.ClientSize.Height;

            // 縦横の小さい方を比率にする
            if (m_dblSizeRateW < m_dblSizeRateH)
            {
                m_dblSizeRate = m_dblSizeRateW;
            }
            else
            {
                m_dblSizeRate = m_dblSizeRateH;
            }

            // 比率に合わせて、マスタ画像サイズを調整する
            picMasterImage.Size = new Size((int)((double)picMasterImage.ClientSize.Width * m_dblSizeRate), (int)((double)picMasterImage.ClientSize.Height * m_dblSizeRate));

            // 表示位置を調整する(中心)
            picMasterImage.Location = new Point((int)((double)pnlMasterImage.ClientSize.Width - (double)picMasterImage.ClientSize.Width) / 2,
                                                (int)((double)pnlMasterImage.ClientSize.Height - (double)picMasterImage.ClientSize.Height) / 2);

            // 境界線の描画
            DrawThreshold();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HinNoImportCsv frmUserImportCsv = new HinNoImportCsv();
            frmUserImportCsv.ShowDialog(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
        }

        private void txtThreshold_Leave(object sender, EventArgs e)
        {
            DrawThreshold();
        }

        private void DrawThreshold()
        {
            string strPicBoxDispLineCtrName = "picBoxDspLine";

            // 変数
            int intColumnThreshold = -1;                    // 列判定用境界線
            int intLineThreshold = -1;                      // 行判定用境界線
            int intColumnThresholdTop = -1;                 // 列判定用境界線の天辺
            int intColumnThresholdBottom = -1;              // 列判定用境界線の底辺

            // 描画の前準備
            // 描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(picMasterImage.Width, picMasterImage.Height);
            // ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            PictureBox pctLineCtrl = null;
            // Penオブジェクト
            Pen p = null;

            // 既存の行判定用境界線コントロール存在チェック
            if (this.Controls.Find(strPicBoxDispLineCtrName, true).Length == 0)
            {
                // 枠線用コントロールの追加
                picMasterImage.Controls.Add(new System.Windows.Forms.PictureBox());
                pctLineCtrl = (PictureBox)picMasterImage.Controls[picMasterImage.Controls.Count - 1];
                pctLineCtrl.Name = strPicBoxDispLineCtrName;
                pctLineCtrl.ClientSize = new Size(picMasterImage.ClientSize.Width, picMasterImage.ClientSize.Height);
            }
            else
            {
                foreach (dynamic ctr in this.Controls.Find(strPicBoxDispLineCtrName, true))
                {
                    pctLineCtrl = ctr;
                    break;
                }
            }

            // 列判定用境界線の描画
            foreach (TextBox txtColumnThreshold in new TextBox[] { txtColumnThreshold01, txtColumnThreshold02,
                                                                   txtColumnThreshold03, txtColumnThreshold04 })
            {
                // 初期化
                intColumnThreshold = -1;

                if (m_intColumn_cnt > 1 && txtColumnThreshold == txtColumnThreshold01)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtColumnThreshold == txtColumnThreshold02)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtColumnThreshold == txtColumnThreshold03)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtColumnThreshold == txtColumnThreshold04)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                }
                // 行判定用境界線が未設定（列数の設定等）の場合はスルー
                if (intColumnThreshold > -1)
                {
                    // 線を描画
                    p = new Pen(Color.Red, 2);
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(p, 0, intColumnThreshold,
                                  picMasterImage.ClientSize.Width, intColumnThreshold);
                }
            }

            // 行判定用境界線の描画
            foreach (TextBox txtLineThreshold in new TextBox[] { txtLineThresholdA1, txtLineThresholdA2,
                                                                 txtLineThresholdB1, txtLineThresholdB2,
                                                                 txtLineThresholdC1, txtLineThresholdC2,
                                                                 txtLineThresholdD1, txtLineThresholdD2,
                                                                 txtLineThresholdE1, txtLineThresholdE2 })
            {
                // 初期化
                intLineThreshold = -1;
                intColumnThresholdTop = -1;
                intColumnThresholdBottom = -1;

                if (txtLineThreshold == txtLineThresholdA1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdA1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (txtLineThreshold == txtLineThresholdA2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdA2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtLineThresholdB1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdB1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtLineThresholdB2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdB2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtLineThresholdC1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdC1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtLineThresholdC2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdC2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtLineThresholdD1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdD1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtLineThresholdD2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdD2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtLineThresholdE1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdE1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtLineThresholdE2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdE2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }
                // 行判定用境界線が未設定（列数の設定等）の場合はスルー
                if (intLineThreshold > -1)
                {
                    // 列判定用境界線の底辺Y座標を設定する
                    if (intColumnThresholdBottom == -1)
                    {
                        intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                    }

                    // 線を描画
                    p = new Pen(Color.Black, 2);
                    p.DashPattern = new float[] { 1.0F, 3.0F, 1.0F, 3.0F };
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    g.DrawLine(p, intLineThreshold, intColumnThresholdTop,
                                  intLineThreshold, intColumnThresholdBottom);
                }
            }

            //リソースを解放する
            p.Dispose();
            g.Dispose();

            //PictureBox1に表示する
            pctLineCtrl.Image = canvas;
        }
    }
}
