Imports System.IO
Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Syncfusion.EJ.PdfViewer

Public Class PdfViewerController
    Inherits ApiController

    '' GET api/<controller>
    'Public Function GetValues() As IEnumerable(Of String)
    '    Return New String() {"value1", "value2"}
    'End Function

    '' GET api/<controller>/5
    'Public Function GetValue(ByVal id As Integer) As String
    '    Return "value"
    'End Function

    '' POST api/<controller>
    'Public Sub PostValue(<FromBody()> ByVal value As String)

    'End Sub

    '' PUT api/<controller>/5
    'Public Sub PutValue(ByVal id As Integer, <FromBody()> ByVal value As String)

    'End Sub

    '' DELETE api/<controller>/5
    'Public Sub DeleteValue(ByVal id As Integer)

    'End Sub

    'Post action for processing the PDF documents.
    Public Function Load(jsonResult As Dictionary(Of String, String)) As Object
        Dim helper As New PdfViewerHelper()
        If jsonResult.ContainsKey("isInitialLoading") Then
            Dim cPdfFilename As String = CStr(System.Web.HttpContext.Current.Session("PdfDocument"))

            helper.Load(HttpContext.Current.Server.MapPath("~/Data/" + cPdfFilename))
        End If
        Return JsonConvert.SerializeObject(helper.ProcessPdf(jsonResult))
    End Function

    'Post action for processing the PDF documents when uploading to the ejPdfviewer widget.
    Public Function FileUpload(jsonResult As Dictionary(Of String, String)) As Object
        Dim helper As New PdfViewerHelper()
        If jsonResult.ContainsKey("uploadedFile") Then
            Dim fileUrl = jsonResult("uploadedFile")
            Dim byteArray As Byte() = Convert.FromBase64String(fileUrl)
            Dim stream As New MemoryStream(byteArray)
            helper.Load(stream)
        End If
        Return JsonConvert.SerializeObject(helper.ProcessPdf(jsonResult))
    End Function

    'Post action for downloading the PDF documents from the ejPdfviewer widget.
    Public Function Download(jsonResult As Dictionary(Of String, String)) As Object
        Dim helper As New PdfViewerHelper()
        Return helper.GetDocumentData(jsonResult)
    End Function
End Class
