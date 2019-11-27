using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using ImageChecker.DTO;
using static ImageChecker.Common;

namespace ImageChecker
{

    public partial class TargetSelection : Form
    {

        TargetInfoDto dtTagetInfo;

        public TargetSelection()
        {
            InitializeComponent();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            dtTagetInfo = new TargetInfoDto();
            DataTable dtWkInfo = dtTagetInfo.getTargetInfoDTO();

            foreach (string line in File.ReadLines("検査対象選択.TSV", Encoding.Default))
            {

                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');

                // データテーブルにデータを格納する
                DataRow drTargetInfo = dtWkInfo.NewRow();
                drTargetInfo["№"] = csv[0];
                drTargetInfo["RollInfo"] = csv[1];
                drTargetInfo["CheckStatus"] = csv[2];
                drTargetInfo["CheckProcess"] = csv[3];
                drTargetInfo["Process"] = csv[7];
                drTargetInfo["Status"] = csv[8];
                drTargetInfo["CheckFlg"] = csv[9];

                dtWkInfo.Rows.Add(drTargetInfo);

            }

            dtTagetInfo.setTargetInfoDTO(dtWkInfo);

            // グリッドビューにボタンを追加
            DataGridViewDisableButtonColumn btnOverdetectionexclusion = new DataGridViewDisableButtonColumn();
            btnOverdetectionexclusion.FlatStyle = FlatStyle.Flat;
            btnOverdetectionexclusion.Width = 180;
            DataGridViewDisableButtonColumn btnResultCheck = new DataGridViewDisableButtonColumn();
            btnResultCheck.FlatStyle = FlatStyle.Flat;
            btnResultCheck.Width = 180;
            DataGridViewDisableButtonColumn btnResult = new DataGridViewDisableButtonColumn();
            btnResult.FlatStyle = FlatStyle.Flat;
            btnResult.Width = 180;
            //DataGridViewDisableButtonColumn btnNotCheck = new DataGridViewDisableButtonColumn();
            //btnNotCheck.FlatStyle = FlatStyle.Standard;
            //btnNotCheck.Width = 180;

            this.dataGridView1.Columns.Add(btnOverdetectionexclusion);
            this.dataGridView1.Columns.Add(btnResultCheck);
            this.dataGridView1.Columns.Add(btnResult);
            //this.dataGridView1.Columns.Add(btnNotCheck);

            this.dataGridView1.MultiSelect = false;

        }

        //private void dispgv()
        //{
        //    // グリッドを初期化
        //    dataGridView1.Rows.Clear();

        //    // ターゲットデータ読み込み
        //    DataTable dtWkInfo = dtTagetInfo.getTargetInfoDTO();

        //    // 新規行の追加を位置的に許可
        //    this.dataGridView1.AllowUserToAddRows = true;

        //    foreach (DataRow drTargetInfo in dtWkInfo.Rows)
        //    {

        //        string[] lstDataGrid = new string[8];
        //        lstDataGrid[0] = drTargetInfo["№"].ToString();
        //        lstDataGrid[1] = drTargetInfo["RollInfo"].ToString();
        //        lstDataGrid[2] = drTargetInfo["CheckStatus"].ToString();
        //        lstDataGrid[3] = drTargetInfo["CheckProcess"].ToString();
        //        lstDataGrid[4] = "過検知除外";
        //        lstDataGrid[5] = "合否確認・判定登録";
        //        lstDataGrid[6] = "検査結果";
        //        lstDataGrid[7] = "結果対象外";

        //        this.dataGridView1.Rows.Add(lstDataGrid);

        //        this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[7].Style.BackColor = Color.Black;

        //        switch (drTargetInfo["Process"].ToString())
        //        {
        //            case "過検知除外":
        //                changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 4, int.Parse(drTargetInfo["Status"].ToString()));
        //                break;
        //            case "合否確認・判定登録":
        //                changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 5, int.Parse(drTargetInfo["Status"].ToString()));
        //                break;
        //            case "検査結果":
        //                changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 6, int.Parse(drTargetInfo["Status"].ToString()));
        //                break;
        //        }

