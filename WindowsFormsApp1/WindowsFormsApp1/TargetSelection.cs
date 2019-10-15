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
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;

namespace WindowsFormsApp1
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

                dtWkInfo.Rows.Add(drTargetInfo);

            }

            dtTagetInfo.setTargetInfoDTO(dtWkInfo);

            // グリッドビューにボタンを追加
            DataGridViewDisableButtonColumn btnOverdetectionexclusion = new DataGridViewDisableButtonColumn();
            btnOverdetectionexclusion.FlatStyle = FlatStyle.Flat;
            btnOverdetectionexclusion.Width = 150;
            DataGridViewDisableButtonColumn btnResultCheck = new DataGridViewDisableButtonColumn();
            btnResultCheck.FlatStyle = FlatStyle.Flat;
            btnResultCheck.Width = 150;
            DataGridViewDisableButtonColumn btnResult = new DataGridViewDisableButtonColumn();
            btnResult.FlatStyle = FlatStyle.Flat;
            btnResult.Width = 150;

            this.dataGridView1.Columns.Add(btnOverdetectionexclusion);
            this.dataGridView1.Columns.Add(btnResultCheck);
            this.dataGridView1.Columns.Add(btnResult);

            this.dataGridView1.MultiSelect = false;

        }

        private void TargetSelection_Activated(object sender, EventArgs e)
        {
            // グリッドを初期化
            dataGridView1.Rows.Clear();

            // ターゲットデータ読み込み
            DataTable dtWkInfo = dtTagetInfo.getTargetInfoDTO();

            // 新規行の追加を位置的に許可
            this.dataGridView1.AllowUserToAddRows = true;
            foreach (DataRow drTargetInfo in dtWkInfo.Rows)
            {

                string[] lstDataGrid = new string[7];
                lstDataGrid[0] = drTargetInfo["№"].ToString();
                lstDataGrid[1] = drTargetInfo["RollInfo"].ToString();
                lstDataGrid[2] = drTargetInfo["CheckStatus"].ToString();
                lstDataGrid[3] = drTargetInfo["CheckProcess"].ToString();
                lstDataGrid[4] = "過検知除外";
                lstDataGrid[5] = "合否確認・判定登録";
                lstDataGrid[6] = "検査結果";

                this.dataGridView1.Rows.Add(lstDataGrid);

                switch (drTargetInfo["Process"].ToString())
                {
                    case "過検知除外":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 4, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                    case "合否確認・判定登録":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 5, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                    case "検査結果":
                        changeStatus(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2], 6, int.Parse(drTargetInfo["Status"].ToString()));
                        break;
                }

            }

            // 新規行を追加させない
            this.dataGridView1.AllowUserToAddRows = false;

        }

        private void TargetSelection_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            lblUser.Text = "作業者名：" + parUserNm;

            // 行選択モードに変更
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 読み取り専用
            this.dataGridView1.ReadOnly = true;

            TargetSelection_Activated(sender, e);

        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ResultCheck frmResultCheck;

            DataGridView dgv = (DataGridView)sender;


            if(e.ColumnIndex < 4)
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
                    Overdetectionexclusion frmOverdetectionexclusion = new Overdetectionexclusion(dtTagetInfo,e.RowIndex);
                    frmOverdetectionexclusion.ShowDialog(this);
                    if (frmOverdetectionexclusion.intRet == 2)
                    {
                        frmResultCheck = new ResultCheck(dtTagetInfo, e.RowIndex);
                        frmResultCheck.ShowDialog(this);
                    }
                    break;
                case 5:
                    frmResultCheck = new ResultCheck(dtTagetInfo, e.RowIndex);
                    frmResultCheck.ShowDialog(this);
                    break;
                case 6:
                    Result2 frmResult = new Result2(dtTagetInfo, e.RowIndex);
                    frmResult.ShowDialog(this);
                    break;
                
            }
        }

        private void changeStatus(DataGridViewRow dtRow,int intTarget,int intStatus)
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

                    if (! btnResultCheck.Enabled)
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
                    dtRow.Cells[intTarget].Style.BackColor = Color.WhiteSmoke;
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

        private void TargetSelection_LocationChanged(object sender, EventArgs e)
        {
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
                        this.DataGridView.Font,
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
