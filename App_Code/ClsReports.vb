Imports System.Data
Imports System.IO
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsReports

    Public Sub New()

        DtList = New DataTable
        DtList.TableName = "dtReports"
        CurrentRptName = Nothing
        CurrentPath = "~/Data/"

        ParamList = New List(Of String)
        ParamListTitle = New List(Of String)

        ParamListValues = New List(Of String)

    End Sub

    Public Property DtList As DataTable

    Public Property ParamList As List(Of String)
    Public Property ParamListTitle As List(Of String)
    Public Property ParamListValues As List(Of String)

    Public Property DtParam1List As DataTable
    Public Property DtParam2List As DataTable

    Public Property CurrentPath As String
    Public Property CurrentRptName As String

    Public Property CurrentRptCode As String
    Public Property CurrentRptDataCode As String

    Public Function getList(Optional ByRef pVrsta As String = "plc_net") As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT
    r.sys_net_reports_tree_id AS Id
    , rg.id AS ParentId   
    , r.report_name AS Naziv
    , r.`report_tag`
FROM
    sys_net_reports_tree_group AS rg
    INNER JOIN sys_net_reports_tree AS r 
        ON (rg.report_group = r.report_group) AND (rg.report_tree_name = r.report_tree_name)
WHERE (r.report_tree_name ='plc_net')
UNION 
SELECT 
	rg.id AS Id, 
	rg.parent_id AS ParentId, 
	rg.report_group AS Naziv,
	'' AS report_tag 
FROM sys_net_reports_tree_group rg
WHERE rg.report_tree_name ='plc_net';
]]>.Value



            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

        End Using

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pSfrReport As String = "000000") As Integer

        If pSfrReport.Length = 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("Id") = pSfrReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getIdx(ByVal pIdReport As Integer) As Integer

        If pIdReport <= 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("hidId") = pIdReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Id")
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("hidId"))
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Sub getReportCode(ByVal pReportTag As String)

        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT rt.`report_code`, rt.`report_data_code` 