        //    }

        //    dataGridView1.DefaultCellStyle.Font = new Font("メイリオ", 8.25F);

        //    // 新規行を追加させない
        //    this.dataGridView1.AllowUserToAddRows = false;

        //}

        private void TargetSelection_Activated(object sender, EventArgs e)
        {
            // グリッドを初期化
            dataGridView1.Rows.Clear();

            // ターゲットデータ読み込み
            DataTable dtWkInfo = dtTagetInfo.getTargetInfoDTO();

            //// 新規行の追加を位置的に許可
            //this.dataGridView1.AllowUserToAddRows = true;

            foreach (DataRow drTargetInfo in dtWkInfo.Rows)
            {

                string[] lstDataGrid = new string[8];
                lstDataGrid[0] = drTargetInfo["№"].ToString();
                lstDataGrid[1] = drTargetInfo["RollInfo"].ToString();
                lstDataGrid[2] = drTargetInfo["CheckStatus"].ToString();
                lstDataGrid[3] = drTargetInfo["CheckProcess"].ToString();
                lstDataGrid[4] = "過検知除外";
                lstDataGrid[5] = "合否確認・判定登録";
                lstDataGrid[6] = "検査結果確認";
                lstDataGrid[7] = "検査対象外";

                this.dataGridView1.Rows.Add(lstDataGrid);

                switch (drTargetInfo["Process"].ToString())
                {
                    case "過検知除外":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1], 4, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                    case "合否確認・判定登録":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1], 5, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                    case "検査結果":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1], 6, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                }


