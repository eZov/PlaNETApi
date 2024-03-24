Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports MySql.Data.Types

Public Class ClsEmployeesPlacanja



    Public Sub New(ByVal pEmployeeId As Integer)

        DtList = New DataTable
        EmployeeId = pEmployeeId

        DtListRad = New DataTable
        DtListBol = New DataTable
        DtListStm = New DataTable
        DtListNkn = New DataTable

    End Sub

    Public Property EmployeeId As Integer


    Public Property DtList As DataTable
    Public Property DtListRad As DataTable
    Public Property DtListBol As DataTable
    Public Property DtListStm As DataTable
    Public Property DtListNkn As DataTable

    Public Function getList() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
SELECT
up.`id`,
  up.Vrsta, 
  up.`Sifra`,
  up.`Naziv`,
  up.`Iznos_Sati`,  
  up.`Iznos_KonObr`,
  up.`Iznos_FinObr`
FROM
  `xfc_uposleni-placanja` up
WHERE EmployeeId=@employeeID AND up.`Vrsta` NOT IN('MRD','SUM','','KRK')
ORDER BY FIELD(up.`Vrsta`,'RAD','ODS','BOL','STM','STN','TPL') ASC;
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtList.Clear()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
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
            If er("id") = pIdReport Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("id"))
                Exit For
            End If
        Next

        Return retVal
    End Function

    Public Function getListRad() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
SELECT
up.`id`,
  up.Vrsta, 
  up.`Sifra`,
  up.`Naziv`,
  up.`Iznos_Sati`,  
  up.`Iznos_KonObr`,
  up.`Iznos_FinObr`
FROM
  `xfc_uposleni-placanja` up
WHERE EmployeeId=@employeeID AND up.`Vrsta` NOT IN('MRD','SUM','','KRK')
 AND up.`Vrsta` IN('RAD','RAD1','RAD111','ODS1' ,'ODS')
ORDER BY FIELD(up.`Vrsta`,'RAD1','RAD111','RAD','ODS1','ODS','BOL','STM','STN','TPL') ASC;
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtListRad.Clear()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtListRad)


        End Using

        Return DtListRad

    End Function

    Public Function getListBol() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
SELECT
up.`id`,
  up.Vrsta, 
  up.`Sifra`,
  up.`Naziv`,
  up.`Iznos_Sati`,  
  up.`Iznos_KonObr`,
  up.`Iznos_FinObr`
FROM
  `xfc_uposleni-placanja` up
WHERE EmployeeId=@employeeID AND up.`Vrsta` NOT IN('MRD','SUM','','KRK')
 AND up.`Vrsta` IN('BOL')
ORDER BY FIELD(up.`Vrsta`,'RAD','ODS','BOL','STM','STN','TPL') ASC;
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtListBol.Clear()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtListBol)


        End Using

        Return DtListBol

    End Function

    Public Function getListStm() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
SELECT
up.`id`,
  up.Vrsta, 
  up.`Sifra`,
  up.`Naziv`,
  up.`Iznos_Sati`,  
  up.`Iznos_KonObr`,
  up.`Iznos_FinObr`
FROM
  `xfc_uposleni-placanja` up
WHERE EmployeeId=@employeeID AND up.`Vrsta` NOT IN('MRD','SUM','','KRK')
 AND up.`Vrsta` IN('STM','STN')
ORDER BY FIELD(up.`Vrsta`,'RAD','ODS','BOL','STM','STN','TPL') ASC;
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtListStm.Clear()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtListStm)


        End Using

        Return DtListStm

    End Function

    Public Function getListNkn() As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            Dim strSQL As String = <![CDATA[ 
SELECT
up.`id`,
  up.Vrsta, 
  up.`Sifra`,
  up.`Naziv`,
  up.`Iznos_Sati`,  
  up.`Iznos_KonObr`,
  up.`Iznos_FinObr`
FROM
  `xfc_uposleni-placanja` up
WHERE EmployeeId=@employeeID AND up.`Vrsta` NOT IN('MRD','SUM','','KRK')
 AND up.`Vrsta` IN('TPL')
ORDER BY FIELD(up.`Vrsta`,'RAD','ODS','BOL','STM','STN','TPL') ASC;
]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtListNkn.Clear()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtListNkn)


        End Using

        Return DtListNkn

    End Function

    Public Sub updateEmpPlacanja(ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE  `xfc_uposleni-placanja`
SET
  Iznos_Sati = @Iznos_Sati, 
  Iznos_KonObr = @Iznos_KonObr, 
  Iznos_FinObr = @Iznos_FinObr
WHERE `id` = @id;
]]>.Value
            mycmd.CommandText = strSQL


            For Each el As Dictionary(Of String, Object) In pRows
                Dim rowKey As Integer = CInt(el("id"))
                Dim rowSati As Double = CDbl(el("Iznos_Sati"))
                Dim rowIznos As Double = CDbl(el("Iznos_KonObr"))
                Dim rowProcenat As Double = CDbl(el("Iznos_FinObr"))

                If rowKey > 0 Then

                    Try
                        mycmd.Parameters.Clear()
                        mycmd.Parameters.AddWithValue("@id", rowKey)
                        mycmd.Parameters.AddWithValue("@Iznos_Sati", rowSati)
                        mycmd.Parameters.AddWithValue("@Iznos_KonObr", rowIznos)
                        mycmd.Parameters.AddWithValue("@Iznos_FinObr", rowProcenat)
                        mycmd.Prepare()

                        mycmd.ExecuteNonQuery()
                    Catch ex As MySqlException

                    End Try

                End If

            Next

        End Using


    End Sub

    Public Sub insertEmpPlacanja()

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
 CALL OBR_insert_uposleniplacanjaEmp(@EmployeeId);
]]>.Value
            mycmd.CommandText = strSQL




            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@EmployeeId", EmployeeId)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As MySqlException

            End Try


        End Using


    End Sub

End Class
