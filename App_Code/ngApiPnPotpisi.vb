Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ngApiPnPotpisi

    'Public Sub New()

    'End Sub


    'Public Property ProtokolList As List(Of ngPnPotpis)



    Public Sub New()

    End Sub


    Public Property List As List(Of ngPnPotpis)

    ' API P-L1 Sql.Select - Validni potpisi
    Public Sub listItems(ByVal pAktivan As Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT 
ep.id,
CONCAT(e.LastName,' ',e.FirstName) AS EmployeeName,
ep.aktivan,ep.id, 
DATE_FORMAT(IFNULL(ep.dat_unosa,'0001-01-01 00:00:00'), '%Y-%m-%dT%H:%i:%s') AS datunosa, 
DATE_FORMAT(IFNULL(ep.dat_prestanka,'0001-01-01 00:00:00'), '%Y-%m-%dT%H:%i:%s') AS datprestanka,   
ep.potpis_publickey AS publickey,
eps.status_from AS statusfrom, 
eps.status_to AS statusto
FROM evd_potpisi ep
INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
LEFT JOIN evd_potpisi_status eps ON eps.evd_potpisi_id=ep.id
WHERE ep.aktivan %__SqlAktivan__%;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            Select Case pAktivan
                Case 0
                    strSQL = strSQL.Replace("%__SqlAktivan__%", " = 0")
                Case Else
                    strSQL = strSQL.Replace("%__SqlAktivan__%", " <> 0")
            End Select

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnPotpis))(GetJson(DtList))

        End Using

    End Sub

    ' API P-L2 Sql.Select - ByOrg ByYear return OneItem
    Public Function listItem(ByVal pEmployeeID As Integer, Optional pId As Integer = -1) As ngPnPotpis

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT 
ep.id,
CONCAT(e.LastName,' ',e.FirstName) AS EmployeeName,
e.EmployeeID AS  employee_id,
ep.aktivan,ep.id, 
DATE_FORMAT(IFNULL(ep.dat_unosa,'0001-01-01 00:00:00'), '%Y-%m-%dT%H:%i:%s') AS datunosa, 
DATE_FORMAT(IFNULL(ep.dat_prestanka,'0001-01-01 00:00:00'), '%Y-%m-%dT%H:%i:%s') AS datprestanka,   
ep.potpis_publickey AS publickey,
eps.status_from AS statusfrom, 
eps.status_to AS statusto
FROM evd_potpisi ep
INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
LEFT JOIN evd_potpisi_status eps ON eps.evd_potpisi_id=ep.id
WHERE 
e.EmployeeID = @EmployeeID 
%__SqlAktivan__%;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            Select Case pId
                Case -1
                    strSQL = strSQL.Replace("%__SqlAktivan__%", " AND ep.aktivan <> 0")
                Case Else
                    strSQL = strSQL.Replace("%__SqlAktivan__%", " AND ep.id = " + pId.ToString)
            End Select

            mycmd.CommandText = strSQL

            '
            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmployeeID)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnPotpis))(GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptyPotpis = New ngPnPotpis()
            _emptyPotpis.employee_id = -1
            _emptyPotpis.employeename = ""
            _emptyPotpis.id = -1
            _emptyPotpis.publickey = ""
            List.Insert(0, _emptyPotpis)
        End If


        Return List.Item(0)

    End Function


    ' API P-C1 Sql.Insert entry u evd_potpisi
    Public Function insItem(ByRef pInsItem As ngPnPotpis) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1
        Dim _LastInsertedId As Integer = -1

        strSQL = <![CDATA[
SELECT COUNT(*) AS potpis FROM `evd_potpisi` ep
WHERE ep.`employee_id`=@employee_id
AND ep.`aktivan` <> 0;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pInsItem.employee_id)
            mycmd.Prepare()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim _potpis As Integer = 0

            Try
                dbRead = mycmd.ExecuteReader
                While dbRead.Read
                    _potpis = dbRead.GetValue(0)
                End While
                dbRead.Close()
            Catch ex As Exception

            End Try


            If _potpis > 0 Then
                Return -1
            End If



            strSQL = <![CDATA[
INSERT INTO evd_potpisi (
  employee_id, potpis_publickey, aktivan
)
VALUES
  (
    @employee_id, '', -1
  );
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pInsItem.employee_id)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    _LastInsertedId = mycmd.LastInsertedId
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, _LastInsertedId, -1)

    End Function

    ' API P-S1 Sql.Update
    Public Function updItem(ByRef pInsItem As ngPnProtokol) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
UPDATE
	evd_protokol
SET
  predmet = @predmet, orgjed = @orgjed
WHERE redbr = @redbr
  AND godina = @godina;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@redbr", pInsItem.redbr)
            mycmd.Parameters.AddWithValue("@predmet", pInsItem.predmet)
            mycmd.Parameters.AddWithValue("@orgjed", pInsItem.orgjed)
            mycmd.Parameters.AddWithValue("@godina", pInsItem.godina)
            mycmd.Prepare()


            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function

    ' API P-D1 Sql.Delete
    Public Function delItem(ByRef pDelItem As ngPnProtokol) As Boolean

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
DELETE
FROM evd_protokol
WHERE redbr = @redbr
  AND godina = @godina;
    ]]>.Value




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@redbr", pDelItem.redbr)
            mycmd.Parameters.AddWithValue("@godina", pDelItem.godina)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return IIf(_RowsAffected > 0, True, False)

    End Function


    '
    ' Utilities
    '
    Private Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function
End Class
