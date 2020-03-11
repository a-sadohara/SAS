using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static ImageChecker.Common;


namespace ImageChecker.DTO
{
    class ReportInfo
    {
        // パラメータ
        string strSQL = string.Empty;
        DataTable dtData;

        // PDF作成関連
        private IList<Stream> m_streams;
        private byte[] bytes;
        LocalReport lReport;

        /// <summary>
        /// 帳票出力
        /// </summary>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intNgCushionCnt">NGクッション数</param>
        /// <param name="intNgImageCnt">NG画像数</param>
        public void OutputReport(
            string strFabricName,
            string strInspectionDate,
            int intInspectionNum,
            int intNgCushionCnt,
            int intNgImageCnt)
        {
            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT
                                   TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI') AS start_datetime
                                 , TO_CHAR(iih.end_datetime,'YYYY/MM/DD HH24:MI') AS end_datetime
                                 , TO_CHAR(iih.decision_start_datetime,'YYYY/MM/DD HH24:MI') AS decision_start_datetime
                                 , TO_CHAR(iih.decision_end_datetime,'YYYY/MM/DD HH24:MI') AS decision_end_datetime
                                 , iih.order_img
                                 , iih.fabric_name
                                 , iih.product_name
                                 , iih.unit_num
                                 , iih.inspection_start_line
                                 , iih.inspection_end_line
                                 , iih.inspection_num
                                 , TO_CHAR(dr.acceptance_check_datetime,'HH24:MI:SS') AS acceptance_check_datetime
                                 , dr.line
                                 , dr.cloumns
                                 , dr.ng_face
                                 , dr.ng_reason
                                 , dr.ng_distance_x
                                 , dr.ng_distance_y
                                 , dr.acceptance_check_worker
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                               INNER JOIN " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                               ON  dr.fabric_name = iih.fabric_name
                               AND dr.inspection_date = iih.inspection_date
                               AND dr.inspection_num = iih.inspection_num
                               WHERE dr.fabric_name = :fabric_name
                               AND   dr.inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   dr.inspection_num = :inspection_num
                               AND   dr.ng_reason IS NOT NULL 
                               ORDER BY 
                                   dr.line ASC 
                                 , dr.cloumns ASC 
                                 , dr.ng_face ASC 
                                 , dr.over_detection_except_datetime ASC ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                // LocalReport作成
                lReport = new LocalReport();
                lReport.ReportPath = @".\KenTanChkSheet.rdlc";
                m_streams = new List<Stream>();

                KenTanChkSheet KTCSDs = new KenTanChkSheet();
                foreach (DataRow dr in dtData.Rows)
                {
                    KenTanChkSheet.KenTanChkSheetTableRow KTCSDr = KTCSDs.KenTanChkSheetTable.NewKenTanChkSheetTableRow();
                    KTCSDr.BeginEdit();

                    // ヘッダ情報
                    KTCSDr.OrderImg = dr["order_img"].ToString();
                    KTCSDr.ProductName = dr["product_name"].ToString();
                    KTCSDr.FabricName = dr["fabric_name"].ToString();
                    KTCSDr.UnitNum = dr["unit_num"].ToString();
                    KTCSDr.StartDatetime = dr["start_datetime"].ToString();
                    KTCSDr.EndDatetime = dr["end_datetime"].ToString();
                    KTCSDr.InspectionLine = dr["inspection_start_line"].ToString() + "～" + dr["inspection_end_line"].ToString();
                    KTCSDr.InspectionNum = dr["inspection_num"].ToString();
                    KTCSDr.DecisionStartDatetime = dr["decision_start_datetime"].ToString();
                    KTCSDr.DecisionEndDatetime = dr["decision_end_datetime"].ToString();
                    KTCSDr.NgCushionCnt = intNgCushionCnt.ToString();
                    KTCSDr.NgImageCnt = intNgImageCnt.ToString();

                    // 明細情報
                    KTCSDr.AcceptanceCheckDatetime = dr["acceptance_check_datetime"].ToString();
                    KTCSDr.Line = dr["line"].ToString();
                    KTCSDr.Cloumns = dr["cloumns"].ToString();
                    KTCSDr.NgFace = dr["ng_face"].ToString();
                    KTCSDr.NgDistanceXY = dr["ng_distance_x"].ToString() + "," + dr["ng_distance_y"].ToString();
                    KTCSDr.NgReason = dr["ng_reason"].ToString();
                    KTCSDr.AcceptanceCheckWorker = dr["acceptance_check_worker"].ToString();

                    KTCSDr.EndEdit();
                    KTCSDs.KenTanChkSheetTable.Rows.Add(KTCSDr);
                }

                // PDF作成
                lReport.DataSources.Add(new ReportDataSource("KenTanChkSheet", (DataTable)KTCSDs.KenTanChkSheetTable));
                CreatePDF();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0054, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }

