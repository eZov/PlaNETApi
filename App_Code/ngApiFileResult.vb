Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Microsoft.VisualBasic

Public Class ngAPIFileResult
    Implements IHttpActionResult

    Private bookStuff As MemoryStream
    Private PdfFileName As String
    Private httpRequestMessage As HttpRequestMessage
    Private httpResponseMessage As HttpResponseMessage

    Public Sub New(ByVal data As MemoryStream, ByVal request As HttpRequestMessage, ByVal filename As String)
        bookStuff = data
        httpRequestMessage = request
        PdfFileName = filename
    End Sub

    Public Function ExecuteAsync(ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage) Implements IHttpActionResult.ExecuteAsync
        httpResponseMessage = httpRequestMessage.CreateResponse(HttpStatusCode.OK)
        httpResponseMessage.Content = New StreamContent(bookStuff)
        httpResponseMessage.Content.Headers.ContentDisposition = New System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        httpResponseMessage.Content.Headers.ContentDisposition.FileName = PdfFileName
        httpResponseMessage.Content.Headers.ContentType = New System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
        Return System.Threading.Tasks.Task.FromResult(httpResponseMessage)
    End Function

    'Private Function IHttpActionResult_ExecuteAsync(cancellationToken As CancellationToken) As Task(Of HttpResponseMessage) Implements IHttpActionResult.ExecuteAsync
    '    Throw New NotImplementedException()
    'End Function

End Class
