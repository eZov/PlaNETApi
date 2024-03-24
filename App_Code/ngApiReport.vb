Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Data
Imports Microsoft.VisualBasic
Imports System.IO
Imports System.Web
Imports Syncfusion.Pdf
Imports Syncfusion.Pdf.Parsing
Imports Syncfusion.Pdf.Graphics
Imports System.Drawing

Public Class ngApiReport

    Dim rpt As ReportDocument
    Dim cReports As ClsReports

    Public Sub setupReport()

        Dim rptDocname As String = ""


        If rpt IsNot Nothing Then
            rpt.Close()
            rpt.Dispose()
        End If

        cReports = New ClsReports


        rpt = New ReportDocument
        rpt.PrintOptions.PaperSize = PaperSize.PaperA4

        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")
        cReports.CurrentRptName = ""

        ' Session("CurrentRptCode") PLL10
        cReports.WriteReportToDisk("PLL10")




        rptDocname = cReports.CurrentPath + cReports.CurrentRptName
        rpt.Load(rptDocname)

        'cReports.ParamListValues = Session("ParamListValues")

        ' CurrentRptDataCode  PLL10
        Dim _DSet As DataSet = cReports.CreateDatasource("PLL10")
        rpt.SetDataSource(_DSet)

        If _DSet.Tables.Count > 0 Then

            rpt.ExportToDisk(ExportFormatType.PortableDocFormat, ngApiPath.getCurrentPath() + "report-test.pdf")
            'CrystalReportViewer1.ReportSource = rpt
            'CrystalReportViewer1.PageZoomFactor = 100
            'CrystalReportViewer1.ToolPanelView = CrystalDecisions.Web.ToolPanelViewType.None

        End If



    End Sub

    Public Function createReport(ByVal pPutNalId As Integer, ByVal pReportCode As String, ByVal pDataCode As String) As String

        Dim _rptDocname As String = ""
        Dim _rptRetFilename As String

        'Dim _rptRetFilename As String = Path.GetRandomFileName()
        '_rptRetFilename = _rptRetFilename.Replace(".", "")
        '_rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pPutNalId.ToString, _rptRetFilename)

        _rptRetFilename = Me.getRandomFileName(pPutNalId)

        If rpt IsNot Nothing Then
            rpt.Close()
            rpt.Dispose()
        End If

        cReports = New ClsReports


        rpt = New ReportDocument
        rpt.PrintOptions.PaperSize = PaperSize.PaperA4

        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")

        cReports.CurrentRptName = ""
        cReports.WriteReportToDisk(pReportCode)     ' uzima CrystalReports iz baze i spašava file u cReports.CurrentRptName

        _rptDocname = cReports.CurrentPath + cReports.CurrentRptName
        rpt.Load(_rptDocname)

        'cReports.ParamListValues = Session("ParamListValues")
        cReports.ParamListValues.Add(pPutNalId)


        ' CurrentRptDataCode  PLL10
        ' Sql procedura PTNL1
        Dim _DSet As DataSet = cReports.CreateDatasource(pDataCode)
        rpt.SetDataSource(_DSet)

        If _DSet.Tables.Count > 0 Then

            ' rpt.SetParameterValue("pPutNalId", 1)
            rpt.ExportToDisk(ExportFormatType.PortableDocFormat, ngApiPath.getCurrentPath() + _rptRetFilename)

        End If

        Return _rptRetFilename

    End Function

    Public Function createMergeReport(ByRef pPNId As List(Of String), ByVal pReportCode As String, ByVal pDataCode As String, ByVal pPotpis As Boolean, ByVal pWatermark As Boolean) As String

        Dim _rptReportname As String = ""
        Dim _rptRetFilename As String = Me.getRandomFileName("grupaPN")

        cleanUp()

        cReports = New ClsReports
        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")

        cReports.CurrentRptName = ""
        cReports.WriteReportToDisk(pReportCode)                         ' uzima CrystalReports iz baze i spašava file u cReports.CurrentRptName, ako CR fajl već postoji, ne radi ništa

        _rptReportname = cReports.CurrentPath + cReports.CurrentRptName ' CR fajl sa pathom


        Dim _rptFileList As New List(Of String)

        For Each el In pPNId

            rpt = New ReportDocument
            rpt.PrintOptions.PaperSize = PaperSize.PaperA4
            rpt.Load(_rptReportname)

            Dim _rptFilename As String = Me.getRandomFileName(el)

            ' Procesira svaki id PN i generiše pdf
            '
            cReports.ParamListValues.Clear()
            cReports.ParamListValues.Add(el)

            Dim _DSet As DataSet = cReports.CreateDatasource(pDataCode)
            rpt.SetDataSource(_DSet)

            If _DSet.Tables.Count > 0 Then

                ' Export to PDF
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, ngApiPath.getCurrentPath() + _rptFilename)

                ' Load PDF report i prebaci u MemStream
                Dim _inMemStream As New MemoryStream
                Dim _lastSignedStream = New FileStream(ngApiPath.getCurrentPath() + _rptFilename, IO.FileMode.Open, IO.FileAccess.Read)
                _lastSignedStream.CopyTo(_inMemStream)

                Dim _outMemStream As New MemoryStream

                Select Case pPotpis
                    Case True
                        ' Insert IMAGE u PDF
                        Dim _outMemStreamImg As MemoryStream = insImg2Report(el, _inMemStream)

                        ' Insert WATERMARK u PDF
                        If pWatermark = True Then
                            Dim _watermarkText As String = String.Format("BHRT - putni nalog {0} je opravdan", el.ToString())
                            _outMemStream = insWatermark2Report(_outMemStreamImg, _watermarkText)
                        Else
                            _outMemStream = _outMemStreamImg
                        End If


                    Case False
                        ' Insert WATERMARK u PDF
                        If pWatermark = True Then
                            Dim _watermarkText As String = String.Format("BHRT - putni nalog {0} je opravdan", el.ToString())
                            _outMemStream = insWatermark2Report(_inMemStream, _watermarkText)
                        Else
                            _outMemStream = _inMemStream
                        End If


                        ' Insert WATERMARK u PDF

                End Select



                Dim _apiDbFiles As New ngApiDbFiles
                _rptFilename = Me.getRandomFileName(el)
                _apiDbFiles.writeToFile(_rptFilename, ngApiPath.getCurrentPath(), _outMemStream)

                _rptFileList.Add(_rptFilename)
            End If
        Next

        Dim _rptFileListPath As New List(Of String)
        For Each el In _rptFileList
            _rptFileListPath.Add(ngApiPath.getCurrentPath() + el)
        Next

        Dim _rptFileListCsv As String = String.Join(",", _rptFileListPath)

        Dim finalDoc As New PdfDocument()

        Dim source As String() = _rptFileListPath.ToArray()
        PdfDocument.Merge(finalDoc, source)

        finalDoc.Save(ngApiPath.getCurrentPath() + _rptRetFilename)
        finalDoc.Close(True)





        Return _rptRetFilename

    End Function

    Public Function createReportAsStream(ByVal pPutNalId As Integer, ByVal pReportCode As String, ByVal pDataCode As String) As MemoryStream

        Dim _rptDocname As String = ""

        Dim _rptRetFilename As String

        'Dim _rptRetFilename As String = Path.GetRandomFileName()

        '_rptRetFilename = _rptRetFilename.Replace(".", "")
        '_rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pPutNalId.ToString, _rptRetFilename)

        _rptRetFilename = Me.getRandomFileName(pPutNalId)

        If rpt IsNot Nothing Then
            rpt.Close()
            rpt.Dispose()
        End If

        cReports = New ClsReports


        rpt = New ReportDocument
        rpt.PrintOptions.PaperSize = PaperSize.PaperA4

        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")
        cReports.CurrentRptName = ""

        ' Session("CurrentRptCode") PLL10
        ' CrystalReport: PTNL1
        cReports.WriteReportToDisk(pReportCode)


        ' CrystalReports report filename
        '
        _rptDocname = cReports.CurrentPath + cReports.CurrentRptName
        rpt.Load(_rptDocname)

        'cReports.ParamListValues = Session("ParamListValues")
        cReports.ParamListValues.Add(pPutNalId)


        ' CurrentRptDataCode  PLL10
        ' Sql procedura PTNL1
        Dim _DSet As DataSet = cReports.CreateDatasource(pDataCode)
        rpt.SetDataSource(_DSet)

        If _DSet.Tables.Count > 0 Then

            ' rpt.SetParameterValue("pPutNalId", 1)
            Dim _rptRetFileSTream As FileStream = rpt.ExportToStream(ExportFormatType.PortableDocFormat)
            Dim _rptRetMemoryStream As New MemoryStream
            _rptRetFileSTream.CopyTo(_rptRetMemoryStream)

            Return _rptRetMemoryStream

        End If

        Return Nothing

    End Function

    Public Function createReport(ByRef pParamProc As List(Of String), ByVal pReportCode As String, ByVal pDataCode As String) As String

        Dim _rptDocname As String = ""
        Dim _rptRetFilename As String = Path.GetRandomFileName()
        _rptRetFilename = _rptRetFilename.Replace(".", "")

        '_rptRetFilename = "putnal_" + pPutNalId.ToString + "_" + _rptRetFilename + ".pdf"

        _rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pDataCode, _rptRetFilename)
        If rpt IsNot Nothing Then
            rpt.Close()
            rpt.Dispose()
        End If

        cReports = New ClsReports


        rpt = New ReportDocument
        rpt.PrintOptions.PaperSize = PaperSize.PaperA4

        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")
        cReports.CurrentRptName = ""

        ' Uzima CR iz db i spašava na disk - naziv fajla se upisuje u CurrentRptName
        ' CrystalReport: PTNL1
        cReports.WriteReportToDisk(pReportCode)


        ' CrystalReports report filename
        '
        _rptDocname = cReports.CurrentPath + cReports.CurrentRptName
        rpt.Load(_rptDocname)

        'cReports.ParamListValues = Session("ParamListValues")
        'cReports.ParamListValues.Add(pPutNalId)
        '
        ' Prebaci sve parametre u cReports.ParamListValues
        '
        cReports.ParamListValues = pParamProc

        ' CurrentRptDataCode  PLL10
        ' Sql procedura PTNL1
        Dim _DSet As DataSet = cReports.CreateDatasource(pDataCode)
        rpt.SetDataSource(_DSet)

        If _DSet.Tables.Count > 0 Then

            ' rpt.SetParameterValue("pPutNalId", 1)
            rpt.ExportToDisk(ExportFormatType.PortableDocFormat, ngApiPath.getCurrentPath() + _rptRetFilename)

        End If

        Return _rptRetFilename

    End Function





    Public Function createReport(ByVal pRetFilename As String, ByVal pReportCode As String, ByRef pParamProc As List(Of String), ByVal pDataCode As String) As String


        ' CleanUp rpt
        '
        cleanUp()
        rpt = New ReportDocument
        rpt.PrintOptions.PaperSize = PaperSize.PaperA4

        cReports = New ClsReports
        cReports.CurrentPath = ngApiPath.getCurrentPath("CrystalReports")
        cReports.CurrentRptName = ""
        cReports.WriteReportToDisk(pReportCode)     ' uzima CrystalReports iz baze i spašava file u cReports.CurrentRptName

        If cReports.CurrentRptName.Length = 0 Then Return Nothing

        Dim _rptDocname As String = cReports.CurrentPath + cReports.CurrentRptName
        rpt.Load(_rptDocname)

        cReports.ParamListValues = pParamProc
        Dim _DSet As DataSet = cReports.CreateDatasource(pDataCode)
        rpt.SetDataSource(_DSet)

        If _DSet.Tables.Count > 0 Then

            rpt.ExportToDisk(ExportFormatType.PortableDocFormat, ngApiPath.getCurrentPath() + pRetFilename)

        End If

        Return pRetFilename

    End Function

    Public Function createReportXML(ByVal pXmlName As String, ByRef pParamProc As List(Of String), ByVal pDataCode As String) As String


        cReports = New ClsReports
        cReports.CurrentPath = ngApiPath.getCurrentPath()
        cReports.CurrentRptName = String.Format("CR_{0}_.xml", pXmlName)

        '
        ' Prebaci sve parametre u cReports.ParamListValues
        '
        cReports.ParamListValues = pParamProc

        ' CurrentRptDataCode  PLL10
        ' Sql procedura PTNL1
        Dim _DSet As DataSet = cReports.CreateDatasourceXML(pDataCode)
        _DSet.WriteXml(cReports.CurrentPath + cReports.CurrentRptName, XmlWriteMode.WriteSchema)

        Return cReports.CurrentPath + cReports.CurrentRptName

    End Function

    Public Function createReportJSON(ByRef pParamProc As List(Of String), ByVal pDataCode As String) As DataSet


        cReports = New ClsReports

        '
        ' Prebaci sve parametre u cReports.ParamListValues
        '
        cReports.ParamListValues = pParamProc

        ' CurrentRptDataCode  PLL10
        ' Sql procedura PTNL1
        Dim _DSet As DataSet = cReports.CreateDatasourceXML(pDataCode)

        Return _DSet

    End Function

    Public Function getRandomFileName(ByVal pPutNalId As Integer) As String

        Dim _rptRetFilename As String = Path.GetRandomFileName()

        _rptRetFilename = _rptRetFilename.Replace(".", "")
        _rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pPutNalId.ToString, _rptRetFilename)

        Return _rptRetFilename

    End Function

    Public Function getRandomFileName(ByVal pName As String) As String

        Dim _rptRetFilename As String = Path.GetRandomFileName()

        _rptRetFilename = _rptRetFilename.Replace(".", "")
        _rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pName, _rptRetFilename)

        Return _rptRetFilename

    End Function



    Private Function insImg2Report(ByVal pPNId As Integer, ByRef pInMemStream As MemoryStream) As MemoryStream

        ''''''''' INSERT SLIKA u dokument '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        ' Uzima pozicije potpisa i evd_potpisi_id za doc (1 ili 2)
        '
        Dim _apiPNalog As New NgApiPnPutniNalog()
        Dim _signPosCertId As Dictionary(Of Integer, Integer) = _apiPNalog.getSignedPos(pPNId, 1)
        Dim _offset As Integer = 0


        Dim _outMemStream As New MemoryStream
        Dim _apiCertificateSign As New ngApiCertificateSign()

        _apiCertificateSign.insSignImges2Doc(_signPosCertId, 1, pInMemStream, _outMemStream, _offset)

        Return _outMemStream

    End Function

    Private Function insWatermark2Report(ByRef _inMemStream As Stream, ByVal _watermarkText As String) As MemoryStream

        'Load the document.

        Dim loadedDocument As New PdfLoadedDocument(_inMemStream)
        Dim loadedPage As PdfPageBase = loadedDocument.Pages(0)
        Dim graphics As PdfGraphics = loadedPage.Graphics

        'set the font
        Dim font As PdfFont = New PdfStandardFont(PdfFontFamily.Helvetica, 30)

        ' watermark text.
        Dim state As PdfGraphicsState = graphics.Save()
        graphics.SetTransparency(0.25F)
        graphics.RotateTransform(-40)
        graphics.DrawString(_watermarkText, font, PdfPens.Red, PdfBrushes.Red, New PointF(-250, 550))

        Dim _outMemStream As New MemoryStream
        loadedDocument.Save(_outMemStream)


        'Save and close the document.
        'loadedDocument.Save("watermark.pdf")
        loadedDocument.Close(True)

        Return _outMemStream

    End Function

    Private Sub cleanUp()

        If rpt IsNot Nothing Then
            rpt.Close()
            rpt.Dispose()
        End If

    End Sub

End Class
