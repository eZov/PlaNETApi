Imports System.IO
Imports Microsoft.VisualBasic

Public Class ngApiPath


    Public Shared Function getCurrentPath(Optional ByVal pSubFolder As String = "") As String

        If pSubFolder.Length > 0 Then pSubFolder = pSubFolder + "/"

        Dim _rptRetFilename As String = HttpContext.Current.Server.MapPath("~/Data/" + pSubFolder)

        Return _rptRetFilename

    End Function

    Public Shared Function getTempPath() As String

        Dim _path As String = HttpContext.Current.Server.MapPath("~/Temp/")
        If Not Directory.Exists(_path) Then Directory.CreateDirectory(_path)

        Return _path

    End Function

    Public Shared Function getRandomFileName(ByVal pName As String) As String

        Dim _rptRetFilename As String = Path.GetRandomFileName()

        _rptRetFilename = _rptRetFilename.Replace(".", "")
        _rptRetFilename = String.Format("putnal_{0}_{1}.pdf", pName, _rptRetFilename)

        Return _rptRetFilename

    End Function

End Class