        /// <summary>
        /// PDF作成
        /// </summary>
        private void CreatePDF()
        {
            try
            {
                bytes = lReport.Render("PDF");

                AutoPrintCls autoprintme = new AutoPrintCls(lReport);
                autoprintme.Print();

                // 一時フォルダにPDFを作成
                string fileName = string.Format("Report{0}.pdf", DateTime.Now.ToString("yyyyMMddhhmmssfff"));
                using (FileStream fs = new FileStream(Path.Combine(g_strPdfOutKenTanCheckSheetPath, fileName), FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lReport.Dispose();
            }
        }

        /// <summary>
        /// The ReportPrintDocument will print all of the pages of a ServerReport or LocalReport.
        /// The pages are rendered when the print document is constructed.  Once constructed,
        /// call Print() on this class to begin printing.
        /// </summary>
        class AutoPrintCls : PrintDocument
        {
            private PageSettings m_pageSettings;
            private int m_currentPage;
            private List<Stream> m_pages = new List<Stream>();

            public AutoPrintCls(ServerReport serverReport)
                : this((Report)serverReport)
            {
                RenderAllServerReportPages(serverReport);
            }

            public AutoPrintCls(LocalReport localReport)
                : this((Report)localReport)
            {
                RenderAllLocalReportPages(localReport);
            }

            private AutoPrintCls(Report report)
            {
                // Set the page settings to the default defined in the report
                ReportPageSettings reportPageSettings = report.GetDefaultPageSettings();

                // The page settings object will use the default printer unless
                // PageSettings.PrinterSettings is changed.  This assumes there
                // is a default printer.
                m_pageSettings = new PageSettings();
                m_pageSettings.PaperSize = reportPageSettings.PaperSize;
                m_pageSettings.Margins = reportPageSettings.Margins;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    foreach (Stream s in m_pages)
                    {
                        s.Dispose();
                    }

                    m_pages.Clear();
                }
            }

            protected override void OnBeginPrint(PrintEventArgs e)
            {
                base.OnBeginPrint(e);

                m_currentPage = 0;
            }

            protected override void OnPrintPage(PrintPageEventArgs e)
            {
                base.OnPrintPage(e);

                Stream pageToPrint = m_pages[m_currentPage];
                pageToPrint.Position = 0;

                // Load each page into a Metafile to draw it.
                using (Metafile pageMetaFile = new Metafile(pageToPrint))
                {
                    Rectangle adjustedRect = new Rectangle(
                            e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
                            e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
                            e.PageBounds.Width,
                            e.PageBounds.Height);

                    // Draw a white background for the report
                    e.Graphics.FillRectangle(Brushes.White, adjustedRect);

                    // Draw the report content
                    e.Graphics.DrawImage(pageMetaFile, adjustedRect);

                    // Prepare for next page.  Make sure we haven't hit the end.
                    m_currentPage++;
                    e.HasMorePages = m_currentPage < m_pages.Count;
                }
            }

            protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
            {
                e.PageSettings = (PageSettings)m_pageSettings.Clone();
            }

            private void RenderAllServerReportPages(ServerReport serverReport)
            {
                try
                {
                    string deviceInfo = CreateEMFDeviceInfo();

                    // Generating Image renderer pages one at a time can be expensive.  In order
                    // to generate page 2, the server would need to recalculate page 1 and throw it
                    // away.  Using PersistStreams causes the server to generate all the pages in
                    // the background but return as soon as page 1 is complete.
                    System.Collections.Specialized.NameValueCollection firstPageParameters = new NameValueCollection();
                    firstPageParameters.Add("rs:PersistStreams", "True");

                    // GetNextStream returns the next page in the sequence from the background process
                    // started by PersistStreams.
                    NameValueCollection nonFirstPageParameters = new NameValueCollection();
                    nonFirstPageParameters.Add("rs:GetNextStream", "True");

                    string mimeType;
                    string fileExtension;


                    Stream pageStream = serverReport.Render("IMAGE", deviceInfo, firstPageParameters, out mimeType, out fileExtension);



                    // The server returns an empty stream when moving beyond the last page.
                    while (pageStream.Length > 0)
                    {
                        m_pages.Add(pageStream);

                        pageStream = serverReport.Render("IMAGE", deviceInfo, nonFirstPageParameters, out mimeType, out fileExtension);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("possible missing information ::  " + e);
                }
            }

            private void RenderAllLocalReportPages(LocalReport localReport)
            {
                try
                {
                    string deviceInfo = CreateEMFDeviceInfo();

                    Warning[] warnings;

                    localReport.Render("IMAGE", deviceInfo, LocalReportCreateStreamCallback, out warnings);
                }
                catch (Exception e)
                {
                    MessageBox.Show("error :: " + e);
                }
            }

            private Stream LocalReportCreateStreamCallback(
                string name,
                string extension,
                Encoding encoding,
                string mimeType,
                bool willSeek)
            {
                MemoryStream stream = new MemoryStream();
                m_pages.Add(stream);

                return stream;
            }

            private string CreateEMFDeviceInfo()
            {
                PaperSize paperSize = m_pageSettings.PaperSize;
                Margins margins = m_pageSettings.Margins;
                string strPrintSetting = string.Empty;

                strPrintSetting += "<DeviceInfo>";
                strPrintSetting += "<OutputFormat>emf</OutputFormat>";
                strPrintSetting += "<StartPage>0</StartPage>";
                strPrintSetting += "<EndPage>0</EndPage>";
                strPrintSetting += "<MarginTop>{0}</MarginTop>";
                strPrintSetting += "<MarginLeft>{1}</MarginLeft>";
                strPrintSetting += "<MarginRight>{2}</MarginRight>";
                strPrintSetting += "<MarginBottom>{3}</MarginBottom>";
                strPrintSetting += "<PageHeight>{4}</PageHeight>";
                strPrintSetting += "<PageWidth>{5}</PageWidth>";
                strPrintSetting += "</DeviceInfo>";

                // The device info string defines the page range to print as well as the size of the page.
                // A start and end page of 0 means generate all pages.
                return string.Format(
                    CultureInfo.InvariantCulture,
                    strPrintSetting,
                    ToInches(margins.Top),
                    ToInches(margins.Left),
                    ToInches(margins.Right),
                    ToInches(margins.Bottom),
                    ToInches(paperSize.Height),
                    ToInches(paperSize.Width));
            }

            private static string ToInches(int hundrethsOfInch)
            {
                double inches = hundrethsOfInch / 100.0;
                return inches.ToString(CultureInfo.InvariantCulture) + "in";
            }
        }
    }
}