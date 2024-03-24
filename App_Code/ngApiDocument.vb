Imports System.Data
Imports System.IO
Imports Syncfusion.DocToPDFConverter
Imports Syncfusion.Pdf



Public Class ngApiDocument

    Public Sub New()

        CurrentRptName = Nothing
        CurrentPath = ngApiPath.getCurrentPath()

    End Sub

    Public Property CurrentPath As String
    Public Property CurrentRptName As String

    Public Function createPdf(ByRef pDestinationTable As DataTable, ByVal pMMGG As String, ByVal pSifraOrgJed As String) As String


        Dim cDocFileName As String = pSifraOrgJed + ".docx"
        Dim cPdfFileName As String = pSifraOrgJed + ".pdf"
        Dim wordDocument As Syncfusion.DocIO.DLS.WordDocument = Nothing

        cDocFileName = String.Format("evid_{0}_{1}.docx", pMMGG, pSifraOrgJed)

        Dim _rptRetFilename As String = Path.GetRandomFileName()
        _rptRetFilename = _rptRetFilename.Replace(".", "")

        Try

            Dim pNazivOrgJed As String = "Test OrgJed"
            'Dim pSifraOrgJed As String = "1050403"
            Dim pNazivSektora As String = "Test sektor"

            Dim fSumaEvidencije As New ClsSumaEvidencije
            fSumaEvidencije.LoadSifarnik("RAD.RW,ODS.RW,BOL.RW")

            wordDocument = fSumaEvidencije.Word_Create(pDestinationTable, pNazivOrgJed, pSifraOrgJed, pMMGG, pNazivSektora)
            'wordDocument.Save(CurrentPath + cDocFileName)
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try


        Try

            Dim converter As New DocToPDFConverter()

            'Converts Word document into PDF document
            Dim pdfDocument As Syncfusion.Pdf.PdfDocument = converter.ConvertToPDF(wordDocument)


            _rptRetFilename = String.Format("evid_{0}_{1}.pdf", pMMGG, _rptRetFilename)
            pdfDocument.Save(CurrentPath + _rptRetFilename)


            pdfDocument.Close()
            'wordDocument.Close()

            'If pShowFrm Then PdfViewerForm.ShowPdfDoc(pdfDocPathName)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            _rptRetFilename = "error.pdf"
        End Try

        Return _rptRetFilename
    End Function


End Class
