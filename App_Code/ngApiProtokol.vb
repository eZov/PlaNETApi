Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ngApiProtokol

    Public Sub New()

    End Sub


    Public Property List As List(Of ngPnProtokol)

    ' API P-L1 Sql.Select - ByYear
    Public Sub listItems(ByVal pYear As Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT
    id
  , redbr
  , predmet
  , orgjed
  , godina
FROM
  evd_protokol
WHERE godina=@pYear;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnProtokol))(GetJson(DtList))

        End Using

    End Sub

    ' API P-L1 Sql.Select - ByOrg ByYear
    Public Sub listItems(ByVal pOrgJed As String, ByVal pYear As Integer)

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT
    id
  , redbr
  , predmet
  , orgjed
  , godina
FROM
  evd_protokol
WHERE godina=@pYear
AND orgjed=@pOrgJed;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pOrgJed", pOrgJed)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnProtokol))(GetJson(DtList))

        End Using

    End Sub

    ' API P-L2 Sql.Select - ByOrg ByYear return OneItem
    Public Function listItem(ByVal pOrgJed As String, ByVal pYear As Integer) As ngPnProtokol

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
    id
  , redbr
  , predmet
  , orgjed
  , godina
FROM
  evd_protokol
WHERE godina=@pYear
AND orgjed=@pOrgJed;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pOrgJed", pOrgJed)
            mycmd.Prepare()


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnProtokol))(GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptyProtokol = New ngPnProtokol()
            _emptyProtokol.redbr = -1
            _emptyProtokol.godina = -1
            _emptyProtokol.orgjed = ""
            _emptyProtokol.predmet = ""

            List.Insert(0, _emptyProtokol)
        End If


        Return List.Item(0)

    End Function

    ' API P-L3 Sql.Select - ByRedBr ByYear return OneItem
    Public Function listItem(ByVal pRedBr As Integer, ByVal pYear As Integer) As ngPnProtokol

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
    id
  , redbr
  , predmet
  , orgjed
  , godina
FROM
  evd_protokol
WHERE godina=@pYear
AND redbr=@pRedBr;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pRedBr", pRedBr)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.List = JsonConvert.DeserializeObject(Of List(Of ngPnProtokol))(GetJson(DtList))

        End Using

        If List.Count = 0 Then

            Dim _emptyProtokol = New ngPnProtokol()
            _emptyProtokol.redbr = -1
            _emptyProtokol.godina = -1
            _emptyProtokol.orgjed = ""
            _emptyProtokol.predmet = ""

            List.Insert(0, _emptyProtokol)
        End If


        Return List.Item(0)

    End Function

    ' API P-L2 Sql.Select - ByOrg ByYear return OneItem iz OrgJed ili nad nivoa
    Public Function listItem(ByVal pOrgJed As String, ByVal pYear As Integer, Optional ByVal pNadNivo As Boolean = False) As ngPnProtokol

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT s3.id, s3.`Sifra` AS S3, s3.`Sifra Nadnivo` AS SN3, 
s2.`Sifra Nadnivo` AS SN2, 
s1.`Sifra Nadnivo` AS SN1, 
s.`Sifra Nadnivo` AS SN0
FROM `sfr_organizacija` s
INNER JOIN `sfr_organizacija` s1 ON s1.`Sifra Nadnivo`=s.`Sifra`
INNER JOIN `sfr_organizacija` s2 ON s2.`Sifra Nadnivo`=s1.`Sifra`
INNER JOIN `sfr_organizacija` s3 ON s3.`Sifra Nadnivo`=s2.`Sifra`
WHERE s3.id IS NOT NULL
AND (s3.`Sifra`=@pOrgJed);
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pOrgJed", pOrgJed)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)

            myda.Fill(DtList)

        End Using



        Dim _nadNivo As String = pOrgJed
        listItem(_nadNivo, pYear)

        If List.Item(0).redbr = -1 AndAlso List.Item(0).godina = -1 AndAlso DtList.Rows.Count > 0 Then
            _nadNivo = DtList.Rows.Item(0).Item("SN3")
            listItem(_nadNivo, pYear)

            If List.Item(0).redbr = -1 AndAlso List.Item(0).godina = -1 Then
                _nadNivo = DtList.Rows.Item(0).Item("SN2")
                listItem(_nadNivo, pYear)
                If List.Item(0).redbr = -1 AndAlso List.Item(0).godina = -1 Then
                    _nadNivo = DtList.Rows.Item(0).Item("SN1")
                    listItem(_nadNivo, pYear)
                    If List.Item(0).redbr = -1 AndAlso List.Item(0).godina = -1 Then
                        _nadNivo = DtList.Rows.Item(0).Item("SN0")
                        listItem(_nadNivo, pYear)

                    End If
                End If
            End If
        End If



        Return List.Item(0)

    End Function

    ' API P-C1 Sql.Insert
    Public Function insItem(ByRef pInsItem As ngPnProtokol) As Integer

        Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1
        Dim _LastInsertedId As Integer = -1

        strSQL = <![CDATA[
INSERT INTO evd_protokol (
  redbr, predmet, orgjed, godina
  )
VALUES
  (
    @redbr, @predmet, @orgjed, @godina
  );
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
  predmet = @predmet, orgjed = @orgjed,
  redbr = @redbr, godina = @godina
WHERE 
  id = @id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pInsItem.id)
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
