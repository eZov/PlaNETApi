Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Syncfusion.OfficeChart
Imports Syncfusion.DocIO
Imports Syncfusion.DocIO.DLS
Imports Syncfusion.DocToPDFConverter
Imports Syncfusion.Pdf
Imports MySql.Data

Public Class ClsFilesInDatabase

    Private sqlText As String

    Private cRawData() As Byte
    Private cFileSize As UInt32

    Private fs As FileStream


    Private cTemplateName As String
    Private cTemplatePath As String

    Public Property DocFolder As String
    Public Property DocName As String
    Public Property DocPath As String



    Public Function WriteDocumentToDisk(ByVal pFileMD5 As String, Optional pSessionId As String = "") As String


        Dim _checkMD5 As Boolean = False

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT filebody, filename, filesize FROM evd_doc_documents WHERE md5=@md5;
]]>.Value
            mycmd.CommandText = strSQL



            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@md5", pFileMD5)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                cFileSize = rd.GetUInt32(rd.GetOrdinal("filesize"))
                DocName = pSessionId + "_" + rd.GetString(rd.GetOrdinal("filename"))

                cRawData = New Byte(cFileSize) {}

                rd.GetBytes(rd.GetOrdinal("filebody"), 0, cRawData, 0, cFileSize)
                fs = New FileStream(HttpContext.Current.Server.MapPath("~/Data/" + DocName), FileMode.Create, FileAccess.ReadWrite)
                fs.Write(cRawData, 0, cFileSize)
                fs.Close()
            End While


        End Using



        _checkMD5 = True

        Return DocName

    End Function

    Public Function WriteReportToDisk(ByVal pReportCode As String, Optional pSessionId As String = "") As String

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT report_structure, report_name, report_size FROM sys_net_reports_structure WHERE report_code=@report_code;
]]>.Value
            mycmd.CommandText = strSQL



            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_code", pReportCode)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                cFileSize = rd.GetUInt32(rd.GetOrdinal("report_size"))
                DocName = pSessionId + "_" + rd.GetString(rd.GetOrdinal("report_name"))

                cRawData = New Byte(cFileSize) {}

                rd.GetBytes(rd.GetOrdinal("report_structure"), 0, cRawData, 0, cFileSize)
                fs = New FileStream(HttpContext.Current.Server.MapPath("~/Data/" + DocName), FileMode.Create, FileAccess.ReadWrite)
                fs.Write(cRawData, 0, cFileSize)
                fs.Close()
            End While


        End Using


        Return HttpContext.Current.Server.MapPath("~/Data/" + DocName)

    End Function

End Class

Enum DocType
    template = 1
    document = 2
End Enum
