﻿Imports System.Net.Http
Imports Microsoft.VisualBasic

Public Class ApiGlobal


    Public Shared Property domainName As String = ""
    Public Shared Property domainConnectionString As String = ""

    Public Shared Sub SetDomain(ByVal _request As String)

        ApiGlobal.domainName = _request
        ApiGlobal.domainConnectionString = ConfigurationManager.ConnectionStrings(ApiGlobal.domainName).ConnectionString

    End Sub

    Public Shared Sub ClearDomain()

        ApiGlobal.domainName = ""
        ApiGlobal.domainConnectionString = ""

    End Sub

End Class
