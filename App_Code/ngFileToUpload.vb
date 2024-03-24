Imports Microsoft.VisualBasic
Imports System

Public Class ngFileToUpload
    Public Property FileName As String
    Public Property FileSize As String
    Public Property FileType As String
    Public Property LastModifiedTime As Long
    Public Property LastModifiedDate As DateTime
    Public Property FileAsBase64 As String
    Public Property FileAsByteArray As Byte()
    Public Property FileAsFile As HttpPostedFileBase
End Class