FROM `sys_net_reports_tags` rt
WHERE rt.`report_tag`=@report_tag;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_tag", pReportTag)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                CurrentRptCode = rd.GetString(rd.GetOrdinal("report_code"))
                CurrentRptDataCode = rd.GetString(rd.GetOrdinal("report_data_code"))
            End While

            rd.Close()

            strSQL = <![CDATA[ 
SELECT 
rd2p.`report_data_parameters_code` 
, rdp.`report_parameter_sql`
FROM `sys_net_reports_data2parameters` rd2p LEFT JOIN `sys_net_reports_data_parameters` rdp
ON rd2p.`report_data_parameters_code`= rdp.`report_data_parameters_code`
WHERE rd2p.`report_data_code`=@report_data_code
ORDER BY rd2p.`report_data_parameters_code`;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_data_code", CurrentRptDataCode)
            mycmd.Prepare()

            rd = mycmd.ExecuteReader()

            ParamList.Clear()
            ParamListTitle.Clear()

            While rd.Read
                'CurrentRptCode = rd.GetUInt32("report_data_parameters_code")
                ParamList.Add(rd.GetString("report_parameter_sql"))
                ParamListTitle.Add(rd.GetString("report_data_parameters_code"))
            End While

        End Using


    End Sub

    Public Function getParamList(ByVal pLstNumSql As Integer) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()


            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
]]>.Value
            strSQL = ParamList.Item(pLstNumSql - 1)

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)

            Select Case pLstNumSql
                Case 1
                    DtParam1List = New DataTable
                    myda.Fill(DtParam1List)
                    Return DtParam1List
                Case 2
                    DtParam2List = New DataTable
                    myda.Fill(DtParam2List)
                    Return DtParam2List
                Case Else
                    Return Nothing
            End Select


        End Using


        Return Nothing

    End Function

    Public Function getParamId(ByRef pDt As DataTable, ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As Integer = -1

        For Each er In pDt.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("id"))
                Return retVal
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Poziva procedure koje imaju PREDDEFINISANE parametre u tabeli sys_net_reports_data_parameters
    ''' Odnos procedure prema parametru definisan u: sys_net_reports_data2parameters
    ''' </summary>
    ''' <param name="pReportDataCode"></param>
    ''' <returns></returns>
    Public Function CreateDatasource(ByVal pReportDataCode As String) As DataSet

        Dim mycmd As MySqlCommand
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim myds As New DataSet

        Dim _nazProcedure As String = ""
        Dim _parametri As String = ""
        Dim _paramNum As Integer = 0

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL {nazivProcedure}({parametri});
]]>.Value


            strSQL = <![CDATA[ 
SELECT 
	rd.`report_sql` 
      , rd.`report_data_definition`
FROM `sys_net_reports_data` rd
WHERE rd.`report_data_code`=@report_data_code;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_data_code", pReportDataCode)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                _nazProcedure = rd.GetString("report_sql")
            End While

            rd.Close()

            strSQL = <![CDATA[ 
SELECT 
COUNT(rd2p.`report_data_parameters_code` ) AS ParamNum
FROM `sys_net_reports_data2parameters` rd2p LEFT JOIN `sys_net_reports_data_parameters` rdp
ON rd2p.`report_data_parameters_code`= rdp.`report_data_parameters_code`
WHERE rd2p.`report_data_code`=@report_data_code 
ORDER BY rd2p.`report_data_parameters_code` ;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_data_code", pReportDataCode)
            mycmd.Prepare()

            rd = mycmd.ExecuteReader()

            While rd.Read
                _paramNum = rd.GetInt32("ParamNum")
            End While

            rd.Close()

            If ParamListValues IsNot Nothing AndAlso _paramNum = ParamListValues.Count Then

                For i As Integer = 1 To ParamListValues.Count
                    _parametri = _parametri + ParamListValues.Item(i - 1)
                    If i < _paramNum Then _parametri = _parametri + ","
                Next


                strSQL = <![CDATA[ 
Call {nazivProcedure}({parametri});
]]>.Value

                strSQL = strSQL.Replace("{nazivProcedure}", _nazProcedure)
                strSQL = strSQL.Replace("{parametri}", _parametri)

                myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
                myda.Fill(myds)

            End If


        End Using


        Return myds

    End Function

    Public Function CreateDatasourceXML(ByVal pReportDataCode As String) As DataSet

        Dim mycmd As MySqlCommand
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim myds As New DataSet

        Dim _nazProcedure As String = ""
        Dim _parametri As String = ""
        Dim _paramNum As Integer = 0

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL {nazivProcedure}({parametri});
]]>.Value


            strSQL = <![CDATA[ 
SELECT 
	rd.`report_sql` 
      , rd.`report_data_definition`
FROM `sys_net_reports_data` rd
WHERE rd.`report_data_code`=@report_data_code;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_data_code", pReportDataCode)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                _nazProcedure = rd.GetString("report_sql")
            End While

            rd.Close()

            strSQL = <![CDATA[ 
SELECT 
COUNT(rd2p.`report_data_parameters_code` ) AS ParamNum
FROM `sys_net_reports_data2parameters` rd2p LEFT JOIN `sys_net_reports_data_parameters` rdp
ON rd2p.`report_data_parameters_code`= rdp.`report_data_parameters_code`
WHERE rd2p.`report_data_code`=@report_data_code 
ORDER BY rd2p.`report_data_parameters_code` ;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@report_data_code", pReportDataCode)
            mycmd.Prepare()

            rd = mycmd.ExecuteReader()

            While rd.Read
                _paramNum = rd.GetInt32("ParamNum")
            End While

            rd.Close()

            If ParamListValues IsNot Nothing AndAlso _paramNum = ParamListValues.Count Then

                For i As Integer = 1 To ParamListValues.Count
                    _parametri = _parametri + ParamListValues.Item(i - 1)
                    If i < _paramNum Then _parametri = _parametri + ","
                Next

                strSQL = <![CDATA[ 
Call {nazivProcedure}({parametri});
]]>.Value

                strSQL = strSQL.Replace("{nazivProcedure}", _nazProcedure)
                strSQL = strSQL.Replace("{parametri}", _parametri)

                myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
                myda.Fill(myds)

            End If


        End Using


        Return myds

    End Function

    Public Function WriteReportToDisk(ByVal pReportCode As String, Optional pSessionId As String = "") As String

        Dim mycmd As MySqlCommand

        CurrentRptName = ""

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
                '
                ' Zanemari SessionId za .rpt fajlove
                '
                '_fileName = pSessionId + "_" + rd.GetString(rd.GetOrdinal("report_name"))
                CurrentRptName = rd.GetString(rd.GetOrdinal("report_name"))

            End While

            If Not ReportExists() Then
                WriteReportToDisk2(pReportCode)
            End If

        End Using


        Return CurrentRptName

    End Function

    Private Sub WriteReportToDisk2(ByVal pReportCode As String)

        Dim _rawData() As Byte
        Dim _fileSize As UInt32
        Dim _fileStream As FileStream

        Dim mycmd As MySqlCommand

        If CurrentRptName Is Nothing Then Exit Sub

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
                _fileSize = rd.GetUInt32(rd.GetOrdinal("report_size"))

                _rawData = New Byte(_fileSize) {}
                rd.GetBytes(rd.GetOrdinal("report_structure"), 0, _rawData, 0, _fileSize)

                _fileStream = New FileStream(HttpContext.Current.Server.MapPath(CurrentPath + CurrentRptName), FileMode.Create, FileAccess.ReadWrite)
                _fileStream.Write(_rawData, 0, _fileSize)
                _fileStream.Close()

            End While

        End Using

    End Sub

    Public Function ReportExists() As Boolean

        Dim _docName As String = ""

        If CurrentRptName Is Nothing Then Return False

        _docName = CurrentRptName

        If File.Exists(HttpContext.Current.Server.MapPath("~/Data/CrystalReports/" + _docName)) Then Return True

        Return False

    End Function
End Class