                //DataGridViewAdvancedBorderStyle newStyle =
                //    new DataGridViewAdvancedBorderStyle();
                //newStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
                //newStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
                //newStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
                //newStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
                //this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].Style.BackColor = Color.WhiteSmoke;
                //this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].Style.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
                //this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].Style.ForeColor = System.Drawing.SystemColors.ControlText;
                ////this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].AdjustCellBorderStyle(newStyle, newStyle, false, false, false, false);
                //this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].Style.SelectionBackColor = Color.WhiteSmoke;
                //this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[7].Style.SelectionForeColor = SystemColors.WindowText;
                
            }

            //foreach (DataGridViewRow r in dataGridView1.Rows)
            //    r.Cells[7].Style.BackColor = Color.WhiteSmoke;

            dataGridView1.DefaultCellStyle.Font = new Font("メイリオ", 8.25F);

        }

        private void TargetSelection_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            lblUser.Text = "作業者名：" + g_parUserNm;

            // 行選択モードに変更
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 読み取り専用
            this.dataGridView1.ReadOnly = false;

            TargetSelection_Activated(sender, e);
            //dispgv();
            //foreach (DataGridViewRow r in dataGridView1.Rows)
            //    r.Cells[7].Style.BackColor = Color.WhiteSmoke;
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ResultCheck frmResultCheck;

            DataGridView dgv = (DataGridView)sender;


            if (e.ColumnIndex < 4)
            {
                return;
            }

            DataGridViewDisableButtonCell btnTarget = (DataGridViewDisableButtonCell)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (!btnTarget.Enabled)
            {
                return;
            }

            switch (e.ColumnIndex)
            {
                case 4:
                    Overdetectionexclusion frmOverdetectionexclusion = new Overdetectionexclusion(dtTagetInfo, e.RowIndex);
                    frmOverdetectionexclusion.ShowDialog(this);
                    this.Visible = true;
                    if (frmOverdetectionexclusion.intRet == 2)
                    {
                        frmResultCheck = new ResultCheck(dtTagetInfo, e.RowIndex);
                        frmResultCheck.ShowDialog(this);
                        this.Visible = true;
                    }
                    break;
                case 5:
                    frmResultCheck = new ResultCheck(dtTagetInfo, e.RowIndex);
                    frmResultCheck.ShowDialog(this);
                    this.Visible = true;
                    break;
                case 6:
                    DisplayResults frmResult = new DisplayResults(dtTagetInfo, e.RowIndex);
                    frmResult.ShowDialog(this);
                    this.Visible = true;
                    if (frmResult.intRet == 1)
                    {
                        frmResultCheck = new ResultCheck(dtTagetInfo, e.RowIndex);
                        frmResultCheck.ShowDialog(this);
                        this.Visible = true;
                    }
                    break;
                //case 7:
                //    if(MessageBox.Show("No." + dgv.Rows[e.RowIndex].Cells[0].Value + "を検査対象外にしますか？"
                //                     , "確認"
                //                     , MessageBoxButtons.YesNo) == DialogResult.Yes)
                //    {
                //        DataTable dtTargetInfo = dtTagetInfo.getTargetInfoDTO();
                //        dtTargetInfo.Rows[e.RowIndex]["CheckFlg"] = "0";
                //        dtTagetInfo.setTargetInfoDTO(dtTargetInfo);
                //    }
                //    break;

            }
        }

        private void changeStatus(DataGridViewRow dtRow, int intTarget, int intStatus)
        {
            DataGridViewDisableButtonCell btnOverdetectionexclusion = (DataGridViewDisableButtonCell)dtRow.Cells[4];
            DataGridViewDisableButtonCell btnResultCheck = (DataGridViewDisableButtonCell)dtRow.Cells[5];
            DataGridViewDisableButtonCell btnResult = (DataGridViewDisableButtonCell)dtRow.Cells[6];

            // ボタンを非活性で初期化する
            btnOverdetectionexclusion.Enabled = false;
            btnResultCheck.Enabled = false;
            btnResult.Enabled = false;

            switch (intTarget)
            {
                case 4:
                    dtRow.Cells[intTarget + 1].Style.BackColor = Color.DarkGray;
                    dtRow.Cells[intTarget + 2].Style.BackColor = Color.DarkGray;
                    break;
                case 5:
                    dtRow.Cells[intTarget - 1].Style.BackColor = Color.WhiteSmoke;
                    dtRow.Cells[intTarget + 1].Style.BackColor = Color.DarkGray;
                    break;
                case 6:
                    dtRow.Cells[intTarget - 1].Style.BackColor = Color.WhiteSmoke;
                    dtRow.Cells[intTarget - 2].Style.BackColor = Color.WhiteSmoke;

                    if (!btnResultCheck.Enabled)
                    {
                        dtRow.Cells[intTarget].Style.BackColor = Color.MediumSeaGreen;
                        dtRow.Cells[intTarget].Style.SelectionBackColor = Color.MediumSeaGreen;
                        btnResult.Enabled = true;
                    }

                    return;
            }

            DataGridViewDisableButtonCell btnTarget = (DataGridViewDisableButtonCell)dtRow.Cells[intTarget];
            switch (intStatus)
            {
                case 1:
                    dtRow.Cells[intTarget].Style.BackColor = Color.MediumSeaGreen;
                    dtRow.Cells[intTarget].Style.SelectionBackColor = Color.MediumSeaGreen;
                    btnTarget.Enabled = true;
                    break;
                case 2:
                    dtRow.Cells[intTarget].Style.BackColor = Color.Crimson;
                    btnTarget.Enabled = false;
                    break;
                case 3:
                    dtRow.Cells[intTarget].Style.BackColor = Color.DarkOrange;
                    dtRow.Cells[intTarget].Style.SelectionBackColor = Color.DarkOrange;
                    btnTarget.Enabled = true;
                    break;
                case 4:
                    dtRow.Cells[intTarget].Style.BackColor = Color.MediumSeaGreen;
                    btnTarget.Enabled = false;
                    break;
                default:
                    dtRow.Cells[intTarget].Style.BackColor = Color.DarkGray;
                    btnTarget.Enabled = false;
                    break;

            }
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void btnDisplayResultsAgo_Click(object sender, EventArgs e)
        {
            DisplayResultsAgo frmResult = new DisplayResultsAgo();
            frmResult.ShowDialog(this);
            this.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows[0].Cells[7].Style.BackColor = Color.Red;
            this.dataGridView1.Rows[1].Cells[7].Style.BackColor = Color.Red;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] spl1;
            string[] spl2;
            spl1 = this.dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Replace("\r\n", "|").Split('|');
            spl2 = this.dataGridView1.SelectedRows[0].Cells[2].Value.ToString().Replace("\r\n", "|").Split('|');

            CheckExcept frmLineCorrect = new CheckExcept(spl1[0].Substring(3, 1), spl1[1].Substring(3), spl1[2].Substring(3), spl1[3].Substring(3), spl2[5].Substring(5),
                                                         spl2[0].Substring(7), spl2[1].Substring(7), spl2[2].Substring(6));
            frmLineCorrect.ShowDialog(this);
            this.Visible = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //if (MessageBox.Show("以下の情報を行補正します。よろしいですか？\r\n" +
            //                    this.dataGridView1.SelectedRows[0].Cells[1].Value +
            //                    this.dataGridView1.SelectedRows[0].Cells[2].Value
            //                  , "確認"
            //                  , MessageBoxButtons.YesNo) == DialogResult.No)
            //{


            //    //LineCorrect();
            //}
            string[] spl1;
            string[] spl2;
            spl1 = this.dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Replace("\r\n", "|").Split('|');
            spl2 = this.dataGridView1.SelectedRows[0].Cells[2].Value.ToString().Replace("\r\n", "|").Split('|');

            LineCorrect frmLineCorrect = new LineCorrect(spl1[0].Substring(3,1), spl1[1].Substring(3), spl1[2].Substring(3), spl1[3].Substring(3), spl2[5].Substring(5),
                                                         spl2[0].Substring(7), spl2[1].Substring(7), spl2[2].Substring(6));
            frmLineCorrect.ShowDialog(this);
            this.Visible = true;
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            //string s = "";
            //for (int i = 0; i <= dataGridView1.Columns.Count - 1; i++)
            //{
            //    s = s + dataGridView1.Columns[i].HeaderText + ":" + dataGridView1.Columns[i].Width.ToString() + ",";
            //}
            //MessageBox.Show(s);
        }
    }

    public class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }

    public class DataGridViewDisableButtonCell : DataGridViewButtonCell
    {
        private bool enabledValue;
        public bool Enabled
        {
            get
            {
                return enabledValue;
            }
            set
            {
                enabledValue = value;
            }
        }

        // Override the Clone method so that the Enabled property is copied.
        public override object Clone()
        {
            DataGridViewDisableButtonCell cell =
                (DataGridViewDisableButtonCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
        }

        // By default, enable the button cell.
        public DataGridViewDisableButtonCell()
        {
            this.enabledValue = true;
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // The button cell is disabled, so paint the border,  
            // background, and disabled button for the cell.
            if (!this.enabledValue)
            {
                // Draw the cell background, if specified.
                if ((paintParts & DataGridViewPaintParts.Background) ==
                    DataGridViewPaintParts.Background)
                {
                    SolidBrush cellBackground =
                        new SolidBrush(cellStyle.BackColor);
                    graphics.FillRectangle(cellBackground, cellBounds);
                    cellBackground.Dispose();
                }

                // Draw the cell borders, if specified.
                if ((paintParts & DataGridViewPaintParts.Border) ==
                    DataGridViewPaintParts.Border)
                {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle,
                        advancedBorderStyle);
                }

                // Calculate the area in which to draw the button.
                Rectangle buttonArea = cellBounds;
                Rectangle buttonAdjustment =
                    this.BorderWidths(advancedBorderStyle);
                buttonArea.X += buttonAdjustment.X;
                buttonArea.Y += buttonAdjustment.Y;
                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                // Draw the disabled button text. 
                if (this.FormattedValue is String)
                {
                    TextRenderer.DrawText(graphics,
                        (string)this.FormattedValue,
                        new Font("メイリオ", 8.25F),
                        buttonArea, SystemColors.WindowText);
                    // 文字色を黒にする
                    // マウスオーバーした際の背景色を変更する
                }
            }
            else
            {
                // The button cell is enabled, so let the base class 
                // handle the painting.
                base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                    elementState, value, formattedValue, errorText,
                    cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }
}
